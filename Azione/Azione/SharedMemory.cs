using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Azione
{
    public unsafe class SharedMemory
    {
        protected struct SharedHeader
        {
            public int BufferSize;
        }

        /// <summary>
        /// Memory mapped file
        /// </summary>
        protected MemoryMappedFile mMappedFile;
        /// <summary>
        /// Memory mapped view
        /// </summary>
        protected MemoryMappedViewAccessor mView;

        /// <summary>
        /// Pointer to start of view
        /// </summary>
        protected byte* mViewPointer;

        /// <summary>
        /// Pointer to start of header
        /// </summary>
        protected SharedHeader* mHeader;

        /// <summary>
        /// Pointer to start of view
        /// </summary>
        protected byte* mBufferPointer;

        /// <summary>
        /// Name
        /// </summary>
        protected string mName;

        /// <summary>
        /// Memory size
        /// </summary>
        protected int mBufferSize;

        /// <summary>
        /// Owner
        /// </summary>
        protected bool mMemoryOwner;

        public SharedMemory(string name, bool memoryOwner, int bufferSize = -1)
        {
            mName = name;
            mMemoryOwner = memoryOwner;
            mBufferSize = bufferSize;
        }

        public void Open()
        {

            int headerSize = Marshal.SizeOf<SharedHeader>();

            if (mMemoryOwner)
            {

                mMappedFile = MemoryMappedFile.CreateNew(mName, mBufferSize + headerSize);
                mView = mMappedFile.CreateViewAccessor(0, mBufferSize, MemoryMappedFileAccess.ReadWrite);
                mView.SafeMemoryMappedViewHandle.AcquirePointer(ref mViewPointer);

                mHeader = (SharedHeader*)mViewPointer;
                mHeader->BufferSize = mBufferSize;

                mBufferPointer = (mViewPointer + headerSize);
            }
            else
            {

                mMappedFile = MemoryMappedFile.OpenExisting(mName);

                using (MemoryMappedViewAccessor access = mMappedFile.CreateViewAccessor(0, headerSize))
                {
                    byte* headerPointer = null;
                    access.SafeMemoryMappedViewHandle.AcquirePointer(ref headerPointer);

                    mBufferSize = ((SharedHeader*)headerPointer)->BufferSize;

                    access.SafeMemoryMappedViewHandle.ReleasePointer();
                }

                mView = mMappedFile.CreateViewAccessor(0, mBufferSize + headerSize, MemoryMappedFileAccess.ReadWrite);
                mView.SafeMemoryMappedViewHandle.AcquirePointer(ref mViewPointer);

                mHeader = (SharedHeader*)mViewPointer;
                mHeader->BufferSize = mBufferSize;

                mBufferPointer = (mViewPointer + headerSize);

            }
        }
    }
}
