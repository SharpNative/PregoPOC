using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Azione
{
    public unsafe class SharedFifo : SharedMemory
    {
        protected struct fifoHeader
        {
            public int Head;
            public int Tail;
            public int Size;
            public int ElemSize;
        }

        private fifoHeader* mHeaderPointer;

        private int* mSizePointer;
        private byte* mDataPointer;

        private Mutex mMutex;


        public SharedFifo(string name, int nodeSize = -1, int nodeCount = -1, bool memoryOwner = false) :
            base(name, memoryOwner)
        {

            if (memoryOwner)
            {
                mMutex = new Mutex(true, name + "_mutex");
                mMutex.ReleaseMutex();

                int size = Marshal.SizeOf<fifoHeader>() + (nodeCount * nodeSize) + (nodeCount * 4);

                mBufferSize = size;

                Open();

                mHeaderPointer = (fifoHeader*)mBufferPointer;
                mHeaderPointer->Size = nodeCount;
                mHeaderPointer->ElemSize = nodeSize;

                mSizePointer = (int*)(mBufferPointer + Marshal.SizeOf<fifoHeader>());
                mDataPointer = (byte*)((byte*)mSizePointer + nodeCount * 4);
            }
            else
            {
                mMutex = new Mutex(false, name + "_mutex");
                

                Open();

                mHeaderPointer = (fifoHeader*)mBufferPointer;

                mSizePointer = (int*)(mBufferPointer + Marshal.SizeOf<fifoHeader>());
                mDataPointer = (byte*)((byte*)mSizePointer + mHeaderPointer->Size * 4);
            }
        }

        public byte[] Pop(bool wait = false)
        {

            if (wait)
            {
                while (mHeaderPointer->Head == mHeaderPointer->Tail)
                    System.Threading.Thread.Sleep(1);
            }

            mMutex.WaitOne();

            byte[] ret = new byte[mHeaderPointer->ElemSize];

            if (mHeaderPointer->Head == mHeaderPointer->Tail)
            {
                mMutex.ReleaseMutex();
                return ret;
            }

            fixed (byte* retPtr = ret)
            {
                UnsafeNativeMethods.CopyMemory((IntPtr)retPtr, (IntPtr)(mDataPointer + (mHeaderPointer->ElemSize * mHeaderPointer->Tail)), (uint)mHeaderPointer->ElemSize);
            }

            mHeaderPointer->Tail++;
            if (mHeaderPointer->Tail == mHeaderPointer->Size)
                mHeaderPointer->Tail = 0;

            mMutex.ReleaseMutex();

            return ret;
        }



        public int Push(byte[] array)
        {
            byte[] emptyArray = new byte[mHeaderPointer->ElemSize];

            mMutex.WaitOne();

            // Is there any room?
            if ((mHeaderPointer->Head + 1 == mHeaderPointer->Tail) || ((mHeaderPointer->Head + 1 == mHeaderPointer->Size) && mHeaderPointer->Tail == 0))
            {

                mMutex.ReleaseMutex();

                return 0;
            }
            else
            {
                fixed (byte* emptyPtr = array)
                {
                    UnsafeNativeMethods.CopyMemory((IntPtr)(mDataPointer + (mHeaderPointer->ElemSize * mHeaderPointer->Head)), (IntPtr)emptyPtr, (uint)mHeaderPointer->ElemSize);
                }

                fixed (byte* ptr = array)
                {
                    UnsafeNativeMethods.CopyMemory((IntPtr)(mDataPointer + (mHeaderPointer->ElemSize * mHeaderPointer->Head)), (IntPtr)ptr, (uint)array.Length);
                }

                mHeaderPointer->Head++;
                

                // Time to flip the tail?
                if (mHeaderPointer->Head >= mHeaderPointer->Size)
                    mHeaderPointer->Head = 0;

                mMutex.ReleaseMutex();

                return array.Length;
            }

        }
    }
}
