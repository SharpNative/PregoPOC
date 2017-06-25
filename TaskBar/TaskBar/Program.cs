using Azione;
using Azione.Cairo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskBar
{
    class Program
    {
        private static int width = 1024;
        private static int height = 35;

        static unsafe void Main(string[] args)
        {

            Window wind = new Window(0, 768 - height, width, height);


            int data = (int)wind.Buffer;

            int surface = Cairo.CreateSurfaceFromData(data, CairoFormat.CAIRO_FORMAT_RGB24, width, height, Cairo.CreateStride(CairoFormat.CAIRO_FORMAT_RGB24, width));

            int context = Cairo.CreateContext(surface);

            Cairo.SetSourceRGB(context, 0.149, 0.184, 0.231);
            Cairo.Rectangle(context, 0, 0, width, height);
            Cairo.Fill(context);

            int startSurface = Cairo.CreateFromPng("start.png");

            Cairo.SetSourceRGB(context, 1, 1, 1);
            Cairo.Rectangle(context, 8, 8, 20, 20);
            Cairo.Stroke(context);
            
            Cairo.DestroyContext(context);
            Cairo.DestroySurface(context);


            wind.Flush();

            Console.ReadLine();
        }
    }
}
