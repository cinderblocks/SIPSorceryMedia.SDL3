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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SIPSorceryMedia.SDL3
{
    public class SDL3AudioSource : IAudioSource
    {
        private static readonly ILogger log = SIPSorcery.LogFactory.CreateLogger<SDL3AudioSource>();

        private readonly (uint id, string name) _audioDevice;
        private IntPtr _audioStream = IntPtr.Zero;

        private readonly IAudioEncoder _audioEncoder;
        private readonly MediaFormatManager<AudioFormat> _audioFormatManager;

        private bool _isStarted = false;
        private bool _isPaused = true;

        private readonly int frameSize = 0;

        private readonly BackgroundWorker backgroundWorker;

        private AudioSamplingRatesEnum audioSamplingRates;

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
            var device = SDL3Helper.GetAudioPlaybackDevice(audioInDeviceName);
            if (!device.HasValue)
                throw new ApplicationException($"Could not get audio device {audioInDeviceName}");

            _audioDevice = device.Value;

            _audioFormatManager = new MediaFormatManager<AudioFormat>(audioEncoder.SupportedFormats);
            _audioEncoder = audioEncoder;

            this.frameSize = frameSize;

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.WorkerSupportsCancellation = true;
        }

        private void RaiseAudioSourceError(string err)
        {
            CloseAudio();
            OnAudioSourceError?.Invoke(err);
        }

        private unsafe void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            while (!backgroundWorker.CancellationPending)
            {
                int size = 0;
                int bufferSize = 0;
                do
                {
                    // Check if device is not stopped
                    if (_audioStream == IntPtr.Zero)
                    {
                        RaiseAudioSourceError($"SDLAudioSource [{_audioDevice.name}] stopped.");
                        return;
                    }

                    size = SDL3Helper.GetAudioStreamQueued(_audioStream);
                    if (size >= frameSize * 2) // Need to use double size since we get byte[] and not short[] from SDL
                    {
                        if (frameSize != 0)
                            bufferSize = frameSize * 2;
                        else
                            bufferSize = size;

                        byte[] buf = new byte[bufferSize];

                        fixed (byte* ptr = &buf[0])
                        {
                            SDL3Helper.PutAudio(_audioStream, (IntPtr)ptr, bufferSize);

                            short[] pcm = buf.Take(bufferSize * 2).Where((x, i) => i % 2 == 0).Select((y, i) => BitConverter.ToInt16(buf, i * 2)).ToArray();
                            OnAudioSourceRawSample?.Invoke(audioSamplingRates, (uint)pcm.Length, pcm);

                            if (OnAudioSourceEncodedSample != null)
                            {
                                var encodedSample = _audioEncoder.EncodeAudio(pcm, _audioFormatManager.SelectedFormat);
                                if (encodedSample.Length > 0)
                                    OnAudioSourceEncodedSample?.Invoke((uint)( pcm.Length * _audioFormatManager.SelectedFormat.RtpClockRate / _audioFormatManager.SelectedFormat.ClockRate), encodedSample);
                            }
                        }

                        size -= bufferSize;
                    }
                } while (size >= frameSize);

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

                _audioStream = SDL3Helper.OpenAudioDeviceStream(_audioDevice.id, ref audioSpec, UnqueueStreamCallback);
                if (_audioStream != IntPtr.Zero)
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

        private void UnqueueStreamCallback(IntPtr userdata, IntPtr stream, int additionalAmount, int totalAmount)
        {
            //throw new NotImplementedException();
        }

        public Task PauseAudio()
        {
            if (_isStarted && !_isPaused)
            {
                if (backgroundWorker.IsBusy)
                    backgroundWorker.CancelAsync();

                if(_audioStream != IntPtr.Zero)
                    SDL3Helper.PauseAudioStreamDevice(_audioStream);
                
                _isPaused = true;
                log.LogDebug("[PauseAudio] Audio source - Id:[{AudioInDeviceId}]", _audioDevice.id);
            }

            return Task.CompletedTask;
        }

        public Task ResumeAudio()
        {
            if (_isStarted && _isPaused)
            {
                if (!backgroundWorker.IsBusy)
                    backgroundWorker.RunWorkerAsync();

                if (_audioStream != IntPtr.Zero)
                    SDL3Helper.PauseAudioStreamDevice(_audioStream);

                _isPaused = false;
                log.LogDebug("[ResumeAudio] Audio source - Id:[{AudioInDeviceId}]", _audioDevice.id);
            }

            return Task.CompletedTask;
        }

        public bool IsAudioSourcePaused()
        {
            return _isPaused;
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

            _isStarted = false;
            _audioStream = IntPtr.Zero;

            return Task.CompletedTask;
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
