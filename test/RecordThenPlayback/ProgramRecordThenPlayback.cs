using System;
using System.Collections.Generic;
using System.Threading;
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
    public class SDL3RecordThenPlayback
    {
        private const int RECORD_SECONDS = 5;
        private const int Frequency = 44100;
        private const SDL_AudioFormat Format = SDL_AudioFormat.SDL_AUDIO_F32;
        private const int Channels = 2;

        public SDL3RecordThenPlayback()
        {
            if (!SDL_Init(SDL_InitFlags.SDL_INIT_AUDIO))
                throw new ApplicationException("Cannot initialize SDL for audio");

            var recordingDevice = SDL3Helper.GetAudioRecordingDevice(string.Empty);
            if (!recordingDevice.HasValue)
                throw new ApplicationException("Recording device not found");

            var playbackDevice = SDL3Helper.GetAudioPlaybackDevice(string.Empty);
            if (!playbackDevice.HasValue)
                throw new ApplicationException("Playback device not found");

            SDL_AudioSpec requestedSpec = new SDL_AudioSpec
            {
                freq = Frequency,
                format = Format,
                channels = Channels
            };

            var recStream = SDL3Helper.OpenAudioDeviceStreamHandle(
                recordingDevice.Value.id,
                requestedSpec,
                null);

            if (recStream == null || recStream.IsInvalid)
                throw new ApplicationException("Cannot open recording device stream");

            SDL3Helper.GetAudioStreamFormat(recStream, out SDL_AudioSpec recSrcSpec, out SDL_AudioSpec recDstSpec);
            int bytesPerSecond = SDL3Helper.GetBytesPerSecond(recDstSpec);
            Console.WriteLine($"Recording stream: {recDstSpec.freq}Hz {recDstSpec.format} {recDstSpec.channels}ch = {bytesPerSecond} bytes/sec");

            var playbackStream = SDL3Helper.OpenAudioDeviceStreamHandle(playbackDevice.Value.id, recDstSpec, null);
            if (playbackStream == null || playbackStream.IsInvalid)
                throw new ApplicationException("Cannot open playback device stream");

            Console.WriteLine($"Recording for {RECORD_SECONDS} seconds...");
            SDL3Helper.ResumeAudioStreamDevice(recStream);

            var recordedChunks = new List<byte[]>();
            int totalRecorded = 0;
            var drainBuf = new byte[65536];
            var recordingStartTime = DateTime.UtcNow;

            while ((DateTime.UtcNow - recordingStartTime).TotalSeconds < RECORD_SECONDS)
            {
                Thread.Sleep(10);
                int read;
                while ((read = SDL3Helper.GetAudioStreamData(recStream, drainBuf, drainBuf.Length)) > 0)
                {
                    var chunk = new byte[read];
                    Buffer.BlockCopy(drainBuf, 0, chunk, 0, read);
                    recordedChunks.Add(chunk);
                    totalRecorded += read;
                }
            }
            SDL3Helper.PauseAudioStreamDevice(recStream);

            float recordedSeconds = totalRecorded / (float)bytesPerSecond;
            Console.WriteLine($"Recorded {totalRecorded} bytes = {recordedSeconds:F2} seconds " +
                              $"(wall clock: {(DateTime.UtcNow - recordingStartTime).TotalSeconds:F2}s)");

            byte[] buffer = new byte[totalRecorded];
            int offset = 0;
            foreach (var chunk in recordedChunks)
            {
                Buffer.BlockCopy(chunk, 0, buffer, offset, chunk.Length);
                offset += chunk.Length;
            }

            Thread.Sleep(500);
            Console.WriteLine("Starting playback...");
            SDL3Helper.ResumeAudioStreamDevice(playbackStream);
            SDL3Helper.PutAudioToStream(playbackStream, buffer, totalRecorded);
            var playbackStartTime = DateTime.UtcNow;

            while (SDL3Helper.GetAudioStreamQueued(playbackStream) > 1024)
                Thread.Sleep(50);
            Thread.Sleep(200);

            Console.WriteLine($"Playback complete. Duration: {(DateTime.UtcNow - playbackStartTime).TotalSeconds:F2}s " +
                              $"(expected: {recordedSeconds:F2}s)");

            try { SDL3Helper.DestroyAudioStream(recStream); } catch { }
            try { SDL3Helper.DestroyAudioStream(playbackStream); } catch { }
            SDL_Quit();
        }
    }
}
