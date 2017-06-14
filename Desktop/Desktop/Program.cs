using Azione;
using Azione.Cairo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop
{
    class Program
    {
        private static int width = 1024;
        private static int height = 768 - 35;

        static unsafe void Main(string[] args)
        {
            Window wind = new Window(0, 0, width, height);


            int data = (int)wind.Buffer;

            int surface = Cairo.CreateSurfaceFromData(data, CairoFormat.CAIRO_FORMAT_RGB24, width, height, Cairo.CreateStride(CairoFormat.CAIRO_FORMAT_RGB24, width));

            int context = Cairo.CreateContext(surface);

            int surfaceImg = Cairo.CreateFromPng("test.png");

            Cairo.SetSourceSurface(context, surfaceImg, 0, 0);
            Cairo.Paint(context);
            
            Cairo.DestroyContext(context);
            Cairo.DestroySurface(context);


            wind.Flush();

            Console.ReadLine();
        }
    }
}
