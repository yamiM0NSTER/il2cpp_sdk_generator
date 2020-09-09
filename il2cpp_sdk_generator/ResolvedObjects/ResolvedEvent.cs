using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    class ResolvedEvent : ResolvedObject
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

        public string ToCode(Int32 indent)
        {
            string code = "";

            code += $"// Event: {Name}\n".Indent(indent);

            if (eventDef.add > -1)
            {
                code += "// Add\n".Indent(indent);
                code += add.ToCode(indent);
            }

            if (eventDef.remove > -1)
            {
                code += "// Remove\n".Indent(indent);
                code += remove.ToCode(indent);
            }

            if (eventDef.raise > -1)
            {
                code += "// Raise\n".Indent(indent);
                code += raise.ToCode(indent);
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
