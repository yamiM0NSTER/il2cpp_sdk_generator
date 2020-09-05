using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    class ResolvedStruct : ResolvedType
    {
        public ResolvedStruct(Il2CppTypeDefinition type, Int32 idx)
        {
            typeDef = type;
            typeDefinitionIndex = idx;
        }

        public override void Resolve()
        {
            if (isResolved)
                return;

            // Resolve fields
            // Resolve static fields
            // Resolve constats (if needed)
            // Resolve methods

            Console.WriteLine("ResolvedStruct::Resolve()");
            isResolved = true;
        }

        public override string ToCode(Int32 indent = 0)
        {
            string code = "";

            code += "ResolvedStruct::ToCode()";

            return code;
        }

    }
}
