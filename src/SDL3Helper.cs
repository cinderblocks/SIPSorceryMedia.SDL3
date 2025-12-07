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
        private static readonly object _initLock = new object();

        public static bool PauseAudioStreamDevice(IntPtr stream) => SDL_PauseAudioStreamDevice(stream);

        public static bool ResumeAudioStreamDevice(IntPtr stream) => SDL_ResumeAudioStreamDevice(stream);

        public static bool IsDevicePaused(uint deviceId) => (SDL_AudioDevicePaused(deviceId));

        public static (uint id, string name)? GetAudioRecordingDevice(string startWithName) => GetAudioDevice(startWithName, true);

        public static (uint id, string name)? GetAudioPlaybackDevice(string startWithName) => GetAudioDevice(startWithName, false);

        public static (uint id, string name)? GetAudioRecordingDevice(int index) => GetAudioDevice(index, true);

        public static (uint id, string name)? GetAudioPlaybackDevice(int index) => GetAudioDevice(index, false);

        public static IReadOnlyDictionary<uint, string> GetAudioPlaybackDevices() => GetAudioDevices(false);

        public static IReadOnlyDictionary<uint, string> GetAudioRecordingDevices() => GetAudioDevices(true);

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
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (len < 0 || len > data.Length) throw new ArgumentOutOfRangeException(nameof(len));
            if (len == 0) return;

            fixed (byte* ptr = &data[0])
            {
                SDL_PutAudioStreamData(stream, (IntPtr)ptr, len);
            }
        }

        // Wrapper to read data from an audio stream
        public static unsafe int GetAudioStreamData(IntPtr stream, IntPtr buf, int len) => SDL_GetAudioStreamData(stream, buf, len);

        // Overload to read directly into a managed byte[] (pins internally)
        public static unsafe int GetAudioStreamData(IntPtr stream, byte[] buf, int len)
        {
            if (buf is null) throw new ArgumentNullException(nameof(buf));
            if (len < 0 || len > buf.Length) throw new ArgumentOutOfRangeException(nameof(len));
            if (len == 0) return 0;

            fixed (byte* ptr = &buf[0])
            {
                return SDL_GetAudioStreamData(stream, (IntPtr)ptr, len);
            }
        }

        public static void InitSDL(SDL_InitFlags flags = SDL_InitFlags.SDL_INIT_AUDIO | SDL_InitFlags.SDL_INIT_TIMER)
        {
            lock (_initLock)
            {
                if (_sdl3Initialised) { return; }

                if (!SDL_Init(flags))
                {
                    // Use a more specific exception type and include the SDL error if available
                    var err = PtrToStringUtf8AndFreeWithSDL((IntPtr)0);
                    throw new InvalidOperationException($"Cannot initialize SDL for audio: {err}");
                }
                _sdl3Initialised = true;
            }
        }

        public static void QuitSDL()
        {
            lock (_initLock)
            {
                if (!_sdl3Initialised) { return; }

                SDL_Quit();
                _sdl3Initialised = false;
            }
        }

        #region PRIVATE methods

        private static IReadOnlyDictionary<uint, string> GetAudioDevices(bool isRecording)
        {
            var result = new Dictionary<uint, string>();

            //Get device count
            int count;
            var devices = isRecording ? SDL_GetAudioRecordingDevices(out count) : SDL_GetAudioPlaybackDevices(out count);

            if (count > 0 && devices != null)
            {
                for (int i = 0; i < devices.Length; i++)
                {
                    try
                    {
                        var device = devices[i];
                        if (result.ContainsKey(device)) continue; // avoid duplicates
                        var name = SDL_GetAudioDeviceName(device) ?? string.Empty;
                        if (name.Length == 0) continue;
                        result.Add(device, name);
                    }
                    catch
                    {
                        // Ignore individual device failures and continue
                    }
                }
            }
            return result;
        }

        private static (uint id, string name)? GetAudioDevice(string startWithName, bool isRecording)
        {
            if (string.IsNullOrWhiteSpace(startWithName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(startWithName));

            (uint, string)? result = null;

            //Get recording device count
            int count;
            var devices = isRecording ? SDL_GetAudioRecordingDevices(out count) : SDL_GetAudioPlaybackDevices(out count);
            if (count > 0 && devices != null)
            {
                uint defaultDevice = isRecording ? SDL_AUDIO_DEVICE_DEFAULT_RECORDING : SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK;
                result = (defaultDevice, string.Empty);

                if (count > 1)
                {
                    for (int i = 1; i < devices.Length; i++)
                    {
                        try
                        {
                            var deviceName = SDL_GetAudioDeviceName(devices[i]) ?? string.Empty;
                            if (deviceName.StartsWith(startWithName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                return (devices[i], deviceName);
                            }
                        }
                        catch
                        {
                            // ignore device name retrieval failures
                        }
                    }
                }

            }
            return result;
        }

        private static (uint id, string name)? GetAudioDevice(int index, bool isRecording)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));

            (uint, string)? result = null;

            int count;
            var devices = isRecording ? SDL_GetAudioRecordingDevices(out count) : SDL_GetAudioPlaybackDevices(out count);
            if (count > 0 && devices != null)
            {
                uint defaultDevice = isRecording ? SDL_AUDIO_DEVICE_DEFAULT_RECORDING : SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK;
                result = (defaultDevice, string.Empty);

                if (index < count && index >= 0 && devices.Length > index)
                {
                    try
                    {
                        result = (devices[index], SDL_GetAudioDeviceName(devices[index]) ?? string.Empty);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
            return result;
        }

        #endregion PRIVATE methods
    }
}
