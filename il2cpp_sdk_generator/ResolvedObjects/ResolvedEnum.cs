using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    class ResolvedEnum : ResolvedType
    {
        public Dictionary<string, object> values = new Dictionary<string, object>();

        public ResolvedEnum(Il2CppTypeDefinition type, Int32 idx)
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

            for (int k = 0; k < typeDef.field_count; k++)
            {
                var fieldDef = Metadata.fieldDefinitions[k + typeDef.fieldStart];
                var type = il2cpp.types[fieldDef.typeIndex];
                if ((type.attrs & il2cpp_Constants.FIELD_ATTRIBUTE_STATIC) == 0)
                    continue;

                if(!MetadataReader.GetDefaultFieldValue(k + typeDef.fieldStart, out var val))
                {
                    Console.WriteLine("ResolvedEnum::Resolve() somehow enum value has no value @_@");
                    continue;
                }
                //typeDef.
                UInt32 offset = il2cppReader.GetFieldOffset(typeDefinitionIndex, k);

                this.values.Add(MetadataReader.GetString(fieldDef.nameIndex), val);
            }

            isResolved = true;
        }

        public override string ToCode(Int32 indent = 0)
        {
            string code = "";

            code += $"enum {Name}\n".Indent(indent);
            code += "{\n".Indent(indent);
            foreach(var pair in values)
            {
                code += $"{pair.Key} = {pair.Value},\n".Indent(indent+2);
            }
            code += "};\n".Indent(indent);

            return code;
        }

        public override string DemangledPrefix()
        {
            return GetVisibility() + "Enum";
        }

        public override void Demangle()
        {
            Dictionary<string, object> demangledValues = new Dictionary<string, object>();
            int idx = 1;
            foreach (var valuePair in values)
            {
                if (valuePair.Key.isCSharpIdentifier())
                {
                    if (valuePair.Key.isCppIdentifier())
                    {
                        demangledValues.Add(valuePair.Key, valuePair.Value);
                        continue;
                    }

                    demangledValues.Add(valuePair.Key.Replace('<', '_').Replace('>', '_'), valuePair.Value);
                    continue;
                }

                demangledValues.Add($"Value_{idx}", valuePair.Value);
                idx++;
            }

            values = demangledValues;
        }

        public override Dictionary<string, Int32> Demangle(Dictionary<string, Int32> demangledPrefixesParent = null)
        {
            Dictionary<string, object> demangledValues = new Dictionary<string, object>();
            int idx = 1;
            foreach (var valuePair in values)
            {
                if (valuePair.Key.isCSharpIdentifier())
                {
                    if (valuePair.Key.isCppIdentifier())
                    {
                        demangledValues.Add(valuePair.Key, valuePair.Value);
                        continue;
                    }

                    demangledValues.Add(valuePair.Key.Replace('<', '_').Replace('>', '_'), valuePair.Value);
                    continue;
                }

                demangledValues.Add($"Value_{idx}", valuePair.Value);
                idx++;
            }

            values = demangledValues;

            return null;
        }
    }
}
