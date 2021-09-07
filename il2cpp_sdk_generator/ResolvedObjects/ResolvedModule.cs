using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    public class ResolvedModule
    {
        public string Name = "";
        public ulong[] methodPointers;

        public ResolvedModule(Il2CppCodeGenModule il2CppCodeGenModule)
        {
            var stream = il2cppReader.stream;
            var reader = il2cppReader.reader;

            stream.Position = (long)Offset.FromVA(il2CppCodeGenModule.moduleName);
            this.Name = reader.ReadNullTerminatedString();

            stream.Position = (long)Offset.FromVA(il2CppCodeGenModule.methodPointers);
            this.methodPointers = reader.ReadArray<ulong>((int)il2CppCodeGenModule.methodPointerCount);

            foreach (ulong pointer in this.methodPointers)
            {
                if (pointer == 0)
                    continue;

                CodeScanner.funcPtrs.Add(pointer);
            }
        }
    }
}
