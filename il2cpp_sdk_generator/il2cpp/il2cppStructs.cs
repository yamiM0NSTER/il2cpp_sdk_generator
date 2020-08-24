using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace il2cpp_sdk_generator
{
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
}
