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
        private TcpConnection mConnection;
        private CompositorPacketFS mPacketFS;
        private Compositor.Compositor mComp;

        // We can do this faster in sharpen
        private Dictionary<int, CompositorWindow> Windows;

        public PacketFSSession(CompositorPacketFS packetFS, Compositor.Compositor comp, TcpConnection connection)
        {
            mComp = comp;
            mConnection = connection;
            mPacketFS = packetFS;

            connection.Start(new TcpConnection.RouteReceivedDataCallback(onData));
            Windows = new Dictionary<int, CompositorWindow>();
        }


        private void onData(ref byte[] data)
        {
            PacketReader[] test = PacketReader.get(ref data);

            foreach (var reader in test)
            {;

                int size = reader.ReadInt32();


                PacketTypes type = (PacketTypes)reader.ReadInt32();

                switch (type)
                {
                    case PacketTypes.CREATE_WINDOW:
                        CreateWindow(reader.ReadStruct<CreateWindowPacket>());
                        break;

                    case PacketTypes.INVALIDATE_WINDOW:
                        InvalidateWindow(reader.ReadInt32());
                        break;
                }
            }
        }

        /// <summary>
        /// Create new window from packet
        /// </summary>
        /// <param name="windowPacket"></param>
        private void CreateWindow(CreateWindowPacket windowPacket)
        {
            var window = mComp.CreateWindow(windowPacket.X, windowPacket.Y, windowPacket.Width, windowPacket.Height);

            mPacketFS.RegisterWindowID(window.ID, this);

            PacketWriter writer = new PacketWriter();
            writer.Write((int)PacketTypes.CREATE_WINDOW_RESPONSE);
            writer.WriteObject(new CreateWindowRespPacket { ID = window.ID });

            mConnection.SendData(writer.GetBytes());

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
            
            using (var consumer = new SharedMemory.BufferReadWrite(@"Global\comp_" + id))
                consumer.Read(data, totalSize, 0);


            byte[] arr = new byte[totalSize];
            Marshal.Copy((IntPtr)data, arr, 0, totalSize);

            mComp.DrawArea(window.Bounds);
        }

        public void Close()
        {
            foreach (var window in Windows)
                mComp.RemoveWindow(window.Value.ID);
        }
    }
}
