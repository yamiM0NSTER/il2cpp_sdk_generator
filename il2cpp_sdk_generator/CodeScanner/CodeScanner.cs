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
        static IMAGE_SECTION_HEADER textSection;
        static IMAGE_SECTION_HEADER il2cppSection;

        public static void Scan(Byte[] bytes)
        {
            m_peBytes = bytes;



            ScanTextSection();
            Scanil2cppSection();

            funcPtrs.Sort();
            Console.WriteLine($"Functions[{funcPtrs.Count}]:");
            Console.WriteLine($"[0x{funcPtrs[0]:X8}]");
            Console.WriteLine($"[0x{funcPtrs[funcPtrs.Count-1]:X8}]");
            //foreach (var addr in funcPtrs)
            //{
            //    //Console.WriteLine($"[0x{addr:X8}]");
            //}
            Console.WriteLine($"References[{m_refs.Count}]");
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

                    if (!funcPtrs.Contains(targetAddr))
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

                    if (!funcPtrs.Contains(targetAddr))
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

                    if (!funcPtrs.Contains(targetAddr))
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

                    if (!funcPtrs.Contains(targetAddr))
                    {
                        //Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind} {instruction.OpCode.Code.ToString()}");
                        funcPtrs.Add(targetAddr);
                    }
                    m_refs.Add(addr, targetAddr);
                }
            }
        }
        
    }
}
