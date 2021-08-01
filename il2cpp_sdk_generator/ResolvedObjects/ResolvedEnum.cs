using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    public class ResolvedEnum : ResolvedType
    {
        public Dictionary<string, object> values = new Dictionary<string, object>();

        public ResolvedEnum(Il2CppTypeDefinition type, Int32 idx)
        {
            typeDef = type;
            typeDefinitionIndex = idx;
            Name = MetadataReader.GetString(typeDef.nameIndex);
            Namespace = MetadataReader.GetString(typeDef.namespaceIndex);
            isMangled = !Name.isCSharpIdentifier();
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

                string fieldName = MetadataReader.GetString(fieldDef.nameIndex);

                FixCppMacros(ref fieldName);

                this.values.Add(fieldName, val);
            }

            isResolved = true;
        }

        static string[] macros =
            {
                "ERROR",
                "TRUE",
                "FALSE"
            };

        void FixCppMacros(ref string fieldName)
        {
            if(macros.Contains(fieldName))
            {
                fieldName = $"_{fieldName}";
            }
        }

        public override async Task ToHeaderCode(StreamWriter sw, Int32 indent = 0)
        {
            if (!isNested)
            {
                await sw.WriteAsync("".Indent(indent));
                sw.Write("#include \"pch.h\"\n\n");
                // Namespace
                if (Namespace != "")
                {
                    sw.Write($"namespace {CppNamespace()}\n".Indent(indent));
                    sw.Write("{\n".Indent(indent));
                    indent += 2;
                }
            }

            string NestedNameStr = DeclarationString();
            //code += $"enum class {Name}\n".Indent(indent);
            sw.Write($"enum class {NestedNameStr}\n".Indent(indent));
            sw.Write("{\n".Indent(indent));
            foreach (var pair in values)
            {
                //sw.Write($"{pair.Key} = 0x{pair.Value:X},\n".Indent(indent + 2));
                sw.Write($"{pair.Key} = {pair.Value},\n".Indent(indent + 2));
            }
            sw.Write("};\n".Indent(indent));

            // end of Namespace
            if (Namespace != "" && !isNested)
            {
                indent -= 2;
                sw.Write("}\n".Indent(indent));
            }
        }

        public override string ToHeaderCode(Int32 indent = 0)
        {
            string code = "";

            if (!isNested)
            {
                code = code.Indent(indent);
                code += "#include \"pch.h\"\n\n";
                // Namespace
                if (Namespace != "")
                {
                    code += $"namespace {CppNamespace()}\n".Indent(indent);
                    code += "{\n".Indent(indent);
                    indent += 2;
                }
            }

            string NestedNameStr = DeclarationString();
            //code += $"enum class {Name}\n".Indent(indent);
            code += $"enum class {NestedNameStr}\n".Indent(indent);
            code += "{\n".Indent(indent);
            foreach(var pair in values)
            {
                code += $"{pair.Key} = 0x{pair.Value:X},\n".Indent(indent+2);
            }
            code += "};\n".Indent(indent);

            // end of Namespace
            if (Namespace != "" && !isNested)
            {
                indent -= 2;
                code += "}\n".Indent(indent);
            }

            return code;
        }

        public override string ToHeaderCodeGlobal(Int32 indent = 0)
        {
            string code = "";

            string NestedNameStr = DeclarationString();
            //code += $"enum class {Name}\n".Indent(indent);
            code += $"enum class {NestedNameStr}\n".Indent(indent);
            code += "{\n".Indent(indent);
            foreach (var pair in values)
            {
                //code += $"{pair.Key} = 0x{pair.Value:X},\n".Indent(indent + 2);
                code += $"{pair.Key} = {pair.Value},\n".Indent(indent + 2);
            }
            code += "};\n".Indent(indent);

            return code;
        }

        public override string DemangledPrefix()
        {
            return GetVisibility() + "Enum";
        }

        public override void Demangle(bool force = false)
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

        public override async Task ToCppCode(StreamWriter sw, Int32 indent = 0)
        {
            await Task.Delay(1);
        }

        public override string ToCppCode(Int32 indent = 0)
        {
            return "";
        }

        public override string ForwardDeclaration(Int32 indent = 0)
        {
            return $"enum class {Name};\n".Indent(indent);
        }
    }
}
