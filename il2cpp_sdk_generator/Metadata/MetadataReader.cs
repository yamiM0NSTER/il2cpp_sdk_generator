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

        public MetadataReader(BinaryReader binaryReader)
        {
            reader = binaryReader;
        }
    }
}
