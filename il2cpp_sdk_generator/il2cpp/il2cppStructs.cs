using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using MethodIndex = System.Int32;
using GenericMethodIndex = System.Int32;
using TypeDefinitionIndex = System.Int32;
using GenericParameterIndex = System.Int32;
using GenericInstIndex = System.Int32;

using uint32_t = System.UInt32;
using int32_t = System.Int32;

namespace il2cpp_sdk_generator
{
    // All structs are spaced by 8 bytes cuz of x64

    class Il2CppMetadataRegistration64
    {
        public long genericClassesCount;
        public ulong genericClasses; // // Il2CppGenericClass* const *
        public long genericInstsCount;
        public ulong genericInsts; // const Il2CppGenericInst* const *
        public long genericMethodTableCount;
        public ulong genericMethodTable; // const Il2CppGenericMethodFunctionsDefinitions*
        public long typesCount;
        public ulong types; // const Il2CppType* const *
        public long methodSpecsCount;
        public ulong methodSpecs; // const Il2CppMethodSpec*

        public long fieldOffsetsCount;
        public ulong fieldOffsets; // const int32_t**

        public long typeDefinitionsSizesCount;
        public ulong typeDefinitionsSizes; // const Il2CppTypeDefinitionSizes**
        public long metadataUsagesCount;
        public ulong metadataUsages; // void** const*

        // use typeDefinitions from metadata to validate?
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
        public ulong methodPointers; // Il2CppMethodPointer
        public long reversePInvokeWrapperCount;
        public ulong reversePInvokeWrappers; // Il2CppMethodPointer
        public long genericMethodPointersCount;
        public ulong genericMethodPointers; // Il2CppMethodPointer
        public long invokerPointersCount;
        public ulong invokerPointers; // InvokerMethod
        public long customAttributeCount;
        public ulong customAttributeGenerators; // CustomAttributesCacheGenerator
        public long unresolvedVirtualCallCount;
        public ulong unresolvedVirtualCallPointers; // Il2CppMethodPointer
        public long interopDataCount;
        public ulong interopData; // Il2CppInteropData
        public long windowsRuntimeFactoryCount;
        public ulong windowsRuntimeFactoryTable; // Il2CppWindowsRuntimeFactoryTableEntry

        // use indexedMethods from metadata to validate?
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
            else if (methodPointersCount > 0)
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

    class Il2CppInteropData
    {
        public ulong delegatePInvokeWrapperFunction; // Il2CppMethodPointer
        public ulong pinvokeMarshalToNativeFunction; // PInvokeMarshalToNativeFunc
        public ulong pinvokeMarshalFromNativeFunction; // PInvokeMarshalFromNativeFunc
        public ulong pinvokeMarshalCleanupFunction; // PInvokeMarshalCleanupFunc
        public ulong createCCWFunction; // CreateCCWFunc
        public ulong guid; // const Il2CppGuid*
        public ulong type; // const Il2CppType*
    }

    class Il2CppGenericClass
    {
        /// <summary>
        /// the generic type definition
        /// </summary>
        public ulong typeDefinitionIndex; // TypeDefinitionIndex
        /// <summary>
        /// a context that contains the type instantiation doesn't contain any method instantiation
        /// </summary>
        public Il2CppGenericContext context; // Il2CppGenericContext
        /// <summary>
        /// if present, the Il2CppClass corresponding to the instantiation.
        /// </summary>
        public ulong cached_class; // Il2CppClass*
    }

    class Il2CppGenericInst
    {
        public ulong type_argc; // uint32_t
        public ulong type_argv; // Il2CppType**
    }

    class Il2CppGenericContext
    {
        /// <summary>
        /// The instantiation corresponding to the class generic parameters
        /// </summary>
        public ulong class_inst; // Il2CppGenericInst*
        /// <summary>
        /// The instantiation corresponding to the method generic parameters
        /// </summary>
        public ulong method_inst; // Il2CppGenericInst*
    }

    class Il2CppGenericMethodFunctionsDefinitions
    {
        public GenericMethodIndex genericMethodIndex;
        public Il2CppGenericMethodIndices indices;
    }

    class Il2CppGenericMethodIndices
    {
        public MethodIndex methodIndex;
        public MethodIndex invokerIndex;
    }

    [StructLayout(LayoutKind.Explicit)]
    class data
    {
        //    union
        //{
        //    // We have this dummy field first because pre C99 compilers (MSVC) can only initializer the first value in a union.
        //    void* dummy;
        //    TypeDefinitionIndex klassIndex; /* for VALUETYPE and CLASS */
        //    const Il2CppType* type;   /* for PTR and SZARRAY */
        //    Il2CppArrayType* array; /* for ARRAY */
        //    //MonoMethodSignature *method;
        //    GenericParameterIndex genericParameterIndex; /* for VAR and MVAR */
        //    Il2CppGenericClass* generic_class; /* for GENERICINST */
        //}
        //data;
        [FieldOffset(0)]
        public ulong dummy;
        /// <summary>
        /// for VALUETYPE and CLASS
        /// </summary>
        [FieldOffset(0)]
        public TypeDefinitionIndex klassIndex;
        /// <summary>
        /// for PTR and SZARRAY
        /// </summary>
        [FieldOffset(0)]
        public ulong type; // const Il2CppType*
        /// <summary>
        /// for ARRAY
        /// </summary>
        [FieldOffset(0)]
        public ulong array; // Il2CppArrayType*
        /// <summary>
        /// for VAR and MVAR
        /// </summary>
        [FieldOffset(0)]
        public GenericParameterIndex genericParameterIndex;
        /// <summary>
        /// for GENERICINST
        /// </summary>
        [FieldOffset(0)]
        public ulong generic_class; // Il2CppGenericClass*
    }

    
    class Il2CppType
    {
        public data data;

        //unsigned int attrs    : 16; /* param attributes or field flags */
        //Il2CppTypeEnum type     : 8;
        //unsigned int num_mods : 6;  /* max 64 modifiers follow at the end */
        //unsigned int byref    : 1;
        //unsigned int pinned   : 1;  /* valid when included in a local var signature */
        //MonoCustomMod modifiers [MONO_ZERO_LEN_ARRAY]; /* this may grow */
        /// <summary>
        /// Fields below data union are sized by bits to save space which is kinda impossible in c#
        /// we can move by bytes though
        /// 
        /// attrs: param attributes or field flags
        /// </summary>
        public UInt16 attrs;
        /// <summary>
        /// Il2CppTypeEnum type     : 8;
        /// unsigned int num_mods : 6;  /* max 64 modifiers follow at the end */
        /// unsigned int byref    : 1;
        /// unsigned int pinned   : 1;  /* valid when included in a local var signature */
        /// </summary>
        public UInt16 bit_var;

        // TODO: Add dummy spacing int32 variable?

        public Il2CppTypeEnum type
        {
            get
            {
                return (Il2CppTypeEnum)(bit_var & 0b1111_1111);
            }
        }

        public UInt32 num_mods
        {
            get
            {
                return (UInt32)(bit_var >> 8 & 0b0011_1111); // 6 bits after 8 used by type
            }
        }

        public UInt32 byref
        {
            get
            {
                return (UInt32)(bit_var >> 14 & 0b1); // 1 bit after 8 + 6 used by type and num_mods
            }
        }

        public UInt32 pinned
        {
            get
            {
                return (UInt32)(bit_var >> 15 & 0b1); // 1 bit after 8 + 6 + 1 used by type and num_mods and byref
            }
        }
    }

    enum Il2CppTypeEnum
    {
        IL2CPP_TYPE_END = 0x00,       /* End of List */
        IL2CPP_TYPE_VOID = 0x01,
        IL2CPP_TYPE_BOOLEAN = 0x02,
        IL2CPP_TYPE_CHAR = 0x03,
        IL2CPP_TYPE_I1 = 0x04,
        IL2CPP_TYPE_U1 = 0x05,
        IL2CPP_TYPE_I2 = 0x06,
        IL2CPP_TYPE_U2 = 0x07,
        IL2CPP_TYPE_I4 = 0x08,
        IL2CPP_TYPE_U4 = 0x09,
        IL2CPP_TYPE_I8 = 0x0a,
        IL2CPP_TYPE_U8 = 0x0b,
        IL2CPP_TYPE_R4 = 0x0c,
        IL2CPP_TYPE_R8 = 0x0d,
        IL2CPP_TYPE_STRING = 0x0e,
        IL2CPP_TYPE_PTR = 0x0f,       /* arg: <type> token */
        IL2CPP_TYPE_BYREF = 0x10,       /* arg: <type> token */
        IL2CPP_TYPE_VALUETYPE = 0x11,       /* arg: <type> token */
        IL2CPP_TYPE_CLASS = 0x12,       /* arg: <type> token */
        IL2CPP_TYPE_VAR = 0x13,       /* Generic parameter in a generic type definition, represented as number (compressed unsigned integer) number */
        IL2CPP_TYPE_ARRAY = 0x14,       /* type, rank, boundsCount, bound1, loCount, lo1 */
        IL2CPP_TYPE_GENERICINST = 0x15,     /* <type> <type-arg-count> <type-1> \x{2026} <type-n> */
        IL2CPP_TYPE_TYPEDBYREF = 0x16,
        IL2CPP_TYPE_I = 0x18,
        IL2CPP_TYPE_U = 0x19,
        IL2CPP_TYPE_FNPTR = 0x1b,        /* arg: full method signature */
        IL2CPP_TYPE_OBJECT = 0x1c,
        IL2CPP_TYPE_SZARRAY = 0x1d,       /* 0-based one-dim-array */
        IL2CPP_TYPE_MVAR = 0x1e,       /* Generic parameter in a generic method definition, represented as number (compressed unsigned integer)  */
        IL2CPP_TYPE_CMOD_REQD = 0x1f,       /* arg: typedef or typeref token */
        IL2CPP_TYPE_CMOD_OPT = 0x20,       /* optional arg: typedef or typref token */
        IL2CPP_TYPE_INTERNAL = 0x21,       /* CLR internal type */

        IL2CPP_TYPE_MODIFIER = 0x40,       /* Or with the following types */
        IL2CPP_TYPE_SENTINEL = 0x41,       /* Sentinel for varargs method signature */
        IL2CPP_TYPE_PINNED = 0x45,       /* Local var that points to pinned object */

        IL2CPP_TYPE_ENUM = 0x55        /* an enumeration */
    }

    // For whatever reason this doesn't have 8 byte spacing
    class Il2CppMethodSpec
    {
        public MethodIndex methodDefinitionIndex;
        public GenericInstIndex classIndexIndex;
        public GenericInstIndex methodIndexIndex;
    }

    // For whatever reason this doesn't have 8 byte spacing
    class Il2CppTypeDefinitionSizes
    {
        public uint32_t instance_size;
        public int32_t native_size;
        public uint32_t static_fields_size;
        public uint32_t thread_static_fields_size;
    }
}
