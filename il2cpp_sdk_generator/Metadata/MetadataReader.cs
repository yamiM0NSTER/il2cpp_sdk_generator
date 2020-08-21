using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace il2cpp_sdk_generator
{
    class MetadataReader
    {
        private BinaryReader reader;

        public MetadataReader(BinaryReader binaryReader)
        {
            reader = binaryReader;
        }

        public void Read()
        {
            // Apparently can't read structures and have to use class?
            Il2CppGlobalMetadataHeader header = reader.Read<Il2CppGlobalMetadataHeader>();
            header.DumpToConsole();


        }
    }
}
