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
        private MemoryStream stream;

        public MetadataReader(MemoryStream memoryStream)
        {
            stream = memoryStream;
            stream.Position = 0;
            reader = new BinaryReader(memoryStream, Encoding.UTF8);
        }

        public void Read()
        {
            Metadata.header = reader.Read<Il2CppGlobalMetadataHeader>();
            Metadata.header.DumpToConsole();

            stream.Position = Metadata.header.stringLiteralOffset;
            Metadata.stringLiterals = reader.ReadArray<Il2CppStringLiteral>(Metadata.header.stringLiteralCount / typeof(Il2CppStringLiteral).GetSizeOf());
            // For now skip strings
            stream.Position = Metadata.header.eventsOffset;
            Metadata.eventDefinitions = reader.ReadArray<Il2CppEventDefinition>(Metadata.header.eventsCount / typeof(Il2CppEventDefinition).GetSizeOf());
            stream.Position = Metadata.header.propertiesOffset;
            Metadata.propertyDefinitions = reader.ReadArray<Il2CppPropertyDefinition>(Metadata.header.propertiesCount / typeof(Il2CppPropertyDefinition).GetSizeOf());
            stream.Position = Metadata.header.methodsOffset;
            Metadata.methodDefinitions = reader.ReadArray<Il2CppMethodDefinition>(Metadata.header.methodsCount / typeof(Il2CppMethodDefinition).GetSizeOf());
            for (int i = 0; i < Metadata.header.methodsCount; i++)
            {
                Metadata.methodDefinitions[i].DumpToConsole();
            }


            return;
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
