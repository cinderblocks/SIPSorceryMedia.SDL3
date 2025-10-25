﻿using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.FFmpeg;
using SIPSorceryMedia.SDL3;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayVideoFile
{
    internal class ProgramPlayVideoFile
    {
        // Path to a valid Video file
        const string VIDEO_FILE_PATH = "./../../../../../media/big_buck_bunny.mp4";

        // Path to FFmpeg library
        private const string FFMPEG_LIB_PATH = @"C:\ffmpeg-4.4.1-full_build-shared\bin"; // On Windows
        //private const string LIB_PATH = @"/usr/local/Cellar/ffmpeg/4.4.1_5/lib"; // On MacBookPro
        //private const string LIB_PATH = @"..\..\..\..\..\lib\x64";

        static AsciiFrame ? asciiFrame = null;
        static SDL3AudioEndPoint ? audioEndPoint = null;

        static void Main(string[] args)
        {
            int audioPlaybackDeviceIndex = 0; // To store the index of the audio playback device selected
            string audioPlaybackDeviceName; // To store the name of the audio playback device selected

            VideoCodecsEnum VideoCodec = VideoCodecsEnum.H264;
            IVideoSource videoSource;
            IAudioSource audioSource;

            AudioEncoder audioEncoder;

            Console.Clear();

            // Initialise FFmpeg librairies
            Console.WriteLine("\nTry to init FFmpeg libraries");
            FFmpegInit.Initialise(FfmpegLogLevelEnum.AV_LOG_FATAL, FFMPEG_LIB_PATH);


            // Init SDL Library - Library files must be in the same folder than the application
            Console.WriteLine("\nTry to init SDL3 libraries - they must be stored in the same folder than this application");
            SDL3Helper.InitSDL();

            Console.WriteLine("\nInit done");

            // Get list of Audio Playback devices
            List<string> sdlDevices = SIPSorceryMedia.SDL3.SDL3Helper.GetAudioPlaybackDevices();

            // Quit since no Audio playback found
            if ((sdlDevices == null) || (sdlDevices.Count == 0))
            {
                Console.WriteLine("No Audio playback devices found ...");
                SDL3Helper.QuitSDL();
                return;
            }

            // Allow end user to select Audio playback device
            if (sdlDevices?.Count > 0)
            {
                while (true)
                {
                    Console.WriteLine("\nSelect audio playback device:");
                    int index = 1;
                    foreach (string device in sdlDevices)
                    {
                        Console.Write($"\n [{index}] - {device} ");
                        index++;
                    }
                    Console.WriteLine("\n");
                    Console.Out.Flush();

                    var keyConsole = Console.ReadKey();
                    if (int.TryParse("" + keyConsole.KeyChar, out int keyValue) && keyValue < index && keyValue >= 0)
                    {
                        audioPlaybackDeviceIndex = keyValue;
                        break;
                    }
                }
            }

            // Get name of the device
            audioPlaybackDeviceName = sdlDevices[audioPlaybackDeviceIndex - 1];
            Console.WriteLine($"\nDevice selected: {audioPlaybackDeviceName}");

            //Create AudioEncoder: Genereic object used to Encode or Decode Audio Sample
            audioEncoder = new AudioEncoder();

            // Create audio end point: it will be used to play back tuhe audio from the video file
            audioEndPoint = new SDL3AudioEndPoint(audioPlaybackDeviceName, audioEncoder);
            audioEndPoint.SetAudioSinkFormat(new AudioFormat(SDPWellKnownMediaFormatsEnum.PCMU));
            audioEndPoint.StartAudioSink();

            // Create object used to display video in Ascii
            asciiFrame = new AsciiFrame();

            // Create VideoSource Interface using video file
            SIPSorceryMedia.FFmpeg.FFmpegFileSource fileSource = new SIPSorceryMedia.FFmpeg.FFmpegFileSource(VIDEO_FILE_PATH, false, audioEncoder);
            videoSource = fileSource as IVideoSource;
            videoSource.RestrictFormats(x => x.Codec == VideoCodec);
            videoSource.SetVideoSourceFormat(videoSource.GetVideoSourceFormats().Find(x => x.Codec == VideoCodec));
            videoSource.OnVideoSourceRawSampleFaster += FileSource_OnVideoSourceRawSampleFaster;

            audioSource = fileSource as IAudioSource;
            audioSource.SetAudioSourceFormat(audioSource.GetAudioSourceFormats().Find(x => x.Codec == AudioCodecsEnum.PCMU));
            audioSource.OnAudioSourceRawSample += FileSource_OnAudioSourceRawSample;

            videoSource.StartVideo();
            audioSource.StartAudio();

            for (var loop = true; loop;)
            {
                var cki = Console.ReadKey(true);
                switch (cki.Key)
                {
                    case ConsoleKey.Q:
                    case ConsoleKey.Enter:
                    case ConsoleKey.Escape:
                        Console.CursorVisible = true;
                        loop = false;
                        break;
                }
            }

            videoSource.CloseVideo();

            // Quit SDL Library
            SDL3Helper.QuitSDL();
        }

        private static void FileSource_OnAudioSourceRawSample(AudioSamplingRatesEnum samplingRate, uint durationMilliseconds, short[] sample)
        {
            byte[] pcmBytes = sample.SelectMany(x => BitConverter.GetBytes(x)).ToArray();

            audioEndPoint?.GotAudioSample(pcmBytes);
        }

        private static void FileSource_OnVideoSourceRawSampleFaster(uint durationMilliseconds, RawImage rawImage)
        {
            asciiFrame.GotRawImage(ref rawImage);
        }
    }
}
