using Azione.Cairo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prego.Compositor
{
    class Compositor
    {
        private int mCairoSurface;
        private int mWidth;
        private int mHeight;

        private Paint mPaint;
        private CompositorPacketFS mCompositorPacketFS;

        private int mCairoContext;
        private int mCurrentNum = 0;

        public Dictionary<int, CompositorWindow> Windows { get; private set; } = new Dictionary<int, CompositorWindow>();

        /// <summary>
        /// Compositor cairo surface
        /// </summary>
        public int CairoSurface { get { return mCairoSurface; } }

        /// <summary>
        /// Paint event
        /// </summary>
        public delegate void Paint();

        public Compositor(int width, int height)
        {
            mWidth = width;
            mHeight = height;

            mCompositorPacketFS = new CompositorPacketFS(this);

            mCairoSurface = Cairo.CreateSurface(CairoFormat.CAIRO_FORMAT_ARGB32, width, height);

            mCairoContext = Cairo.CreateContext(mCairoSurface);

            DrawBlack();
        }
        /// <summary>
        /// Request new window ID
        /// </summary>
        /// <returns></returns>
        public int RequestWindowID()
        {
            return mCurrentNum++;
        }


        /// <summary>
        /// Draw background black
        /// </summary>
        private void DrawBlack()
        {
            Cairo.SetSourceRGB(mCairoContext, 0, 0, 0);
            Cairo.Rectangle(mCairoContext, 0, 0, mWidth, mHeight);
            Cairo.Fill(mCairoContext);
        }

        /// <summary>
        /// Create a new window
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns></returns>
        public CompositorWindow CreateWindow(int x, int y, int width, int height)
        {

            CompositorWindow window = new CompositorWindow(x, y, width, height);
            window.ID = RequestWindowID();

            Windows.Add(window.ID, window);

            int context = Cairo.CreateContext(window.CairoSurface);

            Cairo.SetSourceRGB(context, 1, 1, 1);
            Cairo.Rectangle(context, 0, 0, mWidth, mHeight);
            Cairo.Fill(context);

            Cairo.DestroyContext(context);

            DrawArea(window.Bounds);

            return window;
        }

        /// <summary>
        /// Remove window by ID
        /// </summary>
        /// <param name="windowID"></param>
        public void RemoveWindow(int windowID)
        {
            CompositorWindow window = Windows[windowID];
            
            Windows.Remove(windowID);

            DrawArea(window.Bounds);
        }

        public void MoveWindow(CompositorWindow window, int x, int y)
        {
            Rectangle oldBounds = window.Bounds;
            window.Bounds = new Rectangle(x, y, window.Bounds.Width, window.Bounds.Height);

            DrawArea(oldBounds);
            DrawArea(window.Bounds);
        }

        /// <summary>
        /// Draw area into screen
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public void DrawArea(Rectangle rect)
        {
            Cairo.Rectangle(mCairoContext, rect.X, rect.Y, rect.Width, rect.Height);
            Cairo.Clip(mCairoContext);

            DrawBlack();
            

            List< CompositorWindow> windows = Windows.Values.Where(p => rect.IntersectsWith(p.Bounds)).ToList();

            foreach(CompositorWindow window in windows)
            {
                Cairo.SetSourceSurface(mCairoContext, window.CairoSurface, window.Bounds.X, window.Bounds.Y);
                Cairo.Paint(mCairoContext);

                Cairo.FlushSurface(mCairoSurface);
            }

            Cairo.ResetClip(mCairoContext);

            Redraw();
        }

        private void Redraw()
        {
            mPaint?.Invoke();
        }

        /// <summary>
        /// Set event to be trigger on paint
        /// </summary>
        /// <param name="p">Paint event</param>
        public void SetPaint(Paint p)
        {
            mPaint = p;
        }
        
    }
}
