using SIPSorcery.Media;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.FFmpeg;
using SIPSorceryMedia.SDL3;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CheckCodec
{
    /// <summary>
    /// PCMU/PCMA audio encoder that demonstrates the encode → decode roundtrip.
    /// </summary>
    internal class AudioEncoder : IAudioEncoder
    {
        public List<AudioFormat> SupportedFormats { get; } = new List<AudioFormat>
        {
            new AudioFormat(SDPWellKnownMediaFormatsEnum.PCMU),
            new AudioFormat(SDPWellKnownMediaFormatsEnum.PCMA),
        };

        public byte[] EncodeAudio(short[] pcm, AudioFormat format)
        {
            return format.Codec switch
            {
                AudioCodecsEnum.PCMU => pcm.Select(s => MuLawEncoder.LinearToMuLawSample(s)).ToArray(),
                AudioCodecsEnum.PCMA => pcm.Select(s => ALawEncoder.LinearToALawSample(s)).ToArray(),
                _ => throw new NotSupportedException($"Codec {format.Codec} not supported by this encoder")
            };
        }

        public short[] DecodeAudio(byte[] encoded, AudioFormat format)
        {
            return format.Codec switch
            {
                AudioCodecsEnum.PCMU => encoded.Select(b => MuLawDecoder.MuLawToLinearSample(b)).ToArray(),
                AudioCodecsEnum.PCMA => encoded.Select(b => ALawDecoder.ALawToLinearSample(b)).ToArray(),
                _ => throw new NotSupportedException($"Codec {format.Codec} not supported by this encoder")
            };
        }
    }

    internal class ProgramCheckCodec
    {
        // Path to FFmpeg library — update to match your environment
        private const string FFMPEG_LIB_PATH = @"C:\ffmpeg\bin";

        // Audio file to test (local or remote)
        private const string AUDIO_FILE_PATH =
            @"https://upload.wikimedia.org/wikipedia/commons/0/0f/Pop_RockBrit_%28exploration%29-en_wave.wav";

        // 160 samples = 20ms at 8KHz (G.711 standard frame size)
        private const int FRAME_SIZE = 160;

        static SDL3AudioEndPoint? audioEndPoint;
        static IAudioSource? audioSource;

        static readonly AudioEncoder audioEncoder = new AudioEncoder();
        static AudioFormat? audioFormat;

        static void Main(string[] args)
        {
            Console.Clear();

            Console.WriteLine("Initialising SDL3...");
            SDL3Helper.InitSDL();
            Console.WriteLine("SDL3 initialised.\n");

            var useRecordingDevice = UseRecordingDeviceOrAudioFile();

            string? recordingDeviceName = null;
            if (useRecordingDevice)
            {
                recordingDeviceName = SelectDevice(isRecording: true);
                if (recordingDeviceName == null)
                {
                    SDL3Helper.QuitSDL();
                    return;
                }
            }

            string? playbackDeviceName = SelectDevice(isRecording: false);
            if (playbackDeviceName == null)
            {
                SDL3Helper.QuitSDL();
                return;
            }

            audioFormat = SelectAudioFormat();
            if (audioFormat == null)
            {
                SDL3Helper.QuitSDL();
                return;
            }

            Console.WriteLine(useRecordingDevice
                ? $"\nAudio input:  [{recordingDeviceName}]"
                : $"\nAudio input:  [{AUDIO_FILE_PATH}]");
            Console.WriteLine($"Audio output: [{playbackDeviceName}]");
            Console.WriteLine($"Codec:        [{audioFormat.Value.FormatName}]\n");

            audioEndPoint = new SDL3AudioEndPoint(playbackDeviceName, audioEncoder);
            audioEndPoint.SetAudioSinkFormat(audioFormat.Value);
            audioEndPoint.StartAudioSink();

            if (useRecordingDevice)
            {
                audioSource = new SDL3AudioSource(recordingDeviceName!, audioEncoder, FRAME_SIZE);
            }
            else
            {
                Console.WriteLine("Initialising FFmpeg...");
                FFmpegInit.Initialise(FfmpegLogLevelEnum.AV_LOG_FATAL, FFMPEG_LIB_PATH);
                Console.WriteLine("FFmpeg initialised.\n");

                audioSource = new FFmpegFileSource(AUDIO_FILE_PATH, true, audioEncoder, FRAME_SIZE, false);
            }

            audioSource.OnAudioSourceEncodedSample += AudioSource_OnAudioSourceEncodedSample;

            audioSource.SetAudioSourceFormat(audioFormat.Value);
            audioSource.StartAudio();

            Console.WriteLine("Encoding → decoding → playback. Press Q or Enter to quit.\n");

            for (var loop = true; loop;)
            {
                var cki = Console.ReadKey(true);
                switch (cki.Key)
                {
                    case ConsoleKey.Q:
                    case ConsoleKey.Enter:
                    case ConsoleKey.Escape:
                        loop = false;
                        break;
                }
            }

            audioSource.CloseAudio();
            audioEndPoint.CloseAudioSink();
            SDL3Helper.QuitSDL();
        }

        private static void AudioSource_OnAudioSourceEncodedSample(uint durationRtpUnits, byte[] sample)
        {
            if (audioFormat == null || audioEndPoint == null) return;

            var pcmSample = audioEncoder.DecodeAudio(sample, audioFormat.Value);
            var pcmBytes = new byte[pcmSample.Length * sizeof(short)];
            Buffer.BlockCopy(pcmSample, 0, pcmBytes, 0, pcmBytes.Length);
            audioEndPoint.PutAudioSample(pcmBytes);
        }

        private static string? SelectDevice(bool isRecording)
        {
            string label = isRecording ? "recording" : "playback";
            var devices = isRecording
                ? SDL3Helper.GetAudioRecordingDevices()
                : SDL3Helper.GetAudioPlaybackDevices();

            if (devices == null || devices.Count == 0)
            {
                Console.WriteLine($"No audio {label} devices found.");
                return null;
            }

            var names = new List<string>(devices.Values);

            while (true)
            {
                Console.WriteLine($"\nSelect audio {label} device:");
                for (int i = 0; i < names.Count; i++)
                    Console.WriteLine($"  [{i + 1}] {names[i]}");
                Console.Write("> ");

                var key = Console.ReadKey();
                Console.WriteLine();
                if (int.TryParse("" + key.KeyChar, out int choice) && choice >= 1 && choice <= names.Count)
                    return names[choice - 1];
            }
        }

        private static AudioFormat? SelectAudioFormat()
        {
            var formats = audioEncoder.SupportedFormats;
            if (formats == null || formats.Count == 0)
            {
                Console.WriteLine("No audio formats available.");
                return null;
            }
            if (formats.Count == 1)
                return formats[0];

            while (true)
            {
                Console.WriteLine("\nSelect audio format:");
                for (int i = 0; i < formats.Count; i++)
                    Console.WriteLine($"  [{i + 1}] {formats[i].FormatName}");
                Console.Write("> ");

                var key = Console.ReadKey();
                Console.WriteLine();
                if (int.TryParse("" + key.KeyChar, out int choice) && choice >= 1 && choice <= formats.Count)
                    return formats[choice - 1];
            }
        }

        private static bool UseRecordingDeviceOrAudioFile()
        {
            while (true)
            {
                Console.WriteLine("Audio input source:");
                Console.WriteLine("  [1] Microphone (recording device)");
                Console.WriteLine($"  [2] Audio file ({AUDIO_FILE_PATH})");
                Console.Write("> ");

                var key = Console.ReadKey();
                Console.WriteLine();
                if (key.KeyChar == '1') return true;
                if (key.KeyChar == '2') return false;
            }
        }
    }
}
