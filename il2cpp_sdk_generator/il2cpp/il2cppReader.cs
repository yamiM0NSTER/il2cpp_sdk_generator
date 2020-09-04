using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;

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

            if (il2cpp.metadataRegistration64 != null)
                ReadMetadataRegistration();
        }

        public static void Process()
        {
            // Second pass to avoid stack overflow
            for (int i = 0; i < Metadata.typeDefinitions.Length; i++)
            {
                if (Metadata.typeDefinitions[i].parentIndex == -1)
                    continue;

                var type = il2cpp.types[Metadata.typeDefinitions[i].parentIndex];
                if(type.type == Il2CppTypeEnum.IL2CPP_TYPE_VALUETYPE || type.type == Il2CppTypeEnum.IL2CPP_TYPE_CLASS)
                {
                    if (type.data.klassIndex == i)
                        continue;
                    Metadata.resolvedTypes[i].parentType = MetadataReader.ResolveType(type.data.klassIndex);
                }
            }
            // Resolve enums
            for (int i = 0; i < Metadata.typeDefinitions.Length; i++)
            {
                if (!Metadata.typeDefinitions[i].isEnum)
                    continue;

                ResolvedEnum resolvedEnum = (ResolvedEnum)Metadata.resolvedTypes[i];

                // Resolve enum
                for (int k = 0; k < Metadata.typeDefinitions[i].field_count; k++)
                {
                    var fieldDef = Metadata.fieldDefinitions[k+ Metadata.typeDefinitions[i].fieldStart];
                    var type = il2cpp.types[fieldDef.typeIndex];
                    if((type.attrs & il2cpp_Constants.FIELD_ATTRIBUTE_STATIC) != 0)
                    {
                        MetadataReader.GetDefaultFieldValue(k + Metadata.typeDefinitions[i].fieldStart, out var val);
                        //Console.WriteLine($"{MetadataReader.GetString(fieldDef.nameIndex)} = {val}");
                        resolvedEnum.values.Add(MetadataReader.GetString(fieldDef.nameIndex), val);
                        //values
                    }
                    //type.DumpToConsole();
                }

                Console.WriteLine(resolvedEnum.ToCode(2));

                //   Metadata.resolvedTypes[i].parentType = ResolveType(Metadata.typeDefinitions[i].parentIndex);
            }

            // TODO: Trusted references
            return;

            int CntBefore = CodeScanner.funcPtrs.Count;
            for (int i =0;i< il2cpp.methodPointers.Length;i++)
            {
                if (il2cpp.methodPointers[i] == 0)
                    continue;

                if (CodeScanner.funcPtrs.Contains(il2cpp.methodPointers[i]))
                    continue;

                CodeScanner.funcPtrs.Add(il2cpp.methodPointers[i]);
                Console.WriteLine($"[0x{il2cpp.methodPointers[i]:X8}]");
            }
            for (int i = 0; i < il2cpp.reversePInvokeWrappers.Length; i++)
            {
                if (il2cpp.reversePInvokeWrappers[i] == 0)
                    continue;

                if (CodeScanner.funcPtrs.Contains(il2cpp.reversePInvokeWrappers[i]))
                    continue;

                CodeScanner.funcPtrs.Add(il2cpp.reversePInvokeWrappers[i]);
            }
            for (int i = 0; i < il2cpp.genericMethodPointers.Length; i++)
            {
                if (il2cpp.genericMethodPointers[i] == 0)
                    continue;

                if (CodeScanner.funcPtrs.Contains(il2cpp.genericMethodPointers[i]))
                    continue;

                CodeScanner.funcPtrs.Add(il2cpp.genericMethodPointers[i]);
            }
            for (int i = 0; i < il2cpp.invokerPointers.Length; i++)
            {
                if (il2cpp.invokerPointers[i] == 0)
                    continue;

                if (CodeScanner.funcPtrs.Contains(il2cpp.invokerPointers[i]))
                    continue;

                CodeScanner.funcPtrs.Add(il2cpp.invokerPointers[i]);
            }
            for (int i = 0; i < il2cpp.unresolvedVirtualCallPointers.Length; i++)
            {
                if (il2cpp.unresolvedVirtualCallPointers[i] == 0)
                    continue;

                if (CodeScanner.funcPtrs.Contains(il2cpp.unresolvedVirtualCallPointers[i]))
                    continue;

                CodeScanner.funcPtrs.Add(il2cpp.unresolvedVirtualCallPointers[i]);
            }
            
            Console.WriteLine($"il2cppReader::Process() before funcPtrs[{CntBefore}]");
            Console.WriteLine($"il2cppReader::Process() funcPtrs[{CodeScanner.funcPtrs.Count}]");
            CodeScanner.funcPtrs.Sort();
            Console.WriteLine($"[0x{CodeScanner.funcPtrs[0]:X8}]");
            Console.WriteLine($"[0x{CodeScanner.funcPtrs[CodeScanner.funcPtrs.Count - 1]:X8}]");
        }

        private static void ReadMetadataRegistration()
        {
            Console.WriteLine($"il2cpp.metadataRegistration64.genericClasses: 0x{il2cpp.metadataRegistration64.genericClasses:X8}");
            stream.Position = (long)Offset.FromVA(il2cpp.metadataRegistration64.genericClasses);
            il2cpp.genericClassesPtrs = reader.ReadArray<ulong>((int)il2cpp.metadataRegistration64.genericClassesCount);
            il2cpp.genericClasses = new Il2CppGenericClass[il2cpp.metadataRegistration64.genericClassesCount];
            for (int i = 0; i < il2cpp.genericClassesPtrs.Length; i++)
            {
                stream.Position = (long)Offset.FromVA(il2cpp.genericClassesPtrs[i]);
                il2cpp.genericClasses[i] = reader.Read<Il2CppGenericClass>();
                //Console.WriteLine($"0x{il2cpp.genericClassesPtrs[i]:X8}");
                if(il2cpp.genericClasses[i].cached_class != 0)
                    il2cpp.genericClasses[i].DumpToConsole();
            }

            Console.WriteLine($"il2cpp.metadataRegistration64.genericInsts: 0x{il2cpp.metadataRegistration64.genericInsts:X8}");
            stream.Position = (long)Offset.FromVA(il2cpp.metadataRegistration64.genericInsts);
            il2cpp.genericInstsPtrs = reader.ReadArray<ulong>((int)il2cpp.metadataRegistration64.genericInstsCount);
            il2cpp.genericInsts = new Il2CppGenericInst[il2cpp.metadataRegistration64.genericInstsCount];
            for (int i = 0; i < il2cpp.genericInstsPtrs.Length; i++)
            {
                stream.Position = (long)Offset.FromVA(il2cpp.genericInstsPtrs[i]);
                il2cpp.genericInsts[i] = reader.Read<Il2CppGenericInst>();
                //Console.WriteLine($"0x{il2cpp.genericClassesPtrs[i]:X8}");
                //il2cpp.genericInsts[i].DumpToConsole();
            }

            Console.WriteLine($"il2cpp.metadataRegistration64.genericMethodTable: 0x{il2cpp.metadataRegistration64.genericMethodTable:X8}");
            stream.Position = (long)Offset.FromVA(il2cpp.metadataRegistration64.genericMethodTable);
            il2cpp.genericMethodTable = reader.ReadArray<Il2CppGenericMethodFunctionsDefinitions>((int)il2cpp.metadataRegistration64.genericMethodTableCount);
            //for (int i = 0; i < il2cpp.genericMethodTable.Length; i++)
            //{
            //    il2cpp.genericMethodTable[i].DumpToConsole();
            //}

            Console.WriteLine($"il2cpp.metadataRegistration64.types: 0x{il2cpp.metadataRegistration64.types:X8}");
            stream.Position = (long)Offset.FromVA(il2cpp.metadataRegistration64.types);
            il2cpp.typesPtrs = reader.ReadArray<ulong>((int)il2cpp.metadataRegistration64.typesCount);
            il2cpp.types = new Il2CppType[il2cpp.metadataRegistration64.typesCount];
            for (int i = 0; i < il2cpp.typesPtrs.Length; i++)
            {
                stream.Position = (long)Offset.FromVA(il2cpp.typesPtrs[i]);
                il2cpp.types[i] = reader.Read<Il2CppType>();
                //il2cpp.types[i].DumpToConsole();
                //Console.WriteLine($"0x{il2cpp.types[i].type}");
                //Console.WriteLine($"0x{il2cpp.genericClassesPtrs[i]:X8}");
                //il2cpp.genericInsts[i].DumpToConsole();
            }

            Console.WriteLine($"il2cpp.metadataRegistration64.methodSpecs: 0x{il2cpp.metadataRegistration64.methodSpecs:X8}");
            stream.Position = (long)Offset.FromVA(il2cpp.metadataRegistration64.methodSpecs);
            il2cpp.methodSpecs = reader.ReadArray<Il2CppMethodSpec>((int)il2cpp.metadataRegistration64.methodSpecsCount);
            //for (int i = 0; i < il2cpp.methodSpecs.Length; i++)
            //{
            //    il2cpp.methodSpecs[i].DumpToConsole();
            //}

            Console.WriteLine($"il2cpp.metadataRegistration64.fieldOffsets: 0x{il2cpp.metadataRegistration64.fieldOffsets:X8}");
            stream.Position = (long)Offset.FromVA(il2cpp.metadataRegistration64.fieldOffsets);
            il2cpp.fieldOffsetsPtrs = reader.ReadArray<ulong>((int)il2cpp.metadataRegistration64.fieldOffsetsCount);
            il2cpp.fieldOffsets = new Int32[il2cpp.metadataRegistration64.fieldOffsetsCount];
            for (int i = 0; i < il2cpp.fieldOffsetsPtrs.Length; i++)
            {
                // Skip null ptrs
                if (il2cpp.fieldOffsetsPtrs[i] == 0)
                    continue;

                stream.Position = (long)Offset.FromVA(il2cpp.fieldOffsetsPtrs[i]);
                il2cpp.fieldOffsets[i] = reader.Read<Int32>();
                //il2cpp.fieldOffsets[i].DumpToConsole();
                //il2cpp.types[i].DumpToConsole();
                //Console.WriteLine($"0x{il2cpp.types[i].type}");
                //Console.WriteLine($"0x{il2cpp.genericClassesPtrs[i]:X8}");
                //il2cpp.genericInsts[i].DumpToConsole();
            }

            Console.WriteLine($"il2cpp.metadataRegistration64.typeDefinitionsSizes: 0x{il2cpp.metadataRegistration64.typeDefinitionsSizes:X8}");
            stream.Position = (long)Offset.FromVA(il2cpp.metadataRegistration64.typeDefinitionsSizes);
            il2cpp.typeDefinitionsSizesPtrs = reader.ReadArray<ulong>((int)il2cpp.metadataRegistration64.typeDefinitionsSizesCount);
            il2cpp.typeDefinitionsSizes = new Il2CppTypeDefinitionSizes[il2cpp.metadataRegistration64.typeDefinitionsSizesCount];
            for (int i = 0; i < il2cpp.typeDefinitionsSizesPtrs.Length; i++)
            {
                // Skip null ptrs
                if (il2cpp.typeDefinitionsSizesPtrs[i] == 0)
                    continue;

                stream.Position = (long)Offset.FromVA(il2cpp.typeDefinitionsSizesPtrs[i]);
                il2cpp.typeDefinitionsSizes[i] = reader.Read<Il2CppTypeDefinitionSizes>();
                //il2cpp.typeDefinitionsSizes[i].DumpToConsole();
            //    //il2cpp.types[i].DumpToConsole();
            //    //Console.WriteLine($"0x{il2cpp.types[i].type}");
            //    //Console.WriteLine($"0x{il2cpp.genericClassesPtrs[i]:X8}");
            //    //il2cpp.genericInsts[i].DumpToConsole();
            }

            Console.WriteLine($"il2cpp.metadataRegistration64.metadataUsages: 0x{il2cpp.metadataRegistration64.metadataUsages:X8}");
            stream.Position = (long)Offset.FromVA(il2cpp.metadataRegistration64.metadataUsages);
            il2cpp.metadataUsages = reader.ReadArray<ulong>((int)il2cpp.metadataRegistration64.metadataUsagesCount);
            //for (int i = 0; i < il2cpp.metadataUsages.Length; i++)
            //{
            //    il2cpp.metadataUsages[i].DumpToConsole();
            //}
        }

        private static void ReadCodeRegistration()
        {
            Console.WriteLine($"il2cpp.codeRegistration64.methodPointers: 0x{il2cpp.codeRegistration64.methodPointers:X8}");
            stream.Position = (long)Offset.FromVA(il2cpp.codeRegistration64.methodPointers);
            il2cpp.methodPointers = reader.ReadArray<ulong>((int)il2cpp.codeRegistration64.methodPointersCount);
            //for(int i=0;i< il2cpp.methodPointers.Length;i++)
            //{
            //    Console.WriteLine($"0x{il2cpp.methodPointers[i]:X8}");
            //}
            //stream.Position = il2cpp.codeRegistration64.methodPointers;

            Console.WriteLine($"il2cpp.codeRegistration64.reversePInvokeWrappers: 0x{il2cpp.codeRegistration64.reversePInvokeWrappers:X8}");
            stream.Position = (long)Offset.FromVA(il2cpp.codeRegistration64.reversePInvokeWrappers);
            il2cpp.reversePInvokeWrappers = reader.ReadArray<ulong>((int)il2cpp.codeRegistration64.reversePInvokeWrapperCount);
            //for (int i = 0; i < il2cpp.reversePInvokeWrappers.Length; i++)
            //{
            //    Console.WriteLine($"0x{il2cpp.reversePInvokeWrappers[i]:X8}");
            //}

            Console.WriteLine($"il2cpp.codeRegistration64.genericMethodPointers: 0x{il2cpp.codeRegistration64.genericMethodPointers:X8}");
            stream.Position = (long)Offset.FromVA(il2cpp.codeRegistration64.genericMethodPointers);
            il2cpp.genericMethodPointers = reader.ReadArray<ulong>((int)il2cpp.codeRegistration64.genericMethodPointersCount);
            //for (int i = 0; i < il2cpp.genericMethodPointers.Length; i++)
            //{
            //    Console.WriteLine($"0x{il2cpp.genericMethodPointers[i]:X8}");
            //}

            Console.WriteLine($"il2cpp.codeRegistration64.invokerPointers: 0x{il2cpp.codeRegistration64.invokerPointers:X8}");
            stream.Position = (long)Offset.FromVA(il2cpp.codeRegistration64.invokerPointers);
            il2cpp.invokerPointers = reader.ReadArray<ulong>((int)il2cpp.codeRegistration64.invokerPointersCount);
            //for (int i = 0; i < il2cpp.invokerPointers.Length; i++)
            //{
            //    Console.WriteLine($"0x{il2cpp.invokerPointers[i]:X8}");
            //}

            Console.WriteLine($"il2cpp.codeRegistration64.customAttributeGenerators: 0x{il2cpp.codeRegistration64.customAttributeGenerators:X8}");
            stream.Position = (long)Offset.FromVA(il2cpp.codeRegistration64.customAttributeGenerators);
            il2cpp.customAttributeGenerators = reader.ReadArray<ulong>((int)il2cpp.codeRegistration64.customAttributeCount);
            //for (int i = 0; i < il2cpp.customAttributeGenerators.Length; i++)
            //{
            //    Console.WriteLine($"0x{il2cpp.customAttributeGenerators[i]:X8}");
            //}

            Console.WriteLine($"il2cpp.codeRegistration64.unresolvedVirtualCallPointers: 0x{il2cpp.codeRegistration64.unresolvedVirtualCallPointers:X8}");
            stream.Position = (long)Offset.FromVA(il2cpp.codeRegistration64.unresolvedVirtualCallPointers);
            il2cpp.unresolvedVirtualCallPointers = reader.ReadArray<ulong>((int)il2cpp.codeRegistration64.unresolvedVirtualCallCount);
            //for (int i = 0; i < il2cpp.unresolvedVirtualCallPointers.Length; i++)
            //{
            //    Console.WriteLine($"0x{il2cpp.unresolvedVirtualCallPointers[i]:X8}");
            //}

            Console.WriteLine($"il2cpp.codeRegistration64.interopData: 0x{il2cpp.codeRegistration64.interopData:X8}");
            stream.Position = (long)Offset.FromVA(il2cpp.codeRegistration64.interopData);
            il2cpp.interopData = reader.ReadArray<Il2CppInteropData>((int)il2cpp.codeRegistration64.interopDataCount);
            //for (int i = 0; i < il2cpp.interopData.Length; i++)
            //{
            //    il2cpp.interopData[i].DumpToConsole();
            //    //Console.WriteLine($"0x{il2cpp.interopData[i]:X8}");
            //}

            Console.WriteLine($"il2cpp.codeRegistration64.windowsRuntimeFactoryTable: 0x{il2cpp.codeRegistration64.windowsRuntimeFactoryTable:X8}");
            stream.Position = (long)Offset.FromVA(il2cpp.codeRegistration64.windowsRuntimeFactoryTable);
            il2cpp.windowsRuntimeFactoryTable = reader.ReadArray<ulong>((int)il2cpp.codeRegistration64.windowsRuntimeFactoryCount);
            //for (int i = 0; i < il2cpp.windowsRuntimeFactoryTable.Length; i++)
            //{
            //    Console.WriteLine($"0x{il2cpp.windowsRuntimeFactoryTable[i]:X8}");
            //}
            
        }
    }
}
