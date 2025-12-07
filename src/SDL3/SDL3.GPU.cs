/**
 * @file SDL3.cs
 * @brief C# Bindings for SDL3
 *
 * Copyright 2024, Colin "cryy22" Jackson <c@cryy22.art>.
 * Copyright 2025, Sjofn LLC. - Backport to .NETStandard2.0
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 */

using System;
using System.Runtime.InteropServices;

namespace SDL3;

public static unsafe partial class SDL
{
    public const string SDL_PROP_GPU_DEVICE_CREATE_DEBUGMODE_BOOLEAN = "SDL.gpu.device.create.debugmode";
    public const string SDL_PROP_GPU_DEVICE_CREATE_PREFERLOWPOWER_BOOLEAN = "SDL.gpu.device.create.preferlowpower";
    public const string SDL_PROP_GPU_DEVICE_CREATE_NAME_STRING = "SDL.gpu.device.create.name";
    public const string SDL_PROP_GPU_DEVICE_CREATE_SHADERS_PRIVATE_BOOLEAN = "SDL.gpu.device.create.shaders.private";
    public const string SDL_PROP_GPU_DEVICE_CREATE_SHADERS_SPIRV_BOOLEAN = "SDL.gpu.device.create.shaders.spirv";
    public const string SDL_PROP_GPU_DEVICE_CREATE_SHADERS_DXBC_BOOLEAN = "SDL.gpu.device.create.shaders.dxbc";
    public const string SDL_PROP_GPU_DEVICE_CREATE_SHADERS_DXIL_BOOLEAN = "SDL.gpu.device.create.shaders.dxil";
    public const string SDL_PROP_GPU_DEVICE_CREATE_SHADERS_MSL_BOOLEAN = "SDL.gpu.device.create.shaders.msl";
    public const string SDL_PROP_GPU_DEVICE_CREATE_SHADERS_METALLIB_BOOLEAN = "SDL.gpu.device.create.shaders.metallib";
    public const string SDL_PROP_GPU_DEVICE_CREATE_D3D12_SEMANTIC_NAME_STRING = "SDL.gpu.device.create.d3d12.semantic";
    public const string SDL_PROP_GPU_COMPUTEPIPELINE_CREATE_NAME_STRING = "SDL.gpu.computepipeline.create.name";
    public const string SDL_PROP_GPU_GRAPHICSPIPELINE_CREATE_NAME_STRING = "SDL.gpu.graphicspipeline.create.name";
    public const string SDL_PROP_GPU_SAMPLER_CREATE_NAME_STRING = "SDL.gpu.sampler.create.name";
    public const string SDL_PROP_GPU_SHADER_CREATE_NAME_STRING = "SDL.gpu.shader.create.name";
    public const string SDL_PROP_GPU_TEXTURE_CREATE_D3D12_CLEAR_R_FLOAT = "SDL.gpu.texture.create.d3d12.clear.r";
    public const string SDL_PROP_GPU_TEXTURE_CREATE_D3D12_CLEAR_G_FLOAT = "SDL.gpu.texture.create.d3d12.clear.g";
    public const string SDL_PROP_GPU_TEXTURE_CREATE_D3D12_CLEAR_B_FLOAT = "SDL.gpu.texture.create.d3d12.clear.b";
    public const string SDL_PROP_GPU_TEXTURE_CREATE_D3D12_CLEAR_A_FLOAT = "SDL.gpu.texture.create.d3d12.clear.a";

    public const string SDL_PROP_GPU_TEXTURE_CREATE_D3D12_CLEAR_DEPTH_FLOAT =
        "SDL.gpu.texture.create.d3d12.clear.depth";

    public const string SDL_PROP_GPU_TEXTURE_CREATE_D3D12_CLEAR_STENCIL_UINT8 =
        "SDL.gpu.texture.create.d3d12.clear.stencil";

    public const string SDL_PROP_GPU_TEXTURE_CREATE_NAME_STRING = "SDL.gpu.texture.create.name";
    public const string SDL_PROP_GPU_BUFFER_CREATE_NAME_STRING = "SDL.gpu.buffer.create.name";
    public const string SDL_PROP_GPU_TRANSFERBUFFER_CREATE_NAME_STRING = "SDL.gpu.transferbuffer.create.name";

    public enum SDL_GPUPrimitiveType
    {
        SDL_GPU_PRIMITIVETYPE_TRIANGLELIST = 0,
        SDL_GPU_PRIMITIVETYPE_TRIANGLESTRIP = 1,
        SDL_GPU_PRIMITIVETYPE_LINELIST = 2,
        SDL_GPU_PRIMITIVETYPE_LINESTRIP = 3,
        SDL_GPU_PRIMITIVETYPE_POINTLIST = 4,
    }

    public enum SDL_GPULoadOp
    {
        SDL_GPU_LOADOP_LOAD = 0,
        SDL_GPU_LOADOP_CLEAR = 1,
        SDL_GPU_LOADOP_DONT_CARE = 2,
    }

    public enum SDL_GPUStoreOp
    {
        SDL_GPU_STOREOP_STORE = 0,
        SDL_GPU_STOREOP_DONT_CARE = 1,
        SDL_GPU_STOREOP_RESOLVE = 2,
        SDL_GPU_STOREOP_RESOLVE_AND_STORE = 3,
    }

    public enum SDL_GPUIndexElementSize
    {
        SDL_GPU_INDEXELEMENTSIZE_16BIT = 0,
        SDL_GPU_INDEXELEMENTSIZE_32BIT = 1,
    }

    public enum SDL_GPUTextureFormat
    {
        SDL_GPU_TEXTUREFORMAT_INVALID = 0,
        SDL_GPU_TEXTUREFORMAT_A8_UNORM = 1,
        SDL_GPU_TEXTUREFORMAT_R8_UNORM = 2,
        SDL_GPU_TEXTUREFORMAT_R8G8_UNORM = 3,
        SDL_GPU_TEXTUREFORMAT_R8G8B8A8_UNORM = 4,
        SDL_GPU_TEXTUREFORMAT_R16_UNORM = 5,
        SDL_GPU_TEXTUREFORMAT_R16G16_UNORM = 6,
        SDL_GPU_TEXTUREFORMAT_R16G16B16A16_UNORM = 7,
        SDL_GPU_TEXTUREFORMAT_R10G10B10A2_UNORM = 8,
        SDL_GPU_TEXTUREFORMAT_B5G6R5_UNORM = 9,
        SDL_GPU_TEXTUREFORMAT_B5G5R5A1_UNORM = 10,
        SDL_GPU_TEXTUREFORMAT_B4G4R4A4_UNORM = 11,
        SDL_GPU_TEXTUREFORMAT_B8G8R8A8_UNORM = 12,
        SDL_GPU_TEXTUREFORMAT_BC1_RGBA_UNORM = 13,
        SDL_GPU_TEXTUREFORMAT_BC2_RGBA_UNORM = 14,
        SDL_GPU_TEXTUREFORMAT_BC3_RGBA_UNORM = 15,
        SDL_GPU_TEXTUREFORMAT_BC4_R_UNORM = 16,
        SDL_GPU_TEXTUREFORMAT_BC5_RG_UNORM = 17,
        SDL_GPU_TEXTUREFORMAT_BC7_RGBA_UNORM = 18,
        SDL_GPU_TEXTUREFORMAT_BC6H_RGB_FLOAT = 19,
        SDL_GPU_TEXTUREFORMAT_BC6H_RGB_UFLOAT = 20,
        SDL_GPU_TEXTUREFORMAT_R8_SNORM = 21,
        SDL_GPU_TEXTUREFORMAT_R8G8_SNORM = 22,
        SDL_GPU_TEXTUREFORMAT_R8G8B8A8_SNORM = 23,
        SDL_GPU_TEXTUREFORMAT_R16_SNORM = 24,
        SDL_GPU_TEXTUREFORMAT_R16G16_SNORM = 25,
        SDL_GPU_TEXTUREFORMAT_R16G16B16A16_SNORM = 26,
        SDL_GPU_TEXTUREFORMAT_R16_FLOAT = 27,
        SDL_GPU_TEXTUREFORMAT_R16G16_FLOAT = 28,
        SDL_GPU_TEXTUREFORMAT_R16G16B16A16_FLOAT = 29,
        SDL_GPU_TEXTUREFORMAT_R32_FLOAT = 30,
        SDL_GPU_TEXTUREFORMAT_R32G32_FLOAT = 31,
        SDL_GPU_TEXTUREFORMAT_R32G32B32A32_FLOAT = 32,
        SDL_GPU_TEXTUREFORMAT_R11G11B10_UFLOAT = 33,
        SDL_GPU_TEXTUREFORMAT_R8_UINT = 34,
        SDL_GPU_TEXTUREFORMAT_R8G8_UINT = 35,
        SDL_GPU_TEXTUREFORMAT_R8G8B8A8_UINT = 36,
        SDL_GPU_TEXTUREFORMAT_R16_UINT = 37,
        SDL_GPU_TEXTUREFORMAT_R16G16_UINT = 38,
        SDL_GPU_TEXTUREFORMAT_R16G16B16A16_UINT = 39,
        SDL_GPU_TEXTUREFORMAT_R32_UINT = 40,
        SDL_GPU_TEXTUREFORMAT_R32G32_UINT = 41,
        SDL_GPU_TEXTUREFORMAT_R32G32B32A32_UINT = 42,
        SDL_GPU_TEXTUREFORMAT_R8_INT = 43,
        SDL_GPU_TEXTUREFORMAT_R8G8_INT = 44,
        SDL_GPU_TEXTUREFORMAT_R8G8B8A8_INT = 45,
        SDL_GPU_TEXTUREFORMAT_R16_INT = 46,
        SDL_GPU_TEXTUREFORMAT_R16G16_INT = 47,
        SDL_GPU_TEXTUREFORMAT_R16G16B16A16_INT = 48,
        SDL_GPU_TEXTUREFORMAT_R32_INT = 49,
        SDL_GPU_TEXTUREFORMAT_R32G32_INT = 50,
        SDL_GPU_TEXTUREFORMAT_R32G32B32A32_INT = 51,
        SDL_GPU_TEXTUREFORMAT_R8G8B8A8_UNORM_SRGB = 52,
        SDL_GPU_TEXTUREFORMAT_B8G8R8A8_UNORM_SRGB = 53,
        SDL_GPU_TEXTUREFORMAT_BC1_RGBA_UNORM_SRGB = 54,
        SDL_GPU_TEXTUREFORMAT_BC2_RGBA_UNORM_SRGB = 55,
        SDL_GPU_TEXTUREFORMAT_BC3_RGBA_UNORM_SRGB = 56,
        SDL_GPU_TEXTUREFORMAT_BC7_RGBA_UNORM_SRGB = 57,
        SDL_GPU_TEXTUREFORMAT_D16_UNORM = 58,
        SDL_GPU_TEXTUREFORMAT_D24_UNORM = 59,
        SDL_GPU_TEXTUREFORMAT_D32_FLOAT = 60,
        SDL_GPU_TEXTUREFORMAT_D24_UNORM_S8_UINT = 61,
        SDL_GPU_TEXTUREFORMAT_D32_FLOAT_S8_UINT = 62,
        SDL_GPU_TEXTUREFORMAT_ASTC_4x4_UNORM = 63,
        SDL_GPU_TEXTUREFORMAT_ASTC_5x4_UNORM = 64,
        SDL_GPU_TEXTUREFORMAT_ASTC_5x5_UNORM = 65,
        SDL_GPU_TEXTUREFORMAT_ASTC_6x5_UNORM = 66,
        SDL_GPU_TEXTUREFORMAT_ASTC_6x6_UNORM = 67,
        SDL_GPU_TEXTUREFORMAT_ASTC_8x5_UNORM = 68,
        SDL_GPU_TEXTUREFORMAT_ASTC_8x6_UNORM = 69,
        SDL_GPU_TEXTUREFORMAT_ASTC_8x8_UNORM = 70,
        SDL_GPU_TEXTUREFORMAT_ASTC_10x5_UNORM = 71,
        SDL_GPU_TEXTUREFORMAT_ASTC_10x6_UNORM = 72,
        SDL_GPU_TEXTUREFORMAT_ASTC_10x8_UNORM = 73,
        SDL_GPU_TEXTUREFORMAT_ASTC_10x10_UNORM = 74,
        SDL_GPU_TEXTUREFORMAT_ASTC_12x10_UNORM = 75,
        SDL_GPU_TEXTUREFORMAT_ASTC_12x12_UNORM = 76,
        SDL_GPU_TEXTUREFORMAT_ASTC_4x4_UNORM_SRGB = 77,
        SDL_GPU_TEXTUREFORMAT_ASTC_5x4_UNORM_SRGB = 78,
        SDL_GPU_TEXTUREFORMAT_ASTC_5x5_UNORM_SRGB = 79,
        SDL_GPU_TEXTUREFORMAT_ASTC_6x5_UNORM_SRGB = 80,
        SDL_GPU_TEXTUREFORMAT_ASTC_6x6_UNORM_SRGB = 81,
        SDL_GPU_TEXTUREFORMAT_ASTC_8x5_UNORM_SRGB = 82,
        SDL_GPU_TEXTUREFORMAT_ASTC_8x6_UNORM_SRGB = 83,
        SDL_GPU_TEXTUREFORMAT_ASTC_8x8_UNORM_SRGB = 84,
        SDL_GPU_TEXTUREFORMAT_ASTC_10x5_UNORM_SRGB = 85,
        SDL_GPU_TEXTUREFORMAT_ASTC_10x6_UNORM_SRGB = 86,
        SDL_GPU_TEXTUREFORMAT_ASTC_10x8_UNORM_SRGB = 87,
        SDL_GPU_TEXTUREFORMAT_ASTC_10x10_UNORM_SRGB = 88,
        SDL_GPU_TEXTUREFORMAT_ASTC_12x10_UNORM_SRGB = 89,
        SDL_GPU_TEXTUREFORMAT_ASTC_12x12_UNORM_SRGB = 90,
        SDL_GPU_TEXTUREFORMAT_ASTC_4x4_FLOAT = 91,
        SDL_GPU_TEXTUREFORMAT_ASTC_5x4_FLOAT = 92,
        SDL_GPU_TEXTUREFORMAT_ASTC_5x5_FLOAT = 93,
        SDL_GPU_TEXTUREFORMAT_ASTC_6x5_FLOAT = 94,
        SDL_GPU_TEXTUREFORMAT_ASTC_6x6_FLOAT = 95,
        SDL_GPU_TEXTUREFORMAT_ASTC_8x5_FLOAT = 96,
        SDL_GPU_TEXTUREFORMAT_ASTC_8x6_FLOAT = 97,
        SDL_GPU_TEXTUREFORMAT_ASTC_8x8_FLOAT = 98,
        SDL_GPU_TEXTUREFORMAT_ASTC_10x5_FLOAT = 99,
        SDL_GPU_TEXTUREFORMAT_ASTC_10x6_FLOAT = 100,
        SDL_GPU_TEXTUREFORMAT_ASTC_10x8_FLOAT = 101,
        SDL_GPU_TEXTUREFORMAT_ASTC_10x10_FLOAT = 102,
        SDL_GPU_TEXTUREFORMAT_ASTC_12x10_FLOAT = 103,
        SDL_GPU_TEXTUREFORMAT_ASTC_12x12_FLOAT = 104,
    }

    [Flags]
    public enum SDL_GPUTextureUsageFlags : uint
    {
        SDL_GPU_TEXTUREUSAGE_SAMPLER = 0x1,
        SDL_GPU_TEXTUREUSAGE_COLOR_TARGET = 0x2,
        SDL_GPU_TEXTUREUSAGE_DEPTH_STENCIL_TARGET = 0x4,
        SDL_GPU_TEXTUREUSAGE_GRAPHICS_STORAGE_READ = 0x08,
        SDL_GPU_TEXTUREUSAGE_COMPUTE_STORAGE_READ = 0x10,
        SDL_GPU_TEXTUREUSAGE_COMPUTE_STORAGE_WRITE = 0x20,
    }

    public enum SDL_GPUTextureType
    {
        SDL_GPU_TEXTURETYPE_2D = 0,
        SDL_GPU_TEXTURETYPE_2D_ARRAY = 1,
        SDL_GPU_TEXTURETYPE_3D = 2,
        SDL_GPU_TEXTURETYPE_CUBE = 3,
        SDL_GPU_TEXTURETYPE_CUBE_ARRAY = 4,
    }

    public enum SDL_GPUSampleCount
    {
        SDL_GPU_SAMPLECOUNT_1 = 0,
        SDL_GPU_SAMPLECOUNT_2 = 1,
        SDL_GPU_SAMPLECOUNT_4 = 2,
        SDL_GPU_SAMPLECOUNT_8 = 3,
    }

    public enum SDL_GPUCubeMapFace
    {
        SDL_GPU_CUBEMAPFACE_POSITIVEX = 0,
        SDL_GPU_CUBEMAPFACE_NEGATIVEX = 1,
        SDL_GPU_CUBEMAPFACE_POSITIVEY = 2,
        SDL_GPU_CUBEMAPFACE_NEGATIVEY = 3,
        SDL_GPU_CUBEMAPFACE_POSITIVEZ = 4,
        SDL_GPU_CUBEMAPFACE_NEGATIVEZ = 5,
    }

    [Flags]
    public enum SDL_GPUBufferUsageFlags : uint
    {
        SDL_GPU_BUFFERUSAGE_VERTEX = 0x1,
        SDL_GPU_BUFFERUSAGE_INDEX = 0x2,
        SDL_GPU_BUFFERUSAGE_INDIRECT = 0x4,
        SDL_GPU_BUFFERUSAGE_GRAPHICS_STORAGE_READ = 0x08,
        SDL_GPU_BUFFERUSAGE_COMPUTE_STORAGE_READ = 0x10,
        SDL_GPU_BUFFERUSAGE_COMPUTE_STORAGE_WRITE = 0x20,
    }

    public enum SDL_GPUTransferBufferUsage
    {
        SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD = 0,
        SDL_GPU_TRANSFERBUFFERUSAGE_DOWNLOAD = 1,
    }

    public enum SDL_GPUShaderStage
    {
        SDL_GPU_SHADERSTAGE_VERTEX = 0,
        SDL_GPU_SHADERSTAGE_FRAGMENT = 1,
    }

    [Flags]
    public enum SDL_GPUShaderFormat : uint
    {
        SDL_GPU_SHADERFORMAT_PRIVATE = 0x1,
        SDL_GPU_SHADERFORMAT_SPIRV = 0x2,
        SDL_GPU_SHADERFORMAT_DXBC = 0x4,
        SDL_GPU_SHADERFORMAT_DXIL = 0x08,
        SDL_GPU_SHADERFORMAT_MSL = 0x10,
        SDL_GPU_SHADERFORMAT_METALLIB = 0x20,
    }

    public enum SDL_GPUVertexElementFormat
    {
        SDL_GPU_VERTEXELEMENTFORMAT_INVALID = 0,
        SDL_GPU_VERTEXELEMENTFORMAT_INT = 1,
        SDL_GPU_VERTEXELEMENTFORMAT_INT2 = 2,
        SDL_GPU_VERTEXELEMENTFORMAT_INT3 = 3,
        SDL_GPU_VERTEXELEMENTFORMAT_INT4 = 4,
        SDL_GPU_VERTEXELEMENTFORMAT_UINT = 5,
        SDL_GPU_VERTEXELEMENTFORMAT_UINT2 = 6,
        SDL_GPU_VERTEXELEMENTFORMAT_UINT3 = 7,
        SDL_GPU_VERTEXELEMENTFORMAT_UINT4 = 8,
        SDL_GPU_VERTEXELEMENTFORMAT_FLOAT = 9,
        SDL_GPU_VERTEXELEMENTFORMAT_FLOAT2 = 10,
        SDL_GPU_VERTEXELEMENTFORMAT_FLOAT3 = 11,
        SDL_GPU_VERTEXELEMENTFORMAT_FLOAT4 = 12,
        SDL_GPU_VERTEXELEMENTFORMAT_BYTE2 = 13,
        SDL_GPU_VERTEXELEMENTFORMAT_BYTE4 = 14,
        SDL_GPU_VERTEXELEMENTFORMAT_UBYTE2 = 15,
        SDL_GPU_VERTEXELEMENTFORMAT_UBYTE4 = 16,
        SDL_GPU_VERTEXELEMENTFORMAT_BYTE2_NORM = 17,
        SDL_GPU_VERTEXELEMENTFORMAT_BYTE4_NORM = 18,
        SDL_GPU_VERTEXELEMENTFORMAT_UBYTE2_NORM = 19,
        SDL_GPU_VERTEXELEMENTFORMAT_UBYTE4_NORM = 20,
        SDL_GPU_VERTEXELEMENTFORMAT_SHORT2 = 21,
        SDL_GPU_VERTEXELEMENTFORMAT_SHORT4 = 22,
        SDL_GPU_VERTEXELEMENTFORMAT_USHORT2 = 23,
        SDL_GPU_VERTEXELEMENTFORMAT_USHORT4 = 24,
        SDL_GPU_VERTEXELEMENTFORMAT_SHORT2_NORM = 25,
        SDL_GPU_VERTEXELEMENTFORMAT_SHORT4_NORM = 26,
        SDL_GPU_VERTEXELEMENTFORMAT_USHORT2_NORM = 27,
        SDL_GPU_VERTEXELEMENTFORMAT_USHORT4_NORM = 28,
        SDL_GPU_VERTEXELEMENTFORMAT_HALF2 = 29,
        SDL_GPU_VERTEXELEMENTFORMAT_HALF4 = 30,
    }

    public enum SDL_GPUVertexInputRate
    {
        SDL_GPU_VERTEXINPUTRATE_VERTEX = 0,
        SDL_GPU_VERTEXINPUTRATE_INSTANCE = 1,
    }

    public enum SDL_GPUFillMode
    {
        SDL_GPU_FILLMODE_FILL = 0,
        SDL_GPU_FILLMODE_LINE = 1,
    }

    public enum SDL_GPUCullMode
    {
        SDL_GPU_CULLMODE_NONE = 0,
        SDL_GPU_CULLMODE_FRONT = 1,
        SDL_GPU_CULLMODE_BACK = 2,
    }

    public enum SDL_GPUFrontFace
    {
        SDL_GPU_FRONTFACE_COUNTER_CLOCKWISE = 0,
        SDL_GPU_FRONTFACE_CLOCKWISE = 1,
    }

    public enum SDL_GPUCompareOp
    {
        SDL_GPU_COMPAREOP_INVALID = 0,
        SDL_GPU_COMPAREOP_NEVER = 1,
        SDL_GPU_COMPAREOP_LESS = 2,
        SDL_GPU_COMPAREOP_EQUAL = 3,
        SDL_GPU_COMPAREOP_LESS_OR_EQUAL = 4,
        SDL_GPU_COMPAREOP_GREATER = 5,
        SDL_GPU_COMPAREOP_NOT_EQUAL = 6,
        SDL_GPU_COMPAREOP_GREATER_OR_EQUAL = 7,
        SDL_GPU_COMPAREOP_ALWAYS = 8,
    }

    public enum SDL_GPUStencilOp
    {
        SDL_GPU_STENCILOP_INVALID = 0,
        SDL_GPU_STENCILOP_KEEP = 1,
        SDL_GPU_STENCILOP_ZERO = 2,
        SDL_GPU_STENCILOP_REPLACE = 3,
        SDL_GPU_STENCILOP_INCREMENT_AND_CLAMP = 4,
        SDL_GPU_STENCILOP_DECREMENT_AND_CLAMP = 5,
        SDL_GPU_STENCILOP_INVERT = 6,
        SDL_GPU_STENCILOP_INCREMENT_AND_WRAP = 7,
        SDL_GPU_STENCILOP_DECREMENT_AND_WRAP = 8,
    }

    public enum SDL_GPUBlendOp
    {
        SDL_GPU_BLENDOP_INVALID = 0,
        SDL_GPU_BLENDOP_ADD = 1,
        SDL_GPU_BLENDOP_SUBTRACT = 2,
        SDL_GPU_BLENDOP_REVERSE_SUBTRACT = 3,
        SDL_GPU_BLENDOP_MIN = 4,
        SDL_GPU_BLENDOP_MAX = 5,
    }

    public enum SDL_GPUBlendFactor
    {
        SDL_GPU_BLENDFACTOR_INVALID = 0,
        SDL_GPU_BLENDFACTOR_ZERO = 1,
        SDL_GPU_BLENDFACTOR_ONE = 2,
        SDL_GPU_BLENDFACTOR_SRC_COLOR = 3,
        SDL_GPU_BLENDFACTOR_ONE_MINUS_SRC_COLOR = 4,
        SDL_GPU_BLENDFACTOR_DST_COLOR = 5,
        SDL_GPU_BLENDFACTOR_ONE_MINUS_DST_COLOR = 6,
        SDL_GPU_BLENDFACTOR_SRC_ALPHA = 7,
        SDL_GPU_BLENDFACTOR_ONE_MINUS_SRC_ALPHA = 8,
        SDL_GPU_BLENDFACTOR_DST_ALPHA = 9,
        SDL_GPU_BLENDFACTOR_ONE_MINUS_DST_ALPHA = 10,
        SDL_GPU_BLENDFACTOR_CONSTANT_COLOR = 11,
        SDL_GPU_BLENDFACTOR_ONE_MINUS_CONSTANT_COLOR = 12,
        SDL_GPU_BLENDFACTOR_SRC_ALPHA_SATURATE = 13,
    }

    [Flags]
    public enum SDL_GPUColorComponentFlags : byte
    {
        SDL_GPU_COLORCOMPONENT_R = 0x1,
        SDL_GPU_COLORCOMPONENT_G = 0x2,
        SDL_GPU_COLORCOMPONENT_B = 0x4,
        SDL_GPU_COLORCOMPONENT_A = 0x08,
    }

    public enum SDL_GPUFilter
    {
        SDL_GPU_FILTER_NEAREST = 0,
        SDL_GPU_FILTER_LINEAR = 1,
    }

    public enum SDL_GPUSamplerMipmapMode
    {
        SDL_GPU_SAMPLERMIPMAPMODE_NEAREST = 0,
        SDL_GPU_SAMPLERMIPMAPMODE_LINEAR = 1,
    }

    public enum SDL_GPUSamplerAddressMode
    {
        SDL_GPU_SAMPLERADDRESSMODE_REPEAT = 0,
        SDL_GPU_SAMPLERADDRESSMODE_MIRRORED_REPEAT = 1,
        SDL_GPU_SAMPLERADDRESSMODE_CLAMP_TO_EDGE = 2,
    }

    public enum SDL_GPUPresentMode
    {
        SDL_GPU_PRESENTMODE_VSYNC = 0,
        SDL_GPU_PRESENTMODE_IMMEDIATE = 1,
        SDL_GPU_PRESENTMODE_MAILBOX = 2,
    }

    public enum SDL_GPUSwapchainComposition
    {
        SDL_GPU_SWAPCHAINCOMPOSITION_SDR = 0,
        SDL_GPU_SWAPCHAINCOMPOSITION_SDR_LINEAR = 1,
        SDL_GPU_SWAPCHAINCOMPOSITION_HDR_EXTENDED_LINEAR = 2,
        SDL_GPU_SWAPCHAINCOMPOSITION_HDR10_ST2084 = 3,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUViewport
    {
        public float x;
        public float y;
        public float w;
        public float h;
        public float min_depth;
        public float max_depth;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUTextureTransferInfo
    {
        public IntPtr transfer_buffer;
        public uint offset;
        public uint pixels_per_row;
        public uint rows_per_layer;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUTransferBufferLocation
    {
        public IntPtr transfer_buffer;
        public uint offset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUTextureLocation
    {
        public IntPtr texture;
        public uint mip_level;
        public uint layer;
        public uint x;
        public uint y;
        public uint z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUTextureRegion
    {
        public IntPtr texture;
        public uint mip_level;
        public uint layer;
        public uint x;
        public uint y;
        public uint z;
        public uint w;
        public uint h;
        public uint d;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUBlitRegion
    {
        public IntPtr texture;
        public uint mip_level;
        public uint layer_or_depth_plane;
        public uint x;
        public uint y;
        public uint w;
        public uint h;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUBufferLocation
    {
        public IntPtr buffer;
        public uint offset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUBufferRegion
    {
        public IntPtr buffer;
        public uint offset;
        public uint size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUIndirectDrawCommand
    {
        public uint num_vertices;
        public uint num_instances;
        public uint first_vertex;
        public uint first_instance;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUIndexedIndirectDrawCommand
    {
        public uint num_indices;
        public uint num_instances;
        public uint first_index;
        public int vertex_offset;
        public uint first_instance;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUIndirectDispatchCommand
    {
        public uint groupcount_x;
        public uint groupcount_y;
        public uint groupcount_z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUSamplerCreateInfo
    {
        public SDL_GPUFilter min_filter;
        public SDL_GPUFilter mag_filter;
        public SDL_GPUSamplerMipmapMode mipmap_mode;
        public SDL_GPUSamplerAddressMode address_mode_u;
        public SDL_GPUSamplerAddressMode address_mode_v;
        public SDL_GPUSamplerAddressMode address_mode_w;
        public float mip_lod_bias;
        public float max_anisotropy;
        public SDL_GPUCompareOp compare_op;
        public float min_lod;
        public float max_lod;
        public SDLBool enable_anisotropy;
        public SDLBool enable_compare;
        public byte padding1;
        public byte padding2;
        public uint props;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUVertexBufferDescription
    {
        public uint slot;
        public uint pitch;
        public SDL_GPUVertexInputRate input_rate;
        public uint instance_step_rate;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUVertexAttribute
    {
        public uint location;
        public uint buffer_slot;
        public SDL_GPUVertexElementFormat format;
        public uint offset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUVertexInputState
    {
        public SDL_GPUVertexBufferDescription* vertex_buffer_descriptions;
        public uint num_vertex_buffers;
        public SDL_GPUVertexAttribute* vertex_attributes;
        public uint num_vertex_attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUStencilOpState
    {
        public SDL_GPUStencilOp fail_op;
        public SDL_GPUStencilOp pass_op;
        public SDL_GPUStencilOp depth_fail_op;
        public SDL_GPUCompareOp compare_op;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUColorTargetBlendState
    {
        public SDL_GPUBlendFactor src_color_blendfactor;
        public SDL_GPUBlendFactor dst_color_blendfactor;
        public SDL_GPUBlendOp color_blend_op;
        public SDL_GPUBlendFactor src_alpha_blendfactor;
        public SDL_GPUBlendFactor dst_alpha_blendfactor;
        public SDL_GPUBlendOp alpha_blend_op;
        public SDL_GPUColorComponentFlags color_write_mask;
        public SDLBool enable_blend;
        public SDLBool enable_color_write_mask;
        public byte padding1;
        public byte padding2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUShaderCreateInfo
    {
        public UIntPtr code_size;
        public byte* code;
        public byte* entrypoint;
        public SDL_GPUShaderFormat format;
        public SDL_GPUShaderStage stage;
        public uint num_samplers;
        public uint num_storage_textures;
        public uint num_storage_buffers;
        public uint num_uniform_buffers;
        public uint props;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUTextureCreateInfo
    {
        public SDL_GPUTextureType type;
        public SDL_GPUTextureFormat format;
        public SDL_GPUTextureUsageFlags usage;
        public uint width;
        public uint height;
        public uint layer_count_or_depth;
        public uint num_levels;
        public SDL_GPUSampleCount sample_count;
        public uint props;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUBufferCreateInfo
    {
        public SDL_GPUBufferUsageFlags usage;
        public uint size;
        public uint props;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUTransferBufferCreateInfo
    {
        public SDL_GPUTransferBufferUsage usage;
        public uint size;
        public uint props;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPURasterizerState
    {
        public SDL_GPUFillMode fill_mode;
        public SDL_GPUCullMode cull_mode;
        public SDL_GPUFrontFace front_face;
        public float depth_bias_constant_factor;
        public float depth_bias_clamp;
        public float depth_bias_slope_factor;
        public SDLBool enable_depth_bias;
        public SDLBool enable_depth_clip;
        public byte padding1;
        public byte padding2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUMultisampleState
    {
        public SDL_GPUSampleCount sample_count;
        public uint sample_mask;
        public SDLBool enable_mask;
        public byte padding1;
        public byte padding2;
        public byte padding3;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUDepthStencilState
    {
        public SDL_GPUCompareOp compare_op;
        public SDL_GPUStencilOpState back_stencil_state;
        public SDL_GPUStencilOpState front_stencil_state;
        public byte compare_mask;
        public byte write_mask;
        public SDLBool enable_depth_test;
        public SDLBool enable_depth_write;
        public SDLBool enable_stencil_test;
        public byte padding1;
        public byte padding2;
        public byte padding3;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUColorTargetDescription
    {
        public SDL_GPUTextureFormat format;
        public SDL_GPUColorTargetBlendState blend_state;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUGraphicsPipelineTargetInfo
    {
        public SDL_GPUColorTargetDescription* color_target_descriptions;
        public uint num_color_targets;
        public SDL_GPUTextureFormat depth_stencil_format;
        public SDLBool has_depth_stencil_target;
        public byte padding1;
        public byte padding2;
        public byte padding3;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUGraphicsPipelineCreateInfo
    {
        public IntPtr vertex_shader;
        public IntPtr fragment_shader;
        public SDL_GPUVertexInputState vertex_input_state;
        public SDL_GPUPrimitiveType primitive_type;
        public SDL_GPURasterizerState rasterizer_state;
        public SDL_GPUMultisampleState multisample_state;
        public SDL_GPUDepthStencilState depth_stencil_state;
        public SDL_GPUGraphicsPipelineTargetInfo target_info;
        public uint props;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUComputePipelineCreateInfo
    {
        public UIntPtr code_size;
        public byte* code;
        public byte* entrypoint;
        public SDL_GPUShaderFormat format;
        public uint num_samplers;
        public uint num_readonly_storage_textures;
        public uint num_readonly_storage_buffers;
        public uint num_readwrite_storage_textures;
        public uint num_readwrite_storage_buffers;
        public uint num_uniform_buffers;
        public uint threadcount_x;
        public uint threadcount_y;
        public uint threadcount_z;
        public uint props;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUColorTargetInfo
    {
        public IntPtr texture;
        public uint mip_level;
        public uint layer_or_depth_plane;
        public SDL_FColor clear_color;
        public SDL_GPULoadOp load_op;
        public SDL_GPUStoreOp store_op;
        public IntPtr resolve_texture;
        public uint resolve_mip_level;
        public uint resolve_layer;
        public SDLBool cycle;
        public SDLBool cycle_resolve_texture;
        public byte padding1;
        public byte padding2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUDepthStencilTargetInfo
    {
        public IntPtr texture;
        public float clear_depth;
        public SDL_GPULoadOp load_op;
        public SDL_GPUStoreOp store_op;
        public SDL_GPULoadOp stencil_load_op;
        public SDL_GPUStoreOp stencil_store_op;
        public SDLBool cycle;
        public byte clear_stencil;
        public byte padding1;
        public byte padding2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUBlitInfo
    {
        public SDL_GPUBlitRegion source;
        public SDL_GPUBlitRegion destination;
        public SDL_GPULoadOp load_op;
        public SDL_FColor clear_color;
        public SDL_FlipMode flip_mode;
        public SDL_GPUFilter filter;
        public SDLBool cycle;
        public byte padding1;
        public byte padding2;
        public byte padding3;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUBufferBinding
    {
        public IntPtr buffer;
        public uint offset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUTextureSamplerBinding
    {
        public IntPtr texture;
        public IntPtr sampler;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUStorageBufferReadWriteBinding
    {
        public IntPtr buffer;
        public SDLBool cycle;
        public byte padding1;
        public byte padding2;
        public byte padding3;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GPUStorageTextureReadWriteBinding
    {
        public IntPtr texture;
        public uint mip_level;
        public uint layer;
        public SDLBool cycle;
        public byte padding1;
        public byte padding2;
        public byte padding3;
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GPUSupportsShaderFormats(SDL_GPUShaderFormat format_flags, string name);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GPUSupportsProperties(uint props);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateGPUDevice(SDL_GPUShaderFormat format_flags, SDLBool debug_mode, string name);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateGPUDeviceWithProperties(uint props);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DestroyGPUDevice(IntPtr device);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetNumGPUDrivers();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern string SDL_GetGPUDriver(int index);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern string SDL_GetGPUDeviceDriver(IntPtr device);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_GPUShaderFormat SDL_GetGPUShaderFormats(IntPtr device);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateGPUComputePipeline(IntPtr device,
        in SDL_GPUComputePipelineCreateInfo createinfo);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateGPUGraphicsPipeline(IntPtr device,
        in SDL_GPUGraphicsPipelineCreateInfo createinfo);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateGPUSampler(IntPtr device, in SDL_GPUSamplerCreateInfo createinfo);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateGPUShader(IntPtr device, in SDL_GPUShaderCreateInfo createinfo);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateGPUTexture(IntPtr device, in SDL_GPUTextureCreateInfo createinfo);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateGPUBuffer(IntPtr device, in SDL_GPUBufferCreateInfo createinfo);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateGPUTransferBuffer(IntPtr device,
        in SDL_GPUTransferBufferCreateInfo createinfo);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetGPUBufferName(IntPtr device, IntPtr buffer, string text);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetGPUTextureName(IntPtr device, IntPtr texture, string text);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_InsertGPUDebugLabel(IntPtr command_buffer, string text);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_PushGPUDebugGroup(IntPtr command_buffer, string name);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_PopGPUDebugGroup(IntPtr command_buffer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_ReleaseGPUTexture(IntPtr device, IntPtr texture);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_ReleaseGPUSampler(IntPtr device, IntPtr sampler);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_ReleaseGPUBuffer(IntPtr device, IntPtr buffer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_ReleaseGPUTransferBuffer(IntPtr device, IntPtr transfer_buffer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_ReleaseGPUComputePipeline(IntPtr device, IntPtr compute_pipeline);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_ReleaseGPUShader(IntPtr device, IntPtr shader);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_ReleaseGPUGraphicsPipeline(IntPtr device, IntPtr graphics_pipeline);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_AcquireGPUCommandBuffer(IntPtr device);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_PushGPUVertexUniformData(IntPtr command_buffer, uint slot_index, IntPtr data,
        uint length);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_PushGPUFragmentUniformData(IntPtr command_buffer, uint slot_index, IntPtr data,
        uint length);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_PushGPUComputeUniformData(IntPtr command_buffer, uint slot_index, IntPtr data,
        uint length);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_BeginGPURenderPass(IntPtr command_buffer,
        Span<SDL_GPUColorTargetInfo> color_target_infos, uint num_color_targets,
        in SDL_GPUDepthStencilTargetInfo depth_stencil_target_info);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_BindGPUGraphicsPipeline(IntPtr render_pass, IntPtr graphics_pipeline);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetGPUViewport(IntPtr render_pass, in SDL_GPUViewport viewport);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetGPUScissor(IntPtr render_pass, in SDL_Rect scissor);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetGPUBlendConstants(IntPtr render_pass, SDL_FColor blend_constants);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetGPUStencilReference(IntPtr render_pass, byte reference);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_BindGPUVertexBuffers(IntPtr render_pass, uint first_slot,
        Span<SDL_GPUBufferBinding> bindings, uint num_bindings);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_BindGPUIndexBuffer(IntPtr render_pass, in SDL_GPUBufferBinding binding,
        SDL_GPUIndexElementSize index_element_size);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_BindGPUVertexSamplers(IntPtr render_pass, uint first_slot,
        Span<SDL_GPUTextureSamplerBinding> texture_sampler_bindings, uint num_bindings);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_BindGPUVertexStorageTextures(IntPtr render_pass, uint first_slot,
        Span<IntPtr> storage_textures, uint num_bindings);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_BindGPUVertexStorageBuffers(IntPtr render_pass, uint first_slot,
        Span<IntPtr> storage_buffers, uint num_bindings);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_BindGPUFragmentSamplers(IntPtr render_pass, uint first_slot,
        Span<SDL_GPUTextureSamplerBinding> texture_sampler_bindings, uint num_bindings);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_BindGPUFragmentStorageTextures(IntPtr render_pass, uint first_slot,
        Span<IntPtr> storage_textures, uint num_bindings);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_BindGPUFragmentStorageBuffers(IntPtr render_pass, uint first_slot,
        Span<IntPtr> storage_buffers, uint num_bindings);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DrawGPUIndexedPrimitives(IntPtr render_pass, uint num_indices, uint num_instances,
        uint first_index, int vertex_offset, uint first_instance);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DrawGPUPrimitives(IntPtr render_pass, uint num_vertices, uint num_instances,
        uint first_vertex, uint first_instance);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DrawGPUPrimitivesIndirect(IntPtr render_pass, IntPtr buffer, uint offset,
        uint draw_count);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DrawGPUIndexedPrimitivesIndirect(IntPtr render_pass, IntPtr buffer, uint offset,
        uint draw_count);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_EndGPURenderPass(IntPtr render_pass);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_BeginGPUComputePass(IntPtr command_buffer,
        Span<SDL_GPUStorageTextureReadWriteBinding> storage_texture_bindings, uint num_storage_texture_bindings,
        Span<SDL_GPUStorageBufferReadWriteBinding> storage_buffer_bindings, uint num_storage_buffer_bindings);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_BindGPUComputePipeline(IntPtr compute_pass, IntPtr compute_pipeline);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_BindGPUComputeSamplers(IntPtr compute_pass, uint first_slot,
        Span<SDL_GPUTextureSamplerBinding> texture_sampler_bindings, uint num_bindings);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_BindGPUComputeStorageTextures(IntPtr compute_pass, uint first_slot,
        Span<IntPtr> storage_textures, uint num_bindings);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_BindGPUComputeStorageBuffers(IntPtr compute_pass, uint first_slot,
        Span<IntPtr> storage_buffers, uint num_bindings);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DispatchGPUCompute(IntPtr compute_pass, uint groupcount_x, uint groupcount_y,
        uint groupcount_z);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DispatchGPUComputeIndirect(IntPtr compute_pass, IntPtr buffer, uint offset);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_EndGPUComputePass(IntPtr compute_pass);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_MapGPUTransferBuffer(IntPtr device, IntPtr transfer_buffer, SDLBool cycle);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_UnmapGPUTransferBuffer(IntPtr device, IntPtr transfer_buffer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_BeginGPUCopyPass(IntPtr command_buffer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_UploadToGPUTexture(IntPtr copy_pass, in SDL_GPUTextureTransferInfo source,
        in SDL_GPUTextureRegion destination, SDLBool cycle);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_UploadToGPUBuffer(IntPtr copy_pass, in SDL_GPUTransferBufferLocation source,
        in SDL_GPUBufferRegion destination, SDLBool cycle);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_CopyGPUTextureToTexture(IntPtr copy_pass, in SDL_GPUTextureLocation source,
        in SDL_GPUTextureLocation destination, uint w, uint h, uint d, SDLBool cycle);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_CopyGPUBufferToBuffer(IntPtr copy_pass, in SDL_GPUBufferLocation source,
        in SDL_GPUBufferLocation destination, uint size, SDLBool cycle);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DownloadFromGPUTexture(IntPtr copy_pass, in SDL_GPUTextureRegion source,
        in SDL_GPUTextureTransferInfo destination);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DownloadFromGPUBuffer(IntPtr copy_pass, in SDL_GPUBufferRegion source,
        in SDL_GPUTransferBufferLocation destination);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_EndGPUCopyPass(IntPtr copy_pass);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_GenerateMipmapsForGPUTexture(IntPtr command_buffer, IntPtr texture);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_BlitGPUTexture(IntPtr command_buffer, in SDL_GPUBlitInfo info);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WindowSupportsGPUSwapchainComposition(IntPtr device, IntPtr window,
        SDL_GPUSwapchainComposition swapchain_composition);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WindowSupportsGPUPresentMode(IntPtr device, IntPtr window,
        SDL_GPUPresentMode present_mode);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ClaimWindowForGPUDevice(IntPtr device, IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_ReleaseWindowFromGPUDevice(IntPtr device, IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetGPUSwapchainParameters(IntPtr device, IntPtr window,
        SDL_GPUSwapchainComposition swapchain_composition, SDL_GPUPresentMode present_mode);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetGPUAllowedFramesInFlight(IntPtr device, uint allowed_frames_in_flight);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_GPUTextureFormat SDL_GetGPUSwapchainTextureFormat(IntPtr device, IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_AcquireGPUSwapchainTexture(IntPtr command_buffer, IntPtr window,
        out IntPtr swapchain_texture, out uint swapchain_texture_width, out uint swapchain_texture_height);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WaitForGPUSwapchain(IntPtr device, IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WaitAndAcquireGPUSwapchainTexture(IntPtr command_buffer, IntPtr window,
        out IntPtr swapchain_texture, out uint swapchain_texture_width, out uint swapchain_texture_height);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SubmitGPUCommandBuffer(IntPtr command_buffer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_SubmitGPUCommandBufferAndAcquireFence(IntPtr command_buffer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_CancelGPUCommandBuffer(IntPtr command_buffer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WaitForGPUIdle(IntPtr device);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WaitForGPUFences(IntPtr device, SDLBool wait_all, Span<IntPtr> fences,
        uint num_fences);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_QueryGPUFence(IntPtr device, IntPtr fence);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_ReleaseGPUFence(IntPtr device, IntPtr fence);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GPUTextureFormatTexelBlockSize(SDL_GPUTextureFormat format);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GPUTextureSupportsFormat(IntPtr device, SDL_GPUTextureFormat format,
        SDL_GPUTextureType type, SDL_GPUTextureUsageFlags usage);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GPUTextureSupportsSampleCount(IntPtr device, SDL_GPUTextureFormat format,
        SDL_GPUSampleCount sample_count);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_CalculateGPUTextureFormatSize(SDL_GPUTextureFormat format, uint width, uint height,
        uint depth_or_layer_count);
}