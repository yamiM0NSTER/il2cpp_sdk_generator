using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using int32_t = System.Int32;

using TypeIndex = System.Int32;
using TypeDefinitionIndex = System.Int32;
using EncodedMethodIndex = System.UInt32;


namespace il2cpp_sdk_generator
{
    static class Metadata
    {
        // Direct File data
        public static Il2CppGlobalMetadataHeader header;
        public static Il2CppStringLiteral[] stringLiterals;
        // string data for metadata
        // TODO: maybe add string cache?
        public static Il2CppEventDefinition[] eventDefinitions;
        public static Il2CppPropertyDefinition[] propertyDefinitions;
        public static Il2CppMethodDefinition[] methodDefinitions;
        public static Il2CppParameterDefaultValue[] parameterDefaultValues;
        public static Il2CppFieldDefaultValue[] fieldDefaultValues;
        // fieldAndParameterDefaultValueData
        // Il2CppFieldMarshaledSize
        public static Il2CppParameterDefinition[] parameterDefinitions;
        public static Il2CppFieldDefinition[] fieldDefinitions;
        public static Il2CppGenericParameter[] genericParameters;
        public static TypeIndex[] genericParameterConstraintsIndices; // TypeIndex
        public static Il2CppGenericContainer[] genericContainers;
        public static TypeDefinitionIndex[] nestedTypeIndices; // TypeDefinitionIndex
        public static TypeIndex[] interfaceIndices; // TypeIndex
        public static EncodedMethodIndex[] vtableMethodIndices; // EncodedMethodIndex
        public static Il2CppInterfaceOffsetPair[] interfaceOffsetPairs;
        public static Il2CppTypeDefinition[] typeDefinitions;
        public static Il2CppRGCTXDefinition[] rgctxEntries;
        public static Il2CppImageDefinition[] imageDefinitions;
        public static Il2CppAssemblyDefinition[] assemblies;
        public static Il2CppMetadataUsageList[] metadataUsageLists;
        public static Il2CppMetadataUsagePair[] metadataUsagePairs;
        public static Il2CppFieldRef[] fieldReferences;
        public static int32_t[] referencedAssemblies; // int32_t
        public static Il2CppCustomAttributeTypeRange[] attributeTypeRanges;
        public static TypeIndex[] attributeTypes; // TypeIndex
        public static TypeIndex[] unresolvedVirtualCallParameterTypes; // TypeIndex
        // unresolvedVirtualCallParameterRanges
        // windowsRuntimeTypeNames // Il2CppWindowsRuntimeTypeNamePair
        // exportedTypeDefinitions // TypeDefinitionIndex

        // Processed data
    }
}
