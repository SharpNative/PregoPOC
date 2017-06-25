using Azione.Packets;
using SharedMemory;
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

        private static PacketFSClient mPacketFS;

        private static int mCurMessageID = 0;

        public int WindowID { get; private set; } = -1;

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int X { get; private set; }

        public int Y { get; private set; }

        public byte *Buffer { get; private set; }

        private int mBufSize;

        private int mMessageID;


        private BufferReadWrite mSharedMemory;
        private System.Threading.EventWaitHandle mWindowCreated = new System.Threading.AutoResetEvent(false);

        public OnMouseUpdateEventHandler OnMouseUpdate;


        public delegate void OnMouseUpdateEventHandler(MouseEvent e);


        public Window(int x, int y, int width, int height)
        {
            if (mPacketFS == null)
                mPacketFS = new PacketFSClient();

            Width = width;
            Height = height;
            X = x;
            Y = y;


            mBufSize = width * height * 4;
            Buffer = (byte*)Marshal.AllocHGlobal(mBufSize);

            mMessageID = mCurMessageID++;

            mPacketFS.RegisterMessageID(mMessageID, this);

            CreateWindow();
        }
        
        public void HandleEvent(PacketReader reader)
        {
            HandlePacket(reader);
        }

        public void HandlePacket(PacketReader reader)
        {
            PacketTypes type = (PacketTypes)reader.ReadInt32();

            switch(type)
            {
                case PacketTypes.CREATE_WINDOW_RESPONSE:
                    {
                        CreateWindowRespPacket resp = reader.ReadStruct<CreateWindowRespPacket>();

                        mSharedMemory = new BufferReadWrite(@"Global\compd_" + resp.ID, mBufSize);

                        mPacketFS.RegisterWindow(resp.ID, this);

                        WindowID = resp.ID;
                    }
                    break;

                case PacketTypes.MOUSE_UPDATE:
                    {
                        MouseEvent e = reader.ReadStruct<MouseEvent>();

                        OnMouseUpdate?.Invoke(e);
                    }
                    break;
            }
        }

        public void Move(int x, int y)
        {
            throw new NotImplementedException();
        }

        private void CreateWindow()
        {
            mPacketFS.Send(PacketTypes.CREATE_WINDOW, mMessageID, new CreateWindowPacket { X = X, Y = Y, Height = Height, Width = Width });
            

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


            mPacketFS.SendInt(PacketTypes.INVALIDATE_WINDOW, mMessageID, WindowID);
        }

        public void Close()
        {
            while (WindowID == -1)
            {
                Task.Delay(25).Wait();
            }

            mSharedMemory.Write((IntPtr)Buffer, mBufSize);


            mPacketFS.SendInt(PacketTypes.CLOSE_WINDOW, mMessageID, WindowID);
        }
    }
}
