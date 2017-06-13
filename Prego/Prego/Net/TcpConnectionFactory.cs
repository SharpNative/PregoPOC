using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Prego.Net
{
    class TcpConnectionFactory
    {
        private uint ConnectionCounter;

        public uint Count
        {
            get
            {
                return this.ConnectionCounter;
            }
        }

        public TcpConnection CreateConnection(Socket Sock)
        {
            if (Sock == null)
            {
                return null;
            }

            TcpConnection Connection = new TcpConnection(ConnectionCounter++, Sock);

            return Connection;
        }
    }
}
