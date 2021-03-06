﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    public class ResolvedProperty : ResolvedObject
    {
        public Il2CppPropertyDefinition propDef = null;
        public ResolvedMethod getter = null;
        public ResolvedMethod setter = null;
        public Il2CppMethodDefinition methodDef = null;

        public ResolvedProperty(Il2CppPropertyDefinition propDefinition)
        {
            propDef = propDefinition;
            Name = MetadataReader.GetString(propDef.nameIndex);
            isMangled = Name.isCSharpIdentifier();
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
                code += getter.ToHeaderCode(indent);
            }

            if (propDef.set > -1)
            {
                code += "// Setter\n".Indent(indent);
                code += setter.ToHeaderCode(indent);
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

        public string DemangledPrefix()
        {
            return $"p{StaticString()}_{AccessString()}{MetadataReader.GetSimpleTypeString(il2cpp.types[methodDef.returnType])}";
        }

        public string StaticString()
        {
            if (isStatic)
                return "s";
            return "";
        }

        public string AccessString()
        {
            var accessFlag = methodDef.flags & il2cpp_Constants.METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK;
            switch (accessFlag)
            {
                case il2cpp_Constants.METHOD_ATTRIBUTE_PRIVATE:
                    return "Private";
                case il2cpp_Constants.METHOD_ATTRIBUTE_PUBLIC:
                    return "Public";
                case il2cpp_Constants.METHOD_ATTRIBUTE_FAMILY:
                    return "Protected";
                case il2cpp_Constants.METHOD_ATTRIBUTE_ASSEM:
                case il2cpp_Constants.METHOD_ATTRIBUTE_FAM_AND_ASSEM:
                    return "Internal";
                case il2cpp_Constants.METHOD_ATTRIBUTE_FAM_OR_ASSEM:
                    return "ProtectedInternal";
            }
            return "";
        }

        public void DemangleMethods()
        {
            if (getter != null)
                getter.Name = $"get_{Name}";

            if (setter != null)
            {
                setter.Name = $"set_{Name}";
                setter.resolvedParameters[0].Name = "value";
            }
        }

        public void EnsureCppMethodNames()
        {
            if (getter != null)
            {
                getter.Name = getter.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
            }
            if (setter != null)
            {
                setter.Name = setter.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
            }
        }
    }
}
