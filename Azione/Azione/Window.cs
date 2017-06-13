using Azione.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Azione
{
    public unsafe class Window
    {

        private PacketFSClient mPacketFS;

        public int WindowID { get; private set; } = -1;

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int X { get; private set; }

        public int Y { get; private set; }

        public byte *Buffer { get; private set; }

        private int mBufSize;

        private SharedMemory.BufferReadWrite mSharedMemory;
        
        private System.Threading.EventWaitHandle mWindowCreated = new System.Threading.AutoResetEvent(false);



        public Window(int x, int y, int width, int height)
        {
            Width = width;
            Height = height;
            X = x;
            Y = y;

            mPacketFS = new PacketFSClient();


            mBufSize = width * height * 4;
            Buffer = (byte*)Marshal.AllocHGlobal(mBufSize);
            

            mPacketFS.onDataReceived += new PacketFSClient.DataReceivedHandler(dataReceived);


            createWindow();
        }

        private void dataReceived(byte[] data)
        {
            foreach (var reader in PacketReader.get(ref data))
                HandlePacket(reader);
        }

        private void HandlePacket(PacketReader reader)
        {
            int size = reader.ReadInt32();
            PacketTypes type = (PacketTypes)reader.ReadInt32();

            switch(type)
            {
                case PacketTypes.CREATE_WINDOW_RESPONSE:
                    CreateWindowRespPacket resp = reader.ReadStruct<CreateWindowRespPacket>();

                    WindowID = resp.ID;

                    mSharedMemory = new SharedMemory.BufferReadWrite(@"Global\comp_" + WindowID, mBufSize);
                    break;
            }
        }

        public void Move(int x, int y)
        {
            throw new NotImplementedException();
        }

        private void createWindow()
        {
            mPacketFS.Send(PacketTypes.CREATE_WINDOW, new CreateWindowPacket { X = X, Y = Y, Height = Width, Width = Height });
        }

        public void Flush()
        {

            InvalidateWindow();
        }

        private void InvalidateWindow()
        {
            while (WindowID == -1)
            {
                Task.Delay(25).Wait();
            }

            mSharedMemory.Write((IntPtr)Buffer, mBufSize);

            mPacketFS.SendInt(PacketTypes.INVALIDATE_WINDOW, WindowID);
        }
    }
}
