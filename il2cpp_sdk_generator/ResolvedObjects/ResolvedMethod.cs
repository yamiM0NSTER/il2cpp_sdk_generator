using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    class ResolvedMethod : ResolvedObject
    {
        public Int32 methodIndex;
        public Il2CppMethodDefinition methodDef = null;
        public Il2CppType type = null;
        public List<ResolvedParameter> resolvedParameters = new List<ResolvedParameter>();

        public ResolvedMethod(Il2CppMethodDefinition methodDefinition, Int32 methodIdx)
        {
            methodIndex = methodIdx;
            methodDef = methodDefinition;
            type = il2cpp.types[methodDef.returnType];
            Name = MetadataReader.GetString(methodDef.nameIndex);
            for (int i = 0; i < methodDef.parameterCount; i++)
            {
                ResolvedParameter resolvedParameter = new ResolvedParameter(Metadata.parameterDefinitions[methodDef.parameterStart + i]);
                resolvedParameters.Add(resolvedParameter);
            }
        }

        public string ToCode(Int32 indent = 0)
        {
            string code = "".Indent(indent);

            if (isStatic)
                code += "static ";

            code += $"{MetadataReader.GetTypeString(type)} {Name}(";
            for(int i =0;i<resolvedParameters.Count;i++)
            {
                code += resolvedParameters[i].ToCode();
                if (i < resolvedParameters.Count - 1)
                    code += ", ";
            }
            code += ");\n";
            return code;
        }

        public bool isStatic
        {
            get
            {
                return (methodDef.flags & il2cpp_Constants.METHOD_ATTRIBUTE_STATIC) != 0;
            }
        }
    }
}
