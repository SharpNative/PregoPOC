using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azione.Packets
{
    public enum PacketTypes
    {
        CREATE_WINDOW,
        CREATE_WINDOW_RESPONSE,
        INVALIDATE_WINDOW,
        MOUSE_UPDATE,
        CLOSE_WINDOW
    }
}
