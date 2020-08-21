using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using int32_t = System.Int32;
using int16_t = System.Int16;
using uint32_t = System.UInt32;
using uint16_t = System.UInt16;

using TypeIndex = System.Int32;
using TypeDefinitionIndex = System.Int32;
using FieldIndex = System.Int32;
using DefaultValueIndex = System.Int32;
using DefaultValueDataIndex = System.Int32;
using CustomAttributeIndex = System.Int32;
using ParameterIndex = System.Int32;
using MethodIndex = System.Int32;
using GenericMethodIndex = System.Int32;
using PropertyIndex = System.Int32;
using EventIndex = System.Int32;
using GenericContainerIndex = System.Int32;
using GenericParameterIndex = System.Int32;
using GenericParameterConstraintIndex = System.Int16;
using NestedTypeIndex = System.Int32;
using InterfacesIndex = System.Int32;
using VTableIndex = System.Int32;
using InterfaceOffsetIndex = System.Int32;
using RGCTXIndex = System.Int32;
using StringIndex = System.Int32;
using StringLiteralIndex = System.Int32;
using GenericInstIndex = System.Int32;
using ImageIndex = System.Int32;
using AssemblyIndex = System.Int32;
using InteropDataIndex = System.Int32;


namespace il2cpp_sdk_generator
{
    class Il2CppGlobalMetadataHeader
    {
       public int32_t sanity;
       public int32_t version;
       public int32_t stringLiteralOffset; // string data for managed code
       public int32_t stringLiteralCount;
       public int32_t stringLiteralDataOffset;
       public int32_t stringLiteralDataCount;
       public int32_t stringOffset; // string data for metadata
       public int32_t stringCount;
       public int32_t eventsOffset; // Il2CppEventDefinition
       public int32_t eventsCount;
       public int32_t propertiesOffset; // Il2CppPropertyDefinition
       public int32_t propertiesCount;
       public int32_t methodsOffset; // Il2CppMethodDefinition
       public int32_t methodsCount;
       public int32_t parameterDefaultValuesOffset; // Il2CppParameterDefaultValue
       public int32_t parameterDefaultValuesCount;
       public int32_t fieldDefaultValuesOffset; // Il2CppFieldDefaultValue
       public int32_t fieldDefaultValuesCount;
       public int32_t fieldAndParameterDefaultValueDataOffset; // uint8_t
       public int32_t fieldAndParameterDefaultValueDataCount;
       public int32_t fieldMarshaledSizesOffset; // Il2CppFieldMarshaledSize
       public int32_t fieldMarshaledSizesCount;
       public int32_t parametersOffset; // Il2CppParameterDefinition
       public int32_t parametersCount;
       public int32_t fieldsOffset; // Il2CppFieldDefinition
       public int32_t fieldsCount;
       public int32_t genericParametersOffset; // Il2CppGenericParameter
       public int32_t genericParametersCount;
       public int32_t genericParameterConstraintsOffset; // TypeIndex
       public int32_t genericParameterConstraintsCount;
       public int32_t genericContainersOffset; // Il2CppGenericContainer
       public int32_t genericContainersCount;
       public int32_t nestedTypesOffset; // TypeDefinitionIndex
       public int32_t nestedTypesCount;
       public int32_t interfacesOffset; // TypeIndex
       public int32_t interfacesCount;
       public int32_t vtableMethodsOffset; // EncodedMethodIndex
       public int32_t vtableMethodsCount;
       public int32_t interfaceOffsetsOffset; // Il2CppInterfaceOffsetPair
       public int32_t interfaceOffsetsCount;
       public int32_t typeDefinitionsOffset; // Il2CppTypeDefinition
       public int32_t typeDefinitionsCount;
       public int32_t rgctxEntriesOffset; // Il2CppRGCTXDefinition
       public int32_t rgctxEntriesCount;
       public int32_t imagesOffset; // Il2CppImageDefinition
       public int32_t imagesCount;
       public int32_t assembliesOffset; // Il2CppAssemblyDefinition
       public int32_t assembliesCount;
       public int32_t metadataUsageListsOffset; // Il2CppMetadataUsageList
       public int32_t metadataUsageListsCount;
       public int32_t metadataUsagePairsOffset; // Il2CppMetadataUsagePair
       public int32_t metadataUsagePairsCount;
       public int32_t fieldRefsOffset; // Il2CppFieldRef
       public int32_t fieldRefsCount;
       public int32_t referencedAssembliesOffset; // int32_t
       public int32_t referencedAssembliesCount;
       public int32_t attributesInfoOffset; // Il2CppCustomAttributeTypeRange
       public int32_t attributesInfoCount;
       public int32_t attributeTypesOffset; // TypeIndex
       public int32_t attributeTypesCount;
       public int32_t unresolvedVirtualCallParameterTypesOffset; // TypeIndex
       public int32_t unresolvedVirtualCallParameterTypesCount;
       public int32_t unresolvedVirtualCallParameterRangesOffset; // Il2CppRange
       public int32_t unresolvedVirtualCallParameterRangesCount;
       public int32_t windowsRuntimeTypeNamesOffset; // Il2CppWindowsRuntimeTypeNamePair
       public int32_t windowsRuntimeTypeNamesSize;
       public int32_t exportedTypeDefinitionsOffset; // TypeDefinitionIndex
       public int32_t exportedTypeDefinitionsCount;
    }

    struct Il2CppImageDefinition
    {
        StringIndex nameIndex;
        AssemblyIndex assemblyIndex;

        TypeDefinitionIndex typeStart;
        uint32_t typeCount;

        TypeDefinitionIndex exportedTypeStart;
        uint32_t exportedTypeCount;

        MethodIndex entryPointIndex;
        uint32_t token;

        CustomAttributeIndex customAttributeStart;
        uint32_t customAttributeCount;
    }

    struct Il2CppTypeDefinition
    {
        StringIndex nameIndex;
        StringIndex namespaceIndex;
        TypeIndex byvalTypeIndex;
        TypeIndex byrefTypeIndex;

        TypeIndex declaringTypeIndex;
        TypeIndex parentIndex;
        TypeIndex elementTypeIndex; // we can probably remove this one. Only used for enums

        RGCTXIndex rgctxStartIndex;
       public int32_t rgctxCount;

        GenericContainerIndex genericContainerIndex;

        uint32_t flags;

        FieldIndex fieldStart;
        MethodIndex methodStart;
        EventIndex eventStart;
        PropertyIndex propertyStart;
        NestedTypeIndex nestedTypesStart;
        InterfacesIndex interfacesStart;
        VTableIndex vtableStart;
        InterfacesIndex interfaceOffsetsStart;

        uint16_t method_count;
        uint16_t property_count;
        uint16_t field_count;
        uint16_t event_count;
        uint16_t nested_type_count;
        uint16_t vtable_count;
        uint16_t interfaces_count;
        uint16_t interface_offsets_count;

        // bitfield to portably encode boolean values as single bits
        // 01 - valuetype;
        // 02 - enumtype;
        // 03 - has_finalize;
        // 04 - has_cctor;
        // 05 - is_blittable;
        // 06 - is_import_or_windows_runtime;
        // 07-10 - One of nine possible PackingSize values (0, 1, 2, 4, 8, 16, 32, 64, or 128)
        uint32_t bitfield;
        uint32_t token;
    }

    struct Il2CppMethodDefinition
    {
        StringIndex nameIndex;
        TypeDefinitionIndex declaringType;
        TypeIndex returnType;
        ParameterIndex parameterStart;
        GenericContainerIndex genericContainerIndex;
        MethodIndex methodIndex;
        MethodIndex invokerIndex;
        MethodIndex reversePInvokeWrapperIndex;
        RGCTXIndex rgctxStartIndex;
       public int32_t rgctxCount;
        uint32_t token;
        uint16_t flags;
        uint16_t iflags;
        uint16_t slot;
        uint16_t parameterCount;
    }

    struct Il2CppEventDefinition
    {
        StringIndex nameIndex;
        TypeIndex typeIndex;
        MethodIndex add;
        MethodIndex remove;
        MethodIndex raise;
        uint32_t token;
    }

    struct Il2CppFieldRef
    {
        TypeIndex typeIndex;
        FieldIndex fieldIndex; // local offset into type fields
    }

    struct Il2CppParameterDefinition
    {
        StringIndex nameIndex;
        uint32_t token;
        TypeIndex typeIndex;
    }

    struct Il2CppParameterDefaultValue
    {
        ParameterIndex parameterIndex;
        TypeIndex typeIndex;
        DefaultValueDataIndex dataIndex;
    }

    struct Il2CppFieldDefinition
    {
        StringIndex nameIndex;
        TypeIndex typeIndex;
        uint32_t token;
    }

    struct Il2CppFieldDefaultValue
    {
        FieldIndex fieldIndex;
        TypeIndex typeIndex;
        DefaultValueDataIndex dataIndex;
    }

    struct Il2CppPropertyDefinition
    {
        StringIndex nameIndex;
        MethodIndex get;
        MethodIndex set;
        uint32_t attrs;
        uint32_t token;
    }

    struct Il2CppMetadataUsageList
    {
        uint32_t start;
        uint32_t count;
    }

    struct Il2CppMetadataUsagePair
    {
        uint32_t destinationIndex;
        uint32_t encodedSourceIndex;
    }

    struct Il2CppCustomAttributeTypeRange
    {
        uint32_t token;
       public int32_t start;
       public int32_t count;
    }

    struct Il2CppStringLiteral
    {
        uint32_t length;
        StringLiteralIndex dataIndex;
    }

    struct Il2CppGenericParameter
    {
        GenericContainerIndex ownerIndex;  /* Type or method this parameter was defined in. */
        StringIndex nameIndex;
        GenericParameterConstraintIndex constraintsStart;
        int16_t constraintsCount;
        uint16_t num;
        uint16_t flags;
    }

    struct Il2CppGenericContainer
    {
        /* index of the generic type definition or the generic method definition corresponding to this container */
       public int32_t ownerIndex; // either index into Il2CppClass metadata array or Il2CppMethodDefinition array
       public int32_t type_argc;
        /* If true, we're a generic method, otherwise a generic type definition. */
       public int32_t is_method;
        /* Our type parameters. */
        GenericParameterIndex genericParameterStart;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct Il2CppRGCTXDefinitionData
    {
        [FieldOffset(0)]
       public int32_t rgctxDataDummy;
        [FieldOffset(0)]
        MethodIndex methodIndex;
        [FieldOffset(0)]
        TypeIndex typeIndex;
    }

    enum Il2CppRGCTXDataType
    {
        IL2CPP_RGCTX_DATA_INVALID,
        IL2CPP_RGCTX_DATA_TYPE,
        IL2CPP_RGCTX_DATA_CLASS,
        IL2CPP_RGCTX_DATA_METHOD,
        IL2CPP_RGCTX_DATA_ARRAY,
    }

    struct Il2CppRGCTXDefinition
    {
        Il2CppRGCTXDataType type;
        Il2CppRGCTXDefinitionData data;
    }
}
