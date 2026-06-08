# SIPSorceryMedia.SDL3

A C# library that integrates [SDL3](https://www.libsdl.org/) audio with the [SIPSorcery](https://github.com/sipsorcery-org/sipsorcery) VoIP/WebRTC stack. It implements the `IAudioSink` and `IAudioSource` interfaces from `SIPSorceryMedia.Abstractions`, letting SDL3 serve as the audio device layer in SIP and WebRTC calls.

[![NuGet](https://img.shields.io/nuget/v/SIPSorceryMedia.SDL3)](https://www.nuget.org/packages/SIPSorceryMedia.SDL3)
[![CI](https://github.com/cinderblocks/SIPSorcery.SDL3/actions/workflows/ci.yml/badge.svg)](https://github.com/cinderblocks/SIPSorcery.SDL3/actions/workflows/ci.yml)

## Features

- **Audio playback** — `SDL3AudioEndPoint` implements `IAudioSink`. Receives encoded RTP audio frames, decodes them, and plays through any SDL3 output device.
- **Audio capture** — `SDL3AudioSource` implements `IAudioSource`. Captures from any SDL3 input device, encodes, and fires the standard SIPSorcery sample events.
- **Low-latency design** — bounded queues with drop-oldest backpressure, SDL3 stream callbacks, and `ArrayPool<byte>` throughout.
- **Bundled P/Invoke bindings** — no third-party SDL3 NuGet binding dependency; bindings are included in the library.
- **Supported audio codecs** — PCMU (G.711 µ-law), PCMA (G.711 A-law), G.722, G.729, Opus (codec implementation supplied by the caller via `IAudioEncoder`).

## Target Frameworks

`netstandard2.0` · `netstandard2.1` · `net8.0` · `net9.0` · `net10.0`

## Installation

Install the managed library:

```
dotnet add package SIPSorceryMedia.SDL3
```

You also need the SDL3 native library at runtime. The easiest way is the companion native package:

```
dotnet add package SIPSorceryMedia.SDL3.Native
```

`SIPSorceryMedia.SDL3.Native` bundles pre-built SDL3 binaries and drops them into the right `runtimes/` path automatically:

| RID | Binary |
|-----|--------|
| `win-x64` | `SDL3.dll` |
| `win-x86` | `SDL3.dll` |
| `win-arm64` | `SDL3.dll` |
| `macos-x64` | `libSDL3.dylib` (universal fat binary) |
| `macos-arm64` | `libSDL3.dylib` (universal fat binary) |

The macOS binaries are universal (arm64 + x86_64) fat binaries sourced from the official SDL3 xcframework, so the same file covers both Intel and Apple Silicon regardless of which RID resolves at runtime.

For platforms not covered by the native package, install SDL3 manually:

**Linux**
```sh
# Debian / Ubuntu
sudo apt-get install libsdl3-dev

# Fedora
sudo dnf install SDL3-devel
```

## Quick Start

### Initialize SDL3

Call `InitSDL` once at application startup and `QuitSDL` on shutdown:

```csharp
SDL3Helper.InitSDL();
// ... use the library ...
SDL3Helper.QuitSDL();
```

### Audio Playback (`SDL3AudioEndPoint`)

```csharp
// Pick a device — null/empty falls back to the system default
var devices = SDL3Helper.GetAudioPlaybackDevices();
string deviceName = devices.Values.First(); // or "Default Speakers", etc.

var encoder = new YourAudioEncoder(); // implements IAudioEncoder
using var endpoint = new SDL3AudioEndPoint(deviceName, encoder);

// Wire up to a SIPSorcery session
endpoint.SetAudioSinkFormat(audioFormat);   // triggers device open + start
session.OnAudioFrameReceived += endpoint.GotEncodedMediaFrame;

// Or push raw PCM bytes directly
endpoint.PutAudioSample(pcmBytes);
```

Diagnostic counters (buffer underruns, dropped frames, queue depth) are available via `endpoint.GetStats()`.

### Audio Capture (`SDL3AudioSource`)

```csharp
var devices = SDL3Helper.GetAudioRecordingDevices();
string deviceName = devices.Values.First();

var encoder = new YourAudioEncoder();
using var source = new SDL3AudioSource(deviceName, encoder);

source.OnAudioSourceEncodedSample += (durationRtpUnits, sample) =>
{
    // forward encoded RTP payload to SIPSorcery session
};

source.SetAudioSourceFormat(audioFormat);  // triggers device open + start
```

### Enumerating Devices

```csharp
// Playback devices  — Dictionary<uint deviceId, string name>
var playback = SDL3Helper.GetAudioPlaybackDevices();

// Recording devices
var recording = SDL3Helper.GetAudioRecordingDevices();

// Look up by name prefix (case-insensitive)
var device = SDL3Helper.GetAudioPlaybackDevice("Realtek");

// Look up by index
var device = SDL3Helper.GetAudioRecordingDevice(0);
```

### Integrating with SIPSorcery

```csharp
var encoder = new YourAudioEncoder();
var endpoint = new SDL3AudioEndPoint("Default", encoder);
var source   = new SDL3AudioSource("Default", encoder);

// MediaEndPoints wires both into a SIPSorcery call session
var mediaEndPoints = new MediaEndPoints
{
    AudioSink   = endpoint,
    AudioSource = source,
};

// Or use the helper shortcut on SDL3AudioEndPoint
var mediaEndPoints = endpoint.ToMediaEndPoints();
```

## API Reference

### `SDL3Helper` (static)

| Method | Description |
|--------|-------------|
| `InitSDL(flags)` | Initialize SDL3. Call once at startup. |
| `QuitSDL()` | Shut down SDL3. |
| `GetAudioPlaybackDevices()` | All output devices as `Dictionary<uint, string>`. |
| `GetAudioRecordingDevices()` | All input devices as `Dictionary<uint, string>`. |
| `GetAudioPlaybackDevice(name)` | Find output device by name prefix. |
| `GetAudioRecordingDevice(name)` | Find input device by name prefix. |
| `GetAudioPlaybackDevice(index)` | Find output device by index. |
| `GetAudioRecordingDevice(index)` | Find input device by index. |
| `OpenAudioDeviceStreamHandle(...)` | Open an SDL3 audio stream (SafeHandle). |
| `PutAudioToStream(handle, data, len)` | Write PCM bytes to a stream. |
| `GetAudioStreamData(handle, buf, len)` | Read PCM bytes from a stream. |
| `GetAudioStreamQueued(handle)` | Bytes queued for playback. |
| `GetAudioStreamAvailable(handle)` | Bytes available to read (recording). |
| `PauseAudioStreamDevice(handle)` | Pause a stream's device. |
| `ResumeAudioStreamDevice(handle)` | Resume a stream's device. |
| `DestroyAudioStream(handle)` | Unregister callbacks and dispose a stream. |
| `GetAudioSpec(clockRate, channels)` | Build an `SDL_AudioSpec`. |
| `GetBytesPerSample(spec)` | Bytes per sample for a given spec. |
| `GetBytesPerSecond(spec)` | Bytes per second for a given spec. |

### `SDL3AudioEndPoint`

Implements `IAudioSink`, `IDisposable`.

| Member | Description |
|--------|-------------|
| `SetAudioSinkFormat(format)` | Set codec format, open device, start playback. |
| `GotEncodedMediaFrame(frame)` | Receive an encoded RTP frame for decoding and playback. |
| `PutAudioSample(pcmBytes)` | Queue raw PCM bytes for playback. |
| `GetAudioSinkFormats()` | Supported formats from the encoder. |
| `RestrictFormats(predicate)` | Filter allowed formats. |
| `ToMediaEndPoints()` | Wrap in a `MediaEndPoints`. |
| `StartAudioSink()` / `CloseAudioSink()` | Lifecycle control. |
| `PauseAudioSink()` / `ResumeAudioSink()` | Pause/resume. |
| `GetStats()` | Returns `AudioSinkStats` (underruns, dropped frames, queue depth). |
| `ResetStats()` | Zero the diagnostic counters. |

### `SDL3AudioSource`

Implements `IAudioSource`, `IDisposable`.

| Member | Description |
|--------|-------------|
| `SetAudioSourceFormat(format)` | Set codec format, open device, start capture. |
| `GetAudioSourceFormats()` | Supported formats from the encoder. |
| `RestrictFormats(predicate)` | Filter allowed formats. |
| `StartAudio()` / `CloseAudio()` | Lifecycle control. |
| `PauseAudio()` / `ResumeAudio()` | Pause/resume. |
| `IsAudioSourcePaused()` | Returns current pause state. |
| `HasEncodedAudioSubscribers()` | True if anything is subscribed to `OnAudioSourceEncodedSample`. |
| `GetStats()` | Returns `AudioSourceStats` (overruns, dropped frames). |
| `ResetStats()` | Zero the diagnostic counters. |
| `OnAudioSourceEncodedSample` | Fires with encoded RTP payload after each captured frame. |
| `OnAudioSourceRawSample` | Fires with raw PCM shorts after each captured frame. |

## Sample Projects

The `test/` directory contains standalone console apps:

| Project | Description |
|---------|-------------|
| `PlayAudioFile` | Select a playback device and play a WAV file through SDL3 directly. |
| `PlayVideoFile` | Play a video file with audio; video frames rendered as ASCII art. |
| `RecordThenPlayback` | Record 5 seconds from the default microphone then play it back. |
| `CheckCodec` | Encode/decode a round-trip through a selected audio codec and play the result. |

## Building from Source

```sh
git clone https://github.com/cinderblocks/SIPSorcery.SDL3.git
cd SIPSorcery.SDL3
dotnet build src/SIPSorceryMedia.SDL3.csproj -c Release
```

CI runs automatically on every push and pull request via GitHub Actions. Publishing to NuGet.org and GitHub Packages is triggered by pushing a `v*` tag.

## License

This project is licensed under the [Zlib license](LICENSE).

Original SDL2 implementation by Christophe Irles. SDL3 port and ongoing maintenance by [Sjofn LLC](https://sjofn.com).
