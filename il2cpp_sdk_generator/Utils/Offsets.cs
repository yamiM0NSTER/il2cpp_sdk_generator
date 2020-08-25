using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace il2cpp_sdk_generator
{
    class VA
    {
        public static UInt64 FromRVA(UInt64 rva)
        {
            return rva + PortableExecutable.imageOptionalHeader64.ImageBase;
        }

        // TODO: decide if return 0 or INVALID_VA value
        public static UInt64 FromOffset(UInt64 offset)
        {
            var sectionHeader = PortableExecutable.imageSectionHeaders.FirstOrDefault(header => header.PointerToRawData <= offset && header.PointerToRawData + header.SizeOfRawData >= offset);
            if (sectionHeader == null)
                return 0;

            return FromRVA(offset + sectionHeader.VirtualAddress - sectionHeader.PointerToRawData);
        }
    }

    class RVA
    {
        // TODO: decide if return 0 or INVALID_RVA value if va is lower than imageBase somehow
        public static UInt64 FromVA(UInt64 va)
        {
            return va - PortableExecutable.imageOptionalHeader64.ImageBase;
        }

        // TODO: decide if return 0 or INVALID_RVA value
        public static UInt64 FromOffset(UInt64 offset)
        {
            var sectionHeader = PortableExecutable.imageSectionHeaders.FirstOrDefault(header => header.PointerToRawData <= offset && header.PointerToRawData + header.SizeOfRawData >= offset);
            if (sectionHeader == null)
                return 0;

            return offset + sectionHeader.VirtualAddress - sectionHeader.PointerToRawData;
        }
    }

    class Offset
    {
        // TODO: decide if return 0 or INVALID_OFFSET value
        public static UInt64 FromRVA(UInt64 rva)
        {
            var sectionHeader = PortableExecutable.imageSectionHeaders.FirstOrDefault(header => header.VirtualAddress <= rva && header.VirtualAddress + header.Misc.VirtualSize >= rva);
            if (sectionHeader == null)
                return 0;

            return rva - sectionHeader.VirtualAddress + sectionHeader.PointerToRawData;
        }

        public static UInt64 FromVA(UInt64 va)
        {
            var rva = RVA.FromVA(va);

            return FromRVA(rva);
        }
    }

    class Section
    {
        public static IMAGE_SECTION_HEADER ByRVA(UInt64 rva)
        {
            return PortableExecutable.imageSectionHeaders.FirstOrDefault(header => header.VirtualAddress <= rva && header.VirtualAddress + header.Misc.VirtualSize >= rva);
        }

        public static IMAGE_SECTION_HEADER ByOffset(UInt64 offset)
        {
            return PortableExecutable.imageSectionHeaders.FirstOrDefault(header => header.PointerToRawData <= offset && header.PointerToRawData + header.SizeOfRawData >= offset);
        }
    }
}
