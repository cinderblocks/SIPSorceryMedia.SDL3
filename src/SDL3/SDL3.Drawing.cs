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
    public enum SDL_BlendOperation
    {
        SDL_BLENDOPERATION_ADD = 1,
        SDL_BLENDOPERATION_SUBTRACT = 2,
        SDL_BLENDOPERATION_REV_SUBTRACT = 3,
        SDL_BLENDOPERATION_MINIMUM = 4,
        SDL_BLENDOPERATION_MAXIMUM = 5,
    }

    public enum SDL_BlendFactor
    {
        SDL_BLENDFACTOR_ZERO = 1,
        SDL_BLENDFACTOR_ONE = 2,
        SDL_BLENDFACTOR_SRC_COLOR = 3,
        SDL_BLENDFACTOR_ONE_MINUS_SRC_COLOR = 4,
        SDL_BLENDFACTOR_SRC_ALPHA = 5,
        SDL_BLENDFACTOR_ONE_MINUS_SRC_ALPHA = 6,
        SDL_BLENDFACTOR_DST_COLOR = 7,
        SDL_BLENDFACTOR_ONE_MINUS_DST_COLOR = 8,
        SDL_BLENDFACTOR_DST_ALPHA = 9,
        SDL_BLENDFACTOR_ONE_MINUS_DST_ALPHA = 10,
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_ComposeCustomBlendMode(SDL_BlendFactor srcColorFactor, SDL_BlendFactor dstColorFactor,
        SDL_BlendOperation colorOperation, SDL_BlendFactor srcAlphaFactor, SDL_BlendFactor dstAlphaFactor,
        SDL_BlendOperation alphaOperation);

    public enum SDL_PixelType
    {
        SDL_PIXELTYPE_UNKNOWN = 0,
        SDL_PIXELTYPE_INDEX1 = 1,
        SDL_PIXELTYPE_INDEX4 = 2,
        SDL_PIXELTYPE_INDEX8 = 3,
        SDL_PIXELTYPE_PACKED8 = 4,
        SDL_PIXELTYPE_PACKED16 = 5,
        SDL_PIXELTYPE_PACKED32 = 6,
        SDL_PIXELTYPE_ARRAYU8 = 7,
        SDL_PIXELTYPE_ARRAYU16 = 8,
        SDL_PIXELTYPE_ARRAYU32 = 9,
        SDL_PIXELTYPE_ARRAYF16 = 10,
        SDL_PIXELTYPE_ARRAYF32 = 11,
        SDL_PIXELTYPE_INDEX2 = 12,
    }

    public enum SDL_BitmapOrder
    {
        SDL_BITMAPORDER_NONE = 0,
        SDL_BITMAPORDER_4321 = 1,
        SDL_BITMAPORDER_1234 = 2,
    }

    public enum SDL_PackedOrder
    {
        SDL_PACKEDORDER_NONE = 0,
        SDL_PACKEDORDER_XRGB = 1,
        SDL_PACKEDORDER_RGBX = 2,
        SDL_PACKEDORDER_ARGB = 3,
        SDL_PACKEDORDER_RGBA = 4,
        SDL_PACKEDORDER_XBGR = 5,
        SDL_PACKEDORDER_BGRX = 6,
        SDL_PACKEDORDER_ABGR = 7,
        SDL_PACKEDORDER_BGRA = 8,
    }

    public enum SDL_ArrayOrder
    {
        SDL_ARRAYORDER_NONE = 0,
        SDL_ARRAYORDER_RGB = 1,
        SDL_ARRAYORDER_RGBA = 2,
        SDL_ARRAYORDER_ARGB = 3,
        SDL_ARRAYORDER_BGR = 4,
        SDL_ARRAYORDER_BGRA = 5,
        SDL_ARRAYORDER_ABGR = 6,
    }

    public enum SDL_PackedLayout
    {
        SDL_PACKEDLAYOUT_NONE = 0,
        SDL_PACKEDLAYOUT_332 = 1,
        SDL_PACKEDLAYOUT_4444 = 2,
        SDL_PACKEDLAYOUT_1555 = 3,
        SDL_PACKEDLAYOUT_5551 = 4,
        SDL_PACKEDLAYOUT_565 = 5,
        SDL_PACKEDLAYOUT_8888 = 6,
        SDL_PACKEDLAYOUT_2101010 = 7,
        SDL_PACKEDLAYOUT_1010102 = 8,
    }

    public enum SDL_PixelFormat
    {
        SDL_PIXELFORMAT_UNKNOWN = 0,
        SDL_PIXELFORMAT_INDEX1LSB = 286261504,
        SDL_PIXELFORMAT_INDEX1MSB = 287310080,
        SDL_PIXELFORMAT_INDEX2LSB = 470811136,
        SDL_PIXELFORMAT_INDEX2MSB = 471859712,
        SDL_PIXELFORMAT_INDEX4LSB = 303039488,
        SDL_PIXELFORMAT_INDEX4MSB = 304088064,
        SDL_PIXELFORMAT_INDEX8 = 318769153,
        SDL_PIXELFORMAT_RGB332 = 336660481,
        SDL_PIXELFORMAT_XRGB4444 = 353504258,
        SDL_PIXELFORMAT_XBGR4444 = 357698562,
        SDL_PIXELFORMAT_XRGB1555 = 353570562,
        SDL_PIXELFORMAT_XBGR1555 = 357764866,
        SDL_PIXELFORMAT_ARGB4444 = 355602434,
        SDL_PIXELFORMAT_RGBA4444 = 356651010,
        SDL_PIXELFORMAT_ABGR4444 = 359796738,
        SDL_PIXELFORMAT_BGRA4444 = 360845314,
        SDL_PIXELFORMAT_ARGB1555 = 355667970,
        SDL_PIXELFORMAT_RGBA5551 = 356782082,
        SDL_PIXELFORMAT_ABGR1555 = 359862274,
        SDL_PIXELFORMAT_BGRA5551 = 360976386,
        SDL_PIXELFORMAT_RGB565 = 353701890,
        SDL_PIXELFORMAT_BGR565 = 357896194,
        SDL_PIXELFORMAT_RGB24 = 386930691,
        SDL_PIXELFORMAT_BGR24 = 390076419,
        SDL_PIXELFORMAT_XRGB8888 = 370546692,
        SDL_PIXELFORMAT_RGBX8888 = 371595268,
        SDL_PIXELFORMAT_XBGR8888 = 374740996,
        SDL_PIXELFORMAT_BGRX8888 = 375789572,
        SDL_PIXELFORMAT_ARGB8888 = 372645892,
        SDL_PIXELFORMAT_RGBA8888 = 373694468,
        SDL_PIXELFORMAT_ABGR8888 = 376840196,
        SDL_PIXELFORMAT_BGRA8888 = 377888772,
        SDL_PIXELFORMAT_XRGB2101010 = 370614276,
        SDL_PIXELFORMAT_XBGR2101010 = 374808580,
        SDL_PIXELFORMAT_ARGB2101010 = 372711428,
        SDL_PIXELFORMAT_ABGR2101010 = 376905732,
        SDL_PIXELFORMAT_RGB48 = 403714054,
        SDL_PIXELFORMAT_BGR48 = 406859782,
        SDL_PIXELFORMAT_RGBA64 = 404766728,
        SDL_PIXELFORMAT_ARGB64 = 405815304,
        SDL_PIXELFORMAT_BGRA64 = 407912456,
        SDL_PIXELFORMAT_ABGR64 = 408961032,
        SDL_PIXELFORMAT_RGB48_FLOAT = 437268486,
        SDL_PIXELFORMAT_BGR48_FLOAT = 440414214,
        SDL_PIXELFORMAT_RGBA64_FLOAT = 438321160,
        SDL_PIXELFORMAT_ARGB64_FLOAT = 439369736,
        SDL_PIXELFORMAT_BGRA64_FLOAT = 441466888,
        SDL_PIXELFORMAT_ABGR64_FLOAT = 442515464,
        SDL_PIXELFORMAT_RGB96_FLOAT = 454057996,
        SDL_PIXELFORMAT_BGR96_FLOAT = 457203724,
        SDL_PIXELFORMAT_RGBA128_FLOAT = 455114768,
        SDL_PIXELFORMAT_ARGB128_FLOAT = 456163344,
        SDL_PIXELFORMAT_BGRA128_FLOAT = 458260496,
        SDL_PIXELFORMAT_ABGR128_FLOAT = 459309072,
        SDL_PIXELFORMAT_YV12 = 842094169,
        SDL_PIXELFORMAT_IYUV = 1448433993,
        SDL_PIXELFORMAT_YUY2 = 844715353,
        SDL_PIXELFORMAT_UYVY = 1498831189,
        SDL_PIXELFORMAT_YVYU = 1431918169,
        SDL_PIXELFORMAT_NV12 = 842094158,
        SDL_PIXELFORMAT_NV21 = 825382478,
        SDL_PIXELFORMAT_P010 = 808530000,
        SDL_PIXELFORMAT_EXTERNAL_OES = 542328143,
        SDL_PIXELFORMAT_RGBA32 = 376840196,
        SDL_PIXELFORMAT_ARGB32 = 377888772,
        SDL_PIXELFORMAT_BGRA32 = 372645892,
        SDL_PIXELFORMAT_ABGR32 = 373694468,
        SDL_PIXELFORMAT_RGBX32 = 374740996,
        SDL_PIXELFORMAT_XRGB32 = 375789572,
        SDL_PIXELFORMAT_BGRX32 = 370546692,
        SDL_PIXELFORMAT_XBGR32 = 371595268,
    }

    public enum SDL_ColorType
    {
        SDL_COLOR_TYPE_UNKNOWN = 0,
        SDL_COLOR_TYPE_RGB = 1,
        SDL_COLOR_TYPE_YCBCR = 2,
    }

    public enum SDL_ColorRange
    {
        SDL_COLOR_RANGE_UNKNOWN = 0,
        SDL_COLOR_RANGE_LIMITED = 1,
        SDL_COLOR_RANGE_FULL = 2,
    }

    public enum SDL_ColorPrimaries
    {
        SDL_COLOR_PRIMARIES_UNKNOWN = 0,
        SDL_COLOR_PRIMARIES_BT709 = 1,
        SDL_COLOR_PRIMARIES_UNSPECIFIED = 2,
        SDL_COLOR_PRIMARIES_BT470M = 4,
        SDL_COLOR_PRIMARIES_BT470BG = 5,
        SDL_COLOR_PRIMARIES_BT601 = 6,
        SDL_COLOR_PRIMARIES_SMPTE240 = 7,
        SDL_COLOR_PRIMARIES_GENERIC_FILM = 8,
        SDL_COLOR_PRIMARIES_BT2020 = 9,
        SDL_COLOR_PRIMARIES_XYZ = 10,
        SDL_COLOR_PRIMARIES_SMPTE431 = 11,
        SDL_COLOR_PRIMARIES_SMPTE432 = 12,
        SDL_COLOR_PRIMARIES_EBU3213 = 22,
        SDL_COLOR_PRIMARIES_CUSTOM = 31,
    }

    public enum SDL_TransferCharacteristics
    {
        SDL_TRANSFER_CHARACTERISTICS_UNKNOWN = 0,
        SDL_TRANSFER_CHARACTERISTICS_BT709 = 1,
        SDL_TRANSFER_CHARACTERISTICS_UNSPECIFIED = 2,
        SDL_TRANSFER_CHARACTERISTICS_GAMMA22 = 4,
        SDL_TRANSFER_CHARACTERISTICS_GAMMA28 = 5,
        SDL_TRANSFER_CHARACTERISTICS_BT601 = 6,
        SDL_TRANSFER_CHARACTERISTICS_SMPTE240 = 7,
        SDL_TRANSFER_CHARACTERISTICS_LINEAR = 8,
        SDL_TRANSFER_CHARACTERISTICS_LOG100 = 9,
        SDL_TRANSFER_CHARACTERISTICS_LOG100_SQRT10 = 10,
        SDL_TRANSFER_CHARACTERISTICS_IEC61966 = 11,
        SDL_TRANSFER_CHARACTERISTICS_BT1361 = 12,
        SDL_TRANSFER_CHARACTERISTICS_SRGB = 13,
        SDL_TRANSFER_CHARACTERISTICS_BT2020_10BIT = 14,
        SDL_TRANSFER_CHARACTERISTICS_BT2020_12BIT = 15,
        SDL_TRANSFER_CHARACTERISTICS_PQ = 16,
        SDL_TRANSFER_CHARACTERISTICS_SMPTE428 = 17,
        SDL_TRANSFER_CHARACTERISTICS_HLG = 18,
        SDL_TRANSFER_CHARACTERISTICS_CUSTOM = 31,
    }

    public enum SDL_MatrixCoefficients
    {
        SDL_MATRIX_COEFFICIENTS_IDENTITY = 0,
        SDL_MATRIX_COEFFICIENTS_BT709 = 1,
        SDL_MATRIX_COEFFICIENTS_UNSPECIFIED = 2,
        SDL_MATRIX_COEFFICIENTS_FCC = 4,
        SDL_MATRIX_COEFFICIENTS_BT470BG = 5,
        SDL_MATRIX_COEFFICIENTS_BT601 = 6,
        SDL_MATRIX_COEFFICIENTS_SMPTE240 = 7,
        SDL_MATRIX_COEFFICIENTS_YCGCO = 8,
        SDL_MATRIX_COEFFICIENTS_BT2020_NCL = 9,
        SDL_MATRIX_COEFFICIENTS_BT2020_CL = 10,
        SDL_MATRIX_COEFFICIENTS_SMPTE2085 = 11,
        SDL_MATRIX_COEFFICIENTS_CHROMA_DERIVED_NCL = 12,
        SDL_MATRIX_COEFFICIENTS_CHROMA_DERIVED_CL = 13,
        SDL_MATRIX_COEFFICIENTS_ICTCP = 14,
        SDL_MATRIX_COEFFICIENTS_CUSTOM = 31,
    }

    public enum SDL_ChromaLocation
    {
        SDL_CHROMA_LOCATION_NONE = 0,
        SDL_CHROMA_LOCATION_LEFT = 1,
        SDL_CHROMA_LOCATION_CENTER = 2,
        SDL_CHROMA_LOCATION_TOPLEFT = 3,
    }

    public enum SDL_Colorspace
    {
        SDL_COLORSPACE_UNKNOWN = 0,
        SDL_COLORSPACE_SRGB = 301991328,
        SDL_COLORSPACE_SRGB_LINEAR = 301991168,
        SDL_COLORSPACE_HDR10 = 301999616,
        SDL_COLORSPACE_JPEG = 570426566,
        SDL_COLORSPACE_BT601_LIMITED = 554703046,
        SDL_COLORSPACE_BT601_FULL = 571480262,
        SDL_COLORSPACE_BT709_LIMITED = 554697761,
        SDL_COLORSPACE_BT709_FULL = 571474977,
        SDL_COLORSPACE_BT2020_LIMITED = 554706441,
        SDL_COLORSPACE_BT2020_FULL = 571483657,
        SDL_COLORSPACE_RGB_DEFAULT = 301991328,
        SDL_COLORSPACE_YUV_DEFAULT = 570426566,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Color
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_FColor
    {
        public float r;
        public float g;
        public float b;
        public float a;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Palette
    {
        public int ncolors;
        public SDL_Color* colors;
        public uint version;
        public int refcount;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_PixelFormatDetails
    {
        public SDL_PixelFormat format;
        public byte bits_per_pixel;
        public byte bytes_per_pixel;
        public fixed byte padding[2];
        public uint Rmask;
        public uint Gmask;
        public uint Bmask;
        public uint Amask;
        public byte Rbits;
        public byte Gbits;
        public byte Bbits;
        public byte Abits;
        public byte Rshift;
        public byte Gshift;
        public byte Bshift;
        public byte Ashift;
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern string SDL_GetPixelFormatName(SDL_PixelFormat format);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetMasksForPixelFormat(SDL_PixelFormat format, out int bpp, out uint Rmask,
        out uint Gmask, out uint Bmask, out uint Amask);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_PixelFormat SDL_GetPixelFormatForMasks(int bpp, uint Rmask, uint Gmask, uint Bmask,
        uint Amask);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_PixelFormatDetails* SDL_GetPixelFormatDetails(SDL_PixelFormat format);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Palette* SDL_CreatePalette(int ncolors);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetPaletteColors(IntPtr palette, Span<SDL_Color> colors, int firstcolor,
        int ncolors);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DestroyPalette(IntPtr palette);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_MapRGB(IntPtr format, IntPtr palette, byte r, byte g, byte b);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_MapRGBA(IntPtr format, IntPtr palette, byte r, byte g, byte b, byte a);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_GetRGB(uint pixel, IntPtr format, IntPtr palette, out byte r, out byte g, out byte b);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_GetRGBA(uint pixel, IntPtr format, IntPtr palette, out byte r, out byte g, out byte b,
        out byte a);

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Point
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_FPoint
    {
        public float x;
        public float y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Rect
    {
        public int x;
        public int y;
        public int w;
        public int h;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_FRect
    {
        public float x;
        public float y;
        public float w;
        public float h;
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasRectIntersection(ref SDL_Rect A, ref SDL_Rect B);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetRectIntersection(ref SDL_Rect A, ref SDL_Rect B, out SDL_Rect result);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetRectUnion(ref SDL_Rect A, ref SDL_Rect B, out SDL_Rect result);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetRectEnclosingPoints(Span<SDL_Point> points, int count, ref SDL_Rect clip,
        out SDL_Rect result);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetRectAndLineIntersection(ref SDL_Rect rect, ref int X1, ref int Y1, ref int X2,
        ref int Y2);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_HasRectIntersectionFloat(ref SDL_FRect A, ref SDL_FRect B);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetRectIntersectionFloat(ref SDL_FRect A, ref SDL_FRect B, out SDL_FRect result);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetRectUnionFloat(ref SDL_FRect A, ref SDL_FRect B, out SDL_FRect result);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetRectEnclosingPointsFloat(Span<SDL_FPoint> points, int count, ref SDL_FRect clip,
        out SDL_FRect result);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetRectAndLineIntersectionFloat(ref SDL_FRect rect, ref float X1, ref float Y1,
        ref float X2, ref float Y2);

    public const string SDL_PROP_SURFACE_SDR_WHITE_POINT_FLOAT = "SDL.surface.SDR_white_point";
    public const string SDL_PROP_SURFACE_HDR_HEADROOM_FLOAT = "SDL.surface.HDR_headroom";
    public const string SDL_PROP_SURFACE_TONEMAP_OPERATOR_STRING = "SDL.surface.tonemap";
    public const string SDL_PROP_SURFACE_HOTSPOT_X_NUMBER = "SDL.surface.hotspot.x";
    public const string SDL_PROP_SURFACE_HOTSPOT_Y_NUMBER = "SDL.surface.hotspot.y";

    [Flags]
    public enum SDL_SurfaceFlags : uint
    {
        SDL_SURFACE_PREALLOCATED = 0x1,
        SDL_SURFACE_LOCK_NEEDED = 0x2,
        SDL_SURFACE_LOCKED = 0x4,
        SDL_SURFACE_SIMD_ALIGNED = 0x08,
    }

    public enum SDL_ScaleMode
    {
        SDL_SCALEMODE_NEAREST = 0,
        SDL_SCALEMODE_LINEAR = 1,
    }

    public enum SDL_FlipMode
    {
        SDL_FLIP_NONE = 0,
        SDL_FLIP_HORIZONTAL = 1,
        SDL_FLIP_VERTICAL = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Surface
    {
        public SDL_SurfaceFlags flags;
        public SDL_PixelFormat format;
        public int w;
        public int h;
        public int pitch;
        public IntPtr pixels;
        public int refcount;
        public IntPtr reserved;
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Surface* SDL_CreateSurface(int width, int height, SDL_PixelFormat format);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Surface* SDL_CreateSurfaceFrom(int width, int height, SDL_PixelFormat format,
        IntPtr pixels, int pitch);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DestroySurface(IntPtr surface);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetSurfaceProperties(IntPtr surface);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetSurfaceColorspace(IntPtr surface, SDL_Colorspace colorspace);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Colorspace SDL_GetSurfaceColorspace(IntPtr surface);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Palette* SDL_CreateSurfacePalette(IntPtr surface);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetSurfacePalette(IntPtr surface, IntPtr palette);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Palette* SDL_GetSurfacePalette(IntPtr surface);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_AddSurfaceAlternateImage(IntPtr surface, IntPtr image);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SurfaceHasAlternateImages(IntPtr surface);

    public static Span<IntPtr> SDL_GetSurfaceImages(IntPtr surface)
    {
        var result = SDL_GetSurfaceImages(surface, out var count);
        return new Span<IntPtr>((void*)result, count);
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetSurfaceImages(IntPtr surface, out int count);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_RemoveSurfaceAlternateImages(IntPtr surface);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_LockSurface(IntPtr surface);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_UnlockSurface(IntPtr surface);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Surface* SDL_LoadBMP_IO(IntPtr src, SDLBool closeio);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Surface* SDL_LoadBMP(string file);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SaveBMP_IO(IntPtr surface, IntPtr dst, SDLBool closeio);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SaveBMP(IntPtr surface, string file);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetSurfaceRLE(IntPtr surface, SDLBool enabled);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SurfaceHasRLE(IntPtr surface);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetSurfaceColorKey(IntPtr surface, SDLBool enabled, uint key);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SurfaceHasColorKey(IntPtr surface);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetSurfaceColorKey(IntPtr surface, out uint key);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetSurfaceColorMod(IntPtr surface, byte r, byte g, byte b);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetSurfaceColorMod(IntPtr surface, out byte r, out byte g, out byte b);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetSurfaceAlphaMod(IntPtr surface, byte alpha);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetSurfaceAlphaMod(IntPtr surface, out byte alpha);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetSurfaceBlendMode(IntPtr surface, uint blendMode);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetSurfaceBlendMode(IntPtr surface, IntPtr blendMode);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetSurfaceClipRect(IntPtr surface, ref SDL_Rect rect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetSurfaceClipRect(IntPtr surface, out SDL_Rect rect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_FlipSurface(IntPtr surface, SDL_FlipMode flip);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Surface* SDL_DuplicateSurface(IntPtr surface);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Surface* SDL_ScaleSurface(IntPtr surface, int width, int height, SDL_ScaleMode scaleMode);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Surface* SDL_ConvertSurface(IntPtr surface, SDL_PixelFormat format);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Surface* SDL_ConvertSurfaceAndColorspace(IntPtr surface, SDL_PixelFormat format,
        IntPtr palette, SDL_Colorspace colorspace, uint props);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ConvertPixels(int width, int height, SDL_PixelFormat src_format, IntPtr src,
        int src_pitch, SDL_PixelFormat dst_format, IntPtr dst, int dst_pitch);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ConvertPixelsAndColorspace(int width, int height, SDL_PixelFormat src_format,
        SDL_Colorspace src_colorspace, uint src_properties, IntPtr src, int src_pitch, SDL_PixelFormat dst_format,
        SDL_Colorspace dst_colorspace, uint dst_properties, IntPtr dst, int dst_pitch);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_PremultiplyAlpha(int width, int height, SDL_PixelFormat src_format, IntPtr src,
        int src_pitch, SDL_PixelFormat dst_format, IntPtr dst, int dst_pitch, SDLBool linear);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_PremultiplySurfaceAlpha(IntPtr surface, SDLBool linear);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ClearSurface(IntPtr surface, float r, float g, float b, float a);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_FillSurfaceRect(IntPtr dst, IntPtr rect, uint color);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_FillSurfaceRects(IntPtr dst, Span<SDL_Rect> rects, int count, uint color);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_BlitSurface(IntPtr src, IntPtr srcrect, IntPtr dst, IntPtr dstrect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_BlitSurfaceUnchecked(IntPtr src, IntPtr srcrect, IntPtr dst, IntPtr dstrect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_BlitSurfaceScaled(IntPtr src, IntPtr srcrect, IntPtr dst, IntPtr dstrect,
        SDL_ScaleMode scaleMode);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_BlitSurfaceUncheckedScaled(IntPtr src, IntPtr srcrect, IntPtr dst, IntPtr dstrect,
        SDL_ScaleMode scaleMode);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_BlitSurfaceTiled(IntPtr src, IntPtr srcrect, IntPtr dst, IntPtr dstrect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_BlitSurfaceTiledWithScale(IntPtr src, IntPtr srcrect, float scale,
        SDL_ScaleMode scaleMode, IntPtr dst, IntPtr dstrect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_BlitSurface9Grid(IntPtr src, IntPtr srcrect, int left_width, int right_width,
        int top_height, int bottom_height, float scale, SDL_ScaleMode scaleMode, IntPtr dst, IntPtr dstrect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_MapSurfaceRGB(IntPtr surface, byte r, byte g, byte b);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_MapSurfaceRGBA(IntPtr surface, byte r, byte g, byte b, byte a);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ReadSurfacePixel(IntPtr surface, int x, int y, out byte r, out byte g, out byte b,
        out byte a);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ReadSurfacePixelFloat(IntPtr surface, int x, int y, out float r, out float g,
        out float b, out float a);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WriteSurfacePixel(IntPtr surface, int x, int y, byte r, byte g, byte b, byte a);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_WriteSurfacePixelFloat(IntPtr surface, int x, int y, float r, float g, float b,
        float a);

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_CameraSpec
    {
        public SDL_PixelFormat format;
        public SDL_Colorspace colorspace;
        public int width;
        public int height;
        public int framerate_numerator;
        public int framerate_denominator;
    }

    public enum SDL_CameraPosition
    {
        SDL_CAMERA_POSITION_UNKNOWN = 0,
        SDL_CAMERA_POSITION_FRONT_FACING = 1,
        SDL_CAMERA_POSITION_BACK_FACING = 2,
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetNumCameraDrivers();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern string SDL_GetCameraDriver(int index);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern string SDL_GetCurrentCameraDriver();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetCameras(out int count);

    public static Span<IntPtr> SDL_GetCameraSupportedFormats(uint devid)
    {
        var result = SDL_GetCameraSupportedFormats(devid, out var count);
        return new Span<IntPtr>((void*)result, count);
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetCameraSupportedFormats(uint devid, out int count);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern string SDL_GetCameraName(uint instance_id);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_CameraPosition SDL_GetCameraPosition(uint instance_id);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_OpenCamera(uint instance_id, ref SDL_CameraSpec spec);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetCameraPermissionState(IntPtr camera);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetCameraID(IntPtr camera);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetCameraProperties(IntPtr camera);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetCameraFormat(IntPtr camera, out SDL_CameraSpec spec);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Surface* SDL_AcquireCameraFrame(IntPtr camera, out ulong timestampNS);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_ReleaseCameraFrame(IntPtr camera, IntPtr frame);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_CloseCamera(IntPtr camera);
}