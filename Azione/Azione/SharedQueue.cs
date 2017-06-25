using SharedMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azione
{
    public class SharedQueue
    {
        private SharedFifo mSharedMemory;
        private SharedFifo mSharedMemoryOut;

        private int mMaxSize;

        public SharedQueue(string name, int size, bool server = false)
        {
            mMaxSize = size;

            if (server)
            {
                mSharedMemory = new SharedFifo(name, 21, size, true);
                mSharedMemoryOut = new SharedFifo(name + "_out", 21, size, true);
            }
            else
            {

                mSharedMemory = new SharedFifo(name + "_out");
                mSharedMemoryOut = new SharedFifo(name);
            }
        }

        /// <summary>
        /// Read byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Read()
        {
            byte[] buf = null;

            while(true)
            {
                buf = mSharedMemory.Pop(true);
                if (buf == null)
                {
                    Console.WriteLine("READ WEAIT");
                    continue;
                }

                return buf;
            }
        }

        /// <summary>
        /// Write byte array
        /// </summary>
        /// <param name="bytes"></param>
        public void Write(byte[] bytes)
        {
            while (true)
            {
                int write = mSharedMemoryOut.Push(bytes);
                if (write == 0)
                {
                    Console.WriteLine("WRITE WAIT");
                    continue;
                }

                return;
            }
        }
    }
}
