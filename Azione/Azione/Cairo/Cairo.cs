using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Azione.Cairo
{
    public class Cairo
    {

        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_win32_surface_create")]
        public static extern int Win32SurfaceCreate(int HDC);

        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_image_surface_create")]
        public static extern int CreateSurface(CairoFormat format, int width, int height);

        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_surface_finish")]
        public static extern void FinishSurface(int surface);

        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_surface_flush")]
        public static extern void FlushSurface(int surface);

        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_surface_destroy")]
        public static extern void DestroySurface(int surface);

        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_create")]
        public static extern int CreateContext(int surface);

        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_set_source_rgb")]
        public static extern void SetSourceRGB(int context, double r, double g, double b);

        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_rectangle")]
        public static extern void Rectangle(int context, double x, double y, double width, double height);

        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_fill")]
        public static extern void Fill(int context);

        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_stroke")]
        public static extern void Stroke(int context);

        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_clip")]
        public static extern void Clip(int context);

        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_paint")]
        public static extern void Paint(int context);

        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_line_to")]
        public static extern void LineTo(int context, double x, double y);

        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_move_to")]
        public static extern void MoveTo(int context, double x, double y);

        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_set_source_surface")]
        public static extern void SetSourceSurface(int context, int surface, double x, double y);

        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_image_surface_get_data")]
        public static extern int GetSurfaceData(int surface);

        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_destroy")]
        public static extern int DestroyContext(int context);

        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_reset_clip")]
        public static extern int ResetClip(int context);
        
        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_image_surface_create_for_data")]
        public static extern int CreateSurfaceFromData(int data, CairoFormat format, int width, int height, int stride);
        
        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_format_stride_for_width")]
        public static extern int CreateStride(CairoFormat Format, int width);

        [DllImport("libcairo-2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "cairo_set_line_width")]
        public static extern int SetLineWidth(int context, int width);
    }

    public enum CairoFormat
    {
        CAIRO_FORMAT_ARGB32,
        CAIRO_FORMAT_RGB24,
        CAIRO_FORMAT_A8,
        CAIRO_FORMAT_A1
    }
}
