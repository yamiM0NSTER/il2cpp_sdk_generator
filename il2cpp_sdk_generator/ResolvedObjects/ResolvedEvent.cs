using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    public class ResolvedEvent : ResolvedObject
    {
        public Il2CppEventDefinition eventDef = null;
        public ResolvedMethod add = null;
        public ResolvedMethod remove = null;
        public ResolvedMethod raise = null;
        public Il2CppMethodDefinition methodDef = null;

        public ResolvedEvent(Il2CppEventDefinition eventDefinition)
        {
            eventDef = eventDefinition;
            Name = MetadataReader.GetString(eventDef.nameIndex);
            isMangled = !Name.isCSharpIdentifier();
        }

        public void AssignMethod(Int32 idx, ResolvedMethod resolvedMethod)
        {
            if (eventDef.add == idx)
            {
                add = resolvedMethod;
                methodDef = resolvedMethod.methodDef;
                return;
            }

            if (eventDef.remove == idx)
            {
                remove = resolvedMethod;
                if (eventDef.add < 0)
                    methodDef = resolvedMethod.methodDef;
                return;
            }

            if(eventDef.raise == idx)
            {
                raise = resolvedMethod;
                if (eventDef.add < 0 && eventDef.remove < 0)
                    methodDef = resolvedMethod.methodDef;
                return;
            }
        }

        public async Task ToHeaderCode(StreamWriter sw, Int32 indent = 0)
        {
            await sw.WriteLineAsync($"// Event: {Name}".Indent(indent));

            if (eventDef.add > -1)
            {
                await sw.WriteLineAsync("// Add".Indent(indent));
                await add.ToHeaderCode(sw, indent);
            }

            if (eventDef.remove > -1)
            {
                await sw.WriteLineAsync("// Remove".Indent(indent));
                await remove.ToHeaderCode(sw, indent);
            }

            if (eventDef.raise > -1)
            {
                await sw.WriteLineAsync("// Raise".Indent(indent));
                await raise.ToHeaderCode(sw, indent);
            }
        }

        public string ToCode(Int32 indent)
        {
            string code = "";

            code += $"// Event: {Name}\n".Indent(indent);

            if (eventDef.add > -1)
            {
                code += "// Add\n".Indent(indent);
                code += add.ToHeaderCode(indent);
            }

            if (eventDef.remove > -1)
            {
                code += "// Remove\n".Indent(indent);
                code += remove.ToHeaderCode(indent);
            }

            if (eventDef.raise > -1)
            {
                code += "// Raise\n".Indent(indent);
                code += raise.ToHeaderCode(indent);
            }
            
            return code;
        }

        public async Task ToCppCode(StreamWriter sw, Int32 indent = 0)
        {
            if (eventDef.add > -1)
            {
                await sw.WriteLineAsync("// Add".Indent(indent));
                await add.ToCppCode(sw, indent);
            }

            if (eventDef.remove > -1)
            {
                await sw.WriteLineAsync("// Remove".Indent(indent));
                await remove.ToCppCode(sw, indent);
            }

            if (eventDef.raise > -1)
            {
                await sw.WriteLineAsync("// Raise".Indent(indent));
                await raise.ToCppCode(sw, indent);
            }
        }

        public string ToCppCode(Int32 indent = 0)
        {
            string code = "";

            if (eventDef.add > -1)
            {
                code += "// Add\n".Indent(indent);
                code += add.ToCppCode(indent);
            }

            if (eventDef.remove > -1)
            {
                code += "// Remove\n".Indent(indent);
                code += remove.ToCppCode(indent);
            }

            if (eventDef.raise > -1)
            {
                code += "// Raise\n".Indent(indent);
                code += raise.ToCppCode(indent);
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
            return $"e{StaticString()}_{AccessString()}{MetadataReader.GetSimpleTypeString(il2cpp.types[methodDef.returnType])}";
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
            if (add != null)
            {
                add.Name = $"add_{Name}";
                add.resolvedParameters[0].Name = "value";
            }
            if (remove != null)
            {
                remove.Name = $"remove_{Name}";
                remove.resolvedParameters[0].Name = "value";
            }
            if (raise != null)
                raise.Name = $"raise_{Name}";
        }
    }
}
