using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace il2cpp_sdk_generator
{
    class il2cppReader
    {
        private static BinaryReader reader;
        private static MemoryStream stream;

        public static void Init(MemoryStream memoryStream)
        {
            stream = memoryStream;
            stream.Position = 0;
            reader = new BinaryReader(memoryStream, Encoding.UTF8);
        }


        public static void Read()
        {
            if(il2cpp.codeRegistration64 != null)
                ReadCodeRegistration();
        }

        public static void Process()
        {

        }

        private static void ReadCodeRegistration()
        {
            Console.WriteLine($"il2cpp.codeRegistration64.methodPointers: 0x{il2cpp.codeRegistration64.methodPointers:X8}");
            stream.Position = (long)PortableExecutableReader.OffsetFromVA(il2cpp.codeRegistration64.methodPointers);
            il2cpp.methodPointers = reader.ReadArray<ulong>((int)il2cpp.codeRegistration64.methodPointersCount);
            //for(int i=0;i< il2cpp.methodPointers.Length;i++)
            //{
            //    Console.WriteLine($"0x{il2cpp.methodPointers[i]:X8}");
            //}
            //stream.Position = il2cpp.codeRegistration64.methodPointers;

            Console.WriteLine($"il2cpp.codeRegistration64.reversePInvokeWrappers: 0x{il2cpp.codeRegistration64.reversePInvokeWrappers:X8}");
            stream.Position = (long)PortableExecutableReader.OffsetFromVA(il2cpp.codeRegistration64.reversePInvokeWrappers);
            il2cpp.reversePInvokeWrappers = reader.ReadArray<ulong>((int)il2cpp.codeRegistration64.reversePInvokeWrapperCount);
            for (int i = 0; i < il2cpp.reversePInvokeWrappers.Length; i++)
            {
                Console.WriteLine($"0x{il2cpp.reversePInvokeWrappers[i]:X8}");
            }

            Console.WriteLine($"il2cpp.codeRegistration64.genericMethodPointers: 0x{il2cpp.codeRegistration64.genericMethodPointers:X8}");
            stream.Position = (long)PortableExecutableReader.OffsetFromVA(il2cpp.codeRegistration64.genericMethodPointers);
            il2cpp.genericMethodPointers = reader.ReadArray<ulong>((int)il2cpp.codeRegistration64.genericMethodPointersCount);
            for (int i = 0; i < il2cpp.genericMethodPointers.Length; i++)
            {
                Console.WriteLine($"0x{il2cpp.genericMethodPointers[i]:X8}");
            }

            Console.WriteLine($"il2cpp.codeRegistration64.invokerPointers: 0x{il2cpp.codeRegistration64.invokerPointers:X8}");
            stream.Position = (long)PortableExecutableReader.OffsetFromVA(il2cpp.codeRegistration64.invokerPointers);
            il2cpp.invokerPointers = reader.ReadArray<ulong>((int)il2cpp.codeRegistration64.invokerPointersCount);
            for (int i = 0; i < il2cpp.invokerPointers.Length; i++)
            {
                Console.WriteLine($"0x{il2cpp.invokerPointers[i]:X8}");
            }

            Console.WriteLine($"il2cpp.codeRegistration64.customAttributeGenerators: 0x{il2cpp.codeRegistration64.customAttributeGenerators:X8}");
            stream.Position = (long)PortableExecutableReader.OffsetFromVA(il2cpp.codeRegistration64.customAttributeGenerators);
            il2cpp.customAttributeGenerators = reader.ReadArray<ulong>((int)il2cpp.codeRegistration64.customAttributeCount);
            for (int i = 0; i < il2cpp.customAttributeGenerators.Length; i++)
            {
                Console.WriteLine($"0x{il2cpp.customAttributeGenerators[i]:X8}");
            }

            Console.WriteLine($"il2cpp.codeRegistration64.unresolvedVirtualCallPointers: 0x{il2cpp.codeRegistration64.unresolvedVirtualCallPointers:X8}");
            stream.Position = (long)PortableExecutableReader.OffsetFromVA(il2cpp.codeRegistration64.unresolvedVirtualCallPointers);
            il2cpp.unresolvedVirtualCallPointers = reader.ReadArray<ulong>((int)il2cpp.codeRegistration64.unresolvedVirtualCallCount);
            for (int i = 0; i < il2cpp.unresolvedVirtualCallPointers.Length; i++)
            {
                Console.WriteLine($"0x{il2cpp.unresolvedVirtualCallPointers[i]:X8}");
            }

            Console.WriteLine($"il2cpp.codeRegistration64.interopData: 0x{il2cpp.codeRegistration64.interopData:X8}");
            stream.Position = (long)PortableExecutableReader.OffsetFromVA(il2cpp.codeRegistration64.interopData);
            il2cpp.interopData = reader.ReadArray<Il2CppInteropData>((int)il2cpp.codeRegistration64.interopDataCount);
            for (int i = 0; i < il2cpp.interopData.Length; i++)
            {
                il2cpp.interopData[i].DumpToConsole();
                //Console.WriteLine($"0x{il2cpp.interopData[i]:X8}");
            }

            Console.WriteLine($"il2cpp.codeRegistration64.windowsRuntimeFactoryTable: 0x{il2cpp.codeRegistration64.windowsRuntimeFactoryTable:X8}");
            stream.Position = (long)PortableExecutableReader.OffsetFromVA(il2cpp.codeRegistration64.windowsRuntimeFactoryTable);
            il2cpp.windowsRuntimeFactoryTable = reader.ReadArray<ulong>((int)il2cpp.codeRegistration64.windowsRuntimeFactoryCount);
            //for (int i = 0; i < il2cpp.windowsRuntimeFactoryTable.Length; i++)
            //{
            //    Console.WriteLine($"0x{il2cpp.windowsRuntimeFactoryTable[i]:X8}");
            //}
            
        }
    }
}
