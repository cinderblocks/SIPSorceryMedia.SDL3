using System;
using System.Collections.Generic;
using System.Threading;
using NAudio.CoreAudioApi;
using NAudio.Wave;
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

        public SDL3RecordThenPlayback()
        {
            // --- NAudio recording (bypasses SDL's WASAPI single-packet-per-wakeup bug) ---
            using var capture = new WasapiCapture();
            var captureFormat = capture.WaveFormat;
            Console.WriteLine($"NAudio capture format: {captureFormat.SampleRate}Hz {captureFormat.BitsPerSample}bit " +
                              $"{captureFormat.Channels}ch {captureFormat.Encoding}");

            var sdlFormat = GetSdlFormat(captureFormat);
            if (sdlFormat == SDL_AudioFormat.SDL_AUDIO_UNKNOWN)
                throw new ApplicationException($"No SDL format mapping for NAudio format: {captureFormat}");

            var recordedChunks = new List<byte[]>();
            int totalRecorded = 0;

            capture.DataAvailable += (_, e) =>
            {
                if (e.BytesRecorded <= 0) return;
                var chunk = new byte[e.BytesRecorded];
                Buffer.BlockCopy(e.Buffer, 0, chunk, 0, e.BytesRecorded);
                recordedChunks.Add(chunk);
                totalRecorded += e.BytesRecorded;
            };

            var recordingStopped = new ManualResetEventSlim(false);
            capture.RecordingStopped += (_, _) => recordingStopped.Set();

            Console.WriteLine($"Recording for {RECORD_SECONDS} seconds...");
            var recordingStartTime = DateTime.UtcNow;
            capture.StartRecording();
            Thread.Sleep(RECORD_SECONDS * 1000);
            capture.StopRecording();
            recordingStopped.Wait(TimeSpan.FromSeconds(2));

            float recordedSeconds = totalRecorded / (float)captureFormat.AverageBytesPerSecond;
            Console.WriteLine($"Recorded {totalRecorded} bytes = {recordedSeconds:F2} seconds " +
                              $"(wall clock: {(DateTime.UtcNow - recordingStartTime).TotalSeconds:F2}s)");

            // --- SDL3 playback ---
            if (!SDL_Init(SDL_InitFlags.SDL_INIT_AUDIO))
                throw new ApplicationException("Cannot initialize SDL for audio");

            var sdlSpec = new SDL_AudioSpec
            {
                freq    = captureFormat.SampleRate,
                channels = captureFormat.Channels,
                format  = sdlFormat
            };

            var playbackDevice = SDL3Helper.GetAudioPlaybackDevice(string.Empty);
            if (!playbackDevice.HasValue)
                throw new ApplicationException("Playback device not found");

            var playbackStream = SDL3Helper.OpenAudioDeviceStreamHandle(playbackDevice.Value.id, sdlSpec, null);
            if (playbackStream == null || playbackStream.IsInvalid)
                throw new ApplicationException("Cannot open playback device stream");

            byte[] buffer = new byte[totalRecorded];
            int offset = 0;
            foreach (var chunk in recordedChunks)
            {
                Buffer.BlockCopy(chunk, 0, buffer, offset, chunk.Length);
                offset += chunk.Length;
            }

            Thread.Sleep(200);
            Console.WriteLine("Starting playback...");
            SDL3Helper.ResumeAudioStreamDevice(playbackStream);
            SDL3Helper.PutAudioToStream(playbackStream, buffer, totalRecorded);
            var playbackStartTime = DateTime.UtcNow;

            while (SDL3Helper.GetAudioStreamQueued(playbackStream) > 1024)
                Thread.Sleep(50);
            Thread.Sleep(200);

            Console.WriteLine($"Playback complete. Duration: {(DateTime.UtcNow - playbackStartTime).TotalSeconds:F2}s " +
                              $"(expected: {recordedSeconds:F2}s)");

            try { SDL3Helper.DestroyAudioStream(playbackStream); } catch { }
            SDL_Quit();
        }

        private static SDL_AudioFormat GetSdlFormat(WaveFormat waveFormat)
        {
            if (waveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                return waveFormat.BitsPerSample == 32 ? SDL_AudioFormat.SDL_AUDIO_F32 : SDL_AudioFormat.SDL_AUDIO_UNKNOWN;

            if (waveFormat.Encoding == WaveFormatEncoding.Pcm)
                return waveFormat.BitsPerSample switch
                {
                    8  => SDL_AudioFormat.SDL_AUDIO_U8,
                    16 => SDL_AudioFormat.SDL_AUDIO_S16,
                    32 => SDL_AudioFormat.SDL_AUDIO_S32,
                    _  => SDL_AudioFormat.SDL_AUDIO_UNKNOWN
                };

            return SDL_AudioFormat.SDL_AUDIO_UNKNOWN;
        }
    }
}
