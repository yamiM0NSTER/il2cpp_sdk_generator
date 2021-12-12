using System;
using System.Collections.Generic;
using System.Linq;
using Iced.Intel;

namespace il2cpp_sdk_generator
{
  public class CodeScanner
  {
    static Byte[] m_peBytes;
    public static List<ulong> funcPtrs = new List<ulong>(); // function VA's
    static Dictionary<ulong, ulong> m_refs = new Dictionary<ulong, ulong>(); // reference VA's
    public static Dictionary<ulong, List<ulong>> m_mapFunctionReferences = new Dictionary<ulong, List<ulong>>(); // reference VA's
    public static Dictionary<ulong, List<ulong>> m_mapReferencesToFunction = new Dictionary<ulong, List<ulong>>(); // reference VA's
    public static SortedDictionary<ulong, List<uint>> m_mapFunctionStringLiterals = new SortedDictionary<ulong, List<uint>>();
    private static SortedDictionary<ulong, uint> m_StringLiteralsPointers = new SortedDictionary<ulong, uint>();
    static IMAGE_SECTION_HEADER textSection;
    static IMAGE_SECTION_HEADER il2cppSection;

    public static ulong Delegate_CombineMethodPtr;

    public static void Scan(Byte[] bytes)
    {
      // Get Callback parent type
      for (int i = 0; i < Metadata.resolvedTypes.Length; i++)
      {
        if (Metadata.resolvedTypes[i].isNested)
          continue;

        if (Metadata.resolvedTypes[i].Name != "Delegate")
          continue;

        if (Metadata.resolvedTypes[i].Namespace != "System")
          continue;


        ResolvedClass resolvedClass = Metadata.resolvedTypes[i] as ResolvedClass;

        for (int k = 0; k < resolvedClass.miMethods.Count; k++)
        {
          if (resolvedClass.miMethods[k].Name != "Combine")
            continue;
          if (resolvedClass.miMethods[k].resolvedParameters.Count != 2)
            continue;

          Delegate_CombineMethodPtr = resolvedClass.miMethods[k].methodPtr;
          break;
        }
        //multicastDelegateType = Metadata.resolvedTypes[i];
        break;
      }

      m_peBytes = bytes;

      //ScanTextSection();
      Scanil2cppSection();

      funcPtrs = funcPtrs.Distinct().ToList();
      funcPtrs.Sort();
      for (int i = 0; i < funcPtrs.Count; i++)
      {
        m_mapFunctionReferences.Add(funcPtrs[i], new List<ulong>());
        m_mapReferencesToFunction.Add(funcPtrs[i], new List<ulong>());
        m_mapFunctionStringLiterals.Add(funcPtrs[i], new List<uint>());
      }


      var curFunc = funcPtrs[0];
      var nextFunc = funcPtrs[1];
      int funcPtrNum = 2;

      foreach (var pair in m_refs)
      {
        while (pair.Key >= nextFunc)
        {
          curFunc = nextFunc;
          nextFunc = GetNextFunc(ref funcPtrNum, curFunc);
        }

        m_mapFunctionReferences[curFunc].Add(pair.Value);
        if (m_mapReferencesToFunction.ContainsKey(pair.Value))
          m_mapReferencesToFunction[pair.Value].Add(curFunc);
      }

      curFunc = funcPtrs[0];
      nextFunc = funcPtrs[1];
      funcPtrNum = 2;

      foreach (var pair in m_StringLiteralsPointers)
      {
        while (pair.Key >= nextFunc)
        {
          curFunc = nextFunc;

          nextFunc = GetNextFunc(ref funcPtrNum, curFunc);
        }

        m_mapFunctionStringLiterals[curFunc].Add(pair.Value);
      }

      //RUNTIME_FUNCTION runtimeFunc = null;//PortableExecutable.m_mapRuntimeFunctionPtrs
      //bool bFoundRuntimeFunc = PortableExecutable.m_mapRuntimeFunctionPtrs.TryGetValue(funcPtrs[0], out runtimeFunc);
      //ulong funcEnd = 0;
      //if (bFoundRuntimeFunc)
      //    funcEnd = VA.FromRVA(runtimeFunc.EndAddress);

      //foreach (var pair in m_refs)
      //{
      //    // Next func
      //    if (bFoundRuntimeFunc && pair.Key > funcEnd)
      //    {
      //        curFunc = nextFunc;
      //        //curFunc = funcPtrs.FirstOrDefault(ptr => ptr > funcEnd);
      //        bFoundRuntimeFunc = PortableExecutable.m_mapRuntimeFunctionPtrs.TryGetValue(curFunc, out runtimeFunc);
      //        if (bFoundRuntimeFunc)
      //        {
      //            funcEnd = VA.FromRVA(runtimeFunc.EndAddress);
      //            nextFunc = GetNextFunc(ref funcPtrNum, funcEnd);
      //            //nextFunc = funcPtrs.FirstOrDefault(ptr => ptr > funcEnd);
      //        }
      //        else
      //        {
      //            nextFunc = GetNextFunc(ref funcPtrNum, curFunc);
      //            //nextFunc = funcPtrs.FirstOrDefault(ptr => ptr > curFunc);
      //        }
      //        //if (nextFunc == 0)
      //        //    nextFunc = UInt64.MaxValue;
      //    }
      //    else if (!bFoundRuntimeFunc && pair.Key >= nextFunc)
      //    {
      //        curFunc = nextFunc;
      //        if (curFunc == 0x181E404E0)
      //        {

      //        }
      //        bFoundRuntimeFunc = PortableExecutable.m_mapRuntimeFunctionPtrs.TryGetValue(curFunc, out runtimeFunc);
      //        if (bFoundRuntimeFunc)
      //        {
      //            funcEnd = VA.FromRVA(runtimeFunc.EndAddress);
      //            nextFunc = GetNextFunc(ref funcPtrNum, funcEnd);
      //            //nextFunc = funcPtrs.FirstOrDefault(ptr => ptr > funcEnd);
      //        }
      //        else
      //        {
      //            nextFunc = GetNextFunc(ref funcPtrNum, curFunc);
      //            //nextFunc = funcPtrs.FirstOrDefault(ptr => ptr > curFunc);
      //        }
      //        //if (nextFunc == 0)
      //        //    nextFunc = UInt64.MaxValue;

      //        //nextFunc = funcPtrs.FirstOrDefault(ptr => ptr > curFunc);


      //        //if (funcPtrNum < funcPtrs.Count)
      //        //    nextFunc = funcPtrs[funcPtrNum];
      //        //else
      //        //    nextFunc = UInt64.MaxValue;
      //        //funcPtrNum++;
      //    }

      //    m_mapFunctionReferences[curFunc].Add(pair.Value);
      //    m_mapReferencesToFunction[pair.Value].Add(curFunc);
      //}

      Console.WriteLine($"Functions[{funcPtrs.Count}]:");
      Console.WriteLine($"[0x{funcPtrs[0]:X8}]");
      Console.WriteLine($"[0x{funcPtrs[funcPtrs.Count - 1]:X8}]");
      //foreach (var addr in funcPtrs)
      //{
      //    //Console.WriteLine($"[0x{addr:X8}]");
      //}
      Console.WriteLine($"String Literals[{m_StringLiteralsPointers.Count}]:");
      //foreach (var pair in m_StringLiteralsPointers)
      //{
      //    Console.WriteLine($" [{pair.Key}] => \"{pair.Value}\"");
      //}

      Console.WriteLine($"String Literals[{m_mapFunctionStringLiterals.Count}]:");
      //foreach (var pair in m_mapFunctionStringLiterals)
      //{
      //    if (pair.Value.Count > 1)
      //    {
      //        Console.WriteLine($" [0x{pair.Key:X8}] => \"{pair.Value.Count}\"");
      //    }
      //}
      //


      //

      Console.WriteLine($"References[{m_refs.Count}]");

      for (int i = 0; i < Metadata.resolvedImages.Count; i++)
      {
        //if (Metadata.resolvedImages[i].Name != "Assembly-CSharp.dll")
        //    continue;

        ScanGameAssembly(Metadata.resolvedImages[i]);
      }

      ResolveTrustedRefs();
    }

    static ulong GetNextFunc(ref int curIdx, ulong higherThan = 0)
    {
      ulong candidate = 0;

      if (curIdx >= funcPtrs.Count)
        return UInt64.MaxValue;

      do
      {
        candidate = funcPtrs[curIdx];
        curIdx++;
        if (candidate > higherThan)
          return candidate;
      }
      while (curIdx < funcPtrs.Count);
      //funcPtrs[curIdx];
      return UInt64.MaxValue;
    }

    public static List<ulong> ScanRegionForReferences(ulong va, ulong len)
    {
      List<ulong> refs = new List<ulong>();
      // Store last 10 instructions
      List<Instruction> instructionHistory = new List<Instruction>();

      var codeReader = new ByteArrayCodeReader(m_peBytes, (int)Offset.FromVA(va), (int)len);
      var decoder = Decoder.Create(64, codeReader);
      decoder.IP = va;
      ulong EndRip = va + len;

      while (decoder.IP < EndRip)
      {
        // The method allocates an uninitialized element at the end of the list and
        // returns a reference to it which is initialized by Decode().
        ulong addr = decoder.IP;
        decoder.Decode(out var instruction);
        instructionHistory.Add(instruction);
        if (instructionHistory.Count > 10)
          instructionHistory.RemoveAt(0);
        if (instruction.Mnemonic == Mnemonic.Call)
        {
          if (instruction.Op0Kind == OpKind.Memory || instruction.Op0Kind == OpKind.Register)
            continue;

          ulong targetAddr = instruction.NearBranch64;

          if (targetAddr == Delegate_CombineMethodPtr)
          {
            if (instructionHistory[0].Mnemonic == Mnemonic.Mov && instructionHistory[0].Op0Register == Register.R8 && instructionHistory[0].MemoryBase == Register.RIP)
            {
              // TODO: see what can be done when instruction refers to register eg. RAX+10

              ulong delVA = instructionHistory[0].IPRelativeMemoryAddress;
              if (il2cpp.mapMethodPtrsByMetadataUsages.TryGetValue(delVA, out var val))
              {
                refs.Add(val);
              }
            }
          }
          else
          {
            if (instructionHistory.Count > 4 && instructionHistory[4].Mnemonic == Mnemonic.Mov && instructionHistory[4].Op0Register == Register.R8 && instructionHistory[4].MemoryBase == Register.RIP)
            {
              // TODO: see what can be done when instruction refers to register eg. RAX+10

              ulong methodVA = instructionHistory[4].IPRelativeMemoryAddress;

              if (il2cpp.mapMethodPtrsByMetadataUsages.TryGetValue(methodVA, out var val))
              {
                refs.Add(val);
              }

              //var offset = Offset.FromVA(va);
              //il2cppReader.stream.Position = (long)offset;
              //var MI_Addr = il2cppReader.reader.Read<ulong>();
            }
          }


          //if (!(targetAddr >= beginIP && targetAddr < EndRip) && !(targetAddr >= textBeginIP && targetAddr < textEndRip))
          //{
          //    Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind} {instruction.IsCallNear}");
          //    continue;
          //}
          //if (targetAddr == 0x00000000)
          //    Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind}");

          //if (!funcPtrs.Contains(targetAddr))
          {
            //Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind}");
            //funcPtrs.Add(targetAddr);
            refs.Add(targetAddr);
          }
          //m_refs.Add(addr, targetAddr);
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
          //if (!(targetAddr >= beginIP && targetAddr < endRip) && !(targetAddr >= textBeginIP && targetAddr < textEndRip))
          //{
          //    Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind} {instruction.IsCallNear}");
          //    continue;
          //}
          // TODO: ignore addresses outside of range
          //if (targetAddr == 0x169E4EA10)
          //    Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind}");

          //if (!funcPtrs.Contains(targetAddr))
          {
            //Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind} {instruction.OpCode.Code.ToString()}");
            //funcPtrs.Add(targetAddr);
            refs.Add(targetAddr);
          }
          //m_refs.Add(addr, targetAddr);
        }
        else if (instruction.Mnemonic == Mnemonic.Mov) // stringliterals
        {
          if (instruction.Op1Kind == OpKind.Memory && instruction.IsIPRelativeMemoryOperand)
          {
            var movTarget = instruction.IPRelativeMemoryAddress;

            if (il2cpp.mapStringLiteralPtrsByMetadataUsages.ContainsKey(movTarget))
            {
              refs.Add(movTarget);
            }
          }
        }
      }

      return refs;
    }

    public static List<string> ScanRegionForStringLiterals(ulong va, ulong len)
    {
      List<string> literals = new List<string>();
      // Store last 10 instructions
      List<Instruction> instructionHistory = new List<Instruction>();

      var codeReader = new ByteArrayCodeReader(m_peBytes, (int)Offset.FromVA(va), (int)len);
      var decoder = Decoder.Create(64, codeReader);
      decoder.IP = va;
      ulong EndRip = va + len;

      while (decoder.IP < EndRip)
      {
        // The method allocates an uninitialized element at the end of the list and
        // returns a reference to it which is initialized by Decode().
        ulong addr = decoder.IP;
        decoder.Decode(out var instruction);

        if (instruction.Mnemonic != Mnemonic.Mov) // stringliterals
          continue;

        if (instruction.Op1Kind != OpKind.Memory || !instruction.IsIPRelativeMemoryOperand)
          continue;
        var movTarget = instruction.IPRelativeMemoryAddress;

        if (il2cpp.mapStringLiteralPtrsByMetadataUsages.ContainsKey(movTarget))
          literals.Add(MetadataReader.GetStringLiteralFromIndex(il2cpp.mapStringLiteralPtrsByMetadataUsages[movTarget]));
      }

      return literals;
    }



    static void ScanTextSection()
    {
      if (!PortableExecutable.m_mapSections.TryGetValue("il2cpp", out var il2cppSection))
        return;

      if (!PortableExecutable.m_mapSections.TryGetValue(".text", out var textSection))
        return;

      // Store last 10 instructions
      List<Instruction> instructionHistory = new List<Instruction>();

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
        instructionHistory.Add(instruction);
        if (instructionHistory.Count > 10)
          instructionHistory.RemoveAt(0);
        if (instruction.Mnemonic == Mnemonic.Call)
        {
          if (instruction.Op0Kind == OpKind.Memory || instruction.Op0Kind == OpKind.Register)
            continue;

          ulong targetAddr = instruction.NearBranch64;

          if (targetAddr == Delegate_CombineMethodPtr)
          {

          }
          //
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
          if (instruction.OpCode.Code == Code.Jmp_rel8_64)
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

      // Store last 10 instructions
      List<Instruction> instructionHistory = new List<Instruction>();

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
        instructionHistory.Add(instruction);
        if (instructionHistory.Count > 10)
          instructionHistory.RemoveAt(0);
        if (instruction.Mnemonic == Mnemonic.Call)
        {
          if (instruction.Op0Kind != OpKind.FarBranch16 &&
              instruction.Op0Kind != OpKind.FarBranch32 &&
              instruction.Op0Kind != OpKind.NearBranch16 &&
              instruction.Op0Kind != OpKind.NearBranch32 &&
              instruction.Op0Kind != OpKind.NearBranch64)
            continue;

          if (instruction.Op0Kind == OpKind.Memory || instruction.Op0Kind == OpKind.Register)
            continue;

          ulong targetAddr = instruction.NearBranch64;

          if (targetAddr == Delegate_CombineMethodPtr)
          {
            if (instructionHistory[0].Mnemonic == Mnemonic.Mov && instructionHistory[0].Op0Register == Register.R8 && instructionHistory[0].MemoryBase == Register.RIP)
            {
              // TODO: see what can be done when instruction refers to register eg. RAX+10

              ulong va = instructionHistory[0].IPRelativeMemoryAddress;
              if (il2cpp.mapMethodPtrsByMetadataUsages.TryGetValue(va, out var val))
              {
                // TODO: maybe add as Delegate usage idk
                if (!m_refs.ContainsKey(instructionHistory[0].IP))
                {
                  m_refs.Add(instructionHistory[0].IP, val);
                  //funcPtrs.Add(val);
                }
              }

              //var offset = Offset.FromVA(va);
              //il2cppReader.stream.Position = (long)offset;
              //var MI_Addr = il2cppReader.reader.Read<ulong>();
            }
          }
          else
          {
            if (instructionHistory.Count > 4 && instructionHistory[4].Mnemonic == Mnemonic.Mov && instructionHistory[4].Op0Register == Register.R8 && instructionHistory[4].MemoryBase == Register.RIP)
            {
              // TODO: see what can be done when instruction refers to register eg. RAX+10

              ulong va = instructionHistory[4].IPRelativeMemoryAddress;

              if (il2cpp.mapMethodPtrsByMetadataUsages.TryGetValue(va, out var val))
              {
                // TODO: maybe add as Delegate usage idk
                m_refs.Add(instructionHistory[4].IP, val);
                //funcPtrs.Add(val);
              }

              //var offset = Offset.FromVA(va);
              //il2cppReader.stream.Position = (long)offset;
              //var MI_Addr = il2cppReader.reader.Read<ulong>();
            }
          }


          if (!(targetAddr >= beginIP && targetAddr < endRip) && !(targetAddr >= textBeginIP && targetAddr < textEndRip))
          {
            Console.WriteLine($"[0x{addr:X8}] {instruction.ToString()} {instruction.Op0Kind} {instruction.IsCallNear}");
            continue;
          }

          //funcPtrs.Add(targetAddr);
          m_refs.Add(addr, targetAddr);
        }
        else if (instruction.Mnemonic == Mnemonic.Jmp)
        {
          if (instruction.Op0Kind != OpKind.FarBranch16 &&
              instruction.Op0Kind != OpKind.FarBranch32 &&
              instruction.Op0Kind != OpKind.NearBranch16 &&
              instruction.Op0Kind != OpKind.NearBranch32 &&
              instruction.Op0Kind != OpKind.NearBranch64)
            continue;

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

          //funcPtrs.Add(targetAddr);
          m_refs.Add(addr, targetAddr);
        }
        else if (instruction.Mnemonic == Mnemonic.Mov) // stringliterals
        {
          if (instruction.Op1Kind != OpKind.Memory || !instruction.IsIPRelativeMemoryOperand)
            continue;

          var movTarget = instruction.IPRelativeMemoryAddress;

          if (!il2cpp.mapStringLiteralPtrsByMetadataUsages.ContainsKey(movTarget))
            continue;

          m_StringLiteralsPointers.Add(addr, il2cpp.mapStringLiteralPtrsByMetadataUsages[movTarget]);
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
          var resolvedMethod = resolvedClass.miMethods[i];

          var methodPtr = resolvedMethod.methodPtr;

          if (methodPtr == 0)
            continue;

          if (!resolvedMethod.isMangled || resolvedMethod.isReferenced)
          {
            if (!mapTrustedMethods.ContainsKey(methodPtr))
            {
              mapTrustedMethods.Add(methodPtr, new List<ResolvedMethod>());
            }
            mapTrustedMethods[methodPtr].Add(resolvedMethod);
          }
          else
          {
            if (!mapMangledMethods.ContainsKey(methodPtr))
            {
              mapMangledMethods.Add(methodPtr, new List<ResolvedMethod>());
            }
            mapMangledMethods[methodPtr].Add(resolvedMethod);
          }
        }
      }
      else if (resolvedType is ResolvedStruct)
      {
        ResolvedStruct resolvedStruct = resolvedType as ResolvedStruct;
        for (int i = 0; i < resolvedStruct.miMethods.Count; i++)
        {
          var resolvedMethod = resolvedStruct.miMethods[i];

          var methodPtr = resolvedMethod.methodPtr;

          if (methodPtr == 0)
            continue;

          if (!resolvedMethod.isMangled || resolvedMethod.isReferenced)
          {
            if (!mapTrustedMethods.ContainsKey(methodPtr))
            {
              mapTrustedMethods.Add(methodPtr, new List<ResolvedMethod>());
            }
            mapTrustedMethods[methodPtr].Add(resolvedMethod);
          }
          else
          {
            if (!mapMangledMethods.ContainsKey(methodPtr))
            {
              mapMangledMethods.Add(methodPtr, new List<ResolvedMethod>());
            }
            mapMangledMethods[methodPtr].Add(resolvedMethod);
          }
        }
      }

      for (int i = 0; i < resolvedType.nestedTypes.Count; i++)
      {
        AddRefsToResolve(resolvedType.nestedTypes[i]);
      }
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
          bool success = m_mapFunctionReferences.TryGetValue(trustedMethods[i].Key, out var pointers);

          if (!success)
            continue;
          //var pointers = m_mapFunctionReferences[];

          for (int k = 0; k < pointers.Count; k++)
          {
            if (!mapMangledMethods.TryGetValue(pointers[k], out var methods))
              continue;

            methods.ForEach((m) =>
            {
              if (!mapOverrideLinks.TryGetValue(m, out var virtualMethod))
              {
                m.isReferenced = true;
                return;
              }

              virtualMethod.isReferenced = true;
              if (mapMangledMethods.ContainsKey(virtualMethod.methodPtr))
              {
                mapTrustedMethods.Add(virtualMethod.methodPtr, mapMangledMethods[virtualMethod.methodPtr]);
                mapMangledMethods.Remove(virtualMethod.methodPtr);
              }

              for (int j = 0; j < mapVirtualLinks[virtualMethod].Count; j++)
              {
                mapVirtualLinks[virtualMethod][j].isReferenced = true;
                if (mapMangledMethods.ContainsKey(mapVirtualLinks[virtualMethod][j].methodPtr))
                {
                  mapTrustedMethods.Add(mapVirtualLinks[virtualMethod][j].methodPtr, mapMangledMethods[mapVirtualLinks[virtualMethod][j].methodPtr]);
                  mapMangledMethods.Remove(mapVirtualLinks[virtualMethod][j].methodPtr);
                }
              }
            });

            if (mapMangledMethods.ContainsKey(pointers[k]))
            {
              mapTrustedMethods.Add(pointers[k], methods);
              mapMangledMethods.Remove(pointers[k]);
            }
          }
        }

        //for (int i = 0; i < playerClass.miMethods.Count; i++)
        //{
        //    if (!playerClass.miMethods[i].isMangled || (playerClass.miMethods[i].isMangled && playerClass.miMethods[i].isReferenced))
        //        Console.WriteLine($"{playerClass.GetFullName()} method[{playerClass.miMethods[i].Name}]({playerClass.miMethods[i].methodIndex - playerClass.typeDef.methodStart }) is referenced");
        //    else
        //        Console.WriteLine($"{playerClass.GetFullName()} method[{playerClass.miMethods[i].Name}]({playerClass.miMethods[i].methodIndex - playerClass.typeDef.methodStart }) is not referenced");
        //}
      }
    }

    public static Dictionary<ResolvedMethod, List<ResolvedMethod>> mapVirtualLinks = new Dictionary<ResolvedMethod, List<ResolvedMethod>>();
    public static Dictionary<ResolvedMethod, ResolvedMethod> mapOverrideLinks = new Dictionary<ResolvedMethod, ResolvedMethod>();

    public static void LinkOverride(ResolvedMethod virtualMethod, ResolvedMethod overrideMethod)
    {
      if (!mapVirtualLinks.TryGetValue(virtualMethod, out var list))
      {
        list = new List<ResolvedMethod>();
        mapVirtualLinks.Add(virtualMethod, list);
      }

      list.Add(overrideMethod);
      mapOverrideLinks.Add(overrideMethod, virtualMethod);
    }
  }
}
