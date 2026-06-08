/**
 * @file SDLAudioEndpoint.cs
 * @brief AudioEndpoint using SDL3 to playback an audio stream
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
using SIPSorceryMedia.Abstractions;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Net;
using System.Buffers;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SIPSorceryMedia.SDL3
{
    /// <summary>
    /// Performance statistics for the audio sink.
    /// </summary>
    public struct AudioSinkStats
    {
        public long UnderrunCount { get; set; }
        public long DroppedFrames { get; set; }
        public int QueueDepth { get; set; }
        public bool IsActive { get; set; }
    }

    public class SDL3AudioEndPoint : IAudioSink, IDisposable
    {
        private const int MAX_AUDIO_RENT = 1024 * 1024; // 1 MB
        // Max ~200ms of audio buffered before dropping oldest
        private const int MAX_QUEUE_CAPACITY = 10;

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

        public event SourceErrorDelegate? OnAudioSinkError = null;

        // Bounded channel replaces the legacy BackgroundWorker + ConcurrentQueue + SemaphoreSlim trio.
        // DropOldest keeps the sink caught up under load without unbounded queue growth.
        private readonly Channel<(byte[] Buffer, int Length)> _playbackChannel;
        private CancellationTokenSource? _playCts = null;
        private Task? _playbackTask = null;

        // Stats for monitoring and diagnostics
        private long _underrunCount = 0;
        private long _droppedFrames = 0;
        private int _channelCount = 0; // approximate depth for stats

        /// <summary>
        /// Creates a new audio sink that plays decoded RTP audio through an SDL3 output device.
        /// </summary>
        public SDL3AudioEndPoint(string audioOutDeviceName, IAudioEncoder audioEncoder)
        {
            _audioFormatManager = new MediaFormatManager<AudioFormat>(audioEncoder.SupportedFormats);
            _audioEncoder = audioEncoder;

            var device = SDL3Helper.GetAudioPlaybackDevice(audioOutDeviceName);
            if (!device.HasValue)
                throw new ApplicationException($"Could not get audio playback device named '{audioOutDeviceName}'");
            _audioDevice = device.Value;

            _playbackChannel = Channel.CreateBounded<(byte[] Buffer, int Length)>(
                new BoundedChannelOptions(MAX_QUEUE_CAPACITY)
                {
                    FullMode = BoundedChannelFullMode.DropOldest,
                    SingleReader = true,
                    SingleWriter = false
                });
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
        /// Called when an encoded audio frame has been received. Decodes and queues for playback.
        /// </summary>
        public void GotEncodedMediaFrame(EncodedAudioFrame encodedMediaFrame)
        {
            var audioFormat = encodedMediaFrame.AudioFormat;
            if (!audioFormat.IsEmpty())
            {
                var pcmSample = _audioEncoder.DecodeAudio(encodedMediaFrame.EncodedAudio, audioFormat);
                var pcmBytes = new byte[pcmSample.Length * sizeof(short)];
                Buffer.BlockCopy(pcmSample, 0, pcmBytes, 0, pcmBytes.Length);
                PutAudioSample(pcmBytes);
            }
        }

        public void RestrictFormats(Func<AudioFormat, bool> filter) => _audioFormatManager.RestrictFormats(filter);

        public void SetAudioSinkFormat(AudioFormat audioFormat)
        {
            _audioFormatManager.SetSelectedFormat(audioFormat);
            InitPlaybackDevice();
            StartAudioSink();
        }

        public List<AudioFormat> GetAudioSinkFormats() => _audioFormatManager.GetSourceFormats();

        public MediaEndPoints ToMediaEndPoints()
        {
            return new MediaEndPoints { AudioSink = this };
        }

        /// <summary>
        /// Playback volume as a gain multiplier. 1.0 = unity gain, 0.0 = silent.
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

        private void InitPlaybackDevice()
        {
            try
            {
                // Close any existing stream before opening a new one.
                // Grab and null the old handle under lock so Close can detect it.
                SDL3AudioStreamSafeHandle? prev;
                lock (_stateLock)
                {
                    prev = _audioStream;
                    _audioStream = null;
                    _isStarted = false;
                    _isPaused = true;
                    _isClosed = true;
                }
                if (prev != null)
                {
                    try { SDL3Helper.DestroyAudioStream(prev); } catch (Exception ex) { log.LogError(ex, "Error closing previous audio stream"); }
                }

                AudioFormat audioFormat = _audioFormatManager.SelectedFormat;
                var audioSpec = SDL3Helper.GetAudioSpec(audioFormat.ClockRate, 1);

                // Open without a callback — we push data from the worker task.
                var newHandle = SDL3Helper.OpenAudioDeviceStreamHandle(_audioDevice.id, ref audioSpec, null);

                lock (_stateLock)
                {
                    _audioStream = newHandle;
                }

                if (newHandle != null && !newHandle.IsInvalid)
                    log.LogDebug("[InitPlaybackDevice] Id:[{DeviceId}] Name:[{DeviceName}]", _audioDevice.id, _audioDevice.name);
                else
                {
                    log.LogError("[InitPlaybackDevice] Failed to open device Id:[{DeviceId}] Name:[{DeviceName}]", _audioDevice.id, _audioDevice.name);
                    RaiseAudioSinkError($"SDL3AudioEndPoint failed to initialise device Id:[{_audioDevice.id}] Name:[{_audioDevice.name}]");
                }
            }
            catch (Exception e)
            {
                log.LogError(e, "[InitPlaybackDevice] Exception opening device Id:[{DeviceId}] Name:[{DeviceName}]", _audioDevice.id, _audioDevice.name);
                RaiseAudioSinkError($"SDL3AudioEndPoint failed to initialise device Id:[{_audioDevice.id}] Name:[{_audioDevice.name}]: {e.Message}");
            }
        }

        private async Task PlaybackWorkerAsync(CancellationToken ct)
        {
            var pool = ArrayPool<byte>.Shared;
            try
            {
                await foreach (var (buf, len) in _playbackChannel.Reader.ReadAllAsync(ct).ConfigureAwait(false))
                {
                    Interlocked.Decrement(ref _channelCount);

                    if (buf == null || len <= 0 || len > buf.Length)
                    {
                        if (buf != null) try { pool.Return(buf); } catch (Exception) { }
                        continue;
                    }

                    SDL3AudioStreamSafeHandle? handle;
                    lock (_stateLock) { handle = _audioStream; }

                    if (handle == null || handle.IsInvalid)
                    {
                        try { pool.Return(buf); } catch (Exception) { }
                        continue;
                    }

                    try
                    {
                        // Check SDL queue depth to detect underruns
                        int queued = SDL3Helper.GetAudioStreamQueued(handle);
                        if (queued == 0)
                            Interlocked.Increment(ref _underrunCount);

                        SDL3Helper.PutAudioToStream(handle, buf, len);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "PlaybackWorker: error putting audio to stream");
                    }
                    finally
                    {
                        try { pool.Return(buf); } catch (Exception) { }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                log.LogError(ex, "PlaybackWorker: unexpected error");
            }
        }

        /// <summary>
        /// Queues raw PCM bytes for playback. Oldest frames are dropped when the queue is full.
        /// </summary>
        public void PutAudioSample(byte[] pcmSample)
        {
            if (pcmSample == null || pcmSample.Length == 0) return;

            if (pcmSample.Length > MAX_AUDIO_RENT)
            {
                log.LogWarning("PutAudioSample: sample {Size} bytes exceeds {Max}, dropping", pcmSample.Length, MAX_AUDIO_RENT);
                return;
            }

            var pool = ArrayPool<byte>.Shared;
            var buf = pool.Rent(pcmSample.Length);
            Buffer.BlockCopy(pcmSample, 0, buf, 0, pcmSample.Length);

            // Track before write so the counter is accurate when DropOldest fires
            int depth = Interlocked.Increment(ref _channelCount);
            if (depth > MAX_QUEUE_CAPACITY)
                Interlocked.Increment(ref _droppedFrames);

            if (!_playbackChannel.Writer.TryWrite((buf, pcmSample.Length)))
            {
                // Should not happen with DropOldest, but handle defensively
                Interlocked.Decrement(ref _channelCount);
                try { pool.Return(buf); } catch (Exception) { }
            }
        }

        [Obsolete("Use GotEncodedMediaFrame instead.")]
        public void GotAudioRtp(IPEndPoint remoteEndPoint, uint ssrc, uint seqnum, uint timestamp, int payloadID, bool marker, byte[] payload)
        {
            SDL3AudioStreamSafeHandle? currentHandle;
            lock (_stateLock) { currentHandle = _audioStream; }
            if (currentHandle == null || currentHandle.IsInvalid) return;

            var pcmSample = _audioEncoder.DecodeAudio(payload, _audioFormatManager.SelectedFormat);
            var pcmBytes = new byte[pcmSample.Length * sizeof(short)];
            Buffer.BlockCopy(pcmSample, 0, pcmBytes, 0, pcmBytes.Length);
            PutAudioSample(pcmBytes);
        }

        public AudioSinkStats GetStats()
        {
            bool isActive;
            lock (_stateLock) { isActive = _isStarted && !_isPaused; }
            return new AudioSinkStats
            {
                UnderrunCount = Interlocked.Read(ref _underrunCount),
                DroppedFrames = Interlocked.Read(ref _droppedFrames),
                QueueDepth = Interlocked.CompareExchange(ref _channelCount, 0, 0),
                IsActive = isActive
            };
        }

        public void ResetStats()
        {
            Interlocked.Exchange(ref _underrunCount, 0);
            Interlocked.Exchange(ref _droppedFrames, 0);
        }

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

            if (doPause)
            {
                // Cancel the worker task
                try { _playCts?.Cancel(); } catch (Exception) { }

                if (currentHandle != null && !currentHandle.IsInvalid)
                    SDL3Helper.PauseAudioStreamDevice(currentHandle);

                log.LogDebug("[PauseAudioSink] Id:[{DeviceId}]", _audioDevice.id);
            }

            return Task.CompletedTask;
        }

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
                StartPlaybackTask();
                SDL3Helper.ResumeAudioStreamDevice(currentHandle);
                log.LogDebug("[ResumeAudioSink] Id:[{DeviceId}]", _audioDevice.id);
            }

            return Task.CompletedTask;
        }

        private void StartPlaybackTask()
        {
            // Cancel any running task before starting a new one
            var oldCts = _playCts;
            _playCts = new CancellationTokenSource();
            try { oldCts?.Cancel(); } catch (Exception) { }
            oldCts?.Dispose();

            _playbackTask = Task.Run(() => PlaybackWorkerAsync(_playCts.Token));
        }

        public Task StartAudioSink() => StartAudioSinkAsync();

        public async Task StartAudioSinkAsync()
        {
            bool needResume = false;
            lock (_stateLock)
            {
                if (!_isStarted && _audioStream != null && !_audioStream.IsInvalid)
                {
                    _isStarted = true;
                    _isClosed = false;
                    _isPaused = true;
                    needResume = true;
                }
            }

            if (needResume)
                await ResumeAudioSink().ConfigureAwait(false);
        }

        public Task CloseAudioSink() => CloseAudioSinkAsync();

        public async Task CloseAudioSinkAsync()
        {
            await PauseAudioSink().ConfigureAwait(false);
            CloseSync();
        }

        private void CloseSync()
        {
            SDL3AudioStreamSafeHandle? toDispose;
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
                    SDL3Helper.DestroyAudioStream(toDispose);
                    log.LogDebug("[CloseAudioSink] Id:[{DeviceId}]", _audioDevice.id);
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error destroying audio stream");
                }
            }

            // Return any buffers still queued
            var pool = ArrayPool<byte>.Shared;
            while (_playbackChannel.Reader.TryRead(out var seg))
            {
                Interlocked.Decrement(ref _channelCount);
                try { pool.Return(seg.Buffer); } catch (Exception) { }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                // Cancel worker first so it stops reading the channel
                try { _playCts?.Cancel(); } catch (Exception) { }

                // Synchronous close: null the stream, return queued buffers
                CloseSync();

                // Complete the channel so ReadAllAsync terminates
                _playbackChannel.Writer.TryComplete();

                _playCts?.Dispose();
                _playCts = null;
            }
            // Finalizer path: SafeHandle will free the native stream handle
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
