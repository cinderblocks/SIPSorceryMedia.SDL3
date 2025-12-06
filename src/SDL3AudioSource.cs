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
using SIPSorceryMedia.Abstractions;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SIPSorceryMedia.SDL3
{
    public class SDL3AudioSource : IAudioSource, IDisposable
    {
        private static readonly ILogger log = SIPSorcery.LogFactory.CreateLogger<SDL3AudioSource>();

        private readonly (uint id, string name) _audioDevice;
        private IntPtr _audioStream = IntPtr.Zero;

        private readonly object _stateLock = new object();

        private readonly IAudioEncoder _audioEncoder;
        private readonly MediaFormatManager<AudioFormat> _audioFormatManager;

        private bool _isStarted = false;
        private bool _isPaused = true;

        private readonly int frameSize = 0;

        private readonly BackgroundWorker backgroundWorker;

        private AudioSamplingRatesEnum audioSamplingRates;

        private bool _disposed = false;

        // When true the SDL audio stream callback will provide data; background worker should not pull data.
        private volatile bool _useStreamCallbackReading = false;

        // queue populated by the SDL callback; worker will consume and return buffers to pool
        private readonly ConcurrentQueue<(byte[] Buffer, int Length)> _callbackQueue = new ConcurrentQueue<(byte[] Buffer, int Length)>();
        private readonly SemaphoreSlim _queueSemaphore = new SemaphoreSlim(0);

#region EVENT

        public event EncodedSampleDelegate? OnAudioSourceEncodedSample = null;
        public event RawAudioSampleDelegate? OnAudioSourceRawSample = null;
        public event SourceErrorDelegate? OnAudioSourceError = null;
        public event Action<EncodedAudioFrame>? OnAudioSourceEncodedFrameReady = null;

#endregion EVENT

        public SDL3AudioSource(string audioInDeviceName, IAudioEncoder audioEncoder, int frameSize = 1920)
        {
            if (audioEncoder == null)
                throw new ApplicationException("Audio encoder provided is null");
            var device = SDL3Helper.GetAudioRecordingDevice(audioInDeviceName);
            if (!device.HasValue)
                throw new ApplicationException($"Could not get audio recording device {audioInDeviceName}");

            _audioDevice = device.Value;

            _audioFormatManager = new MediaFormatManager<AudioFormat>(audioEncoder.SupportedFormats);
            _audioEncoder = audioEncoder;

            this.frameSize = frameSize;

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.WorkerSupportsCancellation = true;
        }

        ~SDL3AudioSource()
        {
            Dispose(false);
        }

        private void RaiseAudioSourceError(string err)
        {
            CloseAudio();
            OnAudioSourceError?.Invoke(err);
        }

        private unsafe void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            var pool = ArrayPool<byte>.Shared;

            while (!backgroundWorker.CancellationPending && !_disposed)
            {
                // If we're using the SDL callback to unqueue, the background worker should process queued buffers.
                if (_useStreamCallbackReading)
                {
                    // Wait for a buffer to be available or timeout to re-check cancellation
                    _queueSemaphore.Wait(50);

                    while (_callbackQueue.TryDequeue(out var seg))
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
                            // Return buffer to pool
                            pool.Return(seg.Buffer);
                        }
                    }

                    continue;
                }

                int size = 0;
                int bufferSize = 0;
                do
                {
                    // Check if device is not stopped
                    IntPtr stream;
                    lock (_stateLock)
                    {
                        stream = _audioStream;
                    }

                    if (stream == IntPtr.Zero)
                    {
                        RaiseAudioSourceError($"SDLAudioSource [{_audioDevice.name}] stopped.");
                        return;
                    }

                    size = SDL3Helper.GetAudioStreamQueued(stream);
                    if (size >= frameSize * 2) // Need to use double size since we get byte[] and not short[] from SDL
                    {
                        if (frameSize != 0)
                            bufferSize = frameSize * 2;
                        else
                            bufferSize = size;

                        byte[] buf = pool.Rent(bufferSize);

                        try
                        {
                            fixed (byte* ptr = &buf[0])
                            {
                                // Read from SDL audio stream into our buffer
                                SDL3Helper.GetAudioStreamData(stream, (IntPtr)ptr, bufferSize);

                                int shortCount = bufferSize / sizeof(short);
                                short[] pcm = new short[shortCount];
                                Buffer.BlockCopy(buf, 0, pcm, 0, shortCount * sizeof(short));

                                OnAudioSourceRawSample?.Invoke(audioSamplingRates, (uint)pcm.Length, pcm);

                                if (OnAudioSourceEncodedSample != null)
                                {
                                    var encodedSample = _audioEncoder.EncodeAudio(pcm, _audioFormatManager.SelectedFormat);
                                    if (encodedSample.Length > 0)
                                        OnAudioSourceEncodedSample?.Invoke((uint)( pcm.Length * _audioFormatManager.SelectedFormat.RtpClockRate / _audioFormatManager.SelectedFormat.ClockRate), encodedSample);
                                }
                            }
                        }
                        finally
                        {
                            pool.Return(buf);
                        }

                        size -= bufferSize;
                    }
                } while (size >= frameSize && !_disposed);

                SDL3Helper.Delay(16);
            }
        }

        private void InitRecordingDevice()
        {
            try
            {
                // Stop previous recording device
                CloseAudio();

                // Init recording device.
                AudioFormat audioFormat = _audioFormatManager.SelectedFormat;
                audioSamplingRates = audioFormat.ClockRate == AudioFormat.DEFAULT_CLOCK_RATE * 2 
                    ? AudioSamplingRatesEnum.Rate16KHz : AudioSamplingRatesEnum.Rate8KHz;

                var audioSpec = SDL3Helper.GetAudioSpec(audioFormat.ClockRate);
                //int bytesPerSecond = SDL3Helper.GetBytesPerSecond(audioSpec);

                var stream = SDL3Helper.OpenAudioDeviceStream(_audioDevice.id, ref audioSpec, UnqueueStreamCallback);

                lock (_stateLock)
                {
                    _audioStream = stream;
                }

                // when we open with UnqueueStreamCallback, indicate we will use callback reading
                _useStreamCallbackReading = stream != IntPtr.Zero;

                if (stream != IntPtr.Zero)
                    log.LogDebug("[InitRecordingDevice] Audio source - Id:[{AudioDeviceId}] - DeviceName:[{AudioDeviceName}]", _audioDevice.id, _audioDevice.name);
                else
                {
                    log.LogError("[InitRecordingDevice] SDLAudioSource failed to initialise device. No audio device found with [{AudioDeviceId} ] and [ {AudioDeviceName}]", _audioDevice.id, _audioDevice.name);
                    RaiseAudioSourceError($"SDLAudioSource failed to initialise device. No audio device found with [{_audioDevice.id} ] and [ {_audioDevice.name}]");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "InitRecordingDevice] SDLAudioSource failed to initialise device [{AudioDeviceId} ] - [ {AudioDeviceName}].", _audioDevice.id, _audioDevice.name);
                RaiseAudioSourceError($"SDLAudioSource failed to initialise device [{_audioDevice.name}] and [{_audioDevice.id}] - Exception:[{ex.Message}]");
            }
        }

        private unsafe void UnqueueStreamCallback(IntPtr userdata, IntPtr stream, int additionalAmount, int totalAmount)
        {
            // This callback is invoked by SDL when new data is available on the recording stream.
            // We should read 'additionalAmount' bytes (or however many are available) and enqueue them for processing.
            if (stream == IntPtr.Zero || additionalAmount <= 0)
            {
                return;
            }

            var pool = ArrayPool<byte>.Shared;
            int toRead = additionalAmount;
            byte[] rented = pool.Rent(toRead);

            int read = 0;
            try
            {
                fixed (byte* ptr = &rented[0])
                {
                    read = SDL3Helper.GetAudioStreamData(stream, (IntPtr)ptr, toRead);
                }

                if (read <= 0)
                {
                    pool.Return(rented);
                    return;
                }

                // Enqueue the rented buffer along with the number of valid bytes. The background worker will return it to the pool.
                _callbackQueue.Enqueue((rented, read));
                _queueSemaphore.Release();
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

        public Task PauseAudio()
        {
            IntPtr stream = IntPtr.Zero;
            bool doPause = false;

            lock (_stateLock)
            {
                if (_isStarted && !_isPaused)
                {
                    _isPaused = true;
                    doPause = true;
                    stream = _audioStream;
                }
            }

            if (doPause && stream != IntPtr.Zero)
            {
                if (backgroundWorker.IsBusy)
                    backgroundWorker.CancelAsync();

                SDL3Helper.PauseAudioStreamDevice(stream);
                log.LogDebug("[PauseAudio] Audio source - Id:[{AudioInDeviceId}]", _audioDevice.id);
            }

            return Task.CompletedTask;
        }

        public Task ResumeAudio()
        {
            IntPtr stream = IntPtr.Zero;
            bool doResume = false;

            lock (_stateLock)
            {
                if (_isStarted && _isPaused)
                {
                    _isPaused = false;
                    doResume = true;
                    stream = _audioStream;
                }
            }

            if (doResume && stream != IntPtr.Zero)
            {
                if (!backgroundWorker.IsBusy)
                    backgroundWorker.RunWorkerAsync();

                SDL3Helper.ResumeAudioStreamDevice(stream);
                log.LogDebug("[ResumeAudio] Audio source - Id:[{AudioInDeviceId}]", _audioDevice.id);
            }

            return Task.CompletedTask;
        }

        public bool IsAudioSourcePaused()
        {
            lock (_stateLock)
            {
                return _isPaused;
            }
        }

        public Task StartAudio()
        {
            if (!_isStarted && _audioStream != IntPtr.Zero)
            {
                _isStarted = true;
                _isPaused = true;

                ResumeAudio().Wait();
            }

            return Task.CompletedTask;
        }

        public Task CloseAudio()
        {
            if (_isStarted)
            {
                PauseAudio().Wait();
                if (_audioStream != IntPtr.Zero)
                {
                    SDL3Helper.DestroyAudioStream(_audioStream);
                    log.LogDebug("[CloseAudio] Audio source - Id:[{AudioInDeviceId}] - Name:[{AudioInDeviceName}]", 
                        _audioDevice.id, _audioDevice.name);
                }
            }

            lock (_stateLock)
            {
                _isStarted = false;
                _audioStream = IntPtr.Zero;
                _useStreamCallbackReading = false;
            }

            return Task.CompletedTask;
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
                    // stop background worker gracefully
                    if (backgroundWorker.IsBusy)
                    {
                        backgroundWorker.CancelAsync();
                    }

                    CloseAudio().Wait();

                    // Dispose semaphore
                    _queueSemaphore?.Dispose();

                    // Return any remaining buffers in queue
                    var pool = ArrayPool<byte>.Shared;
                    while (_callbackQueue.TryDequeue(out var seg))
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
                // finalizer: best-effort unmanaged cleanup without touching managed state
                IntPtr streamToDestroy = IntPtr.Zero;
                lock (_stateLock)
                {
                    streamToDestroy = _audioStream;
                    _audioStream = IntPtr.Zero;
                }

                if (streamToDestroy != IntPtr.Zero)
                {
                    try
                    {
                        SDL3Helper.DestroyAudioStream(streamToDestroy);
                    }
                    catch
                    {
                        // swallow exceptions in finalizer
                    }
                }
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
            log.LogDebug("Setting audio source format to {AudioFormatFormatId}:{AudioFormatFormatName} {AudioFormatClockRate}.", 
                audioFormat.FormatID, audioFormat.FormatName, audioFormat.ClockRate);
            _audioFormatManager.SetSelectedFormat(audioFormat);

            InitRecordingDevice();
            StartAudio();
        }

        public void RestrictFormats(Func<AudioFormat, bool> filter)
        {
            _audioFormatManager.RestrictFormats(filter);
        }

        public void ExternalAudioSourceRawSample(AudioSamplingRatesEnum samplingRate, uint durationMilliseconds, short[] sample) 
            => throw new NotImplementedException();

        public bool HasEncodedAudioSubscribers() => OnAudioSourceEncodedSample != null;
    }
}
