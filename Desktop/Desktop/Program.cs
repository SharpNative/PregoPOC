﻿using Azione;
using Azione.Cairo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azione.Packets;

namespace Desktop
{
    class Program
    {
        private static int width = 1024;
        private static int height = 768 - 35;

        private static int CairoSurface;
        private static int CairoContext;

        private static Window wind;

        static unsafe void Main(string[] args)
        {
            wind = new Window(0, 0, width, height);
            wind.OnMouseUpdate = new Window.OnMouseUpdateEventHandler(MouseEventHandler);


            int data = (int)wind.Buffer;

            CairoSurface = Cairo.CreateSurfaceFromData(data, CairoFormat.CAIRO_FORMAT_RGB24, width, height, Cairo.CreateStride(CairoFormat.CAIRO_FORMAT_RGB24, width));

            CairoContext = Cairo.CreateContext(CairoSurface);

            int surfaceImg = Cairo.CreateFromPng("bg.png");

            Cairo.SetSourceSurface(CairoContext, surfaceImg, 0, 0);
            Cairo.Paint(CairoContext);

            wind.Flush();

            Console.ReadLine();

            wind.Close();
        }

        private static void MouseEventHandler(MouseEvent e)
        {
            Cairo.SetSourceRGB(CairoContext, 1, 1, 1);
            Cairo.MoveTo(CairoContext, e.X, e.Y);
            Cairo.LineTo(CairoContext, e.X + 2, e.Y + 2);
            Cairo.Stroke(CairoContext);

            wind.Flush();
        }
    }
}
