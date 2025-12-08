/**
 * @file SDLAudioSource.cs
 * @brief Example of an AudioSource using SDL3 to playback audio stream
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
    public class SDL3AudioSource : IAudioSource, IDisposable
    {
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

        // channel populated by the SDL callback; worker will consume and return buffers to pool
        private readonly Channel<(byte[] Buffer, int Length)> _callbackChannel = Channel.CreateUnbounded<(byte[] Buffer, int Length)>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
        private CancellationTokenSource? _callbackCts = null;
        private Task? _callbackTask = null;

#region EVENT

        public event EncodedSampleDelegate OnAudioSourceEncodedSample = null;
        public event RawAudioSampleDelegate OnAudioSourceRawSample = null;
        public event SourceErrorDelegate OnAudioSourceError = null;
        public event Action<EncodedAudioFrame> OnAudioSourceEncodedFrameReady = null;

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
                var device = SDL3Helper.GetAudioPlaybackDevice(audioInDeviceName!);
                if (!device.HasValue)
                {
                    throw new ApplicationException($"Could not get audio device named '{audioInDeviceName}'");
                }
                _audioDevice = device.Value;
            }

            _audioFormatManager = new MediaFormatManager<AudioFormat>(audioEncoder.SupportedFormats);
            _audioEncoder = audioEncoder;

            this.frameSize = frameSize;
        }

        ~SDL3AudioSource()
        {
            Dispose(false);
        }

        private void RaiseAudioSourceError(string err)
        {
            // fire-and-forget close
            _ = CloseAudioAsync();
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
                        _callbackCts = new CancellationTokenSource();
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
                        // fire-and-forget close
                        _ = CloseAudioAsync();
                        OnAudioSourceError?.Invoke($"SDLAudioSource [{_audioDevice.name}] stopped.");
                        return;
                    }

                    bool added = false;
                    try
                    {
                        currentHandle.DangerousAddRef(ref added);
                        size = SDL3Helper.GetAudioStreamQueued(currentHandle);
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
                            bool add2 = false;
                            try
                            {
                                currentHandle.DangerousAddRef(ref add2);
                                // Read directly into managed buffer using SafeHandle overload
                                SDL3Helper.GetAudioStreamData(currentHandle, buf, bufferSize);
                            }
                            finally
                            {
                                if (add2) currentHandle.DangerousRelease();
                            }

                            int shortCount = bufferSize / sizeof(short);
                            short[] pcm = new short[shortCount];
                            Buffer.BlockCopy(buf, 0, pcm, 0, shortCount * sizeof(short));

                            OnAudioSourceRawSample?.Invoke(audioSamplingRates, (uint)pcm.Length, pcm);

                            if (OnAudioSourceEncodedSample != null)
                            {
                                var encodedSample = _audioEncoder.EncodeAudio(pcm, _audioFormatManager.SelectedFormat);
                                if (encodedSample.Length > 0)
                                    OnAudioSourceEncodedSample?.Invoke((uint)(pcm.Length * _audioFormatManager.SelectedFormat.RtpClockRate / _audioFormatManager.SelectedFormat.ClockRate), encodedSample);
                            }
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
                // Stop previous recording device
                _ = CloseAudioAsync();

                // Init recording device.
                AudioFormat audioFormat = _audioFormatManager.SelectedFormat;
                audioSamplingRates = audioFormat.ClockRate == AudioFormat.DEFAULT_CLOCK_RATE * 2
                    ? AudioSamplingRatesEnum.Rate16KHz : AudioSamplingRatesEnum.Rate8KHz;

                var audioSpec = SDL3Helper.GetAudioSpec(audioFormat.ClockRate);

                // Use SafeHandle-based open
                SDL3AudioStreamSafeHandle? newHandle = SDL3Helper.OpenAudioDeviceStreamHandle(_audioDevice.id, ref audioSpec, UnqueueStreamCallback);

                 lock (_stateLock)
                 {
                     _audioStream?.Dispose();
                     _audioStream = newHandle;
                 }

                 // when we open with UnqueueStreamCallback, indicate we will use callback reading
                 _useStreamCallbackReading = newHandle != null && !newHandle.IsInvalid;

                if (newHandle != null && !newHandle.IsInvalid)
                    log.LogDebug("[InitRecordingDevice] Audio source - Id:[{AudioDeviceId}] - DeviceName:[{AudioDeviceName}]", _audioDevice.id, _audioDevice.name);
                else
                {
                    log.LogError("[InitRecordingDevice] SDLAudioSource failed to initialise device. No audio device found with [{AudioDeviceId} ] and [ {AudioDeviceName}]", _audioDevice.id, _audioDevice.name);
                    _ = CloseAudioAsync();
                    OnAudioSourceError?.Invoke($"SDLAudioSource failed to initialise device. No audio device found with [{_audioDevice.id} ] and [ {_audioDevice.name}]");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "InitRecordingDevice] SDLAudioSource failed to initialise device [{AudioDeviceId} ] - [ {AudioDeviceName}].", _audioDevice.id, _audioDevice.name);
                _ = CloseAudioAsync();
                OnAudioSourceError?.Invoke($"SDLAudioSource failed to initialise device [{_audioDevice.name}] and [{_audioDevice.id}] - Exception:[{ex.Message}]");
            }
        }

        private void UnqueueStreamCallback(IntPtr userdata, SDL3AudioStreamSafeHandle? stream, int additionalAmount, int totalAmount)
        {
            if (stream == null || stream.IsInvalid || additionalAmount <= 0)
            {
                return;
            }

            var pool = ArrayPool<byte>.Shared;
            int toRead = additionalAmount;
            byte[] rented = pool.Rent(toRead);

            int read = 0;
            try
            {
                read = SDL3Helper.GetAudioStreamData(stream, rented, toRead);

                if (read <= 0)
                {
                    pool.Return(rented);
                    return;
                }

                var writer = _callbackChannel.Writer;
                if (!writer.TryWrite((rented, read)))
                {
                    pool.Return(rented);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "UnqueueStreamCallback: error reading audio stream");
                if (rented != null)
                {
                    pool.Return(rented);
                }
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
                    while (reader.TryRead(out var seg))
                    {
                        try
                        {
                            int read = seg.Length;
                            int shortCount = read / sizeof(short);
                            short[] pcm = new short[shortCount];
                            Buffer.BlockCopy(seg.Buffer, 0, pcm, 0, shortCount * sizeof(short));

                            OnAudioSourceRawSample?.Invoke(audioSamplingRates, (uint)pcm.Length, pcm);

                            if (OnAudioSourceEncodedSample != null)
                            {
                                var encodedSample = _audioEncoder.EncodeAudio(pcm, _audioFormatManager.SelectedFormat);
                                if (encodedSample.Length > 0)
                                    OnAudioSourceEncodedSample?.Invoke((uint)(pcm.Length * _audioFormatManager.SelectedFormat.RtpClockRate / _audioFormatManager.SelectedFormat.ClockRate), encodedSample);
                            }
                        }
                        catch (Exception ex)
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
            catch (Exception ex)
            {
                log.LogError(ex, "CallbackWorkerLoopAsync unexpected error");
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
                // cancel main loop
                _mainCts?.Cancel();

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
                    _mainCts = new CancellationTokenSource();
                    _mainTask = Task.Run(() => MainLoopAsync(_mainCts.Token));
                }

                SDL3Helper.ResumeAudioStreamDevice(currentHandle);
                log.LogDebug("[ResumeAudio] Audio source - Id:[{AudioInDeviceId}]", _audioDevice.id);
            }

            return Task.CompletedTask;
        }

        public Task StartAudio()
        {
            return StartAudioAsync();
        }

        public async Task StartAudioAsync()
        {
            if (!_isStarted && _audioStream != null && !_audioStream.IsInvalid)
            {
                _isStarted = true;
                _isPaused = true;

                await ResumeAudio();
            }
        }

        public Task CloseAudio()
        {
            return CloseAudioAsync();
        }

        public async Task CloseAudioAsync()
        {
            if (_isStarted)
            {
                await PauseAudio();
                if (_audioStream != null && !_audioStream.IsInvalid)
                {
                    try
                    {
                        _audioStream.Dispose();
                        log.LogDebug("[CloseAudio] Audio source - Id:[{AudioInDeviceId}] - Name:[{AudioInDeviceName}]", 
                            _audioDevice.id, _audioDevice.name);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "Error destroying audio stream");
                    }
                }
            }

            lock (_stateLock)
            {
                _isStarted = false;
                _audioStream = null;
                _useStreamCallbackReading = false;
            }

            try { _mainCts?.Cancel(); } catch { }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                // dispose managed
                try
                {
                    // cancel main and callback tasks without blocking
                    try { _mainCts?.Cancel(); } catch { }
                    try { _callbackCts?.Cancel(); } catch { }

                    // fire-and-forget close
                    _ = CloseAudio();

                    _callbackCts?.Dispose();
                    _mainCts?.Dispose();

                    // complete channel and return any queued buffers
                    _callbackChannel.Writer.Complete();
                    var pool = ArrayPool<byte>.Shared;
                    while (_callbackChannel.Reader.TryRead(out var seg))
                    {
                        try { pool.Return(seg.Buffer); } catch { }
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error during Dispose CloseAudio");
                }
            }
            else
            {
                // finalizer: nothing - SafeHandle will free native resource when finalized
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public List<AudioFormat> GetAudioSourceFormats()
        {
            return _audioFormatManager.GetSourceFormats();
        }

        public void SetAudioSourceFormat(AudioFormat audioFormat)
        {
            // Fire-and-forget async version to avoid blocking callers expecting synchronous API.
            _ = SetAudioSourceFormatAsync(audioFormat);
        }

        public async Task SetAudioSourceFormatAsync(AudioFormat audioFormat)
        {
            log.LogDebug("Setting audio source format to {AudioFormatFormatId}:{AudioFormatFormatName} {AudioFormatClockRate}.", 
                audioFormat.FormatID, audioFormat.FormatName, audioFormat.ClockRate);
            _audioFormatManager.SetSelectedFormat(audioFormat);
 
            InitRecordingDevice();
            await StartAudio();
        }

        public void RestrictFormats(Func<AudioFormat, bool> filter)
        {
            _audioFormatManager.RestrictFormats(filter);
        }

        public void ExternalAudioSourceRawSample(AudioSamplingRatesEnum samplingRate, uint durationMilliseconds, short[] sample) 
            => throw new NotImplementedException();

        public bool HasEncodedAudioSubscribers() => OnAudioSourceEncodedSample != null;

        public bool IsAudioSourcePaused()
        {
            lock (_stateLock)
            {
                return _isPaused;
            }
        }
    }
}
