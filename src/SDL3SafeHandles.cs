/**
 * @file SDL3SafeHandles.cs
 *
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

using System;
using Microsoft.Win32.SafeHandles;
using static SDL3.SDL;

namespace SIPSorceryMedia.SDL3
{
    // Base class that centralizes SafeHandle behavior for SDL handles.
    // Subclasses must implement ReleaseHandleInternal to perform the correct native cleanup.
    public abstract class SafeSDLHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        protected SafeSDLHandle() : base(true) { }

        protected SafeSDLHandle(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }

        protected SafeSDLHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle)
        {
            SetHandle(handle);
        }

        // Derived types implement actual native release logic here.
        protected abstract bool ReleaseHandleInternal();

        protected override bool ReleaseHandle()
        {
            if (IsInvalid) return true;
            try
            {
                return ReleaseHandleInternal();
            }
            catch
            {
                // swallow exceptions from native cleanup to avoid throwing from finalizer
                return false;
            }
        }
    }

    // SafeHandle for SDL audio streams created by SDL_OpenAudioDeviceStream and freed with SDL_DestroyAudioStream
    public sealed class SDL3AudioStreamSafeHandle : SafeSDLHandle
    {
        public SDL3AudioStreamSafeHandle() : base() { }
        public SDL3AudioStreamSafeHandle(IntPtr handle) : base(handle) { }
        // Allow creating a non-owning wrapper over an existing native pointer (do not free in ReleaseHandle)
        public SDL3AudioStreamSafeHandle(IntPtr handle, bool ownsHandle) : base(handle, ownsHandle) { }

        protected override bool ReleaseHandleInternal()
        {
            try
            {
                // Call native destroy function
                SDL_DestroyAudioStream(handle);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
