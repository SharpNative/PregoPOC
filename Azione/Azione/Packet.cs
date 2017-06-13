using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Azione
{
    public class PacketWriter : BinaryWriter
    {
        private MemoryStream _ms;

        public PacketWriter() : base()
        {
            _ms = new MemoryStream();
            base.OutStream = _ms;
        }

        public void WriteObject(object o)
        {
            int size = Marshal.SizeOf(o);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(o, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            

            Write(arr);
        }

        public byte[] GetBytes()
        {
            base.Close();

            byte[] bytes = _ms.ToArray();

            PacketWriter binw = new PacketWriter();
            binw.Write(bytes.Length + 4);
            binw.Write(bytes);

            return binw._ms.ToArray();
        }
    }

    public class PacketReader : BinaryReader
    {

        public PacketReader(byte[] data)
            : base(new MemoryStream(data))
        {

        }
        
        public static object DeserializeFromStream(MemoryStream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            object o = formatter.Deserialize(stream);
            return o;
        }

        public T ReadStruct<T>()
        {
            byte[] bytes = ReadBytes(Marshal.SizeOf(typeof(T)));

            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return theStructure;
        }

        public static PacketReader[] get(ref byte[] data)
        {
            try
            {
                bool first = true;
                List<PacketReader> packList = null;

                int offset = 0;
                int length = data.Length;
                if (length == 0)
                    return new PacketReader[0];

                while (length > 0)
                {
                    int lng = BitConverter.ToInt32(data, offset);
                    if (lng == data.Length)
                    {
                        PacketReader[] pack = new PacketReader[1];
                        pack[0] = new PacketReader(data);

                        return pack;
                    }
                    else
                    {
                        if (first)
                            packList = new List<PacketReader>();

                        first = false;
                    }

                    byte[] block = new byte[lng];
                    Buffer.BlockCopy(data, offset, block, 0, (lng < length) ? lng : length);
                    packList.Add(new PacketReader(block));

                    offset += lng;
                    length -= lng;
                }

                return packList.ToArray();
            }
            catch (Exception e)
            {

                return new PacketReader[0];
            }

        }
    }
}
