using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    class ResolvedClass : ResolvedType
    {
        List<ResolvedField> instanceFields = new List<ResolvedField>();
        List<ResolvedField> staticFields = new List<ResolvedField>();
        List<ResolvedField> constFields = new List<ResolvedField>();

        public ResolvedClass(Il2CppTypeDefinition type, Int32 idx)
        {
            typeDef = type;
            typeDefinitionIndex = idx;
        }

        public override void Resolve()
        {
            if (isResolved)
                return;

            // Resolve fields
            // Resolve static fields
            // Resolve constats (if needed)
            for (int k = 0; k < typeDef.field_count; k++)
            {
                ResolvedField resolvedField = new ResolvedField(Metadata.fieldDefinitions[k + typeDef.fieldStart], this.typeDefinitionIndex, k);

                if (resolvedField.isStatic)
                    staticFields.Add(resolvedField);
                else if(resolvedField.isConst)
                    constFields.Add(resolvedField);
                else
                    instanceFields.Add(resolvedField);
            }
            
            // Resolve methods

            // Resolve properties (and exclude getters/setters from methods?)

            Console.WriteLine("ResolvedClass::Resolve()");
            isResolved = true;
        }

        public override string ToCode(Int32 indent = 0)
        {
            string code = "";
            
            if(!isNested)
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

            code += $"// Class TypeDefinitionIndex: {typeDefinitionIndex}\n".Indent(indent);
            code += $"struct {Name}".Indent(indent);
            if (parentType != null)
                code += $" : {parentType.GetFullName()}";
            else
                code += $" : Il2CppObject";
            code += "\n";
            code += "{\n".Indent(indent);

            for(int i =0;i< instanceFields.Count;i++)
            {
                code += instanceFields[i].ToCode(indent + 2);
            }


            // TODO: Fields, static fields, const fields, properties, methods, (generics), nested types

            for(int i =0;i < nestedTypes.Count;i++)
            {
                code += nestedTypes[i].ToCode(indent+2);
            }

            code += "};\n".Indent(indent);

            // Namespace
            if (Namespace != "" && !isNested)
            {
                indent -= 2;
                code += "}\n".Indent(indent);
            }

            return code;
        }
    }
}
