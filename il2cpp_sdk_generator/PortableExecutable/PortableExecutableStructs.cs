using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

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


namespace il2cpp_sdk_generator
{
    /// <summary>
    /// DOS .EXE header
    /// </summary>
    class IMAGE_DOS_HEADER
    {
        /// <summary>
        /// Magic number
        /// </summary>
        public USHORT e_magic;
        /// <summary>
        /// Bytes on last page of file
        /// </summary>
        public USHORT e_cblp;
        /// <summary>
        /// Pages in file
        /// </summary>
        public USHORT e_cp;
        /// <summary>
        /// Relocations
        /// </summary>
        public USHORT e_crlc;
        /// <summary>
        /// Size of header in paragraphs
        /// </summary>
        public USHORT e_cparhdr;
        /// <summary>
        /// Minimum extra paragraphs needed
        /// </summary>
        public USHORT e_minalloc;
        /// <summary>
        /// Maximum extra paragraphs needed
        /// </summary>
        public USHORT e_maxalloc;
        /// <summary>
        /// Initial (relative) SS value
        /// </summary>
        public USHORT e_ss;
        /// <summary>
        /// Initial SP value
        /// </summary>
        public USHORT e_sp;
        /// <summary>
        /// Checksum
        /// </summary>
        public USHORT e_csum;
        /// <summary>
        /// Initial IP value
        /// </summary>
        public USHORT e_ip;
        /// <summary>
        /// Initial (relative) CS value
        /// </summary>
        public USHORT e_cs;
        /// <summary>
        /// File address of relocation table
        /// </summary>
        public USHORT e_lfarlc;
        /// <summary>
        /// Overlay number
        /// </summary>
        public USHORT e_ovno;
        /// <summary>
        /// Reserved words
        /// </summary>
        [ArraySize(4)]
        public USHORT[] e_res;
        /// <summary>
        /// OEM identifier (for e_oeminfo)
        /// </summary>
        public USHORT e_oemid;
        /// <summary>
        /// OEM information; e_oemid specific
        /// </summary>
        public USHORT e_oeminfo;
        /// <summary>
        /// Reserved words
        /// </summary>
        [ArraySize(10)]
        public USHORT[] e_res2;
        /// <summary>
        /// File address of new exe header
        /// </summary>
        public LONG e_lfanew;
    }

    class IMAGE_FILE_HEADER
    {
        /// <summary>
        /// The architecture type of the computer. An image file can only be run on the specified computer or a system that emulates the specified computer.
        /// </summary>
        public USHORT Machine;
        /// <summary>
        /// The number of sections. This indicates the size of the section table, which immediately follows the headers. Note that the Windows loader limits the number of sections to 96.
        /// </summary>
        public USHORT NumberOfSections;
        /// <summary>
        /// The low 32 bits of the time stamp of the image. This represents the date and time the image was created by the linker. The value is represented in the number of seconds elapsed since midnight (00:00:00), January 1, 1970, Universal Coordinated Time, according to the system clock.
        /// </summary>
        public ULONG TimeDateStamp;
        /// <summary>
        /// The offset of the symbol table, in bytes, or zero if no COFF symbol table exists.
        /// </summary>
        public ULONG PointerToSymbolTable;
        /// <summary>
        /// The number of symbols in the symbol table.
        /// </summary>
        public ULONG NumberOfSymbols;
        /// <summary>
        /// The size of the optional header, in bytes. This value should be 0 for object files.
        /// </summary>
        public USHORT SizeOfOptionalHeader;
        /// <summary>
        /// The characteristics of the image.
        /// </summary>
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
        /// <summary>
        /// The state of the image file.
        /// </summary>
        public WORD Magic;
        /// <summary>
        /// The major version number of the linker.
        /// </summary>
        public BYTE MajorLinkerVersion;
        /// <summary>
        /// The minor version number of the linker.
        /// </summary>
        public BYTE MinorLinkerVersion;
        /// <summary>
        /// The size of the code section, in bytes, or the sum of all such sections if there are multiple code sections.
        /// </summary>
        public DWORD SizeOfCode;
        /// <summary>
        /// The size of the initialized data section, in bytes, or the sum of all such sections if there are multiple initialized data sections.
        /// </summary>
        public DWORD SizeOfInitializedData;
        /// <summary>
        /// The size of the uninitialized data section, in bytes, or the sum of all such sections if there are multiple uninitialized data sections.
        /// </summary>
        public DWORD SizeOfUninitializedData;
        /// <summary>
        /// A pointer to the entry point function, relative to the image base address. For executable files, this is the starting address. For device drivers, this is the address of the initialization function. The entry point function is optional for DLLs. When no entry point is present, this member is zero.
        /// </summary>
        public DWORD AddressOfEntryPoint;
        /// <summary>
        /// A pointer to the beginning of the code section, relative to the image base.
        /// </summary>
        public DWORD BaseOfCode;
        /// <summary>
        /// The preferred address of the first byte of the image when it is loaded in memory. This value is a multiple of 64K bytes. The default value for DLLs is 0x10000000. The default value for applications is 0x00400000, except on Windows CE where it is 0x00010000.
        /// </summary>
        public ULONGLONG ImageBase;
        /// <summary>
        /// The alignment of sections loaded in memory, in bytes. This value must be greater than or equal to the FileAlignment member. The default value is the page size for the system.
        /// </summary>
        public DWORD SectionAlignment;
        /// <summary>
        /// The alignment of the raw data of sections in the image file, in bytes. The value should be a power of 2 between 512 and 64K (inclusive). The default is 512. If the SectionAlignment member is less than the system page size, this member must be the same as SectionAlignment.
        /// </summary>
        public DWORD FileAlignment;
        /// <summary>
        /// The major version number of the required operating system.
        /// </summary>
        public WORD MajorOperatingSystemVersion;
        /// <summary>
        /// The minor version number of the required operating system.
        /// </summary>
        public WORD MinorOperatingSystemVersion;
        /// <summary>
        /// The major version number of the image.
        /// </summary>
        public WORD MajorImageVersion;
        /// <summary>
        /// The minor version number of the image.
        /// </summary>
        public WORD MinorImageVersion;
        /// <summary>
        /// The major version number of the subsystem.
        /// </summary>
        public WORD MajorSubsystemVersion;
        /// <summary>
        /// The minor version number of the subsystem.
        /// </summary>
        public WORD MinorSubsystemVersion;
        /// <summary>
        /// This member is reserved and must be 0.
        /// </summary>
        public DWORD Win32VersionValue;
        /// <summary>
        /// The size of the image, in bytes, including all headers. Must be a multiple of SectionAlignment.
        /// </summary>
        public DWORD SizeOfImage;
        /// <summary>
        /// The combined size of the following items, rounded to a multiple of the value specified in the FileAlignment member.
        /// <para>* e_lfanew member of IMAGE_DOS_HEADER</para> 
        /// <para>* 4 byte signature</para> 
        /// <para>* size of IMAGE_FILE_HEADER</para> 
        /// <para>* size of optional header</para> 
        /// <para>* size of all section headers</para> 
        /// </summary>
        public DWORD SizeOfHeaders;
        /// <summary>
        /// The image file checksum. The following files are validated at load time: 
        /// <para>* all drivers, </para> 
        /// <para>* any DLL loaded at boot time, </para> 
        /// <para>* and any DLL loaded into a critical system process.</para> 
        /// </summary>
        public DWORD CheckSum;
        /// <summary>
        /// The subsystem required to run this image.
        /// </summary>
        public WORD Subsystem;
        /// <summary>
        /// The DLL characteristics of the image.
        /// </summary>
        public WORD DllCharacteristics;
        /// <summary>
        /// The number of bytes to reserve for the stack. Only the memory specified by the SizeOfStackCommit member is committed at load time; the rest is made available one page at a time until this reserve size is reached.
        /// </summary>
        public ULONGLONG SizeOfStackReserve;
        /// <summary>
        /// The number of bytes to commit for the stack.
        /// </summary>
        public ULONGLONG SizeOfStackCommit;
        /// <summary>
        /// The number of bytes to reserve for the local heap. Only the memory specified by the SizeOfHeapCommit member is committed at load time; the rest is made available one page at a time until this reserve size is reached.
        /// </summary>
        public ULONGLONG SizeOfHeapReserve;
        /// <summary>
        /// The number of bytes to commit for the local heap.
        /// </summary>
        public ULONGLONG SizeOfHeapCommit;
        /// <summary>
        /// This member is obsolete.
        /// </summary>
        public DWORD LoaderFlags;
        /// <summary>
        /// The number of directory entries in the remainder of the optional header. Each entry describes a location and size.
        /// </summary>
        public DWORD NumberOfRvaAndSizes;
        /// <summary>
        /// A pointer to the first IMAGE_DATA_DIRECTORY structure in the data directory.
        /// </summary>
        [ArraySize(PE_Constants.IMAGE_NUMBEROF_DIRECTORY_ENTRIES)]
        public IMAGE_DATA_DIRECTORY[] DataDirectory;
    }

    class IMAGE_DATA_DIRECTORY
    {
        /// <summary>
        /// The relative virtual address of the table.
        /// </summary>
        public ULONG VirtualAddress;
        /// <summary>
        /// The size of the table, in bytes.
        /// </summary>
        public ULONG Size;
    }


    [StructLayout(LayoutKind.Explicit)]
    class PhysAddrVirtSize
    {
        /// <summary>
        /// The file address.
        /// </summary>
        [FieldOffset(0)]
        public DWORD PhysicalAddress;
        /// <summary>
        /// The total size of the section when loaded into memory, in bytes. If this value is greater than the SizeOfRawData member, the section is filled with zeroes. This field is valid only for executable images and should be set to 0 for object files.
        /// </summary>
        [FieldOffset(0)]
        public DWORD VirtualSize;
    }

    class IMAGE_SECTION_HEADER
    {
        /// <summary>
        /// An 8-byte, null-padded UTF-8 string. There is no terminating null character if the string is exactly eight characters long. For longer names, this member contains a forward slash (/) followed by an ASCII representation of a decimal number that is an offset into the string table. Executable images do not use a string table and do not support section names longer than eight characters.
        /// </summary>
        [ArraySize(PE_Constants.IMAGE_SIZEOF_SHORT_NAME)]
        public BYTE[] Name;
        /// <summary>
        /// PhysicalAddress & VirtualSize union
        /// </summary>
        public PhysAddrVirtSize Misc;
        /// <summary>
        /// The address of the first byte of the section when loaded into memory, relative to the image base. For object files, this is the address of the first byte before relocation is applied.
        /// </summary>
        public DWORD VirtualAddress;
        /// <summary>
        /// The size of the initialized data on disk, in bytes. This value must be a multiple of the FileAlignment member of the IMAGE_OPTIONAL_HEADER structure. If this value is less than the VirtualSize member, the remainder of the section is filled with zeroes. If the section contains only uninitialized data, the member is zero.
        /// </summary>
        public DWORD SizeOfRawData;
        /// <summary>
        /// A file pointer to the first page within the COFF file. This value must be a multiple of the FileAlignment member of the IMAGE_OPTIONAL_HEADER structure. If a section contains only uninitialized data, set this member is zero.
        /// </summary>
        public DWORD PointerToRawData;
        /// <summary>
        /// A file pointer to the beginning of the relocation entries for the section. If there are no relocations, this value is zero.
        /// </summary>
        public DWORD PointerToRelocations;
        /// <summary>
        /// A file pointer to the beginning of the line-number entries for the section. If there are no COFF line numbers, this value is zero.
        /// </summary>
        public DWORD PointerToLinenumbers;
        /// <summary>
        /// The number of relocation entries for the section. This value is zero for executable images.
        /// </summary>
        public WORD NumberOfRelocations;
        /// <summary>
        /// The number of line-number entries for the section.
        /// </summary>
        public WORD NumberOfLinenumbers;
        /// <summary>
        /// The characteristics of the image.
        /// </summary>
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
