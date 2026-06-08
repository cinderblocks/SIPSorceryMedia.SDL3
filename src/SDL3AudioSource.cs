/**
 * @file SDLAudioSource.cs
 * @brief AudioSource using SDL3 to capture an audio stream
 *
 * Copyright 2021, Christophe Irles.
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
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
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

using Microsoft.Extensions.Logging;
using SDL3;
using SIPSorceryMedia.Abstractions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SIPSorceryMedia.SDL3
{
    /// <summary>
    /// Performance statistics for audio source.
    /// </summary>
    public struct AudioSourceStats
    {
        public long OverrunCount { get; set; }
        public long DroppedFrames { get; set; }
        public bool IsActive { get; set; }
    }

    public class SDL3AudioSource : IAudioSource, IDisposable
    {
        private const int MAX_AUDIO_RENT = 1024 * 1024; // 1MB cap for rented buffers
        // Channel capacity — max ~500ms of audio at typical frame rates
        private const int CHANNEL_CAPACITY = 25;

        private static readonly ILogger log = SIPSorcery.LogFactory.CreateLogger<SDL3AudioSource>();

        private readonly (uint id, string name) _audioDevice;
        private SDL3AudioStreamSafeHandle? _audioStream = null;

        private readonly object _stateLock = new object();

        private readonly IAudioEncoder _audioEncoder;
        private readonly MediaFormatManager<AudioFormat> _audioFormatManager;

        private bool _isStarted = false;
        private bool _isPaused = true;

        private readonly int frameSize = 0;

        // main capture loop task
        private CancellationTokenSource? _mainCts = null;
        private Task? _mainTask = null;

        private AudioSamplingRatesEnum audioSamplingRates;

        private bool _disposed = false;

        // When true the SDL audio stream callback will provide data; main loop should not pull data.
        private volatile bool _useStreamCallbackReading = false;

        // Bounded channel for callback data
        private readonly Channel<(byte[] Buffer, int Length)> _callbackChannel;
        private CancellationTokenSource? _callbackCts = null;
        private Task? _callbackTask = null;

        // Stats for diagnostics
        private long _overrunCount = 0;
        private long _droppedFrames = 0;

#region EVENT

        // Events are null when no subscribers — this is standard .NET event contract.
        public event EncodedSampleDelegate OnAudioSourceEncodedSample = null!;
        public event RawAudioSampleDelegate OnAudioSourceRawSample = null!;
        public event SourceErrorDelegate OnAudioSourceError = null!;
        public event Action<EncodedAudioFrame> OnAudioSourceEncodedFrameReady = null!;

#endregion EVENT

        public SDL3AudioSource(string audioInDeviceName, IAudioEncoder audioEncoder, int frameSize = 1920)
        {
            if (audioEncoder == null)
                throw new ApplicationException("Audio encoder provided is null");

            if (string.IsNullOrEmpty(audioInDeviceName))
            {
                _audioDevice = (SDL.SDL_AUDIO_DEVICE_DEFAULT_RECORDING, "Default Microphone");
            }
            else
            {
                var device = SDL3Helper.GetAudioRecordingDevice(audioInDeviceName!);
                if (!device.HasValue)
                {
                    throw new ApplicationException($"Could not get audio device named '{audioInDeviceName}'");
                }
                _audioDevice = device.Value;
            }

            _audioFormatManager = new MediaFormatManager<AudioFormat>(audioEncoder.SupportedFormats);
            _audioEncoder = audioEncoder;

            this.frameSize = frameSize;

            _callbackChannel = Channel.CreateBounded<(byte[] Buffer, int Length)>(
                new BoundedChannelOptions(CHANNEL_CAPACITY)
                {
                    FullMode = BoundedChannelFullMode.DropOldest,
                    SingleReader = true,
                    SingleWriter = false
                });
        }

        ~SDL3AudioSource()
        {
            Dispose(false);
        }

        private void RaiseAudioSourceError(string err)
        {
            CloseSync();
            OnAudioSourceError?.Invoke(err);
        }

        private async Task MainLoopAsync(CancellationToken ct)
        {
            var pool = ArrayPool<byte>.Shared;

            while (!ct.IsCancellationRequested && !_disposed)
            {
                // If we're using the SDL callback to unqueue, ensure callback task is running
                if (_useStreamCallbackReading)
                {
                    if (_callbackTask == null || _callbackTask.IsCompleted)
                    {
                        var oldCallbackCts = _callbackCts;
                        _callbackCts = new CancellationTokenSource();
                        try { oldCallbackCts?.Cancel(); } catch (Exception) { }
                        oldCallbackCts?.Dispose();
                        _callbackTask = Task.Run(() => CallbackWorkerLoopAsync(_callbackCts.Token), ct);
                    }

                    try { await Task.Delay(16, ct).ConfigureAwait(false); } catch (OperationCanceledException) { break; }
                    continue;
                }

                int size = 0;

                do
                {
                    SDL3AudioStreamSafeHandle? currentHandle;
                    lock (_stateLock)
                    {
                        currentHandle = _audioStream;
                    }

                    if (currentHandle == null || currentHandle.IsInvalid)
                    {
                        RaiseAudioSourceError($"SDLAudioSource [{_audioDevice.name}] stopped.");
                        return;
                    }

                    bool added = false;
                    try
                    {
                        currentHandle.DangerousAddRef(ref added);
                        size = SDL3Helper.GetAudioStreamAvailable(currentHandle);
                    }
                    finally
                    {
                        if (added) currentHandle.DangerousRelease();
                    }

                    if (size >= frameSize * 2)
                    {
                        var bufferSize = frameSize != 0 ? frameSize * 2 : size;
                        byte[] buf = pool.Rent(bufferSize);

                        try
                        {
                            SDL3Helper.GetAudioStreamData(currentHandle, buf, bufferSize);
                            ProcessAudioBuffer(buf, bufferSize);
                        }
                        finally
                        {
                            pool.Return(buf);
                        }

                        size -= bufferSize;
                    }
                } while (size >= frameSize && !_disposed && !ct.IsCancellationRequested);

                try { await Task.Delay(16, ct).ConfigureAwait(false); } catch (OperationCanceledException) { break; }
            }
        }

        private void InitRecordingDevice()
        {
            try
            {
                CloseSync();

                AudioFormat audioFormat = _audioFormatManager.SelectedFormat;
                audioSamplingRates = audioFormat.ClockRate == AudioFormat.DEFAULT_CLOCK_RATE * 2
                    ? AudioSamplingRatesEnum.Rate16KHz : AudioSamplingRatesEnum.Rate8KHz;

                var audioSpec = SDL3Helper.GetAudioSpec(audioFormat.ClockRate);
                SDL3AudioStreamSafeHandle? newHandle = SDL3Helper.OpenAudioDeviceStreamHandle(_audioDevice.id, ref audioSpec, UnqueueStreamCallback);

                lock (_stateLock)
                {
                    _audioStream = newHandle;
                }

                _useStreamCallbackReading = newHandle != null && !newHandle.IsInvalid;

                if (newHandle != null && !newHandle.IsInvalid)
                    log.LogDebug("[InitRecordingDevice] Audio source - Id:[{AudioDeviceId}] - DeviceName:[{AudioDeviceName}]", _audioDevice.id, _audioDevice.name);
                else
                {
                    log.LogError("[InitRecordingDevice] SDLAudioSource failed to initialise device. No audio device found with [{AudioDeviceId}] and [{AudioDeviceName}]", _audioDevice.id, _audioDevice.name);
                    OnAudioSourceError?.Invoke($"SDLAudioSource failed to initialise device. No audio device found with [{_audioDevice.id}] and [{_audioDevice.name}]");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "[InitRecordingDevice] SDLAudioSource failed to initialise device [{AudioDeviceId}] - [{AudioDeviceName}].", _audioDevice.id, _audioDevice.name);
                OnAudioSourceError?.Invoke($"SDLAudioSource failed to initialise device [{_audioDevice.name}] and [{_audioDevice.id}] - Exception:[{ex.Message}]");
            }
        }

        private void UnqueueStreamCallback(IntPtr userdata, SDL3AudioStreamSafeHandle? stream, int additionalAmount, int totalAmount)
        {
            if (stream == null || stream.IsInvalid || additionalAmount <= 0)
                return;

            var pool = ArrayPool<byte>.Shared;
            int toRead = additionalAmount > MAX_AUDIO_RENT ? MAX_AUDIO_RENT : additionalAmount;

            byte[] rented = pool.Rent(toRead);
            int read = 0;
            try
            {
                GC.KeepAlive(stream);
                read = SDL3Helper.GetAudioStreamData(stream, rented, toRead);

                if (read < 0) read = 0;
                if (read > rented.Length) read = rented.Length;

                if (read <= 0)
                {
                    pool.Return(rented);
                    return;
                }

                if (!_callbackChannel.Writer.TryWrite((rented, read)))
                {
                    Interlocked.Increment(ref _droppedFrames);
                    pool.Return(rented);
                }
            }
            catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)
            {
                log.LogError(ex, "UnqueueStreamCallback: error reading audio stream");
                try { pool.Return(rented); } catch (Exception) { }
            }
        }

        private async Task CallbackWorkerLoopAsync(CancellationToken ct)
        {
            var pool = ArrayPool<byte>.Shared;
            var reader = _callbackChannel.Reader;

            try
            {
                while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
                {
                    // Batch up to 5 buffers per iteration for efficiency
                    int processedCount = 0;
                    const int maxBatchSize = 5;

                    while (processedCount < maxBatchSize && reader.TryRead(out var seg))
                    {
                        processedCount++;
                        try
                        {
                            ProcessAudioBuffer(seg.Buffer, seg.Length);
                        }
                        catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)
                        {
                            log.LogError(ex, "Error processing queued audio buffer");
                        }
                        finally
                        {
                            pool.Return(seg.Buffer);
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)
            {
                log.LogError(ex, "CallbackWorkerLoopAsync unexpected error");
            }
        }

        private void ProcessAudioBuffer(byte[] buffer, int length)
        {
            int shortCount = length / sizeof(short);
            // ArrayPool<short> — subscribers must not retain the array reference past the event handler return
            var pool = ArrayPool<short>.Shared;
            var pcm = pool.Rent(shortCount);
            try
            {
                Buffer.BlockCopy(buffer, 0, pcm, 0, shortCount * sizeof(short));

                OnAudioSourceRawSample?.Invoke(audioSamplingRates, (uint)shortCount, pcm);

                if (OnAudioSourceEncodedSample != null || OnAudioSourceEncodedFrameReady != null)
                {
                    var encodedSample = _audioEncoder.EncodeAudio(pcm, _audioFormatManager.SelectedFormat);
                    if (encodedSample.Length > 0)
                    {
                        var fmt = _audioFormatManager.SelectedFormat;
                        uint durationRtp = (uint)(shortCount * fmt.RtpClockRate / fmt.ClockRate);
                        uint durationMs  = (uint)(shortCount * 1000 / fmt.ClockRate);

                        OnAudioSourceEncodedSample?.Invoke(durationRtp, encodedSample);
                        OnAudioSourceEncodedFrameReady?.Invoke(new EncodedAudioFrame(0, fmt, durationMs, encodedSample));
                    }
                }
            }
            finally
            {
                pool.Return(pcm);
            }
        }

        public AudioSourceStats GetStats()
        {
            bool isActive;
            lock (_stateLock) { isActive = _isStarted && !_isPaused; }
            return new AudioSourceStats
            {
                OverrunCount = Interlocked.Read(ref _overrunCount),
                DroppedFrames = Interlocked.Read(ref _droppedFrames),
                IsActive = isActive
            };
        }

        public void ResetStats()
        {
            Interlocked.Exchange(ref _overrunCount, 0);
            Interlocked.Exchange(ref _droppedFrames, 0);
        }

        /// <summary>
        /// Capture volume as a gain multiplier. 1.0 = unity gain, 0.0 = silent.
        /// </summary>
        public float Volume
        {
            get
            {
                SDL3AudioStreamSafeHandle? handle;
                lock (_stateLock) { handle = _audioStream; }
                return SDL3Helper.GetAudioStreamGain(handle);
            }
            set
            {
                SDL3AudioStreamSafeHandle? handle;
                lock (_stateLock) { handle = _audioStream; }
                SDL3Helper.SetAudioStreamGain(handle, Math.Max(0f, value));
            }
        }

        public Task PauseAudio()
        {
            SDL3AudioStreamSafeHandle? currentHandle = null;
            bool doPause = false;

            lock (_stateLock)
            {
                if (_isStarted && !_isPaused)
                {
                    _isPaused = true;
                    doPause = true;
                    currentHandle = _audioStream;
                }
            }

            if (doPause && currentHandle != null && !currentHandle.IsInvalid)
            {
                try { _mainCts?.Cancel(); } catch (Exception) { }
                SDL3Helper.PauseAudioStreamDevice(currentHandle);
                log.LogDebug("[PauseAudio] Audio source - Id:[{AudioInDeviceId}]", _audioDevice.id);
            }

            return Task.CompletedTask;
        }

        public Task ResumeAudio()
        {
            SDL3AudioStreamSafeHandle? currentHandle = null;
            bool doResume = false;

            lock (_stateLock)
            {
                if (_isStarted && _isPaused)
                {
                    _isPaused = false;
                    doResume = true;
                    currentHandle = _audioStream;
                }
            }

            return ResumeAudioInternal(doResume, currentHandle);
        }

        private Task ResumeAudioInternal(bool doResume, SDL3AudioStreamSafeHandle? currentHandle)
        {
            if (doResume && currentHandle != null && !currentHandle.IsInvalid)
            {
                if (_mainTask == null || _mainTask.IsCompleted)
                {
                    var oldCts = _mainCts;
                    _mainCts = new CancellationTokenSource();
                    try { oldCts?.Cancel(); } catch (Exception) { }
                    oldCts?.Dispose();
                    _mainTask = Task.Run(() => MainLoopAsync(_mainCts.Token));
                }

                SDL3Helper.ResumeAudioStreamDevice(currentHandle);
                log.LogDebug("[ResumeAudio] Audio source - Id:[{AudioInDeviceId}]", _audioDevice.id);
            }

            return Task.CompletedTask;
        }

        public Task StartAudio() => StartAudioAsync();

        public async Task StartAudioAsync()
        {
            bool needResume = false;
            lock (_stateLock)
            {
                if (!_isStarted && _audioStream != null && !_audioStream.IsInvalid)
                {
                    _isStarted = true;
                    _isPaused = true;
                    needResume = true;
                }
            }
            if (needResume)
                await ResumeAudio().ConfigureAwait(false);
        }

        public Task CloseAudio() => CloseAudioAsync();

        public Task CloseAudioAsync()
        {
            CloseSync();
            return Task.CompletedTask;
        }

        private void CloseSync()
        {
            SDL3AudioStreamSafeHandle? toDestroy;
            lock (_stateLock)
            {
                toDestroy = _audioStream;
                _audioStream = null;
                _isStarted = false;
                _isPaused = true;
                _useStreamCallbackReading = false;
            }

            try { _mainCts?.Cancel(); } catch (Exception) { }
            try { _callbackCts?.Cancel(); } catch (Exception) { }

            if (toDestroy != null && !toDestroy.IsInvalid)
            {
                try { SDL3Helper.PauseAudioStreamDevice(toDestroy); } catch (Exception) { }
                try
                {
                    SDL3Helper.DestroyAudioStream(toDestroy);
                    log.LogDebug("[CloseAudio] Audio source - Id:[{AudioInDeviceId}] - Name:[{AudioDeviceName}]", _audioDevice.id, _audioDevice.name);
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error destroying audio stream");
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                try { _mainCts?.Cancel(); } catch (Exception) { }
                try { _callbackCts?.Cancel(); } catch (Exception) { }

                CloseSync();

                _callbackChannel.Writer.TryComplete();
                var pool = ArrayPool<byte>.Shared;
                while (_callbackChannel.Reader.TryRead(out var seg))
                {
                    try { pool.Return(seg.Buffer); } catch (Exception) { }
                }

                _callbackCts?.Dispose();
                _callbackCts = null;
                _mainCts?.Dispose();
                _mainCts = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public List<AudioFormat> GetAudioSourceFormats() => _audioFormatManager.GetSourceFormats();

        public void SetAudioSourceFormat(AudioFormat audioFormat)
        {
            log.LogDebug("Setting audio source format to {AudioFormatFormatId}:{AudioFormatFormatName} {AudioFormatClockRate}.",
                audioFormat.FormatID, audioFormat.FormatName, audioFormat.ClockRate);
            _audioFormatManager.SetSelectedFormat(audioFormat);
            try
            {
                InitRecordingDevice();
                StartAudio().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "SetAudioSourceFormat failed");
                OnAudioSourceError?.Invoke($"SetAudioSourceFormat failed: {ex.Message}");
            }
        }

        public void RestrictFormats(Func<AudioFormat, bool> filter) => _audioFormatManager.RestrictFormats(filter);

        public void ExternalAudioSourceRawSample(AudioSamplingRatesEnum samplingRate, uint durationMilliseconds, short[] sample)
        {
            if (sample == null || sample.Length == 0) return;
            var bytes = new byte[sample.Length * sizeof(short)];
            Buffer.BlockCopy(sample, 0, bytes, 0, bytes.Length);
            ProcessAudioBuffer(bytes, bytes.Length);
        }

        public bool HasEncodedAudioSubscribers() => OnAudioSourceEncodedSample != null;

        public bool IsAudioSourcePaused()
        {
            lock (_stateLock) { return _isPaused; }
        }
    }
}
