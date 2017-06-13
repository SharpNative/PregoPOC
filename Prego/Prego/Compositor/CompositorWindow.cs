using Azione.Cairo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prego.Compositor
{
    class CompositorWindow
    {
        public int ID { get; set; }

        public Rectangle Bounds { get; set; }

        public int CairoSurface { get; private set; }

        public int GraphicsData { get; private set; }
        
        public CompositorWindow(int x, int y, int width, int height)
        {
            Bounds = new Rectangle(x, y, width, height);

            CairoSurface = Cairo.CreateSurface(CairoFormat.CAIRO_FORMAT_RGB24, width, height);

            GraphicsData = Cairo.GetSurfaceData(CairoSurface);
        }


    }
}
