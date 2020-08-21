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
    struct Il2CppGlobalMetadataHeader
    {
        int32_t sanity;
        int32_t version;
        int32_t stringLiteralOffset; // string data for managed code
        int32_t stringLiteralCount;
        int32_t stringLiteralDataOffset;
        int32_t stringLiteralDataCount;
        int32_t stringOffset; // string data for metadata
        int32_t stringCount;
        int32_t eventsOffset; // Il2CppEventDefinition
        int32_t eventsCount;
        int32_t propertiesOffset; // Il2CppPropertyDefinition
        int32_t propertiesCount;
        int32_t methodsOffset; // Il2CppMethodDefinition
        int32_t methodsCount;
        int32_t parameterDefaultValuesOffset; // Il2CppParameterDefaultValue
        int32_t parameterDefaultValuesCount;
        int32_t fieldDefaultValuesOffset; // Il2CppFieldDefaultValue
        int32_t fieldDefaultValuesCount;
        int32_t fieldAndParameterDefaultValueDataOffset; // uint8_t
        int32_t fieldAndParameterDefaultValueDataCount;
        int32_t fieldMarshaledSizesOffset; // Il2CppFieldMarshaledSize
        int32_t fieldMarshaledSizesCount;
        int32_t parametersOffset; // Il2CppParameterDefinition
        int32_t parametersCount;
        int32_t fieldsOffset; // Il2CppFieldDefinition
        int32_t fieldsCount;
        int32_t genericParametersOffset; // Il2CppGenericParameter
        int32_t genericParametersCount;
        int32_t genericParameterConstraintsOffset; // TypeIndex
        int32_t genericParameterConstraintsCount;
        int32_t genericContainersOffset; // Il2CppGenericContainer
        int32_t genericContainersCount;
        int32_t nestedTypesOffset; // TypeDefinitionIndex
        int32_t nestedTypesCount;
        int32_t interfacesOffset; // TypeIndex
        int32_t interfacesCount;
        int32_t vtableMethodsOffset; // EncodedMethodIndex
        int32_t vtableMethodsCount;
        int32_t interfaceOffsetsOffset; // Il2CppInterfaceOffsetPair
        int32_t interfaceOffsetsCount;
        int32_t typeDefinitionsOffset; // Il2CppTypeDefinition
        int32_t typeDefinitionsCount;
        int32_t rgctxEntriesOffset; // Il2CppRGCTXDefinition
        int32_t rgctxEntriesCount;
        int32_t imagesOffset; // Il2CppImageDefinition
        int32_t imagesCount;
        int32_t assembliesOffset; // Il2CppAssemblyDefinition
        int32_t assembliesCount;
        int32_t metadataUsageListsOffset; // Il2CppMetadataUsageList
        int32_t metadataUsageListsCount;
        int32_t metadataUsagePairsOffset; // Il2CppMetadataUsagePair
        int32_t metadataUsagePairsCount;
        int32_t fieldRefsOffset; // Il2CppFieldRef
        int32_t fieldRefsCount;
        int32_t referencedAssembliesOffset; // int32_t
        int32_t referencedAssembliesCount;
        int32_t attributesInfoOffset; // Il2CppCustomAttributeTypeRange
        int32_t attributesInfoCount;
        int32_t attributeTypesOffset; // TypeIndex
        int32_t attributeTypesCount;
        int32_t unresolvedVirtualCallParameterTypesOffset; // TypeIndex
        int32_t unresolvedVirtualCallParameterTypesCount;
        int32_t unresolvedVirtualCallParameterRangesOffset; // Il2CppRange
        int32_t unresolvedVirtualCallParameterRangesCount;
        int32_t windowsRuntimeTypeNamesOffset; // Il2CppWindowsRuntimeTypeNamePair
        int32_t windowsRuntimeTypeNamesSize;
        int32_t exportedTypeDefinitionsOffset; // TypeDefinitionIndex
        int32_t exportedTypeDefinitionsCount;
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
        int32_t rgctxCount;

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
        int32_t rgctxCount;
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
        int32_t start;
        int32_t count;
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
        int32_t ownerIndex; // either index into Il2CppClass metadata array or Il2CppMethodDefinition array
        int32_t type_argc;
        /* If true, we're a generic method, otherwise a generic type definition. */
        int32_t is_method;
        /* Our type parameters. */
        GenericParameterIndex genericParameterStart;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct Il2CppRGCTXDefinitionData
    {
        [FieldOffset(0)]
        int32_t rgctxDataDummy;
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
