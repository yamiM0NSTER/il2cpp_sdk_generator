using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace il2cpp_sdk_generator
{
    class ArrayBuilder
    {
        MemoryStream stream = new MemoryStream();

        public long Length
        {
            get { return stream.Length; }
        }

        public ArrayBuilder()
        {

        }

        public void Append(byte b)
        {
            stream.WriteByte(b);
        }

        public byte[] ToArray()
        {
            //return stream.GetBuffer();
            byte[] data = new byte[Length];
            Array.Copy(stream.GetBuffer(), data, Length);
            return data;
        }
    }
}
