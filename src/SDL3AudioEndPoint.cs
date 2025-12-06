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
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SIPSorceryMedia.SDL3
{
    public class SDL3AudioEndPoint : IAudioSink
    {
        private readonly ILogger log = SIPSorcery.LogFactory.CreateLogger<SDL3AudioEndPoint>();


        private readonly IAudioEncoder _audioEncoder;
        private readonly MediaFormatManager<AudioFormat> _audioFormatManager;

        private readonly (uint id, string name) _audioDevice;
        private IntPtr _audioStream = IntPtr.Zero;

        protected bool _isStarted = false;
        protected bool _isPaused = true;
        protected bool _isClosed = true;

        public event SourceErrorDelegate ? OnAudioSinkError = null;

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

            var device = SDL3Helper.GetAudioRecordingDevice(audioOutDeviceName);
            if (!device.HasValue)
            {
                throw new ApplicationException($"Could not get audio recording device named {audioOutDeviceName}");
            }
            _audioDevice = device.Value;
        }

        private void RaiseAudioSinkError(string err)
        {
            CloseAudioSink();
            OnAudioSinkError?.Invoke(err);
        }

        public void GotEncodedMediaFrame(EncodedAudioFrame encodedMediaFrame)
        {
            var audioFormat = encodedMediaFrame.AudioFormat;

            if (_audioStream != IntPtr.Zero && !audioFormat.IsEmpty())
            {
                // Decode sample
                var pcmSample = _audioEncoder.DecodeAudio(encodedMediaFrame.EncodedAudio, audioFormat);
                // Convert short[] to byte[] efficiently
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
            return new MediaEndPoints
            {
                AudioSink = this,
            };
        }

        private void InitPlaybackDevice()
        {
            try
            {
                // Stop previous playback device
                CloseAudioSink();

                // Init Playback device.
                AudioFormat audioFormat = _audioFormatManager.SelectedFormat;
                var audioSpec = SDL3Helper.GetAudioSpec(audioFormat.ClockRate, 1);

                _audioStream = SDL3Helper.OpenAudioDeviceStream(_audioDevice.id, ref audioSpec, FeedStreamCallback);
                if(_audioStream != IntPtr.Zero)
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
                RaiseAudioSinkError($"SDLAudioEndPoint failed to initialise device. No audio device found - Id:[{_audioDevice.id}] - DeviceName:[{_audioDevice.name}] - Exception:[{e.Message}]");
            }
        }

        private void FeedStreamCallback(IntPtr userdata, IntPtr stream, int additionalAmount, int totalAmount)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Event handler for playing audio samples received from the remote call party.
        /// </summary>
        /// <param name="pcmSample">Raw PCM sample from remote party.</param>
        public void PutAudioSample(byte[] pcmSample)
        {
            if (_audioStream != IntPtr.Zero)
            {
                SDL3Helper.PutAudioToStream(_audioStream, ref pcmSample, pcmSample.Length);
            }
        }

        [Obsolete("Use GotEncodeMediaFrame instead.")]
        public void GotAudioRtp(IPEndPoint remoteEndPoint, uint ssrc, uint seqnum, uint timestamp, int payloadID, bool marker, byte[] payload)
        {
            if (_audioStream == IntPtr.Zero) { return; }

            // Decode sample
            var pcmSample = _audioEncoder.DecodeAudio(payload, _audioFormatManager.SelectedFormat);
            var pcmBytes = new byte[pcmSample.Length * sizeof(short)];
            Buffer.BlockCopy(pcmSample, 0, pcmBytes, 0, pcmBytes.Length);
            PutAudioSample(pcmBytes);
        }

        public Task PauseAudioSink()
        {
            if (_isStarted && !_isPaused)
            {
                SDL3Helper.PauseAudioStreamDevice(_audioStream);
                _isPaused = true;

                log.LogDebug("[PauseAudioSink] Audio output - Id:[{AudioOutDeviceId}]", _audioDevice.id);
            }

            return Task.CompletedTask;
        }

        public Task ResumeAudioSink()
        {
            if (_isStarted && _isPaused)
            {
                SDL3Helper.ResumeAudioStreamDevice(_audioStream);
                _isPaused = false;

                log.LogDebug("[ResumeAudioSink] Audio output - Id:[{AudioOutDeviceId}]", _audioDevice.id);
            }

            return Task.CompletedTask;
        }

        public Task StartAudioSink()
        {
            if(!_isStarted && _audioStream != IntPtr.Zero)
            {
                _isStarted = true;
                _isClosed = false;
                _isPaused = true;

                ResumeAudioSink();
            }

            return Task.CompletedTask;
        }

        public Task CloseAudioSink()
        {
            if (_isStarted && (_audioStream != IntPtr.Zero))
            {
                PauseAudioSink().Wait();
                SDL3Helper.DestroyAudioStream(_audioStream);

                _isClosed = true;
                _isStarted = false;

                log.LogDebug("[CloseAudioSink] Audio output - Id:[{AudioOutDeviceId}]", _audioDevice.id);

                _audioStream = IntPtr.Zero;
            }

            return Task.CompletedTask;
        }
    }
}

