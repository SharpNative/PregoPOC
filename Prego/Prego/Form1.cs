﻿using Azione.Cairo;
using Prego.Compositor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Prego
{
    public partial class Form1 : Form
    {
        private Compositor.Compositor mCompsitor;


        public Form1()
        {
            InitializeComponent();
            
            this.ClientSize = new Size(1024, 768);

            mCompsitor = new Compositor.Compositor(1024, 768);
            mCompsitor.SetPaint(new Compositor.Compositor.Paint(Redraw));
            

            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
        }

        private void Redraw()
        {
            Invoke((MethodInvoker)delegate {
                Refresh();
            });
        }
        

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int surface = Cairo.Win32SurfaceCreate((int)e.Graphics.GetHdc());

            int context = Cairo.CreateContext(surface);

            Cairo.SetSourceSurface(context, mCompsitor.CairoSurface, 0, 0);
            Cairo.Paint(context);
                
            Cairo.DestroySurface(surface);

            e.Graphics.ReleaseHdc();
        }
    }
}
