using SIPSorceryMedia.Abstractions;
using System;
using System.Collections.Generic;

using static SDL3.SDL;


namespace SIPSorceryMedia.SDL3
{
    public class SDL3Helper
    {
        private static bool _sdl3Initialised = false;

        public static bool IsDeviceStopped(uint deviceId) => (SDL_GetAudioDeviceStatus(deviceId) == SDL_AudioStatus.SDL_AUDIO_STOPPED);

        public static bool IsDevicePaused(uint deviceId) => (SDL_GetAudioDeviceStatus(deviceId) == SDL_AudioStatus.SDL_AUDIO_PAUSED);

        public static bool IsDevicePlaying(uint deviceId) => (SDL_GetAudioDeviceStatus(deviceId) == SDL_AudioStatus.SDL_AUDIO_PLAYING);

        public static string ? GetAudioRecordingDevice(string startWithName) => GetAudioDevice(startWithName, true);

        public static  string? GetAudioPlaybackDevice(string startWithName) => GetAudioDevice(startWithName, false);

        public static string? GetAudioRecordingDevice(int index) => GetAudioDevice(index, true);

        public static string? GetAudioPlaybackDevice(int index) => GetAudioDevice(index, false);

        public static List<string> GetAudioPlaybackDevices() => GetAudioDevices(false);

        public static List<string> GetAudioRecordingDevices() => GetAudioDevices(true);

        public static SDL_AudioSpec GetAudioSpec(int clockRate = AudioFormat.DEFAULT_CLOCK_RATE, byte channels = 1, ushort samples = 960)
        {
            SDL_AudioSpec desiredPlaybackSpec = new SDL_AudioSpec();
            desiredPlaybackSpec.freq = clockRate;
            desiredPlaybackSpec.format = AUDIO_S16SYS;
            desiredPlaybackSpec.channels = channels; // Value returned by (byte)ffmpeg.av_get_channel_layout_nb_channels(ffmpeg.AV_CH_LAYOUT_MONO);
            desiredPlaybackSpec.silence = 0;
            desiredPlaybackSpec.samples = samples;
            //desiredPlaybackSpec.userdata = null;

            return desiredPlaybackSpec;
        }

        public static int GetBytesPerSample(SDL_AudioSpec sdlAudioSpec)
        {
            //Calculate per sample bytes
            return sdlAudioSpec.channels * (SDL_AUDIO_BITSIZE(sdlAudioSpec.format) / 8);
        }

        public static int GetBytesPerSecond(SDL_AudioSpec sdlAudioSpec)
        {
            //Calculate bytes per second
            return sdlAudioSpec.freq * GetBytesPerSample(sdlAudioSpec);
        }

        public static uint GetQueuedAudioSize(uint deviceID) => SDL_GetQueuedAudioSize(deviceID);

        public static uint DequeueAudio(uint deviceID, IntPtr ptr, uint bufferSize) => SDL_DequeueAudio(deviceID, ptr, bufferSize);

        public static void Delay(uint ms) => SDL_Delay(ms);

        public static uint OpenAudioPlaybackDevice(string deviceName, ref SDL_AudioSpec audioSpec) => SDL_OpenAudioDevice(deviceName, SDL_FALSE, ref audioSpec, out SDL_AudioSpec receivedPlaybackSpec, SDL_FALSE);

        public static uint OpenAudioRecordingDevice(string deviceName, ref SDL_AudioSpec audioSpec) => SDL_OpenAudioDevice(deviceName, SDL_TRUE, ref audioSpec, out SDL_AudioSpec receivedPlaybackSpec, SDL_FALSE);

        public static void CloseAudioPlaybackDevice(uint deviceId) => CloseAudioDevice(deviceId);

        public static void CloseAudioRecordingDevice(uint deviceId) => CloseAudioDevice(deviceId);

        public static void PauseAudioPlaybackDevice(uint deviceId, bool pauseOn = true) => SDL_PauseAudioDevice(deviceId, pauseOn ? SDL_TRUE : SDL_FALSE);

        public static void PauseAudioRecordingDevice(uint deviceId, bool pauseOn = true) => SDL_PauseAudioDevice(deviceId, pauseOn ? SDL_TRUE : SDL_FALSE);

        public static unsafe void QueueAudioPlaybackDevice(uint deviceId, ref byte[] data, uint len)
        {
            fixed (byte* ptr = &data[0])
                SDL_QueueAudio(deviceId, (IntPtr)ptr, len);
        }

        public static void InitSDL(uint flags = SDL_INIT_AUDIO | SDL_INIT_TIMER)
        {
            if (!_sdl3Initialised)
            {
                //SDL_SetHint(SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");

                if (SDL_Init(flags) < 0)
                    throw new ApplicationException($"Cannot initialized SDL for Audio purpose");

                _sdl3Initialised = true;
            }
        }

        public static void QuitSDL()
        {
            if (_sdl3Initialised)
            {
                SDL_Quit();
                _sdl3Initialised = false;
            }
        }

#region PRIVATE methods

        private static List<string> GetAudioDevices(bool isCapture)
        {
            List<string> result = new List<string>();
            int isCaptureDevice = isCapture ? SDL_TRUE : SDL_FALSE;

            //Get capture device count
            int deviceCount = SDL_GetNumAudioDevices(isCaptureDevice);

            if (deviceCount > 0)
            {
                string name;
                for (int index = 0; index < deviceCount; index++)
                {
                    name = SDL_GetAudioDeviceName(index, isCaptureDevice);
                    if (!string.IsNullOrEmpty(name))
                        result.Add(name);
                }
            }
            return result;
        }

        private static string? GetAudioDevice(string startWithName, bool isCapture)
        {
            string ? result = null;
            int isCaptureDevice = isCapture ? SDL_TRUE : SDL_FALSE;

            //Get capture device count
            int deviceCount = SDL_GetNumAudioDevices(isCaptureDevice);

            if (deviceCount > 0)
            {
                result = SDL_GetAudioDeviceName(0, isCaptureDevice);

                if (deviceCount > 1)
                {
                    for (int index = 1; index < deviceCount; index++)
                    {
                        string deviceName = SDL_GetAudioDeviceName(index, isCaptureDevice);
                        if (deviceName.StartsWith(startWithName, StringComparison.InvariantCultureIgnoreCase))
                            return deviceName;
                    }
                }
                
            }
            return result;
        }

        private static string? GetAudioDevice(int index, bool isCapture)
        {
            string? result = null;
            int isCaptureDevice = isCapture ? SDL_TRUE : SDL_FALSE;

            //Get capture device count
            int deviceCount = SDL_GetNumAudioDevices(isCaptureDevice);

            if ( (deviceCount > 0) && (index < deviceCount) )
            {
                return SDL_GetAudioDeviceName(index, isCaptureDevice);
            }
            return result;
        }

        private static void CloseAudioDevice(uint deviceId) =>  SDL_CloseAudioDevice(deviceId);

#endregion PRIVATE methods
    }
}
