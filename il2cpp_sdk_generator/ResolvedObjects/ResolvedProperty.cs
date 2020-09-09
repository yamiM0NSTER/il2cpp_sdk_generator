using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    class ResolvedProperty : ResolvedObject
    {
        public Il2CppPropertyDefinition propDef = null;
        public ResolvedMethod getter = null;
        public ResolvedMethod setter = null;
        public Il2CppMethodDefinition methodDef = null;

        public ResolvedProperty(Il2CppPropertyDefinition propDefinition)
        {
            propDef = propDefinition;
            Name = MetadataReader.GetString(propDef.nameIndex);
        }

        public void AssignMethod(Int32 idx, ResolvedMethod resolvedMethod)
        {
            if(propDef.get == idx)
            {
                getter = resolvedMethod;
                methodDef = resolvedMethod.methodDef;
                return;
            }

            if (propDef.set == idx)
            {
                setter = resolvedMethod;
                if(propDef.get < 0)
                    methodDef = resolvedMethod.methodDef;
                return;
            }
        }

        public string ToCode(Int32 indent)
        {
            string code = "";

            code += $"// Property: {Name}\n".Indent(indent);

            if (propDef.get> -1)
            {
                code += "// Getter\n".Indent(indent);
                code += getter.ToCode(indent);
            }

            if (propDef.set > -1)
            {
                code += "// Setter\n".Indent(indent);
                code += setter.ToCode(indent);
            }
            
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
