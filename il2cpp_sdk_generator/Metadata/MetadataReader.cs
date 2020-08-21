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
            Metadata.header = reader.Read<Il2CppGlobalMetadataHeader>();

            stream.Position = Metadata.header.stringLiteralOffset;
            Metadata.stringLiterals = new Il2CppStringLiteral[Metadata.header.stringLiteralCount];
            for (int i = 0; i < Metadata.header.stringLiteralCount; i++)
            {
                Metadata.stringLiterals[i] = reader.Read<Il2CppStringLiteral>();
                //Metadata.stringLiterals[i].DumpToConsole();
            }

            // For now skip strings

            stream.Position = Metadata.header.eventsOffset;
            Metadata.eventDefinitions = new Il2CppEventDefinition[Metadata.header.eventsCount];
            for (int i = 0; i < Metadata.header.eventsCount; i++)
            {
                Metadata.eventDefinitions[i] = reader.Read<Il2CppEventDefinition>();
                //Metadata.eventDefinitions[i].DumpToConsole();
            }

            stream.Position = Metadata.header.propertiesOffset;
            Metadata.propertyDefinitions = new Il2CppPropertyDefinition[Metadata.header.propertiesCount];
            for (int i = 0; i < Metadata.header.eventsCount; i++)
            {
                Metadata.propertyDefinitions[i] = reader.Read<Il2CppPropertyDefinition>();
                //Metadata.propertyDefinitions[i].DumpToConsole();
            }

            stream.Position = Metadata.header.methodsOffset;
            Metadata.methodDefinitions = new Il2CppMethodDefinition[Metadata.header.methodsCount];
            for (int i = 0; i < Metadata.header.eventsCount; i++)
            {
                Metadata.methodDefinitions[i] = reader.Read<Il2CppMethodDefinition>();
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
