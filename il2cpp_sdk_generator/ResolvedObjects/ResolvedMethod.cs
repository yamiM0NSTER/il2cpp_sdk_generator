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

            if (Name == ".ctor")
            {
                string test = Name.Replace('.', '_');
                string test2 = Name.Replace(".", "_");
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

        public bool isVirtual
        {
            get
            {
                return (methodDef.flags & il2cpp_Constants.METHOD_ATTRIBUTE_VIRTUAL) != 0 && 
                    (methodDef.flags & il2cpp_Constants.METHOD_ATTRIBUTE_VTABLE_LAYOUT_MASK) == il2cpp_Constants.METHOD_ATTRIBUTE_NEW_SLOT;
            }
        }

        public bool isOverride
        {
            get
            {
                UInt32 vtableflags = methodDef.flags & il2cpp_Constants.METHOD_ATTRIBUTE_VTABLE_LAYOUT_MASK;
                return ((methodDef.flags & il2cpp_Constants.METHOD_ATTRIBUTE_VIRTUAL) != 0 && vtableflags != il2cpp_Constants.METHOD_ATTRIBUTE_NEW_SLOT) ||
                    ((methodDef.flags & il2cpp_Constants.METHOD_ATTRIBUTE_FINAL) != 0 && vtableflags == il2cpp_Constants.METHOD_ATTRIBUTE_REUSE_SLOT) ||
                    ((methodDef.flags & il2cpp_Constants.METHOD_ATTRIBUTE_ABSTRACT) != 0 && vtableflags == il2cpp_Constants.METHOD_ATTRIBUTE_REUSE_SLOT);
            }
        }

        public string DemangledPrefix()
        {
            return $"m{StaticString()}_{VirtualString()}{AccessString()}{MetadataReader.GetSimpleTypeString(il2cpp.types[methodDef.returnType])}";
        }

        public string VirtualString()
        {
            if (isVirtual)
                return "Virtual";
            else if (isOverride)
                return "Override";
            return "";
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

        public void DemangleParams()
        {
            for(int i =0;i< resolvedParameters.Count;i++)
            {
                ResolvedParameter resolvedParameter = resolvedParameters[i];
                if (resolvedParameter.Name.isCSharpIdentifier())
                {
                    if (resolvedParameter.Name.isCppIdentifier())
                        continue;

                    resolvedParameter.Name = resolvedParameter.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
                    continue;
                }

                resolvedParameter.Name = $"_{i}";
            }
        }
    }
}
