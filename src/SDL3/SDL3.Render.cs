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
    public const string SDL_PROP_RENDERER_CREATE_NAME_STRING = "SDL.renderer.create.name";
    public const string SDL_PROP_RENDERER_CREATE_WINDOW_POINTER = "SDL.renderer.create.window";
    public const string SDL_PROP_RENDERER_CREATE_SURFACE_POINTER = "SDL.renderer.create.surface";
    public const string SDL_PROP_RENDERER_CREATE_OUTPUT_COLORSPACE_NUMBER = "SDL.renderer.create.output_colorspace";
    public const string SDL_PROP_RENDERER_CREATE_PRESENT_VSYNC_NUMBER = "SDL.renderer.create.present_vsync";
    public const string SDL_PROP_RENDERER_CREATE_VULKAN_INSTANCE_POINTER = "SDL.renderer.create.vulkan.instance";
    public const string SDL_PROP_RENDERER_CREATE_VULKAN_SURFACE_NUMBER = "SDL.renderer.create.vulkan.surface";

    public const string SDL_PROP_RENDERER_CREATE_VULKAN_PHYSICAL_DEVICE_POINTER =
        "SDL.renderer.create.vulkan.physical_device";

    public const string SDL_PROP_RENDERER_CREATE_VULKAN_DEVICE_POINTER = "SDL.renderer.create.vulkan.device";

    public const string SDL_PROP_RENDERER_CREATE_VULKAN_GRAPHICS_QUEUE_FAMILY_INDEX_NUMBER =
        "SDL.renderer.create.vulkan.graphics_queue_family_index";

    public const string SDL_PROP_RENDERER_CREATE_VULKAN_PRESENT_QUEUE_FAMILY_INDEX_NUMBER =
        "SDL.renderer.create.vulkan.present_queue_family_index";

    public const string SDL_PROP_RENDERER_NAME_STRING = "SDL.renderer.name";
    public const string SDL_PROP_RENDERER_WINDOW_POINTER = "SDL.renderer.window";
    public const string SDL_PROP_RENDERER_SURFACE_POINTER = "SDL.renderer.surface";
    public const string SDL_PROP_RENDERER_VSYNC_NUMBER = "SDL.renderer.vsync";
    public const string SDL_PROP_RENDERER_MAX_TEXTURE_SIZE_NUMBER = "SDL.renderer.max_texture_size";
    public const string SDL_PROP_RENDERER_TEXTURE_FORMATS_POINTER = "SDL.renderer.texture_formats";
    public const string SDL_PROP_RENDERER_OUTPUT_COLORSPACE_NUMBER = "SDL.renderer.output_colorspace";
    public const string SDL_PROP_RENDERER_HDR_ENABLED_BOOLEAN = "SDL.renderer.HDR_enabled";
    public const string SDL_PROP_RENDERER_SDR_WHITE_POINT_FLOAT = "SDL.renderer.SDR_white_point";
    public const string SDL_PROP_RENDERER_HDR_HEADROOM_FLOAT = "SDL.renderer.HDR_headroom";
    public const string SDL_PROP_RENDERER_D3D9_DEVICE_POINTER = "SDL.renderer.d3d9.device";
    public const string SDL_PROP_RENDERER_D3D11_DEVICE_POINTER = "SDL.renderer.d3d11.device";
    public const string SDL_PROP_RENDERER_D3D11_SWAPCHAIN_POINTER = "SDL.renderer.d3d11.swap_chain";
    public const string SDL_PROP_RENDERER_D3D12_DEVICE_POINTER = "SDL.renderer.d3d12.device";
    public const string SDL_PROP_RENDERER_D3D12_SWAPCHAIN_POINTER = "SDL.renderer.d3d12.swap_chain";
    public const string SDL_PROP_RENDERER_D3D12_COMMAND_QUEUE_POINTER = "SDL.renderer.d3d12.command_queue";
    public const string SDL_PROP_RENDERER_VULKAN_INSTANCE_POINTER = "SDL.renderer.vulkan.instance";
    public const string SDL_PROP_RENDERER_VULKAN_SURFACE_NUMBER = "SDL.renderer.vulkan.surface";
    public const string SDL_PROP_RENDERER_VULKAN_PHYSICAL_DEVICE_POINTER = "SDL.renderer.vulkan.physical_device";
    public const string SDL_PROP_RENDERER_VULKAN_DEVICE_POINTER = "SDL.renderer.vulkan.device";

    public const string SDL_PROP_RENDERER_VULKAN_GRAPHICS_QUEUE_FAMILY_INDEX_NUMBER =
        "SDL.renderer.vulkan.graphics_queue_family_index";

    public const string SDL_PROP_RENDERER_VULKAN_PRESENT_QUEUE_FAMILY_INDEX_NUMBER =
        "SDL.renderer.vulkan.present_queue_family_index";

    public const string SDL_PROP_RENDERER_VULKAN_SWAPCHAIN_IMAGE_COUNT_NUMBER =
        "SDL.renderer.vulkan.swapchain_image_count";

    public const string SDL_PROP_RENDERER_GPU_DEVICE_POINTER = "SDL.renderer.gpu.device";
    public const string SDL_PROP_TEXTURE_CREATE_COLORSPACE_NUMBER = "SDL.texture.create.colorspace";
    public const string SDL_PROP_TEXTURE_CREATE_FORMAT_NUMBER = "SDL.texture.create.format";
    public const string SDL_PROP_TEXTURE_CREATE_ACCESS_NUMBER = "SDL.texture.create.access";
    public const string SDL_PROP_TEXTURE_CREATE_WIDTH_NUMBER = "SDL.texture.create.width";
    public const string SDL_PROP_TEXTURE_CREATE_HEIGHT_NUMBER = "SDL.texture.create.height";
    public const string SDL_PROP_TEXTURE_CREATE_SDR_WHITE_POINT_FLOAT = "SDL.texture.create.SDR_white_point";
    public const string SDL_PROP_TEXTURE_CREATE_HDR_HEADROOM_FLOAT = "SDL.texture.create.HDR_headroom";
    public const string SDL_PROP_TEXTURE_CREATE_D3D11_TEXTURE_POINTER = "SDL.texture.create.d3d11.texture";
    public const string SDL_PROP_TEXTURE_CREATE_D3D11_TEXTURE_U_POINTER = "SDL.texture.create.d3d11.texture_u";
    public const string SDL_PROP_TEXTURE_CREATE_D3D11_TEXTURE_V_POINTER = "SDL.texture.create.d3d11.texture_v";
    public const string SDL_PROP_TEXTURE_CREATE_D3D12_TEXTURE_POINTER = "SDL.texture.create.d3d12.texture";
    public const string SDL_PROP_TEXTURE_CREATE_D3D12_TEXTURE_U_POINTER = "SDL.texture.create.d3d12.texture_u";
    public const string SDL_PROP_TEXTURE_CREATE_D3D12_TEXTURE_V_POINTER = "SDL.texture.create.d3d12.texture_v";
    public const string SDL_PROP_TEXTURE_CREATE_METAL_PIXELBUFFER_POINTER = "SDL.texture.create.metal.pixelbuffer";
    public const string SDL_PROP_TEXTURE_CREATE_OPENGL_TEXTURE_NUMBER = "SDL.texture.create.opengl.texture";
    public const string SDL_PROP_TEXTURE_CREATE_OPENGL_TEXTURE_UV_NUMBER = "SDL.texture.create.opengl.texture_uv";
    public const string SDL_PROP_TEXTURE_CREATE_OPENGL_TEXTURE_U_NUMBER = "SDL.texture.create.opengl.texture_u";
    public const string SDL_PROP_TEXTURE_CREATE_OPENGL_TEXTURE_V_NUMBER = "SDL.texture.create.opengl.texture_v";
    public const string SDL_PROP_TEXTURE_CREATE_OPENGLES2_TEXTURE_NUMBER = "SDL.texture.create.opengles2.texture";
    public const string SDL_PROP_TEXTURE_CREATE_OPENGLES2_TEXTURE_UV_NUMBER = "SDL.texture.create.opengles2.texture_uv";
    public const string SDL_PROP_TEXTURE_CREATE_OPENGLES2_TEXTURE_U_NUMBER = "SDL.texture.create.opengles2.texture_u";
    public const string SDL_PROP_TEXTURE_CREATE_OPENGLES2_TEXTURE_V_NUMBER = "SDL.texture.create.opengles2.texture_v";
    public const string SDL_PROP_TEXTURE_CREATE_VULKAN_TEXTURE_NUMBER = "SDL.texture.create.vulkan.texture";
    public const string SDL_PROP_TEXTURE_COLORSPACE_NUMBER = "SDL.texture.colorspace";
    public const string SDL_PROP_TEXTURE_FORMAT_NUMBER = "SDL.texture.format";
    public const string SDL_PROP_TEXTURE_ACCESS_NUMBER = "SDL.texture.access";
    public const string SDL_PROP_TEXTURE_WIDTH_NUMBER = "SDL.texture.width";
    public const string SDL_PROP_TEXTURE_HEIGHT_NUMBER = "SDL.texture.height";
    public const string SDL_PROP_TEXTURE_SDR_WHITE_POINT_FLOAT = "SDL.texture.SDR_white_point";
    public const string SDL_PROP_TEXTURE_HDR_HEADROOM_FLOAT = "SDL.texture.HDR_headroom";
    public const string SDL_PROP_TEXTURE_D3D11_TEXTURE_POINTER = "SDL.texture.d3d11.texture";
    public const string SDL_PROP_TEXTURE_D3D11_TEXTURE_U_POINTER = "SDL.texture.d3d11.texture_u";
    public const string SDL_PROP_TEXTURE_D3D11_TEXTURE_V_POINTER = "SDL.texture.d3d11.texture_v";
    public const string SDL_PROP_TEXTURE_D3D12_TEXTURE_POINTER = "SDL.texture.d3d12.texture";
    public const string SDL_PROP_TEXTURE_D3D12_TEXTURE_U_POINTER = "SDL.texture.d3d12.texture_u";
    public const string SDL_PROP_TEXTURE_D3D12_TEXTURE_V_POINTER = "SDL.texture.d3d12.texture_v";
    public const string SDL_PROP_TEXTURE_OPENGL_TEXTURE_NUMBER = "SDL.texture.opengl.texture";
    public const string SDL_PROP_TEXTURE_OPENGL_TEXTURE_UV_NUMBER = "SDL.texture.opengl.texture_uv";
    public const string SDL_PROP_TEXTURE_OPENGL_TEXTURE_U_NUMBER = "SDL.texture.opengl.texture_u";
    public const string SDL_PROP_TEXTURE_OPENGL_TEXTURE_V_NUMBER = "SDL.texture.opengl.texture_v";
    public const string SDL_PROP_TEXTURE_OPENGL_TEXTURE_TARGET_NUMBER = "SDL.texture.opengl.target";
    public const string SDL_PROP_TEXTURE_OPENGL_TEX_W_FLOAT = "SDL.texture.opengl.tex_w";
    public const string SDL_PROP_TEXTURE_OPENGL_TEX_H_FLOAT = "SDL.texture.opengl.tex_h";
    public const string SDL_PROP_TEXTURE_OPENGLES2_TEXTURE_NUMBER = "SDL.texture.opengles2.texture";
    public const string SDL_PROP_TEXTURE_OPENGLES2_TEXTURE_UV_NUMBER = "SDL.texture.opengles2.texture_uv";
    public const string SDL_PROP_TEXTURE_OPENGLES2_TEXTURE_U_NUMBER = "SDL.texture.opengles2.texture_u";
    public const string SDL_PROP_TEXTURE_OPENGLES2_TEXTURE_V_NUMBER = "SDL.texture.opengles2.texture_v";
    public const string SDL_PROP_TEXTURE_OPENGLES2_TEXTURE_TARGET_NUMBER = "SDL.texture.opengles2.target";
    public const string SDL_PROP_TEXTURE_VULKAN_TEXTURE_NUMBER = "SDL.texture.vulkan.texture";

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Vertex
    {
        public SDL_FPoint position;
        public SDL_FColor color;
        public SDL_FPoint tex_coord;
    }

    public enum SDL_TextureAccess
    {
        SDL_TEXTUREACCESS_STATIC = 0,
        SDL_TEXTUREACCESS_STREAMING = 1,
        SDL_TEXTUREACCESS_TARGET = 2,
    }

    public enum SDL_RendererLogicalPresentation
    {
        SDL_LOGICAL_PRESENTATION_DISABLED = 0,
        SDL_LOGICAL_PRESENTATION_STRETCH = 1,
        SDL_LOGICAL_PRESENTATION_LETTERBOX = 2,
        SDL_LOGICAL_PRESENTATION_OVERSCAN = 3,
        SDL_LOGICAL_PRESENTATION_INTEGER_SCALE = 4,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Texture
    {
        public SDL_PixelFormat format;
        public int w;
        public int h;
        public int refcount;
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetNumRenderDrivers();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern string SDL_GetRenderDriver(int index);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_CreateWindowAndRenderer(string title, int width, int height,
        SDL_WindowFlags window_flags, out IntPtr window, out IntPtr renderer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateRenderer(IntPtr window, string name);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateRendererWithProperties(uint props);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_CreateSoftwareRenderer(IntPtr surface);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetRenderer(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetRenderWindow(IntPtr renderer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern string SDL_GetRendererName(IntPtr renderer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetRendererProperties(IntPtr renderer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetRenderOutputSize(IntPtr renderer, out int w, out int h);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetCurrentRenderOutputSize(IntPtr renderer, out int w, out int h);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Texture* SDL_CreateTexture(IntPtr renderer, SDL_PixelFormat format,
        SDL_TextureAccess access, int w, int h);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Texture* SDL_CreateTextureFromSurface(IntPtr renderer, IntPtr surface);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Texture* SDL_CreateTextureWithProperties(IntPtr renderer, uint props);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetTextureProperties(IntPtr texture);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetRendererFromTexture(IntPtr texture);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetTextureSize(IntPtr texture, out float w, out float h);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetTextureColorMod(IntPtr texture, byte r, byte g, byte b);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetTextureColorModFloat(IntPtr texture, float r, float g, float b);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetTextureColorMod(IntPtr texture, out byte r, out byte g, out byte b);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetTextureColorModFloat(IntPtr texture, out float r, out float g, out float b);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetTextureAlphaMod(IntPtr texture, byte alpha);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetTextureAlphaModFloat(IntPtr texture, float alpha);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetTextureAlphaMod(IntPtr texture, out byte alpha);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetTextureAlphaModFloat(IntPtr texture, out float alpha);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetTextureBlendMode(IntPtr texture, uint blendMode);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetTextureBlendMode(IntPtr texture, IntPtr blendMode);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetTextureScaleMode(IntPtr texture, SDL_ScaleMode scaleMode);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetTextureScaleMode(IntPtr texture, out SDL_ScaleMode scaleMode);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_UpdateTexture(IntPtr texture, ref SDL_Rect rect, IntPtr pixels, int pitch);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_UpdateYUVTexture(IntPtr texture, ref SDL_Rect rect, IntPtr Yplane, int Ypitch,
        IntPtr Uplane, int Upitch, IntPtr Vplane, int Vpitch);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_UpdateNVTexture(IntPtr texture, ref SDL_Rect rect, IntPtr Yplane, int Ypitch,
        IntPtr UVplane, int UVpitch);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_LockTexture(IntPtr texture, ref SDL_Rect rect, out IntPtr pixels, out int pitch);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_LockTextureToSurface(IntPtr texture, ref SDL_Rect rect, out IntPtr surface);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_UnlockTexture(IntPtr texture);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetRenderTarget(IntPtr renderer, IntPtr texture);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Texture* SDL_GetRenderTarget(IntPtr renderer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetRenderLogicalPresentation(IntPtr renderer, int w, int h,
        SDL_RendererLogicalPresentation mode);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetRenderLogicalPresentation(IntPtr renderer, out int w, out int h,
        out SDL_RendererLogicalPresentation mode);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetRenderLogicalPresentationRect(IntPtr renderer, out SDL_FRect rect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderCoordinatesFromWindow(IntPtr renderer, float window_x, float window_y,
        out float x, out float y);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderCoordinatesToWindow(IntPtr renderer, float x, float y, out float window_x,
        out float window_y);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_ConvertEventToRenderCoordinates(IntPtr renderer, ref SDL_Event @event);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetRenderViewport(IntPtr renderer, ref SDL_Rect rect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetRenderViewport(IntPtr renderer, out SDL_Rect rect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderViewportSet(IntPtr renderer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetRenderSafeArea(IntPtr renderer, out SDL_Rect rect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetRenderClipRect(IntPtr renderer, ref SDL_Rect rect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetRenderClipRect(IntPtr renderer, out SDL_Rect rect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderClipEnabled(IntPtr renderer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetRenderScale(IntPtr renderer, float scaleX, float scaleY);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetRenderScale(IntPtr renderer, out float scaleX, out float scaleY);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetRenderDrawColor(IntPtr renderer, byte r, byte g, byte b, byte a);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetRenderDrawColorFloat(IntPtr renderer, float r, float g, float b, float a);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool
        SDL_GetRenderDrawColor(IntPtr renderer, out byte r, out byte g, out byte b, out byte a);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetRenderDrawColorFloat(IntPtr renderer, out float r, out float g, out float b,
        out float a);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetRenderColorScale(IntPtr renderer, float scale);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetRenderColorScale(IntPtr renderer, out float scale);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetRenderDrawBlendMode(IntPtr renderer, uint blendMode);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetRenderDrawBlendMode(IntPtr renderer, IntPtr blendMode);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderClear(IntPtr renderer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderPoint(IntPtr renderer, float x, float y);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderPoints(IntPtr renderer, Span<SDL_FPoint> points, int count);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderLine(IntPtr renderer, float x1, float y1, float x2, float y2);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderLines(IntPtr renderer, Span<SDL_FPoint> points, int count);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderRect(IntPtr renderer, ref SDL_FRect rect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderRects(IntPtr renderer, Span<SDL_FRect> rects, int count);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderFillRect(IntPtr renderer, ref SDL_FRect rect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderFillRects(IntPtr renderer, Span<SDL_FRect> rects, int count);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderTexture(IntPtr renderer, IntPtr texture, ref SDL_FRect srcrect,
        ref SDL_FRect dstrect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderTextureRotated(IntPtr renderer, IntPtr texture, ref SDL_FRect srcrect,
        ref SDL_FRect dstrect, double angle, ref SDL_FPoint center, SDL_FlipMode flip);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderTextureAffine(IntPtr renderer, IntPtr texture, in SDL_FRect srcrect,
        in SDL_FPoint origin, in SDL_FPoint right, in SDL_FPoint down);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderTextureTiled(IntPtr renderer, IntPtr texture, ref SDL_FRect srcrect,
        float scale, ref SDL_FRect dstrect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderTexture9Grid(IntPtr renderer, IntPtr texture, ref SDL_FRect srcrect,
        float left_width, float right_width, float top_height, float bottom_height, float scale, ref SDL_FRect dstrect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderGeometry(IntPtr renderer, IntPtr texture, Span<SDL_Vertex> vertices,
        int num_vertices, Span<int> indices, int num_indices);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderGeometryRaw(IntPtr renderer, IntPtr texture, IntPtr xy, int xy_stride,
        IntPtr color, int color_stride, IntPtr uv, int uv_stride, int num_vertices, IntPtr indices, int num_indices,
        int size_indices);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_Surface* SDL_RenderReadPixels(IntPtr renderer, ref SDL_Rect rect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderPresent(IntPtr renderer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DestroyTexture(IntPtr texture);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_DestroyRenderer(IntPtr renderer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_FlushRenderer(IntPtr renderer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetRenderMetalLayer(IntPtr renderer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_GetRenderMetalCommandEncoder(IntPtr renderer);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_AddVulkanRenderSemaphores(IntPtr renderer, uint wait_stage_mask,
        long wait_semaphore, long signal_semaphore);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_SetRenderVSync(IntPtr renderer, int vsync);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_GetRenderVSync(IntPtr renderer, out int vsync);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderDebugText(IntPtr renderer, float x, float y, string str);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLBool SDL_RenderDebugTextFormat(IntPtr renderer, float x, float y, string fmt);
}