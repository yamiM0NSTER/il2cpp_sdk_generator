using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    public class ResolvedMethod : ResolvedObject
    {
        public Int32 methodIndex;
        public Il2CppMethodDefinition methodDef = null;
        public Il2CppType returnType = null;
        public List<ResolvedParameter> resolvedParameters = new List<ResolvedParameter>();
        public ResolvedType declaringType = null;
        public string MI_Name = "";
        public List<ulong> refAddrs = new List<ulong>();
        public List<ResolvedMethod> refMethods = new List<ResolvedMethod>();
        public bool isReferenced { get; set; }

        public ResolvedMethod(Il2CppMethodDefinition methodDefinition, Int32 methodIdx)
        {
            methodIndex = methodIdx;
            methodDef = methodDefinition;
            returnType = il2cpp.types[methodDef.returnType];
            Name = MetadataReader.GetString(methodDef.nameIndex);
            for (int i = 0; i < methodDef.parameterCount; i++)
            {
                ResolvedParameter resolvedParameter = new ResolvedParameter(Metadata.parameterDefinitions[methodDef.parameterStart + i]);
                resolvedParameters.Add(resolvedParameter);
            }
            isMangled = !Name.isCSharpIdentifier();
            if (isMangled)
                isReferenced = false;
        }

        public string ToHeaderCode(Int32 indent = 0)
        {
            string code = "".Indent(indent);

            if (isStatic)
                code += "static ";

            code += $"{MetadataReader.GetTypeString(returnType)} {Name}(";
            for(int i =0;i<resolvedParameters.Count;i++)
            {
                code += resolvedParameters[i].ToCode();
                if (i < resolvedParameters.Count - 1)
                    code += ", ";
            }
            code += ");\n";
            return code;
        }

        public string ToCppCode(Int32 indent = 0)
        {
            bool needsPtr = false;
            if (returnType.type != Il2CppTypeEnum.IL2CPP_TYPE_PTR && returnType.type != Il2CppTypeEnum.IL2CPP_TYPE_CLASS)
                needsPtr = true;
            string code = "".Indent(indent);
            string returnTypeStr = MetadataReader.GetTypeString(returnType);
            code += $"{returnTypeStr} {declaringType.NestedName()}{Name}(";
            for (int i = 0; i < resolvedParameters.Count; i++)
            {
                code += resolvedParameters[i].ToCode();
                if (i < resolvedParameters.Count - 1)
                    code += ", ";
            }
            code += ")\n";
            code += "{\n".Indent(indent);
            string paramStr = "nullptr";
            string instanceStr = isStatic ? "nullptr" : "this";
            if(resolvedParameters.Count > 0)
            {
                paramStr = "params";
                code += $"void* params[{resolvedParameters.Count}] = ".Indent(indent+2);
                code += "{ ";
                for(int i =0;i< resolvedParameters.Count;i++)
                {
                    if (resolvedParameters[i].type.type == Il2CppTypeEnum.IL2CPP_TYPE_VALUETYPE || 
                        resolvedParameters[i].type.type == Il2CppTypeEnum.IL2CPP_TYPE_BOOLEAN ||
                        resolvedParameters[i].type.type == Il2CppTypeEnum.IL2CPP_TYPE_R4 ||
                        resolvedParameters[i].type.type == Il2CppTypeEnum.IL2CPP_TYPE_R8)
                        code += "&";
                    code += resolvedParameters[i].Name;
                    if (i < resolvedParameters.Count - 1)
                        code += ", ";
                }
                code += "};\n";
            }

            //void* params[4] = { parameter_1, &parameter_2, &parameter_3, &parameter_4 };
            code += "".Indent(indent + 2);
            if (returnTypeStr != "void")
                code += "Il2CppBoxedObject* ret = ";
            code += $"il2cpp::runtime_invoke({MI_Name}, {instanceStr}, {paramStr});\n";
            //resolvedParameters.Count
            
            if (returnTypeStr != "void")
            {
                code += $"return ".Indent(indent + 2);
                if (needsPtr)
                    code += "*";
                code += $"({returnTypeStr}";
                if (needsPtr)
                    code += "*";
                code += $")il2cpp::object_unbox(ret);\n";
            }
            
            // code
            code += "}\n".Indent(indent);
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
            string prefix = $"{ReferencedString()}m{StaticString()}_{VirtualString()}{AccessString()}{MetadataReader.GetSimpleTypeString(returnType)}";
            for(int i =0;i<resolvedParameters.Count;i++)
            {
                if (!prefix.EndsWith("_"))
                    prefix += "_";

                prefix += MetadataReader.GetSimpleTypeString(resolvedParameters[i].type);
            }
            return prefix;
        }

        public string ReferencedString()
        {
            if (isReferenced || !isMangled)
                return "";
            return "u";
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
