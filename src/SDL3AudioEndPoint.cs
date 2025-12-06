/**
 * @file SDLAudioEndpoint.cs
 * @brief Example of an AudioEndpoint using SDL3 to playback audio stream
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Buffers;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SIPSorceryMedia.SDL3
{
    public class SDL3AudioEndPoint : IAudioSink, IDisposable
    {
        private readonly ILogger log = SIPSorcery.LogFactory.CreateLogger<SDL3AudioEndPoint>();

        private readonly IAudioEncoder _audioEncoder;
        private readonly MediaFormatManager<AudioFormat> _audioFormatManager;

        private readonly (uint id, string name) _audioDevice;
        private SDL3AudioStreamSafeHandle? _audioStream = null;

        private readonly object _stateLock = new object();

        protected bool _isStarted = false;
        protected bool _isPaused = true;
        protected bool _isClosed = true;

        private bool _disposed = false;

        public event SourceErrorDelegate ? OnAudioSinkError = null;

        // Playback queue populated by producers; worker will dequeue and call SDL to put data into stream
        private readonly ConcurrentQueue<(byte[] Buffer, int Length)> _playbackQueue = new ConcurrentQueue<(byte[] Buffer, int Length)>();
        private readonly SemaphoreSlim _playbackSemaphore = new SemaphoreSlim(0);
        private readonly BackgroundWorker _playbackWorker;

        /// <summary>
        /// Creates a new basic RTP session that captures and renders audio to/from the system devices.
        /// </summary>
        /// <param name="audioEncoder">An audio encoder that can be used to encode and decode
        /// specific audio codecs.</param>
        /// <param name="audioOutDeviceName">Name of the requested audio playback to use.</param>
        public SDL3AudioEndPoint(string audioOutDeviceName, IAudioEncoder audioEncoder)
        {
            _audioFormatManager = new MediaFormatManager<AudioFormat>(audioEncoder.SupportedFormats);
            _audioEncoder = audioEncoder;

            var device = SDL3Helper.GetAudioPlaybackDevice(audioOutDeviceName);
            if (!device.HasValue)
            {
                throw new ApplicationException($"Could not get audio recording device named {audioOutDeviceName}");
            }
            _audioDevice = device.Value;

            _playbackWorker = new BackgroundWorker();
            _playbackWorker.DoWork += PlaybackWorker_DoWork;
            _playbackWorker.WorkerSupportsCancellation = true;
        }

        ~SDL3AudioEndPoint()
        {
            Dispose(false);
        }

        private void RaiseAudioSinkError(string err)
        {
            _ = CloseAudioSinkAsync();
            OnAudioSinkError?.Invoke(err);
        }

        /// <summary>
        /// Called when an encoded audio frame has been received. The frame will be decoded
        /// and queued for playback on the endpoint.
        /// </summary>
        /// <param name="encodedMediaFrame">The encoded audio frame to decode and play.</param>
        public void GotEncodedMediaFrame(EncodedAudioFrame encodedMediaFrame)
        {
            var audioFormat = encodedMediaFrame.AudioFormat;

            if (!audioFormat.IsEmpty())
            {
                // Decode sample
                var pcmSample = _audioEncoder.DecodeAudio(encodedMediaFrame.EncodedAudio, audioFormat);
                // Convert short[] to byte[] efficiently
                var pcmBytes = new byte[pcmSample.Length * sizeof(short)];
                Buffer.BlockCopy(pcmSample, 0, pcmBytes, 0, pcmBytes.Length);
                PutAudioSample(pcmBytes);
            }
        }

        /// <summary>
        /// Restrict the available audio formats for this endpoint using the provided filter.
        /// </summary>
        /// <param name="filter">A predicate used to select allowed audio formats.</param>
        public void RestrictFormats(Func<AudioFormat, bool> filter) => _audioFormatManager.RestrictFormats(filter);

        /// <summary>
        /// Sets the audio format to use for playback and initializes the playback device.
        /// </summary>
        /// <param name="audioFormat">The desired audio format.</param>
        public void SetAudioSinkFormat(AudioFormat audioFormat)
        {
            _audioFormatManager.SetSelectedFormat(audioFormat);
            InitPlaybackDevice();
            StartAudioSink();
        }

        /// <summary>
        /// Gets the list of available audio sink formats supported by the encoder.
        /// </summary>
        /// <returns>A list of supported audio formats.</returns>
        public List<AudioFormat> GetAudioSinkFormats() => _audioFormatManager.GetSourceFormats();

        /// <summary>
        /// Returns this endpoint wrapped in a <see cref="MediaEndPoints"/> instance.
        /// </summary>
        /// <returns>A <see cref="MediaEndPoints"/> containing this audio sink.</returns>
        public MediaEndPoints ToMediaEndPoints()
        {
            return new MediaEndPoints
            {
                AudioSink = this,
            };
        }

        private void InitPlaybackDevice()
        {
            try
            {
                // Stop previous playback device (async close, don't block caller)
                _ = CloseAudioSinkAsync();

                // Init Playback device.
                AudioFormat audioFormat = _audioFormatManager.SelectedFormat;
                var audioSpec = SDL3Helper.GetAudioSpec(audioFormat.ClockRate, 1);

                var streamPtr = SDL3Helper.OpenAudioDeviceStream(_audioDevice.id, ref audioSpec, FeedStreamCallback);

                SDL3AudioStreamSafeHandle? newHandle = null;
                if (streamPtr != IntPtr.Zero)
                {
                    newHandle = new SDL3AudioStreamSafeHandle(streamPtr);
                }

                lock (_stateLock)
                {
                    // dispose previous handle if present
                    _audioStream?.Dispose();
                    _audioStream = newHandle;
                }

                if(newHandle != null && !newHandle.IsInvalid)
                    log.LogDebug("[InitPlaybackDevice] Id:[{AudioDeviceId}] - DeviceName:[{AudioDeviceName}]", _audioDevice.id, _audioDevice.name);
                else
                {
                    log.LogError("[InitPlaybackDevice] SDLAudioEndPoint failed to initialise device. No audio device found - Id:[{AudioDeviceId} ] - DeviceName:[ {AudioDeviceName}]", _audioDevice.id, _audioDevice.name);
                    RaiseAudioSinkError($"SDLAudioEndPoint failed to initialise device. No audio device found - Id:[{_audioDevice.id} ] - DeviceName:[ {_audioDevice.name}]");
                }
            }
            catch (Exception e)
            {
                log.LogError(e, "[InitPlaybackDevice] SDLAudioEndPoint failed to initialise device - Id:[{AudioDeviceId}   ] - DeviceName:[   {AudioDeviceName}]", _audioDevice.id, _audioDevice.name);
                RaiseAudioSinkError($"SDLAudioEndPoint failed to initialise device. No audio device found - Id:[{_audioDevice.id}] - DeviceName:[{_audioDevice.name}] - Exception:[{e.Message}]" );
            }
        }

        private void FeedStreamCallback(IntPtr userdata, IntPtr stream, int additionalAmount, int totalAmount)
        {
            try
            {
                SDL3AudioStreamSafeHandle? currentHandle;
                lock (_stateLock)
                {
                    currentHandle = _audioStream;
                }

                if (currentHandle == null || currentHandle.IsInvalid)
                {
                    return;
                }

                IntPtr currentStream = currentHandle.DangerousGetHandle();

                // Log debug info for diagnostics. Do not perform heavy work here.
                int queued = SDL3Helper.GetAudioStreamQueued(currentStream);
                log.LogDebug("[FeedStreamCallback] DeviceId:{DeviceId} additionalAmount:{Additional} totalAmount:{Total} queued:{Queued}", _audioDevice.id, additionalAmount, totalAmount, queued);

                // Signal worker that space is available
                if (additionalAmount > 0)
                {
                    try { _playbackSemaphore.Release(); } catch { }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "[FeedStreamCallback] Exception in audio stream callback");
            }
        }

        private void PlaybackWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            var pool = ArrayPool<byte>.Shared;

            while (!_playbackWorker.CancellationPending && !_disposed)
            {
                // Wait for available buffer or timeout
                _playbackSemaphore.Wait(50);

                while (_playbackQueue.TryDequeue(out var seg))
                {
                    SDL3AudioStreamSafeHandle? currentHandle;
                    lock (_stateLock)
                    {
                        currentHandle = _audioStream;
                    }

                    if (currentHandle == null || currentHandle.IsInvalid)
                    {
                        // return buffer
                        try { pool.Return(seg.Buffer); } catch { }
                        continue;
                    }

                    IntPtr streamPtr = currentHandle.DangerousGetHandle();

                    try
                    {
                        // Put bytes into SDL stream
                        SDL3Helper.PutAudioToStream(streamPtr, ref seg.Buffer, seg.Length);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "PlaybackWorker: error putting audio to stream");
                    }
                    finally
                    {
                        // Return buffer to pool
                        try { pool.Return(seg.Buffer); } catch { }
                    }
                }
            }
        }

        /// <summary>
        /// Queues a raw PCM audio sample for playback.
        /// </summary>
        /// <param name="pcmSample">A buffer containing raw PCM audio bytes.</param>
        public void PutAudioSample(byte[] pcmSample)
        {
            if (pcmSample == null || pcmSample.Length == 0) return;

            // Rent a buffer and copy data to avoid producers holding onto arrays and to reuse buffers
            var pool = ArrayPool<byte>.Shared;
            var buf = pool.Rent(pcmSample.Length);
            Buffer.BlockCopy(pcmSample, 0, buf, 0, pcmSample.Length);

            // Enqueue for playback worker
            _playbackQueue.Enqueue((buf, pcmSample.Length));

            // Signal worker
            try { _playbackSemaphore.Release(); } catch { }
        }

        [Obsolete("Use GotEncodeMediaFrame instead.")]
        /// <summary>
        /// Obsolete: Handle a received RTP audio payload. This method decodes the RTP
        /// payload using the selected audio format and queues it for playback.
        /// </summary>
        /// <remarks>Use <see cref="GotEncodedMediaFrame(EncodedAudioFrame)"/> instead.</remarks>
        public void GotAudioRtp(IPEndPoint remoteEndPoint, uint ssrc, uint seqnum, uint timestamp, int payloadID, bool marker, byte[] payload)
        {
            SDL3AudioStreamSafeHandle? currentHandle;
            lock (_stateLock)
            {
                currentHandle = _audioStream;
            }

            if (currentHandle == null || currentHandle.IsInvalid) { return; }

            // Decode sample
            var pcmSample = _audioEncoder.DecodeAudio(payload, _audioFormatManager.SelectedFormat);
            var pcmBytes = new byte[pcmSample.Length * sizeof(short)];
            Buffer.BlockCopy(pcmSample, 0, pcmBytes, 0, pcmBytes.Length);
            PutAudioSample(pcmBytes);
        }

        /// <summary>
        /// Pause audio sink operation.
        /// </summary>
        /// <remarks>
        /// This synchronous API returns a completed Task. Prefer using the async counterpart
        /// `PauseAudioSinkAsync` when calling from asynchronous code to avoid blocking or
        /// surprising synchronous behavior.
        /// </remarks>
        public Task PauseAudioSink()
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
                // stop worker
                if (_playbackWorker.IsBusy)
                    _playbackWorker.CancelAsync();

                SDL3Helper.PauseAudioStreamDevice(currentHandle.DangerousGetHandle());
                log.LogDebug("[PauseAudioSink] Audio output - Id:[{AudioOutDeviceId}]", _audioDevice.id);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Resume audio sink operation.
        /// </summary>
        /// <remarks>
        /// This synchronous API returns a completed Task. Prefer using the async counterpart
        /// `ResumeAudioSinkAsync` when calling from asynchronous code to avoid blocking or
        /// surprising synchronous behavior.
        /// </remarks>
        public Task ResumeAudioSink()
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

            if (doResume && currentHandle != null && !currentHandle.IsInvalid)
            {
                if (!_playbackWorker.IsBusy)
                    _playbackWorker.RunWorkerAsync();

                SDL3Helper.ResumeAudioStreamDevice(currentHandle.DangerousGetHandle());
                log.LogDebug("[ResumeAudioSink] Audio output - Id:[{AudioOutDeviceId}]", _audioDevice.id);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Starts the audio sink. This synchronous wrapper forwards to <see cref="StartAudioSinkAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the start operation.</returns>
        public Task StartAudioSink()
        {
            return StartAudioSinkAsync();
        }

        /// <summary>
        /// Asynchronously starts the audio sink and resumes playback if required.
        /// </summary>
        /// <returns>A <see cref="Task"/> that completes when the operation has been initiated.</returns>
        public async Task StartAudioSinkAsync()
        {
            bool needResume = false;
            lock (_stateLock)
            {
                if(!_isStarted && _audioStream != null && !_audioStream.IsInvalid)
                {
                    _isStarted = true;
                    _isClosed = false;
                    _isPaused = true;
                    needResume = true;
                }
            }

            if (needResume)
            {
                // call resume outside lock
                await ResumeAudioSink().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Closes the audio sink. This synchronous wrapper forwards to <see cref="CloseAudioSinkAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the close operation.</returns>
        public Task CloseAudioSink()
        {
            return CloseAudioSinkAsync();
        }

        /// <summary>
        /// Asynchronously closes the audio sink, disposes the underlying audio stream and clears queued buffers.
        /// </summary>
        /// <returns>A <see cref="Task"/> that completes when the sink has been closed.</returns>
        public async Task CloseAudioSinkAsync()
        {
            // Ensure audio paused first
            await PauseAudioSink().ConfigureAwait(false);

            SDL3AudioStreamSafeHandle? toDispose = null;

            lock (_stateLock)
            {
                toDispose = _audioStream;
                _audioStream = null;
                _isClosed = true;
                _isStarted = false;
                _isPaused = true;
            }

            if (toDispose != null && !toDispose.IsInvalid)
            {
                try
                {
                    toDispose.Dispose();
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error destroying audio stream");
                }

                log.LogDebug("[CloseAudioSink] Audio output - Id:[{AudioOutDeviceId}]", _audioDevice.id);
            }

            // clear any queued buffers
            var pool = ArrayPool<byte>.Shared;
            while (_playbackQueue.TryDequeue(out var seg))
            {
                try { pool.Return(seg.Buffer); } catch { }
            }

            return;
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
                    // Fire-and-forget close to avoid blocking Dispose
                    _ = CloseAudioSinkAsync();

                    // dispose worker and semaphore
                    if (_playbackWorker.IsBusy)
                    {
                        _playbackWorker.CancelAsync();
                    }
                    _playbackSemaphore?.Dispose();
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error during Dispose CloseAudioSink");
                }
            }
            else
            {
                // finalizer: nothing to do, SafeHandle will free the native resource
            }
        }

        /// <summary>
        /// Disposes the endpoint and releases managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

