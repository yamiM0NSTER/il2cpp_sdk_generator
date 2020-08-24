using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using uint8_t = System.Byte;
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
using EncodedMethodIndex = System.UInt32;
using size_t = System.UInt64;


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

    class Il2CppImageDefinition
    {
        public StringIndex nameIndex;
        public AssemblyIndex assemblyIndex;

        public TypeDefinitionIndex typeStart;
        public uint32_t typeCount;

        public TypeDefinitionIndex exportedTypeStart;
        public uint32_t exportedTypeCount;

        public MethodIndex entryPointIndex;
        public uint32_t token;

        public CustomAttributeIndex customAttributeStart;
        public uint32_t customAttributeCount;
    }

    class Il2CppTypeDefinition
    {
        public StringIndex nameIndex;
        public StringIndex namespaceIndex;
        public TypeIndex byvalTypeIndex;
        public TypeIndex byrefTypeIndex;

        public TypeIndex declaringTypeIndex;
        public TypeIndex parentIndex;
        public TypeIndex elementTypeIndex; // we can probably remove this one. Only used for enums

        public RGCTXIndex rgctxStartIndex;
        public int32_t rgctxCount;

        public GenericContainerIndex genericContainerIndex;

        public uint32_t flags;

        public FieldIndex fieldStart;
        public MethodIndex methodStart;
        public EventIndex eventStart;
        public PropertyIndex propertyStart;
        public NestedTypeIndex nestedTypesStart;
        public InterfacesIndex interfacesStart;
        public VTableIndex vtableStart;
        public InterfacesIndex interfaceOffsetsStart;

        public uint16_t method_count;
        public uint16_t property_count;
        public uint16_t field_count;
        public uint16_t event_count;
        public uint16_t nested_type_count;
        public uint16_t vtable_count;
        public uint16_t interfaces_count;
        public uint16_t interface_offsets_count;

        // bitfield to portably encode boolean values as single bits
        // 01 - valuetype;
        // 02 - enumtype;
        // 03 - has_finalize;
        // 04 - has_cctor;
        // 05 - is_blittable;
        // 06 - is_import_or_windows_runtime;
        // 07-10 - One of nine possible PackingSize values (0, 1, 2, 4, 8, 16, 32, 64, or 128)
        public uint32_t bitfield;
        public uint32_t token;
    }

    class Il2CppMethodDefinition
    {
        public StringIndex nameIndex;
        public TypeDefinitionIndex declaringType;
        public TypeIndex returnType;
        public ParameterIndex parameterStart;
        public GenericContainerIndex genericContainerIndex;
        public MethodIndex methodIndex;
        public MethodIndex invokerIndex;
        public MethodIndex reversePInvokeWrapperIndex;
        public RGCTXIndex rgctxStartIndex;
        public int32_t rgctxCount;
        public uint32_t token;
        public uint16_t flags;
        public uint16_t iflags;
        public uint16_t slot;
        public uint16_t parameterCount;
    }

    class Il2CppEventDefinition
    {
        public StringIndex nameIndex;
        public TypeIndex typeIndex;
        public MethodIndex add;
        public MethodIndex remove;
        public MethodIndex raise;
        public uint32_t token;
    }

    class Il2CppFieldRef
    {
        public TypeIndex typeIndex;
        public FieldIndex fieldIndex; // local offset into type fields
    }

    class Il2CppParameterDefinition
    {
        public StringIndex nameIndex;
        public uint32_t token;
        public TypeIndex typeIndex;
    }

    class Il2CppParameterDefaultValue
    {
        public ParameterIndex parameterIndex;
        public TypeIndex typeIndex;
        public DefaultValueDataIndex dataIndex;
    }

    class Il2CppFieldDefinition
    {
        public StringIndex nameIndex;
        public TypeIndex typeIndex;
        public uint32_t token;
    }

    class Il2CppFieldDefaultValue
    {
        public FieldIndex fieldIndex;
        public TypeIndex typeIndex;
        public DefaultValueDataIndex dataIndex;
    }

    class Il2CppPropertyDefinition
    {
        public StringIndex nameIndex;
        public MethodIndex get;
        public MethodIndex set;
        public uint32_t attrs;
        public uint32_t token;
    }

    class Il2CppMetadataUsageList
    {
        public uint32_t start;
        public uint32_t count;
    }

    class Il2CppMetadataUsagePair
    {
        public uint32_t destinationIndex;
        public uint32_t encodedSourceIndex;
    }

    class Il2CppCustomAttributeTypeRange
    {
        public uint32_t token;
        public int32_t start;
        public int32_t count;
    }

    class Il2CppStringLiteral
    {
        public uint32_t length;
        public StringLiteralIndex dataIndex;
    }

    class Il2CppGenericParameter
    {
        public GenericContainerIndex ownerIndex;  /* Type or method this parameter was defined in. */
        public StringIndex nameIndex;
        public GenericParameterConstraintIndex constraintsStart;
        public int16_t constraintsCount;
        public uint16_t num;
        public uint16_t flags;
    }

    class Il2CppGenericContainer
    {
        /* index of the generic type definition or the generic method definition corresponding to this container */
        public int32_t ownerIndex; // either index into Il2CppClass metadata array or Il2CppMethodDefinition array
        public int32_t type_argc;
        /* If true, we're a generic method, otherwise a generic type definition. */
        public int32_t is_method;
        /* Our type parameters. */
        public GenericParameterIndex genericParameterStart;
    }

    [StructLayout(LayoutKind.Explicit)]
    class Il2CppRGCTXDefinitionData
    {
        [FieldOffset(0)]
        public int32_t rgctxDataDummy;
        [FieldOffset(0)]
        public MethodIndex methodIndex;
        [FieldOffset(0)]
        public TypeIndex typeIndex;
    }

    enum Il2CppRGCTXDataType
    {
        IL2CPP_RGCTX_DATA_INVALID,
        IL2CPP_RGCTX_DATA_TYPE,
        IL2CPP_RGCTX_DATA_CLASS,
        IL2CPP_RGCTX_DATA_METHOD,
        IL2CPP_RGCTX_DATA_ARRAY,
    }

    class Il2CppRGCTXDefinition
    {
        public Il2CppRGCTXDataType type;
        public Il2CppRGCTXDefinitionData data;
    }

    class Il2CppInterfaceOffsetPair
    {
        public TypeIndex interfaceTypeIndex;
        public int32_t offset;
    }
    
    class Il2CppAssemblyNameDefinition
    {
        public StringIndex nameIndex;
        public StringIndex cultureIndex;
        public StringIndex hashValueIndex;
        public StringIndex publicKeyIndex;
        public uint32_t hash_alg;
        public int32_t hash_len;
        public uint32_t flags;
        public int32_t major;
        public int32_t minor;
        public int32_t build;
        public int32_t revision;
        [ArraySize(Metadata_Constants.PUBLIC_KEY_BYTE_LENGTH)]
        public uint8_t[] public_key_token;
    }


    class Il2CppAssemblyDefinition
    {
        public ImageIndex imageIndex;
        public uint32_t token;
        public int32_t referencedAssemblyStart;
        public int32_t referencedAssemblyCount;
        public Il2CppAssemblyNameDefinition aname;
    }


    class Il2CppMetadataRegistration64
    {
        public long genericClassesCount;
        public ulong genericClasses;
        public long genericInstsCount;
        public ulong genericInsts;
        public long genericMethodTableCount;
        public ulong genericMethodTable;
        public long typesCount;
        public ulong types;
        public long methodSpecsCount;
        public ulong methodSpecs;

        public long fieldOffsetsCount;
        public ulong fieldOffsets;

        public long typeDefinitionsSizesCount;
        public ulong typeDefinitionsSizes;
        public long metadataUsagesCount;
        public ulong metadataUsages;

        public bool Validate()
        {
            // These should never be below 0
            if (genericClassesCount < 0 ||
               genericInstsCount < 0 ||
               genericMethodTableCount < 0 ||
               typesCount < 0 ||
               methodSpecsCount < 0 ||
               fieldOffsetsCount < 0 ||
               typeDefinitionsSizesCount < 0 ||
               metadataUsagesCount < 0)
                return false;

            if (genericClassesCount == 0)
            {
                if (genericClasses != 0)
                    return false;
            }
            else if (genericClassesCount > 0)
            {
                if (genericClasses <= PortableExecutable.imageOptionalHeader64.ImageBase)
                    return false;
            }

            if (genericInstsCount == 0)
            {
                if (genericInsts != 0)
                    return false;
            }
            else if (genericInstsCount > 0)
            {
                if (genericInsts <= PortableExecutable.imageOptionalHeader64.ImageBase)
                    return false;
            }

            if (genericMethodTableCount == 0)
            {
                if (genericMethodTable != 0)
                    return false;
            }
            else if (genericMethodTableCount > 0)
            {
                if (genericMethodTable <= PortableExecutable.imageOptionalHeader64.ImageBase)
                    return false;
            }

            if (typesCount == 0)
            {
                if (types != 0)
                    return false;
            }
            else if (typesCount > 0)
            {
                if (types <= PortableExecutable.imageOptionalHeader64.ImageBase)
                    return false;
            }

            if (methodSpecsCount == 0)
            {
                if (methodSpecs != 0)
                    return false;
            }
            else if (methodSpecsCount > 0)
            {
                if (methodSpecs <= PortableExecutable.imageOptionalHeader64.ImageBase)
                    return false;
            }

            if (fieldOffsetsCount == 0)
            {
                if (fieldOffsets != 0)
                    return false;
            }
            else if (fieldOffsetsCount > 0)
            {
                if (fieldOffsets <= PortableExecutable.imageOptionalHeader64.ImageBase)
                    return false;
            }

            if (typeDefinitionsSizesCount == 0)
            {
                if (typeDefinitionsSizes != 0)
                    return false;
            }
            else if (typeDefinitionsSizesCount > 0)
            {
                if (typeDefinitionsSizes <= PortableExecutable.imageOptionalHeader64.ImageBase)
                    return false;
            }

            if (metadataUsagesCount == 0)
            {
                if (metadataUsages != 0)
                    return false;
            }
            else if (metadataUsagesCount > 0)
            {
                if (metadataUsages <= PortableExecutable.imageOptionalHeader64.ImageBase)
                    return false;
            }

            return true;
        }
    }

    class Il2CppCodeRegistration64
    {
        public long methodPointersCount;
        public ulong methodPointers;
        public long reversePInvokeWrapperCount;
        public ulong reversePInvokeWrappers;
        public long genericMethodPointersCount;
        public ulong genericMethodPointers;
        public long invokerPointersCount;
        public ulong invokerPointers;
        public long customAttributeCount;
        public ulong customAttributeGenerators;
        public long unresolvedVirtualCallCount;
        public ulong unresolvedVirtualCallPointers;
        public long interopDataCount;
        public ulong interopData;
        public long windowsRuntimeFactoryCount;
        public ulong windowsRuntimeFactoryTable;


        public bool Validate()
        {
            // These should never be below 0
            if (methodPointersCount < 0 ||
               reversePInvokeWrapperCount < 0 ||
               genericMethodPointersCount < 0 ||
               invokerPointersCount < 0 ||
               customAttributeCount < 0 ||
               unresolvedVirtualCallCount < 0 ||
               interopDataCount < 0 ||
               windowsRuntimeFactoryCount < 0)
                return false;

            if (methodPointersCount == 0)
            {
                if (methodPointers != 0)
                    return false;
            }
            else if(methodPointersCount > 0)
            {
                if (methodPointers <= PortableExecutable.imageOptionalHeader64.ImageBase)
                    return false;
            }

            if (reversePInvokeWrapperCount == 0)
            {
                if (reversePInvokeWrappers != 0)
                    return false;
            }
            else if (reversePInvokeWrapperCount > 0)
            {
                if (reversePInvokeWrappers <= PortableExecutable.imageOptionalHeader64.ImageBase)
                    return false;
            }

            if (reversePInvokeWrapperCount == 0)
            {
                if (reversePInvokeWrappers != 0)
                    return false;
            }
            else if (reversePInvokeWrapperCount > 0)
            {
                if (reversePInvokeWrappers <= PortableExecutable.imageOptionalHeader64.ImageBase)
                    return false;
            }

            if (genericMethodPointersCount == 0)
            {
                if (genericMethodPointers != 0)
                    return false;
            }
            else if (genericMethodPointersCount > 0)
            {
                if (genericMethodPointers <= PortableExecutable.imageOptionalHeader64.ImageBase)
                    return false;
            }

            if (invokerPointersCount == 0)
            {
                if (invokerPointers != 0)
                    return false;
            }
            else if (invokerPointersCount > 0)
            {
                if (invokerPointers <= PortableExecutable.imageOptionalHeader64.ImageBase)
                    return false;
            }

            if (customAttributeCount == 0)
            {
                if (customAttributeGenerators != 0)
                    return false;
            }
            else if (customAttributeCount > 0)
            {
                if (customAttributeGenerators <= PortableExecutable.imageOptionalHeader64.ImageBase)
                    return false;
            }

            if (unresolvedVirtualCallCount == 0)
            {
                if (unresolvedVirtualCallPointers != 0)
                    return false;
            }
            else if (unresolvedVirtualCallCount > 0)
            {
                if (unresolvedVirtualCallPointers <= PortableExecutable.imageOptionalHeader64.ImageBase)
                    return false;
            }

            if (interopDataCount == 0)
            {
                if (interopData != 0)
                    return false;
            }
            else if (interopDataCount > 0)
            {
                if (interopData <= PortableExecutable.imageOptionalHeader64.ImageBase)
                    return false;
            }

            if (windowsRuntimeFactoryCount == 0)
            {
                if (windowsRuntimeFactoryTable != 0)
                    return false;
            }
            else if (windowsRuntimeFactoryCount > 0)
            {
                if (windowsRuntimeFactoryTable <= PortableExecutable.imageOptionalHeader64.ImageBase)
                    return false;
            }


            return true;
        }
    }

    static class Metadata_Constants
    {
        public const int32_t PUBLIC_KEY_BYTE_LENGTH = 8;
    }
}
