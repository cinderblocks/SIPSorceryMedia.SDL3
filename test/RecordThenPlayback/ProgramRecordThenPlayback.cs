using System;
using static SDL3.SDL;
using SIPSorceryMedia.SDL3;

namespace RecordThenPlayback
{
    internal class ProgramRecordThenPlayback
    {
        static void Main(string[] args)
        {
            Console.WriteLine("RecordThenPlayback: Starting");
            new SDL3RecordThenPlayback();
        }
    }
}

namespace SIPSorceryMedia.SDL3
{
    using System;
    using System.Threading;

    public unsafe class SDL3RecordThenPlayback
    {
        // local record seconds constant to avoid cross-class reference
        private const int RECORD_SECONDS_LOCAL = 5;

        // audio format used for both record and playback
        private const int Frequency = 44100;
        private const SDL_AudioFormat Format = SDL_AudioFormat.SDL_AUDIO_F32;
        private const int Channels = 2;

        // buffer for exactly RECORD_SECONDS of audio
        private readonly byte[] _buffer;
        private readonly int _targetBytes;

        // device handles/ids
        private readonly IntPtr _recordingStream;
        private readonly IntPtr _playbackStream;
        private readonly uint _recordingDeviceId;
        private readonly uint _playbackDeviceId;

        // writer and reader positions and counters (safely updated from callbacks)
        private volatile int _writePos = 0;
        private volatile int _readPos = 0;
        private long _recordedBytes = 0;
        private long _playedBytes = 0;

        public SDL3RecordThenPlayback()
        {
            if (!SDL_Init(SDL_InitFlags.SDL_INIT_AUDIO))
                throw new ApplicationException("Cannot initialize SDL for audio");

            var recordingDevice = SDL3Helper.GetAudioRecordingDevice(null);
            if (!recordingDevice.HasValue)
                throw new ApplicationException("Recording device not found");

            var playbackDevice = SDL3Helper.GetAudioPlaybackDevice(null);
            if (!playbackDevice.HasValue)
                throw new ApplicationException("Playback device not found");

            SDL_AudioSpec desiredRecordingSpec = new SDL_AudioSpec
            {
                freq = Frequency,
                format = Format,
                channels = Channels
            };

            SDL_AudioSpec desiredPlaybackSpec = new SDL_AudioSpec
            {
                freq = Frequency,
                format = Format,
                channels = Channels
            };

            // Open streams and register callbacks
            _recordingStream = SDL3Helper.OpenAudioDeviceStream(recordingDevice.Value.id, ref desiredRecordingSpec, FillRecordingCallback);
            if (_recordingStream == IntPtr.Zero)
                throw new ApplicationException("Cannot open recording device stream");

            _playbackStream = SDL3Helper.OpenAudioDeviceStream(playbackDevice.Value.id, ref desiredPlaybackSpec, FillPlaybackCallback);
            if (_playbackStream == IntPtr.Zero)
                throw new ApplicationException("Cannot open playback device stream");

            // keep device ids for pause/resume
            _recordingDeviceId = recordingDevice.Value.id;
            _playbackDeviceId = playbackDevice.Value.id;

            // compute bytes per second from spec (helper returns bytes for freq/channels/format)
            int bytesPerSecond = SDL3Helper.GetBytesPerSecond(desiredRecordingSpec);
            _targetBytes = RECORD_SECONDS_LOCAL * bytesPerSecond;
            _buffer = new byte[_targetBytes];

            // RECORD -> wait for 5 seconds of data -> notify -> wait 5 seconds -> PLAYBACK
            Console.WriteLine("Starting recording for {0} seconds...", RECORD_SECONDS_LOCAL);
            SDL3Helper.ResumeAudioStreamDevice(_recordingStream);

            // Wait until we have recorded target bytes
            while (Interlocked.Read(ref _recordedBytes) < _targetBytes)
            {
                Thread.Sleep(10);
            }

            // Stop recording
            SDL_PauseAudioDevice(_recordingDeviceId);
            Console.WriteLine("Finished recording.");

            // Wait 2 seconds before playback
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Console.WriteLine("Starting playback...");

            // Prepare playback counters and start playback
            Interlocked.Exchange(ref _playedBytes, 0);
            Volatile.Write(ref _readPos, 0);

            SDL3Helper.ResumeAudioStreamDevice(_playbackStream);

            // Wait until playback consumed all recorded bytes
            while (Interlocked.Read(ref _playedBytes) < _targetBytes)
            {
                Thread.Sleep(10);
            }

            SDL_PauseAudioDevice(_playbackDeviceId);
            Console.WriteLine("Finished playback.");

            SDL_Quit();
        }

        // Recording callback: single-producer append into _buffer until target is reached
        private void FillRecordingCallback(IntPtr userdata, IntPtr stream, int additionalAmount, int totalAmount)
        {
            if (additionalAmount <= 0)
                return;

            // how many bytes are still needed
            int remaining = _targetBytes - (int)Interlocked.Read(ref _recordedBytes);
            if (remaining <= 0)
                return;

            int toCopy = Math.Min(additionalAmount, remaining);

            int pos = _writePos;
            int tail = _targetBytes - pos;

            if (toCopy <= tail)
            {
                fixed (byte* dst = &_buffer[pos])
                {
                    Buffer.MemoryCopy((byte*)stream, dst, toCopy, toCopy);
                }
            }
            else
            {
                // copy tail then head (shouldn't usually happen because we stop at target)
                fixed (byte* dst = &_buffer[pos])
                {
                    Buffer.MemoryCopy((byte*)stream, dst, tail, tail);
                }
                fixed (byte* dst0 = &_buffer[0])
                {
                    Buffer.MemoryCopy((byte*)stream + tail, dst0, toCopy - tail, toCopy - tail);
                }
            }

            // advance write pos and recorded count
            int newPos = (pos + toCopy) % _targetBytes;
            Volatile.Write(ref _writePos, newPos);
            Interlocked.Add(ref _recordedBytes, toCopy);
        }

        // Playback callback: single-consumer read from _buffer into stream until consumed
        private void FillPlaybackCallback(IntPtr userdata, IntPtr stream, int additionalAmount, int totalAmount)
        {
            if (additionalAmount <= 0)
                return;

            int recorded = (int)Interlocked.Read(ref _recordedBytes);
            int played = (int)Interlocked.Read(ref _playedBytes);
            int available = Math.Min(recorded - played, _targetBytes - _readPos);
            if (available <= 0)
            {
                // nothing yet or finished; zero-fill playback buffer to avoid noise
                ZeroFillPlaybackBuffer((byte*)stream, additionalAmount);
                return;
            }

            int toCopy = Math.Min(additionalAmount, available);

            int pos = _readPos;
            int tail = _targetBytes - pos;

            if (toCopy <= tail)
            {
                fixed (byte* src = &_buffer[pos])
                {
                    Buffer.MemoryCopy(src, (byte*)stream, toCopy, toCopy);
                }
            }
            else
            {
                fixed (byte* src = &_buffer[pos])
                {
                    Buffer.MemoryCopy(src, (byte*)stream, tail, tail);
                }
                fixed (byte* src0 = &_buffer[0])
                {
                    Buffer.MemoryCopy(src0, (byte*)stream + tail, toCopy - tail, toCopy - tail);
                }
            }

            int newPos = (pos + toCopy) % _targetBytes;
            Volatile.Write(ref _readPos, newPos);
            Interlocked.Add(ref _playedBytes, toCopy);

            // If additionalAmount was larger, zero-fill the remainder to avoid playback garbage
            if (toCopy < additionalAmount)
                ZeroFillPlaybackBuffer((byte*)stream + toCopy, additionalAmount - toCopy);
        }

        private static void ZeroFillPlaybackBuffer(byte* dst, int length)
        {
            if (length <= 0) return;
            // simple zero fill
            for (int i = 0; i < length; i++)
                dst[i] = 0;
        }
    }
}
