using Azione.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using Prego.Net;

namespace Prego.Compositor
{
    /// <summary>
    /// NOTE: FOR NOW IT USES TCP! Sharpen will use its packetfs
    /// </summary>
    class CompositorPacketFS
    {
        private Dictionary<int, PacketFSSession> Sessions = new Dictionary<int, PacketFSSession>();
        private Compositor mComp;

        private int mCurrentNum = 0;
        
        public CompositorPacketFS(Compositor comp)
        {
            mComp = comp;
            TcpConnectionManager manager = new TcpConnectionManager("127.0.0.1", 6666, 200);
            manager.OnNewConnection = new TcpConnectionManager.HandleNewConnectionHandler(newConnectionHandler);
            manager.GetListener().Start();
        }


        /// <summary>
        /// Request window ID
        /// </summary>
        /// <param name="windowID"></param>
        /// <param name="session"></param>
        public void RegisterWindowID(int windowID, PacketFSSession session)
        {
            Sessions = new Dictionary<int, PacketFSSession>();
        }

        /// <summary>
        /// Handle new connection
        /// </summary>
        /// <param name="connection"></param>
        private void newConnectionHandler(TcpConnection connection)
        {
            connection.Session = new PacketFSSession(this, mComp, connection);
        }
    }
}
