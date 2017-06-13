using Azione.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azione
{
    public class PacketFSClient
    {
        public delegate void DataReceivedHandler(byte[] data);

        public DataReceivedHandler onDataReceived { get; set; }


        private SocketClient mClient;

        public PacketFSClient()
        {
            mClient = new SocketClient();
            mClient.DataReceived += new SocketClient.OnReceiveCallBack(dataReceived);
            mClient.Connect("127.0.0.1", 6666);
        }

        private void dataReceived(SocketClient sender, byte[] data)
        {

            onDataReceived?.Invoke(data);
        }

        public void Send(PacketTypes type, object data)
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((int)type);
            writer.WriteObject(data);

            mClient.Send(writer.GetBytes());
        }

        public void SendInt(PacketTypes type, int data)
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((int)type);
            writer.Write(data);

            mClient.Send(writer.GetBytes());
        }
    }
}
