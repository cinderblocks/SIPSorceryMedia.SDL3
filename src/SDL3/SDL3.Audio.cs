/**
 * @file SDL3.cs
 * @brief C# Bindings for SDL3
 *
 * Copyright 2024, Colin "cryy22" Jackson <c@cryy22.art>.
 * Copyright 2025, Sjofn LLC. - Backport to .NETStandard2.0
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 */

using System;
using System.Runtime.InteropServices;

namespace SDL3;

public static unsafe partial class SDL
{
    public enum SDL_AudioFormat
    {
        SDL_AUDIO_UNKNOWN = 0,
        SDL_AUDIO_U8 = 8,
        SDL_AUDIO_S8 = 32776,
        SDL_AUDIO_S16LE = 32784,
        SDL_AUDIO_S16BE = 36880,
        SDL_AUDIO_S32LE = 32800,
        SDL_AUDIO_S32BE = 36896,
        SDL_AUDIO_F32LE = 33056,
        SDL_AUDIO_F32BE = 37152,
        SDL_AUDIO_S16 = 32784,
        SDL_AUDIO_S32 = 32800,
        SDL_AUDIO_F32 = 33056,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_AudioSpec
    {
        public SDL_AudioFormat format;
        public int channels;
        public int freq;
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetNumAudioDrivers();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern string SDL_GetAudioDriver(int index);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern string SDL_GetCurrentAudioDriver();

    [DllImport(nativeLibName, EntryPoint = "SDL_GetAudioPlaybackDevices", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr SDL_GetAudioPlaybackDevices_native(out int count);

    public static uint[] SDL_GetAudioPlaybackDevices(out int count)
    {
        IntPtr p = SDL_GetAudioPlaybackDevices_native(out count);
        return PtrToUint32ArrayAndFreeWithSDL(p, count);
    }

    [DllImport(nativeLibName, EntryPoint = "SDL_GetAudioRecordingDevices", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr SDL_GetAudioRecordingDevices_native(out int count);

    public static uint[] SDL_GetAudioRecordingDevices(out int count)
    {
        IntPtr p = SDL_GetAudioRecordingDevices_native(out count);
        return PtrToUint32ArrayAndFreeWithSDL(p, count);
    }

    public static string? SDL_GetAudioDeviceName(int devId)
    {
        IntPtr ptr;
        try
        {
            ptr = SDL_GetAudioDeviceName_Native(devId);
        }
        catch { return null; }
        if (ptr == IntPtr.Zero) { return null; }

        return Marshal.PtrToStringAnsi(ptr) ?? string.Empty;
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetAudioDeviceName")]
    private static extern IntPtr SDL_GetAudioDeviceName_Native(int devId);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetAudioDeviceFormat(uint devid, out SDL_AudioSpec spec, out int sample_frames);

    public static Span<int> SDL_GetAudioDeviceChannelMap(uint devid)
    {
        var result = SDL_GetAudioDeviceChannelMap(devid, out var count);
        return new Span<int>((void*)result, count);
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetAudioDeviceChannelMap(uint devid, out int count);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_OpenAudioDevice(uint devid, ref SDL_AudioSpec spec);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_IsAudioDevicePhysical(uint devid);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_IsAudioDevicePlayback(uint devid);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_PauseAudioDevice(uint dev);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ResumeAudioDevice(uint dev);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_AudioDevicePaused(uint dev);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float SDL_GetAudioDeviceGain(uint devid);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetAudioDeviceGain(uint devid, float gain);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_CloseAudioDevice(uint devid);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_BindAudioStreams(uint devid, Span<IntPtr> streams, int num_streams);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_BindAudioStream(uint devid, IntPtr stream);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_UnbindAudioStreams(Span<IntPtr> streams, int num_streams);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_UnbindAudioStream(IntPtr stream);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetAudioStreamDevice(IntPtr stream);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateAudioStream(ref SDL_AudioSpec src_spec, ref SDL_AudioSpec dst_spec);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetAudioStreamProperties(IntPtr stream);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetAudioStreamFormat(IntPtr stream, out SDL_AudioSpec src_spec,
        out SDL_AudioSpec dst_spec);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetAudioStreamFormat(IntPtr stream, ref SDL_AudioSpec src_spec,
        ref SDL_AudioSpec dst_spec);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float SDL_GetAudioStreamFrequencyRatio(IntPtr stream);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetAudioStreamFrequencyRatio(IntPtr stream, float ratio);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float SDL_GetAudioStreamGain(IntPtr stream);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetAudioStreamGain(IntPtr stream, float gain);

    public static Span<int> SDL_GetAudioStreamInputChannelMap(IntPtr stream)
    {
        var result = SDL_GetAudioStreamInputChannelMap(stream, out var count);
        return new Span<int>((void*)result, count);
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetAudioStreamInputChannelMap(IntPtr stream, out int count);

    public static Span<int> SDL_GetAudioStreamOutputChannelMap(IntPtr stream)
    {
        var result = SDL_GetAudioStreamOutputChannelMap(stream, out var count);
        return new Span<int>((void*)result, count);
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetAudioStreamOutputChannelMap(IntPtr stream, out int count);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetAudioStreamInputChannelMap(IntPtr stream, Span<int> chmap, int count);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetAudioStreamOutputChannelMap(IntPtr stream, Span<int> chmap, int count);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_PutAudioStreamData(IntPtr stream, IntPtr buf, int len);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetAudioStreamData(IntPtr stream, IntPtr buf, int len);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetAudioStreamAvailable(IntPtr stream);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetAudioStreamQueued(IntPtr stream);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_FlushAudioStream(IntPtr stream);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ClearAudioStream(IntPtr stream);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_PauseAudioStreamDevice(IntPtr stream);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ResumeAudioStreamDevice(IntPtr stream);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_AudioStreamDevicePaused(IntPtr stream);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_LockAudioStream(IntPtr stream);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_UnlockAudioStream(IntPtr stream);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SDL_AudioStreamCallback(IntPtr userdata, IntPtr stream, int additional_amount,
        int total_amount);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetAudioStreamGetCallback(IntPtr stream, SDL_AudioStreamCallback callback,
        IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetAudioStreamPutCallback(IntPtr stream, SDL_AudioStreamCallback callback,
        IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DestroyAudioStream(IntPtr stream);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_OpenAudioDeviceStream(uint devid, ref SDL_AudioSpec spec,
        SDL_AudioStreamCallback? callback, IntPtr userdata);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SDL_AudioPostmixCallback(IntPtr userdata, SDL_AudioSpec* spec, float* buffer, int buflen);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetAudioPostmixCallback(uint devid, SDL_AudioPostmixCallback callback,
        IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_LoadWAV_IO(IntPtr src, SDLBool closeio, out SDL_AudioSpec spec,
        out IntPtr audio_buf, out uint audio_len);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_LoadWAV(string path, out SDL_AudioSpec spec, out IntPtr audio_buf,
        out uint audio_len);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_MixAudio(IntPtr dst, IntPtr src, SDL_AudioFormat format, uint len, float volume);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ConvertAudioSamples(ref SDL_AudioSpec src_spec, IntPtr src_data, int src_len,
        ref SDL_AudioSpec dst_spec, IntPtr dst_data, out int dst_len);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern string SDL_GetAudioFormatName(SDL_AudioFormat format);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetSilenceValueForFormat(SDL_AudioFormat format);

    public const uint SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK =  0xFFFFFFFFu;
    public const uint SDL_AUDIO_DEVICE_DEFAULT_RECORDING = 0xFFFFFFFEu;
    public const ushort SDL_AUDIO_MASK_BITSIZE = 0xFF;
    public const ushort SDL_AUDIO_MASK_DATATYPE = (1 << 8);
    public const ushort SDL_AUDIO_MASK_ENDIAN = (1 << 12);
    public const ushort SDL_AUDIO_MASK_SIGNED = (1 << 15);

    public static ushort SDL_AUDIO_BITSIZE(ushort x)
    {
        return (ushort)(x & SDL_AUDIO_MASK_BITSIZE);
    }
}