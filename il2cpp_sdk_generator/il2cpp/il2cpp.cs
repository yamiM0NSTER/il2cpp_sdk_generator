using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace il2cpp_sdk_generator
{
    class il2cpp
    {
        // Code Registration
        public static Il2CppCodeRegistration64 codeRegistration64 = null;
        public static ulong[] methodPointers; // VA's
        public static ulong[] reversePInvokeWrappers; // VA's
        public static ulong[] genericMethodPointers; // VA's
        public static ulong[] invokerPointers; // VA's
        public static ulong[] customAttributeGenerators; // VA's
        public static ulong[] unresolvedVirtualCallPointers; // VA's
        public static Il2CppInteropData[] interopData; // Il2CppInteropData
        public static ulong[] windowsRuntimeFactoryTable; // VA's

        // Metadata Registration
        public static Il2CppMetadataRegistration64 metadataRegistration64 = null;
        public static ulong[] genericClassesPtrs;
        public static Il2CppGenericClass[] genericClasses;
        public static ulong[] genericInstsPtrs;
        public static Il2CppGenericInst[] genericInsts;
        public static Il2CppGenericMethodFunctionsDefinitions[] genericMethodTable;
        public static ulong[] typesPtrs;
        public static Il2CppType[] types;
        public static Il2CppMethodSpec[] methodSpecs;
        public static ulong[] fieldOffsetsPtrs;
        public static Int32[] fieldOffsets;
        public static ulong[] typeDefinitionsSizesPtrs;
        public static Il2CppTypeDefinitionSizes[] typeDefinitionsSizes;
        public static ulong[] metadataUsages;


    }
}
