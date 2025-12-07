This package contains prebuilt native SDL3 binaries for Windows (x86/x64/arm64).

Place the prebuilt binaries in the corresponding runtimes/*/native folders before packing:

runtimes/win-x64/native/SDL3.dll
runtimes/win-x86/native/SDL3.dll
runtimes/win-arm64/native/SDL3.dll

When creating the NuGet package, run:

  dotnet pack src/SIPSorceryMedia.SDL3.Native/SIPSorceryMedia.SDL3.Native.csproj -c Release

The resulting package will include the native libraries and can be referenced as a dependency.
