using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace il2cpp_sdk_generator
{
    class PortableExecutable
    {
        // Direct File data
        public static IMAGE_DOS_HEADER dosHeader;
        public static UInt32 signature;
        public static IMAGE_FILE_HEADER imageFileHeader;
        public static IMAGE_OPTIONAL_HEADER64 imageOptionalHeader64;
        public static IMAGE_SECTION_HEADER[] imageSectionHeaders;

        // Import
        public static IMPORT_LOOKUP_TABLE_ENTRY[] importLookupTableEntries;
        public static HINT_NAME_TABLE_ENTRY[] hintNameTableEntries;


        // Exception 
        public static RUNTIME_FUNCTION[] runtimeFunctions;

        // Processed data
        public static Dictionary<string, IMAGE_SECTION_HEADER> m_mapSections = new Dictionary<string, IMAGE_SECTION_HEADER>();
        public static Dictionary<ulong, RUNTIME_FUNCTION> m_mapRuntimeFunctionPtrs = new Dictionary<ulong, RUNTIME_FUNCTION>(); // VA's
    }
}
