using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using int32_t = System.Int32;
using TypeIndex = System.Int32;
using TypeDefinitionIndex = System.Int32;
using EncodedMethodIndex = System.UInt32;

namespace il2cpp_sdk_generator
{
  public class MetadataReader
  {
    private static BinaryReader reader;
    public static MemoryStream stream;

    public MetadataReader(MemoryStream memoryStream)
    {
      stream = memoryStream;
      stream.Position = 0;
      reader = new BinaryReader(memoryStream, Encoding.UTF8);
    }

    // Here we just read whole Metadata file
    public void Read()
    {
      Metadata.header = reader.Read<Il2CppGlobalMetadataHeader>();
      Metadata.header.DumpToConsole();

      stream.Position = Metadata.header.stringLiteralOffset;
      Metadata.stringLiterals = reader.ReadArray<Il2CppStringLiteral>(Metadata.header.stringLiteralCount / typeof(Il2CppStringLiteral).GetSizeOf());
      // For now skip strings
      stream.Position = Metadata.header.eventsOffset;
      Metadata.eventDefinitions = reader.ReadArray<Il2CppEventDefinition>(Metadata.header.eventsCount / typeof(Il2CppEventDefinition).GetSizeOf());
      stream.Position = Metadata.header.propertiesOffset;
      Metadata.propertyDefinitions = reader.ReadArray<Il2CppPropertyDefinition>(Metadata.header.propertiesCount / typeof(Il2CppPropertyDefinition).GetSizeOf());
      stream.Position = Metadata.header.methodsOffset;
      Metadata.methodDefinitions = reader.ReadArray<Il2CppMethodDefinition>(Metadata.header.methodsCount / typeof(Il2CppMethodDefinition).GetSizeOf());
      stream.Position = Metadata.header.parameterDefaultValuesOffset;
      Metadata.parameterDefaultValues = reader.ReadArray<Il2CppParameterDefaultValue>(Metadata.header.parameterDefaultValuesCount / typeof(Il2CppParameterDefaultValue).GetSizeOf());
      stream.Position = Metadata.header.fieldDefaultValuesOffset;
      Metadata.fieldDefaultValues = reader.ReadArray<Il2CppFieldDefaultValue>(Metadata.header.fieldDefaultValuesCount / typeof(Il2CppFieldDefaultValue).GetSizeOf());
      // fieldAndParameterDefaultValueData
      // Il2CppFieldMarshaledSize
      stream.Position = Metadata.header.parametersOffset;
      Metadata.parameterDefinitions = reader.ReadArray<Il2CppParameterDefinition>(Metadata.header.parametersCount / typeof(Il2CppParameterDefinition).GetSizeOf());
      stream.Position = Metadata.header.fieldsOffset;
      Metadata.fieldDefinitions = reader.ReadArray<Il2CppFieldDefinition>(Metadata.header.fieldsCount / typeof(Il2CppFieldDefinition).GetSizeOf());
      stream.Position = Metadata.header.genericParametersOffset;
      Metadata.genericParameters = reader.ReadArray<Il2CppGenericParameter>(Metadata.header.genericParametersCount / typeof(Il2CppGenericParameter).GetSizeOf());
      stream.Position = Metadata.header.genericParameterConstraintsOffset;
      Metadata.genericParameterConstraintsIndices = reader.ReadArray<TypeIndex>(Metadata.header.genericParameterConstraintsCount / typeof(TypeIndex).GetSizeOf());
      stream.Position = Metadata.header.genericContainersOffset;
      Metadata.genericContainers = reader.ReadArray<Il2CppGenericContainer>(Metadata.header.genericContainersCount / typeof(Il2CppGenericContainer).GetSizeOf());
      stream.Position = Metadata.header.nestedTypesOffset;
      Metadata.nestedTypeIndices = reader.ReadArray<TypeDefinitionIndex>(Metadata.header.nestedTypesCount / typeof(TypeDefinitionIndex).GetSizeOf());
      stream.Position = Metadata.header.interfacesOffset;
      Metadata.interfaceIndices = reader.ReadArray<TypeIndex>(Metadata.header.interfacesCount / typeof(TypeIndex).GetSizeOf());
      stream.Position = Metadata.header.vtableMethodsOffset;
      Metadata.vtableMethodIndices = reader.ReadArray<EncodedMethodIndex>(Metadata.header.vtableMethodsCount / typeof(EncodedMethodIndex).GetSizeOf());
      stream.Position = Metadata.header.interfaceOffsetsOffset;
      Metadata.interfaceOffsetPairs = reader.ReadArray<Il2CppInterfaceOffsetPair>(Metadata.header.interfaceOffsetsCount / typeof(Il2CppInterfaceOffsetPair).GetSizeOf());
      stream.Position = Metadata.header.typeDefinitionsOffset;
      Metadata.typeDefinitions = reader.ReadArray<Il2CppTypeDefinition>(Metadata.header.typeDefinitionsCount / typeof(Il2CppTypeDefinition).GetSizeOf());
      //stream.Position = Metadata.header.rgctxEntriesOffset;
      //Metadata.rgctxEntries = reader.ReadArray<Il2CppRGCTXDefinition>(Metadata.header.rgctxEntriesCount / typeof(Il2CppRGCTXDefinition).GetSizeOf());
      stream.Position = Metadata.header.imagesOffset;
      Metadata.imageDefinitions = reader.ReadArray<Il2CppImageDefinition>(Metadata.header.imagesCount / typeof(Il2CppImageDefinition).GetSizeOf());
      stream.Position = Metadata.header.assembliesOffset;
      Metadata.assemblies = reader.ReadArray<Il2CppAssemblyDefinition>(Metadata.header.assembliesCount / typeof(Il2CppAssemblyDefinition).GetSizeOf());
      stream.Position = Metadata.header.metadataUsageListsOffset;
      Metadata.metadataUsageLists = reader.ReadArray<Il2CppMetadataUsageList>(Metadata.header.metadataUsageListsCount / typeof(Il2CppMetadataUsageList).GetSizeOf());
      stream.Position = Metadata.header.metadataUsagePairsOffset;
      Metadata.metadataUsagePairs = reader.ReadArray<Il2CppMetadataUsagePair>(Metadata.header.metadataUsagePairsCount / typeof(Il2CppMetadataUsagePair).GetSizeOf());
      stream.Position = Metadata.header.fieldRefsOffset;
      Metadata.fieldReferences = reader.ReadArray<Il2CppFieldRef>(Metadata.header.fieldRefsCount / typeof(Il2CppFieldRef).GetSizeOf());
      stream.Position = Metadata.header.referencedAssembliesOffset;
      Metadata.referencedAssemblies = reader.ReadArray<int32_t>(Metadata.header.referencedAssembliesCount / typeof(int32_t).GetSizeOf());
      stream.Position = Metadata.header.attributesInfoOffset;
      Metadata.attributeTypeRanges = reader.ReadArray<Il2CppCustomAttributeTypeRange>(Metadata.header.attributesInfoCount / typeof(Il2CppCustomAttributeTypeRange).GetSizeOf());
      stream.Position = Metadata.header.attributeTypesOffset;
      Metadata.attributeTypes = reader.ReadArray<TypeIndex>(Metadata.header.attributeTypesCount / typeof(TypeIndex).GetSizeOf());
      stream.Position = Metadata.header.unresolvedVirtualCallParameterRangesOffset;
      Metadata.unresolvedVirtualCallParameterTypes = reader.ReadArray<TypeIndex>(Metadata.header.unresolvedVirtualCallParameterRangesCount / typeof(TypeIndex).GetSizeOf());
      // unresolvedVirtualCallParameterRanges
      // windowsRuntimeTypeNames // Il2CppWindowsRuntimeTypeNamePair
      // exportedTypeDefinitions // TypeDefinitionIndex
    }

    // Here we process whatever can be processed for later use
    public void Process()
    {
      ResolveTypes();

      for (int i = 0; i < Metadata.fieldDefaultValues.Length; i++)
      {
        Metadata.mapFieldDefValues.Add(Metadata.fieldDefaultValues[i].fieldIndex, Metadata.fieldDefaultValues[i]);
      }

      //Metadata.mapParameterDefValues = Metadata.parameterDefaultValues.ToDictionary(x => x.parameterIndex);
      for (int i = 0; i < Metadata.parameterDefaultValues.Length; i++)
      {
        Metadata.mapParameterDefValues.Add(Metadata.parameterDefaultValues[i].parameterIndex, Metadata.parameterDefaultValues[i]);
      }

      foreach (var image in Metadata.imageDefinitions)
      {
        var resolvedImage = ResolveImage(image);
        Metadata.resolvedImages.Add(resolvedImage);
      }
      Console.WriteLine();

      // MetadataRegistration.metadataUsagesCount is actually never used so we get real value ourselves
      il2cpp.realMetadataUsagesCount = 0;
      for (int i = 0; i < Metadata.metadataUsagePairs.Length; i++)
      {
        var usagePair = Metadata.metadataUsagePairs[i];
        if (il2cpp.realMetadataUsagesCount < usagePair.destinationIndex)
          il2cpp.realMetadataUsagesCount = usagePair.destinationIndex;

        if (usagePair.EncodedIndexType == Il2CppMetadataUsage.kIl2CppMetadataUsageMethodDef)
        {
          if (Metadata.usageMethods.ContainsKey(usagePair.destinationIndex))
            continue;

          Metadata.usageMethods.Add(usagePair.destinationIndex, usagePair.DecodedMethodIndex);
        }
        else if (usagePair.EncodedIndexType == Il2CppMetadataUsage.kIl2CppMetadataUsageStringLiteral)
        {
          if (Metadata.usageStringLiterals.ContainsKey(usagePair.destinationIndex))
            continue;

          Metadata.usageStringLiterals.Add(usagePair.destinationIndex, usagePair.DecodedMethodIndex);
        }
      }
    }

    public static string GetStringLiteralFromIndex(UInt32 idx)
    {
      var stringLiteral = Metadata.stringLiterals[idx];

      string result = "";
      lock (stream)
      {
        stream.Position = Metadata.header.stringLiteralDataOffset + stringLiteral.dataIndex;
        result = reader.ReadString(stringLiteral.length);
      }

      return result;
    }

    static Dictionary<Int32, string> mapIndexStringCache = new Dictionary<Int32, string>();

    public static string GetString(Int32 idx)
    {
      if (mapIndexStringCache.TryGetValue(idx, out string result))
        return result;

      stream.Position = Metadata.header.stringOffset + idx;
      result = reader.ReadNullTerminatedString();
      mapIndexStringCache.Add(idx, result);
      return result;
    }

    public ResolvedImage ResolveImage(Il2CppImageDefinition image)
    {
      ResolvedImage resolvedImage = new ResolvedImage();
      resolvedImage.Name = GetString(image.nameIndex);

      for (int i = image.typeStart; i < image.typeStart + image.typeCount; i++)
      {
        var resolvedType = Metadata.resolvedTypes[i];
        resolvedType.resolvedImage = resolvedImage;
        if (resolvedType.isNested)
          continue;

        if (!resolvedImage.Namespaces.TryGetValue(resolvedType.Namespace, out var resolvedNamespace))
        {
          resolvedNamespace = new ResolvedNamespace();
          resolvedNamespace.Name = resolvedType.Namespace;
          resolvedImage.Namespaces.Add(resolvedType.Namespace, resolvedNamespace);
        }

        // TODO: depending on type put to correct var. enums, classes/structs?
        if (resolvedType is ResolvedEnum)
          resolvedNamespace.Enums.Add(resolvedType);
        else
          resolvedNamespace.Types.Add(resolvedType);

        //if (resolvedImage.Name == "Assembly-CSharp.dll")
        //{
        //    resolvedType.DumpToConsole();
        //    foreach(var nestedType in resolvedType.nestedTypes)
        //    {
        //        nestedType.DumpToConsole(2);
        //    }
        //}
      }

      //Console.WriteLine($"Image: {resolvedImage.Name}");
      //Console.WriteLine($"Namespaces: {resolvedImage.Namespaces.Count}");
      //foreach (var pair in resolvedImage.Namespaces)
      //{
      //    Console.WriteLine($" {pair.Value.Name}: {pair.Value.Types.Count}");
      //    for (int i = 0; i < pair.Value.Types.Count; i++)
      //    {
      //        Console.WriteLine($"  {pair.Value.Types[i].Name} 0x{pair.Value.Types[i].typeDef.flags:X8}");
      //    }
      //    //resolvedImage.Namespaces.
      //}


      return resolvedImage;
    }

    public void ResolveTypes()
    {
      Metadata.resolvedTypes = new ResolvedType[Metadata.typeDefinitions.Length];
      for (int i = 0; i < Metadata.typeDefinitions.Length; i++)
      {
        ResolveType(i);
        //if (Metadata.typeDefinitions[i].genericContainerIndex < 0)
        //    continue;
      }
    }


    const uint ClassSemanticsMask = 32;

    internal static Dictionary<Int32, ResolvedGenericClass> mapResolvedGenericClasses = new Dictionary<Int32, ResolvedGenericClass>();
    static List<ResolvedGenericClass> resolvedGenericClasses = new List<ResolvedGenericClass>();

    public static ResolvedType ResolveType(Int32 typeIdx)
    {
      if (Metadata.resolvedTypes[typeIdx] != null)
        return Metadata.resolvedTypes[typeIdx];

      var typeDef = Metadata.typeDefinitions[typeIdx];

      ResolvedType resolvedType = null;

      // TODO: Resolved class, struct, enum, interface
      //resolvedType = new ResolvedType(typeDef, typeIdx);

      if (typeDef.isEnum)
      {
        resolvedType = new ResolvedEnum(typeDef, typeIdx);
      }
      else
      {
        uint flag = typeDef.flags & ClassSemanticsMask;
        uint Interface = 32;
        if (flag == Interface)
        {
          resolvedType = new ResolvedInterface(typeDef, typeIdx);
        }
        else
        {
          if (typeDef.isValueType)
          {
            resolvedType = new ResolvedStruct(typeDef, typeIdx);
            if (typeDef.genericContainerIndex >= 0)
            {

            }
          }
          else
          {
            if (typeDef.genericContainerIndex >= 0)
            {
              resolvedType = new ResolvedGenericClass(typeDef, typeIdx);
              mapResolvedGenericClasses.Add(typeIdx, (ResolvedGenericClass)resolvedType);
              resolvedGenericClasses.Add((ResolvedGenericClass)resolvedType);
            }
            else
            {
              resolvedType = new ResolvedClass(typeDef, typeIdx);
            }
          }

        }
        //if(typeDef.genericContainerIndex >= 0)
        //{
        //    mapResolvedGenericClasses.Add(typeIdx, new ResolvedGenericClass(typeDef, typeIdx));
        //    resolvedGenericClasses.Add(new ResolvedGenericClass(typeDef, typeIdx));
        //}
        //methodDef.genericContainerIndex >= 0
      }

      for (int i = 0; i < typeDef.nested_type_count; i++)
      {
        Int32 nestedTypeIndex = Metadata.nestedTypeIndices[typeDef.nestedTypesStart + i];
        ResolvedType nestedType = ResolveType(nestedTypeIndex);
        if (resolvedType.Name == "TaskFactory`1" && nestedType.Name == "FromAsyncTrimPromise`1")
          continue;
        if (resolvedType.Name == "Array" && (nestedType.Name == "EmptyInternalEnumerator`1" || nestedType.Name == "InternalEnumerator`1"))
          continue;
        nestedType.declaringType = resolvedType;
        resolvedType.nestedTypes.Add(nestedType);
      }

      Metadata.resolvedTypes[typeIdx] = resolvedType;
      if (resolvedType.isNested)
        Metadata.nestedTypes.Add(resolvedType);

      return resolvedType;
    }

    // Do it like TryGetValue in Dictionary
    public static bool GetDefaultFieldValue(Int32 fieldIndex, out object val)
    {
      val = null;
      if (!Metadata.mapFieldDefValues.TryGetValue(fieldIndex, out var il2CppFieldDefaultValue))
        return false;

      // !!!!!!!!! requires il2cpp to be processed
      var type = il2cpp.types[il2CppFieldDefaultValue.typeIndex];
      stream.Position = il2CppFieldDefaultValue.dataIndex + Metadata.header.fieldAndParameterDefaultValueDataOffset;
      switch (type.type)
      {
        case Il2CppTypeEnum.IL2CPP_TYPE_U1:
          {
            val = reader.Read<byte>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_U2:
          {
            val = reader.Read<UInt16>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_U4:
          {
            val = reader.Read<UInt32>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_U8:
          {
            val = reader.Read<UInt64>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_I1:
          {
            val = reader.Read<SByte>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_I2:
          {
            val = reader.Read<Int16>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_I4:
          {
            val = reader.Read<Int32>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_I8:
          {
            val = reader.Read<Int64>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_BOOLEAN:
          {
            val = reader.Read<Boolean>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_STRING:
          {
            int strlen = reader.Read<Int32>();
            val = Encoding.UTF8.GetString(reader.ReadBytes(strlen));
            Console.WriteLine($"GetDefaultFieldValue: string => {val}");
            break;
          }
        default:
          {
            Console.WriteLine($"GetDefaultFieldValue: unhandled type => {type.type}");
            return false;
          }
      }

      return true;
    }

    // !!!!!!!!! requires il2cpp to be processed
    public static void ProcessDefaultParamValues()
    {
      for (int i = 0; i < Metadata.header.parameterDefaultValuesCount; i++)
      {
        if (i == 103846)
        {

        }
        object val = null;
        if (!Metadata.mapParameterDefValues.TryGetValue(i, out var il2CppParameterDefaultValue))
        {
          Metadata.mapParamDefValues.Add(i, val);
          continue;
        }

        //Il2CppParameterDefaultValue il2CppParameterDefaultValue = Metadata.parameterDefaultValues[i];

        var type = il2cpp.types[il2CppParameterDefaultValue.typeIndex];



        if (il2CppParameterDefaultValue.dataIndex == -1)
        {
          // TODO: Store null value
          //val = null;
          switch (type.type)
          {
            case Il2CppTypeEnum.IL2CPP_TYPE_U1:
            case Il2CppTypeEnum.IL2CPP_TYPE_U2:
            case Il2CppTypeEnum.IL2CPP_TYPE_U4:
            case Il2CppTypeEnum.IL2CPP_TYPE_U8:
            case Il2CppTypeEnum.IL2CPP_TYPE_I1:
            case Il2CppTypeEnum.IL2CPP_TYPE_I2:
            case Il2CppTypeEnum.IL2CPP_TYPE_I4:
            case Il2CppTypeEnum.IL2CPP_TYPE_I8:
            case Il2CppTypeEnum.IL2CPP_TYPE_VALUETYPE:
              {
                val = 0;
                break;
              }
            case Il2CppTypeEnum.IL2CPP_TYPE_BOOLEAN:
              {
                val = false;
                break;
              }
          }

          if (type.type == Il2CppTypeEnum.IL2CPP_TYPE_I4)
          {
            val = 0;
          }
          Metadata.mapParamDefValues.Add(i, val);
          continue;
          //return true;
        }

        stream.Position = il2CppParameterDefaultValue.dataIndex + Metadata.header.fieldAndParameterDefaultValueDataOffset;
        switch (type.type)
        {
          case Il2CppTypeEnum.IL2CPP_TYPE_U1:
            {
              // TODO: Store value
              val = reader.Read<byte>();
              break;
            }
          case Il2CppTypeEnum.IL2CPP_TYPE_U2:
            {
              // TODO: Store value
              val = reader.Read<UInt16>();
              break;
            }
          case Il2CppTypeEnum.IL2CPP_TYPE_U4:
            {
              // TODO: Store value
              val = reader.Read<UInt32>();
              break;
            }
          case Il2CppTypeEnum.IL2CPP_TYPE_U8:
            {
              // TODO: Store value
              val = reader.Read<UInt64>();
              break;
            }
          case Il2CppTypeEnum.IL2CPP_TYPE_I1:
            {
              // TODO: Store value
              val = reader.Read<SByte>();
              break;
            }
          case Il2CppTypeEnum.IL2CPP_TYPE_I2:
            {
              // TODO: Store value
              val = reader.Read<Int16>();
              break;
            }
          case Il2CppTypeEnum.IL2CPP_TYPE_I4:
            {
              // TODO: Store value
              val = reader.Read<Int32>();
              break;
            }
          case Il2CppTypeEnum.IL2CPP_TYPE_I8:
            {
              // TODO: Store value
              val = reader.Read<Int64>();
              break;
            }
          case Il2CppTypeEnum.IL2CPP_TYPE_BOOLEAN:
            {
              // TODO: Store value
              val = reader.Read<Boolean>();
              break;
            }
          case Il2CppTypeEnum.IL2CPP_TYPE_STRING:
            {
              // TODO: Store value
              int strlen = reader.Read<Int32>();
              val = Encoding.UTF8.GetString(reader.ReadBytes(strlen));
              //Console.WriteLine($"GetDefaultParameterValue: string => {val}");
              break;
            }
          case Il2CppTypeEnum.IL2CPP_TYPE_R4:
            {
              // TODO: Store value
              val = reader.Read<float>();
              break;
            }
          case Il2CppTypeEnum.IL2CPP_TYPE_R8:
            {
              // TODO: Store value
              val = reader.Read<double>();
              break;
            }
          case Il2CppTypeEnum.IL2CPP_TYPE_CHAR:
            {
              // TODO: Store value
              val = BitConverter.ToChar(reader.ReadBytes(2), 0);
              break;
            }
          default:
            {
              Console.WriteLine($"GetDefaultParameterValue: unhandled type => {type.type}");
              //return false;
              break;
            }
        }

        Metadata.mapParamDefValues.Add(i, val);
        //Metadata.mapParameterDefValues.Add(i, )
      }


    }

    // Do it like TryGetValue in Dictionary
    public static bool GetDefaultParameterValue(Int32 parameterIndex, out object val)
    {
      val = null;

      if (!Metadata.mapParamDefValues.TryGetValue(parameterIndex, out var res))
        return false;

      val = res;
      return true;


      if (!Metadata.mapParameterDefValues.TryGetValue(parameterIndex, out var il2CppParameterDefaultValue))
        return false;



      // !!!!!!!!! requires il2cpp to be processed
      var type = il2cpp.types[il2CppParameterDefaultValue.typeIndex];
      if (il2CppParameterDefaultValue.dataIndex == -1)
      {
        val = null;
        return true;
      }

      stream.Position = il2CppParameterDefaultValue.dataIndex + Metadata.header.fieldAndParameterDefaultValueDataOffset;
      switch (type.type)
      {
        case Il2CppTypeEnum.IL2CPP_TYPE_U1:
          {
            val = reader.Read<byte>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_U2:
          {
            val = reader.Read<UInt16>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_U4:
          {
            val = reader.Read<UInt32>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_U8:
          {
            val = reader.Read<UInt64>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_I1:
          {
            val = reader.Read<SByte>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_I2:
          {
            val = reader.Read<Int16>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_I4:
          {
            val = reader.Read<Int32>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_I8:
          {
            val = reader.Read<Int64>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_BOOLEAN:
          {
            val = reader.Read<Boolean>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_STRING:
          {
            int strlen = reader.Read<Int32>();
            val = Encoding.UTF8.GetString(reader.ReadBytes(strlen));
            Console.WriteLine($"GetDefaultParameterValue: string => {val}");
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_R4:
          {
            val = reader.Read<float>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_R8:
          {
            val = reader.Read<double>();
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_CHAR:
          {
            val = BitConverter.ToChar(reader.ReadBytes(2), 0);
            break;
          }
        default:
          {
            Console.WriteLine($"GetDefaultParameterValue: unhandled type => {type.type}");
            return false;
          }
      }

      return true;
    }

    public static Dictionary<Il2CppType, string> mapTypeStringCache = new Dictionary<Il2CppType, string>();
    public static Dictionary<Il2CppType, string> mapSimpleTypeStringCache = new Dictionary<Il2CppType, string>();

    public static string GetTypeString(Il2CppType type, bool useCache = true)
    {
      if (useCache && mapTypeStringCache.TryGetValue(type, out string result))
        return result;

      switch (type.type)
      {
        case Il2CppTypeEnum.IL2CPP_TYPE_VOID:
          result = "void";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_BOOLEAN:
          result = "bool";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_CHAR:
          result = "Il2CppChar";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_I1: // SBYTE
          result = "int8_t";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_U1:
          result = "uint8_t";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_I2:
          result = "int16_t";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_U2:
          result = "uint16_t";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_I4:
          result = "int32_t";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_U4:
          result = "uint32_t";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_I8:
          result = "int64_t";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_U8:
          result = "uint64_t";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_R4:
          result = "float";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_R8:
          result = "double";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_STRING:
          result = "Il2CppString*";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_VALUETYPE:
          {
            result = Metadata.resolvedTypes[type.data.klassIndex].GetFullName();
            if (TypeBlacklist.isBlacklisted(result, Metadata.resolvedTypes[type.data.klassIndex].isMangled) && !(Metadata.resolvedTypes[type.data.klassIndex] is ResolvedEnum))
            {
              result = $"Il2CppStruct /*{result}*/";
            }
          }
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_CLASS:
          {
            result = $"{Metadata.resolvedTypes[type.data.klassIndex].GetFullName()}";
            if (TypeBlacklist.isBlacklisted(result, Metadata.resolvedTypes[type.data.klassIndex].isMangled))
            {
              result = $"Il2CppObject /*{result}*/";
            }
            result += "*";
          }
          break;
        // TODO: confirm that's actually equivalent
        case Il2CppTypeEnum.IL2CPP_TYPE_I:
        case Il2CppTypeEnum.IL2CPP_TYPE_U:
          result = "void*";
          break;
        // TODO: confirm that's actually equivalent or just Il2CppObject*
        case Il2CppTypeEnum.IL2CPP_TYPE_OBJECT:
          result = "Il2CppBoxedObject*";
          break;
        // TODO: figure how the fuck to use c# multidimensional arrays as c++
        case Il2CppTypeEnum.IL2CPP_TYPE_ARRAY:
          {
            Il2CppArrayType il2CppArrayType = il2cppReader.GetIl2CppArrayType(type.data.arrayPtr);
            result = $"::Array<{GetTypeString(il2cppReader.GetIl2CppType(il2CppArrayType.etypePtr))}>*";
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_SZARRAY:
          result = $"::Array<{GetTypeString(il2cppReader.GetIl2CppType(type.data.typePtr))}>*";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_PTR:
          result = $"{GetTypeString(il2cppReader.GetIl2CppType(type.data.typePtr))}*";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_GENERICINST:
          {
            // TODO: Change to generated structs when ready
            Resolvedil2CppGenericClass genericClass = il2cppReader.GetIl2CppGenericClass(type.data.generic_classPtr);
            //Il2CppGenericClass generic_class = il2cppReader.GetIl2CppGenericClass(type.data.generic_classPtr);
            string test = Metadata.resolvedTypes[genericClass.genericClass.typeDefinitionIndex].ReturnTypeString();
            //string.Format()
            string typeStr = $"{Metadata.resolvedTypes[genericClass.genericClass.typeDefinitionIndex].GetFullName()}";
            // For whatever reason generic type names end with ` and digit (eg. List`1)
            if (typeStr.Contains('`'))
            {
              int idx = typeStr.IndexOf('`');
              typeStr = typeStr.Remove(idx, typeStr.Length - idx);
            }

            typeStr += "<";

            string[] genericParams = new string[genericClass.classParameters.Count];
            if (genericClass.classParameters != null)
            {
              for (int i = 0; i < genericClass.classParameters.Count; i++)
              {
                genericParams[i] = GetTypeString(genericClass.classParameters[i]).Replace("::", "_").Replace("*", "Ptr").Replace("<", "_").Replace(">", "_");
              }
            }
            //Il2CppGenericInst generic_inst = il2cppReader.GetIl2CppGenericInst(generic_class.context.class_instPtr);

            //ulong[] pointers = il2cppReader.GetGenericInstPointerArray(generic_inst.type_argv, (Int32)generic_inst.type_argc);
            //for(int i =0;i<pointers.Length;i++)
            //{
            //    genericParams[i] = GetTypeString(il2cppReader.GetIl2CppType(pointers[i]));
            //    typeStr += genericParams[i];
            //    if (i < pointers.Length - 1)
            //        typeStr += ",";
            //}
            //typeStr += ">*";
            //result = typeStr;

            if (TypeBlacklist.isBlacklisted(test, Metadata.resolvedTypes[genericClass.genericClass.typeDefinitionIndex].isMangled))
            {
              test = $"Il2CppObject /*{test}*/";
            }

            if (Metadata.resolvedTypes[genericClass.genericClass.typeDefinitionIndex] is ResolvedClass || Metadata.resolvedTypes[genericClass.genericClass.typeDefinitionIndex] is ResolvedGenericClass || Metadata.resolvedTypes[genericClass.genericClass.typeDefinitionIndex] is ResolvedInterface)
              test += "*";
            result = string.Format(test, genericParams);
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_VAR:
        case Il2CppTypeEnum.IL2CPP_TYPE_MVAR:
          {
            Il2CppGenericParameter il2CppGenericParameter = Metadata.genericParameters[type.data.genericParameterIndex];
            result = GetString(il2CppGenericParameter.nameIndex);
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_TYPEDBYREF:
          result = "Il2CppTypedRef";
          break;
        // TODO
        case Il2CppTypeEnum.IL2CPP_TYPE_FNPTR:
        case Il2CppTypeEnum.IL2CPP_TYPE_BYREF:
        case Il2CppTypeEnum.IL2CPP_TYPE_CMOD_REQD:
        case Il2CppTypeEnum.IL2CPP_TYPE_CMOD_OPT:
        case Il2CppTypeEnum.IL2CPP_TYPE_INTERNAL:
        case Il2CppTypeEnum.IL2CPP_TYPE_MODIFIER:
        case Il2CppTypeEnum.IL2CPP_TYPE_SENTINEL:
        case Il2CppTypeEnum.IL2CPP_TYPE_PINNED:
        case Il2CppTypeEnum.IL2CPP_TYPE_ENUM:
          result = "x";
          break;
        default:
          result = "Type";
          break;
      }

      lock (mapTypeStringCache)
      {
        if (mapTypeStringCache.ContainsKey(type))
          mapTypeStringCache[type] = result;
        else
          mapTypeStringCache.Add(type, result);
      }

      return result;
    }

    public static ResolvedType GetResolvedType(Il2CppType type)
    {
      switch (type.type)
      {

        default:
          return null;
        case Il2CppTypeEnum.IL2CPP_TYPE_CLASS:
        case Il2CppTypeEnum.IL2CPP_TYPE_VALUETYPE:
          {
            return Metadata.resolvedTypes[type.data.klassIndex];
          }

        case Il2CppTypeEnum.IL2CPP_TYPE_GENERICINST:
          {
            Resolvedil2CppGenericClass genericClass = il2cppReader.GetIl2CppGenericClass(type.data.generic_classPtr);
            return Metadata.resolvedTypes[genericClass.genericClass.typeDefinitionIndex];
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_ARRAY:
        case Il2CppTypeEnum.IL2CPP_TYPE_SZARRAY:
        case Il2CppTypeEnum.IL2CPP_TYPE_PTR:
          return GetResolvedType(il2cppReader.GetIl2CppType(type.data.typePtr));
      }
    }

    public static string GetSimpleTypeString(Il2CppType type)
    {
      if (mapSimpleTypeStringCache.TryGetValue(type, out string result))
        return result;

      switch (type.type)
      {
        case Il2CppTypeEnum.IL2CPP_TYPE_VOID:
          result = "Void";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_BOOLEAN:
          result = "Bool";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_CHAR:
          result = "Char";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_I1: // SBYTE
          result = "Int8";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_U1:
          result = "UInt8";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_I2:
          result = "Int16";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_U2:
          result = "UInt16";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_I4:
          result = "Int32";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_U4:
          result = "UInt32";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_I8:
          result = "Int64";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_U8:
          result = "UInt64";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_R4:
          result = "Float";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_R8:
          result = "Double";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_STRING:
          result = "String";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_CLASS:
        case Il2CppTypeEnum.IL2CPP_TYPE_VALUETYPE:
          result = Metadata.resolvedTypes[type.data.klassIndex].Name;
          break;
        // TODO: confirm that's actually equivalent
        case Il2CppTypeEnum.IL2CPP_TYPE_I:
        case Il2CppTypeEnum.IL2CPP_TYPE_U:
          result = "VoidPtr";
          break;
        // TODO: confirm that's actually equivalent or just Il2CppObject*
        case Il2CppTypeEnum.IL2CPP_TYPE_OBJECT:
          result = "Object";
          break;
        // TODO: figure how the fuck to use c# multidimensional arrays as c++
        case Il2CppTypeEnum.IL2CPP_TYPE_ARRAY:
          {
            Il2CppArrayType il2CppArrayType = il2cppReader.GetIl2CppArrayType(type.data.arrayPtr);
            result = $"MArray_{GetSimpleTypeString(il2cppReader.GetIl2CppType(il2CppArrayType.etypePtr))}_";
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_SZARRAY:
          result = $"Array_{GetSimpleTypeString(il2cppReader.GetIl2CppType(type.data.typePtr))}_";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_PTR:
          result = $"{GetSimpleTypeString(il2cppReader.GetIl2CppType(type.data.typePtr))}_";
          break;
        case Il2CppTypeEnum.IL2CPP_TYPE_GENERICINST:
          {
            // TODO: Change to generated structs when ready
            Resolvedil2CppGenericClass genericClass = il2cppReader.GetIl2CppGenericClass(type.data.generic_classPtr);
            //Il2CppGenericClass generic_class = il2cppReader.GetIl2CppGenericClass(type.data.generic_classPtr);

            string typeStr = $"{Metadata.resolvedTypes[genericClass.genericClass.typeDefinitionIndex].Name}";
            // For whatever reason generic type names end with ` and digit (eg. List`1)
            if (typeStr.Contains('`'))
            {
              int idx = typeStr.IndexOf('`');
              typeStr = typeStr.Remove(idx, typeStr.Length - idx);
            }

            typeStr += "_";

            Il2CppGenericInst generic_inst = il2cppReader.GetIl2CppGenericInst(genericClass.genericClass.context.class_instPtr);
            ulong[] pointers = il2cppReader.GetGenericInstPointerArray(generic_inst.type_argv, (Int32)generic_inst.type_argc);
            for (int i = 0; i < pointers.Length; i++)
            {
              typeStr += GetSimpleTypeString(il2cppReader.GetIl2CppType(pointers[i]));
              if (i < pointers.Length - 1)
                typeStr += "_";
            }
            typeStr += "_";
            result = typeStr;
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_VAR:
        case Il2CppTypeEnum.IL2CPP_TYPE_MVAR:
          {
            Il2CppGenericParameter il2CppGenericParameter = Metadata.genericParameters[type.data.genericParameterIndex];
            result = GetString(il2CppGenericParameter.nameIndex);
            break;
          }
        case Il2CppTypeEnum.IL2CPP_TYPE_TYPEDBYREF:
          result = "TypedRef";
          break;
        // TODO
        case Il2CppTypeEnum.IL2CPP_TYPE_FNPTR:
        case Il2CppTypeEnum.IL2CPP_TYPE_BYREF:
        case Il2CppTypeEnum.IL2CPP_TYPE_CMOD_REQD:
        case Il2CppTypeEnum.IL2CPP_TYPE_CMOD_OPT:
        case Il2CppTypeEnum.IL2CPP_TYPE_INTERNAL:
        case Il2CppTypeEnum.IL2CPP_TYPE_MODIFIER:
        case Il2CppTypeEnum.IL2CPP_TYPE_SENTINEL:
        case Il2CppTypeEnum.IL2CPP_TYPE_PINNED:
        case Il2CppTypeEnum.IL2CPP_TYPE_ENUM:
          result = "x";
          break;
        default:
          result = "Type";
          break;
      }

      mapSimpleTypeStringCache.Add(type, result);
      return result;
    }
  }
}
