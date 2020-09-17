using System;
using System.Collections.Generic;
using System.Linq;
using Iced.Intel;

namespace il2cpp_sdk_generator
{
    class CodeScanner
    {
        static Byte[] m_peBytes;
        public static List<ulong> funcPtrs = new List<ulong>(); // function VA's
        static Dictionary<ulong, ulong> m_refs = new Dictionary<ulong, ulong>(); // reference VA's
        static Dictionary<ulong, List<ulong>> m_mapFunctionReferences = new Dictionary<ulong, List<ulong>>(); // reference VA's
        static Dictionary<ulong, List<ulong>> m_mapReferencesToFunction = new Dictionary<ulong, List<ulong>>(); // reference VA's
        static IMAGE_SECTION_HEADER textSection;
        static IMAGE_SECTION_HEADER il2cppSection;

        public static void Scan(Byte[] bytes)
        {
            m_peBytes = bytes;

            ScanTextSection();
            Scanil2cppSection();

            funcPtrs = funcPtrs.Distinct().ToList();
            funcPtrs.Sort();
            for (int i =0;i< funcPtrs.Count;i++)
            {
                m_mapFunctionReferences.Add(funcPtrs[i], new List<ulong>());
                m_mapReferencesToFunction.Add(funcPtrs[i], new List<ulong>());
            }

            var curFunc = funcPtrs[0];
            var nextFunc = funcPtrs[1];
            int funcPtrNum = 2;
            foreach (var pair in m_refs)
            {
                if(pair.Key >= nextFunc)
                {
                    curFunc = nextFunc;
                    if (funcPtrNum < funcPtrs.Count)
                        nextFunc = funcPtrs[funcPtrNum];
                    else
                        nextFunc = UInt64.MaxValue;
                    funcPtrNum++;
                }

                m_mapFunctionReferences[curFunc].Add(pair.Value);
                m_mapReferencesToFunction[pair.Value].Add(curFunc);
            }

            Console.WriteLine($"Functions[{funcPtrs.Count}]:");
            Console.WriteLine($"[0x{funcPtrs[0]:X8}]");
            Console.WriteLine($"[0x{funcPtrs[funcPtrs.Count - 1]:X8}]");
            //foreach (var addr in funcPtrs)
            //{
            //    //Console.WriteLine($"[0x{addr:X8}]");
            //}
            Console.WriteLine($"References[{m_refs.Count}]");

            for (int i = 0; i < Metadata.resolvedImages.Count; i++)
            {
                //if (Metadata.resolvedImages[i].Name != "Assembly-CSharp.dll")
                //    continue;

                ScanGameAssembly(Metadata.resolvedImages[i]);
            }

            ResolveTrustedRefs();
        }
        
        static void ScanTextSection()
        {
            if (!PortableExecutable.m_mapSections.TryGetValue("il2cpp", out var il2cppSection))
                return;

            if (!PortableExecutable.m_mapSections.TryGetValue(".text", out var textSection))
                return;

            var codeReader = new ByteArrayCodeReader(m_peBytes, (int)textSection.PointerToRawData, (int)textSection.SizeOfRawData);
            var decoder = Decoder.Create(64, codeReader);
            ulong textBeginIP = textSection.VirtualAddress + PortableExecutable.imageOptionalHeader64.ImageBase;
            decoder.IP = textBeginIP;
            ulong textEndRip = textBeginIP + textSection.SizeOfRawData;

            ulong il2cppBeginIP = il2cppSection.VirtualAddress + PortableExecutable.imageOptionalHeader64.ImageBase;
            ulong il2cppEndRip = il2cppBeginIP + il2cppSection.SizeOfRawData;
            while (decoder.IP < textEndRip)
            {
                // The method allocates an uninitialized element at the end of the list and
                // returns a reference to it which is initialized by Decode().
                ulong addr = decoder.IP;
                decoder.Decode(out var instruction);
                if (instruction.Mnemonic == Mnemonic.Call)
                {
                    if (instruction.Op0Kind == OpKind.Memory || instruction.Op0Kind == OpKind.Register)
                        continue;

                    ulong targetAddr = instruction.NearBranch64;
                    if (!(targetAddr >= il2cppBeginIP && targetAddr < il2cppEndRip) && !(targetAddr >= textBeginIP && targetAddr < textEndRip))
                    {
                        Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind} {instruction.IsCallNear}");
                        continue;
                    }

                    //if (!funcPtrs.Contains(targetAddr))
                    {
                        //Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind}");
                        funcPtrs.Add(targetAddr);
                    }
                    m_refs.Add(addr, targetAddr);
                }
                else if (instruction.Mnemonic == Mnemonic.Jmp)
                {
                    if(instruction.OpCode.Code == Code.Jmp_rel8_64)
                    {
                        continue;
                    }

                    if (instruction.OpCode.Code != Code.Jmp_rel32_64)
                    {
                        Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind} {instruction.OpCode.Code.ToString()}");
                        continue;
                    }

                    if (instruction.Op0Kind == OpKind.Memory || instruction.Op0Kind == OpKind.Register)
                        continue;

                    ulong targetAddr = instruction.NearBranch64;


                    if (!(targetAddr >= il2cppBeginIP && targetAddr < il2cppEndRip) && !(targetAddr >= textBeginIP && targetAddr < textEndRip))
                    {
                        Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind} {instruction.IsCallNear}");
                        continue;
                    }
                    //// TODO: ignore addresses outside of range
                    //if (targetAddr == 0x169E4EA10)
                    //    Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind}");

                    //if (!funcPtrs.Contains(targetAddr))
                    {
                        //Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind}");
                        funcPtrs.Add(targetAddr);
                    }
                    m_refs.Add(addr, targetAddr);
                }
            }
        }

        static void Scanil2cppSection()
        {
            if (!PortableExecutable.m_mapSections.TryGetValue("il2cpp", out var il2cppSection))
                return;

            if (!PortableExecutable.m_mapSections.TryGetValue(".text", out var textSection))
                return;

            var codeReader = new ByteArrayCodeReader(m_peBytes, (int)il2cppSection.PointerToRawData, (int)il2cppSection.SizeOfRawData);
            var decoder = Decoder.Create(64, codeReader);
            ulong beginIP = il2cppSection.VirtualAddress + PortableExecutable.imageOptionalHeader64.ImageBase;
            decoder.IP = beginIP;
            ulong endRip = beginIP + il2cppSection.SizeOfRawData;

            ulong textBeginIP = textSection.VirtualAddress + PortableExecutable.imageOptionalHeader64.ImageBase;
            ulong textEndRip = textBeginIP + textSection.SizeOfRawData;
            while (decoder.IP < endRip)
            {
                // The method allocates an uninitialized element at the end of the list and
                // returns a reference to it which is initialized by Decode().
                ulong addr = decoder.IP;
                decoder.Decode(out var instruction);
                if (instruction.Mnemonic == Mnemonic.Call)
                {
                    if (instruction.Op0Kind == OpKind.Memory || instruction.Op0Kind == OpKind.Register)
                        continue;

                    ulong targetAddr = instruction.NearBranch64;
                    //instruction

                    if (!(targetAddr >= beginIP && targetAddr < endRip) && !(targetAddr >= textBeginIP && targetAddr < textEndRip))
                    {
                        Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind} {instruction.IsCallNear}");
                        continue;
                    }
                    //if (targetAddr == 0x00000000)
                    //    Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind}");

                    //if (!funcPtrs.Contains(targetAddr))
                    {
                        //Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind}");
                        funcPtrs.Add(targetAddr);
                    }
                    m_refs.Add(addr, targetAddr);
                }
                else if (instruction.Mnemonic == Mnemonic.Jmp)
                {
                    if (instruction.Op0Kind == OpKind.Memory || instruction.Op0Kind == OpKind.Register)
                        continue;

                    if (instruction.OpCode.Code == Code.Jmp_rel8_64 || instruction.OpCode.Code == Code.Jmp_rm64)
                        continue;

                    if (instruction.OpCode.Code != Code.Jmp_rel32_64)
                    {
                        Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind} {instruction.OpCode.Code.ToString()}");
                        continue;
                    }

                    ulong targetAddr = instruction.NearBranch64;
                    if (!(targetAddr >= beginIP && targetAddr < endRip) && !(targetAddr >= textBeginIP && targetAddr < textEndRip))
                    {
                        Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind} {instruction.IsCallNear}");
                        continue;
                    }
                    // TODO: ignore addresses outside of range
                    //if (targetAddr == 0x169E4EA10)
                    //    Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind}");

                    //if (!funcPtrs.Contains(targetAddr))
                    {
                        //Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind} {instruction.OpCode.Code.ToString()}");
                        funcPtrs.Add(targetAddr);
                    }
                    m_refs.Add(addr, targetAddr);
                }
            }
        }

        static ResolvedClass playerClass = null;
        static Dictionary<ulong, List<ResolvedMethod>> mapTrustedMethods = new Dictionary<ulong, List<ResolvedMethod>>();
        static Dictionary<ulong, List<ResolvedMethod>> mapMangledMethods = new Dictionary<ulong, List<ResolvedMethod>>();

        static void ScanGameAssembly(ResolvedImage resolvedImage)
        {
            foreach (var resolvedNamespace in resolvedImage.Namespaces.Values)
            {
                for (int k = 0; k < resolvedNamespace.Types.Count; k++)
                {
                    AddRefsToResolve(resolvedNamespace.Types[k]);
                }
            }
        }

        static void AddRefsToResolve(ResolvedType resolvedType)
        {
            if (resolvedType is ResolvedClass)
            {
                ResolvedClass resolvedClass = resolvedType as ResolvedClass;
                for (int i = 0; i < resolvedClass.miMethods.Count; i++)
                {
                    if (resolvedClass.miMethods[i].methodDef.methodIndex == -1 || resolvedClass.miMethods[i].methodDef.methodIndex >= il2cpp.codeRegistration64.methodPointersCount)
                        continue;

                    //if (!resolvedClass.miMethods[i].isMangled)
                    //    continue;

                    var methodPtr = il2cpp.methodPointers[resolvedClass.miMethods[i].methodDef.methodIndex];
                    if (!resolvedClass.miMethods[i].isMangled)
                    {
                        if (!mapTrustedMethods.ContainsKey(methodPtr))
                        {
                            mapTrustedMethods.Add(methodPtr, new List<ResolvedMethod>());
                        }
                        mapTrustedMethods[methodPtr].Add(resolvedClass.miMethods[i]);
                    }
                    else
                    {
                        if (!mapMangledMethods.ContainsKey(methodPtr))
                        {
                            mapMangledMethods.Add(methodPtr, new List<ResolvedMethod>());
                        }
                        mapMangledMethods[methodPtr].Add(resolvedClass.miMethods[i]);
                    }
                }

                if (resolvedClass.Name == "Player")
                {
                    playerClass = resolvedClass;
                }

            }
            else if (resolvedType is ResolvedStruct)
            {
                ResolvedStruct resolvedStruct = resolvedType as ResolvedStruct;
                for (int i = 0; i < resolvedStruct.miMethods.Count; i++)
                {
                    if (resolvedStruct.miMethods[i].methodDef.methodIndex == -1 || resolvedStruct.miMethods[i].methodDef.methodIndex >= il2cpp.codeRegistration64.methodPointersCount)
                        continue;

                    //if (!resolvedClass.miMethods[i].isMangled)
                    //    continue;

                    var methodPtr = il2cpp.methodPointers[resolvedStruct.miMethods[i].methodDef.methodIndex];
                    if (!resolvedStruct.miMethods[i].isMangled)
                    {
                        if (!mapTrustedMethods.ContainsKey(methodPtr))
                        {
                            mapTrustedMethods.Add(methodPtr, new List<ResolvedMethod>());
                        }
                        mapTrustedMethods[methodPtr].Add(resolvedStruct.miMethods[i]);
                    }
                    else
                    {
                        if (!mapMangledMethods.ContainsKey(methodPtr))
                        {
                            mapMangledMethods.Add(methodPtr, new List<ResolvedMethod>());
                        }
                        mapMangledMethods[methodPtr].Add(resolvedStruct.miMethods[i]);
                    }
                }
            }

            for (int i = 0; i < resolvedType.nestedTypes.Count; i++)
                AddRefsToResolve(resolvedType.nestedTypes[i]);
        }

        static void ResolveTrustedRefs()
        {
            while (mapTrustedMethods.Count > 0)
            {
                Console.WriteLine();
                // Pass
                var trustedMethods = mapTrustedMethods.ToArray();
                mapTrustedMethods.Clear();
                for (int i = 0; i < trustedMethods.Length; i++)
                {
                    var pointers = m_mapFunctionReferences[trustedMethods[i].Key];

                    for (int k = 0; k < pointers.Count; k++)
                    {
                        if (!mapMangledMethods.TryGetValue(pointers[k], out var methods))
                            continue;

                        methods.ForEach((m) => { m.isReferenced = true; });
                        mapTrustedMethods.Add(pointers[k], methods);
                        mapMangledMethods.Remove(pointers[k]);
                    }
                }

                for (int i = 0; i < playerClass.miMethods.Count; i++)
                {
                    if (!playerClass.miMethods[i].isMangled || (playerClass.miMethods[i].isMangled && playerClass.miMethods[i].isReferenced))
                        Console.WriteLine($"{playerClass.GetFullName()} method[{playerClass.miMethods[i].Name}]({playerClass.miMethods[i].methodIndex - playerClass.typeDef.methodStart }) is referenced");
                    else
                        Console.WriteLine($"{playerClass.GetFullName()} method[{playerClass.miMethods[i].Name}]({playerClass.miMethods[i].methodIndex - playerClass.typeDef.methodStart }) is not referenced");
                }
            }
        }
    }
}
