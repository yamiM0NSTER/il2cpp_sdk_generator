using System;
using System.Collections.Generic;
using System.IO;
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
            isMangled = !Name.isCSharpIdentifier();
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

        public async Task ToCppCode(StreamWriter sw, Int32 indent)
        {
            if (propDef.get > -1)
            {
                await getter.ToCppCode(sw, indent);
                await sw.WriteLineAsync();
            }

            if (propDef.set > -1)
            {
                await setter.ToCppCode(sw, indent);
                await sw.WriteLineAsync();
            }
        }

        public string ToCppCode(Int32 indent)
        {
            string code = "";

            //code += $"// Property: {Name}\n".Indent(indent);

            if (propDef.get > -1)
            {
                //code += "// Getter\n".Indent(indent);
                code += getter.ToCppCode(indent);
                code += "\n";
            }

            if (propDef.set > -1)
            {
                //code += "// Setter\n".Indent(indent);
                code += setter.ToCppCode(indent);
                code += "\n";
            }

            return code;
        }

        public async Task ToHeaderCode(StreamWriter sw, Int32 indent)
        {
            await sw.WriteLineAsync($"// Property: {Name}".Indent(indent));

            Il2CppType returnType = null;

            string propertyString = $"__declspec(property (".Indent(indent);

            bool bMakePropString = true;

            if (propDef.get > -1)
            {
                propertyString += $"get={getter.Name.CSharpToCppIdentifier()}";
                returnType = getter.returnType;

                bMakePropString = getter.resolvedParameters.Count == 0;

                await sw.WriteLineAsync("// Getter".Indent(indent));
                await getter.ToHeaderCode(sw, indent);
            }
            
            if (propDef.get > -1 && propDef.set > -1)
            {
                propertyString += ",";
            }

            if (propDef.set > -1)
            {
                propertyString += $"put={setter.Name.CSharpToCppIdentifier()}";
                returnType = setter.resolvedParameters[0].type;

                if (bMakePropString)
                    bMakePropString = setter.resolvedParameters.Count == 1;

                await sw.WriteLineAsync("// Setter".Indent(indent));
                await setter.ToHeaderCode(sw, indent);
            }

            if (!this.isStatic && bMakePropString)
            {
                propertyString += $")) {MetadataReader.GetTypeString(returnType)} {this.Name.CSharpToCppIdentifier()};";
                await sw.WriteLineAsync(propertyString);
            }
        }

        public string ToHeaderCode(Int32 indent)
        {
            string code = "";

            code += $"// Property: {Name}\n".Indent(indent);

            Il2CppType returnType = null;

            string propertyString = $"__declspec(property (".Indent(indent);

            bool bMakePropString = true;

            if (propDef.get> -1)
            {
                propertyString += $"get={getter.Name.CSharpToCppIdentifier()}";
                returnType = getter.returnType;

                bMakePropString = getter.resolvedParameters.Count == 0;

                code += "// Getter\n".Indent(indent);
                code += getter.ToHeaderCode(indent);
            }

            if (propDef.get > -1 && propDef.set > -1)
            {
                propertyString += ",";
            }

            if (propDef.set > -1)
            {
                propertyString += $"put={setter.Name.CSharpToCppIdentifier()}";
                returnType = setter.resolvedParameters[0].type;

                if(bMakePropString)
                    bMakePropString = setter.resolvedParameters.Count == 1;

                code += "// Setter\n".Indent(indent);
                code += setter.ToHeaderCode(indent);
            }
            
            if(!this.isStatic && bMakePropString)
            {
                propertyString += $")) {MetadataReader.GetTypeString(returnType)} {this.Name.CSharpToCppIdentifier()};\n";
                code += propertyString;
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
