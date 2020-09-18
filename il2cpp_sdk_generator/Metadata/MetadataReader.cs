using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using int32_t = System.Int32;
using TypeIndex = System.Int32;
using TypeDefinitionIndex = System.Int32;
using EncodedMethodIndex = System.UInt32;

namespace il2cpp_sdk_generator
{
    public class MetadataReader
    {
        private static BinaryReader reader;
        private static MemoryStream stream;

        public MetadataReader(MemoryStream memoryStream)
        {
            stream = memoryStream;
            stream.Position = 0;
            reader = new BinaryReader(memoryStream, Encoding.UTF8);
        }

        // Here we just read whole Metadata file
        public void Read()
        {
            Metadata.header = reader.Read<Il2CppGlobalMetadataHeader>();
            Metadata.header.DumpToConsole();

            stream.Position = Metadata.header.stringLiteralOffset;
            Metadata.stringLiterals = reader.ReadArray<Il2CppStringLiteral>(Metadata.header.stringLiteralCount / typeof(Il2CppStringLiteral).GetSizeOf());
            // For now skip strings
            stream.Position = Metadata.header.eventsOffset;
            Metadata.eventDefinitions = reader.ReadArray<Il2CppEventDefinition>(Metadata.header.eventsCount / typeof(Il2CppEventDefinition).GetSizeOf());
            stream.Position = Metadata.header.propertiesOffset;
            Metadata.propertyDefinitions = reader.ReadArray<Il2CppPropertyDefinition>(Metadata.header.propertiesCount / typeof(Il2CppPropertyDefinition).GetSizeOf());
            stream.Position = Metadata.header.methodsOffset;
            Metadata.methodDefinitions = reader.ReadArray<Il2CppMethodDefinition>(Metadata.header.methodsCount / typeof(Il2CppMethodDefinition).GetSizeOf());
            stream.Position = Metadata.header.parametersOffset;
            Metadata.parameterDefaultValues = reader.ReadArray<Il2CppParameterDefaultValue>(Metadata.header.parametersCount / typeof(Il2CppParameterDefaultValue).GetSizeOf());
            stream.Position = Metadata.header.fieldDefaultValuesOffset;
            Metadata.fieldDefaultValues = reader.ReadArray<Il2CppFieldDefaultValue>(Metadata.header.fieldDefaultValuesCount / typeof(Il2CppFieldDefaultValue).GetSizeOf());
            // fieldAndParameterDefaultValueData
            // Il2CppFieldMarshaledSize
            stream.Position = Metadata.header.parametersOffset;
            Metadata.parameterDefinitions = reader.ReadArray<Il2CppParameterDefinition>(Metadata.header.parametersCount / typeof(Il2CppParameterDefinition).GetSizeOf());
            stream.Position = Metadata.header.fieldsOffset;
            Metadata.fieldDefinitions = reader.ReadArray<Il2CppFieldDefinition>(Metadata.header.fieldsCount / typeof(Il2CppFieldDefinition).GetSizeOf());
            stream.Position = Metadata.header.genericParametersOffset;
            Metadata.genericParameters = reader.ReadArray<Il2CppGenericParameter>(Metadata.header.genericParametersCount / typeof(Il2CppGenericParameter).GetSizeOf());
            stream.Position = Metadata.header.genericParameterConstraintsOffset;
            Metadata.genericParameterConstraintsIndices = reader.ReadArray<TypeIndex>(Metadata.header.genericParameterConstraintsCount / typeof(TypeIndex).GetSizeOf());
            stream.Position = Metadata.header.genericContainersOffset;
            Metadata.genericContainers = reader.ReadArray<Il2CppGenericContainer>(Metadata.header.genericContainersCount / typeof(Il2CppGenericContainer).GetSizeOf());
            stream.Position = Metadata.header.nestedTypesOffset;
            Metadata.nestedTypeIndices = reader.ReadArray<TypeDefinitionIndex>(Metadata.header.nestedTypesCount / typeof(TypeDefinitionIndex).GetSizeOf());
            stream.Position = Metadata.header.interfacesOffset;
            Metadata.interfaceIndices = reader.ReadArray<TypeIndex>(Metadata.header.interfacesCount / typeof(TypeIndex).GetSizeOf());
            stream.Position = Metadata.header.vtableMethodsOffset;
            Metadata.vtableMethodIndices = reader.ReadArray<EncodedMethodIndex>(Metadata.header.vtableMethodsCount / typeof(EncodedMethodIndex).GetSizeOf());
            stream.Position = Metadata.header.interfaceOffsetsOffset;
            Metadata.interfaceOffsetPairs = reader.ReadArray<Il2CppInterfaceOffsetPair>(Metadata.header.interfaceOffsetsCount / typeof(Il2CppInterfaceOffsetPair).GetSizeOf());
            stream.Position = Metadata.header.typeDefinitionsOffset;
            Metadata.typeDefinitions = reader.ReadArray<Il2CppTypeDefinition>(Metadata.header.typeDefinitionsCount / typeof(Il2CppTypeDefinition).GetSizeOf());
            stream.Position = Metadata.header.rgctxEntriesOffset;
            Metadata.rgctxEntries = reader.ReadArray<Il2CppRGCTXDefinition>(Metadata.header.rgctxEntriesCount / typeof(Il2CppRGCTXDefinition).GetSizeOf());
            stream.Position = Metadata.header.imagesOffset;
            Metadata.imageDefinitions = reader.ReadArray<Il2CppImageDefinition>(Metadata.header.imagesCount / typeof(Il2CppImageDefinition).GetSizeOf());
            stream.Position = Metadata.header.assembliesOffset;
            Metadata.assemblies = reader.ReadArray<Il2CppAssemblyDefinition>(Metadata.header.assembliesCount / typeof(Il2CppAssemblyDefinition).GetSizeOf());
            stream.Position = Metadata.header.metadataUsageListsOffset;
            Metadata.metadataUsageLists = reader.ReadArray<Il2CppMetadataUsageList>(Metadata.header.metadataUsageListsCount / typeof(Il2CppMetadataUsageList).GetSizeOf());
            stream.Position = Metadata.header.metadataUsagePairsOffset;
            Metadata.metadataUsagePairs = reader.ReadArray<Il2CppMetadataUsagePair>(Metadata.header.metadataUsagePairsCount / typeof(Il2CppMetadataUsagePair).GetSizeOf());
            stream.Position = Metadata.header.fieldRefsOffset;
            Metadata.fieldReferences = reader.ReadArray<Il2CppFieldRef>(Metadata.header.fieldRefsCount / typeof(Il2CppFieldRef).GetSizeOf());
            stream.Position = Metadata.header.referencedAssembliesOffset;
            Metadata.referencedAssemblies = reader.ReadArray<int32_t>(Metadata.header.referencedAssembliesCount / typeof(int32_t).GetSizeOf());
            stream.Position = Metadata.header.attributesInfoOffset;
            Metadata.attributeTypeRanges = reader.ReadArray<Il2CppCustomAttributeTypeRange>(Metadata.header.attributesInfoCount / typeof(Il2CppCustomAttributeTypeRange).GetSizeOf());
            stream.Position = Metadata.header.attributeTypesOffset;
            Metadata.attributeTypes = reader.ReadArray<TypeIndex>(Metadata.header.attributeTypesCount / typeof(TypeIndex).GetSizeOf());
            stream.Position = Metadata.header.unresolvedVirtualCallParameterRangesOffset;
            Metadata.unresolvedVirtualCallParameterTypes = reader.ReadArray<TypeIndex>(Metadata.header.unresolvedVirtualCallParameterRangesCount / typeof(TypeIndex).GetSizeOf());
            // unresolvedVirtualCallParameterRanges
            // windowsRuntimeTypeNames // Il2CppWindowsRuntimeTypeNamePair
            // exportedTypeDefinitions // TypeDefinitionIndex
        }

        // Here we process whatever can be processed for later use
        public void Process()
        {
            ResolveTypes();

            for(int i =0; i < Metadata.fieldDefaultValues.Length;i++)
            {
                Metadata.mapFieldDefValues.Add(Metadata.fieldDefaultValues[i].fieldIndex, Metadata.fieldDefaultValues[i]);
            }

            foreach (var image in Metadata.imageDefinitions)
            {
                var resolvedImage = ResolveImage(image);
                Metadata.resolvedImages.Add(resolvedImage);
            }
            Console.WriteLine();
        }

        static Dictionary<Int32, string> mapIndexStringCache = new Dictionary<Int32, string>();

        public static string GetString(Int32 idx)
        {
            if(mapIndexStringCache.TryGetValue(idx, out string result))
                return result;

            stream.Position = Metadata.header.stringOffset + idx;
            result = reader.ReadNullTerminatedString();
            mapIndexStringCache.Add(idx, result);
            return result;
        }

        public ResolvedImage ResolveImage(Il2CppImageDefinition image)
        {
            ResolvedImage resolvedImage = new ResolvedImage();
            resolvedImage.Name = GetString(image.nameIndex);

            for(int i = image.typeStart; i< image.typeStart+image.typeCount;i++)
            {
                var resolvedType = Metadata.resolvedTypes[i];
                if (resolvedType.isNested)
                    continue;

                if (!resolvedImage.Namespaces.TryGetValue(resolvedType.Namespace, out var resolvedNamespace))
                {
                    resolvedNamespace = new ResolvedNamespace();
                    resolvedNamespace.Name = resolvedType.Namespace;
                    resolvedImage.Namespaces.Add(resolvedType.Namespace, resolvedNamespace);
                }

                // TODO: depending on type put to correct var. enums, classes/structs?
                resolvedNamespace.Types.Add(resolvedType);

                //if (resolvedImage.Name == "Assembly-CSharp.dll")
                //{
                //    resolvedType.DumpToConsole();
                //    foreach(var nestedType in resolvedType.nestedTypes)
                //    {
                //        nestedType.DumpToConsole(2);
                //    }
                //}
            }

            Console.WriteLine($"Image: {resolvedImage.Name}");
            Console.WriteLine($"Namespaces: {resolvedImage.Namespaces.Count}");
            foreach (var pair in resolvedImage.Namespaces)
            {
                Console.WriteLine($" {pair.Value.Name}: {pair.Value.Types.Count}");
                for (int i = 0; i < pair.Value.Types.Count; i++)
                {
                    Console.WriteLine($"  {pair.Value.Types[i].Name} 0x{pair.Value.Types[i].typeDef.flags:X8}");
                }
                //resolvedImage.Namespaces.
            }


            return resolvedImage;
        }

        public void ResolveTypes()
        {
            Metadata.resolvedTypes = new ResolvedType[Metadata.typeDefinitions.Length];
            for(int i=0;i< Metadata.typeDefinitions.Length;i++)
            {
                ResolveType(i);
                //if (Metadata.typeDefinitions[i].genericContainerIndex < 0)
                //    continue;
            }
        }

        
        const uint ClassSemanticsMask = 32;

        public static ResolvedType ResolveType(Int32 typeIdx)
        {
            if (Metadata.resolvedTypes[typeIdx] != null)
                return Metadata.resolvedTypes[typeIdx];

            var typeDef = Metadata.typeDefinitions[typeIdx];
            
            ResolvedType resolvedType = null;
            
            // TODO: Resolved class, struct, enum, interface
            //resolvedType = new ResolvedType(typeDef, typeIdx);

            if (typeDef.isEnum)
            {
                resolvedType = new ResolvedEnum(typeDef, typeIdx);
            }
            else
            {
                uint flag = typeDef.flags & ClassSemanticsMask;
                uint Interface = 32;
                if (flag == Interface)
                {
                    resolvedType = new ResolvedInterface(typeDef, typeIdx);
                }
                else
                {
                    if (typeDef.isValueType)
                        resolvedType = new ResolvedStruct(typeDef, typeIdx);
                    else
                        resolvedType = new ResolvedClass(typeDef, typeIdx);
                }
            }

            for (int i = 0; i < typeDef.nested_type_count; i++)
            {
                Int32 nestedTypeIndex = Metadata.nestedTypeIndices[typeDef.nestedTypesStart + i];
                ResolvedType nestedType = ResolveType(nestedTypeIndex);
                nestedType.declaringType = resolvedType;
                resolvedType.nestedTypes.Add(nestedType);
            }

            Metadata.resolvedTypes[typeIdx] = resolvedType;
            if (resolvedType.isNested)
                Metadata.nestedTypes.Add(resolvedType);

            return resolvedType;
        }

        // Do it like TryGetValue in Dictionary
        public static bool GetDefaultFieldValue(Int32 fieldIndex, out object val)
        {
            val = null;
            if(!Metadata.mapFieldDefValues.TryGetValue(fieldIndex, out var il2CppFieldDefaultValue))
                return false;

            // !!!!!!!!! requires il2cpp to be processed
            var type = il2cpp.types[il2CppFieldDefaultValue.typeIndex];
            stream.Position = il2CppFieldDefaultValue.dataIndex + Metadata.header.fieldAndParameterDefaultValueDataOffset;
            switch(type.type)
            {
                case Il2CppTypeEnum.IL2CPP_TYPE_U1:
                    {
                        val = reader.Read<byte>();
                        break;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_U2:
                    {
                        val = reader.Read<UInt16>();
                        break;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_U4:
                    {
                        val = reader.Read<UInt32>();
                        break;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_U8:
                    {
                        val = reader.Read<UInt64>();
                        break;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_I1:
                    {
                        val = reader.Read<SByte>();
                        break;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_I2:
                    {
                        val = reader.Read<Int16>();
                        break;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_I4:
                    {
                        val = reader.Read<Int32>();
                        break;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_I8:
                    {
                        val = reader.Read<Int64>();
                        break;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_BOOLEAN:
                    {
                        val = reader.Read<Boolean>();
                        break;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_STRING:
                    {
                        int strlen = reader.Read<Int32>();
                        val = Encoding.UTF8.GetString(reader.ReadBytes(strlen));
                        Console.WriteLine($"GetDefaultFieldValue: string => {val}");
                        break;
                    }
                default:
                    {
                        Console.WriteLine($"GetDefaultFieldValue: unhandled type => {type.type}");
                        return false;
                    }
            }

            return true;
        }

        static Dictionary<Il2CppType, string> mapTypeStringCache = new Dictionary<Il2CppType, string>();

        public static string GetTypeString(Il2CppType type)
        {
            if(mapTypeStringCache.TryGetValue(type, out string result))
                return result;

            switch(type.type)
            {
                case Il2CppTypeEnum.IL2CPP_TYPE_VOID:
                    result = "void";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_BOOLEAN:
                    result = "bool";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_CHAR:
                    result = "Il2CppChar";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_I1: // SBYTE
                    result = "int8_t";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_U1:
                    result = "uint8_t";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_I2:
                    result = "int16_t";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_U2:
                    result = "uint16_t";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_I4:
                    result = "int32_t";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_U4:
                    result = "uint32_t";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_I8:
                    result = "int64_t";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_U8:
                    result = "uint64_t";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_R4:
                    result = "float";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_R8:
                    result = "double";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_STRING:
                    result = "Il2CppString*";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_VALUETYPE:
                    result = Metadata.resolvedTypes[type.data.klassIndex].GetFullName();
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_CLASS:
                    result = $"{Metadata.resolvedTypes[type.data.klassIndex].GetFullName()}*";
                    break;
                // TODO: confirm that's actually equivalent
                case Il2CppTypeEnum.IL2CPP_TYPE_I:
                case Il2CppTypeEnum.IL2CPP_TYPE_U:
                    result = "void*";
                    break;
                // TODO: confirm that's actually equivalent or just Il2CppObject*
                case Il2CppTypeEnum.IL2CPP_TYPE_OBJECT:
                    result = "Il2CppBoxedObject*";
                    break;
                // TODO: figure how the fuck to use c# multidimensional arrays as c++
                case Il2CppTypeEnum.IL2CPP_TYPE_ARRAY:
                    {
                        Il2CppArrayType il2CppArrayType = il2cppReader.GetIl2CppArrayType(type.data.arrayPtr);
                        result = $"MArray<{GetTypeString(il2cppReader.GetIl2CppType(il2CppArrayType.etypePtr))}>*";
                        break;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_SZARRAY:
                        result = $"Array<{GetTypeString(il2cppReader.GetIl2CppType(type.data.typePtr))}>*";
                        break;
                case Il2CppTypeEnum.IL2CPP_TYPE_PTR:
                    result = $"{GetTypeString(il2cppReader.GetIl2CppType(type.data.typePtr))}*";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_GENERICINST:
                    {
                        // TODO: Change to generated structs when ready
                        Il2CppGenericClass generic_class = il2cppReader.GetIl2CppGenericClass(type.data.generic_classPtr);
                        string typeStr = $"{Metadata.resolvedTypes[generic_class.typeDefinitionIndex].GetFullName()}";
                        // For whatever reason generic type names end with ` and digit (eg. List`1)
                        if(typeStr.Contains('`'))
                        {
                            int idx = typeStr.IndexOf('`');
                            typeStr = typeStr.Remove(idx, typeStr.Length - idx);
                        }

                        typeStr += "<";

                        Il2CppGenericInst generic_inst = il2cppReader.GetIl2CppGenericInst(generic_class.context.class_instPtr);
                        ulong[] pointers = il2cppReader.GetGenericInstPointerArray(generic_inst.type_argv, (Int32)generic_inst.type_argc);
                        for(int i =0;i<pointers.Length;i++)
                        {
                            typeStr += GetTypeString(il2cppReader.GetIl2CppType(pointers[i]));
                            if (i < pointers.Length - 1)
                                typeStr += ",";
                        }
                        typeStr += ">*";
                        result = typeStr;
                        break;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_VAR:
                case Il2CppTypeEnum.IL2CPP_TYPE_MVAR:
                    {
                        Il2CppGenericParameter il2CppGenericParameter = Metadata.genericParameters[type.data.genericParameterIndex];
                        result = GetString(il2CppGenericParameter.nameIndex);
                        break;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_TYPEDBYREF:
                    result = "Il2CppTypedRef";
                    break;
                // TODO
                case Il2CppTypeEnum.IL2CPP_TYPE_FNPTR:
                case Il2CppTypeEnum.IL2CPP_TYPE_BYREF:
                case Il2CppTypeEnum.IL2CPP_TYPE_CMOD_REQD:
                case Il2CppTypeEnum.IL2CPP_TYPE_CMOD_OPT:
                case Il2CppTypeEnum.IL2CPP_TYPE_INTERNAL:
                case Il2CppTypeEnum.IL2CPP_TYPE_MODIFIER:
                case Il2CppTypeEnum.IL2CPP_TYPE_SENTINEL:
                case Il2CppTypeEnum.IL2CPP_TYPE_PINNED:
                case Il2CppTypeEnum.IL2CPP_TYPE_ENUM:
                    result = "x";
                    break;
                default:
                    result = "Type";
                    break;
            }

            mapTypeStringCache.Add(type, result);
            return result;
        }

        public static string GetSimpleTypeString(Il2CppType type)
        {
            //if (mapTypeStringCache.TryGetValue(type, out string result))
            //    return result;
            string result;

            switch (type.type)
            {
                case Il2CppTypeEnum.IL2CPP_TYPE_VOID:
                    result = "void";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_BOOLEAN:
                    result = "bool";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_CHAR:
                    result = "Il2CppChar";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_I1: // SBYTE
                    result = "int8_t";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_U1:
                    result = "uint8_t";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_I2:
                    result = "int16_t";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_U2:
                    result = "uint16_t";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_I4:
                    result = "int32_t";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_U4:
                    result = "uint32_t";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_I8:
                    result = "int64_t";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_U8:
                    result = "uint64_t";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_R4:
                    result = "float";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_R8:
                    result = "double";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_STRING:
                    result = "Il2CppString";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_VALUETYPE:
                    result = Metadata.resolvedTypes[type.data.klassIndex].Name;
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_CLASS:
                    result = $"{Metadata.resolvedTypes[type.data.klassIndex].Name}";
                    break;
                // TODO: confirm that's actually equivalent
                case Il2CppTypeEnum.IL2CPP_TYPE_I:
                case Il2CppTypeEnum.IL2CPP_TYPE_U:
                    result = "voidPtr";
                    break;
                // TODO: confirm that's actually equivalent or just Il2CppObject*
                case Il2CppTypeEnum.IL2CPP_TYPE_OBJECT:
                    result = "Il2CppBoxedObject";
                    break;
                // TODO: figure how the fuck to use c# multidimensional arrays as c++
                case Il2CppTypeEnum.IL2CPP_TYPE_ARRAY:
                    {
                        Il2CppArrayType il2CppArrayType = il2cppReader.GetIl2CppArrayType(type.data.arrayPtr);
                        result = $"MArray_{GetSimpleTypeString(il2cppReader.GetIl2CppType(il2CppArrayType.etypePtr))}_";
                        break;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_SZARRAY:
                    result = $"Array_{GetSimpleTypeString(il2cppReader.GetIl2CppType(type.data.typePtr))}_";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_PTR:
                    result = $"{GetSimpleTypeString(il2cppReader.GetIl2CppType(type.data.typePtr))}_";
                    break;
                case Il2CppTypeEnum.IL2CPP_TYPE_GENERICINST:
                    {
                        // TODO: Change to generated structs when ready
                        Il2CppGenericClass generic_class = il2cppReader.GetIl2CppGenericClass(type.data.generic_classPtr);
                        string typeStr = $"{Metadata.resolvedTypes[generic_class.typeDefinitionIndex].Name}";
                        // For whatever reason generic type names end with ` and digit (eg. List`1)
                        if (typeStr.Contains('`'))
                        {
                            int idx = typeStr.IndexOf('`');
                            typeStr = typeStr.Remove(idx, typeStr.Length - idx);
                        }

                        typeStr += "_";

                        Il2CppGenericInst generic_inst = il2cppReader.GetIl2CppGenericInst(generic_class.context.class_instPtr);
                        ulong[] pointers = il2cppReader.GetGenericInstPointerArray(generic_inst.type_argv, (Int32)generic_inst.type_argc);
                        for (int i = 0; i < pointers.Length; i++)
                        {
                            typeStr += GetSimpleTypeString(il2cppReader.GetIl2CppType(pointers[i]));
                            if (i < pointers.Length - 1)
                                typeStr += "_";
                        }
                        typeStr += "_";
                        result = typeStr;
                        break;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_VAR:
                case Il2CppTypeEnum.IL2CPP_TYPE_MVAR:
                    {
                        Il2CppGenericParameter il2CppGenericParameter = Metadata.genericParameters[type.data.genericParameterIndex];
                        result = GetString(il2CppGenericParameter.nameIndex);
                        break;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_TYPEDBYREF:
                    result = "Il2CppTypedRef";
                    break;
                // TODO
                case Il2CppTypeEnum.IL2CPP_TYPE_FNPTR:
                case Il2CppTypeEnum.IL2CPP_TYPE_BYREF:
                case Il2CppTypeEnum.IL2CPP_TYPE_CMOD_REQD:
                case Il2CppTypeEnum.IL2CPP_TYPE_CMOD_OPT:
                case Il2CppTypeEnum.IL2CPP_TYPE_INTERNAL:
                case Il2CppTypeEnum.IL2CPP_TYPE_MODIFIER:
                case Il2CppTypeEnum.IL2CPP_TYPE_SENTINEL:
                case Il2CppTypeEnum.IL2CPP_TYPE_PINNED:
                case Il2CppTypeEnum.IL2CPP_TYPE_ENUM:
                    result = "x";
                    break;
                default:
                    result = "Type";
                    break;
            }

            //mapTypeStringCache.Add(type, result);
            return result;
        }
    }
}
