using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace il2cpp_sdk_generator
{
    static class Metadata
    {
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
        public static System.Int32[] genericParameterConstraintsIndices; // TypeIndex
        public static Il2CppGenericContainer[] genericContainers;
        public static System.Int32[] nestedTypeIndices; // TypeDefinitionIndex
        public static System.Int32[] interfaceIndices; // TypeIndex
        public static System.UInt32[] vtableMethodIndices; // EncodedMethodIndex
        public static Il2CppInterfaceOffsetPair[] interfaceOffsetPairs;
        public static Il2CppTypeDefinition[] typeDefinitions;
        public static Il2CppRGCTXDefinition[] rgctxEntries;
        public static Il2CppImageDefinition[] imageDefinitions;
        public static Il2CppAssemblyDefinition[] assemblies;
        public static Il2CppMetadataUsageList[] metadataUsageLists;
        public static Il2CppMetadataUsagePair[] metadataUsagePairs;
        public static Il2CppFieldRef[] fieldReferences;
        public static System.Int32[] referencedAssemblies; // int32_t
        public static Il2CppCustomAttributeTypeRange[] attributeTypeRanges;
        public static System.Int32[] attributeTypes; // TypeIndex
        // unresolvedVirtualCallParameterTypes
        // unresolvedVirtualCallParameterRanges
        // windowsRuntimeTypeNames // Il2CppWindowsRuntimeTypeNamePair
        // exportedTypeDefinitions // TypeDefinitionIndex
    }
}
