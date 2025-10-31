using System;
using System.Linq;
using static SDL3.SDL;

namespace SIPSorceryMedia.SDL3
{
    public unsafe class SDL3RecordThenPlayback
    {
        private enum RecordingState
        {
            //SELECTING_DEVICE,
            STOPPED,
            RECORDING,
            RECORDED,
            PLAYBACK,
            ERROR
        };

        //Maximum number of supported recording devices
        private const int MAX_RECORDING_DEVICES = 10;

        //Maximum recording time
        private const int MAX_RECORDING_SECONDS = 5;

        //Maximum recording time plus padding
        private const int RECORDING_BUFFER_SECONDS = MAX_RECORDING_SECONDS + 1;

        //Received audio spec
        private readonly SDL_AudioSpec gReceivedRecordingSpec;
        private SDL_AudioSpec gReceivedPlaybackSpec;

        //Recording data buffer
        private readonly byte[] gRecordingBuffer; // Uint8

        //Size of data buffer
        private readonly uint gBufferByteSize = 0; // Uint32

        //Position in data buffer
        private uint gBufferBytePosition = 0; // Uint32

        //Maximum position in data buffer for recording
        private readonly uint gBufferByteMaxPosition = 0; //Uint32

        //Number of available devices
        private readonly IntPtr recordingStream;
        private readonly IntPtr playbackStream;

        //String recordingDeviceNameToChoose = "External Mic";
        private readonly string recordingDeviceNameToChoose = "Microphone (2";

        //String playbackDeviceNameToChoose = "Realtek HD Audio 2nd output";
        private readonly string playbackDeviceNameToChoose = "Speakers (2";

        private readonly RecordingState currentState = RecordingState.STOPPED;


        public SDL3RecordThenPlayback()
        {
            //SDL_SetHint(SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");

            if (!SDL_Init(SDL_InitFlags.SDL_INIT_AUDIO))
                throw new ApplicationException($"Cannot initialized SDL for Audio purpose");

            var recordingDevice = SDL3Helper.GetAudioRecordingDevice(recordingDeviceNameToChoose);
            if (!recordingDevice.HasValue)
                throw new ApplicationException($"Recording device not found");

            var playbackDevice = SDL3Helper.GetAudioPlaybackDevice(playbackDeviceNameToChoose);
            if (!playbackDevice.HasValue)
                throw new ApplicationException($"Playback device not found");

            //Default audio spec - recording
            SDL_AudioSpec desiredRecordingSpec = new SDL_AudioSpec
            {
                freq = 44100,
                format = SDL_AudioFormat.SDL_AUDIO_F32,
                channels = 2
            };

            //Open recording device
            recordingStream = SDL3Helper.OpenAudioDeviceStream(recordingDevice.Value.id, ref desiredRecordingSpec, FillRecordingCallback);

            //Device failed to open
            if (recordingStream == IntPtr.Zero)
                throw new ApplicationException($"Cannot open recording device");


            //Default audio spec - playback
            SDL_AudioSpec desiredPlaybackSpec = new SDL_AudioSpec
            {
                freq = 44100,
                format = SDL_AudioFormat.SDL_AUDIO_F32,
                channels = 2
            };

            //Open playback device
            playbackStream = SDL3Helper.OpenAudioDeviceStream(playbackDevice.Value.id, ref desiredPlaybackSpec, FillPlaybackCallback);

            //Device failed to open
            if (playbackStream == IntPtr.Zero)
                throw new ApplicationException($"Cannot open playback device");


            //Calculate per sample bytes
            int bytesPerSample = gReceivedRecordingSpec.channels * (SDL_AUDIO_BITSIZE((ushort)gReceivedRecordingSpec.format) / 8);

            //Calculate bytes per second
            int bytesPerSecond = SDL3Helper.GetBytesPerSecond(gReceivedRecordingSpec);

            //Calculate buffer size
            gBufferByteSize = (uint)(RECORDING_BUFFER_SECONDS * bytesPerSecond);

            //Calculate max buffer use
            gBufferByteMaxPosition = (uint)(MAX_RECORDING_SECONDS * bytesPerSecond);

            //Allocate and initialize byte buffer
            gRecordingBuffer = Enumerable.Repeat((byte)0, (int)gBufferByteSize).ToArray();

            bool quit = false;

            currentState = RecordingState.STOPPED;

            while (!quit)
            {
                switch (currentState)
                {
                    case RecordingState.STOPPED:
                        //Go back to beginning of buffer
                        gBufferBytePosition = 0;

                        //Start recording
                        SDL3Helper.ResumeAudioStreamDevice(recordingStream);

                        //Go on to next state
                        currentState = RecordingState.RECORDING;
                        break;

                    case RecordingState.RECORDING:
                        //Lock callback
                        //SDL_LockAudioDevice(recordingStream);

                        //Finished recording
                        if (gBufferBytePosition > gBufferByteMaxPosition)
                        {
                            //Stop recording audio
                            SDL_PauseAudioDevice(recordingDevice.Value.id);

                            //Go back to beginning of buffer
                            gBufferBytePosition = 0;

                            //Start playback
                            SDL_ResumeAudioDevice(recordingDevice.Value.id);

                            //Go on to next state
                            currentState = RecordingState.PLAYBACK;
                        }

                        //Unlock callback
                        //SDL_UnlockAudioDevice(recordingStream);
                        break;

                    case RecordingState.PLAYBACK:
                        //Lock callback
                        //SDL_LockAudioDevice(playbackStream);

                        //Finished playback
                        if (gBufferBytePosition > gBufferByteMaxPosition)
                        {
                            //Stop playing audio
                            SDL_PauseAudioDevice(playbackDevice.Value.id);

                            //Go on to next state
                            currentState = RecordingState.STOPPED;
                            quit = true;
                        }

                        //Unlock callback
                        //SDL_UnlockAudioDevice(playbackStream);
                        break;
                }
            }

            SDL_Quit();
        }

        private void FillRecordingCallback(IntPtr userdata, IntPtr stream, int additionalAmount, int totalAmount)
        {
            //Copy audio from stream
            fixed (byte* ptr = &gRecordingBuffer[gBufferBytePosition])
            {
                Buffer.MemoryCopy((byte*)stream, ptr, additionalAmount, additionalAmount);
            }                          

            //Move along buffer
            gBufferBytePosition += (uint)additionalAmount;
        }

        void FillPlaybackCallback(IntPtr userdata, IntPtr stream, int additionalAmount, int totalAmount)
        {
            //Copy audio from stream
            fixed (byte* ptr = &gRecordingBuffer[gBufferBytePosition])
            {
                Buffer.MemoryCopy(ptr, (byte*)stream, additionalAmount, additionalAmount);
            }

            //Move along buffer
            gBufferBytePosition += (uint)additionalAmount;
        }

        //public unsafe SDL3AudioPlayBack(String path)
        //      {
        //SDL_SetHint(SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");

        //if (SDL_Init(SDL_INIT_AUDIO | SDL_INIT_TIMER) < 0)
        //         {
        //             throw new ApplicationException($"Cannot initialize SDL for Audio purpose");
        //         }


        //// Get microphones
        //int nbMicrophones = SDL_GetNumAudioDevices((int)SDL_bool.SDL_TRUE);
        //for (int index = 0; index < nbMicrophones; index++)
        //{
        //	String microphone = SDL_GetAudioDeviceName(index, (int)SDL_bool.SDL_TRUE);
        //}

        //// Get playback
        //int nbAudioPlaybacks = SDL_GetNumAudioDevices((int)SDL_bool.SDL_FALSE);
        //for (int index = 0; index < nbAudioPlaybacks; index++)
        //{
        //	String audioPlayback = SDL_GetAudioDeviceName(index, (int)SDL_bool.SDL_FALSE);
        //}


        ////SDL_AudioSpec
        //SDL_AudioSpec wanted_spec = new SDL_AudioSpec();
        //wanted_spec.freq = 44100;
        //wanted_spec.format = AUDIO_S16SYS;
        //wanted_spec.channels = 2;
        //wanted_spec.silence = 0;
        //wanted_spec.samples = 1024;
        //wanted_spec.callback = fill_audio;

        //SDL_AudioSpec obtained_spec;

        //String defaultAudioPlayback = SDL_GetAudioDeviceName(0, (int)SDL_bool.SDL_FALSE);
        //GCHandle handle = GCHandle.Alloc(defaultAudioPlayback);
        //IntPtr devicePlayBack = (IntPtr)handle;
        //if (SDL_OpenAudioDevice(devicePlayBack, (int)SDL_bool.SDL_FALSE, ref wanted_spec, out obtained_spec, (int)SDL_AUDIO_ALLOW_FORMAT_CHANGE) < 0)
        //{
        //	handle.Free();
        //	throw new ApplicationException($"Cannot open audio");
        //}
        //handle.Free();

        //using (FileStream fs = File.OpenRead(path))
        //{
        //	fs.Position = 0;

        //	int pcm_buffer_size = 4096;
        //	byte [] pcm_buffer = new byte[pcm_buffer_size];
        //	int data_count = 0;

        //	//Play
        //	SDL_PauseAudio(0);

        //	while (true)
        //	{
        //		if( fs.Read(pcm_buffer, data_count, pcm_buffer_size) != pcm_buffer_size)
        //                 {
        //			break;
        //			//fs.Position = 0;
        //			//fs.Read(pcm_buffer, pcm_buffer_size, 1);
        //			//data_count = 0;
        //		}

        //		data_count += pcm_buffer_size;

        //		//Audio buffer length
        //		audio_len = (uint)pcm_buffer_size;
        //		audio_pos = pcm_buffer;

        //		while (audio_len > 0)//Wait until finish
        //			SDL_Delay(1);
        //	}
        //	SDL_Quit();
        //}
        //}

        //public unsafe void fill_audio(IntPtr userdata, IntPtr stream, int len)
        //{
        //	//SDL 2.0
        //	ZeroMemory(stream, new IntPtr(len));

        //	if (audio_len == 0)     /*  Only  play  if  we  have  data  left  */
        //		return;
        //	len = (len > audio_len ? (int)audio_len : len);  /*  Mix  as  much  data  as  possible  */


        //	byte[] dst = new byte[Marshal.SizeOf(stream)];
        //	Marshal.Copy(stream, dst, 0, Marshal.SizeOf(stream));

        //	SDL_MixAudio(dst, audio_pos, (uint)len, SDL_MIX_MAXVOLUME);
        //	audio_len -= (uint)len;
        //}
    }
}
