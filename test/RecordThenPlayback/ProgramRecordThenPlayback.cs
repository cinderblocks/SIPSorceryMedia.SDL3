using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using static SDL3.SDL;
using SIPSorceryMedia.SDL3;
using System.Runtime.InteropServices;

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
        private const int RECORD_SECONDS_LOCAL = 5;
        private const int Frequency = 44100;
        private const SDL_AudioFormat Format = SDL_AudioFormat.SDL_AUDIO_F32;
        private const int Channels = 2;

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

            SDL_AudioSpec desiredSpec = new SDL_AudioSpec
            {
                freq = Frequency,
                format = Format,
                channels = Channels
            };

            Console.WriteLine($"Requesting: {desiredSpec.freq}Hz {desiredSpec.format} {desiredSpec.channels}ch");

            // Open recording device and create a stream bound to it
            uint recDeviceId = SDL_OpenAudioDevice(recordingDevice.Value.id, ref desiredSpec);
            if (recDeviceId == 0)
                throw new ApplicationException("Cannot open recording device");

            IntPtr recStreamPtr = SDL_CreateAudioStream(ref desiredSpec, ref desiredSpec);
            if (recStreamPtr == IntPtr.Zero)
                throw new ApplicationException("Cannot create recording stream");

            if (!SDL_BindAudioStream(recDeviceId, recStreamPtr))
                throw new ApplicationException("Cannot bind recording stream to device");

            var playbackStream = SDL3Helper.OpenAudioDeviceStreamHandle(playbackDevice.Value.id, ref desiredSpec, null);
            if (playbackStream == null || playbackStream.IsInvalid)
                throw new ApplicationException("Cannot open playback device stream");

            int bytesPerSecond = SDL3Helper.GetBytesPerSecond(desiredSpec);
            Console.WriteLine($"Audio format: {Frequency}Hz {Format} {Channels}ch = {bytesPerSecond} bytes/sec");

            List<byte[]> recordedChunks = new List<byte[]>();
            int totalRecorded = 0;
            bool recording = true;

            Thread recordingThread = new Thread(() =>
            {
                while (recording)
                {
                    int available = SDL_GetAudioStreamAvailable(recStreamPtr);
                    if (available > 0)
                    {
                        byte[] chunk = new byte[available];
                        GCHandle gch = GCHandle.Alloc(chunk, GCHandleType.Pinned);
                        try
                        {
                            IntPtr bufPtr = gch.AddrOfPinnedObject();
                            int read = SDL_GetAudioStreamData(recStreamPtr, bufPtr, available);
                            if (read > 0)
                            {
                                Array.Resize(ref chunk, read);
                                lock (recordedChunks)
                                {
                                    recordedChunks.Add(chunk);
                                    totalRecorded += read;
                                }
                            }
                        }
                        finally
                        {
                            gch.Free();
                        }
                    }
                    Thread.Sleep(1);
                }
            });
            recordingThread.Priority = ThreadPriority.Highest;

            Console.WriteLine("Starting recording for {0} seconds...", RECORD_SECONDS_LOCAL);
            SDL_ResumeAudioDevice(recDeviceId);

            var recordingStartTime = DateTime.UtcNow;
            recordingThread.Start();

            Thread.Sleep(TimeSpan.FromSeconds(RECORD_SECONDS_LOCAL));

            recording = false;
            SDL_PauseAudioDevice(recDeviceId);
            recordingThread.Join(1000);

            var recordingDuration = (DateTime.UtcNow - recordingStartTime).TotalSeconds;
            Console.WriteLine($"Finished recording. Duration: {recordingDuration:F2} seconds");
            Console.WriteLine($"Recorded {totalRecorded} bytes = {totalRecorded / (float)bytesPerSecond:F2} seconds of audio");

            byte[] buffer = new byte[totalRecorded];
            int offset = 0;
            lock (recordedChunks)
            {
                foreach (var chunk in recordedChunks)
                {
                    Buffer.BlockCopy(chunk, 0, buffer, offset, chunk.Length);
                    offset += chunk.Length;
                }
            }

            Thread.Sleep(TimeSpan.FromSeconds(1));
            Console.WriteLine("Starting playback...");

            SDL3Helper.ResumeAudioStreamDevice(playbackStream);

            var playbackStartTime = DateTime.UtcNow;
            int chunkSize = bytesPerSecond / 20;
            int totalPlayed = 0;
            var pool = ArrayPool<byte>.Shared;

            while (totalPlayed < totalRecorded)
            {
                int remaining = totalRecorded - totalPlayed;
                int toPush = Math.Min(chunkSize, remaining);
                
                byte[] temp = pool.Rent(toPush);
                try
                {
                    Buffer.BlockCopy(buffer, totalPlayed, temp, 0, toPush);
                    SDL3Helper.PutAudioToStream(playbackStream, temp, toPush);
                    totalPlayed += toPush;
                }
                finally
                {
                    pool.Return(temp);
                }
                
                Thread.Sleep(45);
            }

            Console.WriteLine("Waiting for playback to complete...");
            while (SDL3Helper.GetAudioStreamQueued(playbackStream) > 1024)
            {
                Thread.Sleep(50);
            }
            Thread.Sleep(500);

            var playbackDuration = (DateTime.UtcNow - playbackStartTime).TotalSeconds;
            Console.WriteLine($"Finished playback. Duration: {playbackDuration:F2} seconds");

            SDL_DestroyAudioStream(recStreamPtr);
            SDL_CloseAudioDevice(recDeviceId);
            try { SDL3Helper.DestroyAudioStream(playbackStream); } catch { }

            SDL_Quit();
        }
    }
}
