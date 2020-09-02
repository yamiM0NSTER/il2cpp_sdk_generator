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

        List<IMAGE_SECTION_HEADER> codeSections = new List<IMAGE_SECTION_HEADER>();
        List<IMAGE_SECTION_HEADER> dataSections = new List<IMAGE_SECTION_HEADER>();

        public PortableExecutableReader(MemoryStream memoryStream)
        {
            stream = memoryStream;
            stream.Position = 0;
            reader = new BinaryReader(memoryStream, Encoding.UTF8);
        }

        // Here we just read all useful information about assembly
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

            //
            if (PortableExecutable.imageFileHeader.Machine == PE_Constants.IMAGE_FILE_MACHINE_AMD64)
            {
                Console.WriteLine($"IMAGE_FILE_HEADER Machine is x64 which is correct!");
                // We read all 16 dataDirectory entries even though they can be invalid
                long posBeforeimageOptionalHeader64 = stream.Position;
                Console.WriteLine($"Position before imageOptionalHeader64: 0x{posBeforeimageOptionalHeader64:X8}");
                PortableExecutable.imageOptionalHeader64 = reader.Read<IMAGE_OPTIONAL_HEADER64>();
                PortableExecutable.imageOptionalHeader64.DumpToConsole();
                foreach(var directory in PortableExecutable.imageOptionalHeader64.DataDirectory)
                {
                    Console.WriteLine($"directory start: 0x{directory.RelativeVirtualAddress:X}");
                    Console.WriteLine($"directory end: 0x{directory.RelativeVirtualAddress + directory.Size:X}");
                }
                Console.WriteLine($"Position after imageOptionalHeader64: 0x{stream.Position:X8}");
                Console.WriteLine($"Calculated Position after imageOptionalHeader64: 0x{posBeforeimageOptionalHeader64 + PE_Constants.IMAGE_OPTIONAL_HEADER64_DATA_DICTIONARY64_OFFSET + PortableExecutable.imageOptionalHeader64.NumberOfRvaAndSizes * typeof(IMAGE_DATA_DIRECTORY).GetSizeOf():X8}");
                stream.Position = posBeforeimageOptionalHeader64 + PE_Constants.IMAGE_OPTIONAL_HEADER64_DATA_DICTIONARY64_OFFSET + PortableExecutable.imageOptionalHeader64.NumberOfRvaAndSizes * typeof(IMAGE_DATA_DIRECTORY).GetSizeOf();
            }

            Console.WriteLine($"ImageBase: 0x{PortableExecutable.imageOptionalHeader64.ImageBase:X8}");
            PortableExecutable.imageSectionHeaders = reader.ReadArray<IMAGE_SECTION_HEADER>(PortableExecutable.imageFileHeader.NumberOfSections);
            foreach(var section in PortableExecutable.imageSectionHeaders)
            {
                PortableExecutable.m_mapSections.Add(System.Text.Encoding.UTF8.GetString(section.Name).TrimEnd('\0'), section);

                Console.WriteLine($"Section name: {System.Text.Encoding.UTF8.GetString(section.Name)} [{System.Text.Encoding.UTF8.GetString(section.Name).TrimEnd('\0').Length}]");
                section.DumpToConsole();
                Console.WriteLine($"Section RVA VirtualAddress: 0x{section.VirtualAddress:X8}");
                Console.WriteLine($"Section VirtualAddress+VirtualSize: 0x{section.VirtualAddress+section.Misc.VirtualSize:X8}");
                Console.WriteLine($"Section characteristics: {section.Characteristics:X4}");

                //if ((section.Characteristics & PE_Constants.CHARACTERISTICS_DATA_MASK) == PE_Constants.CHARACTERISTICS_DATA_MASK)
                //    dataSections.Add(section);
                //else
                if ((section.Characteristics & PE_Constants.CHARACTERISTICS_DATA_MASK2) == PE_Constants.CHARACTERISTICS_DATA_MASK2)
                    dataSections.Add(section);
                else if ((section.Characteristics & PE_Constants.CHARACTERISTICS_CODE_MASK) == PE_Constants.CHARACTERISTICS_CODE_MASK)
                    codeSections.Add(section);

                // TODO: COMDAT data if present
                if ((section.Characteristics & PE_Constants.IMAGE_SCN_LNK_COMDAT) == PE_Constants.IMAGE_SCN_LNK_COMDAT)
                    Console.WriteLine("COMDAT FOUND");
            }

            // TODO: If symbol table exists try to read what is there, maybe useful
            // COFF Symbol table
            if(PortableExecutable.imageFileHeader.PointerToSymbolTable != 0)
            {

            }

            ReadExportDirectory();
            ReadImportDirectory();
            ReadExceptionDirectory();

            Console.WriteLine($"dataSections.Count: {dataSections.Count}");
            Console.WriteLine($"codeSections.Count: {codeSections.Count}");
        }

        // Here we process whatever can be processed for later use
        public void Process()
        {
            if (FindRegistrationByPattern(ref il2cpp.CodeRegistrationAddress, ref il2cpp.MetadataRegistrationAddress))
            {
                Console.WriteLine("Managed to find Registrations by pattern");
                Console.WriteLine($"MetadataRegistrationAddress: 0x{il2cpp.MetadataRegistrationAddress:X}");
                Console.WriteLine($"CodeRegistrationAddress: 0x{il2cpp.CodeRegistrationAddress:X}");
            }
            else if (FindRegistrationbyStructs(ref il2cpp.CodeRegistrationAddress, ref il2cpp.MetadataRegistrationAddress))
            {
                Console.WriteLine("Fall back to searching Registrations by structures");
                Console.WriteLine($"MetadataRegistrationAddress: 0x{il2cpp.MetadataRegistrationAddress:X}");
                Console.WriteLine($"CodeRegistrationAddress: 0x{il2cpp.CodeRegistrationAddress:X}");
            }
            else
            {
                Console.WriteLine("Failed to find Registrations");
                il2cpp.CodeRegistrationAddress = 0;
                il2cpp.MetadataRegistrationAddress = 0;
            }

            // TODO: Trusted references
            return;
            for (int i = 0; i < PortableExecutable.runtimeFunctions.Length; i++)
            {
                CodeScanner.funcPtrs.Add(VA.FromRVA(PortableExecutable.runtimeFunctions[i].BeginAddress));
            }
        }
        // Function code exists when using Visual Studio when making x64 build
        const string RegistrationPattern = "4C 8D 05 ?? ?? ?? ?? 48 8D 15 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E9 ?? ?? ?? ??";
        const long leaInstructionSize = 7;
        const long MetadataRegistrationInstructionOffset = leaInstructionSize * 1;
        const long CodeRegistrationInstructionOffset = leaInstructionSize * 2;
        
        // Works with Unity 2018 & 2019 x64 OwO
        // Works with VRChat build 976 => Unity 2018.4.20 il2cpp 24.1
        // Works with BusinessTour => Unity 2019.4.1 il2cpp 24.3
        // Doesn't work with bigscreen -> Unity 2018.4.22 il2cpp 24.1
        private bool FindRegistrationByPattern(ref UInt64 CodeRegistrationAddress, ref UInt64 MetadataRegistrationAddress)
        {
            List<long> candidates = new List<long>();

            // if somehow .text section doesn't exist so we just search whole file ¯\_(ツ)_/¯
            long offset = 0;
            long max_offset = BinaryPattern.m_assemblySize;
            long max_len = BinaryPattern.m_assemblySize;

            // Get .text section because function should be there
            IMAGE_SECTION_HEADER searchSection;
            if (PortableExecutable.m_mapSections.TryGetValue(".text", out searchSection))
            {
                offset = searchSection.PointerToRawData;
                max_offset = searchSection.PointerToRawData + searchSection.SizeOfRawData;
                max_len = searchSection.SizeOfRawData;
            }

            while ((offset = BinaryPattern.FindPattern(offset, max_len, "4C 8D 05 ?? ?? ?? ?? 48 8D 15 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E9 ?? ?? ?? ??")) != 0)
            {
                candidates.Add(offset);
                // Increment so we can keep searching
                offset++;
                max_len = max_offset - offset;
            }
            Console.WriteLine($"Candidates: {candidates.Count}");

            for(int i = 0;i<candidates.Count;i++)
            {
                // Get MetadataRegistrationAddress
                int address = BinaryPattern.GetInt32_LE(candidates[i] + MetadataRegistrationInstructionOffset + 3);
                MetadataRegistrationAddress = VA.FromOffset((UInt64)candidates[i] + MetadataRegistrationInstructionOffset + leaInstructionSize) + (UInt64)address;

                // Since we are reading it, we can cache it for later use
                stream.Position = (long)Offset.FromVA(MetadataRegistrationAddress);
                il2cpp.metadataRegistration64 = reader.Read<Il2CppMetadataRegistration64>();
                if (!il2cpp.metadataRegistration64.Validate())
                    continue;

                // Get CodeRegistrationAddress
                address = BinaryPattern.GetInt32_LE(candidates[i] + CodeRegistrationInstructionOffset + 3);
                CodeRegistrationAddress = VA.FromOffset((UInt64)candidates[i] + CodeRegistrationInstructionOffset + leaInstructionSize) + (UInt64)address;

                // Since we are reading it, we can cache it for later use
                stream.Position = (long)Offset.FromVA(CodeRegistrationAddress);
                il2cpp.codeRegistration64 = reader.Read<Il2CppCodeRegistration64>();
                if (!il2cpp.codeRegistration64.Validate())
                    continue;

                return true;
            }

            // If not found ensure it's set to null
            il2cpp.metadataRegistration64 = null;
            il2cpp.codeRegistration64 = null;

            return false;
        }

        private bool FindRegistrationbyStructs(ref UInt64 CodeRegistrationAddress, ref UInt64 MetadataRegistrationAddress)
        {
            CodeRegistrationAddress = FindCodeRegistration();
            if (CodeRegistrationAddress == 0UL)
                return false;

            MetadataRegistrationAddress = FindMetadataRegistration();
            if (MetadataRegistrationAddress == 0UL)
                return false;

            return true;
        }

        const long unresolvedVirtualCallCountOffset64 = 10L * 8L;
        // Works with VRChat build 976 => Unity 2018.4.20 il2cpp 24.1
        // Doesn't work BusinessTour => Unity 2019.4.1 il2cpp 24.3 =>>>> 99% because of different metadata structure and we depend on it to be correct
        // Works with bigscreen -> Unity 2018.4.22 il2cpp 24.1
        private ulong FindCodeRegistration()
        {
            // PE should store indexed method pointers
            int methodsCount = Metadata.methodDefinitions.Count(x => x.methodIndex >= 0);

            // Code Registration should be in .rdata, otherwise something changed/file was mangled
            IMAGE_SECTION_HEADER rdataSection;
            if (!PortableExecutable.m_mapSections.TryGetValue(".rdata", out rdataSection))
                return 0;

            IMAGE_SECTION_HEADER textSection;
            PortableExecutable.m_mapSections.TryGetValue(".text", out textSection);

            IMAGE_SECTION_HEADER il2cppSection;
            PortableExecutable.m_mapSections.TryGetValue("il2cpp", out il2cppSection);

            long offset = rdataSection.PointerToRawData;
            long max_offset = rdataSection.PointerToRawData + rdataSection.SizeOfRawData;

            stream.Position = offset;
            while (stream.Position < max_offset)
            {
                long address = stream.Position;
                long methodsCountCandidate = reader.ReadInt64();
                // Read values until we find indexed methods number
                if (methodsCountCandidate != methodsCount)
                    continue;

                // Next value should be VA(Virtual address) to array of methods
                // should be within .rdata
                // E0 B7 85 84 01 => 0x018485B7E0 
                long methodTableOffset = (long)Offset.FromVA(reader.ReadUInt64());
                Console.WriteLine($"methodTableOffset: 0x{methodTableOffset:X8}");
                if(rdataSection != Section.ByOffset((ulong)methodTableOffset))
                    goto KEEP_SEARCHING;

                // There should be enough space till end of section to contain whole table
                if (methodTableOffset + (long)methodsCount*sizeof(ulong) > max_offset)
                    goto KEEP_SEARCHING;

                // check unresolvedVirtualCallCount
                // NOTE: not sure if it always matches metadata values
                // works with vrchat build 976
                // works with bigscreen 
                // doesn't work with business tour due to il2cpp version mismatch => probably some changes in structures 
                stream.Position = address + unresolvedVirtualCallCountOffset64; // unresolvedVirtualCallCount is 11th field
                long unresolvedVirtualCallCountCandidate = reader.ReadInt64();
                if(unresolvedVirtualCallCountCandidate != Metadata.unresolvedVirtualCallParameterTypes.Length)
                    goto KEEP_SEARCHING;

                // Try to read whole structure and check pointers
                stream.Position = address;
                // Since we are reading it, we can cache it for later use
                il2cpp.codeRegistration64 = reader.Read<Il2CppCodeRegistration64>();
                if(!il2cpp.codeRegistration64.Validate())
                    goto KEEP_SEARCHING;

                Console.WriteLine($"unresolvedVirtualCallCountCandidate: {unresolvedVirtualCallCountCandidate}");

                // Sanity check all methods to be valid pointers
                stream.Position = methodTableOffset;
                // Read table of method VA's(Virtual addresses) and validate
                // 60 97 55 80 01 00 00 00 => 0x0180558760
                var methodPtrs = reader.ReadArray<ulong>(methodsCount);
                bool bFoundFaultyPtr = false;
                for(int i=0;i < methodsCount; i++)
                {
                    IMAGE_SECTION_HEADER section = Section.ByRVA(RVA.FromVA(methodPtrs[i]));
                    // methods should be in execution sections
                    if (il2cppSection != section && textSection != section)
                    {
                        Console.WriteLine($"i: {i} addr: 0x{methodPtrs[i]:X8} section: {System.Text.Encoding.UTF8.GetString(Section.ByRVA(RVA.FromVA(methodPtrs[i])).Name).TrimEnd('\0')} :Thonk:");
                        bFoundFaultyPtr = true;
                        break;
                    }
                }

                if(bFoundFaultyPtr)
                    goto KEEP_SEARCHING;

                return VA.FromOffset((ulong)address);

                KEEP_SEARCHING:
                // move back position + size of checked value
                stream.Position = address + sizeof(long);
                continue;
            }

            // If not found ensure it's set to null
            il2cpp.codeRegistration64 = null;

            return 0;
        }
        
        const long fieldOffsetsCountOffset64 = 10L * 8L; // fieldOffsetsCount is 11th field
        /// <summary>
        ///  FindMetadataRegistration
        ///  in x64 spacing is 8bytes so we read longs even for int32 values
        ///  Fields [confirmed] that can be compared against:
        ///  fieldOffsetsCount
        ///  typeDefinitions
        ///  metadataUsages
        ///  
        /// </summary>
        /// <returns></returns>
        private ulong FindMetadataRegistration()
        {
            // PE should store type definitions
            int typeDefinitions = Metadata.typeDefinitions.Length;
            Console.WriteLine($"typeDefinitions: {typeDefinitions}");

            // Metadata Registration should be in .rdata, otherwise something changed/file was mangled
            IMAGE_SECTION_HEADER rdataSection;
            if (!PortableExecutable.m_mapSections.TryGetValue(".rdata", out rdataSection))
                return 0;

            long offset = rdataSection.PointerToRawData;
            long max_offset = rdataSection.PointerToRawData + rdataSection.SizeOfRawData;

            // Search for fieldOffsetsCount candidate
            stream.Position = offset;
            while (stream.Position < max_offset)
            {
                long address = stream.Position;
                long fieldOffsetsCountCandidate = reader.ReadInt64();
                if (fieldOffsetsCountCandidate != typeDefinitions)
                    goto KEEP_SEARCHING;

                // Next value should be address to array so we skip it, maybe compare to imageBase
                long fieldOffsetsAddr = reader.ReadInt64();
                //Console.WriteLine($"Skip1: {fieldOffsetsAddr}");
                long typeDefinitionsCandidate = reader.ReadInt64();
                // This value should be typeDefinitionsCount and same as fieldOffsetsCount
                if (typeDefinitionsCandidate != typeDefinitions)
                    goto KEEP_SEARCHING;

                //Console.WriteLine($"FindMetadataRegistration addr?: 0x{VAFromOffset((ulong)(address- 10L * 8L)):X8}");

                // Try to read structure from assumed address
                stream.Position = address - fieldOffsetsCountOffset64; // fieldOffsetsCount is 11th field
                // Since we are reading it, we can cache it for later use
                il2cpp.metadataRegistration64 = reader.Read<Il2CppMetadataRegistration64>();
                if(!il2cpp.metadataRegistration64.Validate())
                    goto KEEP_SEARCHING;

                // TODO: sanity check if pointer arrays are valid?

                return VA.FromOffset((ulong)(address - fieldOffsetsCountOffset64)); // fieldOffsetsCount is 11th field

                KEEP_SEARCHING:
                // move back position + size of checked value
                stream.Position = address + sizeof(long);
                continue;
            }

            // If not found ensure it's set to null
            il2cpp.metadataRegistration64 = null;

            return 0;
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
                Console.WriteLine($"Export Directory is empty.");
                return;
            }

            stream.Position = (long)Offset.FromRVA(dataDirectory.RelativeVirtualAddress);

            EXPORT_DIRECTORY_TABLE exportDirectoryTable = reader.Read<EXPORT_DIRECTORY_TABLE>();
            exportDirectoryTable.DumpToConsole();

            stream.Position = (long)Offset.FromRVA(exportDirectoryTable.NameRVA);
            // NULL-terminated string
            // TODO: Read string till null function
            Console.WriteLine($"exportDirectoryTable name from RVA: {reader.ReadNullTerminatedString()}");

            if(exportDirectoryTable.AddressTableEntries != exportDirectoryTable.NumberofNamePointers)
            {
                // Skip if number of functions doesn't equal nuumber of names until ordinal table is prepared.
                return;
            }

            // Read Export Address Table
            if(exportDirectoryTable.ExportAddressTableRVA > 0)
            {
                stream.Position = (long)Offset.FromRVA(exportDirectoryTable.ExportAddressTableRVA);
                EXPORT_ADDRESS_TABLE_ENTRY[] exportAddressTableEntries = reader.ReadArray<EXPORT_ADDRESS_TABLE_ENTRY>((int)exportDirectoryTable.AddressTableEntries);
                for (int i=0;i<exportDirectoryTable.AddressTableEntries;i++)
                {
                    var section = Section.ByRVA(exportAddressTableEntries[i].ExportRVA);
                    var section_name = System.Text.Encoding.UTF8.GetString(section.Name);
                    // Exports are external
                    if(!section_name.StartsWith(".text"))
                    {
                        exportAddressTableEntries[i].DumpToConsole();
                        Console.WriteLine($"Section name: {section_name}");
                    }
                    // else all are within PE

                    Console.WriteLine($"Func[{i}] 0x{VA.FromRVA(exportAddressTableEntries[i].ExportRVA):X8} section: {section_name}");
                }
            }

            // Read Export names
            if (exportDirectoryTable.NamePointerRVA > 0)
            {
                stream.Position = (long)Offset.FromRVA(exportDirectoryTable.NamePointerRVA);
                EXPORT_NAME_TABLE_ENTRY[] exportNameTableEntries = reader.ReadArray<EXPORT_NAME_TABLE_ENTRY>((int)exportDirectoryTable.NumberofNamePointers);
                for (int i = 0; i < exportDirectoryTable.NumberofNamePointers; i++)
                {
                    
                    var section = Section.ByRVA(exportNameTableEntries[i].NameRVA);
                    var section_name = System.Text.Encoding.UTF8.GetString(section.Name);
                    //exportNameTableEntries[i].DumpToConsole();
                    stream.Position = (long)Offset.FromRVA(exportNameTableEntries[i].NameRVA);
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
            stream.Position = (long)Offset.FromRVA(dataDirectory.RelativeVirtualAddress);

            IMPORT_DIRECTORY_TABLE_ENTRY[] importDirectoryTables = reader.ReadArray<IMPORT_DIRECTORY_TABLE_ENTRY>((int)dataDirectory.Size / typeof(IMPORT_DIRECTORY_TABLE_ENTRY).GetSizeOf());
            foreach(var importDirectoryTable in importDirectoryTables)
            {
                //importDirectoryTable.DumpToConsole();
                if (importDirectoryTable.ImportLookupTableRVA == 0)
                    continue;
                stream.Position = (long)Offset.FromRVA(importDirectoryTable.NameRVA);
                Console.WriteLine($"DLL: {reader.ReadNullTerminatedString()}");
                stream.Position = (long)Offset.FromRVA(importDirectoryTable.ImportLookupTableRVA);
                List<IMPORT_LOOKUP_TABLE_ENTRY> importLookupTableList = new List<IMPORT_LOOKUP_TABLE_ENTRY>();
                IMPORT_LOOKUP_TABLE_ENTRY importLookupTableEntry = reader.Read<IMPORT_LOOKUP_TABLE_ENTRY>();
                while (importLookupTableEntry.Ordinal_NameFlag != 0 || importLookupTableEntry.OrdinalNumber != 0 || importLookupTableEntry.Hint_NameTableRVA != 0)
                {
                    importLookupTableList.Add(importLookupTableEntry);

                    //Console.WriteLine($"{importLookupTableEntry.ImportByOrdinal}");
                    //importLookupTableEntry.DumpToConsole();
                    importLookupTableEntry = reader.Read<IMPORT_LOOKUP_TABLE_ENTRY>();
                }
                PortableExecutable.importLookupTableEntries = importLookupTableList.ToArray();
                PortableExecutable.hintNameTableEntries = new HINT_NAME_TABLE_ENTRY[PortableExecutable.importLookupTableEntries.Length];

                for(int i =0; i<PortableExecutable.importLookupTableEntries.Length;i++)
                {
                    if (!PortableExecutable.importLookupTableEntries[i].ImportByOrdinal)
                    {
                        stream.Position = (long)Offset.FromRVA(PortableExecutable.importLookupTableEntries[i].Hint_NameTableRVA);
                        PortableExecutable.hintNameTableEntries[i] = reader.Read<HINT_NAME_TABLE_ENTRY>();
                        //PortableExecutable.hintNameTableEntries[i].DumpToConsole();
                        Console.WriteLine($" {PortableExecutable.hintNameTableEntries[i].Name}");
                    }
                    else
                    {
                        Console.WriteLine($" Ordinal: {PortableExecutable.importLookupTableEntries[i].OrdinalNumber}");
                    }
                }
            }
        }

        public void ReadExceptionDirectory()
        {
            if (PortableExecutable.imageOptionalHeader64.NumberOfRvaAndSizes < PE_Constants.IMAGE_DIRECTORY_ENTRY_EXCEPTION + 1)
            {
                Console.WriteLine($"Exception Directory over declared entries!");
                return;
            }

            IMAGE_DATA_DIRECTORY dataDirectory = PortableExecutable.imageOptionalHeader64.DataDirectory[PE_Constants.IMAGE_DIRECTORY_ENTRY_EXCEPTION];
            if (dataDirectory.RelativeVirtualAddress == 0)
            {
                Console.WriteLine($"Exception Directory empty.");
                return;
            }

            // Read Table
            stream.Position = (long)Offset.FromRVA(dataDirectory.RelativeVirtualAddress);

            PortableExecutable.runtimeFunctions = reader.ReadArray<RUNTIME_FUNCTION>((int)dataDirectory.Size / typeof(RUNTIME_FUNCTION).GetSizeOf());
            Console.WriteLine($"PortableExecutable.runtimeFunctions[{PortableExecutable.runtimeFunctions.Length}]");
            //foreach (var entry in PortableExecutable.exceptionTableEntries)
            //{
            //    Console.WriteLine($"[0x{VA.FromRVA(entry.BeginAddress):X8}]");
            //}
        }
    }
}
