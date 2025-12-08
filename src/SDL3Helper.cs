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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using static SDL3.SDL;

namespace SIPSorceryMedia.SDL3
{
    public static class SDL3Helper
    {
        private static bool _sdl3Initialised = false;
        private static readonly object _initLock = new object();

        // Keep native delegate instances alive for the lifetime of the stream
        private static readonly ConcurrentDictionary<IntPtr, SDL_AudioStreamCallback> _nativeCallbacks = new();

        // New managed callback that exposes SafeHandle instead of raw IntPtr
        public delegate void SDL_AudioStreamHandleCallback(IntPtr userdata, SDL3AudioStreamSafeHandle? stream, int additional_amount, int total_amount);

        // Preferred SafeHandle-based API with SafeHandle-aware callback
        public static SDL3AudioStreamSafeHandle? OpenAudioDeviceStreamHandle(uint deviceId, ref SDL_AudioSpec audioSpec, SDL_AudioStreamHandleCallback? callback)
        {
            SDL_AudioStreamCallback? nativeCallback = null;

            if (callback != null)
            {
                // create native callback that will convert IntPtr stream to a non-owning SafeHandle wrapper
                nativeCallback = (userdata, streamPtr, additional, total) =>
                {
                    SDL3AudioStreamSafeHandle? streamHandle = streamPtr == IntPtr.Zero ? null : new SDL3AudioStreamSafeHandle(streamPtr, false);
                    try
                    {
                        callback(userdata, streamHandle, additional, total);
                    }
                    catch
                    {
                        // swallow exceptions in callback
                    }
                };
            }

            var ptr = SDL_OpenAudioDeviceStream(deviceId, ref audioSpec, nativeCallback, IntPtr.Zero);
            if (ptr == IntPtr.Zero) return null;

            var safeHandle = new SDL3AudioStreamSafeHandle(ptr);

            if (nativeCallback != null)
            {
                // keep the native callback alive keyed by the native stream pointer
                _nativeCallbacks[ptr] = nativeCallback!;
            }

            return safeHandle;
        }

        public static void DestroyAudioStream(SDL3AudioStreamSafeHandle? streamHandle)
        {
            if (streamHandle == null) return;

            // remove native callback if present
            bool added = false;
            try
            {
                streamHandle.DangerousAddRef(ref added);
                var ptr = streamHandle.DangerousGetHandle();
                _nativeCallbacks.TryRemove(ptr, out _);
            }
            catch
            {
                // ignore
            }
            finally
            {
                if (added) streamHandle.DangerousRelease();
            }

            try { streamHandle.Dispose(); } catch { }
        }

        public static bool PauseAudioStreamDevice(SDL3AudioStreamSafeHandle? streamHandle)
        {
            if (streamHandle == null || streamHandle.IsInvalid) return false;
            bool added = false;
            try
            {
                streamHandle.DangerousAddRef(ref added);
                return SDL_PauseAudioStreamDevice(streamHandle.DangerousGetHandle());
            }
            finally
            {
                if (added) streamHandle.DangerousRelease();
            }
        }

        public static bool ResumeAudioStreamDevice(SDL3AudioStreamSafeHandle? streamHandle)
        {
            if (streamHandle == null || streamHandle.IsInvalid) return false;
            bool added = false;
            try
            {
                streamHandle.DangerousAddRef(ref added);
                return SDL_ResumeAudioStreamDevice(streamHandle.DangerousGetHandle());
            }
            finally
            {
                if (added) streamHandle.DangerousRelease();
            }
        }

        public static bool PutAudio(SDL3AudioStreamSafeHandle? streamHandle, IntPtr ptr, int bufferSize)
        {
            if (streamHandle == null || streamHandle.IsInvalid) return false;
            bool added = false;
            try
            {
                streamHandle.DangerousAddRef(ref added);
                return SDL_PutAudioStreamData(streamHandle.DangerousGetHandle(), ptr, bufferSize);
            }
            finally
            {
                if (added) streamHandle.DangerousRelease();
            }
        }

        public static unsafe bool PutAudioToStream(SDL3AudioStreamSafeHandle? streamHandle, ref byte[] data, int len)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (len < 0 || len > data.Length) throw new ArgumentOutOfRangeException(nameof(len));
            if (len == 0) return true;
            if (streamHandle == null || streamHandle.IsInvalid) return false;

            bool added = false;
            try
            {
                streamHandle.DangerousAddRef(ref added);
                fixed (byte* ptr = &data[0])
                {
                    return SDL_PutAudioStreamData(streamHandle.DangerousGetHandle(), (IntPtr)ptr, len);
                }
            }
            finally
            {
                if (added) streamHandle.DangerousRelease();
            }
        }

        public static int GetAudioStreamData(SDL3AudioStreamSafeHandle? streamHandle, IntPtr buf, int len)
        {
            if (streamHandle == null || streamHandle.IsInvalid) return 0;
            bool added = false;
            try
            {
                streamHandle.DangerousAddRef(ref added);
                return SDL_GetAudioStreamData(streamHandle.DangerousGetHandle(), buf, len);
            }
            finally
            {
                if (added) streamHandle.DangerousRelease();
            }
        }

        public static unsafe int GetAudioStreamData(SDL3AudioStreamSafeHandle? streamHandle, byte[] buf, int len)
        {
            if (buf is null) throw new ArgumentNullException(nameof(buf));
            if (len < 0 || len > buf.Length) throw new ArgumentOutOfRangeException(nameof(len));
            if (len == 0) return 0;
            if (streamHandle == null || streamHandle.IsInvalid) return 0;

            bool added = false;
            try
            {
                streamHandle.DangerousAddRef(ref added);
                fixed (byte* ptr = &buf[0])
                {
                    return SDL_GetAudioStreamData(streamHandle.DangerousGetHandle(), (IntPtr)ptr, len);
                }
            }
            finally
            {
                if (added) streamHandle.DangerousRelease();
            }
        }

        public static int GetAudioStreamQueued(SDL3AudioStreamSafeHandle? streamHandle)
        {
            if (streamHandle == null || streamHandle.IsInvalid) return 0;
            bool added = false;
            try
            {
                streamHandle.DangerousAddRef(ref added);
                return SDL_GetAudioStreamQueued(streamHandle.DangerousGetHandle());
            }
            finally
            {
                if (added) streamHandle.DangerousRelease();
            }
        }

        public static bool IsDevicePaused(uint deviceId) => SDL_AudioDevicePaused(deviceId);

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

        public static void InitSDL(SDL_InitFlags flags = SDL_InitFlags.SDL_INIT_AUDIO | SDL_InitFlags.SDL_INIT_TIMER)
        {
            lock (_initLock)
            {
                if (_sdl3Initialised) { return; }

                if (!SDL_Init(flags))
                {
                    string err = null;
                    try { err = SDL_GetError(); } catch { err = null; }
                    if (string.IsNullOrEmpty(err)) err = "Unknown SDL error";
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
            // Treat null or whitespace startWithName as "no filter" and return the default device.
            bool hasFilter = !string.IsNullOrWhiteSpace(startWithName);

            (uint, string)? result = null;

            int count;
            var devices = isRecording ? SDL_GetAudioRecordingDevices(out count) : SDL_GetAudioPlaybackDevices(out count);
            if (count > 0 && devices != null)
            {
                uint defaultDevice = isRecording ? SDL_AUDIO_DEVICE_DEFAULT_RECORDING : SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK;
                result = (defaultDevice, string.Empty);

                if (hasFilter && count > 1)
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
