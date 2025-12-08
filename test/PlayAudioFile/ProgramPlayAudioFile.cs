using SIPSorceryMedia.SDL3;
using System;
using System.Linq;
using static SDL3.SDL;

namespace PlayAudioFile
{
    internal class Program
    {
        // Path to a valid audio WAV path
        private const string WAV_FILE_PATH = "./../../../../../media/file_example_WAV_5MG.wav";
        //const String WAV_FILE_PATH = "./../../../../../media/file_example_WAV_1MG.wav";

        private static IntPtr audio_buffer;/* Pointer to wave data - uint8 */
        private static uint audio_len;     /* Length of wave data * - uint32 */
        private static int audio_pos;      /* Current play position */

        private static SDL_AudioSpec audio_spec;
        private static SDL_Event sdlEvent;
        private static uint deviceId; // SDL Device Id

        private static bool end_audio_file = false;

        // SafeHandle for the opened audio stream
        private static SDL3AudioStreamSafeHandle? _streamHandle;

        private static void Main(string[] args)
        {
            int deviceIndex = 0; // To store the index of the audio playback device selected

            Console.Clear();
            Console.WriteLine("\nTry to init SDL3 libraries - they must be stored in the same folder than this application");

            // Init SDL Library - Library files must be in the same folder as the application
            SDL3Helper.InitSDL();

            Console.WriteLine("\nInit done");

            // Get list of Audio Playback devices
            var sdlDevices = SDL3Helper.GetAudioPlaybackDevices();

            // Quit since no Audio playback found
            if (sdlDevices.Count == 0)
            {
                Console.WriteLine("No Audio playback devices found ...");
                SDL3Helper.QuitSDL();
                return;
            }

            // Allow end user to select Audio playback device
            if (sdlDevices?.Count > 0)
            {
                while (true)
                {

                    Console.WriteLine("\nSelect audio playback device:");
                    int index = 1;
                    foreach (var d in sdlDevices)
                    {
                        Console.Write($"\n [{index}] - {d.Value} ");
                        index++;
                    }
                    Console.WriteLine("\n");
                    Console.Out.Flush();

                    var keyConsole = Console.ReadKey();
                    if (int.TryParse("" + keyConsole.KeyChar, out int keyValue) && keyValue < index && keyValue >= 0)
                    {
                        deviceIndex = keyValue-1;
                        break;
                    }
                }
            }

            // Get name of the device
            var device = sdlDevices?.ElementAt(deviceIndex); // To store the name of the audio playback device selected
            Console.WriteLine($"\nDevice selected: {device.Value.Value}");

            // Open WAV file:
            if (!SDL_LoadWAV(WAV_FILE_PATH, out audio_spec, out audio_buffer, out audio_len))
            {
                Console.WriteLine("\nCannot open audio file - its format is not supported");
                SDL3Helper.QuitSDL();
                return;
            }

            // Check len of the WAV file
            if (audio_len == 0)
            {
                Console.WriteLine("\nAudio file not found - path is incorrect");
                SDL3Helper.QuitSDL();
                return;
            }

            // Open audio file and start to play Wav file
            deviceId = OpenAudioDevice(device.Value.Key);

            if (deviceId == 0)
            {
                Console.WriteLine("\nCannot open Audio device ...");
                SDL3Helper.QuitSDL();
                return;
            }

            Console.WriteLine($"\nPlaying file: {WAV_FILE_PATH}");

            SDL_FlushEvents((uint)SDL_EventType.SDL_EVENT_AUDIO_DEVICE_ADDED, (uint)SDL_EventType.SDL_EVENT_AUDIO_DEVICE_REMOVED);
            while (!end_audio_file)
            {

                while (SDL_PollEvent(out sdlEvent))
                {
                    if (sdlEvent.type == (uint)SDL_EventType.SDL_EVENT_QUIT)
                    {
                        end_audio_file = true;
                    }
                }
                SDL_Delay(100);
            }

            // Free WAV file
            SDL_free(audio_buffer);

            // Dispose stream handle using helper to unregister callbacks
            try { SDL3Helper.DestroyAudioStream(_streamHandle); } catch { }

            // Close audio file
            CloseAudioDevice(deviceId);

            // Quit SDL Library
            SDL3Helper.QuitSDL();
        }

        private static void CloseAudioDevice(uint id)
        {
            if (id != 0)
                SDL_CloseAudioDevice(id);
        }

        private static uint OpenAudioDevice(uint id)
        {
            _streamHandle = SDL3Helper.OpenAudioDeviceStreamHandle(id, ref audio_spec, FeedAudioCallback);
            if (_streamHandle != null && !_streamHandle.IsInvalid)
            {
                /* Let the audio run */
                SDL3Helper.ResumeAudioStreamDevice(_streamHandle);
            }
            return id;
        }

        private static void FeedAudioCallback(IntPtr userdata, SDL3AudioStreamSafeHandle? stream, int additionalAmount, int totalAmount)
        {
            if (end_audio_file) { return; }

            /* Set up the pointers */
            var waveptr = IntPtr.Add(audio_buffer, audio_pos); // Uint8
            var waveleft = (int)(audio_len - audio_pos);

            // prefer provided stream handle, fallback to stored handle
            SDL3AudioStreamSafeHandle? handleToUse = stream ?? _streamHandle;
            if (handleToUse == null || handleToUse.IsInvalid) return;

            if (waveleft <= additionalAmount)
            {
                SDL3Helper.PutAudio(handleToUse, waveptr, waveleft);
                audio_pos = 0;
                end_audio_file = true;
            }
            else
            {
                SDL3Helper.PutAudio(handleToUse, waveptr, additionalAmount);
                audio_pos += additionalAmount;
            }
        }
    }
}
