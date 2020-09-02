﻿using System;
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
    class MetadataReader
    {
        private BinaryReader reader;
        private MemoryStream stream;

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

            foreach (var image in Metadata.imageDefinitions)
            {
                var resolvedImage = ResolveImage(image);
                Console.WriteLine($"Image: {GetString(image.nameIndex)}x");
                Metadata.resolvedImages.Add(resolvedImage);
            }
            Console.WriteLine();
        }

        public string GetString(Int32 idx)
        {
            stream.Position = Metadata.header.stringOffset + idx;
            return reader.ReadNullTerminatedString();
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

                if(resolvedImage.Name == "Assembly-CSharp.dll")
                {
                    resolvedType.DumpToConsole();
                    foreach(var nestedType in resolvedType.nestedTypes)
                    {
                        nestedType.DumpToConsole(2);
                    }
                }
            }

            return resolvedImage;
        }

        public void ResolveTypes()
        {
            Metadata.resolvedTypes = new ResolvedType[Metadata.typeDefinitions.Length];
            for(int i=0;i< Metadata.typeDefinitions.Length;i++)
            {
                ResolveType(i);
            }
        }

        public ResolvedType ResolveType(Int32 typeIdx)
        {
            if (Metadata.resolvedTypes[typeIdx] != null)
                return Metadata.resolvedTypes[typeIdx];

            var typeDef = Metadata.typeDefinitions[typeIdx];
            // TODO: Resolved class, struct, enum
            ResolvedType resolvedType = new ResolvedType(typeDef);
            resolvedType.Name = GetString(typeDef.nameIndex);
            resolvedType.Namespace = GetString(typeDef.namespaceIndex);

            for(int i =0;i<typeDef.nested_type_count;i++)
            {
                Int32 nestedTypeIndex = Metadata.nestedTypeIndices[typeDef.nestedTypesStart + i];
                resolvedType.nestedTypes.Add(ResolveType(nestedTypeIndex));
            }

            Metadata.resolvedTypes[typeIdx] = resolvedType;
            if (resolvedType.isNested)
                Metadata.nestedTypes.Add(resolvedType);

            return resolvedType;
        }
    }
}
