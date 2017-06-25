using Azione;
using Azione.Cairo;
using Azione.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Window
{
    class Program
    {
        private static int width = 1024;
        private static int height =900;

        private static int CairoSurface;
        private static int CairoContext;

        private static Window wind;

        static unsafe void Main(string[] args)
        {
            wind = new Window(0, 768 - height, width, height);
            wind.OnMouseUpdate = new Window.OnMouseUpdateEventHandler(MouseEventHandler);


            int data = (int)wind.Buffer;

            CairoSurface = Cairo.CreateSurfaceFromData(data, CairoFormat.CAIRO_FORMAT_RGB24, width, height, Cairo.CreateStride(CairoFormat.CAIRO_FORMAT_RGB24, width));

            CairoContext = Cairo.CreateContext(CairoSurface);

            Cairo.SetSourceRGB(CairoContext, 0.149, 0.184, 0.231);
            Cairo.Rectangle(CairoContext, 0, 0, width, height);
            Cairo.Fill(CairoContext);




            wind.Flush();

            Console.ReadLine();

            wind.Close();

            System.Threading.Thread.Sleep(5);
        }


        private static void MouseEventHandler(MouseEvent e)
        {
            Console.WriteLine("MOUSE: {0}:{1}:{2}:{3}", e.X, e.Y, e.LeftButton, e.RightButton);
        }
    }
}
