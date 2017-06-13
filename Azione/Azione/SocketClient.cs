using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Azione
{
    class SocketClient
    {
        byte[] buffer = new byte[51200];

        internal delegate void OnReceiveCallBack(SocketClient sender, byte[] data);
        internal event OnReceiveCallBack DataReceived;

        Socket socket;

        internal bool Connected
        {
            get
            {
                if (socket != null)
                    return socket.Connected;

                return false;
            }
        }

        internal SocketClient()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        internal bool Connect(string ip, int port)
        {
            try
            {
                socket.Connect(ip, port);
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallBack, null);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        internal void Send(byte[] data)
        {
            socket.Send(data);
        }

        internal void ReceiveCallBack(IAsyncResult iar)
        {
            //try
            //{
            int rec = socket.EndReceive(iar);

            if (rec == 0)
            {
                // Sumthings fcked
                return;
            }

            if (rec != -4)
            {

            }

            if (DataReceived != null)
            {

                byte[] toProcess = ChompBytes(buffer, 0, rec);
                DataReceived.Invoke(this, toProcess);
            }

            buffer = new byte[51200];
            //}
            //catch (Exception e)
            //{
            //    var test = "";
            //}

            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallBack, null);
        }

        public static byte[] ChompBytes(byte[] bzBytes, int Offset, int numBytes)
        {
            int End = (Offset + numBytes);
            if (End > bzBytes.Length)
                End = bzBytes.Length;

            int chunkLength = End - numBytes;
            if (numBytes > bzBytes.Length)
                numBytes = bzBytes.Length;
            if (numBytes < 0)
                numBytes = 0;

            byte[] bzChunk = new byte[numBytes];
            for (int x = 0; x < numBytes; x++)
            {
                bzChunk[x] = bzBytes[Offset++];
            }

            return bzChunk;
        }
    }
}
