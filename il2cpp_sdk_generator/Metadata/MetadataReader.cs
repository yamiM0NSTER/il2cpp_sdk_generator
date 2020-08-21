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
        private Stream stream;

        public MetadataReader(BinaryReader binaryReader)
        {
            reader = binaryReader;
            stream = binaryReader.BaseStream;
        }

        public void Read()
        {
            // Apparently can't read structures and have to use class?
            Il2CppGlobalMetadataHeader header = reader.Read<Il2CppGlobalMetadataHeader>();
            header.DumpToConsole();

            // Offsets need position manips
            stream.Position = header.imagesOffset;
            // TODO: ReadArrays?
            for(int i=0;i< header.imagesCount;i++)
            {
                Il2CppImageDefinition il2CppImageDefinition = reader.Read<Il2CppImageDefinition>();
                //il2CppImageDefinition.DumpToConsole();
            }

        }
    }
}
