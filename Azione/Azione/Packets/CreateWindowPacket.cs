﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azione.Packets
{
    [Serializable]
    public struct CreateWindowPacket
    {
        public int X;

        public int Y;

        public int Width;

        public int Height;
    }
}
