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
    private const string nativeLibName = "SDL3";

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_memcpy(IntPtr dst, IntPtr src, IntPtr len);

    // /usr/local/include/SDL3/SDL_stdinc.h

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_malloc(UIntPtr size);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_free(IntPtr mem);

    // /usr/local/include/SDL3/SDL_assert.h

    public enum SDL_AssertState
    {
        SDL_ASSERTION_RETRY = 0,
        SDL_ASSERTION_BREAK = 1,
        SDL_ASSERTION_ABORT = 2,
        SDL_ASSERTION_IGNORE = 3,
        SDL_ASSERTION_ALWAYS_IGNORE = 4,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_AssertData
    {
        public SDLBool always_ignore;
        public uint trigger_count;
        public byte* condition;
        public byte* filename;
        public int linenum;
        public byte* function;
        public SDL_AssertData* next;
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_AssertState SDL_ReportAssertion(ref SDL_AssertData data, string func, string file, int line);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate SDL_AssertState SDL_AssertionHandler(SDL_AssertData* data, IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetAssertionHandler(SDL_AssertionHandler handler, IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_AssertionHandler SDL_GetDefaultAssertionHandler();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_AssertionHandler SDL_GetAssertionHandler(out IntPtr puserdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_AssertData* SDL_GetAssertionReport();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_ResetAssertionReport();

    // /usr/local/include/SDL3/SDL_asyncio.h

    public enum SDL_AsyncIOTaskType
    {
        SDL_ASYNCIO_TASK_READ = 0,
        SDL_ASYNCIO_TASK_WRITE = 1,
        SDL_ASYNCIO_TASK_CLOSE = 2,
    }

    public enum SDL_AsyncIOResult
    {
        SDL_ASYNCIO_COMPLETE = 0,
        SDL_ASYNCIO_FAILURE = 1,
        SDL_ASYNCIO_CANCELED = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_AsyncIOOutcome
    {
        public IntPtr asyncio;
        public SDL_AsyncIOTaskType type;
        public SDL_AsyncIOResult result;
        public IntPtr buffer;
        public ulong offset;
        public ulong bytes_requested;
        public ulong bytes_transferred;
        public IntPtr userdata;
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern long SDL_GetAsyncIOSize(IntPtr asyncio);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ReadAsyncIO(IntPtr asyncio, IntPtr ptr, ulong offset, ulong size, IntPtr queue, IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WriteAsyncIO(IntPtr asyncio, IntPtr ptr, ulong offset, ulong size, IntPtr queue, IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_CloseAsyncIO(IntPtr asyncio, SDLBool flush, IntPtr queue, IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateAsyncIOQueue();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DestroyAsyncIOQueue(IntPtr queue);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetAsyncIOResult(IntPtr queue, out SDL_AsyncIOOutcome outcome);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WaitAsyncIOResult(IntPtr queue, out SDL_AsyncIOOutcome outcome, int timeoutMS);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SignalAsyncIOQueue(IntPtr queue);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_LoadFileAsync(string file, IntPtr queue, IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_AsyncIOFromFile(string file, string mode);

    // /usr/local/include/SDL3/SDL_atomic.h

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_TryLockSpinlock(IntPtr @lock);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_LockSpinlock(IntPtr @lock);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_UnlockSpinlock(IntPtr @lock);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_MemoryBarrierReleaseFunction();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_MemoryBarrierAcquireFunction();

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_AtomicInt
    {
        public int value;
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_CompareAndSwapAtomicInt(ref SDL_AtomicInt a, int oldval, int newval);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_SetAtomicInt(ref SDL_AtomicInt a, int v);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetAtomicInt(ref SDL_AtomicInt a);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_AddAtomicInt(ref SDL_AtomicInt a, int v);

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_AtomicU32
    {
        public uint value;
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_CompareAndSwapAtomicU32(ref SDL_AtomicU32 a, uint oldval, uint newval);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_SetAtomicU32(ref SDL_AtomicU32 a, uint v);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetAtomicU32(ref SDL_AtomicU32 a);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_CompareAndSwapAtomicPointer(ref IntPtr a, IntPtr oldval, IntPtr newval);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_SetAtomicPointer(ref IntPtr a, IntPtr v);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetAtomicPointer(ref IntPtr a);

    // /usr/local/include/SDL3/SDL_endian.h

    // /usr/local/include/SDL3/SDL_error.h

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetError(string fmt);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_OutOfMemory();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] 
    public static extern string SDL_GetError();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ClearError();

    // /usr/local/include/SDL3/SDL_properties.h

    public enum SDL_PropertyType
    {
        SDL_PROPERTY_TYPE_INVALID = 0,
        SDL_PROPERTY_TYPE_POINTER = 1,
        SDL_PROPERTY_TYPE_STRING = 2,
        SDL_PROPERTY_TYPE_NUMBER = 3,
        SDL_PROPERTY_TYPE_FLOAT = 4,
        SDL_PROPERTY_TYPE_BOOLEAN = 5,
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetGlobalProperties();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_CreateProperties();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_CopyProperties(uint src, uint dst);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_LockProperties(uint props);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_UnlockProperties(uint props);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SDL_CleanupPropertyCallback(IntPtr userdata, IntPtr value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetPointerPropertyWithCleanup(uint props, string name, IntPtr value, SDL_CleanupPropertyCallback cleanup, IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetPointerProperty(uint props, string name, IntPtr value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetStringProperty(uint props, string name, string value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetNumberProperty(uint props, string name, long value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetFloatProperty(uint props, string name, float value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetBooleanProperty(uint props, string name, SDLBool value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasProperty(uint props, string name);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_PropertyType SDL_GetPropertyType(uint props, string name);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetPointerProperty(uint props, string name, IntPtr default_value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] public static extern string SDL_GetStringProperty(uint props, string name, string default_value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern long SDL_GetNumberProperty(uint props, string name, long default_value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float SDL_GetFloatProperty(uint props, string name, float default_value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetBooleanProperty(uint props, string name, SDLBool default_value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ClearProperty(uint props, string name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SDL_EnumeratePropertiesCallback(IntPtr userdata, uint props, byte* name);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_EnumerateProperties(uint props, SDL_EnumeratePropertiesCallback callback, IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DestroyProperties(uint props);

    // /usr/local/include/SDL3/SDL_thread.h

    public const string SDL_PROP_THREAD_CREATE_ENTRY_FUNCTION_POINTER = "SDL.thread.create.entry_function";
    public const string SDL_PROP_THREAD_CREATE_NAME_STRING = "SDL.thread.create.name";
    public const string SDL_PROP_THREAD_CREATE_USERDATA_POINTER = "SDL.thread.create.userdata";
    public const string SDL_PROP_THREAD_CREATE_STACKSIZE_NUMBER = "SDL.thread.create.stacksize";

    public enum SDL_ThreadPriority
    {
        SDL_THREAD_PRIORITY_LOW = 0,
        SDL_THREAD_PRIORITY_NORMAL = 1,
        SDL_THREAD_PRIORITY_HIGH = 2,
        SDL_THREAD_PRIORITY_TIME_CRITICAL = 3,
    }

    public enum SDL_ThreadState
    {
        SDL_THREAD_UNKNOWN = 0,
        SDL_THREAD_ALIVE = 1,
        SDL_THREAD_DETACHED = 2,
        SDL_THREAD_COMPLETE = 3,
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int SDL_ThreadFunction(IntPtr data);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateThreadRuntime(SDL_ThreadFunction fn, string name, IntPtr data, IntPtr pfnBeginThread, IntPtr pfnEndThread);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateThreadWithPropertiesRuntime(uint props, IntPtr pfnBeginThread, IntPtr pfnEndThread);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] public static extern string SDL_GetThreadName(IntPtr thread);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong SDL_GetCurrentThreadID();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong SDL_GetThreadID(IntPtr thread);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetCurrentThreadPriority(SDL_ThreadPriority priority);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_WaitThread(IntPtr thread, IntPtr status);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_ThreadState SDL_GetThreadState(IntPtr thread);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DetachThread(IntPtr thread);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetTLS(IntPtr id);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SDL_TLSDestructorCallback(IntPtr value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetTLS(IntPtr id, IntPtr value, SDL_TLSDestructorCallback destructor);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_CleanupTLS();

    // /usr/local/include/SDL3/SDL_mutex.h

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateMutex();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_LockMutex(IntPtr mutex);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_TryLockMutex(IntPtr mutex);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_UnlockMutex(IntPtr mutex);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DestroyMutex(IntPtr mutex);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateRWLock();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_LockRWLockForReading(IntPtr rwlock);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_LockRWLockForWriting(IntPtr rwlock);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_TryLockRWLockForReading(IntPtr rwlock);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_TryLockRWLockForWriting(IntPtr rwlock);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_UnlockRWLock(IntPtr rwlock);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DestroyRWLock(IntPtr rwlock);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateSemaphore(uint initial_value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DestroySemaphore(IntPtr sem);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_WaitSemaphore(IntPtr sem);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_TryWaitSemaphore(IntPtr sem);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WaitSemaphoreTimeout(IntPtr sem, int timeoutMS);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SignalSemaphore(IntPtr sem);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetSemaphoreValue(IntPtr sem);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateCondition();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DestroyCondition(IntPtr cond);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SignalCondition(IntPtr cond);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_BroadcastCondition(IntPtr cond);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_WaitCondition(IntPtr cond, IntPtr mutex);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WaitConditionTimeout(IntPtr cond, IntPtr mutex, int timeoutMS);

    public enum SDL_InitStatus
    {
        SDL_INIT_STATUS_UNINITIALIZED = 0,
        SDL_INIT_STATUS_INITIALIZING = 1,
        SDL_INIT_STATUS_INITIALIZED = 2,
        SDL_INIT_STATUS_UNINITIALIZING = 3,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_InitState
    {
        public SDL_AtomicInt status;
        public ulong thread;
        public IntPtr reserved;
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ShouldInit(ref SDL_InitState state);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ShouldQuit(ref SDL_InitState state);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetInitialized(ref SDL_InitState state, SDLBool initialized);

    // /usr/local/include/SDL3/SDL_iostream.h

    public const string SDL_PROP_IOSTREAM_WINDOWS_HANDLE_POINTER = "SDL.iostream.windows.handle";
    public const string SDL_PROP_IOSTREAM_STDIO_FILE_POINTER = "SDL.iostream.stdio.file";
    public const string SDL_PROP_IOSTREAM_FILE_DESCRIPTOR_NUMBER = "SDL.iostream.file_descriptor";
    public const string SDL_PROP_IOSTREAM_ANDROID_AASSET_POINTER = "SDL.iostream.android.aasset";
    public const string SDL_PROP_IOSTREAM_MEMORY_POINTER = "SDL.iostream.memory.base";
    public const string SDL_PROP_IOSTREAM_MEMORY_SIZE_NUMBER = "SDL.iostream.memory.size";
    public const string SDL_PROP_IOSTREAM_DYNAMIC_MEMORY_POINTER = "SDL.iostream.dynamic.memory";
    public const string SDL_PROP_IOSTREAM_DYNAMIC_CHUNKSIZE_NUMBER = "SDL.iostream.dynamic.chunksize";

    public enum SDL_IOStatus
    {
        SDL_IO_STATUS_READY = 0,
        SDL_IO_STATUS_ERROR = 1,
        SDL_IO_STATUS_EOF = 2,
        SDL_IO_STATUS_NOT_READY = 3,
        SDL_IO_STATUS_READONLY = 4,
        SDL_IO_STATUS_WRITEONLY = 5,
    }

    public enum SDL_IOWhence
    {
        SDL_IO_SEEK_SET = 0,
        SDL_IO_SEEK_CUR = 1,
        SDL_IO_SEEK_END = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_IOStreamInterface
    {
        public uint version;
        public IntPtr size; // WARN_ANONYMOUS_FUNCTION_POINTER
        public IntPtr seek; // WARN_ANONYMOUS_FUNCTION_POINTER
        public IntPtr read; // WARN_ANONYMOUS_FUNCTION_POINTER
        public IntPtr write; // WARN_ANONYMOUS_FUNCTION_POINTER
        public IntPtr flush; // WARN_ANONYMOUS_FUNCTION_POINTER
        public IntPtr close; // WARN_ANONYMOUS_FUNCTION_POINTER
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_IOFromFile(string file, string mode);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_IOFromMem(IntPtr mem, UIntPtr size);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_IOFromConstMem(IntPtr mem, UIntPtr size);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_IOFromDynamicMem();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_OpenIO(ref SDL_IOStreamInterface iface, IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_CloseIO(IntPtr context);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetIOProperties(IntPtr context);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_IOStatus SDL_GetIOStatus(IntPtr context);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern long SDL_GetIOSize(IntPtr context);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern long SDL_SeekIO(IntPtr context, long offset, SDL_IOWhence whence);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern long SDL_TellIO(IntPtr context);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern UIntPtr SDL_ReadIO(IntPtr context, IntPtr ptr, UIntPtr size);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern UIntPtr SDL_WriteIO(IntPtr context, IntPtr ptr, UIntPtr size);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern UIntPtr SDL_IOprintf(IntPtr context, string fmt);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_FlushIO(IntPtr context);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_LoadFile_IO(IntPtr src, out UIntPtr datasize, SDLBool closeio);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_LoadFile(string file, out UIntPtr datasize);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SaveFile_IO(IntPtr src, IntPtr data, UIntPtr datasize, SDLBool closeio);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SaveFile(string file, IntPtr data, UIntPtr datasize);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ReadU8(IntPtr src, out byte value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ReadS8(IntPtr src, out sbyte value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ReadU16LE(IntPtr src, out ushort value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ReadS16LE(IntPtr src, out short value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ReadU16BE(IntPtr src, out ushort value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ReadS16BE(IntPtr src, out short value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ReadU32LE(IntPtr src, out uint value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ReadS32LE(IntPtr src, out int value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ReadU32BE(IntPtr src, out uint value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ReadS32BE(IntPtr src, out int value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ReadU64LE(IntPtr src, out ulong value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ReadS64LE(IntPtr src, out long value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ReadU64BE(IntPtr src, out ulong value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ReadS64BE(IntPtr src, out long value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WriteU8(IntPtr dst, byte value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WriteS8(IntPtr dst, sbyte value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WriteU16LE(IntPtr dst, ushort value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WriteS16LE(IntPtr dst, short value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WriteU16BE(IntPtr dst, ushort value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WriteS16BE(IntPtr dst, short value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WriteU32LE(IntPtr dst, uint value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WriteS32LE(IntPtr dst, int value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WriteU32BE(IntPtr dst, uint value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WriteS32BE(IntPtr dst, int value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WriteU64LE(IntPtr dst, ulong value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WriteS64LE(IntPtr dst, long value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WriteU64BE(IntPtr dst, ulong value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WriteS64BE(IntPtr dst, long value);

    // /usr/local/include/SDL3/SDL_clipboard.h

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetClipboardText(string text);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] public static extern string SDL_GetClipboardText();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasClipboardText();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetPrimarySelectionText(string text);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] public static extern string SDL_GetPrimarySelectionText();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasPrimarySelectionText();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr SDL_ClipboardDataCallback(IntPtr userdata, byte* mime_type, IntPtr size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SDL_ClipboardCleanupCallback(IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetClipboardData(SDL_ClipboardDataCallback callback, SDL_ClipboardCleanupCallback cleanup, IntPtr userdata, IntPtr mime_types, UIntPtr num_mime_types);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ClearClipboardData();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetClipboardData(string mime_type, out UIntPtr size);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasClipboardData(string mime_type);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetClipboardMimeTypes(out UIntPtr num_mime_types);

    // /usr/local/include/SDL3/SDL_cpuinfo.h

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetNumLogicalCPUCores();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetCPUCacheLineSize();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasAltiVec();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasMMX();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasSSE();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasSSE2();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasSSE3();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasSSE41();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasSSE42();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasAVX();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasAVX2();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasAVX512F();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasARMSIMD();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasNEON();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasLSX();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasLASX();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetSystemRAM();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern UIntPtr SDL_GetSIMDAlignment();

    // /usr/local/include/SDL3/SDL_video.h

    public const string SDL_PROP_GLOBAL_VIDEO_WAYLAND_WL_DISPLAY_POINTER = "SDL.video.wayland.wl_display";
    public const string SDL_PROP_DISPLAY_HDR_ENABLED_BOOLEAN = "SDL.display.HDR_enabled";
    public const string SDL_PROP_DISPLAY_KMSDRM_PANEL_ORIENTATION_NUMBER = "SDL.display.KMSDRM.panel_orientation";
    public const string SDL_PROP_WINDOW_CREATE_ALWAYS_ON_TOP_BOOLEAN = "SDL.window.create.always_on_top";
    public const string SDL_PROP_WINDOW_CREATE_BORDERLESS_BOOLEAN = "SDL.window.create.borderless";
    public const string SDL_PROP_WINDOW_CREATE_FOCUSABLE_BOOLEAN = "SDL.window.create.focusable";
    public const string SDL_PROP_WINDOW_CREATE_EXTERNAL_GRAPHICS_CONTEXT_BOOLEAN = "SDL.window.create.external_graphics_context";
    public const string SDL_PROP_WINDOW_CREATE_FLAGS_NUMBER = "SDL.window.create.flags";
    public const string SDL_PROP_WINDOW_CREATE_FULLSCREEN_BOOLEAN = "SDL.window.create.fullscreen";
    public const string SDL_PROP_WINDOW_CREATE_HEIGHT_NUMBER = "SDL.window.create.height";
    public const string SDL_PROP_WINDOW_CREATE_HIDDEN_BOOLEAN = "SDL.window.create.hidden";
    public const string SDL_PROP_WINDOW_CREATE_HIGH_PIXEL_DENSITY_BOOLEAN = "SDL.window.create.high_pixel_density";
    public const string SDL_PROP_WINDOW_CREATE_MAXIMIZED_BOOLEAN = "SDL.window.create.maximized";
    public const string SDL_PROP_WINDOW_CREATE_MENU_BOOLEAN = "SDL.window.create.menu";
    public const string SDL_PROP_WINDOW_CREATE_METAL_BOOLEAN = "SDL.window.create.metal";
    public const string SDL_PROP_WINDOW_CREATE_MINIMIZED_BOOLEAN = "SDL.window.create.minimized";
    public const string SDL_PROP_WINDOW_CREATE_MODAL_BOOLEAN = "SDL.window.create.modal";
    public const string SDL_PROP_WINDOW_CREATE_MOUSE_GRABBED_BOOLEAN = "SDL.window.create.mouse_grabbed";
    public const string SDL_PROP_WINDOW_CREATE_OPENGL_BOOLEAN = "SDL.window.create.opengl";
    public const string SDL_PROP_WINDOW_CREATE_PARENT_POINTER = "SDL.window.create.parent";
    public const string SDL_PROP_WINDOW_CREATE_RESIZABLE_BOOLEAN = "SDL.window.create.resizable";
    public const string SDL_PROP_WINDOW_CREATE_TITLE_STRING = "SDL.window.create.title";
    public const string SDL_PROP_WINDOW_CREATE_TRANSPARENT_BOOLEAN = "SDL.window.create.transparent";
    public const string SDL_PROP_WINDOW_CREATE_TOOLTIP_BOOLEAN = "SDL.window.create.tooltip";
    public const string SDL_PROP_WINDOW_CREATE_UTILITY_BOOLEAN = "SDL.window.create.utility";
    public const string SDL_PROP_WINDOW_CREATE_VULKAN_BOOLEAN = "SDL.window.create.vulkan";
    public const string SDL_PROP_WINDOW_CREATE_WIDTH_NUMBER = "SDL.window.create.width";
    public const string SDL_PROP_WINDOW_CREATE_X_NUMBER = "SDL.window.create.x";
    public const string SDL_PROP_WINDOW_CREATE_Y_NUMBER = "SDL.window.create.y";
    public const string SDL_PROP_WINDOW_CREATE_COCOA_WINDOW_POINTER = "SDL.window.create.cocoa.window";
    public const string SDL_PROP_WINDOW_CREATE_COCOA_VIEW_POINTER = "SDL.window.create.cocoa.view";
    public const string SDL_PROP_WINDOW_CREATE_WAYLAND_SURFACE_ROLE_CUSTOM_BOOLEAN = "SDL.window.create.wayland.surface_role_custom";
    public const string SDL_PROP_WINDOW_CREATE_WAYLAND_CREATE_EGL_WINDOW_BOOLEAN = "SDL.window.create.wayland.create_egl_window";
    public const string SDL_PROP_WINDOW_CREATE_WAYLAND_WL_SURFACE_POINTER = "SDL.window.create.wayland.wl_surface";
    public const string SDL_PROP_WINDOW_CREATE_WIN32_HWND_POINTER = "SDL.window.create.win32.hwnd";
    public const string SDL_PROP_WINDOW_CREATE_WIN32_PIXEL_FORMAT_HWND_POINTER = "SDL.window.create.win32.pixel_format_hwnd";
    public const string SDL_PROP_WINDOW_CREATE_X11_WINDOW_NUMBER = "SDL.window.create.x11.window";
    public const string SDL_PROP_WINDOW_SHAPE_POINTER = "SDL.window.shape";
    public const string SDL_PROP_WINDOW_HDR_ENABLED_BOOLEAN = "SDL.window.HDR_enabled";
    public const string SDL_PROP_WINDOW_SDR_WHITE_LEVEL_FLOAT = "SDL.window.SDR_white_level";
    public const string SDL_PROP_WINDOW_HDR_HEADROOM_FLOAT = "SDL.window.HDR_headroom";
    public const string SDL_PROP_WINDOW_ANDROID_WINDOW_POINTER = "SDL.window.android.window";
    public const string SDL_PROP_WINDOW_ANDROID_SURFACE_POINTER = "SDL.window.android.surface";
    public const string SDL_PROP_WINDOW_UIKIT_WINDOW_POINTER = "SDL.window.uikit.window";
    public const string SDL_PROP_WINDOW_UIKIT_METAL_VIEW_TAG_NUMBER = "SDL.window.uikit.metal_view_tag";
    public const string SDL_PROP_WINDOW_UIKIT_OPENGL_FRAMEBUFFER_NUMBER = "SDL.window.uikit.opengl.framebuffer";
    public const string SDL_PROP_WINDOW_UIKIT_OPENGL_RENDERBUFFER_NUMBER = "SDL.window.uikit.opengl.renderbuffer";
    public const string SDL_PROP_WINDOW_UIKIT_OPENGL_RESOLVE_FRAMEBUFFER_NUMBER = "SDL.window.uikit.opengl.resolve_framebuffer";
    public const string SDL_PROP_WINDOW_KMSDRM_DEVICE_INDEX_NUMBER = "SDL.window.kmsdrm.dev_index";
    public const string SDL_PROP_WINDOW_KMSDRM_DRM_FD_NUMBER = "SDL.window.kmsdrm.drm_fd";
    public const string SDL_PROP_WINDOW_KMSDRM_GBM_DEVICE_POINTER = "SDL.window.kmsdrm.gbm_dev";
    public const string SDL_PROP_WINDOW_COCOA_WINDOW_POINTER = "SDL.window.cocoa.window";
    public const string SDL_PROP_WINDOW_COCOA_METAL_VIEW_TAG_NUMBER = "SDL.window.cocoa.metal_view_tag";
    public const string SDL_PROP_WINDOW_OPENVR_OVERLAY_ID = "SDL.window.openvr.overlay_id";
    public const string SDL_PROP_WINDOW_VIVANTE_DISPLAY_POINTER = "SDL.window.vivante.display";
    public const string SDL_PROP_WINDOW_VIVANTE_WINDOW_POINTER = "SDL.window.vivante.window";
    public const string SDL_PROP_WINDOW_VIVANTE_SURFACE_POINTER = "SDL.window.vivante.surface";
    public const string SDL_PROP_WINDOW_WIN32_HWND_POINTER = "SDL.window.win32.hwnd";
    public const string SDL_PROP_WINDOW_WIN32_HDC_POINTER = "SDL.window.win32.hdc";
    public const string SDL_PROP_WINDOW_WIN32_INSTANCE_POINTER = "SDL.window.win32.instance";
    public const string SDL_PROP_WINDOW_WAYLAND_DISPLAY_POINTER = "SDL.window.wayland.display";
    public const string SDL_PROP_WINDOW_WAYLAND_SURFACE_POINTER = "SDL.window.wayland.surface";
    public const string SDL_PROP_WINDOW_WAYLAND_VIEWPORT_POINTER = "SDL.window.wayland.viewport";
    public const string SDL_PROP_WINDOW_WAYLAND_EGL_WINDOW_POINTER = "SDL.window.wayland.egl_window";
    public const string SDL_PROP_WINDOW_WAYLAND_XDG_SURFACE_POINTER = "SDL.window.wayland.xdg_surface";
    public const string SDL_PROP_WINDOW_WAYLAND_XDG_TOPLEVEL_POINTER = "SDL.window.wayland.xdg_toplevel";
    public const string SDL_PROP_WINDOW_WAYLAND_XDG_TOPLEVEL_EXPORT_HANDLE_STRING = "SDL.window.wayland.xdg_toplevel_export_handle";
    public const string SDL_PROP_WINDOW_WAYLAND_XDG_POPUP_POINTER = "SDL.window.wayland.xdg_popup";
    public const string SDL_PROP_WINDOW_WAYLAND_XDG_POSITIONER_POINTER = "SDL.window.wayland.xdg_positioner";
    public const string SDL_PROP_WINDOW_X11_DISPLAY_POINTER = "SDL.window.x11.display";
    public const string SDL_PROP_WINDOW_X11_SCREEN_NUMBER = "SDL.window.x11.screen";
    public const string SDL_PROP_WINDOW_X11_WINDOW_NUMBER = "SDL.window.x11.window";

    public enum SDL_SystemTheme
    {
        SDL_SYSTEM_THEME_UNKNOWN = 0,
        SDL_SYSTEM_THEME_LIGHT = 1,
        SDL_SYSTEM_THEME_DARK = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_DisplayMode
    {
        public uint displayID;
        public SDL_PixelFormat format;
        public int w;
        public int h;
        public float pixel_density;
        public float refresh_rate;
        public int refresh_rate_numerator;
        public int refresh_rate_denominator;
        public IntPtr @internal;
    }

    public enum SDL_DisplayOrientation
    {
        SDL_ORIENTATION_UNKNOWN = 0,
        SDL_ORIENTATION_LANDSCAPE = 1,
        SDL_ORIENTATION_LANDSCAPE_FLIPPED = 2,
        SDL_ORIENTATION_PORTRAIT = 3,
        SDL_ORIENTATION_PORTRAIT_FLIPPED = 4,
    }

    [Flags]
    public enum SDL_WindowFlags : ulong
    {
        SDL_WINDOW_FULLSCREEN = 0x1,
        SDL_WINDOW_OPENGL = 0x2,
        SDL_WINDOW_OCCLUDED = 0x4,
        SDL_WINDOW_HIDDEN = 0x08,
        SDL_WINDOW_BORDERLESS = 0x10,
        SDL_WINDOW_RESIZABLE = 0x20,
        SDL_WINDOW_MINIMIZED = 0x40,
        SDL_WINDOW_MAXIMIZED = 0x080,
        SDL_WINDOW_MOUSE_GRABBED = 0x100,
        SDL_WINDOW_INPUT_FOCUS = 0x200,
        SDL_WINDOW_MOUSE_FOCUS = 0x400,
        SDL_WINDOW_EXTERNAL = 0x0800,
        SDL_WINDOW_MODAL = 0x1000,
        SDL_WINDOW_HIGH_PIXEL_DENSITY = 0x2000,
        SDL_WINDOW_MOUSE_CAPTURE = 0x4000,
        SDL_WINDOW_MOUSE_RELATIVE_MODE = 0x08000,
        SDL_WINDOW_ALWAYS_ON_TOP = 0x10000,
        SDL_WINDOW_UTILITY = 0x20000,
        SDL_WINDOW_TOOLTIP = 0x40000,
        SDL_WINDOW_POPUP_MENU = 0x080000,
        SDL_WINDOW_KEYBOARD_GRABBED = 0x100000,
        SDL_WINDOW_VULKAN = 0x10000000,
        SDL_WINDOW_METAL = 0x20000000,
        SDL_WINDOW_TRANSPARENT = 0x40000000,
        SDL_WINDOW_NOT_FOCUSABLE = 0x080000000,
    }

    public enum SDL_FlashOperation
    {
        SDL_FLASH_CANCEL = 0,
        SDL_FLASH_BRIEFLY = 1,
        SDL_FLASH_UNTIL_FOCUSED = 2,
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr SDL_EGLAttribArrayCallback();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr SDL_EGLIntArrayCallback();

    public enum SDL_GLAttr
    {
        SDL_GL_RED_SIZE = 0,
        SDL_GL_GREEN_SIZE = 1,
        SDL_GL_BLUE_SIZE = 2,
        SDL_GL_ALPHA_SIZE = 3,
        SDL_GL_BUFFER_SIZE = 4,
        SDL_GL_DOUBLEBUFFER = 5,
        SDL_GL_DEPTH_SIZE = 6,
        SDL_GL_STENCIL_SIZE = 7,
        SDL_GL_ACCUM_RED_SIZE = 8,
        SDL_GL_ACCUM_GREEN_SIZE = 9,
        SDL_GL_ACCUM_BLUE_SIZE = 10,
        SDL_GL_ACCUM_ALPHA_SIZE = 11,
        SDL_GL_STEREO = 12,
        SDL_GL_MULTISAMPLEBUFFERS = 13,
        SDL_GL_MULTISAMPLESAMPLES = 14,
        SDL_GL_ACCELERATED_VISUAL = 15,
        SDL_GL_RETAINED_BACKING = 16,
        SDL_GL_CONTEXT_MAJOR_VERSION = 17,
        SDL_GL_CONTEXT_MINOR_VERSION = 18,
        SDL_GL_CONTEXT_FLAGS = 19,
        SDL_GL_CONTEXT_PROFILE_MASK = 20,
        SDL_GL_SHARE_WITH_CURRENT_CONTEXT = 21,
        SDL_GL_FRAMEBUFFER_SRGB_CAPABLE = 22,
        SDL_GL_CONTEXT_RELEASE_BEHAVIOR = 23,
        SDL_GL_CONTEXT_RESET_NOTIFICATION = 24,
        SDL_GL_CONTEXT_NO_ERROR = 25,
        SDL_GL_FLOATBUFFERS = 26,
        SDL_GL_EGL_PLATFORM = 27,
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetNumVideoDrivers();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] public static extern string SDL_GetVideoDriver(int index);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] public static extern string SDL_GetCurrentVideoDriver();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_SystemTheme SDL_GetSystemTheme();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetDisplays(out int count);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetPrimaryDisplay();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetDisplayProperties(uint displayID);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] public static extern string SDL_GetDisplayName(uint displayID);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetDisplayBounds(uint displayID, out SDL_Rect rect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetDisplayUsableBounds(uint displayID, out SDL_Rect rect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_DisplayOrientation SDL_GetNaturalDisplayOrientation(uint displayID);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_DisplayOrientation SDL_GetCurrentDisplayOrientation(uint displayID);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float SDL_GetDisplayContentScale(uint displayID);

    public static Span<IntPtr> SDL_GetFullscreenDisplayModes(uint displayID)
    {
        var result = SDL_GetFullscreenDisplayModes(displayID, out var count);
        return new Span<IntPtr>((void*)result, count);
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetFullscreenDisplayModes(uint displayID, out int count);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetClosestFullscreenDisplayMode(uint displayID, int w, int h, float refresh_rate, SDLBool include_high_density_modes, out SDL_DisplayMode closest);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_DisplayMode* SDL_GetDesktopDisplayMode(uint displayID);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_DisplayMode* SDL_GetCurrentDisplayMode(uint displayID);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetDisplayForPoint(ref SDL_Point point);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetDisplayForRect(ref SDL_Rect rect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetDisplayForWindow(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float SDL_GetWindowPixelDensity(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float SDL_GetWindowDisplayScale(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowFullscreenMode(IntPtr window, ref SDL_DisplayMode mode);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_DisplayMode* SDL_GetWindowFullscreenMode(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetWindowICCProfile(IntPtr window, out UIntPtr size);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_PixelFormat SDL_GetWindowPixelFormat(IntPtr window);

    public static Span<IntPtr> SDL_GetWindows()
    {
        var result = SDL_GetWindows(out var count);
        return new Span<IntPtr>((void*)result, count);
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetWindows(out int count);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateWindow(string title, int w, int h, SDL_WindowFlags flags);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreatePopupWindow(IntPtr parent, int offset_x, int offset_y, int w, int h, SDL_WindowFlags flags);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateWindowWithProperties(uint props);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetWindowID(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetWindowFromID(uint id);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetWindowParent(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetWindowProperties(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_WindowFlags SDL_GetWindowFlags(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowTitle(IntPtr window, string title);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] public static extern string SDL_GetWindowTitle(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowIcon(IntPtr window, IntPtr icon);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowPosition(IntPtr window, int x, int y);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetWindowPosition(IntPtr window, out int x, out int y);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowSize(IntPtr window, int w, int h);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetWindowSize(IntPtr window, out int w, out int h);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetWindowSafeArea(IntPtr window, out SDL_Rect rect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowAspectRatio(IntPtr window, float min_aspect, float max_aspect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetWindowAspectRatio(IntPtr window, out float min_aspect, out float max_aspect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetWindowBordersSize(IntPtr window, out int top, out int left, out int bottom, out int right);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetWindowSizeInPixels(IntPtr window, out int w, out int h);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowMinimumSize(IntPtr window, int min_w, int min_h);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetWindowMinimumSize(IntPtr window, out int w, out int h);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowMaximumSize(IntPtr window, int max_w, int max_h);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetWindowMaximumSize(IntPtr window, out int w, out int h);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowBordered(IntPtr window, SDLBool bordered);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowResizable(IntPtr window, SDLBool resizable);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowAlwaysOnTop(IntPtr window, SDLBool on_top);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ShowWindow(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HideWindow(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RaiseWindow(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_MaximizeWindow(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_MinimizeWindow(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RestoreWindow(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowFullscreen(IntPtr window, SDLBool fullscreen);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SyncWindow(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WindowHasSurface(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Surface* SDL_GetWindowSurface(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowSurfaceVSync(IntPtr window, int vsync);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetWindowSurfaceVSync(IntPtr window, out int vsync);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_UpdateWindowSurface(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_UpdateWindowSurfaceRects(IntPtr window, Span<SDL_Rect> rects, int numrects);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_DestroyWindowSurface(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowKeyboardGrab(IntPtr window, SDLBool grabbed);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowMouseGrab(IntPtr window, SDLBool grabbed);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetWindowKeyboardGrab(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetWindowMouseGrab(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetGrabbedWindow();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowMouseRect(IntPtr window, ref SDL_Rect rect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Rect* SDL_GetWindowMouseRect(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowOpacity(IntPtr window, float opacity);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float SDL_GetWindowOpacity(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowParent(IntPtr window, IntPtr parent);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowModal(IntPtr window, SDLBool modal);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowFocusable(IntPtr window, SDLBool focusable);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ShowWindowSystemMenu(IntPtr window, int x, int y);

    public enum SDL_HitTestResult
    {
        SDL_HITTEST_NORMAL = 0,
        SDL_HITTEST_DRAGGABLE = 1,
        SDL_HITTEST_RESIZE_TOPLEFT = 2,
        SDL_HITTEST_RESIZE_TOP = 3,
        SDL_HITTEST_RESIZE_TOPRIGHT = 4,
        SDL_HITTEST_RESIZE_RIGHT = 5,
        SDL_HITTEST_RESIZE_BOTTOMRIGHT = 6,
        SDL_HITTEST_RESIZE_BOTTOM = 7,
        SDL_HITTEST_RESIZE_BOTTOMLEFT = 8,
        SDL_HITTEST_RESIZE_LEFT = 9,
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate SDL_HitTestResult SDL_HitTest(IntPtr win, SDL_Point* area, IntPtr data);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowHitTest(IntPtr window, SDL_HitTest callback, IntPtr callback_data);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetWindowShape(IntPtr window, IntPtr shape);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_FlashWindow(IntPtr window, SDL_FlashOperation operation);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DestroyWindow(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ScreenSaverEnabled();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_EnableScreenSaver();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_DisableScreenSaver();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GL_LoadLibrary(string path);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GL_GetProcAddress(string proc);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_EGL_GetProcAddress(string proc);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_GL_UnloadLibrary();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GL_ExtensionSupported(string extension);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_GL_ResetAttributes();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GL_SetAttribute(SDL_GLAttr attr, int value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GL_GetAttribute(SDL_GLAttr attr, out int value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GL_CreateContext(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GL_MakeCurrent(IntPtr window, IntPtr context);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GL_GetCurrentWindow();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GL_GetCurrentContext();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_EGL_GetCurrentDisplay();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_EGL_GetCurrentConfig();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_EGL_GetWindowSurface(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_EGL_SetAttributeCallbacks(SDL_EGLAttribArrayCallback platformAttribCallback, SDL_EGLIntArrayCallback surfaceAttribCallback, SDL_EGLIntArrayCallback contextAttribCallback, IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GL_SetSwapInterval(int interval);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GL_GetSwapInterval(out int interval);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GL_SwapWindow(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GL_DestroyContext(IntPtr context);

    // /usr/local/include/SDL3/SDL_dialog.h

    public const string SDL_PROP_FILE_DIALOG_FILTERS_POINTER = "SDL.filedialog.filters";
    public const string SDL_PROP_FILE_DIALOG_NFILTERS_NUMBER = "SDL.filedialog.nfilters";
    public const string SDL_PROP_FILE_DIALOG_WINDOW_POINTER = "SDL.filedialog.window";
    public const string SDL_PROP_FILE_DIALOG_LOCATION_STRING = "SDL.filedialog.location";
    public const string SDL_PROP_FILE_DIALOG_MANY_BOOLEAN = "SDL.filedialog.many";
    public const string SDL_PROP_FILE_DIALOG_TITLE_STRING = "SDL.filedialog.title";
    public const string SDL_PROP_FILE_DIALOG_ACCEPT_STRING = "SDL.filedialog.accept";
    public const string SDL_PROP_FILE_DIALOG_CANCEL_STRING = "SDL.filedialog.cancel";

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_DialogFileFilter
    {
        public byte* name;
        public byte* pattern;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SDL_DialogFileCallback(IntPtr userdata, IntPtr filelist, int filter);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_ShowOpenFileDialog(SDL_DialogFileCallback callback, IntPtr userdata, IntPtr window, Span<SDL_DialogFileFilter> filters, int nfilters, string default_location, SDLBool allow_many);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_ShowSaveFileDialog(SDL_DialogFileCallback callback, IntPtr userdata, IntPtr window, Span<SDL_DialogFileFilter> filters, int nfilters, string default_location);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_ShowOpenFolderDialog(SDL_DialogFileCallback callback, IntPtr userdata, IntPtr window, string default_location, SDLBool allow_many);

    public enum SDL_FileDialogType
    {
        SDL_FILEDIALOG_OPENFILE = 0,
        SDL_FILEDIALOG_SAVEFILE = 1,
        SDL_FILEDIALOG_OPENFOLDER = 2,
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_ShowFileDialogWithProperties(SDL_FileDialogType type, SDL_DialogFileCallback callback, IntPtr userdata, uint props);

    // /usr/local/include/SDL3/SDL_guid.h

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GUID
    {
        public fixed byte data[16];
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_GUIDToString(SDL_GUID guid, string pszGUID, int cbGUID);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_GUID SDL_StringToGUID(string pchGUID);

    // /usr/local/include/SDL3/SDL_power.h

    public enum SDL_PowerState
    {
        SDL_POWERSTATE_ERROR = -1,
        SDL_POWERSTATE_UNKNOWN = 0,
        SDL_POWERSTATE_ON_BATTERY = 1,
        SDL_POWERSTATE_NO_BATTERY = 2,
        SDL_POWERSTATE_CHARGING = 3,
        SDL_POWERSTATE_CHARGED = 4,
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_PowerState SDL_GetPowerInfo(out int seconds, out int percent);

    // /usr/local/include/SDL3/SDL_filesystem.h

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] public static extern string SDL_GetBasePath();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] public static extern string SDL_GetPrefPath(string org, string app);

    public enum SDL_Folder
    {
        SDL_FOLDER_HOME = 0,
        SDL_FOLDER_DESKTOP = 1,
        SDL_FOLDER_DOCUMENTS = 2,
        SDL_FOLDER_DOWNLOADS = 3,
        SDL_FOLDER_MUSIC = 4,
        SDL_FOLDER_PICTURES = 5,
        SDL_FOLDER_PUBLICSHARE = 6,
        SDL_FOLDER_SAVEDGAMES = 7,
        SDL_FOLDER_SCREENSHOTS = 8,
        SDL_FOLDER_TEMPLATES = 9,
        SDL_FOLDER_VIDEOS = 10,
        SDL_FOLDER_COUNT = 11,
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] public static extern string SDL_GetUserFolder(SDL_Folder folder);

    public enum SDL_PathType
    {
        SDL_PATHTYPE_NONE = 0,
        SDL_PATHTYPE_FILE = 1,
        SDL_PATHTYPE_DIRECTORY = 2,
        SDL_PATHTYPE_OTHER = 3,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_PathInfo
    {
        public SDL_PathType type;
        public ulong size;
        public long create_time;
        public long modify_time;
        public long access_time;
    }

    [Flags]
    public enum SDL_GlobFlags : uint
    {
        SDL_GLOB_CASEINSENSITIVE = 0x1,
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_CreateDirectory(string path);

    public enum SDL_EnumerationResult
    {
        SDL_ENUM_CONTINUE = 0,
        SDL_ENUM_SUCCESS = 1,
        SDL_ENUM_FAILURE = 2,
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate SDL_EnumerationResult SDL_EnumerateDirectoryCallback(IntPtr userdata, byte* dirname, byte* fname);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_EnumerateDirectory(string path, SDL_EnumerateDirectoryCallback callback, IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RemovePath(string path);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenamePath(string oldpath, string newpath);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_CopyFile(string oldpath, string newpath);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetPathInfo(string path, out SDL_PathInfo info);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GlobDirectory(string path, string pattern, SDL_GlobFlags flags, out int count);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] public static extern string SDL_GetCurrentDirectory();

    // /usr/local/include/SDL3/SDL_haptic.h

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_HapticDirection
    {
        public byte type;
        public fixed int dir[3];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_HapticConstant
    {
        public ushort type;
        public SDL_HapticDirection direction;
        public uint length;
        public ushort delay;
        public ushort button;
        public ushort interval;
        public short level;
        public ushort attack_length;
        public ushort attack_level;
        public ushort fade_length;
        public ushort fade_level;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_HapticPeriodic
    {
        public ushort type;
        public SDL_HapticDirection direction;
        public uint length;
        public ushort delay;
        public ushort button;
        public ushort interval;
        public ushort period;
        public short magnitude;
        public short offset;
        public ushort phase;
        public ushort attack_length;
        public ushort attack_level;
        public ushort fade_length;
        public ushort fade_level;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_HapticCondition
    {
        public ushort type;
        public SDL_HapticDirection direction;
        public uint length;
        public ushort delay;
        public ushort button;
        public ushort interval;
        public fixed ushort right_sat[3];
        public fixed ushort left_sat[3];
        public fixed short right_coeff[3];
        public fixed short left_coeff[3];
        public fixed ushort deadband[3];
        public fixed short center[3];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_HapticRamp
    {
        public ushort type;
        public SDL_HapticDirection direction;
        public uint length;
        public ushort delay;
        public ushort button;
        public ushort interval;
        public short start;
        public short end;
        public ushort attack_length;
        public ushort attack_level;
        public ushort fade_length;
        public ushort fade_level;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_HapticLeftRight
    {
        public ushort type;
        public uint length;
        public ushort large_magnitude;
        public ushort small_magnitude;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_HapticCustom
    {
        public ushort type;
        public SDL_HapticDirection direction;
        public uint length;
        public ushort delay;
        public ushort button;
        public ushort interval;
        public byte channels;
        public ushort period;
        public ushort samples;
        public ushort* data;
        public ushort attack_length;
        public ushort attack_level;
        public ushort fade_length;
        public ushort fade_level;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SDL_HapticEffect
    {
        [FieldOffset(0)]
        public ushort type;
        [FieldOffset(0)]
        public SDL_HapticConstant constant;
        [FieldOffset(0)]
        public SDL_HapticPeriodic periodic;
        [FieldOffset(0)]
        public SDL_HapticCondition condition;
        [FieldOffset(0)]
        public SDL_HapticRamp ramp;
        [FieldOffset(0)]
        public SDL_HapticLeftRight leftright;
        [FieldOffset(0)]
        public SDL_HapticCustom custom;
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetHaptics(out int count);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] public static extern string SDL_GetHapticNameForID(uint instance_id);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_OpenHaptic(uint instance_id);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetHapticFromID(uint instance_id);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetHapticID(IntPtr haptic);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] public static extern string SDL_GetHapticName(IntPtr haptic);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_IsMouseHaptic();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_OpenHapticFromMouse();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_IsJoystickHaptic(IntPtr joystick);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_OpenHapticFromJoystick(IntPtr joystick);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_CloseHaptic(IntPtr haptic);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetMaxHapticEffects(IntPtr haptic);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetMaxHapticEffectsPlaying(IntPtr haptic);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetHapticFeatures(IntPtr haptic);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetNumHapticAxes(IntPtr haptic);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HapticEffectSupported(IntPtr haptic, ref SDL_HapticEffect effect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_CreateHapticEffect(IntPtr haptic, ref SDL_HapticEffect effect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_UpdateHapticEffect(IntPtr haptic, int effect, ref SDL_HapticEffect data);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RunHapticEffect(IntPtr haptic, int effect, uint iterations);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_StopHapticEffect(IntPtr haptic, int effect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DestroyHapticEffect(IntPtr haptic, int effect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetHapticEffectStatus(IntPtr haptic, int effect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetHapticGain(IntPtr haptic, int gain);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetHapticAutocenter(IntPtr haptic, int autocenter);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_PauseHaptic(IntPtr haptic);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ResumeHaptic(IntPtr haptic);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_StopHapticEffects(IntPtr haptic);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HapticRumbleSupported(IntPtr haptic);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_InitHapticRumble(IntPtr haptic);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_PlayHapticRumble(IntPtr haptic, float strength, uint length);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_StopHapticRumble(IntPtr haptic);

    // /usr/local/include/SDL3/SDL_hidapi.h

    public enum SDL_hid_bus_type
    {
        SDL_HID_API_BUS_UNKNOWN = 0,
        SDL_HID_API_BUS_USB = 1,
        SDL_HID_API_BUS_BLUETOOTH = 2,
        SDL_HID_API_BUS_I2C = 3,
        SDL_HID_API_BUS_SPI = 4,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_hid_device_info
    {
        public byte* path;
        public ushort vendor_id;
        public ushort product_id;
        public byte* serial_number;
        public ushort release_number;
        public byte* manufacturer_string;
        public byte* product_string;
        public ushort usage_page;
        public ushort usage;
        public int interface_number;
        public int interface_class;
        public int interface_subclass;
        public int interface_protocol;
        public SDL_hid_bus_type bus_type;
        public SDL_hid_device_info* next;
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_hid_init();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_hid_exit();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_hid_device_change_count();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_hid_device_info* SDL_hid_enumerate(ushort vendor_id, ushort product_id);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_hid_free_enumeration(IntPtr devs); // WARN_UNKNOWN_POINTER_PARAMETER

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_hid_open(ushort vendor_id, ushort product_id, string serial_number);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_hid_open_path(string path);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_hid_write(IntPtr dev, IntPtr data, UIntPtr length); // WARN_UNKNOWN_POINTER_PARAMETER

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_hid_read_timeout(IntPtr dev, IntPtr data, UIntPtr length, int milliseconds); // WARN_UNKNOWN_POINTER_PARAMETER

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_hid_read(IntPtr dev, IntPtr data, UIntPtr length); // WARN_UNKNOWN_POINTER_PARAMETER

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_hid_set_nonblocking(IntPtr dev, int nonblock);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_hid_send_feature_report(IntPtr dev, IntPtr data, UIntPtr length); // WARN_UNKNOWN_POINTER_PARAMETER

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_hid_get_feature_report(IntPtr dev, IntPtr data, UIntPtr length); // WARN_UNKNOWN_POINTER_PARAMETER

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_hid_get_input_report(IntPtr dev, IntPtr data, UIntPtr length); // WARN_UNKNOWN_POINTER_PARAMETER

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_hid_close(IntPtr dev);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_hid_get_manufacturer_string(IntPtr dev, string @string, UIntPtr maxlen);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_hid_get_product_string(IntPtr dev, string @string, UIntPtr maxlen);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_hid_get_serial_number_string(IntPtr dev, string @string, UIntPtr maxlen);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_hid_get_indexed_string(IntPtr dev, int string_index, string @string, UIntPtr maxlen);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_hid_device_info* SDL_hid_get_device_info(IntPtr dev);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_hid_get_report_descriptor(IntPtr dev, IntPtr buf, UIntPtr buf_size); // WARN_UNKNOWN_POINTER_PARAMETER

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_hid_ble_scan(SDLBool active);

    // /usr/local/include/SDL3/SDL_hints.h

    public const string SDL_HINT_ALLOW_ALT_TAB_WHILE_GRABBED = "SDL_ALLOW_ALT_TAB_WHILE_GRABBED";
    public const string SDL_HINT_ANDROID_ALLOW_RECREATE_ACTIVITY = "SDL_ANDROID_ALLOW_RECREATE_ACTIVITY";
    public const string SDL_HINT_ANDROID_BLOCK_ON_PAUSE = "SDL_ANDROID_BLOCK_ON_PAUSE";
    public const string SDL_HINT_ANDROID_LOW_LATENCY_AUDIO = "SDL_ANDROID_LOW_LATENCY_AUDIO";
    public const string SDL_HINT_ANDROID_TRAP_BACK_BUTTON = "SDL_ANDROID_TRAP_BACK_BUTTON";
    public const string SDL_HINT_APP_ID = "SDL_APP_ID";
    public const string SDL_HINT_APP_NAME = "SDL_APP_NAME";
    public const string SDL_HINT_APPLE_TV_CONTROLLER_UI_EVENTS = "SDL_APPLE_TV_CONTROLLER_UI_EVENTS";
    public const string SDL_HINT_APPLE_TV_REMOTE_ALLOW_ROTATION = "SDL_APPLE_TV_REMOTE_ALLOW_ROTATION";
    public const string SDL_HINT_AUDIO_ALSA_DEFAULT_DEVICE = "SDL_AUDIO_ALSA_DEFAULT_DEVICE";
    public const string SDL_HINT_AUDIO_ALSA_DEFAULT_PLAYBACK_DEVICE = "SDL_AUDIO_ALSA_DEFAULT_PLAYBACK_DEVICE";
    public const string SDL_HINT_AUDIO_ALSA_DEFAULT_RECORDING_DEVICE = "SDL_AUDIO_ALSA_DEFAULT_RECORDING_DEVICE";
    public const string SDL_HINT_AUDIO_CATEGORY = "SDL_AUDIO_CATEGORY";
    public const string SDL_HINT_AUDIO_CHANNELS = "SDL_AUDIO_CHANNELS";
    public const string SDL_HINT_AUDIO_DEVICE_APP_ICON_NAME = "SDL_AUDIO_DEVICE_APP_ICON_NAME";
    public const string SDL_HINT_AUDIO_DEVICE_SAMPLE_FRAMES = "SDL_AUDIO_DEVICE_SAMPLE_FRAMES";
    public const string SDL_HINT_AUDIO_DEVICE_STREAM_NAME = "SDL_AUDIO_DEVICE_STREAM_NAME";
    public const string SDL_HINT_AUDIO_DEVICE_STREAM_ROLE = "SDL_AUDIO_DEVICE_STREAM_ROLE";
    public const string SDL_HINT_AUDIO_DISK_INPUT_FILE = "SDL_AUDIO_DISK_INPUT_FILE";
    public const string SDL_HINT_AUDIO_DISK_OUTPUT_FILE = "SDL_AUDIO_DISK_OUTPUT_FILE";
    public const string SDL_HINT_AUDIO_DISK_TIMESCALE = "SDL_AUDIO_DISK_TIMESCALE";
    public const string SDL_HINT_AUDIO_DRIVER = "SDL_AUDIO_DRIVER";
    public const string SDL_HINT_AUDIO_DUMMY_TIMESCALE = "SDL_AUDIO_DUMMY_TIMESCALE";
    public const string SDL_HINT_AUDIO_FORMAT = "SDL_AUDIO_FORMAT";
    public const string SDL_HINT_AUDIO_FREQUENCY = "SDL_AUDIO_FREQUENCY";
    public const string SDL_HINT_AUDIO_INCLUDE_MONITORS = "SDL_AUDIO_INCLUDE_MONITORS";
    public const string SDL_HINT_AUTO_UPDATE_JOYSTICKS = "SDL_AUTO_UPDATE_JOYSTICKS";
    public const string SDL_HINT_AUTO_UPDATE_SENSORS = "SDL_AUTO_UPDATE_SENSORS";
    public const string SDL_HINT_BMP_SAVE_LEGACY_FORMAT = "SDL_BMP_SAVE_LEGACY_FORMAT";
    public const string SDL_HINT_CAMERA_DRIVER = "SDL_CAMERA_DRIVER";
    public const string SDL_HINT_CPU_FEATURE_MASK = "SDL_CPU_FEATURE_MASK";
    public const string SDL_HINT_JOYSTICK_DIRECTINPUT = "SDL_JOYSTICK_DIRECTINPUT";
    public const string SDL_HINT_FILE_DIALOG_DRIVER = "SDL_FILE_DIALOG_DRIVER";
    public const string SDL_HINT_DISPLAY_USABLE_BOUNDS = "SDL_DISPLAY_USABLE_BOUNDS";
    public const string SDL_HINT_EMSCRIPTEN_ASYNCIFY = "SDL_EMSCRIPTEN_ASYNCIFY";
    public const string SDL_HINT_EMSCRIPTEN_CANVAS_SELECTOR = "SDL_EMSCRIPTEN_CANVAS_SELECTOR";
    public const string SDL_HINT_EMSCRIPTEN_KEYBOARD_ELEMENT = "SDL_EMSCRIPTEN_KEYBOARD_ELEMENT";
    public const string SDL_HINT_ENABLE_SCREEN_KEYBOARD = "SDL_ENABLE_SCREEN_KEYBOARD";
    public const string SDL_HINT_EVDEV_DEVICES = "SDL_EVDEV_DEVICES";
    public const string SDL_HINT_EVENT_LOGGING = "SDL_EVENT_LOGGING";
    public const string SDL_HINT_FORCE_RAISEWINDOW = "SDL_FORCE_RAISEWINDOW";
    public const string SDL_HINT_FRAMEBUFFER_ACCELERATION = "SDL_FRAMEBUFFER_ACCELERATION";
    public const string SDL_HINT_GAMECONTROLLERCONFIG = "SDL_GAMECONTROLLERCONFIG";
    public const string SDL_HINT_GAMECONTROLLERCONFIG_FILE = "SDL_GAMECONTROLLERCONFIG_FILE";
    public const string SDL_HINT_GAMECONTROLLERTYPE = "SDL_GAMECONTROLLERTYPE";
    public const string SDL_HINT_GAMECONTROLLER_IGNORE_DEVICES = "SDL_GAMECONTROLLER_IGNORE_DEVICES";
    public const string SDL_HINT_GAMECONTROLLER_IGNORE_DEVICES_EXCEPT = "SDL_GAMECONTROLLER_IGNORE_DEVICES_EXCEPT";
    public const string SDL_HINT_GAMECONTROLLER_SENSOR_FUSION = "SDL_GAMECONTROLLER_SENSOR_FUSION";
    public const string SDL_HINT_GDK_TEXTINPUT_DEFAULT_TEXT = "SDL_GDK_TEXTINPUT_DEFAULT_TEXT";
    public const string SDL_HINT_GDK_TEXTINPUT_DESCRIPTION = "SDL_GDK_TEXTINPUT_DESCRIPTION";
    public const string SDL_HINT_GDK_TEXTINPUT_MAX_LENGTH = "SDL_GDK_TEXTINPUT_MAX_LENGTH";
    public const string SDL_HINT_GDK_TEXTINPUT_SCOPE = "SDL_GDK_TEXTINPUT_SCOPE";
    public const string SDL_HINT_GDK_TEXTINPUT_TITLE = "SDL_GDK_TEXTINPUT_TITLE";
    public const string SDL_HINT_HIDAPI_LIBUSB = "SDL_HIDAPI_LIBUSB";
    public const string SDL_HINT_HIDAPI_LIBUSB_WHITELIST = "SDL_HIDAPI_LIBUSB_WHITELIST";
    public const string SDL_HINT_HIDAPI_UDEV = "SDL_HIDAPI_UDEV";
    public const string SDL_HINT_GPU_DRIVER = "SDL_GPU_DRIVER";
    public const string SDL_HINT_HIDAPI_ENUMERATE_ONLY_CONTROLLERS = "SDL_HIDAPI_ENUMERATE_ONLY_CONTROLLERS";
    public const string SDL_HINT_HIDAPI_IGNORE_DEVICES = "SDL_HIDAPI_IGNORE_DEVICES";
    public const string SDL_HINT_IME_IMPLEMENTED_UI = "SDL_IME_IMPLEMENTED_UI";
    public const string SDL_HINT_IOS_HIDE_HOME_INDICATOR = "SDL_IOS_HIDE_HOME_INDICATOR";
    public const string SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS = "SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS";
    public const string SDL_HINT_JOYSTICK_ARCADESTICK_DEVICES = "SDL_JOYSTICK_ARCADESTICK_DEVICES";
    public const string SDL_HINT_JOYSTICK_ARCADESTICK_DEVICES_EXCLUDED = "SDL_JOYSTICK_ARCADESTICK_DEVICES_EXCLUDED";
    public const string SDL_HINT_JOYSTICK_BLACKLIST_DEVICES = "SDL_JOYSTICK_BLACKLIST_DEVICES";
    public const string SDL_HINT_JOYSTICK_BLACKLIST_DEVICES_EXCLUDED = "SDL_JOYSTICK_BLACKLIST_DEVICES_EXCLUDED";
    public const string SDL_HINT_JOYSTICK_DEVICE = "SDL_JOYSTICK_DEVICE";
    public const string SDL_HINT_JOYSTICK_ENHANCED_REPORTS = "SDL_JOYSTICK_ENHANCED_REPORTS";
    public const string SDL_HINT_JOYSTICK_FLIGHTSTICK_DEVICES = "SDL_JOYSTICK_FLIGHTSTICK_DEVICES";
    public const string SDL_HINT_JOYSTICK_FLIGHTSTICK_DEVICES_EXCLUDED = "SDL_JOYSTICK_FLIGHTSTICK_DEVICES_EXCLUDED";
    public const string SDL_HINT_JOYSTICK_GAMEINPUT = "SDL_JOYSTICK_GAMEINPUT";
    public const string SDL_HINT_JOYSTICK_GAMECUBE_DEVICES = "SDL_JOYSTICK_GAMECUBE_DEVICES";
    public const string SDL_HINT_JOYSTICK_GAMECUBE_DEVICES_EXCLUDED = "SDL_JOYSTICK_GAMECUBE_DEVICES_EXCLUDED";
    public const string SDL_HINT_JOYSTICK_HIDAPI = "SDL_JOYSTICK_HIDAPI";
    public const string SDL_HINT_JOYSTICK_HIDAPI_COMBINE_JOY_CONS = "SDL_JOYSTICK_HIDAPI_COMBINE_JOY_CONS";
    public const string SDL_HINT_JOYSTICK_HIDAPI_GAMECUBE = "SDL_JOYSTICK_HIDAPI_GAMECUBE";
    public const string SDL_HINT_JOYSTICK_HIDAPI_GAMECUBE_RUMBLE_BRAKE = "SDL_JOYSTICK_HIDAPI_GAMECUBE_RUMBLE_BRAKE";
    public const string SDL_HINT_JOYSTICK_HIDAPI_JOY_CONS = "SDL_JOYSTICK_HIDAPI_JOY_CONS";
    public const string SDL_HINT_JOYSTICK_HIDAPI_JOYCON_HOME_LED = "SDL_JOYSTICK_HIDAPI_JOYCON_HOME_LED";
    public const string SDL_HINT_JOYSTICK_HIDAPI_LUNA = "SDL_JOYSTICK_HIDAPI_LUNA";
    public const string SDL_HINT_JOYSTICK_HIDAPI_NINTENDO_CLASSIC = "SDL_JOYSTICK_HIDAPI_NINTENDO_CLASSIC";
    public const string SDL_HINT_JOYSTICK_HIDAPI_PS3 = "SDL_JOYSTICK_HIDAPI_PS3";
    public const string SDL_HINT_JOYSTICK_HIDAPI_PS3_SIXAXIS_DRIVER = "SDL_JOYSTICK_HIDAPI_PS3_SIXAXIS_DRIVER";
    public const string SDL_HINT_JOYSTICK_HIDAPI_PS4 = "SDL_JOYSTICK_HIDAPI_PS4";
    public const string SDL_HINT_JOYSTICK_HIDAPI_PS4_REPORT_INTERVAL = "SDL_JOYSTICK_HIDAPI_PS4_REPORT_INTERVAL";
    public const string SDL_HINT_JOYSTICK_HIDAPI_PS5 = "SDL_JOYSTICK_HIDAPI_PS5";
    public const string SDL_HINT_JOYSTICK_HIDAPI_PS5_PLAYER_LED = "SDL_JOYSTICK_HIDAPI_PS5_PLAYER_LED";
    public const string SDL_HINT_JOYSTICK_HIDAPI_SHIELD = "SDL_JOYSTICK_HIDAPI_SHIELD";
    public const string SDL_HINT_JOYSTICK_HIDAPI_STADIA = "SDL_JOYSTICK_HIDAPI_STADIA";
    public const string SDL_HINT_JOYSTICK_HIDAPI_STEAM = "SDL_JOYSTICK_HIDAPI_STEAM";
    public const string SDL_HINT_JOYSTICK_HIDAPI_STEAM_HOME_LED = "SDL_JOYSTICK_HIDAPI_STEAM_HOME_LED";
    public const string SDL_HINT_JOYSTICK_HIDAPI_STEAMDECK = "SDL_JOYSTICK_HIDAPI_STEAMDECK";
    public const string SDL_HINT_JOYSTICK_HIDAPI_STEAM_HORI = "SDL_JOYSTICK_HIDAPI_STEAM_HORI";
    public const string SDL_HINT_JOYSTICK_HIDAPI_SWITCH = "SDL_JOYSTICK_HIDAPI_SWITCH";
    public const string SDL_HINT_JOYSTICK_HIDAPI_SWITCH_HOME_LED = "SDL_JOYSTICK_HIDAPI_SWITCH_HOME_LED";
    public const string SDL_HINT_JOYSTICK_HIDAPI_SWITCH_PLAYER_LED = "SDL_JOYSTICK_HIDAPI_SWITCH_PLAYER_LED";
    public const string SDL_HINT_JOYSTICK_HIDAPI_VERTICAL_JOY_CONS = "SDL_JOYSTICK_HIDAPI_VERTICAL_JOY_CONS";
    public const string SDL_HINT_JOYSTICK_HIDAPI_WII = "SDL_JOYSTICK_HIDAPI_WII";
    public const string SDL_HINT_JOYSTICK_HIDAPI_WII_PLAYER_LED = "SDL_JOYSTICK_HIDAPI_WII_PLAYER_LED";
    public const string SDL_HINT_JOYSTICK_HIDAPI_XBOX = "SDL_JOYSTICK_HIDAPI_XBOX";
    public const string SDL_HINT_JOYSTICK_HIDAPI_XBOX_360 = "SDL_JOYSTICK_HIDAPI_XBOX_360";
    public const string SDL_HINT_JOYSTICK_HIDAPI_XBOX_360_PLAYER_LED = "SDL_JOYSTICK_HIDAPI_XBOX_360_PLAYER_LED";
    public const string SDL_HINT_JOYSTICK_HIDAPI_XBOX_360_WIRELESS = "SDL_JOYSTICK_HIDAPI_XBOX_360_WIRELESS";
    public const string SDL_HINT_JOYSTICK_HIDAPI_XBOX_ONE = "SDL_JOYSTICK_HIDAPI_XBOX_ONE";
    public const string SDL_HINT_JOYSTICK_HIDAPI_XBOX_ONE_HOME_LED = "SDL_JOYSTICK_HIDAPI_XBOX_ONE_HOME_LED";
    public const string SDL_HINT_JOYSTICK_IOKIT = "SDL_JOYSTICK_IOKIT";
    public const string SDL_HINT_JOYSTICK_LINUX_CLASSIC = "SDL_JOYSTICK_LINUX_CLASSIC";
    public const string SDL_HINT_JOYSTICK_LINUX_DEADZONES = "SDL_JOYSTICK_LINUX_DEADZONES";
    public const string SDL_HINT_JOYSTICK_LINUX_DIGITAL_HATS = "SDL_JOYSTICK_LINUX_DIGITAL_HATS";
    public const string SDL_HINT_JOYSTICK_LINUX_HAT_DEADZONES = "SDL_JOYSTICK_LINUX_HAT_DEADZONES";
    public const string SDL_HINT_JOYSTICK_MFI = "SDL_JOYSTICK_MFI";
    public const string SDL_HINT_JOYSTICK_RAWINPUT = "SDL_JOYSTICK_RAWINPUT";
    public const string SDL_HINT_JOYSTICK_RAWINPUT_CORRELATE_XINPUT = "SDL_JOYSTICK_RAWINPUT_CORRELATE_XINPUT";
    public const string SDL_HINT_JOYSTICK_ROG_CHAKRAM = "SDL_JOYSTICK_ROG_CHAKRAM";
    public const string SDL_HINT_JOYSTICK_THREAD = "SDL_JOYSTICK_THREAD";
    public const string SDL_HINT_JOYSTICK_THROTTLE_DEVICES = "SDL_JOYSTICK_THROTTLE_DEVICES";
    public const string SDL_HINT_JOYSTICK_THROTTLE_DEVICES_EXCLUDED = "SDL_JOYSTICK_THROTTLE_DEVICES_EXCLUDED";
    public const string SDL_HINT_JOYSTICK_WGI = "SDL_JOYSTICK_WGI";
    public const string SDL_HINT_JOYSTICK_WHEEL_DEVICES = "SDL_JOYSTICK_WHEEL_DEVICES";
    public const string SDL_HINT_JOYSTICK_WHEEL_DEVICES_EXCLUDED = "SDL_JOYSTICK_WHEEL_DEVICES_EXCLUDED";
    public const string SDL_HINT_JOYSTICK_ZERO_CENTERED_DEVICES = "SDL_JOYSTICK_ZERO_CENTERED_DEVICES";
    public const string SDL_HINT_JOYSTICK_HAPTIC_AXES = "SDL_JOYSTICK_HAPTIC_AXES";
    public const string SDL_HINT_KEYCODE_OPTIONS = "SDL_KEYCODE_OPTIONS";
    public const string SDL_HINT_KMSDRM_DEVICE_INDEX = "SDL_KMSDRM_DEVICE_INDEX";
    public const string SDL_HINT_KMSDRM_REQUIRE_DRM_MASTER = "SDL_KMSDRM_REQUIRE_DRM_MASTER";
    public const string SDL_HINT_LOGGING = "SDL_LOGGING";
    public const string SDL_HINT_MAC_BACKGROUND_APP = "SDL_MAC_BACKGROUND_APP";
    public const string SDL_HINT_MAC_CTRL_CLICK_EMULATE_RIGHT_CLICK = "SDL_MAC_CTRL_CLICK_EMULATE_RIGHT_CLICK";
    public const string SDL_HINT_MAC_OPENGL_ASYNC_DISPATCH = "SDL_MAC_OPENGL_ASYNC_DISPATCH";
    public const string SDL_HINT_MAC_OPTION_AS_ALT = "SDL_MAC_OPTION_AS_ALT";
    public const string SDL_HINT_MAC_SCROLL_MOMENTUM = "SDL_MAC_SCROLL_MOMENTUM";
    public const string SDL_HINT_MAIN_CALLBACK_RATE = "SDL_MAIN_CALLBACK_RATE";
    public const string SDL_HINT_MOUSE_AUTO_CAPTURE = "SDL_MOUSE_AUTO_CAPTURE";
    public const string SDL_HINT_MOUSE_DOUBLE_CLICK_RADIUS = "SDL_MOUSE_DOUBLE_CLICK_RADIUS";
    public const string SDL_HINT_MOUSE_DOUBLE_CLICK_TIME = "SDL_MOUSE_DOUBLE_CLICK_TIME";
    public const string SDL_HINT_MOUSE_DEFAULT_SYSTEM_CURSOR = "SDL_MOUSE_DEFAULT_SYSTEM_CURSOR";
    public const string SDL_HINT_MOUSE_EMULATE_WARP_WITH_RELATIVE = "SDL_MOUSE_EMULATE_WARP_WITH_RELATIVE";
    public const string SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH = "SDL_MOUSE_FOCUS_CLICKTHROUGH";
    public const string SDL_HINT_MOUSE_NORMAL_SPEED_SCALE = "SDL_MOUSE_NORMAL_SPEED_SCALE";
    public const string SDL_HINT_MOUSE_RELATIVE_MODE_CENTER = "SDL_MOUSE_RELATIVE_MODE_CENTER";
    public const string SDL_HINT_MOUSE_RELATIVE_SPEED_SCALE = "SDL_MOUSE_RELATIVE_SPEED_SCALE";
    public const string SDL_HINT_MOUSE_RELATIVE_SYSTEM_SCALE = "SDL_MOUSE_RELATIVE_SYSTEM_SCALE";
    public const string SDL_HINT_MOUSE_RELATIVE_WARP_MOTION = "SDL_MOUSE_RELATIVE_WARP_MOTION";
    public const string SDL_HINT_MOUSE_RELATIVE_CURSOR_VISIBLE = "SDL_MOUSE_RELATIVE_CURSOR_VISIBLE";
    public const string SDL_HINT_MOUSE_TOUCH_EVENTS = "SDL_MOUSE_TOUCH_EVENTS";
    public const string SDL_HINT_MUTE_CONSOLE_KEYBOARD = "SDL_MUTE_CONSOLE_KEYBOARD";
    public const string SDL_HINT_NO_SIGNAL_HANDLERS = "SDL_NO_SIGNAL_HANDLERS";
    public const string SDL_HINT_OPENGL_LIBRARY = "SDL_OPENGL_LIBRARY";
    public const string SDL_HINT_EGL_LIBRARY = "SDL_EGL_LIBRARY";
    public const string SDL_HINT_OPENGL_ES_DRIVER = "SDL_OPENGL_ES_DRIVER";
    public const string SDL_HINT_OPENVR_LIBRARY = "SDL_OPENVR_LIBRARY";
    public const string SDL_HINT_ORIENTATIONS = "SDL_ORIENTATIONS";
    public const string SDL_HINT_POLL_SENTINEL = "SDL_POLL_SENTINEL";
    public const string SDL_HINT_PREFERRED_LOCALES = "SDL_PREFERRED_LOCALES";
    public const string SDL_HINT_QUIT_ON_LAST_WINDOW_CLOSE = "SDL_QUIT_ON_LAST_WINDOW_CLOSE";
    public const string SDL_HINT_RENDER_DIRECT3D_THREADSAFE = "SDL_RENDER_DIRECT3D_THREADSAFE";
    public const string SDL_HINT_RENDER_DIRECT3D11_DEBUG = "SDL_RENDER_DIRECT3D11_DEBUG";
    public const string SDL_HINT_RENDER_VULKAN_DEBUG = "SDL_RENDER_VULKAN_DEBUG";
    public const string SDL_HINT_RENDER_GPU_DEBUG = "SDL_RENDER_GPU_DEBUG";
    public const string SDL_HINT_RENDER_GPU_LOW_POWER = "SDL_RENDER_GPU_LOW_POWER";
    public const string SDL_HINT_RENDER_DRIVER = "SDL_RENDER_DRIVER";
    public const string SDL_HINT_RENDER_LINE_METHOD = "SDL_RENDER_LINE_METHOD";
    public const string SDL_HINT_RENDER_METAL_PREFER_LOW_POWER_DEVICE = "SDL_RENDER_METAL_PREFER_LOW_POWER_DEVICE";
    public const string SDL_HINT_RENDER_VSYNC = "SDL_RENDER_VSYNC";
    public const string SDL_HINT_RETURN_KEY_HIDES_IME = "SDL_RETURN_KEY_HIDES_IME";
    public const string SDL_HINT_ROG_GAMEPAD_MICE = "SDL_ROG_GAMEPAD_MICE";
    public const string SDL_HINT_ROG_GAMEPAD_MICE_EXCLUDED = "SDL_ROG_GAMEPAD_MICE_EXCLUDED";
    public const string SDL_HINT_RPI_VIDEO_LAYER = "SDL_RPI_VIDEO_LAYER";
    public const string SDL_HINT_SCREENSAVER_INHIBIT_ACTIVITY_NAME = "SDL_SCREENSAVER_INHIBIT_ACTIVITY_NAME";
    public const string SDL_HINT_SHUTDOWN_DBUS_ON_QUIT = "SDL_SHUTDOWN_DBUS_ON_QUIT";
    public const string SDL_HINT_STORAGE_TITLE_DRIVER = "SDL_STORAGE_TITLE_DRIVER";
    public const string SDL_HINT_STORAGE_USER_DRIVER = "SDL_STORAGE_USER_DRIVER";
    public const string SDL_HINT_THREAD_FORCE_REALTIME_TIME_CRITICAL = "SDL_THREAD_FORCE_REALTIME_TIME_CRITICAL";
    public const string SDL_HINT_THREAD_PRIORITY_POLICY = "SDL_THREAD_PRIORITY_POLICY";
    public const string SDL_HINT_TIMER_RESOLUTION = "SDL_TIMER_RESOLUTION";
    public const string SDL_HINT_TOUCH_MOUSE_EVENTS = "SDL_TOUCH_MOUSE_EVENTS";
    public const string SDL_HINT_TRACKPAD_IS_TOUCH_ONLY = "SDL_TRACKPAD_IS_TOUCH_ONLY";
    public const string SDL_HINT_TV_REMOTE_AS_JOYSTICK = "SDL_TV_REMOTE_AS_JOYSTICK";
    public const string SDL_HINT_VIDEO_ALLOW_SCREENSAVER = "SDL_VIDEO_ALLOW_SCREENSAVER";
    public const string SDL_HINT_VIDEO_DISPLAY_PRIORITY = "SDL_VIDEO_DISPLAY_PRIORITY";
    public const string SDL_HINT_VIDEO_DOUBLE_BUFFER = "SDL_VIDEO_DOUBLE_BUFFER";
    public const string SDL_HINT_VIDEO_DRIVER = "SDL_VIDEO_DRIVER";
    public const string SDL_HINT_VIDEO_DUMMY_SAVE_FRAMES = "SDL_VIDEO_DUMMY_SAVE_FRAMES";
    public const string SDL_HINT_VIDEO_EGL_ALLOW_GETDISPLAY_FALLBACK = "SDL_VIDEO_EGL_ALLOW_GETDISPLAY_FALLBACK";
    public const string SDL_HINT_VIDEO_FORCE_EGL = "SDL_VIDEO_FORCE_EGL";
    public const string SDL_HINT_VIDEO_MAC_FULLSCREEN_SPACES = "SDL_VIDEO_MAC_FULLSCREEN_SPACES";
    public const string SDL_HINT_VIDEO_MAC_FULLSCREEN_MENU_VISIBILITY = "SDL_VIDEO_MAC_FULLSCREEN_MENU_VISIBILITY";
    public const string SDL_HINT_VIDEO_MINIMIZE_ON_FOCUS_LOSS = "SDL_VIDEO_MINIMIZE_ON_FOCUS_LOSS";
    public const string SDL_HINT_VIDEO_OFFSCREEN_SAVE_FRAMES = "SDL_VIDEO_OFFSCREEN_SAVE_FRAMES";
    public const string SDL_HINT_VIDEO_SYNC_WINDOW_OPERATIONS = "SDL_VIDEO_SYNC_WINDOW_OPERATIONS";
    public const string SDL_HINT_VIDEO_WAYLAND_ALLOW_LIBDECOR = "SDL_VIDEO_WAYLAND_ALLOW_LIBDECOR";
    public const string SDL_HINT_VIDEO_WAYLAND_MODE_EMULATION = "SDL_VIDEO_WAYLAND_MODE_EMULATION";
    public const string SDL_HINT_VIDEO_WAYLAND_MODE_SCALING = "SDL_VIDEO_WAYLAND_MODE_SCALING";
    public const string SDL_HINT_VIDEO_WAYLAND_PREFER_LIBDECOR = "SDL_VIDEO_WAYLAND_PREFER_LIBDECOR";
    public const string SDL_HINT_VIDEO_WAYLAND_SCALE_TO_DISPLAY = "SDL_VIDEO_WAYLAND_SCALE_TO_DISPLAY";
    public const string SDL_HINT_VIDEO_WIN_D3DCOMPILER = "SDL_VIDEO_WIN_D3DCOMPILER";
    public const string SDL_HINT_VIDEO_X11_NET_WM_BYPASS_COMPOSITOR = "SDL_VIDEO_X11_NET_WM_BYPASS_COMPOSITOR";
    public const string SDL_HINT_VIDEO_X11_NET_WM_PING = "SDL_VIDEO_X11_NET_WM_PING";
    public const string SDL_HINT_VIDEO_X11_NODIRECTCOLOR = "SDL_VIDEO_X11_NODIRECTCOLOR";
    public const string SDL_HINT_VIDEO_X11_SCALING_FACTOR = "SDL_VIDEO_X11_SCALING_FACTOR";
    public const string SDL_HINT_VIDEO_X11_VISUALID = "SDL_VIDEO_X11_VISUALID";
    public const string SDL_HINT_VIDEO_X11_WINDOW_VISUALID = "SDL_VIDEO_X11_WINDOW_VISUALID";
    public const string SDL_HINT_VIDEO_X11_XRANDR = "SDL_VIDEO_X11_XRANDR";
    public const string SDL_HINT_VITA_ENABLE_BACK_TOUCH = "SDL_VITA_ENABLE_BACK_TOUCH";
    public const string SDL_HINT_VITA_ENABLE_FRONT_TOUCH = "SDL_VITA_ENABLE_FRONT_TOUCH";
    public const string SDL_HINT_VITA_MODULE_PATH = "SDL_VITA_MODULE_PATH";
    public const string SDL_HINT_VITA_PVR_INIT = "SDL_VITA_PVR_INIT";
    public const string SDL_HINT_VITA_RESOLUTION = "SDL_VITA_RESOLUTION";
    public const string SDL_HINT_VITA_PVR_OPENGL = "SDL_VITA_PVR_OPENGL";
    public const string SDL_HINT_VITA_TOUCH_MOUSE_DEVICE = "SDL_VITA_TOUCH_MOUSE_DEVICE";
    public const string SDL_HINT_VULKAN_DISPLAY = "SDL_VULKAN_DISPLAY";
    public const string SDL_HINT_VULKAN_LIBRARY = "SDL_VULKAN_LIBRARY";
    public const string SDL_HINT_WAVE_FACT_CHUNK = "SDL_WAVE_FACT_CHUNK";
    public const string SDL_HINT_WAVE_CHUNK_LIMIT = "SDL_WAVE_CHUNK_LIMIT";
    public const string SDL_HINT_WAVE_RIFF_CHUNK_SIZE = "SDL_WAVE_RIFF_CHUNK_SIZE";
    public const string SDL_HINT_WAVE_TRUNCATION = "SDL_WAVE_TRUNCATION";
    public const string SDL_HINT_WINDOW_ACTIVATE_WHEN_RAISED = "SDL_WINDOW_ACTIVATE_WHEN_RAISED";
    public const string SDL_HINT_WINDOW_ACTIVATE_WHEN_SHOWN = "SDL_WINDOW_ACTIVATE_WHEN_SHOWN";
    public const string SDL_HINT_WINDOW_ALLOW_TOPMOST = "SDL_WINDOW_ALLOW_TOPMOST";
    public const string SDL_HINT_WINDOW_FRAME_USABLE_WHILE_CURSOR_HIDDEN = "SDL_WINDOW_FRAME_USABLE_WHILE_CURSOR_HIDDEN";
    public const string SDL_HINT_WINDOWS_CLOSE_ON_ALT_F4 = "SDL_WINDOWS_CLOSE_ON_ALT_F4";
    public const string SDL_HINT_WINDOWS_ENABLE_MENU_MNEMONICS = "SDL_WINDOWS_ENABLE_MENU_MNEMONICS";
    public const string SDL_HINT_WINDOWS_ENABLE_MESSAGELOOP = "SDL_WINDOWS_ENABLE_MESSAGELOOP";
    public const string SDL_HINT_WINDOWS_GAMEINPUT = "SDL_WINDOWS_GAMEINPUT";
    public const string SDL_HINT_WINDOWS_RAW_KEYBOARD = "SDL_WINDOWS_RAW_KEYBOARD";
    public const string SDL_HINT_WINDOWS_FORCE_SEMAPHORE_KERNEL = "SDL_WINDOWS_FORCE_SEMAPHORE_KERNEL";
    public const string SDL_HINT_WINDOWS_INTRESOURCE_ICON = "SDL_WINDOWS_INTRESOURCE_ICON";
    public const string SDL_HINT_WINDOWS_INTRESOURCE_ICON_SMALL = "SDL_WINDOWS_INTRESOURCE_ICON_SMALL";
    public const string SDL_HINT_WINDOWS_USE_D3D9EX = "SDL_WINDOWS_USE_D3D9EX";
    public const string SDL_HINT_WINDOWS_ERASE_BACKGROUND_MODE = "SDL_WINDOWS_ERASE_BACKGROUND_MODE";
    public const string SDL_HINT_X11_FORCE_OVERRIDE_REDIRECT = "SDL_X11_FORCE_OVERRIDE_REDIRECT";
    public const string SDL_HINT_X11_WINDOW_TYPE = "SDL_X11_WINDOW_TYPE";
    public const string SDL_HINT_X11_XCB_LIBRARY = "SDL_X11_XCB_LIBRARY";
    public const string SDL_HINT_XINPUT_ENABLED = "SDL_XINPUT_ENABLED";
    public const string SDL_HINT_ASSERT = "SDL_ASSERT";
    public const string SDL_HINT_PEN_MOUSE_EVENTS = "SDL_PEN_MOUSE_EVENTS";
    public const string SDL_HINT_PEN_TOUCH_EVENTS = "SDL_PEN_TOUCH_EVENTS";

    public enum SDL_HintPriority
    {
        SDL_HINT_DEFAULT = 0,
        SDL_HINT_NORMAL = 1,
        SDL_HINT_OVERRIDE = 2,
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetHintWithPriority(string name, string value, SDL_HintPriority priority);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetHint(string name, string value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ResetHint(string name);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_ResetHints();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] public static extern string SDL_GetHint(string name);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetHintBoolean(string name, SDLBool default_value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SDL_HintCallback(IntPtr userdata, byte* name, byte* oldValue, byte* newValue);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_AddHintCallback(string name, SDL_HintCallback callback, IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_RemoveHintCallback(string name, SDL_HintCallback callback, IntPtr userdata);

    // /usr/local/include/SDL3/SDL_init.h

    public const string SDL_PROP_APP_METADATA_NAME_STRING = "SDL.app.metadata.name";
    public const string SDL_PROP_APP_METADATA_VERSION_STRING = "SDL.app.metadata.version";
    public const string SDL_PROP_APP_METADATA_IDENTIFIER_STRING = "SDL.app.metadata.identifier";
    public const string SDL_PROP_APP_METADATA_CREATOR_STRING = "SDL.app.metadata.creator";
    public const string SDL_PROP_APP_METADATA_COPYRIGHT_STRING = "SDL.app.metadata.copyright";
    public const string SDL_PROP_APP_METADATA_URL_STRING = "SDL.app.metadata.url";
    public const string SDL_PROP_APP_METADATA_TYPE_STRING = "SDL.app.metadata.type";

    [Flags]
    public enum SDL_InitFlags : uint
    {
        SDL_INIT_TIMER = 0x1,
        SDL_INIT_AUDIO = 0x10,
        SDL_INIT_VIDEO = 0x20,
        SDL_INIT_JOYSTICK = 0x200,
        SDL_INIT_HAPTIC = 0x1000,
        SDL_INIT_GAMEPAD = 0x2000,
        SDL_INIT_EVENTS = 0x4000,
        SDL_INIT_SENSOR = 0x08000,
        SDL_INIT_CAMERA = 0x10000,
    }

    public enum SDL_AppResult
    {
        SDL_APP_CONTINUE = 0,
        SDL_APP_SUCCESS = 1,
        SDL_APP_FAILURE = 2,
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate SDL_AppResult SDL_AppInit_func(IntPtr appstate, int argc, IntPtr argv);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate SDL_AppResult SDL_AppIterate_func(IntPtr appstate);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate SDL_AppResult SDL_AppEvent_func(IntPtr appstate, SDL_Event* evt);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SDL_AppQuit_func(IntPtr appstate, SDL_AppResult result);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_Init(SDL_InitFlags flags);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_InitSubSystem(SDL_InitFlags flags);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_QuitSubSystem(SDL_InitFlags flags);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_InitFlags SDL_WasInit(SDL_InitFlags flags);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_Quit();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_IsMainThread();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SDL_MainThreadCallback(IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RunOnMainThread(SDL_MainThreadCallback callback, IntPtr userdata, SDLBool wait_complete);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetAppMetadata(string appname, string appversion, string appidentifier);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetAppMetadataProperty(string name, string value);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] public static extern string SDL_GetAppMetadataProperty(string name);

    // /usr/local/include/SDL3/SDL_loadso.h

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_LoadObject(string sofile);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_LoadFunction(IntPtr handle, string name);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_UnloadObject(IntPtr handle);

    // /usr/local/include/SDL3/SDL_locale.h

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Locale
    {
        public byte* language;
        public byte* country;
    }

    public static Span<IntPtr> SDL_GetPreferredLocales()
    {
        var result = SDL_GetPreferredLocales(out var count);
        return new Span<IntPtr>((void*)result, count);
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetPreferredLocales(out int count);

    // /usr/local/include/SDL3/SDL_log.h

    public enum SDL_LogCategory
    {
        SDL_LOG_CATEGORY_APPLICATION = 0,
        SDL_LOG_CATEGORY_ERROR = 1,
        SDL_LOG_CATEGORY_ASSERT = 2,
        SDL_LOG_CATEGORY_SYSTEM = 3,
        SDL_LOG_CATEGORY_AUDIO = 4,
        SDL_LOG_CATEGORY_VIDEO = 5,
        SDL_LOG_CATEGORY_RENDER = 6,
        SDL_LOG_CATEGORY_INPUT = 7,
        SDL_LOG_CATEGORY_TEST = 8,
        SDL_LOG_CATEGORY_GPU = 9,
        SDL_LOG_CATEGORY_RESERVED2 = 10,
        SDL_LOG_CATEGORY_RESERVED3 = 11,
        SDL_LOG_CATEGORY_RESERVED4 = 12,
        SDL_LOG_CATEGORY_RESERVED5 = 13,
        SDL_LOG_CATEGORY_RESERVED6 = 14,
        SDL_LOG_CATEGORY_RESERVED7 = 15,
        SDL_LOG_CATEGORY_RESERVED8 = 16,
        SDL_LOG_CATEGORY_RESERVED9 = 17,
        SDL_LOG_CATEGORY_RESERVED10 = 18,
        SDL_LOG_CATEGORY_CUSTOM = 19,
    }

    public enum SDL_LogPriority
    {
        SDL_LOG_PRIORITY_INVALID = 0,
        SDL_LOG_PRIORITY_TRACE = 1,
        SDL_LOG_PRIORITY_VERBOSE = 2,
        SDL_LOG_PRIORITY_DEBUG = 3,
        SDL_LOG_PRIORITY_INFO = 4,
        SDL_LOG_PRIORITY_WARN = 5,
        SDL_LOG_PRIORITY_ERROR = 6,
        SDL_LOG_PRIORITY_CRITICAL = 7,
        SDL_LOG_PRIORITY_COUNT = 8,
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetLogPriorities(SDL_LogPriority priority);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetLogPriority(int category, SDL_LogPriority priority);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_LogPriority SDL_GetLogPriority(int category);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_ResetLogPriorities();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetLogPriorityPrefix(SDL_LogPriority priority, string prefix);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_Log(string fmt);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_LogTrace(int category, string fmt);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_LogVerbose(int category, string fmt);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_LogDebug(int category, string fmt);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_LogInfo(int category, string fmt);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_LogWarn(int category, string fmt);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_LogError(int category, string fmt);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_LogCritical(int category, string fmt);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_LogMessage(int category, SDL_LogPriority priority, string fmt);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SDL_LogOutputFunction(IntPtr userdata, int category, SDL_LogPriority priority, byte* message);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_LogOutputFunction SDL_GetDefaultLogOutputFunction();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_GetLogOutputFunction(out SDL_LogOutputFunction callback, out IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetLogOutputFunction(SDL_LogOutputFunction callback, IntPtr userdata);

    // /usr/local/include/SDL3/SDL_messagebox.h

    [Flags]
    public enum SDL_MessageBoxFlags : uint
    {
        SDL_MESSAGEBOX_ERROR = 0x10,
        SDL_MESSAGEBOX_WARNING = 0x20,
        SDL_MESSAGEBOX_INFORMATION = 0x40,
        SDL_MESSAGEBOX_BUTTONS_LEFT_TO_RIGHT = 0x080,
        SDL_MESSAGEBOX_BUTTONS_RIGHT_TO_LEFT = 0x100,
    }

    [Flags]
    public enum SDL_MessageBoxButtonFlags : uint
    {
        SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT = 0x1,
        SDL_MESSAGEBOX_BUTTON_ESCAPEKEY_DEFAULT = 0x2,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_MessageBoxButtonData
    {
        public SDL_MessageBoxButtonFlags flags;
        public int buttonID;
        public byte* text;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_MessageBoxColor
    {
        public byte r;
        public byte g;
        public byte b;
    }

    public enum SDL_MessageBoxColorType
    {
        SDL_MESSAGEBOX_COLOR_BACKGROUND = 0,
        SDL_MESSAGEBOX_COLOR_TEXT = 1,
        SDL_MESSAGEBOX_COLOR_BUTTON_BORDER = 2,
        SDL_MESSAGEBOX_COLOR_BUTTON_BACKGROUND = 3,
        SDL_MESSAGEBOX_COLOR_BUTTON_SELECTED = 4,
        SDL_MESSAGEBOX_COLOR_COUNT = 5,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_MessageBoxColorScheme
    {
        public SDL_MessageBoxColor colors0;
        public SDL_MessageBoxColor colors1;
        public SDL_MessageBoxColor colors2;
        public SDL_MessageBoxColor colors3;
        public SDL_MessageBoxColor colors4;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_MessageBoxData
    {
        public SDL_MessageBoxFlags flags;
        public IntPtr window;
        public byte* title;
        public byte* message;
        public int numbuttons;
        public SDL_MessageBoxButtonData* buttons;
        public SDL_MessageBoxColorScheme* colorScheme;
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ShowMessageBox(ref SDL_MessageBoxData messageboxdata, out int buttonid);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ShowSimpleMessageBox(SDL_MessageBoxFlags flags, string title, string message, IntPtr window);

    // /usr/local/include/SDL3/SDL_metal.h

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_Metal_CreateView(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_Metal_DestroyView(IntPtr view);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_Metal_GetLayer(IntPtr view);

    // /usr/local/include/SDL3/SDL_misc.h

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_OpenURL(string url);

    // /usr/local/include/SDL3/SDL_platform.h

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] 
    public static extern string SDL_GetPlatform();

    // /usr/local/include/SDL3/SDL_process.h

    public const string SDL_PROP_PROCESS_CREATE_ARGS_POINTER = "SDL.process.create.args";
    public const string SDL_PROP_PROCESS_CREATE_ENVIRONMENT_POINTER = "SDL.process.create.environment";
    public const string SDL_PROP_PROCESS_CREATE_STDIN_NUMBER = "SDL.process.create.stdin_option";
    public const string SDL_PROP_PROCESS_CREATE_STDIN_POINTER = "SDL.process.create.stdin_source";
    public const string SDL_PROP_PROCESS_CREATE_STDOUT_NUMBER = "SDL.process.create.stdout_option";
    public const string SDL_PROP_PROCESS_CREATE_STDOUT_POINTER = "SDL.process.create.stdout_source";
    public const string SDL_PROP_PROCESS_CREATE_STDERR_NUMBER = "SDL.process.create.stderr_option";
    public const string SDL_PROP_PROCESS_CREATE_STDERR_POINTER = "SDL.process.create.stderr_source";
    public const string SDL_PROP_PROCESS_CREATE_STDERR_TO_STDOUT_BOOLEAN = "SDL.process.create.stderr_to_stdout";
    public const string SDL_PROP_PROCESS_CREATE_BACKGROUND_BOOLEAN = "SDL.process.create.background";
    public const string SDL_PROP_PROCESS_PID_NUMBER = "SDL.process.pid";
    public const string SDL_PROP_PROCESS_STDIN_POINTER = "SDL.process.stdin";
    public const string SDL_PROP_PROCESS_STDOUT_POINTER = "SDL.process.stdout";
    public const string SDL_PROP_PROCESS_STDERR_POINTER = "SDL.process.stderr";
    public const string SDL_PROP_PROCESS_BACKGROUND_BOOLEAN = "SDL.process.background";

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateProcess(IntPtr args, SDLBool pipe_stdio);

    public enum SDL_ProcessIO
    {
        SDL_PROCESS_STDIO_INHERITED = 0,
        SDL_PROCESS_STDIO_NULL = 1,
        SDL_PROCESS_STDIO_APP = 2,
        SDL_PROCESS_STDIO_REDIRECT = 3,
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateProcessWithProperties(uint props);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetProcessProperties(IntPtr process);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_ReadProcess(IntPtr process, out UIntPtr datasize, out int exitcode);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetProcessInput(IntPtr process);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetProcessOutput(IntPtr process);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_KillProcess(IntPtr process, SDLBool force);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WaitProcess(IntPtr process, SDLBool block, out int exitcode);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DestroyProcess(IntPtr process);

    // /usr/local/include/SDL3/SDL_storage.h

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_StorageInterface
    {
        public uint version;
        public IntPtr close; // WARN_ANONYMOUS_FUNCTION_POINTER
        public IntPtr ready; // WARN_ANONYMOUS_FUNCTION_POINTER
        public IntPtr enumerate; // WARN_ANONYMOUS_FUNCTION_POINTER
        public IntPtr info; // WARN_ANONYMOUS_FUNCTION_POINTER
        public IntPtr read_file; // WARN_ANONYMOUS_FUNCTION_POINTER
        public IntPtr write_file; // WARN_ANONYMOUS_FUNCTION_POINTER
        public IntPtr mkdir; // WARN_ANONYMOUS_FUNCTION_POINTER
        public IntPtr remove; // WARN_ANONYMOUS_FUNCTION_POINTER
        public IntPtr rename; // WARN_ANONYMOUS_FUNCTION_POINTER
        public IntPtr copy; // WARN_ANONYMOUS_FUNCTION_POINTER
        public IntPtr space_remaining; // WARN_ANONYMOUS_FUNCTION_POINTER
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_OpenTitleStorage(string @override, uint props);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_OpenUserStorage(string org, string app, uint props);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_OpenFileStorage(string path);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_OpenStorage(ref SDL_StorageInterface iface, IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_CloseStorage(IntPtr storage);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_StorageReady(IntPtr storage);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetStorageFileSize(IntPtr storage, string path, out ulong length);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ReadStorageFile(IntPtr storage, string path, IntPtr destination, ulong length);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WriteStorageFile(IntPtr storage, string path, IntPtr source, ulong length);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_CreateStorageDirectory(IntPtr storage, string path);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_EnumerateStorageDirectory(IntPtr storage, string path, SDL_EnumerateDirectoryCallback callback, IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RemoveStoragePath(IntPtr storage, string path);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenameStoragePath(IntPtr storage, string oldpath, string newpath);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_CopyStorageFile(IntPtr storage, string oldpath, string newpath);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetStoragePathInfo(IntPtr storage, string path, out SDL_PathInfo info);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong SDL_GetStorageSpaceRemaining(IntPtr storage);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GlobStorageDirectory(IntPtr storage, string path, string pattern, SDL_GlobFlags flags, out int count);

    // /usr/local/include/SDL3/SDL_system.h

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_IsTablet();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_IsTV();

    public enum SDL_Sandbox
    {
        SDL_SANDBOX_NONE = 0,
        SDL_SANDBOX_UNKNOWN_CONTAINER = 1,
        SDL_SANDBOX_FLATPAK = 2,
        SDL_SANDBOX_SNAP = 3,
        SDL_SANDBOX_MACOS = 4,
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Sandbox SDL_GetSandbox();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_OnApplicationWillTerminate();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_OnApplicationDidReceiveMemoryWarning();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_OnApplicationWillEnterBackground();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_OnApplicationDidEnterBackground();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_OnApplicationWillEnterForeground();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_OnApplicationDidEnterForeground();

    // /usr/local/include/SDL3/SDL_time.h

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_DateTime
    {
        public int year;
        public int month;
        public int day;
        public int hour;
        public int minute;
        public int second;
        public int nanosecond;
        public int day_of_week;
        public int utc_offset;
    }

    public enum SDL_DateFormat
    {
        SDL_DATE_FORMAT_YYYYMMDD = 0,
        SDL_DATE_FORMAT_DDMMYYYY = 1,
        SDL_DATE_FORMAT_MMDDYYYY = 2,
    }

    public enum SDL_TimeFormat
    {
        SDL_TIME_FORMAT_24HR = 0,
        SDL_TIME_FORMAT_12HR = 1,
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetDateTimeLocalePreferences(out SDL_DateFormat dateFormat, out SDL_TimeFormat timeFormat);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetCurrentTime(IntPtr ticks);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_TimeToDateTime(long ticks, out SDL_DateTime dt, SDLBool localTime);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_DateTimeToTime(ref SDL_DateTime dt, IntPtr ticks);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_TimeToWindows(long ticks, out uint dwLowDateTime, out uint dwHighDateTime);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern long SDL_TimeFromWindows(uint dwLowDateTime, uint dwHighDateTime);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetDaysInMonth(int year, int month);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetDayOfYear(int year, int month, int day);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetDayOfWeek(int year, int month, int day);

    // /usr/local/include/SDL3/SDL_timer.h

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong SDL_GetTicks();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong SDL_GetTicksNS();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong SDL_GetPerformanceCounter();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong SDL_GetPerformanceFrequency();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_Delay(uint ms);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DelayNS(ulong ns);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DelayPrecise(ulong ns);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint SDL_TimerCallback(IntPtr userdata, uint timerID, uint interval);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_AddTimer(uint interval, SDL_TimerCallback callback, IntPtr userdata);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate ulong SDL_NSTimerCallback(IntPtr userdata, uint timerID, ulong interval);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_AddTimerNS(ulong interval, SDL_NSTimerCallback callback, IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RemoveTimer(uint id);

    // /usr/local/include/SDL3/SDL_tray.h

    [Flags]
    public enum SDL_TrayEntryFlags : uint
    {
        SDL_TRAYENTRY_BUTTON = 0x00000001u,
        SDL_TRAYENTRY_CHECKBOX = 0x00000002u,
        SDL_TRAYENTRY_SUBMENU = 0x00000004u,
        SDL_TRAYENTRY_DISABLED = 0x80000000u,
        SDL_TRAYENTRY_CHECKED = 0x40000000u,
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SDL_TrayCallback(IntPtr userdata, IntPtr entry);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateTray(IntPtr icon, string tooltip);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetTrayIcon(IntPtr tray, IntPtr icon);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetTrayTooltip(IntPtr tray, string tooltip);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateTrayMenu(IntPtr tray);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateTraySubmenu(IntPtr entry);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetTrayMenu(IntPtr tray);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetTraySubmenu(IntPtr entry);

    public static Span<IntPtr> SDL_GetTrayEntries(IntPtr menu)
    {
        var result = SDL_GetTrayEntries(menu, out var size);
        return new Span<IntPtr>((void*)result, size);
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetTrayEntries(IntPtr menu, out int size);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_RemoveTrayEntry(IntPtr entry);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_InsertTrayEntryAt(IntPtr menu, int pos, string label, SDL_TrayEntryFlags flags);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetTrayEntryLabel(IntPtr entry, string label);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] public static extern string SDL_GetTrayEntryLabel(IntPtr entry);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetTrayEntryChecked(IntPtr entry, SDLBool check);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetTrayEntryChecked(IntPtr entry);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetTrayEntryEnabled(IntPtr entry, SDLBool enabled);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetTrayEntryEnabled(IntPtr entry);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetTrayEntryCallback(IntPtr entry, SDL_TrayCallback callback, IntPtr userdata);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_ClickTrayEntry(IntPtr entry);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DestroyTray(IntPtr tray);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetTrayEntryParent(IntPtr entry);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetTrayMenuParentEntry(IntPtr menu);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetTrayMenuParentTray(IntPtr menu);

    // /usr/local/include/SDL3/SDL_version.h

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetVersion();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)] public static extern string SDL_GetRevision();

    // /usr/local/include/SDL3/SDL_main.h

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int SDL_main_func(int argc, IntPtr argv);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetMainReady();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_RunApp(int argc, IntPtr argv, SDL_main_func mainFunction, IntPtr reserved);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_EnterAppMainCallbacks(int argc, IntPtr argv, SDL_AppInit_func appinit, SDL_AppIterate_func appiter, SDL_AppEvent_func appevent, SDL_AppQuit_func appquit);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_GDKSuspendComplete();
}

