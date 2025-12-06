/**
 * @file SDL3Helper.cs
 * @brief Helper classes for SDL3
 *
 * Copyright 2025, Christophe Irles.
 * Copyright 2025, Sjofn LLC.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice,
 *    this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 *
 * 3. Neither the name of the copyright holder nor the names of its
 *    contributors may be used to endorse or promote products derived from this
 *    software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS”
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using SIPSorceryMedia.Abstractions;
using System;
using System.Collections.Generic;

using static SDL3.SDL;


namespace SIPSorceryMedia.SDL3
{
    public class SDL3Helper
    {
        private static bool _sdl3Initialised = false;

        public static bool PauseAudioStreamDevice(IntPtr stream) => SDL_PauseAudioStreamDevice(stream);

        public static bool ResumeAudioStreamDevice(IntPtr stream) => SDL_ResumeAudioStreamDevice(stream);

        public static bool IsDevicePaused(uint deviceId) => (SDL_AudioDevicePaused(deviceId));

        public static (uint id, string name)? GetAudioRecordingDevice(string startWithName) => GetAudioDevice(startWithName, true);

        public static (uint id, string name)? GetAudioPlaybackDevice(string startWithName) => GetAudioDevice(startWithName, false);

        public static (uint id, string name)? GetAudioRecordingDevice(int index) => GetAudioDevice(index, true);

        public static (uint id, string name)? GetAudioPlaybackDevice(int index) => GetAudioDevice(index, false);

        public static Dictionary<uint, string> GetAudioPlaybackDevices() => GetAudioDevices(false);

        public static Dictionary<uint, string> GetAudioRecordingDevices() => GetAudioDevices(true);

        public static SDL_AudioSpec GetAudioSpec(int clockRate = AudioFormat.DEFAULT_CLOCK_RATE, byte channels = 1)
        {
            SDL_AudioSpec desiredPlaybackSpec = new SDL_AudioSpec
            {
                freq = clockRate,
                format = SDL_AudioFormat.SDL_AUDIO_S16,
                channels = channels
            };

            return desiredPlaybackSpec;
        }

        public static int GetBytesPerSample(SDL_AudioSpec sdlAudioSpec)
        {
            //Calculate per sample bytes
            return sdlAudioSpec.channels * (SDL_AUDIO_BITSIZE((ushort)sdlAudioSpec.format) / 8);
        }

        public static int GetBytesPerSecond(SDL_AudioSpec sdlAudioSpec)
        {
            //Calculate bytes per second
            return sdlAudioSpec.freq * GetBytesPerSample(sdlAudioSpec);
        }

        public static bool PutAudio(IntPtr stream, IntPtr ptr, int bufferSize) => SDL_PutAudioStreamData(stream, ptr, bufferSize);

        public static int GetAudioStreamQueued(IntPtr stream) => SDL_GetAudioStreamQueued(stream);

        public static void Delay(uint ms) => SDL_Delay(ms);

        public static IntPtr OpenAudioDeviceStream(uint deviceId, ref SDL_AudioSpec audioSpec, SDL_AudioStreamCallback callback) =>
            SDL_OpenAudioDeviceStream(deviceId, ref audioSpec, callback, IntPtr.Zero);

        public static void DestroyAudioStream(IntPtr stream) => SDL_DestroyAudioStream(stream);

        public static unsafe void PutAudioToStream(IntPtr stream, ref byte[] data, int len)
        {
            fixed (byte* ptr = &data[0])
                SDL_PutAudioStreamData(stream, (IntPtr)ptr, len);
        }

        // Wrapper to read data from an audio stream
        public static unsafe int GetAudioStreamData(IntPtr stream, IntPtr buf, int len) => SDL_GetAudioStreamData(stream, buf, len);

        public static void InitSDL(SDL_InitFlags flags = SDL_InitFlags.SDL_INIT_AUDIO | SDL_InitFlags.SDL_INIT_TIMER)
        {
            if (_sdl3Initialised) { return; }

            if (!SDL_Init(flags))
            {
                throw new ApplicationException($"Cannot initialized SDL for Audio purpose");
            }
            _sdl3Initialised = true;
        }

        public static void QuitSDL()
        {
            if (!_sdl3Initialised) { return; }

            SDL_Quit();
            _sdl3Initialised = false;
        }

        #region PRIVATE methods

        private static Dictionary<uint, string> GetAudioDevices(bool isRecording)
        {
            Dictionary<uint, string> result = new Dictionary<uint, string>();

            //Get device count
            int count;
            var devices = isRecording ? SDL_GetAudioRecordingDevices(out count) : SDL_GetAudioPlaybackDevices(out count);

            if (count > 0)
            {
                foreach (var device in devices)
                {
                    var name = SDL_GetAudioDeviceName(device);
                    if (!string.IsNullOrEmpty(name))
                        result.Add(device, name);
                }
            }
            return result;
        }

        private static (uint id, string name)? GetAudioDevice(string startWithName, bool isRecording)
        {
            (uint, string)? result = null;

            //Get recording device count
            int count;
            var devices = isRecording ? SDL_GetAudioRecordingDevices(out count) : SDL_GetAudioPlaybackDevices(out count);
            if (count > 0)
            {
                uint defaultDevice = isRecording
                    ? SDL_AUDIO_DEVICE_DEFAULT_RECORDING
                    : SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK;
                result = (defaultDevice, SDL_GetAudioDeviceName(defaultDevice));

                if (count > 1)
                {
                    for (int i = 1; i < devices.Length; i++)
                    {
                        var deviceName = SDL_GetAudioDeviceName(devices[i]);
                        if (deviceName.StartsWith(startWithName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return (devices[i], deviceName);
                        }
                    }
                }

            }
            return result;
        }

        private static (uint id, string name)? GetAudioDevice(int index, bool isRecording)
        {
            (uint, string)? result = null;

            int count;
            var devices = isRecording ? SDL_GetAudioRecordingDevices(out count) : SDL_GetAudioPlaybackDevices(out count);
            if (count > 0)
            {
                if (index < count)
                {
                    result = (devices[index], SDL_GetAudioDeviceName(devices[index]));
                }
            }
            return result;
        }

        #endregion PRIVATE methods
    }
}
