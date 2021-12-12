using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator {
  public class ResolvedMethod : ResolvedObject {
    public Int32 methodIndex;
    public Il2CppMethodDefinition methodDef = null;
    public Il2CppType returnType = null;
    public List<ResolvedParameter> resolvedParameters = new List<ResolvedParameter>();
    public ResolvedType declaringType = null;
    public string MI_Name = "";
    public List<ulong> refAddrs = new List<ulong>();
    public List<ResolvedMethod> refMethods = new List<ResolvedMethod>();
    public bool isReferenced { get; set; }
    public ResolvedMethod overridenMethod = null;
    private ulong _methodPtr = UInt64.MaxValue;
    public ulong methodPtr {
      get {
        if (_methodPtr != UInt64.MaxValue)
          return _methodPtr;

        if (methodIndex < 0)
          return 0;

        var method = (methodDef.token & 0xffffff);
        if (method == 0)
          return 0;

        // In the event of an exception, the method pointer is not set in the file
        // This probably means it has been optimized away by the compiler, or is an unused generic method
        try {
          // Remove ARM Thumb marker LSB if necessary
          //start = Binary.ModuleMethodPointers[module][method - 1];
          _methodPtr = this.declaringType.resolvedImage.resolvedModule.methodPointers[method - 1];

          if (_methodPtr != 0) {
            var section = Section.ByRVA(RVA.FromVA(_methodPtr));
            if (!section.ReadableName.Contains("il2cpp")) {

            }
          }
        }
        catch (IndexOutOfRangeException) {
          _methodPtr = 0;
        }
        return _methodPtr;
      }
    }

    public bool isCtor = false;

    public List<string> StringLiterals {
      get {
        var literals = new List<string>();

        if (methodPtr == 0)
          return literals;

        var literalPtrs = CodeScanner.m_mapFunctionStringLiterals[methodPtr];
        for (int i = 0; i < literalPtrs.Count; i++) {
          var str = MetadataReader.GetStringLiteralFromIndex(literalPtrs[i]).Replace("\n", "\\n").Replace("\r", "\\r");
          literals.Add(str);
        }

        return literals;
      }
    }

    public int StringLiteralsCount {
      get {
        if (methodPtr == 0)
          return 0;

        return CodeScanner.m_mapFunctionStringLiterals[methodPtr].Count;
      }
    }


    public ResolvedMethod(Il2CppMethodDefinition methodDefinition, Int32 methodIdx, ResolvedType declType) {
      methodIndex = methodIdx;
      methodDef = methodDefinition;
      returnType = il2cpp.types[methodDef.returnType];
      declaringType = declType;
      Name = MetadataReader.GetString(methodDef.nameIndex);
      for (int i = 0; i < methodDef.parameterCount; i++) {
        ResolvedParameter resolvedParameter = new ResolvedParameter(Metadata.parameterDefinitions[methodDef.parameterStart + i], methodDef.parameterStart + i);
        resolvedParameters.Add(resolvedParameter);
      }

      isMangled = !Name.isCSharpIdentifier();
      if (isMangled)
        isReferenced = false;

      isCtor = Name == ".ctor";
    }



    public async Task ToHeaderCode(StreamWriter sw, Int32 indent = 0) {
      var comment = methodPtr == 0 ? "// " : "";

      await sw.WriteAsync("".Indent(indent));

      if (methodDef.slot != il2cpp_Constants.kInvalidIl2CppMethodSlot) {
        await sw.WriteAsync($"// Slot: {methodDef.slot}\n");
        await sw.WriteAsync($"".Indent(indent));
      }

      if (methodPtr != 0) {
        await sw.WriteAsync($"// RVA: 0x{RVA.FromVA(methodPtr):X} VA: 0x{methodPtr:X16}\n");
        await sw.WriteAsync($"".Indent(indent));

        //var stringliterals = CodeScanner.
      }

      if (this.StringLiteralsCount > 0) {
        var literals = this.StringLiterals;
        await sw.WriteAsync($"// Strings:\n");
        await sw.WriteAsync($"{comment}".Indent(indent));
        for (int i = 0; i < this.StringLiteralsCount; i++) {
          await sw.WriteAsync($"// \"{literals[i]}\"\n");
          await sw.WriteAsync($"".Indent(indent));
        }
      }

      await sw.WriteAsync(comment);

      if (isStatic)
        await sw.WriteAsync($"static ");

      if (isVirtual)
        await sw.WriteAsync($"/*virtual*/ ");

      //if(isCtor && declaringType is ResolvedStruct)
      //{
      //    code += $"{declaringType.Name}(";
      //    for (int i = 0; i < resolvedParameters.Count; i++)
      //    {
      //        code += resolvedParameters[i].ToHeaderCode();
      //        if (i < resolvedParameters.Count - 1)
      //            code += ", ";
      //    }

      //    code += ")";
      //    if (isOverride && ((isMangled && overridenMethod != null) || !isMangled))
      //        code += " /*override*/";
      //    code += ";\n";
      //    code += "".Indent(indent);
      //}

      await sw.WriteAsync($"{MetadataReader.GetTypeString(returnType)} {Name.CSharpToCppIdentifier()}(");
      for (int i = 0; i < resolvedParameters.Count; i++) {
        await sw.WriteAsync(resolvedParameters[i].ToHeaderCode());
        if (i < resolvedParameters.Count - 1)
          await sw.WriteAsync(", ");
      }

      await sw.WriteAsync(")");
      if (isOverride && ((isMangled && overridenMethod != null) || !isMangled))
        await sw.WriteAsync(" /*override*/");
      await sw.WriteAsync($";\n");
    }

    public string ToHeaderCode(Int32 indent = 0) {
      string code = "".Indent(indent);

      if (methodDef.slot != il2cpp_Constants.kInvalidIl2CppMethodSlot) {
        code += $"// Slot: {methodDef.slot}\n";
        code += "".Indent(indent);
      }

      if (methodPtr != 0) {
        code += $"// RVA: 0x{RVA.FromVA(methodPtr):X} VA: 0x{methodPtr:X16}\n";
        code += "".Indent(indent);
      }

      if (this.StringLiteralsCount > 0) {
        var literals = this.StringLiterals;
        code += $"// Strings:\n";
        code += "".Indent(indent);

        for (int i = 0; i < this.StringLiteralsCount; i++) {
          code += $"// \"{literals[i]}\"\n";
          code += "".Indent(indent);
        }
      }

      if (isStatic)
        code += "static ";

      if (isVirtual)
        code += "/*virtual*/ ";

      //if(isCtor && declaringType is ResolvedStruct)
      //{
      //    code += $"{declaringType.Name}(";
      //    for (int i = 0; i < resolvedParameters.Count; i++)
      //    {
      //        code += resolvedParameters[i].ToHeaderCode();
      //        if (i < resolvedParameters.Count - 1)
      //            code += ", ";
      //    }

      //    code += ")";
      //    if (isOverride && ((isMangled && overridenMethod != null) || !isMangled))
      //        code += " /*override*/";
      //    code += ";\n";
      //    code += "".Indent(indent);
      //}

      code += $"{MetadataReader.GetTypeString(returnType)} {Name.CSharpToCppIdentifier()}(";
      for (int i = 0; i < resolvedParameters.Count; i++) {
        code += resolvedParameters[i].ToHeaderCode();
        if (i < resolvedParameters.Count - 1)
          code += ", ";
      }

      code += ")";
      if (isOverride && ((isMangled && overridenMethod != null) || !isMangled))
        code += " /*override*/";
      code += ";\n";

      return code;
    }

    public override string Name {
      get {
        if (isOverride && overridenMethod != null)
          return overridenMethod.Name;
        return base.Name;
      }
    }

    public async Task ToCppCode(StreamWriter sw, Int32 indent = 0) {
      var comment = methodPtr == 0 ? "// " : "";

      bool needsPtr = false;
      if (returnType.type != Il2CppTypeEnum.IL2CPP_TYPE_PTR && returnType.type != Il2CppTypeEnum.IL2CPP_TYPE_CLASS && returnType.type != Il2CppTypeEnum.IL2CPP_TYPE_STRING)
        needsPtr = true;
      await sw.WriteAsync("".Indent(indent));
      string returnTypeStr = MetadataReader.GetTypeString(returnType);

      string paramStr = "nullptr";
      string instanceStr = isStatic ? "nullptr" : "this";

      //if (isCtor && declaringType is ResolvedStruct)
      //{
      //    sw.Write($"{declaringType.NestedName()}{declaringType.Name}(";
      //    for (int i = 0; i < resolvedParameters.Count; i++)
      //    {
      //        sw.Write(resolvedParameters[i].ToCppCode();
      //        if (i < resolvedParameters.Count - 1)
      //            sw.Write(", ";
      //    }
      //    sw.Write(")\n";
      //    sw.Write("{\n".Indent(indent);
      //    if (resolvedParameters.Count > 0)
      //    {
      //        paramStr = "params";
      //        sw.Write($"void* params[{resolvedParameters.Count}] = {{".Indent(indent + 2);
      //        for (int i = 0; i < resolvedParameters.Count; i++)
      //        {
      //            if (resolvedParameters[i].isValueType && !resolvedParameters[i].isOut)
      //                sw.Write("&";

      //            sw.Write(resolvedParameters[i].Name;
      //            if (i < resolvedParameters.Count - 1)
      //                sw.Write(", ";
      //        }
      //        sw.Write("};\n";
      //    }

      //    //void* params[4] = { parameter_1, &parameter_2, &parameter_3, &parameter_4 };
      //    sw.Write("".Indent(indent + 2);
      //    if (returnTypeStr != "void")
      //        sw.Write("Il2CppBoxedObject* ret = ";
      //    sw.Write($"il2cpp::runtime_invoke({MI_Name}, {instanceStr}, {paramStr});\n";
      //    //resolvedParameters.Count

      //    if (returnTypeStr != "void")
      //    {
      //        sw.Write($"return ".Indent(indent + 2);
      //        if (needsPtr)
      //            sw.Write("*";
      //        sw.Write($"({returnTypeStr}";
      //        if (needsPtr)
      //            sw.Write("*";
      //        sw.Write($")il2cpp::object_unbox(ret);\n";
      //    }

      //    // code
      //    sw.Write("}\n".Indent(indent);
      //}


      await sw.WriteAsync($"{comment}{returnTypeStr} {declaringType.NestedName()}{Name.CSharpToCppIdentifier()}(");
      for (int i = 0; i < resolvedParameters.Count; i++) {
        await resolvedParameters[i].ToCppCode(sw);
        //sw.Write(resolvedParameters[i].ToCppCode());
        if (i < resolvedParameters.Count - 1)
          await sw.WriteAsync(", ");
      }
      await sw.WriteAsync(")\n");
      await sw.WriteAsync($"{comment}{{\n".Indent(indent));

      if (resolvedParameters.Count > 0) {
        paramStr = "params";
        await sw.WriteAsync($"{comment}void* params[{resolvedParameters.Count}] = {{".Indent(indent + 2));
        for (int i = 0; i < resolvedParameters.Count; i++) {
          if (resolvedParameters[i].isValueType && !resolvedParameters[i].isOut)
            await sw.WriteAsync("&");

          await sw.WriteAsync(resolvedParameters[i].Name);
          if (i < resolvedParameters.Count - 1)
            await sw.WriteAsync(", ");
        }
        await sw.WriteAsync("};\n");
      }

      //void* params[4] = { parameter_1, &parameter_2, &parameter_3, &parameter_4 };
      await sw.WriteAsync($"{comment}".Indent(indent + 2));

      if (returnTypeStr.EndsWith("*")) {
        await sw.WriteAsync($"return ({returnTypeStr})il2cpp::runtime_invoke({MI_Name}, {instanceStr}, {paramStr});\n");
      }
      else {
        if (returnTypeStr != "void")
          await sw.WriteAsync($"{comment}Il2CppBoxedObject * ret = ");
        await sw.WriteAsync($"{comment}il2cpp::runtime_invoke({MI_Name}, {instanceStr}, {paramStr});\n");
        //resolvedParameters.Count

        if (returnTypeStr != "void") {
          await sw.WriteAsync($"{comment}return ".Indent(indent + 2));
          if (needsPtr)
            await sw.WriteAsync("*");
          await sw.WriteAsync($"({returnTypeStr}");
          if (needsPtr)
            await sw.WriteAsync("*");
          await sw.WriteAsync($")il2cpp::object_unbox(ret);\n");
        }
      }

      // code
      await sw.WriteAsync($"{comment}}}\n".Indent(indent));
    }

    public string ToCppCode(Int32 indent = 0) {
      bool needsPtr = false;
      if (returnType.type != Il2CppTypeEnum.IL2CPP_TYPE_PTR && returnType.type != Il2CppTypeEnum.IL2CPP_TYPE_CLASS && returnType.type != Il2CppTypeEnum.IL2CPP_TYPE_STRING)
        needsPtr = true;
      string code = "".Indent(indent);
      string returnTypeStr = MetadataReader.GetTypeString(returnType);

      string paramStr = "nullptr";
      string instanceStr = isStatic ? "nullptr" : "this";

      //if (isCtor && declaringType is ResolvedStruct)
      //{
      //    code += $"{declaringType.NestedName()}{declaringType.Name}(";
      //    for (int i = 0; i < resolvedParameters.Count; i++)
      //    {
      //        code += resolvedParameters[i].ToCppCode();
      //        if (i < resolvedParameters.Count - 1)
      //            code += ", ";
      //    }
      //    code += ")\n";
      //    code += "{\n".Indent(indent);
      //    if (resolvedParameters.Count > 0)
      //    {
      //        paramStr = "params";
      //        code += $"void* params[{resolvedParameters.Count}] = {{".Indent(indent + 2);
      //        for (int i = 0; i < resolvedParameters.Count; i++)
      //        {
      //            if (resolvedParameters[i].isValueType && !resolvedParameters[i].isOut)
      //                code += "&";

      //            code += resolvedParameters[i].Name;
      //            if (i < resolvedParameters.Count - 1)
      //                code += ", ";
      //        }
      //        code += "};\n";
      //    }

      //    //void* params[4] = { parameter_1, &parameter_2, &parameter_3, &parameter_4 };
      //    code += "".Indent(indent + 2);
      //    if (returnTypeStr != "void")
      //        code += "Il2CppBoxedObject* ret = ";
      //    code += $"il2cpp::runtime_invoke({MI_Name}, {instanceStr}, {paramStr});\n";
      //    //resolvedParameters.Count

      //    if (returnTypeStr != "void")
      //    {
      //        code += $"return ".Indent(indent + 2);
      //        if (needsPtr)
      //            code += "*";
      //        code += $"({returnTypeStr}";
      //        if (needsPtr)
      //            code += "*";
      //        code += $")il2cpp::object_unbox(ret);\n";
      //    }

      //    // code
      //    code += "}\n".Indent(indent);
      //}

      code += $"{returnTypeStr} {declaringType.NestedName()}{Name.CSharpToCppIdentifier()}(";
      for (int i = 0; i < resolvedParameters.Count; i++) {
        code += resolvedParameters[i].ToCppCode();
        if (i < resolvedParameters.Count - 1)
          code += ", ";
      }
      code += ")\n";
      code += "{\n".Indent(indent);

      if (resolvedParameters.Count > 0) {
        paramStr = "params";
        code += $"void* params[{resolvedParameters.Count}] = {{".Indent(indent + 2);
        for (int i = 0; i < resolvedParameters.Count; i++) {
          if (resolvedParameters[i].isValueType && !resolvedParameters[i].isOut)
            code += "&";

          code += resolvedParameters[i].Name;
          if (i < resolvedParameters.Count - 1)
            code += ", ";
        }
        code += "};\n";
      }

      //void* params[4] = { parameter_1, &parameter_2, &parameter_3, &parameter_4 };
      code += "".Indent(indent + 2);

      if (returnTypeStr.EndsWith("*")) {
        code += $"return ({returnTypeStr})il2cpp::runtime_invoke({MI_Name}, {instanceStr}, {paramStr});\n";
      }
      else {
        if (returnTypeStr != "void")
          code += "Il2CppBoxedObject* ret = ";
        code += $"il2cpp::runtime_invoke({MI_Name}, {instanceStr}, {paramStr});\n";
        //resolvedParameters.Count

        if (returnTypeStr != "void") {
          code += $"return ".Indent(indent + 2);
          if (needsPtr)
            code += "*";
          code += $"({returnTypeStr}";
          if (needsPtr)
            code += "*";
          code += $")il2cpp::object_unbox(ret);\n";
        }
      }


      // code
      code += "}\n".Indent(indent);
      return code;
    }

    public bool isStatic {
      get {
        return (methodDef.flags & il2cpp_Constants.METHOD_ATTRIBUTE_STATIC) != 0;
      }
    }

    public bool isVirtual {
      get {
        return (methodDef.flags & il2cpp_Constants.METHOD_ATTRIBUTE_VIRTUAL) != 0 &&
            (methodDef.flags & il2cpp_Constants.METHOD_ATTRIBUTE_VTABLE_LAYOUT_MASK) == il2cpp_Constants.METHOD_ATTRIBUTE_NEW_SLOT;
      }
    }

    public bool isOverride {
      get {
        UInt32 vtableflags = methodDef.flags & il2cpp_Constants.METHOD_ATTRIBUTE_VTABLE_LAYOUT_MASK;
        return ((methodDef.flags & il2cpp_Constants.METHOD_ATTRIBUTE_VIRTUAL) != 0 && vtableflags != il2cpp_Constants.METHOD_ATTRIBUTE_NEW_SLOT) ||
            ((methodDef.flags & il2cpp_Constants.METHOD_ATTRIBUTE_FINAL) != 0 && vtableflags == il2cpp_Constants.METHOD_ATTRIBUTE_REUSE_SLOT) ||
            ((methodDef.flags & il2cpp_Constants.METHOD_ATTRIBUTE_ABSTRACT) != 0 && vtableflags == il2cpp_Constants.METHOD_ATTRIBUTE_REUSE_SLOT);
      }
    }

    public bool isValidOverride {
      get {
        return isOverride && overridenMethod != null;
      }
    }

    public uint accessFlag {
      get {
        return methodDef.flags & il2cpp_Constants.METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK;
      }
    }

    public bool isPublic {
      get {
        return this.accessFlag == il2cpp_Constants.METHOD_ATTRIBUTE_PUBLIC;
      }
    }

    public bool isPrivate {
      get => this.accessFlag == il2cpp_Constants.METHOD_ATTRIBUTE_PRIVATE;
    }

    public override bool isMangled {
      get {
        if (overridenMethod == null)
          return base.isMangled;

        return overridenMethod.isMangled;
      }
    }

    public string ValidString() {
      if (isOverride && overridenMethod == null)
        return "Invalid";
      return "";
    }

    public string DemangledPrefix() {
      string prefix = $"{ReferencedString()}m{StaticString()}_{ValidString()}{VirtualString()}{AccessString()}{MetadataReader.GetSimpleTypeString(returnType)}";
      for (int i = 0; i < resolvedParameters.Count; i++) {
        if (!prefix.EndsWith("_"))
          prefix += "_";

        prefix += MetadataReader.GetSimpleTypeString(resolvedParameters[i].type);
      }
      return prefix;
    }

    public string ReferencedString() {
      if (isReferenced || !isMangled)
        return "";
      return "u";
    }

    public string VirtualString() {
      if (isVirtual)
        return "Virtual";
      else if (isOverride)
        return "Override";
      return "";
    }

    public string StaticString() {
      if (isStatic)
        return "s";
      return "";
    }

    public string AccessString() {
      var accessFlag = methodDef.flags & il2cpp_Constants.METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK;
      switch (accessFlag) {
        case il2cpp_Constants.METHOD_ATTRIBUTE_PRIVATE:
          return "Private";
        case il2cpp_Constants.METHOD_ATTRIBUTE_PUBLIC:
          return "Public";
        case il2cpp_Constants.METHOD_ATTRIBUTE_FAMILY:
          return "Protected";
        case il2cpp_Constants.METHOD_ATTRIBUTE_ASSEM:
        case il2cpp_Constants.METHOD_ATTRIBUTE_FAM_AND_ASSEM:
          return "Internal";
        case il2cpp_Constants.METHOD_ATTRIBUTE_FAM_OR_ASSEM:
          return "ProtectedInternal";
      }
      return "";
    }



    public void DemangleParams() {
      for (int i = 0; i < resolvedParameters.Count; i++) {
        ResolvedParameter resolvedParameter = resolvedParameters[i];
        if (resolvedParameter.Name.isCSharpIdentifier()) {
          if (resolvedParameter.Name.isCppIdentifier())
            continue;

          resolvedParameter.Name = resolvedParameter.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
          continue;
        }

        resolvedParameter.Name = $"_{i}";
      }
    }

    static public List<ResolvedMethod> broken_methods = new List<ResolvedMethod>();

    internal void ResolveOverride() {
      if (!isOverride)
        return;

      if (declaringType.parentType == null)
        return;

      if (declaringType is ResolvedClass) {
        ResolvedClass resolvedClass = declaringType.parentType as ResolvedClass;
        while (overridenMethod == null && resolvedClass != null) {
          if (resolvedClass.slottedMethods.TryGetValue(methodDef.slot, out var resolvedMethod)) {
            overridenMethod = resolvedMethod;
            break;
            //return;
          }
          else {
            resolvedClass = (ResolvedClass)resolvedClass.parentType;
          }
        }
      }
      else if (declaringType is ResolvedStruct) {
        ResolvedStruct resolvedStruct = declaringType.parentType as ResolvedStruct;
        while (overridenMethod == null && resolvedStruct != null) {
          if (resolvedStruct.slottedMethods.TryGetValue(methodDef.slot, out var resolvedMethod)) {
            overridenMethod = resolvedMethod;
            break;
            //return;
          }
          else {
            resolvedStruct = (ResolvedStruct)resolvedStruct.parentType;
          }
        }
      }

      if (overridenMethod == null)
        broken_methods.Add(this);

      // TODO: Link Override methods and Virtual methods for reference linking
      if (overridenMethod != null) {
        CodeScanner.LinkOverride(overridenMethod, this);
      }
    }
  }
}
