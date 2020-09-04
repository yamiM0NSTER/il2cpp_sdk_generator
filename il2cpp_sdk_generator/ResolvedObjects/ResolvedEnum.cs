using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    class ResolvedEnum : ResolvedType
    {
        public Dictionary<string, object> values = new Dictionary<string, object>();

        public ResolvedEnum(Il2CppTypeDefinition type)
        {
            typeDef = type;
        }

        public string ToCode(Int32 indent = 0)
        {
            string code = "";

            code += $"enum {Name}\n".Indent(indent);
            code += "{\n".Indent(indent);
            foreach(var pair in values)
            {
                code += $"{pair.Key} = {pair.Value},\n".Indent(indent+2);
            }
            code += "};\n".Indent(indent);

            return code;
        }
    }
}
