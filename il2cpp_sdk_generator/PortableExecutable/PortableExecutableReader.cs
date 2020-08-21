using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace il2cpp_sdk_generator
{
    class PortableExecutableReader
    {
        private BinaryReader reader;
        private MemoryStream stream;

        public PortableExecutableReader(MemoryStream memoryStream)
        {
            stream = memoryStream;
            stream.Position = 0;
            reader = new BinaryReader(memoryStream, Encoding.UTF8);
        }

        public void Read()
        {
            IMAGE_DOS_HEADER dosHeader = reader.Read<IMAGE_DOS_HEADER>();
            dosHeader.DumpToConsole();

            if (dosHeader.e_magic != PE_Constants.IMAGE_DOS_SIGNATURE)
            {
                Console.WriteLine($"PortableExecutable e_magic not valid: {dosHeader.e_magic}");
                return;
            }
            stream.Position = dosHeader.e_lfanew;
            // signature can be uint16 but we want explicitly windows NT signature
            UInt32 signature = reader.ReadUInt32();
            if(signature != PE_Constants.IMAGE_NT_SIGNATURE)
            {
                Console.WriteLine($"PortableExecutable signature not windows NT: {signature}");
                return;
            }

            IMAGE_FILE_HEADER imageFileHeader = reader.Read<IMAGE_FILE_HEADER>();
            imageFileHeader.DumpToConsole();

            if(imageFileHeader.Machine == PE_Constants.IMAGE_FILE_MACHINE_AMD64)
            {
                Console.WriteLine($"IMAGE_FILE_HEADER Machine is x64 which is correct!");
                IMAGE_OPTIONAL_HEADER64 imageOptionalHeader64 = reader.Read<IMAGE_OPTIONAL_HEADER64>();
                imageOptionalHeader64.DumpToConsole();
                foreach(var dataDirectory in imageOptionalHeader64.DataDirectory)
                {
                    dataDirectory.DumpToConsole();
                }
            }

            IMAGE_SECTION_HEADER[] imagegSectionHeaders = reader.ReadArray<IMAGE_SECTION_HEADER>(imageFileHeader.NumberOfSections);
            foreach(var section in imagegSectionHeaders)
            {
                section.DumpToConsole();
            }
        }
    }
}
