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
using System.Text;

namespace SDL3;

public static unsafe partial class SDL
{
    public readonly record struct SDLBool
    {
        private readonly byte value;

        internal const byte FALSE_VALUE = 0;
        internal const byte TRUE_VALUE = 1;

        internal SDLBool(byte value)
        {
            this.value = value;
        }

        public static implicit operator bool(SDLBool b)
        {
            return b.value != FALSE_VALUE;
        }

        public static implicit operator SDLBool(bool b)
        {
            return new SDLBool(b ? TRUE_VALUE : FALSE_VALUE);
        }

        public bool Equals(SDLBool other)
        {
            return other.value == value;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }

    public static IntPtr Utf8StringToNative(string? managed)
    {
        if (managed is null)
        {
            return IntPtr.Zero;
        }

        var bytes = Encoding.UTF8.GetBytes(managed);
        var ptr = Marshal.AllocHGlobal(bytes.Length + 1);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        Marshal.WriteByte(ptr, bytes.Length, 0);
        return ptr;
    }

    public static void FreeNativeUtf8(IntPtr nativePtr)
    {
        if (nativePtr == IntPtr.Zero)
        {
            return;
        }

        Marshal.FreeHGlobal(nativePtr);
    }

    public static string PtrToStringUtf8(IntPtr nativePtr)
    {
        if (nativePtr == IntPtr.Zero)
        {
            return string.Empty;
        }

        int len = 0;
        while (Marshal.ReadByte(nativePtr, len) != 0)
        {
            len++;
        }

        if (len == 0)
        {
            return string.Empty;
        }

        var buffer = new byte[len];
        Marshal.Copy(nativePtr, buffer, 0, len);
        return Encoding.UTF8.GetString(buffer);
    }

    public static string PtrToStringUtf8AndFreeWithSDL(IntPtr nativePtr)
    {
        if (nativePtr == IntPtr.Zero)
        {
            return string.Empty;
        }

        var s = PtrToStringUtf8(nativePtr);
        SDL_free(nativePtr);
        return s;
    }

    public static System.Collections.Generic.List<uint> PtrToUint32List(IntPtr nativePtr, int count)
    {
        var list = new System.Collections.Generic.List<uint>(count > 0 ? count : 0);
        if (nativePtr == IntPtr.Zero || count <= 0)
        {
            return list;
        }

        var tmp = new int[count];
        Marshal.Copy(nativePtr, tmp, 0, count);
        for (int i = 0; i < count; i++)
        {
            list.Add(unchecked((uint)tmp[i]));
        }

        return list;
    }

    public static uint[] PtrToUint32Array(IntPtr nativePtr, int count)
    {
        if (nativePtr == IntPtr.Zero || count <= 0)
        {
            return Array.Empty<uint>();
        }

        var tmp = new int[count];
        Marshal.Copy(nativePtr, tmp, 0, count);
        var result = new uint[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = unchecked((uint)tmp[i]);
        }

        return result;
    }

    public static uint[] PtrToUint32ArrayAndFreeWithSDL(IntPtr nativePtr, int count)
    {
        if (nativePtr == IntPtr.Zero)
        {
            return Array.Empty<uint>();
        }

        var arr = PtrToUint32Array(nativePtr, count);
        SDL_free(nativePtr);
        return arr;
    }

    public static unsafe Span<uint> PtrToUint32Span(IntPtr nativePtr, int count)
    {
        if (nativePtr == IntPtr.Zero || count <= 0) return Span<uint>.Empty;
        return new Span<uint>((void*)nativePtr, count);
    }
}
