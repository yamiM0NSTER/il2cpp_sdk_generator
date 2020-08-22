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
            PortableExecutable.dosHeader = reader.Read<IMAGE_DOS_HEADER>();
            PortableExecutable.dosHeader.DumpToConsole();

            if (PortableExecutable.dosHeader.e_magic != PE_Constants.IMAGE_DOS_SIGNATURE)
            {
                Console.WriteLine($"PortableExecutable e_magic not valid: {PortableExecutable.dosHeader.e_magic}");
                return;
            }
            stream.Position = PortableExecutable.dosHeader.e_lfanew;
            // signature can be uint16 but we want explicitly windows NT signature
            PortableExecutable.signature = reader.ReadUInt32();
            if(PortableExecutable.signature != PE_Constants.IMAGE_NT_SIGNATURE)
            {
                Console.WriteLine($"PortableExecutable signature not windows NT: {PortableExecutable.signature}");
                return;
            }

            PortableExecutable.imageFileHeader = reader.Read<IMAGE_FILE_HEADER>();
            PortableExecutable.imageFileHeader.DumpToConsole();

            if(PortableExecutable.imageFileHeader.Machine == PE_Constants.IMAGE_FILE_MACHINE_AMD64)
            {
                Console.WriteLine($"IMAGE_FILE_HEADER Machine is x64 which is correct!");
                PortableExecutable.imageOptionalHeader64 = reader.Read<IMAGE_OPTIONAL_HEADER64>();
                PortableExecutable.imageOptionalHeader64.DumpToConsole();
            }

            PortableExecutable.imageSectionHeaders = reader.ReadArray<IMAGE_SECTION_HEADER>(PortableExecutable.imageFileHeader.NumberOfSections);
            foreach(var section in PortableExecutable.imageSectionHeaders)
            {
                Console.WriteLine($"Section name: {System.Text.Encoding.UTF8.GetString(section.Name)}");
                section.DumpToConsole();
            }

            ReadExportDirectory();
            ReadImportDirectory();
        }

        // Proper
        public UInt64 VAFromRVA(UInt32 rva)
        {
            return rva + PortableExecutable.imageOptionalHeader64.ImageBase;
        }

        public UInt32 RVAFromVA(UInt64 va)
        {
            return (UInt32)(va - PortableExecutable.imageOptionalHeader64.ImageBase);
        }

        public UInt32 OffsetFromRVA(UInt32 rva)
        {
            var sectionHeader = PortableExecutable.imageSectionHeaders.First(header => header.VirtualAddress <= rva && header.VirtualAddress + header.Misc.VirtualSize >= rva);

            return rva - sectionHeader.VirtualAddress + sectionHeader.PointerToRawData;
        }

        public IMAGE_SECTION_HEADER GetSectionByRVA(UInt32 rva)
        {
            return PortableExecutable.imageSectionHeaders.First(header => header.VirtualAddress <= rva && header.VirtualAddress + header.Misc.VirtualSize >= rva);
        }

        public UInt32 OffsetFromVA(UInt64 va)
        {
            var rva = RVAFromVA(va);
            
            return OffsetFromRVA(rva);
        }

        public void ReadExportDirectory()
        {
            if(PortableExecutable.imageOptionalHeader64.NumberOfRvaAndSizes < PE_Constants.IMAGE_DIRECTORY_ENTRY_EXPORT+1)
            {
                Console.WriteLine($"Export Directory over declared entries!");
                return;
            }
            
            IMAGE_DATA_DIRECTORY dataDirectory = PortableExecutable.imageOptionalHeader64.DataDirectory[PE_Constants.IMAGE_DIRECTORY_ENTRY_EXPORT];
            if(dataDirectory.RelativeVirtualAddress == 0)
            {
                Console.WriteLine($"Export Directory empty.");
                return;
            }

            stream.Position = OffsetFromRVA(dataDirectory.RelativeVirtualAddress);

            EXPORT_DIRECTORY_TABLE exportDirectoryTable = reader.Read<EXPORT_DIRECTORY_TABLE>();
            exportDirectoryTable.DumpToConsole();

            stream.Position = OffsetFromRVA(exportDirectoryTable.NameRVA);
            // NULL-terminated string
            // TODO: Read string till null function
            Console.WriteLine($"exportDirectoryTable name from RVA: {reader.ReadNullTerminatedString()}");
            
            //Byte[] name = reader.ReadArray<Byte>(17);
            //Console.WriteLine($"exportDirectoryTable name from RVA: {System.Text.Encoding.UTF8.GetString(name)}");

            if(exportDirectoryTable.AddressTableEntries != exportDirectoryTable.NumberofNamePointers)
            {
                // Skip if number of functions doesn't equal nuumber of names until ordinal table is prepared.
                return;
            }
            // Read Export Address Table
            if(exportDirectoryTable.ExportAddressTableRVA > 0)
            {
                stream.Position = OffsetFromRVA(exportDirectoryTable.ExportAddressTableRVA);
                EXPORT_ADDRESS_TABLE_ENTRY[] exportAddressTableEntries = reader.ReadArray<EXPORT_ADDRESS_TABLE_ENTRY>((int)exportDirectoryTable.AddressTableEntries);
                for (int i=0;i<exportDirectoryTable.AddressTableEntries;i++)
                {
                    var section = GetSectionByRVA(exportAddressTableEntries[i].ExportRVA);
                    var section_name = System.Text.Encoding.UTF8.GetString(section.Name);
                    // Exports are external
                    if(!section_name.StartsWith(".text"))
                    {
                        exportAddressTableEntries[i].DumpToConsole();
                        Console.WriteLine($"Section name: {section_name}");
                    }
                    // else all are within PE
                }
            }

            // Read Export names
            if (exportDirectoryTable.NamePointerRVA > 0)
            {
                stream.Position = OffsetFromRVA(exportDirectoryTable.NamePointerRVA);
                EXPORT_NAME_TABLE_ENTRY[] exportNameTableEntries = reader.ReadArray<EXPORT_NAME_TABLE_ENTRY>((int)exportDirectoryTable.NumberofNamePointers);
                for (int i = 0; i < exportDirectoryTable.NumberofNamePointers; i++)
                {
                    
                    var section = GetSectionByRVA(exportNameTableEntries[i].NameRVA);
                    var section_name = System.Text.Encoding.UTF8.GetString(section.Name);
                    //exportNameTableEntries[i].DumpToConsole();
                    stream.Position = OffsetFromRVA(exportNameTableEntries[i].NameRVA);
                    //reader.ReadString();
                    
                    //Byte[] ExportName = reader.ReadArray<Byte>(17);
                    Console.WriteLine($"Export name from RVA: {reader.ReadNullTerminatedString()}");
                    //Console.WriteLine($"Section name: {section_name}");
                    //stream.Position = OffsetFromRVA(exportDirectoryTable.NamePointerRVA);

                }
            }

            // Skip Ordinal Table for now.


            // TODO: Combine and store? Maybe by Ordinal index?
        }

        public void ReadImportDirectory()
        {
            if (PortableExecutable.imageOptionalHeader64.NumberOfRvaAndSizes < PE_Constants.IMAGE_DIRECTORY_ENTRY_IMPORT + 1)
            {
                Console.WriteLine($"Import Directory over declared entries!");
                return;
            }

            IMAGE_DATA_DIRECTORY dataDirectory = PortableExecutable.imageOptionalHeader64.DataDirectory[PE_Constants.IMAGE_DIRECTORY_ENTRY_IMPORT];
            if (dataDirectory.RelativeVirtualAddress == 0)
            {
                Console.WriteLine($"Import Directory empty.");
                return;
            }

            // Read Table
            stream.Position = OffsetFromRVA(dataDirectory.RelativeVirtualAddress);

            IMPORT_DIRECTORY_TABLE_ENTRY[] importDirectoryTables = reader.ReadArray<IMPORT_DIRECTORY_TABLE_ENTRY>((int)dataDirectory.Size / typeof(IMPORT_DIRECTORY_TABLE_ENTRY).GetSizeOf());
            foreach(var importDirectoryTable in importDirectoryTables)
            {
                importDirectoryTable.DumpToConsole();
                if (importDirectoryTable.ImportLookupTableRVA == 0)
                    continue;
                stream.Position = OffsetFromRVA(importDirectoryTable.ImportLookupTableRVA);
                List<IMPORT_LOOKUP_TABLE_ENTRY> importLookupTableList = new List<IMPORT_LOOKUP_TABLE_ENTRY>();
                IMPORT_LOOKUP_TABLE_ENTRY importLookupTableEntry = reader.Read<IMPORT_LOOKUP_TABLE_ENTRY>();
                while (importLookupTableEntry.Ordinal_NameFlag != 0)
                {
                    importLookupTableList.Add(importLookupTableEntry);

                    //Console.WriteLine($"{importLookupTableEntry.ImportByOrdinal}");
                    //importLookupTableEntry.DumpToConsole();
                    importLookupTableEntry = reader.Read<IMPORT_LOOKUP_TABLE_ENTRY>();
                }


                foreach (var entry in importLookupTableList)
                {
                    if (!entry.ImportByOrdinal)
                    {
                        stream.Position = OffsetFromRVA(entry.Hint_NameTableRVA);
                        HINT_NAME_TABLE_ENTRY hintNameTableEntry = reader.Read<HINT_NAME_TABLE_ENTRY>();
                        hintNameTableEntry.DumpToConsole();
                    }
                }
            }
            //IMPORT_DIRECTORY_TABLE importDirectoryTable = reader.Read<IMPORT_DIRECTORY_TABLE>();



            //stream.Position = OffsetFromRVA(exportDirectoryTable.NameRVA);
            //// NULL-terminated string
            //// TODO: Read string till null function
            //Console.WriteLine($"exportDirectoryTable name from RVA: {reader.ReadNullTerminatedString()}");

            ////Byte[] name = reader.ReadArray<Byte>(17);
            ////Console.WriteLine($"exportDirectoryTable name from RVA: {System.Text.Encoding.UTF8.GetString(name)}");

            //if (exportDirectoryTable.AddressTableEntries != exportDirectoryTable.NumberofNamePointers)
            //{
            //    // Skip if number of functions doesn't equal nuumber of names until ordinal table is prepared.
            //    return;
            //}
            //// Read Export Address Table
            //if (exportDirectoryTable.ExportAddressTableRVA > 0)
            //{
            //    stream.Position = OffsetFromRVA(exportDirectoryTable.ExportAddressTableRVA);
            //    EXPORT_ADDRESS_TABLE_ENTRY[] exportAddressTableEntries = reader.ReadArray<EXPORT_ADDRESS_TABLE_ENTRY>((int)exportDirectoryTable.AddressTableEntries);
            //    for (int i = 0; i < exportDirectoryTable.AddressTableEntries; i++)
            //    {
            //        var section = GetSectionByRVA(exportAddressTableEntries[i].ExportRVA);
            //        var section_name = System.Text.Encoding.UTF8.GetString(section.Name);
            //        // Exports are external
            //        if (!section_name.StartsWith(".text"))
            //        {
            //            exportAddressTableEntries[i].DumpToConsole();
            //            Console.WriteLine($"Section name: {section_name}");
            //        }
            //        // else all are within PE
            //    }
            //}

            //// Read Export names
            //if (exportDirectoryTable.NamePointerRVA > 0)
            //{
            //    stream.Position = OffsetFromRVA(exportDirectoryTable.NamePointerRVA);
            //    EXPORT_NAME_TABLE_ENTRY[] exportNameTableEntries = reader.ReadArray<EXPORT_NAME_TABLE_ENTRY>((int)exportDirectoryTable.NumberofNamePointers);
            //    for (int i = 0; i < exportDirectoryTable.NumberofNamePointers; i++)
            //    {

            //        var section = GetSectionByRVA(exportNameTableEntries[i].NameRVA);
            //        var section_name = System.Text.Encoding.UTF8.GetString(section.Name);
            //        //exportNameTableEntries[i].DumpToConsole();
            //        stream.Position = OffsetFromRVA(exportNameTableEntries[i].NameRVA);
            //        //reader.ReadString();

            //        //Byte[] ExportName = reader.ReadArray<Byte>(17);
            //        Console.WriteLine($"Export name from RVA: {reader.ReadNullTerminatedString()}");
            //        //Console.WriteLine($"Section name: {section_name}");
            //        //stream.Position = OffsetFromRVA(exportDirectoryTable.NamePointerRVA);

            //    }
            //}

            //// Skip Ordinal Table for now.


            //// TODO: Combine and store? Maybe by Ordinal index?
        }
    }
}
