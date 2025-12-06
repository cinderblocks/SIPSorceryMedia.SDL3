using System;
using System.Buffers;
using static SDL3.SDL;
using SIPSorceryMedia.SDL3;

namespace RecordThenPlayback
{
    internal class Program
    {
        // record duration in seconds
        private const int RECORD_SECONDS = 5;

        static void Main(string[] args)
        {
            Console.WriteLine("RecordThenPlayback: Starting");

            SDL3Helper.InitSDL();

            // choose first recording device
            var devices = SDL3Helper.GetAudioRecordingDevices();
            if (devices.Count == 0)
            {
                Console.WriteLine("No recording devices found");
                SDL3Helper.QuitSDL();
                return;
            }

            var device = devices.Values.GetEnumerator();
            device.MoveNext();
            var deviceName = device.Current;
            var deviceId = uint.Parse(devices.Keys.GetEnumerator().Current.ToString());

            Console.WriteLine($"Using recording device: {deviceName}");

            // open stream and record into buffer
            var spec = SDL3Helper.GetAudioSpec();
            IntPtr stream = SDL3Helper.OpenAudioDeviceStream(devices.Keys.GetEnumerator().Current, ref spec, null);

            if (stream == IntPtr.Zero)
            {
                Console.WriteLine("Failed to open recording stream");
                SDL3Helper.QuitSDL();
                return;
            }

            // We'll use SDL_PauseAudioDevice / Resume directly via SDL to record and playback
            Console.WriteLine("Recording...");
            SDL3Helper.ResumeAudioStreamDevice(stream);
            SDL_Delay((uint)RECORD_SECONDS * 1000);
            SDL3Helper.PauseAudioStreamDevice(stream);

            Console.WriteLine("Playback after 5 seconds delay...");
            SDL_Delay(5000);

            SDL3Helper.ResumeAudioStreamDevice(stream);
            SDL_Delay((uint)RECORD_SECONDS * 1000);
            SDL3Helper.PauseAudioStreamDevice(stream);

            SDL3Helper.DestroyAudioStream(stream);
            SDL3Helper.QuitSDL();
        }
    }
}
