using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    class ResolvedInterface : ResolvedType
    {
        bool isGeneric = false;
        string genericTemplate = "";

        public ResolvedInterface(Il2CppTypeDefinition type, Int32 idx)
        {
            typeDef = type;
            typeDefinitionIndex = idx;
            Name = MetadataReader.GetString(typeDef.nameIndex);
            Namespace = MetadataReader.GetString(typeDef.namespaceIndex);
        }

        public override void Resolve()
        {
            if (isResolved)
                return;

            if (typeDef.genericContainerIndex >= 0)
            {
                isGeneric = true;
                if (Name.Contains('`'))
                {
                    int idx = Name.IndexOf('`');
                    Name = Name.Remove(idx, Name.Length - idx);
                }

                MakeGenericTemplate();
            }

            isResolved = true;
        }

        public override string ToHeaderCode(Int32 indent = 0)
        {
            string code = "";

            if (!isNested)
            {
                code += "#pragma once\n\n".Indent(indent);
                // Namespace
                if (Namespace != "")
                {
                    code += $"namespace {CppNamespace()}\n".Indent(indent);
                    code += "{\n".Indent(indent);
                    indent += 2;
                }
            }

            code += $"// Interface TypeDefinitionIndex: {typeDefinitionIndex}\n".Indent(indent);

            //if (typeDef.genericContainerIndex >= 0)
            //    code += $"{genericTemplate}\n".Indent(indent);
            code += $"struct {Name}".Indent(indent);
            if (parentType != null)
                code += $" : {parentType.GetFullName()}";
            else
                code += $" : Il2CppObject";
            code += "\n";
            code += "{\n".Indent(indent);

            code += "};\n".Indent(indent);

            // Namespace
            if (Namespace != "" && !isNested)
            {
                indent -= 2;
                code += "}\n".Indent(indent);
            }

            return code;
        }

        private void MakeGenericTemplate()
        {
            Il2CppGenericContainer generic_container = Metadata.genericContainers[typeDef.genericContainerIndex];

            genericTemplate = "template <";
            for (int i = 0; i < generic_container.type_argc; i++)
            {
                Il2CppGenericParameter generic_parameter = Metadata.genericParameters[generic_container.genericParameterStart + i];
                genericTemplate += $"typename {MetadataReader.GetString(generic_parameter.nameIndex)}";
                if (i < generic_container.type_argc - 1)
                    genericTemplate += ", ";
            }
            genericTemplate += ">";
        }

        public override string DemangledPrefix()
        {
            return GetVisibility()+"Interface";
        }
    }
}
