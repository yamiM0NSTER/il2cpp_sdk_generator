using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    class ResolvedInterface : ResolvedType
    {
        public ResolvedInterface(Il2CppTypeDefinition type, Int32 idx)
        {
            typeDef = type;
            typeDefinitionIndex = idx;
        }

        public override void Resolve()
        {
            if (isResolved)
                return;

            Console.WriteLine("BUT. WHO ASKED?");
            isResolved = true;
        }

        public override string ToCode(Int32 indent = 0)
        {
            string code = "";

            code += "BUT. WHO ASKED?";

            return code;
        }

    }
}
