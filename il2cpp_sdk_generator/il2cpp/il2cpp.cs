using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace il2cpp_sdk_generator
{
    class il2cpp
    {
        public static Il2CppCodeRegistration64 codeRegistration64 = null;
        public static ulong[] methodPointers; // VA's
        public static ulong[] reversePInvokeWrappers; // VA's
        public static ulong[] genericMethodPointers; // VA's
        public static ulong[] invokerPointers; // VA's
        public static ulong[] customAttributeGenerators; // VA's
        public static ulong[] unresolvedVirtualCallPointers; // VA's
        public static ulong[] interopData; // VA's
        public static ulong[] windowsRuntimeFactoryTable; // VA's


        public static Il2CppMetadataRegistration64 metadataRegistration64 = null;
        


    }
}
