using System;
using System.Buffers;
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

        // device handles/ids (use SafeHandle for streams)
        private SDL3AudioStreamSafeHandle? _recordingStream;
        private SDL3AudioStreamSafeHandle? _playbackStream;
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

            // Open streams and register callbacks (SafeHandle)
            _recordingStream = SDL3Helper.OpenAudioDeviceStreamHandle(recordingDevice.Value.id, ref desiredRecordingSpec, FillRecordingCallback);
            if (_recordingStream == null || _recordingStream.IsInvalid)
                throw new ApplicationException("Cannot open recording device stream");

            _playbackStream = SDL3Helper.OpenAudioDeviceStreamHandle(playbackDevice.Value.id, ref desiredPlaybackSpec, FillPlaybackCallback);
            if (_playbackStream == null || _playbackStream.IsInvalid)
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

            // dispose handles via helper to unregister callbacks
            try { SDL3Helper.DestroyAudioStream(_recordingStream); } catch { }
            try { SDL3Helper.DestroyAudioStream(_playbackStream); } catch { }

            SDL_Quit();
        }

        // Recording callback: single-producer append into _buffer until target is reached
        private void FillRecordingCallback(IntPtr userdata, SDL3AudioStreamSafeHandle? stream, int additionalAmount, int totalAmount)
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

            if (stream == null || stream.IsInvalid)
                return;

            var pool = ArrayPool<byte>.Shared;
            byte[] rented = pool.Rent(toCopy);
            try
            {
                int read = SDL3Helper.GetAudioStreamData(stream, rented, toCopy);
                if (read <= 0)
                {
                    return;
                }

                if (read <= tail)
                {
                    Buffer.BlockCopy(rented, 0, _buffer, pos, read);
                }
                else
                {
                    Buffer.BlockCopy(rented, 0, _buffer, pos, tail);
                    Buffer.BlockCopy(rented, tail, _buffer, 0, read - tail);
                }
                toCopy = read;
            }
            finally
            {
                try { pool.Return(rented); } catch { }
            }

            // advance write pos and recorded count
            int newPos = (pos + toCopy) % _targetBytes;
            Volatile.Write(ref _writePos, newPos);
            Interlocked.Add(ref _recordedBytes, toCopy);
        }

        // Playback callback: single-consumer read from _buffer into stream until consumed
        private void FillPlaybackCallback(IntPtr userdata, SDL3AudioStreamSafeHandle? stream, int additionalAmount, int totalAmount)
        {
            if (additionalAmount <= 0)
                return;

            int recorded = (int)Interlocked.Read(ref _recordedBytes);
            int played = (int)Interlocked.Read(ref _playedBytes);
            int available = Math.Min(recorded - played, _targetBytes - _readPos);
            if (available <= 0)
            {
                // nothing yet or finished; zero-fill playback buffer to avoid noise
                if (stream == null || stream.IsInvalid) return;
                try
                {
                    ZeroFillPlaybackBuffer(stream, additionalAmount);
                }
                catch { }
                return;
            }

            int toCopy = Math.Min(additionalAmount, available);

            int pos = _readPos;
            int tail = _targetBytes - pos;

            if (stream == null || stream.IsInvalid) return;

            var poolOut = ArrayPool<byte>.Shared;
            byte[] temp = poolOut.Rent(toCopy);
            try
            {
                if (toCopy <= tail)
                {
                    Buffer.BlockCopy(_buffer, pos, temp, 0, toCopy);
                }
                else
                {
                    Buffer.BlockCopy(_buffer, pos, temp, 0, tail);
                    Buffer.BlockCopy(_buffer, 0, temp, tail, toCopy - tail);
                }

                // send to native stream (helper will pin)
                SDL3Helper.PutAudioToStream(stream, temp, toCopy);

                int newPos = (pos + toCopy) % _targetBytes;
                Volatile.Write(ref _readPos, newPos);
                Interlocked.Add(ref _playedBytes, toCopy);

                if (toCopy < additionalAmount)
                {
                    int remainder = additionalAmount - toCopy;
                    byte[] zero = poolOut.Rent(remainder);
                    try
                    {
                        Array.Clear(zero, 0, remainder);
                        SDL3Helper.PutAudioToStream(stream, zero, remainder);
                    }
                    finally { try { poolOut.Return(zero); } catch { } }
                }
            }
            finally
            {
                try { poolOut.Return(temp); } catch { }
            }
        }

        private static void ZeroFillPlaybackBuffer(SDL3AudioStreamSafeHandle? stream, int length)
        {
            if (length <= 0) return;
            var pool = ArrayPool<byte>.Shared;
            byte[] zero = pool.Rent(length);
            try
            {
                Array.Clear(zero, 0, length);
                SDL3Helper.PutAudioToStream(stream, zero, length);
            }
            finally
            {
                try { pool.Return(zero); } catch { }
            }
        }
    }
}
