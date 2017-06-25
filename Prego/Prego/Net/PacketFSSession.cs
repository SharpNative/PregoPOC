using Azione;
using Azione.Cairo;
using Azione.Packets;
using Prego.Compositor;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Prego.Net
{
    class PacketFSSession
    {
        private CompositorPacketFS mPacketFS;
        private Compositor.Compositor mComp;

        private SharedQueue mSharedQueueGeneral;
        private SharedQueue mSharedQueueEvent;

        private Task mReadTask;

        private int mPid;

        // We can do this faster in sharpen
        private Dictionary<int, CompositorWindow> Windows;

        public PacketFSSession(int pid, CompositorPacketFS packetFS, Compositor.Compositor comp)
        {
            mComp = comp;
            mPid = pid;
            mPacketFS = packetFS;

            mSharedQueueGeneral = new SharedQueue(@"Global\comp_" + mPid, 4048, true);
            mSharedQueueEvent = new SharedQueue(@"Global\compe_" + mPid, 4048, true);

            Windows = new Dictionary<int, CompositorWindow>();

            mReadTask = new Task(() => ListenForData());
            mReadTask.Start();
        }


        public void ListenForData()
        {
            while (true)
            {
                byte[] buf = mSharedQueueGeneral.Read();

                onData(buf);
            }

        }

        public void SendWindowEvent(PacketTypes type, int windowID, object o)
        {
            PacketWriter writer = new PacketWriter();


            // Send window and message ID
            writer.Write(-1);
            writer.Write(windowID);

            writer.Write((int)type);
            writer.WriteObject(o);

            mSharedQueueEvent.Write(writer.GetBytes());
        }

        private void onData(byte[] data)
        {
            PacketReader reader = new PacketReader(data);

            int messageID = reader.ReadInt32();
                
            PacketTypes type = (PacketTypes)reader.ReadInt32();

            switch (type)
            {
                case PacketTypes.CREATE_WINDOW:
                    CreateWindow(messageID, reader.ReadStruct<CreateWindowPacket>());
                    break;

                case PacketTypes.INVALIDATE_WINDOW:
                    InvalidateWindow(reader.ReadInt32());
                    break;

                case PacketTypes.CLOSE_WINDOW:
                    CloseWindow(reader.ReadInt32());
                    break;
            }
            
        }

        /// <summary>
        /// Create new window from packet
        /// </summary>
        /// <param name="windowPacket"></param>
        private void CreateWindow(int messageID, CreateWindowPacket windowPacket)
        {
            var window = mComp.CreateWindow(windowPacket.X, windowPacket.Y, windowPacket.Width, windowPacket.Height);

            mPacketFS.RegisterWindowID(window.ID, this);

            PacketWriter writer = new PacketWriter();
            writer.Write(messageID);

            // We have no current window ID
            writer.Write(-1);
            writer.Write((int)PacketTypes.CREATE_WINDOW_RESPONSE);
            writer.WriteObject(new CreateWindowRespPacket { ID = window.ID });

            mSharedQueueGeneral.Write(writer.GetBytes());

            Windows.Add(window.ID, window);
        }

        /// <summary>
        /// NOTE: this code can be done better when ported!
        /// </summary>
        /// <param name="id"></param>
        private unsafe void InvalidateWindow(int id)
        {
            if (!Windows.Keys.Contains(id))
                return;

            var window = Windows[id];

            int totalSize = window.Bounds.Width * window.Bounds.Height * 4;

            IntPtr data = (IntPtr)Cairo.GetSurfaceData(window.CairoSurface);
            
            using (var consumer = new SharedMemory.BufferReadWrite(@"Global\compd_" + id))
                consumer.Read(data, totalSize, 0);


            byte[] arr = new byte[totalSize];
            Marshal.Copy((IntPtr)data, arr, 0, totalSize);

            mComp.DrawArea(window.Bounds);
        }

        /// <summary>
        /// NOTE: this code can be done better when ported!
        /// </summary>
        /// <param name="id"></param>
        private unsafe void CloseWindow(int id)
        {
            if (!Windows.Keys.Contains(id))
                return;

            var window = Windows[id];

            Windows.Remove(id);
            mComp.Windows.Remove(id);

            mComp.DrawArea(window.Bounds);
        }

        public void Close()
        {
            foreach (var window in Windows)
                mComp.RemoveWindow(window.Value.ID);
        }
    }
}
