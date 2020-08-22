using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace il2cpp_sdk_generator
{
    class PortableExecutable
    {
        public static IMAGE_DOS_HEADER dosHeader;
        public static UInt32 signature;
        public static IMAGE_FILE_HEADER imageFileHeader;
        public static IMAGE_OPTIONAL_HEADER64 imageOptionalHeader64;
        public static IMAGE_SECTION_HEADER[] imageSectionHeaders;
    }
}
