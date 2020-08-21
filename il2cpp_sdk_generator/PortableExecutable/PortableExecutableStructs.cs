using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// References
// https://blog.kowalczyk.info/articles/pefileformat.html
// https://docs.microsoft.com/en-us/windows/win32/debug/pe-format

using BYTE = System.Byte;
using USHORT = System.UInt16;
using WORD = System.UInt16;
// LONG in c/c++ is int32
using LONG = System.Int32;
using ULONG = System.UInt32;
using DWORD = System.UInt32;
using UCHAR = System.Byte;
using ULONGLONG = System.UInt64;
using System.Runtime.InteropServices;

namespace il2cpp_sdk_generator
{
    // DOS .EXE header
    class IMAGE_DOS_HEADER
    {
        public USHORT e_magic;         // Magic number
        public USHORT e_cblp;          // Bytes on last page of file
        public USHORT e_cp;            // Pages in file
        public USHORT e_crlc;          // Relocations
        public USHORT e_cparhdr;       // Size of header in paragraphs
        public USHORT e_minalloc;      // Minimum extra paragraphs needed
        public USHORT e_maxalloc;      // Maximum extra paragraphs needed
        public USHORT e_ss;            // Initial (relative) SS value
        public USHORT e_sp;            // Initial SP value
        public USHORT e_csum;          // Checksum
        public USHORT e_ip;            // Initial IP value
        public USHORT e_cs;            // Initial (relative) CS value
        public USHORT e_lfarlc;        // File address of relocation table
        public USHORT e_ovno;          // Overlay number
        [ArraySize(4)]
        public USHORT[] e_res;         // Reserved words
        public USHORT e_oemid;         // OEM identifier (for e_oeminfo)
        public USHORT e_oeminfo;       // OEM information; e_oemid specific
        [ArraySize(10)]
        public USHORT[] e_res2;        // Reserved words
        public LONG e_lfanew;          // File address of new exe header
    }

    class IMAGE_FILE_HEADER
    {
        public USHORT Machine;
        public USHORT NumberOfSections;
        public ULONG TimeDateStamp;
        public ULONG PointerToSymbolTable;
        public ULONG NumberOfSymbols;
        public USHORT SizeOfOptionalHeader;
        public USHORT Characteristics;
    }

    class IMAGE_OPTIONAL_HEADER32
    {
        //
        // Standard fields.
        //
        public USHORT Magic;
        public UCHAR MajorLinkerVersion;
        public UCHAR MinorLinkerVersion;
        public ULONG SizeOfCode;
        public ULONG SizeOfInitializedData;
        public ULONG SizeOfUninitializedData;
        public ULONG AddressOfEntryPoint;
        public ULONG BaseOfCode;
        public ULONG BaseOfData;
        //
        // NT additional fields.
        //
        public ULONG ImageBase;
        public ULONG SectionAlignment;
        public ULONG FileAlignment;
        public USHORT MajorOperatingSystemVersion;
        public USHORT MinorOperatingSystemVersion;
        public USHORT MajorImageVersion;
        public USHORT MinorImageVersion;
        public USHORT MajorSubsystemVersion;
        public USHORT MinorSubsystemVersion;
        public ULONG Reserved1;
        public ULONG SizeOfImage;
        public ULONG SizeOfHeaders;
        public ULONG CheckSum;
        public USHORT Subsystem;
        public USHORT DllCharacteristics;
        public ULONG SizeOfStackReserve;
        public ULONG SizeOfStackCommit;
        public ULONG SizeOfHeapReserve;
        public ULONG SizeOfHeapCommit;
        public ULONG LoaderFlags;
        public ULONG NumberOfRvaAndSizes;
        [ArraySize(PE_Constants.IMAGE_NUMBEROF_DIRECTORY_ENTRIES)]
        public IMAGE_DATA_DIRECTORY[] DataDirectory;
    }

    class IMAGE_OPTIONAL_HEADER64
    {
        public WORD Magic;
        public BYTE MajorLinkerVersion;
        public BYTE MinorLinkerVersion;
        public DWORD SizeOfCode;
        public DWORD SizeOfInitializedData;
        public DWORD SizeOfUninitializedData;
        public DWORD AddressOfEntryPoint;
        public DWORD BaseOfCode;
        public ULONGLONG ImageBase;
        public DWORD SectionAlignment;
        public DWORD FileAlignment;
        public WORD MajorOperatingSystemVersion;
        public WORD MinorOperatingSystemVersion;
        public WORD MajorImageVersion;
        public WORD MinorImageVersion;
        public WORD MajorSubsystemVersion;
        public WORD MinorSubsystemVersion;
        public DWORD Win32VersionValue;
        public DWORD SizeOfImage;
        public DWORD SizeOfHeaders;
        public DWORD CheckSum;
        public WORD Subsystem;
        public WORD DllCharacteristics;
        public ULONGLONG SizeOfStackReserve;
        public ULONGLONG SizeOfStackCommit;
        public ULONGLONG SizeOfHeapReserve;
        public ULONGLONG SizeOfHeapCommit;
        public DWORD LoaderFlags;
        public DWORD NumberOfRvaAndSizes;
        [ArraySize(PE_Constants.IMAGE_NUMBEROF_DIRECTORY_ENTRIES)]
        public IMAGE_DATA_DIRECTORY[] DataDirectory;
    }

    class IMAGE_DATA_DIRECTORY
    {
        public ULONG VirtualAddress;
        public ULONG Size;
    }

    [StructLayout(LayoutKind.Explicit)]
    class PhysAddrVirtSize
    {
        [FieldOffset(0)]
        public DWORD PhysicalAddress;
        [FieldOffset(0)]
        public DWORD VirtualSize;
    }

    class IMAGE_SECTION_HEADER
    {
        [ArraySize(PE_Constants.IMAGE_SIZEOF_SHORT_NAME)]
        public BYTE[] Name;
        public PhysAddrVirtSize Misc; // union
        public DWORD VirtualAddress;
        public DWORD SizeOfRawData;
        public DWORD PointerToRawData;
        public DWORD PointerToRelocations;
        public DWORD PointerToLinenumbers;
        public WORD NumberOfRelocations;
        public WORD NumberOfLinenumbers;
        public DWORD Characteristics;
    }

    static class PE_Constants
    {
        public const USHORT IMAGE_DOS_SIGNATURE    = 0x5A4D; // MZ
        public const USHORT IMAGE_OS2_SIGNATURE    = 0x454E; // NE
        public const USHORT IMAGE_OS2_SIGNATURE_LE = 0x454C; // LE
        public const LONG   IMAGE_NT_SIGNATURE     = 0x00004550; // PE00

        public const USHORT IMAGE_NUMBEROF_DIRECTORY_ENTRIES = 16;


        // Machine types
        public const USHORT IMAGE_FILE_MACHINE_UNKNOWN = 0x0;
        public const USHORT IMAGE_FILE_MACHINE_TARGET_HOST = 0x0001;  // Useful for indicating we want to interact with the host and not a WoW guest.
        public const USHORT IMAGE_FILE_MACHINE_I386 = 0x014c;  // Intel 386.
        public const USHORT IMAGE_FILE_MACHINE_R3000 = 0x0162;  // MIPS little-endian, 0x160 big-endian
        public const USHORT IMAGE_FILE_MACHINE_R4000 = 0x0166;  // MIPS little-endian
        public const USHORT IMAGE_FILE_MACHINE_R10000 = 0x0168;  // MIPS little-endian
        public const USHORT IMAGE_FILE_MACHINE_WCEMIPSV2 = 0x0169;  // MIPS little-endian WCE v2
        public const USHORT IMAGE_FILE_MACHINE_ALPHA = 0x0184; // Alpha_AXP
        public const USHORT IMAGE_FILE_MACHINE_SH3 = 0x01a2; // SH3 little-endian Hitachi SH3 
        public const USHORT IMAGE_FILE_MACHINE_SH3DSP = 0x01a3; // Hitachi SH3 DSP 
        public const USHORT IMAGE_FILE_MACHINE_SH3E = 0x01a4; // SH3E little-endian
        public const USHORT IMAGE_FILE_MACHINE_SH4 = 0x01a6; // SH4 little-endian Hitachi SH4 
        public const USHORT IMAGE_FILE_MACHINE_SH5 = 0x01a8; // SH5 Hitachi SH5
        public const USHORT IMAGE_FILE_MACHINE_ARM = 0x01c0; // ARM Little-Endian
        public const USHORT IMAGE_FILE_MACHINE_THUMB = 0x01c2; // ARM Thumb/Thumb-2 Little-Endian
        public const USHORT IMAGE_FILE_MACHINE_ARMNT = 0x01c4; // ARM Thumb-2 Little-Endian
        public const USHORT IMAGE_FILE_MACHINE_AM33 = 0x01d3;
        public const USHORT IMAGE_FILE_MACHINE_POWERPC = 0x01F0; // IBM PowerPC Little-Endian
        public const USHORT IMAGE_FILE_MACHINE_POWERPCFP = 0x01f1;
        public const USHORT IMAGE_FILE_MACHINE_IA64 = 0x0200; // Intel 64
        public const USHORT IMAGE_FILE_MACHINE_MIPS16 = 0x0266; // MIPS
        public const USHORT IMAGE_FILE_MACHINE_ALPHA64 = 0x0284; // ALPHA64
        public const USHORT IMAGE_FILE_MACHINE_MIPSFPU = 0x0366; // MIPS
        public const USHORT IMAGE_FILE_MACHINE_MIPSFPU16 = 0x0466; // MIPS
        public const USHORT IMAGE_FILE_MACHINE_AXP64 = IMAGE_FILE_MACHINE_ALPHA64;
        public const USHORT IMAGE_FILE_MACHINE_TRICORE = 0x0520;  // Infineon
        public const USHORT IMAGE_FILE_MACHINE_CEF = 0x0CEF;
        public const USHORT IMAGE_FILE_MACHINE_EBC = 0x0EBC; // EFI Byte Code
        public const USHORT IMAGE_FILE_MACHINE_AMD64 = 0x8664; // AMD64 (K8) x64
        public const USHORT IMAGE_FILE_MACHINE_M32R = 0x9041; // M32R little-endian
        public const USHORT IMAGE_FILE_MACHINE_ARM64 = 0xAA64;  // ARM64 Little-Endian
        public const USHORT IMAGE_FILE_MACHINE_CEE = 0xC0EE;

        // Section Header
        public const USHORT IMAGE_SIZEOF_SHORT_NAME = 8;
    }
}
