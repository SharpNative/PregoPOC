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
        private SharedQueue mGeneralQueue;
        private SharedQueue mComQueue;
        private SharedQueue mEventQueue;

        private Dictionary<int, Window> Windows;
        private Dictionary<int, Window> Messages;

        private Task mReadTask;
        private Task mReadEventTask;

        public PacketFSClient()
        {
            mGeneralQueue = new SharedQueue(@"Global\comp", 10);

            Windows = new Dictionary<int, Window>();
            Messages = new Dictionary<int, Window>();

            PacketWriter writer = new PacketWriter();
            writer.Write(1);

            mGeneralQueue.Write(writer.GetBytes());


            byte[] resp = mGeneralQueue.Read();

            PacketReader packet = new PacketReader(resp);


            int pid = packet.ReadInt32();
            
            mComQueue = new SharedQueue(@"Global\comp_" + pid, 4048);
            mEventQueue = new SharedQueue(@"Global\compe_" + pid, 4048);

            mReadTask = new Task(() => ListenForData());
            mReadTask.Start();

            mReadEventTask = new Task(() => ListenForDataEvent());
            mReadEventTask.Start();

        }

        public void RegisterWindow(int wid, Window wind)
        {

            Windows.Add(wid, wind);
        }
        
        public void RegisterMessageID(int mid, Window wind)
        {

            Messages.Add(mid, wind);
        }


        public void ListenForData()
        {
            while (true)
            {
                byte[] buf = mComQueue.Read();

                PacketReader reader = new PacketReader(buf);
                int messageID = reader.ReadInt32();
                int windowID = reader.ReadInt32();

                Window wind;

                if (messageID != -1)
                {
                    wind = Messages[messageID];
                }
                else
                {
                    wind = Windows[windowID];
                }

                wind.HandlePacket(reader);
            }
        }


        public void ListenForDataEvent()
        {
            while (true)
            {
                byte[] buf = mEventQueue.Read();

                PacketReader reader = new PacketReader(buf);
                int messageID = reader.ReadInt32();
                int windowID = reader.ReadInt32();

                Window wind;

                if (messageID != -1)
                {
                    wind = Messages[messageID];
                }
                else
                {
                    wind = Windows[windowID];
                }

                wind.HandlePacket(reader);
            }
        }


        public void Send(PacketTypes type, int messageID, object o)
        {
            PacketWriter writer = new PacketWriter();
            writer.Write(messageID);
            writer.Write((int)type);

            if (o != null)
                writer.WriteObject(o);

            WriteGeneral(writer);
        }

        public void SendInt(PacketTypes type, int windowID, int val)
        {
            PacketWriter writer = new PacketWriter();
            writer.Write(windowID);
            writer.Write((int)type);
            writer.Write(val);

            WriteGeneral(writer);
        }

        public void WriteGeneral(PacketWriter packet)
        {
            mComQueue.Write(packet.GetBytes());
        }
    }
}
