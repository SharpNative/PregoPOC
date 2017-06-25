using Azione;
using Azione.Cairo;
using Azione.Packets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Forms;

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
        private MouseEvent mEvent;
        private MouseEvent mOldEvent;
        private int mCursorSurface;

        private object lockObj = new object();

        public Dictionary<int, CompositorWindow> Windows { get; private set; } = new Dictionary<int, CompositorWindow>();

        private AutoResetEvent mAutoEvent = new AutoResetEvent(true);

        public CompositorWindow mActiveWindow;

        /// <summary>
        /// Compositor cairo surface
        /// </summary>
        public int CairoSurface { get { return mCairoSurface; } }

        private BufferBlock<Rectangle> mDrawQueue = new BufferBlock<Rectangle>();

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
            mCursorSurface = Cairo.CreateFromPng("cursor.png");

            mEvent = new MouseEvent();
            mOldEvent = new MouseEvent();

            DrawBlack();

            Task.Run(() => { DrawThread(); });
        }
        
        private void DrawThread()
        {
            while(true)
            {
                Rectangle rect = mDrawQueue.ReceiveAsync().Result;

                Cairo.Rectangle(mCairoContext, rect.X, rect.Y, rect.Width, rect.Height);
                Cairo.Clip(mCairoContext);

                DrawBlack();


                List<CompositorWindow> windows = Windows.Values.Where(p => rect.IntersectsWith(p.Bounds)).ToList();

                foreach (CompositorWindow window in windows)
                {
                    Cairo.SetSourceSurface(mCairoContext, window.CairoSurface, window.Bounds.X, window.Bounds.Y);
                    Cairo.Paint(mCairoContext);

                    Cairo.FlushSurface(mCairoSurface);
                }

                Cairo.ResetClip(mCairoContext);

                // Paint mouse
                Cairo.SetSourceSurface(mCairoContext, mCursorSurface, mEvent.X, mEvent.Y);
                Cairo.Paint(mCairoContext);

                Redraw();
            }

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
        /// Handle form mouse down
        /// </summary>
        /// <param name="button"></param>
        public void MouseButtonDown(MouseButtons button)
        {
            if (button == MouseButtons.Left)
                mEvent.LeftButton = true;
            else if (button == MouseButtons.Right)
                mEvent.RightButton = true;
            //else if(button == MouseButtons.Middle)

            HandleMouse();

            mOldEvent = mEvent;
        }

        /// <summary>
        /// Handle form mouse up
        /// </summary>
        /// <param name="button"></param>
        public void MouseButtonUp(MouseButtons button)
        {
            if (button == MouseButtons.Left)
                mEvent.LeftButton = false;
            else if (button == MouseButtons.Right)
                mEvent.RightButton = true;

            HandleMouse();

            mOldEvent = mEvent;
        }

        /// <summary>
        /// Handle mouse move
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MouseMove(int x, int y)
        {
            mEvent.X = x;
            mEvent.Y = y;

            HandleMouse();

            mOldEvent = mEvent;
        }

        /// <summary>
        /// Handle compositor mouse
        /// </summary>
        public void HandleMouse()
        {
            DrawArea(new Rectangle(mOldEvent.X, mOldEvent.Y, 20, 32));
            DrawArea(new Rectangle(mEvent.X, mEvent.Y, 20, 32));

            if (mActiveWindow != null)
            {
                mCompositorPacketFS.Sessions[mActiveWindow.ID].SendWindowEvent(PacketTypes.MOUSE_UPDATE, mActiveWindow.ID, mEvent);
            }
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

            mActiveWindow = window;

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
            mDrawQueue.Post(rect);
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
