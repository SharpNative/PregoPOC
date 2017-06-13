using Prego.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Prego.Net
{
    class TcpConnection
    {
        private readonly int RCV_BUFFER_SIZE = 2048;

        public readonly uint Id;
        public readonly DateTime Created;

        private Socket Socket;

        private byte[] Buffer;

        private AsyncCallback DataReceivedCallback;
        private RouteReceivedDataCallback RouteDataCallback;

        public PacketFSSession Session
        {
            get;
            set;
        }

        public delegate void RouteReceivedDataCallback(ref byte[] Data);

        public int AgeInSeconds
        {
            get
            {
                int s = (int)(DateTime.Now - Created).TotalSeconds;

                if (s < 0)
                {
                    s = 0;
                }

                return s;
            }
        }

        public string IPAddress
        {
            get
            {
                if (Socket == null)
                {
                    return "";
                }

                return Socket.RemoteEndPoint.ToString().Split(':')[0];
            }
        }

        public Boolean IsAlive
        {
            get { return (Socket != null); }
        }

        public TcpConnection(uint Id, Socket Sock)
        {
            this.Id = Id;
            this.Socket = Sock;
            this.Created = DateTime.Now;
        }

        public void Start(RouteReceivedDataCallback DataRouter)
        {
            this.Buffer = new byte[RCV_BUFFER_SIZE];
            this.DataReceivedCallback = new AsyncCallback(DataReceived);
            this.RouteDataCallback = DataRouter;

            WaitForData();
        }

        public Boolean TestConnection()
        {
            try
            {
                return this.Socket.Send(new byte[] { 0 }) > 0;
            }
            catch { return false; }
        }

        private void ConnectionDead()
        {
            try
            {

                Socket.Close();
                Session.Close();

            }
            catch (Exception e)
            {

            }
        }

        public void SendData(byte[] Data)
        {
            if (!this.IsAlive)
            {
                return;
            }

            try
            {
                this.Socket.Send(Data);
            }
            catch (SocketException)
            {
                ConnectionDead();
            }
            catch (ObjectDisposedException)
            {
                ConnectionDead();
            }

            catch (Exception e)
            {
            }
        }
        
        public Socket GetSocket()
        {
            return this.Socket;
        }

        private void WaitForData()
        {
            if (this.IsAlive)
            {
                try
                {
                    Socket.BeginReceive(this.Buffer, 0, RCV_BUFFER_SIZE, SocketFlags.None, DataReceivedCallback, null);
                }

                catch (SocketException)
                {
                    ConnectionDead();
                }

                catch (ObjectDisposedException)
                {
                    ConnectionDead();
                }

                catch (Exception e)
                {
                    ConnectionDead();
                }
            }
        }

        private void DataReceived(IAsyncResult iAr)
        {
            if (!this.IsAlive)
            {
                return;
            }

            int rcvBytesCount = 0;

            try
            {
                rcvBytesCount = Socket.EndReceive(iAr);
            }

            catch (ObjectDisposedException)
            {
                ConnectionDead();
                return;
            }

            catch (Exception e)
            {
                ConnectionDead();
                return;
            }

            if (rcvBytesCount < 1)
            {
                ConnectionDead();
                return;
            }

            if (rcvBytesCount > 0)
            {
                byte[] toProcess = ByteUtil.ChompBytes(Buffer, 0, rcvBytesCount);
                RouteData(ref toProcess);
            }

            WaitForData();

        }

        private void RouteData(ref Byte[] Data)
        {
            if (this.RouteDataCallback != null)
            {
                this.RouteDataCallback.Invoke(ref Data);
            }
        }
    }
}
