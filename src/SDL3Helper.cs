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
using System.Runtime.InteropServices;

using static SDL3.SDL;

namespace SIPSorceryMedia.SDL3
{
    public static class SDL3Helper
    {
        private static bool _sdl3Initialised = false;
        private static readonly object _initLock = new object();

        // Keep native delegate instances alive for the lifetime of the stream
        private static readonly ConcurrentDictionary<IntPtr, SDL_AudioStreamCallback> _nativeCallbacks = new();
        // Keep managed SafeHandle-aware callbacks alive as well to make lifetime explicit
        private static readonly ConcurrentDictionary<IntPtr, SDL_AudioStreamHandleCallback?> _managedCallbacks = new();

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
                // also keep the managed wrapper callback alive explicitly
                _managedCallbacks[ptr] = callback;
            }

            return safeHandle;
        }

        // Encapsulate DangerousAddRef/DangerousGetHandle usage so callers don't repeat unsafe patterns.
        private static T WithHandle<T>(SDL3AudioStreamSafeHandle? streamHandle, Func<IntPtr, T> func, T defaultValue = default)
        {
            if (streamHandle == null || streamHandle.IsInvalid) return defaultValue;
            bool added = false;
            try
            {
                streamHandle.DangerousAddRef(ref added);
                var ptr = streamHandle.DangerousGetHandle();
                return func(ptr);
            }
            catch
            {
                return defaultValue;
            }
            finally
            {
                if (added) streamHandle.DangerousRelease();
            }
        }

        public static void DestroyAudioStream(SDL3AudioStreamSafeHandle? streamHandle)
        {
            if (streamHandle == null) return;

            // remove native callback if present
            WithHandle(streamHandle, ptr =>
            {
                _nativeCallbacks.TryRemove(ptr, out _);
                _managedCallbacks.TryRemove(ptr, out _);
                return true;
            });

            try { streamHandle.Dispose(); } catch { }
        }

        public static bool PauseAudioStreamDevice(SDL3AudioStreamSafeHandle? streamHandle)
        {
            if (streamHandle == null || streamHandle.IsInvalid) return false;
            return WithHandle(streamHandle, ptr => SDL_PauseAudioStreamDevice(ptr) ? true : false, false);
        }

        public static bool ResumeAudioStreamDevice(SDL3AudioStreamSafeHandle? streamHandle)
        {
            if (streamHandle == null || streamHandle.IsInvalid) return false;
            return WithHandle(streamHandle, ptr => SDL_ResumeAudioStreamDevice(ptr) ? true : false, false);
        }

        public static bool PutAudio(SDL3AudioStreamSafeHandle? streamHandle, IntPtr ptr, int bufferSize)
        {
            if (streamHandle == null || streamHandle.IsInvalid) return false;
            return WithHandle(streamHandle, streamPtr => SDL_PutAudioStreamData(streamPtr, ptr, bufferSize) ? true : false, false);
        }

        public static bool PutAudioToStream(SDL3AudioStreamSafeHandle? streamHandle, byte[] data, int len)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (len < 0 || len > data.Length) throw new ArgumentOutOfRangeException(nameof(len));
            if (len == 0) return true;

            return WithHandle(streamHandle, streamPtr =>
            {
                GCHandle gch = default;
                try
                {
                    gch = GCHandle.Alloc(data, GCHandleType.Pinned);
                    IntPtr dataPtr = gch.AddrOfPinnedObject();
                    bool result = SDL_PutAudioStreamData(streamPtr, dataPtr, len);
                    GC.KeepAlive(data);
                    return result;
                }
                finally
                {
                    if (gch.IsAllocated) gch.Free();
                }
            }, false);
        }

        public static int GetAudioStreamData(SDL3AudioStreamSafeHandle? streamHandle, IntPtr buf, int len)
        {
            if (streamHandle == null || streamHandle.IsInvalid) return 0;
            return WithHandle(streamHandle, ptr => SDL_GetAudioStreamData(ptr, buf, len), 0);
        }

        public static int GetAudioStreamData(SDL3AudioStreamSafeHandle? streamHandle, byte[] buf, int len)
        {
            if (buf is null) throw new ArgumentNullException(nameof(buf));
            if (len < 0 || len > buf.Length) throw new ArgumentOutOfRangeException(nameof(len));
            if (len == 0) return 0;

            return WithHandle(streamHandle, ptr =>
            {
                GCHandle gch = default;
                try
                {
                    gch = GCHandle.Alloc(buf, GCHandleType.Pinned);
                    IntPtr bufPtr = gch.AddrOfPinnedObject();
                    int result = SDL_GetAudioStreamData(ptr, bufPtr, len);
                    GC.KeepAlive(buf);
                    return result;
                }
                finally
                {
                    if (gch.IsAllocated) gch.Free();
                }
            }, 0);
        }

        public static int GetAudioStreamQueued(SDL3AudioStreamSafeHandle? streamHandle)
        {
            if (streamHandle == null || streamHandle.IsInvalid) return 0;
            return WithHandle(streamHandle, ptr => SDL_GetAudioStreamQueued(ptr), 0);
        }

        public static int GetAudioStreamAvailable(SDL3AudioStreamSafeHandle? streamHandle)
        {
            if (streamHandle == null || streamHandle.IsInvalid) return 0;
            return WithHandle(streamHandle, ptr => SDL_GetAudioStreamAvailable(ptr), 0);
        }

        public static bool SetAudioStreamFrequencyRatio(SDL3AudioStreamSafeHandle? streamHandle, float ratio)
        {
            if (streamHandle == null || streamHandle.IsInvalid) return false;
            return WithHandle<bool>(streamHandle, ptr => SDL_SetAudioStreamFrequencyRatio(ptr, ratio) ? true : false, false);
        }

        public static bool GetAudioStreamFormat(SDL3AudioStreamSafeHandle? streamHandle, out SDL_AudioSpec srcSpec, out SDL_AudioSpec dstSpec)
        {
            srcSpec = default;
            dstSpec = default;
            if (streamHandle == null || streamHandle.IsInvalid) return false;
            
            bool added = false;
            try
            {
                streamHandle.DangerousAddRef(ref added);
                var ptr = streamHandle.DangerousGetHandle();
                return SDL_GetAudioStreamFormat(ptr, out srcSpec, out dstSpec);
            }
            catch
            {
                return false;
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
            // Determine bytes per single sample (per channel) based on format.
            int bytesPerSingleSample;
            switch (sdlAudioSpec.format)
            {
                case SDL_AudioFormat.SDL_AUDIO_U8:
                case SDL_AudioFormat.SDL_AUDIO_S8:
                    bytesPerSingleSample = 1;
                    break;
                case SDL_AudioFormat.SDL_AUDIO_S16:
                case SDL_AudioFormat.SDL_AUDIO_S16BE:
                    bytesPerSingleSample = 2;
                    break;
                case SDL_AudioFormat.SDL_AUDIO_S32:
                case SDL_AudioFormat.SDL_AUDIO_S32BE:
                    bytesPerSingleSample = 4;
                    break;
                case SDL_AudioFormat.SDL_AUDIO_F32:
                case SDL_AudioFormat.SDL_AUDIO_F32BE:
                    bytesPerSingleSample = 4;
                    break;
                case SDL_AudioFormat.SDL_AUDIO_UNKNOWN:
                default:
                    // Fallback: try to extract bitsize using macro; if that fails assume 2 bytes (16-bit)
                    int bits = SDL_AUDIO_BITSIZE((ushort)sdlAudioSpec.format);
                    bytesPerSingleSample = (bits > 0) ? (bits / 8) : 2;
                    break;
            }

            return sdlAudioSpec.channels * bytesPerSingleSample;
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

            var devices = isRecording ? SDL_GetAudioRecordingDevices(out var count) : SDL_GetAudioPlaybackDevices(out count);

            if (count > 0)
            {
                int limit = Math.Min(count, devices.Length);
                for (int i = 0; i < limit; i++)
                {
                    try
                    {
                        var deviceId = devices[i];
                        if (result.ContainsKey(deviceId)) { continue; }

                        var name = SDL_GetAudioDeviceName((int)deviceId) ?? string.Empty;
                        if (name.Length == 0) { continue; }
                        result.Add(deviceId, name);
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

            var devices = isRecording ? SDL_GetAudioRecordingDevices(out var count) : SDL_GetAudioPlaybackDevices(out count);
            if (count > 0)
            {
                uint defaultDevice = isRecording ? SDL_AUDIO_DEVICE_DEFAULT_RECORDING : SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK;
                result = (defaultDevice, string.Empty);

                if (hasFilter && count > 1)
                {
                    // Use index-based name lookup to avoid passing device-id as 'index' to native call
                    int limit = Math.Min(count, devices.Length);
                    for (int i = 1; i < limit; i++)
                    {
                        try
                        {
                            var deviceId = devices[i];
                            var deviceName = SDL_GetAudioDeviceName((int)deviceId) ?? string.Empty;
                            if (deviceName.StartsWith(startWithName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                return (deviceId, deviceName);
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

            var devices = isRecording ? SDL_GetAudioRecordingDevices(out var count) : SDL_GetAudioPlaybackDevices(out count);
            if (count > 0)
            {
                uint defaultDevice = isRecording ? SDL_AUDIO_DEVICE_DEFAULT_RECORDING : SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK;
                result = (defaultDevice, string.Empty);

                if (index < count && devices.Length > index)
                {
                    try
                    {
                        var deviceId = devices[index];
                        var name = SDL_GetAudioDeviceName((int)deviceId) ?? string.Empty;
                        result = (deviceId, name);
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

        /// <summary>
        /// Copy from a source buffer into a rented destination buffer while pinning the source briefly.
        /// This centralizes pinning and ensures the source memory won't be moved during the copy.
        /// </summary>
        public static void CopyToRentedBuffer(byte[] source, byte[] dest, int len)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (dest is null) throw new ArgumentNullException(nameof(dest));
            if (len < 0 || len > source.Length || len > dest.Length) throw new ArgumentOutOfRangeException(nameof(len));
            if (len == 0) return;

            GCHandle gch = default;
            try
            {
                gch = GCHandle.Alloc(source, GCHandleType.Pinned);
                IntPtr srcPtr = gch.AddrOfPinnedObject();
                Marshal.Copy(srcPtr, dest, 0, len);
                GC.KeepAlive(source);
            }
            finally
            {
                if (gch.IsAllocated) gch.Free();
            }
        }

        /// <summary>
        /// Convenience wrapper that pins the buffer and forwards to PutAudioToStream.
        /// </summary>
        public static bool PutAudioPinned(SDL3AudioStreamSafeHandle? streamHandle, byte[] data, int len)
        {
            return PutAudioToStream(streamHandle, data, len);
        }
    }
}
