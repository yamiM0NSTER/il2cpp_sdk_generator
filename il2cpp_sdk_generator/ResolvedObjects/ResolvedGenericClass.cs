using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    class ResolvedGenericClass : ResolvedType
    {
        public List<Resolvedil2CppGenericClass> genericClasses = new List<Resolvedil2CppGenericClass>();
        string[] genericClassesNames;

        public ResolvedGenericClass(Il2CppTypeDefinition type, Int32 idx)
        {
            typeDef = type;
            typeDefinitionIndex = idx;
            Name = MetadataReader.GetString(typeDef.nameIndex);
            Namespace = MetadataReader.GetString(typeDef.namespaceIndex);
            Name = Name.Replace("`", "_");
            isMangled = !Name.isCSharpIdentifier();
        }

        public override void Resolve()
        {
            if (isResolved)
                return;
        }

        public override async Task ToHeaderCode(StreamWriter sw, Int32 indent = 0)
        {
            if (!isNested)
            {
                await sw.WriteAsync("#pragma once\n\n".Indent(indent));
                // Namespace
                if (Namespace != "")
                {
                    sw.Write($"namespace {CppNamespace()}\n".Indent(indent));
                    sw.Write("{\n".Indent(indent));
                    indent += 2;
                }
            }

            sw.Write($"// Class TypeDefinitionIndex: {typeDefinitionIndex}\n".Indent(indent));

            //if (typeDef.genericContainerIndex >= 0)
            //    sw.Write$"{genericTemplate}\n".Indent(indent);
            string NestedNameStr = DeclarationString();
            //NestedNameStr = NestedNameStr.Substring(0, NestedNameStr.Length - 2);
            sw.Write($"struct {NestedNameStr}".Indent(indent));
            //sw.Write$"struct {Name}".Indent(indent);
            if (parentType != null)
                sw.Write($" : {parentType.GetFullName()}");
            else
                sw.Write($" : Il2CppObject");
            sw.Write("\n");
            sw.Write("{\n".Indent(indent));
            indent += 2;
            if (nestedTypes.Count > 0)
            {
                for (int i = 0; i < nestedTypes.Count; i++)
                {
                    sw.Write(nestedTypes[i].ForwardDeclaration(indent));
                    sw.Write("\n");
                }
            }

            //genericClassesNames = new string[genericClasses.Count];
            //for (int i = 0; i < genericClasses.Count; i++)
            //{
            //    string test = "";
            //    string[] genericParams = new string[genericClasses[i].classParameters.Count];
            //    if (genericClasses[i].classParameters != null)
            //    {
            //        for (int k = 0; k < genericClasses[i].classParameters.Count; k++)
            //        {
            //            //if (genericClasses[i].classParameters[k].type == Il2CppTypeEnum.IL2CPP_TYPE_VAR)
            //            //{
            //            //    test = "GenGen";
            //            //    break;
            //            //}

            //            //genericParams[k] = MetadataReader.GetTypeString(genericClasses[i].classParameters[k]).Replace("::", "_").Replace("*", "Ptr").Replace("<", "_").Replace(">", "_");
            //            test += MetadataReader.GetTypeString(genericClasses[i].classParameters[k]).Replace("::", "_").Replace("*", "Ptr").Replace("<", "_").Replace(">", "_");
            //            ////genericParams[k] = MetadataReader.GetTypeString(genericClasses[i].classParameters[k]);

            //            if (k < genericClasses[i].classParameters.Count - 1 && !test.EndsWith("_"))
            //            {
            //                test += "_";
            //            }
            //        }
            //    }

            //    //test = string.Format(test, genericParams);
            //    if (genericClassesNames.Contains(test))
            //    {
            //        continue;
            //    }

            //    genericClassesNames[i] = test;
            //    sw.Write($"struct {test};\n".Indent(indent);
            //}

            //if (typeDef.genericContainerIndex == -1)
            //{

            //    if (instanceFields.Count > 0)
            //    {
            //        for (int i = 0; i < instanceFields.Count; i++)
            //        {
            //            sw.Write(instanceFields[i].ToCode(indent + 2);
            //        }
            //        sw.Write("\n";
            //    }
            //    if (instanceProperties.Count > 0)
            //    {
            //        sw.Write("// Instance Properties\n".Indent(indent + 2);
            //        for (int i = 0; i < instanceProperties.Count; i++)
            //        {
            //            sw.Write(instanceProperties[i].ToCode(indent + 2);
            //        }
            //        sw.Write("\n";
            //    }
            //    if (staticProperties.Count > 0)
            //    {
            //        sw.Write("// Static Properties\n".Indent(indent + 2);
            //        for (int i = 0; i < staticProperties.Count; i++)
            //        {
            //            sw.Write(staticProperties[i].ToCode(indent + 2);
            //        }
            //        sw.Write("\n";
            //    }
            //    if (instanceEvents.Count > 0)
            //    {
            //        sw.Write("// Instance Events\n".Indent(indent + 2);
            //        for (int i = 0; i < instanceEvents.Count; i++)
            //        {
            //            sw.Write(instanceEvents[i].ToCode(indent + 2);
            //        }
            //        sw.Write("\n";
            //    }
            //    if (staticEvents.Count > 0)
            //    {
            //        sw.Write("// Static Events\n".Indent(indent + 2);
            //        for (int i = 0; i < staticEvents.Count; i++)
            //        {
            //            sw.Write(staticEvents[i].ToCode(indent + 2);
            //        }
            //        sw.Write("\n";
            //    }
            //    if (instanceMethods.Count > 0)
            //    {
            //        sw.Write("// Instance Methods\n".Indent(indent + 2);
            //        for (int i = 0; i < instanceMethods.Count; i++)
            //        {
            //            sw.Write(instanceMethods[i].ToHeaderCode(indent + 2);
            //        }
            //        sw.Write("\n";
            //    }
            //    if (staticMethods.Count > 0)
            //    {
            //        sw.Write("// Static Methods\n".Indent(indent + 2);
            //        for (int i = 0; i < staticMethods.Count; i++)
            //        {
            //            sw.Write(staticMethods[i].ToHeaderCode(indent + 2);
            //        }
            //        sw.Write("\n";
            //    }
            //}
            // TODO: Fields, static fields, const fields, properties, methods, (generics), nested types


            // il2cpp stuff
            sw.Write("\n");
            //sw.Write("// il2cpp stuff\n".Indent(indent + 2);
            //sw.Write("static Il2CppClass* il2cppClass;\n".Indent(indent + 2);
            //sw.Write("// methods\n".Indent(indent + 2);
            //for (int i = 0; i < miMethods.Count; i++)
            //{
            //    sw.Write($"static ::MethodInfo* {miMethods[i].MI_Name};\n".Indent(indent + 2);
            //}
            //sw.Write("static void il2cpp_init();\n".Indent(indent + 2);
            indent -= 2;
            sw.Write("};\n".Indent(indent));

            //for (int i = 0; i < genericClasses.Count; i++)
            //{
            //    if (genericClassesNames[i] == null)
            //        continue;

            //    string test = Metadata.resolvedTypes[typeDefinitionIndex].ReturnTypeString();
            //    if(test.Contains('{'))
            //    {
            //        test = test.Substring(0, test.IndexOf('{') - 2);
            //    }

            //    test += "::";
            //    test += genericClassesNames[i];
            //    string[] genericParams = new string[genericClasses[i].classParameters.Count];

            //    sw.Write($"struct {test} : {Name}\n".Indent(indent);
            //    sw.Write("{\n".Indent(indent);
            //    sw.Write("};\n".Indent(indent);
            //}

            if (nestedTypes.Count > 0)
            {
                for (int i = 0; i < nestedTypes.Count; i++)
                {
                    sw.Write(nestedTypes[i].ToHeaderCode(indent));
                    sw.Write("\n");
                }
            }

            // Namespace
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

            //if (typeDef.genericContainerIndex >= 0)
            //    code += $"{genericTemplate}\n".Indent(indent);
            string NestedNameStr = DeclarationString();
            //NestedNameStr = NestedNameStr.Substring(0, NestedNameStr.Length - 2);
            code += $"struct {NestedNameStr}".Indent(indent);
            //code += $"struct {Name}".Indent(indent);
            if (parentType != null)
                code += $" : {parentType.GetFullName()}";
            else
                code += $" : Il2CppObject";
            code += "\n";
            code += "{\n".Indent(indent);
            indent += 2;
            if (nestedTypes.Count > 0)
            {
                for (int i = 0; i < nestedTypes.Count; i++)
                {
                    code += nestedTypes[i].ForwardDeclaration(indent);
                    code += "\n";
                }
            }

            //genericClassesNames = new string[genericClasses.Count];
            //for (int i = 0; i < genericClasses.Count; i++)
            //{
            //    string test = "";
            //    string[] genericParams = new string[genericClasses[i].classParameters.Count];
            //    if (genericClasses[i].classParameters != null)
            //    {
            //        for (int k = 0; k < genericClasses[i].classParameters.Count; k++)
            //        {
            //            //if (genericClasses[i].classParameters[k].type == Il2CppTypeEnum.IL2CPP_TYPE_VAR)
            //            //{
            //            //    test = "GenGen";
            //            //    break;
            //            //}

            //            //genericParams[k] = MetadataReader.GetTypeString(genericClasses[i].classParameters[k]).Replace("::", "_").Replace("*", "Ptr").Replace("<", "_").Replace(">", "_");
            //            test += MetadataReader.GetTypeString(genericClasses[i].classParameters[k]).Replace("::", "_").Replace("*", "Ptr").Replace("<", "_").Replace(">", "_");
            //            ////genericParams[k] = MetadataReader.GetTypeString(genericClasses[i].classParameters[k]);

            //            if (k < genericClasses[i].classParameters.Count - 1 && !test.EndsWith("_"))
            //            {
            //                test += "_";
            //            }
            //        }
            //    }

            //    //test = string.Format(test, genericParams);
            //    if (genericClassesNames.Contains(test))
            //    {
            //        continue;
            //    }

            //    genericClassesNames[i] = test;
            //    code += $"struct {test};\n".Indent(indent);
            //}

            //if (typeDef.genericContainerIndex == -1)
            //{

            //    if (instanceFields.Count > 0)
            //    {
            //        for (int i = 0; i < instanceFields.Count; i++)
            //        {
            //            code += instanceFields[i].ToCode(indent + 2);
            //        }
            //        code += "\n";
            //    }
            //    if (instanceProperties.Count > 0)
            //    {
            //        code += "// Instance Properties\n".Indent(indent + 2);
            //        for (int i = 0; i < instanceProperties.Count; i++)
            //        {
            //            code += instanceProperties[i].ToCode(indent + 2);
            //        }
            //        code += "\n";
            //    }
            //    if (staticProperties.Count > 0)
            //    {
            //        code += "// Static Properties\n".Indent(indent + 2);
            //        for (int i = 0; i < staticProperties.Count; i++)
            //        {
            //            code += staticProperties[i].ToCode(indent + 2);
            //        }
            //        code += "\n";
            //    }
            //    if (instanceEvents.Count > 0)
            //    {
            //        code += "// Instance Events\n".Indent(indent + 2);
            //        for (int i = 0; i < instanceEvents.Count; i++)
            //        {
            //            code += instanceEvents[i].ToCode(indent + 2);
            //        }
            //        code += "\n";
            //    }
            //    if (staticEvents.Count > 0)
            //    {
            //        code += "// Static Events\n".Indent(indent + 2);
            //        for (int i = 0; i < staticEvents.Count; i++)
            //        {
            //            code += staticEvents[i].ToCode(indent + 2);
            //        }
            //        code += "\n";
            //    }
            //    if (instanceMethods.Count > 0)
            //    {
            //        code += "// Instance Methods\n".Indent(indent + 2);
            //        for (int i = 0; i < instanceMethods.Count; i++)
            //        {
            //            code += instanceMethods[i].ToHeaderCode(indent + 2);
            //        }
            //        code += "\n";
            //    }
            //    if (staticMethods.Count > 0)
            //    {
            //        code += "// Static Methods\n".Indent(indent + 2);
            //        for (int i = 0; i < staticMethods.Count; i++)
            //        {
            //            code += staticMethods[i].ToHeaderCode(indent + 2);
            //        }
            //        code += "\n";
            //    }
            //}
            // TODO: Fields, static fields, const fields, properties, methods, (generics), nested types


            // il2cpp stuff
            code += "\n";
            //code += "// il2cpp stuff\n".Indent(indent + 2);
            //code += "static Il2CppClass* il2cppClass;\n".Indent(indent + 2);
            //code += "// methods\n".Indent(indent + 2);
            //for (int i = 0; i < miMethods.Count; i++)
            //{
            //    code += $"static ::MethodInfo* {miMethods[i].MI_Name};\n".Indent(indent + 2);
            //}
            //code += "static void il2cpp_init();\n".Indent(indent + 2);
            indent -= 2;
            code += "};\n".Indent(indent);

            //for (int i = 0; i < genericClasses.Count; i++)
            //{
            //    if (genericClassesNames[i] == null)
            //        continue;

            //    string test = Metadata.resolvedTypes[typeDefinitionIndex].ReturnTypeString();
            //    if(test.Contains('{'))
            //    {
            //        test = test.Substring(0, test.IndexOf('{') - 2);
            //    }

            //    test += "::";
            //    test += genericClassesNames[i];
            //    string[] genericParams = new string[genericClasses[i].classParameters.Count];

            //    code += $"struct {test} : {Name}\n".Indent(indent);
            //    code += "{\n".Indent(indent);
            //    code += "};\n".Indent(indent);
            //}

            if (nestedTypes.Count > 0)
            {
                for (int i = 0; i < nestedTypes.Count; i++)
                {
                    code += nestedTypes[i].ToHeaderCode(indent);
                    code += "\n";
                }
            }

            // Namespace
            if (Namespace != "" && !isNested)
            {
                indent -= 2;
                code += "}\n".Indent(indent);
            }

            return code;
        }

        public override async Task ToCppCode(StreamWriter sw, Int32 indent = 0)
        {
            if (!isNested)
            {
                await sw.WriteAsync("#include \"pch.h\"\n\n".Indent(indent));
                // Namespace
                if (Namespace != "")
                {
                    sw.Write($"namespace {CppNamespace()}\n".Indent(indent));
                    sw.Write("{\n".Indent(indent));
                    indent += 2;
                }
            }
            // TODO: Init any static fields
            string NestedNameStr = NestedName();
            //sw.Write($"Il2CppClass* {NestedNameStr}il2cppClass = nullptr;\n".Indent(indent);
            //for (int i = 0; i < miMethods.Count; i++)
            //{
            //    sw.Write($"::MethodInfo* {NestedNameStr}{miMethods[i].MI_Name} = nullptr;\n".Indent(indent);
            //}

            //if (typeDef.genericContainerIndex == -1)
            //{
            //    // methods
            //    for (int i = 0; i < instanceMethods.Count; i++)
            //    {
            //        sw.Write(instanceMethods[i].ToCppCode(indent);
            //    }
            //}
            // TODO: nested types
            for (int i = 0; i < nestedTypes.Count; i++)
            {
                sw.Write(nestedTypes[i].ToCppCode(indent + 2));
                sw.Write("\n");
            }

            // il2cpp_init
            //sw.Write($"void {NestedNameStr}il2cpp_init()\n".Indent(indent);
            //sw.Write("{\n".Indent(indent);
            //// TODO: grab right type from typedef
            //sw.Write($"il2cppClass = il2cpp::class_from_il2cpp_type(il2cpp::m_pMetadataRegistration->types[{typeDef.byvalTypeIndex}]);\n".Indent(indent + 2);
            //sw.Write("il2cpp::runtime_class_init(il2cppClass);\n".Indent(indent + 2);
            // Methods
            //for (int i = 0; i < miMethods.Count; i++)
            //{
            //    sw.Write($"{miMethods[i].MI_Name} = (::MethodInfo*)il2cppClass->methods[{miMethods[i].methodIndex - typeDef.methodStart}];\n".Indent(indent + 2);
            //}
            //sw.Write("}\n".Indent(indent);
            // end of Namespace
            if (Namespace != "" && !isNested)
            {
                indent -= 2;
                sw.Write("}\n".Indent(indent));
            }
        }

        public override string ToCppCode(Int32 indent = 0)
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
            // TODO: Init any static fields
            string NestedNameStr = NestedName();
            //code += $"Il2CppClass* {NestedNameStr}il2cppClass = nullptr;\n".Indent(indent);
            //for (int i = 0; i < miMethods.Count; i++)
            //{
            //    code += $"::MethodInfo* {NestedNameStr}{miMethods[i].MI_Name} = nullptr;\n".Indent(indent);
            //}

            //if (typeDef.genericContainerIndex == -1)
            //{
            //    // methods
            //    for (int i = 0; i < instanceMethods.Count; i++)
            //    {
            //        code += instanceMethods[i].ToCppCode(indent);
            //    }
            //}
            // TODO: nested types
            for (int i = 0; i < nestedTypes.Count; i++)
            {
                code += nestedTypes[i].ToCppCode(indent + 2);
                code += "\n";
            }

            // il2cpp_init
            //code += $"void {NestedNameStr}il2cpp_init()\n".Indent(indent);
            //code += "{\n".Indent(indent);
            //// TODO: grab right type from typedef
            //code += $"il2cppClass = il2cpp::class_from_il2cpp_type(il2cpp::m_pMetadataRegistration->types[{typeDef.byvalTypeIndex}]);\n".Indent(indent + 2);
            //code += "il2cpp::runtime_class_init(il2cppClass);\n".Indent(indent + 2);
            // Methods
            //for (int i = 0; i < miMethods.Count; i++)
            //{
            //    code += $"{miMethods[i].MI_Name} = (::MethodInfo*)il2cppClass->methods[{miMethods[i].methodIndex - typeDef.methodStart}];\n".Indent(indent + 2);
            //}
            //code += "}\n".Indent(indent);
            // end of Namespace
            if (Namespace != "" && !isNested)
            {
                indent -= 2;
                code += "}\n".Indent(indent);
            }

            return code;
        }

        public override string DemangledPrefix()
        {
            if (parentType == Demangler.multicastDelegateType)
            {
                return base.DemangledPrefix() + "Callback";
            }
            
            return base.DemangledPrefix() + "GenericClass";
        }

        public override void Demangle(bool force = false)
        {
            if (isDemangled)
                return;

            base.Demangle(force);

            Dictionary<string, Int32> demangledPrefixes = new Dictionary<string, Int32>();
            if (parentType != null)
            {
                // If type inherits make sure parent is demangled first
                if (!Demangler.demangledTypes.TryGetValue(parentType, out var demangledPrefixesParent))
                {
                    if (!Demangler.demangleQueue.TryGetValue(parentType, out var list))
                    {
                        list = new List<ResolvedType>();
                        Demangler.demangleQueue.Add(parentType, list);
                    }
                    list.Add(this);
                    return;
                }
                // Make copy so other inheriting classes can still use parent dictionary
                demangledPrefixes = new Dictionary<string, int>(demangledPrefixesParent);
            }
            // Demangle here

            // Demangle fields
            //for (int i = 0; i < instanceFields.Count; i++)
            //{
            //    ResolvedField resolvedField = instanceFields[i];
            //    if (resolvedField.Name.isCSharpIdentifier())
            //    {
            //        if (resolvedField.Name.isCppIdentifier())
            //            continue;

            //        resolvedField.Name = resolvedField.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
            //        continue;
            //    }

            //    string demangledPrefix = instanceFields[i].DemangledPrefix();
            //    if (!demangledPrefix.EndsWith("_"))
            //        demangledPrefix += "_";
            //    if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx))
            //    {
            //        idx = 0;
            //        demangledPrefixes.Add(demangledPrefix, idx);
            //    }
            //    idx++;
            //    resolvedField.Name = $"{demangledPrefix}{idx}";
            //    demangledPrefixes[demangledPrefix] = idx;
            //    //code += instanceFields[i].ToCode(indent + 2);
            //}

            //// Demangle properties
            //for (int i = 0; i < instanceProperties.Count; i++)
            //{
            //    ResolvedProperty resolvedProperty = instanceProperties[i];
            //    if (resolvedProperty.Name.isCSharpIdentifier())
            //    {
            //        if (resolvedProperty.Name.isCppIdentifier())
            //            continue;

            //        resolvedProperty.EnsureCppMethodNames();
            //        resolvedProperty.Name = resolvedProperty.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
            //        continue;
            //    }

            //    string demangledPrefix = resolvedProperty.DemangledPrefix();
            //    if (!demangledPrefix.EndsWith("_"))
            //        demangledPrefix += "_";
            //    if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx))
            //    {
            //        idx = 0;
            //        demangledPrefixes.Add(demangledPrefix, idx);
            //    }
            //    idx++;
            //    resolvedProperty.Name = $"{demangledPrefix}{idx}";
            //    demangledPrefixes[demangledPrefix] = idx;
            //    resolvedProperty.DemangleMethods();
            //}

            //for (int i = 0; i < staticProperties.Count; i++)
            //{
            //    ResolvedProperty resolvedProperty = staticProperties[i];
            //    if (resolvedProperty.Name.isCSharpIdentifier())
            //    {
            //        if (resolvedProperty.Name.isCppIdentifier())
            //            continue;

            //        resolvedProperty.EnsureCppMethodNames();
            //        resolvedProperty.Name = resolvedProperty.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
            //        continue;
            //    }

            //    string demangledPrefix = resolvedProperty.DemangledPrefix();
            //    if (!demangledPrefix.EndsWith("_"))
            //        demangledPrefix += "_";
            //    if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx))
            //    {
            //        idx = 0;
            //        demangledPrefixes.Add(demangledPrefix, idx);
            //    }
            //    idx++;
            //    resolvedProperty.Name = $"{demangledPrefix}{idx}";
            //    demangledPrefixes[demangledPrefix] = idx;
            //    resolvedProperty.DemangleMethods();
            //}
            //// Demangle Events
            //for (int i = 0; i < instanceEvents.Count; i++)
            //{
            //    ResolvedEvent resolvedEvent = instanceEvents[i];
            //    if (resolvedEvent.Name.isCSharpIdentifier())
            //    {
            //        if (resolvedEvent.Name.isCppIdentifier())
            //            continue;

            //        resolvedEvent.Name = resolvedEvent.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
            //        continue;
            //    }

            //    string demangledPrefix = resolvedEvent.DemangledPrefix();
            //    if (!demangledPrefix.EndsWith("_"))
            //        demangledPrefix += "_";
            //    if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx))
            //    {
            //        idx = 0;
            //        demangledPrefixes.Add(demangledPrefix, idx);
            //    }
            //    idx++;
            //    resolvedEvent.Name = $"{demangledPrefix}{idx}";
            //    demangledPrefixes[demangledPrefix] = idx;
            //    resolvedEvent.DemangleMethods();
            //}
            //for (int i = 0; i < staticEvents.Count; i++)
            //{
            //    ResolvedEvent resolvedEvent = staticEvents[i];
            //    if (resolvedEvent.Name.isCSharpIdentifier())
            //    {
            //        if (resolvedEvent.Name.isCppIdentifier())
            //            continue;

            //        resolvedEvent.Name = resolvedEvent.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
            //        continue;
            //    }

            //    string demangledPrefix = resolvedEvent.DemangledPrefix();
            //    if (!demangledPrefix.EndsWith("_"))
            //        demangledPrefix += "_";
            //    if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx))
            //    {
            //        idx = 0;
            //        demangledPrefixes.Add(demangledPrefix, idx);
            //    }
            //    idx++;
            //    resolvedEvent.Name = $"{demangledPrefix}{idx}";
            //    demangledPrefixes[demangledPrefix] = idx;
            //    resolvedEvent.DemangleMethods();
            //}

            //// Demangle Methods
            //for (int i = 0; i < instanceMethods.Count; i++)
            //{
            //    ResolvedMethod resolvedMethod = instanceMethods[i];
            //    resolvedMethod.DemangleParams();
            //    if (resolvedMethod.Name.isCSharpIdentifier())
            //    {
            //        if (resolvedMethod.Name.isCppIdentifier())
            //            continue;

            //        resolvedMethod.Name = resolvedMethod.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
            //        continue;
            //    }

            //    string demangledPrefix = resolvedMethod.DemangledPrefix();
            //    if (!demangledPrefix.EndsWith("_"))
            //        demangledPrefix += "_";
            //    if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx))
            //    {
            //        idx = 0;
            //        demangledPrefixes.Add(demangledPrefix, idx);
            //    }
            //    idx++;
            //    resolvedMethod.Name = $"{demangledPrefix}{idx}";
            //    demangledPrefixes[demangledPrefix] = idx;
            //}

            //for (int i = 0; i < staticMethods.Count; i++)
            //{
            //    ResolvedMethod resolvedMethod = staticMethods[i];
            //    resolvedMethod.DemangleParams();
            //    if (resolvedMethod.Name.isCSharpIdentifier())
            //    {
            //        if (resolvedMethod.Name.isCppIdentifier())
            //            continue;

            //        resolvedMethod.Name = resolvedMethod.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
            //        continue;
            //    }

            //    string demangledPrefix = resolvedMethod.DemangledPrefix();
            //    if (!demangledPrefix.EndsWith("_"))
            //        demangledPrefix += "_";
            //    if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx))
            //    {
            //        idx = 0;
            //        demangledPrefixes.Add(demangledPrefix, idx);
            //    }
            //    idx++;
            //    resolvedMethod.Name = $"{demangledPrefix}{idx}";
            //    demangledPrefixes[demangledPrefix] = idx;
            //}

            //instanceMethods.Sort(SortMethods);
            //staticMethods.Sort(SortMethods);

            Rules.AssignResolvedObject(this);

            isDemangled = true;
            // Add type as demangled
            Demangler.demangledTypes.Add(this, demangledPrefixes);
            // Resolve all types waiting for this type
            if (!Demangler.demangleQueue.TryGetValue(this, out var demangleQueue))
                return;
            // remove from queue
            Demangler.demangleQueue.Remove(this);

            for (int i = 0; i < demangleQueue.Count; i++)
            {
                demangleQueue[i].Demangle(force);
            }
        }

        public override string ForwardDeclaration(Int32 indent = 0)
        {
            return $"struct {Name};\n".Indent(indent);
        }

        public void TestInsideTypes()
        {
            for(int i =0;i<genericClasses.Count;i++)
            {
                string test = Metadata.resolvedTypes[typeDefinitionIndex].ReturnTypeString();
                test += "::";

                string[] genericParams = new string[genericClasses[i].classParameters.Count];
                if (genericClasses[i].classParameters != null)
                {
                    for(int k = 0;k< genericClasses[i].classParameters.Count;k++)
                    {
                        if(genericClasses[i].classParameters[k].type == Il2CppTypeEnum.IL2CPP_TYPE_VAR)
                        {
                            test = "GenGen";
                            break;
                        }

                        test += MetadataReader.GetSimpleTypeString(genericClasses[i].classParameters[k]);
                        //genericParams[k] = MetadataReader.GetTypeString(genericClasses[i].classParameters[k]);

                        if(k < genericClasses[i].classParameters.Count-1 && !test.EndsWith("_"))
                        {
                            test += "_";
                        }
                    }
                }
                //Il2CppGenericInst generic_inst = il2cppReader.GetIl2CppGenericInst(genericClasses[i].context.class_instPtr);
                
                //ulong[] pointers = il2cppReader.GetGenericInstPointerArray(generic_inst.type_argv, (Int32)generic_inst.type_argc);
                //for (int k = 0; k < pointers.Length; k++)
                //{
                //    genericParams[k] = MetadataReader.GetTypeString(il2cppReader.GetIl2CppType(pointers[k]));
                //}
                //if (Metadata.resolvedTypes[typeDefinitionIndex] is ResolvedClass)
                //    test += "*";
                //test = string.Format(test, genericParams);
            }
        }
    }
}
