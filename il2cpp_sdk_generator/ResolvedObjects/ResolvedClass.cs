using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator {
  public class ResolvedClass : ResolvedType {
    bool isGeneric = false;
    string genericTemplate = "";


    public ResolvedClass(Il2CppTypeDefinition type, Int32 idx) {
      typeDef = type;
      typeDefinitionIndex = idx;
      Name = MetadataReader.GetString(typeDef.nameIndex);
      Namespace = MetadataReader.GetString(typeDef.namespaceIndex);
      isMangled = !Name.isCSharpIdentifier();
    }

    public override void Resolve() {
      if (isResolved)
        return;

      // TODO: Just generate all existing generics as diffferent structs
      // Resolve generic name
      if (typeDef.genericContainerIndex >= 0) {
        isGeneric = true;
        Name = Name.Replace("`", "_");
        //if (Name.Contains('`'))
        //{
        //    int idx = Name.IndexOf('`');
        //    Name = Name.Remove(idx, Name.Length - idx);
        //}

        //MakeGenericTemplate();
      }

      // Resolve fields
      // Resolve static fields
      // Resolve constats (if needed)
      for (int i = 0; i < typeDef.field_count; i++) {
        ResolvedField resolvedField = new ResolvedField(Metadata.fieldDefinitions[i + typeDef.fieldStart], this.typeDefinitionIndex, i);

        if (resolvedField.isStatic)
          staticFields.Add(resolvedField);
        else if (resolvedField.isConst)
          constFields.Add(resolvedField);
        else
          instanceFields.Add(resolvedField);
      }

      for (int i = 0; i < nestedTypes.Count; i++) {
        ResolvedType resolvedType = nestedTypes[i];
        resolvedType.requiresForwardCode = instanceFields.Find(f => MetadataReader.GetResolvedType(f.type) == resolvedType) != null;
      }

      Dictionary<Int32, ResolvedProperty> propertyMethods = new Dictionary<int, ResolvedProperty>();
      ResolvedProperty[] properties = new ResolvedProperty[typeDef.property_count];
      // Resolve properties (and exclude getters/setters from methods?)
      for (int i = 0; i < typeDef.property_count; i++) {
        Il2CppPropertyDefinition propDef = Metadata.propertyDefinitions[typeDef.propertyStart + i];
        ResolvedProperty resolvedProperty = new ResolvedProperty(propDef);
        if (propDef.get > -1)
          propertyMethods.Add(propDef.get, resolvedProperty);
        if (propDef.set > -1)
          propertyMethods.Add(propDef.set, resolvedProperty);
        properties[i] = resolvedProperty;
      }

      Dictionary<Int32, ResolvedEvent> eventMethods = new Dictionary<int, ResolvedEvent>();
      ResolvedEvent[] events = new ResolvedEvent[typeDef.event_count];
      // Resolve events (and exclude add/remove/raise from methods?)
      for (int i = 0; i < typeDef.event_count; i++) {
        Il2CppEventDefinition eventDef = Metadata.eventDefinitions[typeDef.eventStart + i];
        ResolvedEvent resolvedEvent = new ResolvedEvent(eventDef);
        if (eventDef.add > -1)
          eventMethods.Add(eventDef.add, resolvedEvent);
        if (eventDef.remove > -1)
          eventMethods.Add(eventDef.remove, resolvedEvent);
        if (eventDef.raise > -1)
          eventMethods.Add(eventDef.raise, resolvedEvent);
        events[i] = resolvedEvent;
      }


      // Resolve methods
      // TODO: generic methods
      for (int i = 0; i < typeDef.method_count; i++) {
        Il2CppMethodDefinition methodDef = Metadata.methodDefinitions[typeDef.methodStart + i];

        if (typeDef.genericContainerIndex < 0 && methodDef.genericContainerIndex >= 0) {

        }

        // For now skip generic methods
        if (methodDef.genericContainerIndex >= 0)
          continue;

        ResolvedMethod resolvedMethod = new ResolvedMethod(methodDef, typeDef.methodStart + i, this);
        miMethods.Add(resolvedMethod);
        if (methodDef.slot != il2cpp_Constants.kInvalidIl2CppMethodSlot)
          slottedMethods.Add(methodDef.slot, resolvedMethod);
        if (propertyMethods.TryGetValue(i, out var resolvedProperty)) {
          resolvedMethod.isReferenced = true;
          resolvedProperty.AssignMethod(i, resolvedMethod);
          continue;
        }

        if (eventMethods.TryGetValue(i, out var resolvedEvent)) {
          resolvedMethod.isReferenced = true;
          resolvedEvent.AssignMethod(i, resolvedMethod);
          continue;
        }

        if (!resolvedMethod.isStatic)
          instanceMethods.Add(resolvedMethod);
        else {
          staticMethods.Add(resolvedMethod);
        }

        //if (methodDef.genericContainerIndex >= 0 && name == "GetComponent")
        //    Console.WriteLine();
      }

      // Split properties into instanced and static
      for (int i = 0; i < properties.Length; i++) {
        if (properties[i].isStatic)
          staticProperties.Add(properties[i]);
        else
          instanceProperties.Add(properties[i]);
      }

      // Split events into instanced and static
      for (int i = 0; i < events.Length; i++) {
        if (events[i].isStatic)
          staticEvents.Add(events[i]);
        else
          instanceEvents.Add(events[i]);
      }

      //Console.WriteLine("ResolvedClass::Resolve()");
      isResolved = true;
    }

    public override async Task ToHeaderCode(StreamWriter sw, Int32 indent = 0) {
      GenerateMINames();

      if (!isNested) {
        await sw.WriteLineAsync("#pragma once".Indent(indent));
        await sw.WriteLineAsync();
        // Namespace
        if (Namespace != "") {
          await sw.WriteLineAsync($"namespace {CppNamespace()}".Indent(indent));
          await sw.WriteLineAsync("{".Indent(indent));
          indent += 2;
        }
      }

      await sw.WriteLineAsync($"// Class TypeDefinitionIndex: {typeDefinitionIndex}".Indent(indent));

      //if (typeDef.genericContainerIndex >= 0)
      //    code += $"{genericTemplate}\n".Indent(indent);
      string NestedNameStr = DeclarationString();
      //NestedNameStr = NestedNameStr.Substring(0, NestedNameStr.Length - 2);
      await sw.WriteAsync($"struct {NestedNameStr}".Indent(indent));
      //code += $"struct {Name}".Indent(indent);
      if (parentType != null)
        await sw.WriteAsync($" : {parentType.GetFullName()}");
      else
        await sw.WriteAsync($" : Il2CppObject");
      await sw.WriteLineAsync();
      await sw.WriteLineAsync("{".Indent(indent));

      indent += 2;

      if (nestedTypes.Count > 0) {
        for (int i = 0; i < nestedTypes.Count; i++) {
          if ((Name == "TMP_Text" && (nestedTypes[i].Name == "SpecialCharacter" || nestedTypes[i].Name == "TextBackingContainer"))) {
            string nestedCode = nestedTypes[i].ToHeaderCode(indent);
            //nestedCode = nestedCode.Replace($"{GetFullName()}", "");
            //nestedCode = nestedCode.Replace($"{Name}::", "");
            nestedCode = nestedCode.Replace($"{nestedTypes[i].DeclarationString()}", $"{nestedTypes[i].Name}");
            await sw.WriteAsync(nestedCode);
            await sw.WriteAsync("\n");
            continue;
          }

          if (nestedTypes[i].GetType() == typeof(ResolvedInterface)) {
            continue;
          }

          await sw.WriteAsync(nestedTypes[i].ForwardDeclaration(indent));
          await sw.WriteLineAsync();
        }
      }

      if (typeDef.genericContainerIndex == -1) {

        if (instanceFields.Count > 0) {
          for (int i = 0; i < instanceFields.Count; i++) {
            await sw.WriteAsync(instanceFields[i].ToCode(indent));
          }
          await sw.WriteLineAsync();
        }
        if (instanceProperties.Count > 0) {
          await sw.WriteLineAsync("// Instance Properties".Indent(indent));
          for (int i = 0; i < instanceProperties.Count; i++) {
            await instanceProperties[i].ToHeaderCode(sw, indent);
          }
          await sw.WriteLineAsync();
        }
        if (staticProperties.Count > 0) {
          await sw.WriteLineAsync("// Static Properties".Indent(indent));
          for (int i = 0; i < staticProperties.Count; i++) {
            await staticProperties[i].ToHeaderCode(sw, indent);
          }
          await sw.WriteLineAsync();
        }
        if (instanceEvents.Count > 0) {
          await sw.WriteLineAsync("// Instance Events".Indent(indent));
          for (int i = 0; i < instanceEvents.Count; i++) {
            await instanceEvents[i].ToHeaderCode(sw, indent);
          }
          await sw.WriteLineAsync();
        }
        if (staticEvents.Count > 0) {
          await sw.WriteLineAsync("// Static Events".Indent(indent));
          for (int i = 0; i < staticEvents.Count; i++) {
            await staticEvents[i].ToHeaderCode(sw, indent);
          }
          await sw.WriteLineAsync();
        }
        if (instanceMethods.Count > 0) {
          await sw.WriteLineAsync("// Instance Methods".Indent(indent));
          for (int i = 0; i < instanceMethods.Count; i++) {
            await instanceMethods[i].ToHeaderCode(sw, indent);
          }
          await sw.WriteLineAsync();
        }
        if (staticMethods.Count > 0) {
          await sw.WriteLineAsync("// Static Methods".Indent(indent));
          for (int i = 0; i < staticMethods.Count; i++) {
            await staticMethods[i].ToHeaderCode(sw, indent);
          }
          await sw.WriteLineAsync();
        }
      }
      // TODO: Fields, static fields, const fields, properties, methods, (generics), nested types


      // il2cpp stuff
      await sw.WriteLineAsync();
      //await sw.WriteAsync($"{Name}() {{}}\n".Indent(indent));
      //await sw.WriteAsync("\n");
      //await sw.WriteAsync("\n");
      await sw.WriteLineAsync("// il2cpp stuff".Indent(indent));
      await sw.WriteLineAsync("static bool _il2cpp_initialized;".Indent(indent));
      await sw.WriteLineAsync("static ::Il2CppClass* il2cppClass;".Indent(indent));
      await sw.WriteLineAsync("static System::Type* il2cppObjectType;".Indent(indent));
      await sw.WriteLineAsync("// static fields".Indent(indent));
      for (int i = 0; i < staticFields.Count; i++) {
        await sw.WriteLineAsync($"static ::FieldInfo* FI_{staticFields[i].Name.CSharpToCppIdentifier()};".Indent(indent));
      }
      await sw.WriteLineAsync();
      await sw.WriteLineAsync("// methods".Indent(indent));
      for (int i = 0; i < miMethods.Count; i++) {
        await sw.WriteLineAsync($"static ::MethodInfo* {miMethods[i].MI_Name};".Indent(indent));
      }
      await sw.WriteLineAsync("static void il2cpp_init();".Indent(indent));
      await sw.WriteLineAsync();
      // Extensions
      await sw.WriteLineAsync("// Extension methods".Indent(indent));

      await OutputConstructorMethods(sw, indent);

      if (this.Name == "GameObject" && this.Namespace == "UnityEngine") {
        await GameObjectExtensions(sw, indent);
      }
      else if (this.Name == "Component" && this.Namespace == "UnityEngine") {
        await ComponentExtensions(sw, indent);
      }

      indent -= 2;

      await sw.WriteLineAsync("};".Indent(indent));

      if (Name == "ParticleSystem") {
        int nBurst = -1;
        int nMinMaxCurve = -1;
        for (int i = 0; i < nestedTypes.Count; i++) {
          if (nestedTypes[i].Name == "Burst")
            nBurst = i;
          else if (nestedTypes[i].Name == "MinMaxCurve")
            nMinMaxCurve = i;
        }

        if (nMinMaxCurve >= 0 && nBurst >= 0 && nMinMaxCurve > nBurst) {
          ResolvedType resolvedType = nestedTypes[nMinMaxCurve];
          nestedTypes[nMinMaxCurve] = nestedTypes[nBurst];
          nestedTypes[nBurst] = resolvedType;
        }
      }

      if (nestedTypes.Count > 0) {
        for (int i = 0; i < nestedTypes.Count; i++) {
          if ((Name == "TMP_Text" && (nestedTypes[i].Name == "SpecialCharacter" || nestedTypes[i].Name == "TextBackingContainer"))) {
            continue;
          }

          await nestedTypes[i].ToHeaderCode(sw, indent);
          //sw.Write(nestedTypes[i].ToHeaderCode(indent));
          await sw.WriteLineAsync();
        }
      }

      // Namespace
      if (Namespace != "" && !isNested) {
        indent -= 2;
        await sw.WriteLineAsync("}".Indent(indent));
      }
    }

    async Task OutputConstructorMethods(StreamWriter sw, Int32 indent = 0) {
      string NestedNameStr = DeclarationString();

      for (int i = 0; i < instanceMethods.Count; i++) {
        if (instanceMethods[i].Name == ".ctor") {

        }

        if (instanceMethods[i].Name == "_ctor") {

        }

        if (instanceMethods[i].Name != ".ctor")
          continue;

        ResolvedMethod resolvedMethod = instanceMethods[i];

        string returnString = MetadataReader.GetTypeString(resolvedMethod.returnType);
        await sw.WriteAsync($"static {NestedNameStr}* New(".Indent(indent));

        var resolvedParameters = resolvedMethod.resolvedParameters;
        for (int k = 0; k < resolvedParameters.Count; k++) {
          await sw.WriteAsync(resolvedParameters[k].ToHeaderCode());
          if (k < resolvedParameters.Count - 1)
            await sw.WriteAsync(", ");
        }
        await sw.WriteLineAsync(")");
        await sw.WriteLineAsync("{".Indent(indent));
        await sw.WriteLineAsync($"{NestedNameStr}* obj = ({NestedNameStr}*)il2cpp::object_new(il2cppClass);".Indent(indent + 2));
        string paramStr = "nullptr";
        if (resolvedParameters.Count > 0) {
          paramStr = "params";
          await sw.WriteAsync($"void* params[{resolvedParameters.Count}] = {{".Indent(indent + 2));
          for (int k = 0; k < resolvedParameters.Count; k++) {
            if (resolvedParameters[k].isValueType && !resolvedParameters[k].isOut)
              await sw.WriteAsync("&");

            await sw.WriteAsync(resolvedParameters[k].Name);
            if (k < resolvedParameters.Count - 1)
              await sw.WriteAsync(", ");
          }
          await sw.WriteLineAsync("};");
        }
        await sw.WriteLineAsync($"il2cpp::runtime_invoke({resolvedMethod.MI_Name}, obj, {paramStr});".Indent(indent + 2));
        //await sw.WriteAsync($"obj->_ctor(".Indent(indent + 2));
        //for (int k = 0; k < resolvedParameters.Count; k++)
        //{
        //    await sw.WriteAsync(resolvedParameters[k].Name);
        //    if (k < resolvedParameters.Count - 1)
        //        await sw.WriteAsync(", ");
        //}
        //await sw.WriteLineAsync(");");
        await sw.WriteLineAsync("return obj;".Indent(indent + 2));
        await sw.WriteLineAsync("}".Indent(indent));
        await sw.WriteLineAsync();
      }
    }

    public override string ToHeaderCode(Int32 indent = 0) {
      GenerateMINames();
      StringBuilder stringBuilder = new StringBuilder();

      if (!isNested) {
        stringBuilder.Append("#pragma once\n\n".Indent(indent));
        // Namespace
        if (Namespace != "") {
          stringBuilder.Append($"namespace {CppNamespace()}\n".Indent(indent));
          stringBuilder.Append("{\n".Indent(indent));
          indent += 2;
        }
      }

      stringBuilder.Append($"// Class TypeDefinitionIndex: {typeDefinitionIndex}\n".Indent(indent));

      //if (typeDef.genericContainerIndex >= 0)
      //    code += $"{genericTemplate}\n".Indent(indent);
      string NestedNameStr = DeclarationString();
      //NestedNameStr = NestedNameStr.Substring(0, NestedNameStr.Length - 2);
      stringBuilder.Append($"struct {NestedNameStr}".Indent(indent));
      //code += $"struct {Name}".Indent(indent);
      if (parentType != null)
        stringBuilder.Append($" : {parentType.GetFullName()}");
      else
        stringBuilder.Append($" : Il2CppObject");
      stringBuilder.Append("\n");
      stringBuilder.Append("{\n".Indent(indent));

      if (nestedTypes.Count > 0) {
        for (int i = 0; i < nestedTypes.Count; i++) {
          stringBuilder.Append(nestedTypes[i].ForwardDeclaration(indent + 2));
          stringBuilder.Append("\n");
        }
      }

      if (typeDef.genericContainerIndex == -1) {

        if (instanceFields.Count > 0) {
          for (int i = 0; i < instanceFields.Count; i++) {
            stringBuilder.Append(instanceFields[i].ToCode(indent + 2));
          }
          stringBuilder.Append("\n");
        }
        if (instanceProperties.Count > 0) {
          stringBuilder.Append("// Instance Properties\n".Indent(indent + 2));
          for (int i = 0; i < instanceProperties.Count; i++) {
            stringBuilder.Append(instanceProperties[i].ToHeaderCode(indent + 2));
          }
          stringBuilder.Append("\n");
        }
        if (staticProperties.Count > 0) {
          stringBuilder.Append("// Static Properties\n".Indent(indent + 2));
          for (int i = 0; i < staticProperties.Count; i++) {
            stringBuilder.Append(staticProperties[i].ToHeaderCode(indent + 2));
          }
          stringBuilder.Append("\n");
        }
        if (instanceEvents.Count > 0) {
          stringBuilder.Append("// Instance Events\n".Indent(indent + 2));
          for (int i = 0; i < instanceEvents.Count; i++) {
            stringBuilder.Append(instanceEvents[i].ToCode(indent + 2));
          }
          stringBuilder.Append("\n");
        }
        if (staticEvents.Count > 0) {
          stringBuilder.Append("// Static Events\n".Indent(indent + 2));
          for (int i = 0; i < staticEvents.Count; i++) {
            stringBuilder.Append(staticEvents[i].ToCode(indent + 2));
          }
          stringBuilder.Append("\n");
        }
        if (instanceMethods.Count > 0) {
          stringBuilder.Append("// Instance Methods\n".Indent(indent + 2));
          for (int i = 0; i < instanceMethods.Count; i++) {
            stringBuilder.Append(instanceMethods[i].ToHeaderCode(indent + 2));
          }
          stringBuilder.Append("\n");
        }
        if (staticMethods.Count > 0) {
          stringBuilder.Append("// Static Methods\n".Indent(indent + 2));
          for (int i = 0; i < staticMethods.Count; i++) {
            stringBuilder.Append(staticMethods[i].ToHeaderCode(indent + 2));
          }
          stringBuilder.Append("\n");
        }
      }
      // TODO: Fields, static fields, const fields, properties, methods, (generics), nested types


      // il2cpp stuff
      stringBuilder.Append("\n");
      //stringBuilder.Append($"{Name}() {{}}\n".Indent(indent + 2));
      //stringBuilder.Append("\n");
      //stringBuilder.Append("\n");
      stringBuilder.Append("// il2cpp stuff\n".Indent(indent + 2));
      stringBuilder.Append("static ::Il2CppClass* il2cppClass;\n".Indent(indent + 2));
      stringBuilder.Append("static System::Type* il2cppObjectType;\n".Indent(indent + 2));
      stringBuilder.Append("// static fields\n".Indent(indent + 2));
      for (int i = 0; i < staticFields.Count; i++) {
        stringBuilder.Append($"static ::FieldInfo* FI_{staticFields[i].Name.CSharpToCppIdentifier()};\n".Indent(indent + 2));
      }
      stringBuilder.Append("\n");
      stringBuilder.Append("// methods\n".Indent(indent + 2));
      for (int i = 0; i < miMethods.Count; i++) {
        stringBuilder.Append($"static ::MethodInfo* {miMethods[i].MI_Name};\n".Indent(indent + 2));
      }
      stringBuilder.Append("static void il2cpp_init();\n".Indent(indent + 2));

      stringBuilder.Append("};\n".Indent(indent));

      if (Name == "ParticleSystem") {
        int nBurst = -1;
        int nMinMaxCurve = -1;
        for (int i = 0; i < nestedTypes.Count; i++) {
          if (nestedTypes[i].Name == "Burst")
            nBurst = i;
          else if (nestedTypes[i].Name == "MinMaxCurve")
            nMinMaxCurve = i;
        }

        if (nMinMaxCurve >= 0 && nBurst >= 0 && nMinMaxCurve > nBurst) {
          ResolvedType resolvedType = nestedTypes[nMinMaxCurve];
          nestedTypes[nMinMaxCurve] = nestedTypes[nBurst];
          nestedTypes[nBurst] = resolvedType;
        }
      }

      if (nestedTypes.Count > 0) {
        for (int i = 0; i < nestedTypes.Count; i++) {
          stringBuilder.Append(nestedTypes[i].ToHeaderCode(indent));
          stringBuilder.Append("\n");
        }
      }

      // Namespace
      if (Namespace != "" && !isNested) {
        indent -= 2;
        stringBuilder.Append("}\n".Indent(indent));
      }

      return stringBuilder.ToString();
    }

    //public override string ToHeaderCode(Int32 indent = 0)
    //{
    //    GenerateMINames();
    //    string code = "";

    //    if (!isNested)
    //    {
    //        code += "#pragma once\n\n".Indent(indent);
    //        // Namespace
    //        if (Namespace != "")
    //        {
    //            code += $"namespace {CppNamespace()}\n".Indent(indent);
    //            code += "{\n".Indent(indent);
    //            indent += 2;
    //        }
    //    }

    //    code += $"// Class TypeDefinitionIndex: {typeDefinitionIndex}\n".Indent(indent);

    //    //if (typeDef.genericContainerIndex >= 0)
    //    //    code += $"{genericTemplate}\n".Indent(indent);
    //    string NestedNameStr = DeclarationString();
    //    //NestedNameStr = NestedNameStr.Substring(0, NestedNameStr.Length - 2);
    //    code += $"struct {NestedNameStr}".Indent(indent);
    //    //code += $"struct {Name}".Indent(indent);
    //    if (parentType != null)
    //        code += $" : {parentType.GetFullName()}";
    //    else
    //        code += $" : Il2CppObject";
    //    code += "\n";
    //    code += "{\n".Indent(indent);

    //    if (nestedTypes.Count > 0)
    //    {
    //        for (int i = 0; i < nestedTypes.Count; i++)
    //        {
    //            code += nestedTypes[i].ForwardDeclaration(indent + 2);
    //            code += "\n";
    //        }
    //    }

    //    if (typeDef.genericContainerIndex == -1)
    //    {

    //        if (instanceFields.Count > 0)
    //        {
    //            for (int i = 0; i < instanceFields.Count; i++)
    //            {
    //                code += instanceFields[i].ToCode(indent + 2);
    //            }
    //            code += "\n";
    //        }
    //        if (instanceProperties.Count > 0)
    //        {
    //            code += "// Instance Properties\n".Indent(indent + 2);
    //            for (int i = 0; i < instanceProperties.Count; i++)
    //            {
    //                code += instanceProperties[i].ToCode(indent + 2);
    //            }
    //            code += "\n";
    //        }
    //        if (staticProperties.Count > 0)
    //        {
    //            code += "// Static Properties\n".Indent(indent + 2);
    //            for (int i = 0; i < staticProperties.Count; i++)
    //            {
    //                code += staticProperties[i].ToCode(indent + 2);
    //            }
    //            code += "\n";
    //        }
    //        if (instanceEvents.Count > 0)
    //        {
    //            code += "// Instance Events\n".Indent(indent + 2);
    //            for (int i = 0; i < instanceEvents.Count; i++)
    //            {
    //                code += instanceEvents[i].ToCode(indent + 2);
    //            }
    //            code += "\n";
    //        }
    //        if (staticEvents.Count > 0)
    //        {
    //            code += "// Static Events\n".Indent(indent + 2);
    //            for (int i = 0; i < staticEvents.Count; i++)
    //            {
    //                code += staticEvents[i].ToCode(indent + 2);
    //            }
    //            code += "\n";
    //        }
    //        if (instanceMethods.Count > 0)
    //        {
    //            code += "// Instance Methods\n".Indent(indent + 2);
    //            for (int i = 0; i < instanceMethods.Count; i++)
    //            {
    //                code += instanceMethods[i].ToHeaderCode(indent + 2);
    //            }
    //            code += "\n";
    //        }
    //        if (staticMethods.Count > 0)
    //        {
    //            code += "// Static Methods\n".Indent(indent + 2);
    //            for (int i = 0; i < staticMethods.Count; i++)
    //            {
    //                code += staticMethods[i].ToHeaderCode(indent + 2);
    //            }
    //            code += "\n";
    //        }
    //    }
    //    // TODO: Fields, static fields, const fields, properties, methods, (generics), nested types


    //    // il2cpp stuff
    //    code += "\n";
    //    code += "// il2cpp stuff\n".Indent(indent + 2);
    //    code += "static Il2CppClass* il2cppClass;\n".Indent(indent + 2);
    //    code += "// methods\n".Indent(indent + 2);
    //    for (int i = 0; i < miMethods.Count; i++)
    //    {
    //        code += $"static ::MethodInfo* {miMethods[i].MI_Name};\n".Indent(indent + 2);
    //    }
    //    code += "static void il2cpp_init();\n".Indent(indent + 2);

    //    code += "};\n".Indent(indent);

    //    if (nestedTypes.Count > 0)
    //    {
    //        for (int i = 0; i < nestedTypes.Count; i++)
    //        {
    //            code += nestedTypes[i].ToHeaderCode(indent);
    //            code += "\n";
    //        }
    //    }

    //    // Namespace
    //    if (Namespace != "" && !isNested)
    //    {
    //        indent -= 2;
    //        code += "}\n".Indent(indent);
    //    }

    //    return code;
    //}

    private void MakeGenericTemplate() {
      Il2CppGenericContainer generic_container = Metadata.genericContainers[typeDef.genericContainerIndex];

      genericTemplate = "template <";
      for (int i = 0; i < generic_container.type_argc; i++) {
        Il2CppGenericParameter generic_parameter = Metadata.genericParameters[generic_container.genericParameterStart + i];
        genericTemplate += $"typename {MetadataReader.GetString(generic_parameter.nameIndex)}";
        if (i < generic_container.type_argc - 1)
          genericTemplate += ", ";
      }
      genericTemplate += ">";
    }

    public override string DemangledPrefix() {
      if (parentType == Demangler.multicastDelegateType)
        return base.DemangledPrefix() + "Callback";

      if (this.isCoroutine)
        return base.DemangledPrefix() + "Coroutine";

      return base.DemangledPrefix() + "Class";
    }

    public void GenerateMINames() {
      Dictionary<string, List<ResolvedMethod>> mapMIMethods = new Dictionary<string, List<ResolvedMethod>>();
      for (int i = 0; i < miMethods.Count; i++) {
        if (!mapMIMethods.TryGetValue(miMethods[i].Name, out var methodList)) {
          methodList = new List<ResolvedMethod>();
          mapMIMethods.Add(miMethods[i].Name, methodList);
        }

        methodList.Add(miMethods[i]);
      }

      foreach (var pair in mapMIMethods) {
        List<ResolvedMethod> methods = pair.Value;
        //string prefix = pair.Key;

        if (methods.Count == 1) {
          methods[0].MI_Name = $"MI_{methods[0].Name.CSharpToCppIdentifier()}";
          continue;
        }

        for (int i = 0; i < methods.Count; i++) {
          methods[i].MI_Name = $"MI_{methods[i].Name.CSharpToCppIdentifier()}_{i + 1}";
        }
      }
    }

    private static int SortMethods(ResolvedMethod m1, ResolvedMethod m2) {
      if (m1 == null) {
        // If m1 is null and y is null, they're
        // equal.
        if (m2 == null)
          return 0;

        // If m1 is null and y is not null, y
        // is greater.
        return -1;
      }

      // If m1 is not null and m2 is null, m1 is greater.
      if (m2 == null)
        return 1;

      if (m1.isMangled && !m2.isMangled)
        return 1;
      else if (!m1.isMangled && m2.isMangled)
        return -1;

      //if (m1.Name.StartsWith("um") && !m2.Name.StartsWith("um"))
      //    return 1;
      //else if (!m1.Name.StartsWith("um") && m2.Name.StartsWith("um"))
      //    return -1;

      // Sort strings with ordinary string comparison.
      return m1.Name.CompareTo(m2.Name);
    }

    public override void Demangle(bool force = false) {
      if (isDemangled) {
        base.Demangle(force);
        return;
      }

      if (Name == "Player") {
        //System.Threading.ThreadPool
      }

      base.Demangle(force);

      Dictionary<string, Int32> demangledPrefixes = new Dictionary<string, Int32>();
      if (parentType != null) {
        // If type inherits make sure parent is demangled first
        if (!Demangler.demangledTypes.TryGetValue(parentType, out var demangledPrefixesParent)) {
          if (!Demangler.demangleQueue.TryGetValue(parentType, out var list)) {
            list = new List<ResolvedType>();
            Demangler.demangleQueue.Add(parentType, list);
          }
          list.Add(this);
          return;
        }
        // Make copy so other inheriting classes can still use parent dictionary
        demangledPrefixes = new Dictionary<string, int>(demangledPrefixesParent);
      }
      // Demangle here

      // Demangle fields
      for (int i = 0; i < instanceFields.Count; i++) {
        ResolvedField resolvedField = instanceFields[i];
        if (!resolvedField.isMangled)
          continue;

        if (!force && resolvedField.Name.isCSharpIdentifier()) {
          if (resolvedField.Name.isCppIdentifier())
            continue;

          // @TODO: remove backingfield
          resolvedField.Name = resolvedField.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
          continue;
        }

        string demangledPrefix = resolvedField.DemangledPrefix();
        if (!demangledPrefix.EndsWith("_"))
          demangledPrefix += "_";
        if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx)) {
          idx = 0;
          demangledPrefixes.Add(demangledPrefix, idx);
        }
        idx++;
        resolvedField.Name = $"{demangledPrefix}{idx}";
        demangledPrefixes[demangledPrefix] = idx;
        //code += instanceFields[i].ToCode(indent + 2);
      }
      for (int i = 0; i < staticFields.Count; i++) {
        ResolvedField resolvedField = staticFields[i];
        if (!resolvedField.isMangled)
          continue;
        if (!force && resolvedField.Name.isCSharpIdentifier()) {
          if (resolvedField.Name.isCppIdentifier())
            continue;

          resolvedField.Name = resolvedField.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
          continue;
        }

        string demangledPrefix = resolvedField.DemangledPrefix();
        if (!demangledPrefix.EndsWith("_"))
          demangledPrefix += "_";
        if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx)) {
          idx = 0;
          demangledPrefixes.Add(demangledPrefix, idx);
        }
        idx++;
        resolvedField.Name = $"{demangledPrefix}{idx}";
        demangledPrefixes[demangledPrefix] = idx;
        //code += instanceFields[i].ToCode(indent + 2);
      }


      // Demangle properties
      for (int i = 0; i < instanceProperties.Count; i++) {
        ResolvedProperty resolvedProperty = instanceProperties[i];
        if (!resolvedProperty.isMangled)
          continue;
        if (!force && resolvedProperty.Name.isCSharpIdentifier()) {
          if (resolvedProperty.Name.isCppIdentifier())
            continue;

          resolvedProperty.EnsureCppMethodNames();
          resolvedProperty.Name = resolvedProperty.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
          continue;
        }

        string demangledPrefix = resolvedProperty.DemangledPrefix();
        if (!demangledPrefix.EndsWith("_"))
          demangledPrefix += "_";
        if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx)) {
          idx = 0;
          demangledPrefixes.Add(demangledPrefix, idx);
        }
        idx++;
        resolvedProperty.Name = $"{demangledPrefix}{idx}";
        demangledPrefixes[demangledPrefix] = idx;
        resolvedProperty.DemangleMethods();
      }

      for (int i = 0; i < staticProperties.Count; i++) {
        ResolvedProperty resolvedProperty = staticProperties[i];
        if (!resolvedProperty.isMangled)
          continue;
        if (!force && resolvedProperty.Name.isCSharpIdentifier()) {
          if (resolvedProperty.Name.isCppIdentifier())
            continue;

          resolvedProperty.EnsureCppMethodNames();
          resolvedProperty.Name = resolvedProperty.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
          continue;
        }

        string demangledPrefix = resolvedProperty.DemangledPrefix();
        if (!demangledPrefix.EndsWith("_"))
          demangledPrefix += "_";
        if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx)) {
          idx = 0;
          demangledPrefixes.Add(demangledPrefix, idx);
        }
        idx++;
        resolvedProperty.Name = $"{demangledPrefix}{idx}";
        demangledPrefixes[demangledPrefix] = idx;
        resolvedProperty.DemangleMethods();
      }
      // Demangle Events
      for (int i = 0; i < instanceEvents.Count; i++) {
        ResolvedEvent resolvedEvent = instanceEvents[i];
        if (!resolvedEvent.isMangled)
          continue;
        if (!force && resolvedEvent.Name.isCSharpIdentifier()) {
          if (resolvedEvent.Name.isCppIdentifier())
            continue;

          resolvedEvent.Name = resolvedEvent.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
          continue;
        }

        string demangledPrefix = resolvedEvent.DemangledPrefix();
        if (!demangledPrefix.EndsWith("_"))
          demangledPrefix += "_";
        if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx)) {
          idx = 0;
          demangledPrefixes.Add(demangledPrefix, idx);
        }
        idx++;
        resolvedEvent.Name = $"{demangledPrefix}{idx}";
        demangledPrefixes[demangledPrefix] = idx;
        resolvedEvent.DemangleMethods();
      }
      for (int i = 0; i < staticEvents.Count; i++) {
        ResolvedEvent resolvedEvent = staticEvents[i];
        if (!resolvedEvent.isMangled)
          continue;
        if (!force && resolvedEvent.Name.isCSharpIdentifier()) {
          if (resolvedEvent.Name.isCppIdentifier())
            continue;

          resolvedEvent.Name = resolvedEvent.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
          continue;
        }

        string demangledPrefix = resolvedEvent.DemangledPrefix();
        if (!demangledPrefix.EndsWith("_"))
          demangledPrefix += "_";
        if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx)) {
          idx = 0;
          demangledPrefixes.Add(demangledPrefix, idx);
        }
        idx++;
        resolvedEvent.Name = $"{demangledPrefix}{idx}";
        demangledPrefixes[demangledPrefix] = idx;
        resolvedEvent.DemangleMethods();
      }

      // Demangle Methods
      for (int i = 0; i < instanceMethods.Count; i++) {
        ResolvedMethod resolvedMethod = instanceMethods[i];
        resolvedMethod.DemangleParams();
        if (!resolvedMethod.isMangled)
          continue;
        if (!force && resolvedMethod.Name.isCSharpIdentifier()) {
          if (resolvedMethod.Name.isCppIdentifier())
            continue;

          resolvedMethod.Name = resolvedMethod.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
          continue;
        }

        string demangledPrefix = resolvedMethod.DemangledPrefix();
        if (!demangledPrefix.EndsWith("_"))
          demangledPrefix += "_";
        if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx)) {
          idx = 0;
          demangledPrefixes.Add(demangledPrefix, idx);
        }
        idx++;
        resolvedMethod.Name = $"{demangledPrefix}{idx}";
        demangledPrefixes[demangledPrefix] = idx;
      }

      for (int i = 0; i < staticMethods.Count; i++) {
        ResolvedMethod resolvedMethod = staticMethods[i];
        resolvedMethod.DemangleParams();
        if (!resolvedMethod.isMangled)
          continue;
        if (!force && resolvedMethod.Name.isCSharpIdentifier()) {
          if (resolvedMethod.Name.isCppIdentifier())
            continue;

          resolvedMethod.Name = resolvedMethod.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
          continue;
        }

        string demangledPrefix = resolvedMethod.DemangledPrefix();
        if (!demangledPrefix.EndsWith("_"))
          demangledPrefix += "_";
        if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx)) {
          idx = 0;
          demangledPrefixes.Add(demangledPrefix, idx);
        }
        idx++;
        resolvedMethod.Name = $"{demangledPrefix}{idx}";
        demangledPrefixes[demangledPrefix] = idx;
      }

      instanceMethods.Sort(SortMethods);
      staticMethods.Sort(SortMethods);

      Rules.AssignResolvedObject(this);

      isDemangled = true;
      // Add type as demangled
      Demangler.demangledTypes.Add(this, demangledPrefixes);
      // Resolve all types waiting for this type
      if (!Demangler.demangleQueue.TryGetValue(this, out var demangleQueue))
        return;
      // remove from queue
      Demangler.demangleQueue.Remove(this);

      for (int i = 0; i < demangleQueue.Count; i++) {
        demangleQueue[i].Demangle(force);
      }
    }

    public override async Task ToCppCode(StreamWriter sw, Int32 indent = 0) {
      if (!isNested) {
        await sw.WriteAsync("#include \"pch.h\"\n\n");
        //code = code.Indent(indent);
        //code += "#include \"pch.h\"\n\n";

        // Namespace
        if (Namespace != "") {
          await sw.WriteAsync($"namespace {CppNamespace()}\n".Indent(indent));
          //code += $"namespace {CppNamespace()}\n".Indent(indent);
          await sw.WriteAsync("{\n".Indent(indent));
          //code += "{\n".Indent(indent);
          indent += 2;
        }
      }
      // TODO: Init any static fields
      string NestedNameStr = NestedName();

      await sw.WriteAsync($"bool {NestedNameStr}_il2cpp_initialized = false;\n".Indent(indent));
      await sw.WriteAsync($"Il2CppClass* {NestedNameStr}il2cppClass = nullptr;\n".Indent(indent));
      await sw.WriteAsync($"System::Type* {NestedNameStr}il2cppObjectType = nullptr;\n".Indent(indent));
      //code += $"Il2CppClass* {NestedNameStr}il2cppClass = nullptr;\n".Indent(indent);
      for (int i = 0; i < miMethods.Count; i++) {
        await sw.WriteAsync($"::MethodInfo* {NestedNameStr}{miMethods[i].MI_Name} = nullptr;\n".Indent(indent));
      }

      await sw.WriteAsync("// static fields\n".Indent(indent));
      for (int i = 0; i < staticFields.Count; i++) {
        await sw.WriteAsync($"::FieldInfo* {NestedNameStr}FI_{staticFields[i].Name.CSharpToCppIdentifier()} = nullptr;\n".Indent(indent));
      }

      if (typeDef.genericContainerIndex == -1) {
        for (int i = 0; i < instanceProperties.Count; i++) {
          await instanceProperties[i].ToCppCode(sw, indent);
        }

        for (int i = 0; i < staticProperties.Count; i++) {
          await staticProperties[i].ToCppCode(sw, indent);
        }
        //code += "\n";

        // methods
        for (int i = 0; i < instanceMethods.Count; i++) {
          await instanceMethods[i].ToCppCode(sw, indent);
        }

        // methods
        for (int i = 0; i < staticMethods.Count; i++) {
          await staticMethods[i].ToCppCode(sw, indent);
        }

        for (int i = 0; i < instanceEvents.Count; i++) {
          await instanceEvents[i].ToCppCode(sw, indent);
        }
      }
      // TODO: nested types
      for (int i = 0; i < nestedTypes.Count; i++) {
        if (nestedTypes[i].GetType() == typeof(ResolvedInterface)) {
          continue;
        }

        await nestedTypes[i].ToCppCode(sw, indent + 2);
        await sw.WriteAsync("\n");
      }

      // il2cpp_init
      await sw.WriteAsync($"void {NestedNameStr}il2cpp_init()\n".Indent(indent));
      await sw.WriteAsync("{\n".Indent(indent));
      indent += 2;
      await sw.WriteAsync("if(_il2cpp_initialized) {;\n".Indent(indent));
      indent += 2;
      await sw.WriteAsync($"Console::cwprintf(COLOR_LIGHT_RED, L\"{NestedNameStr}il2cpp_init() already initialized!\");\n".Indent(indent));
      await sw.WriteAsync("return;\n".Indent(indent));
      indent -= 2;
      await sw.WriteAsync("}\n".Indent(indent));
      // TODO: grab right type from typedef
      await sw.WriteAsync($"il2cppClass = il2cpp::class_from_il2cpp_type(il2cpp::m_pMetadataRegistration->types[{typeDef.byvalTypeIndex}]);\n".Indent(indent));
      await sw.WriteAsync("il2cpp::runtime_class_init(il2cppClass);\n".Indent(indent));

      await sw.WriteAsync("if (il2cppClass->has_initialization_error == 1) {\n".Indent(indent));
      await sw.WriteAsync($"Console::cwprintf(COLOR_LIGHT_RED, L\"{NestedNameStr}il2cpp_init() failed\");\n".Indent(indent + 2));
      await sw.WriteAsync("return;\n".Indent(indent + 2));
      await sw.WriteAsync("}\n".Indent(indent));

      await sw.WriteAsync("void* iter = nullptr;\n".Indent(indent));
      await sw.WriteAsync("il2cpp::class_get_methods(il2cppClass, &iter);\n".Indent(indent));
      await sw.WriteAsync("iter = nullptr;\n".Indent(indent));
      await sw.WriteAsync("il2cpp::class_get_fields(il2cppClass, &iter);\n".Indent(indent));
      // Static Fields
      for (int i = 0; i < staticFields.Count; i++) {
        await sw.WriteAsync($"FI_{staticFields[i].Name.CSharpToCppIdentifier()} = (::FieldInfo*)il2cppClass->fields + {staticFields[i].fieldIndex};\n".Indent(indent));
      }
      // Methods
      for (int i = 0; i < miMethods.Count; i++) {
        await sw.WriteAsync($"{miMethods[i].MI_Name} = (::MethodInfo*)il2cppClass->methods[{miMethods[i].methodIndex - typeDef.methodStart}];\n".Indent(indent));
      }
      await sw.WriteAsync("il2cppObjectType = (System::Type*)malloc(sizeof(Il2CppReflectionType));\n".Indent(indent));
      await sw.WriteAsync("memcpy(il2cppObjectType, il2cpp::class_get_object(il2cppClass), sizeof(Il2CppReflectionType));\n".Indent(indent));

      // Init nested types
      for (int i = 0; i < nestedTypes.Count; i++) {
        if (!nestedTypes[i].isClass && !nestedTypes[i].isStruct)
          continue;

        await sw.WriteAsync($"{nestedTypes[i].NestedName()}il2cpp_init();\n".Indent(indent));
      }
      await sw.WriteAsync("_il2cpp_initialized = true;;\n".Indent(indent));
      indent -= 2;
      await sw.WriteAsync("}\n".Indent(indent));

      // end of Namespace
      if (Namespace != "" && !isNested) {
        indent -= 2;
        await sw.WriteAsync("}\n".Indent(indent));
      }
    }

    public override string ToCppCode(Int32 indent = 0) {
      StringBuilder stringBuilder = new StringBuilder();

      if (!isNested) {
        stringBuilder.Append("#include \"pch.h\"\n\n");
        //code = code.Indent(indent);
        //code += "#include \"pch.h\"\n\n";

        // Namespace
        if (Namespace != "") {
          stringBuilder.Append($"namespace {CppNamespace()}\n".Indent(indent));
          //code += $"namespace {CppNamespace()}\n".Indent(indent);
          stringBuilder.Append("{\n".Indent(indent));
          //code += "{\n".Indent(indent);
          indent += 2;
        }
      }
      // TODO: Init any static fields
      string NestedNameStr = NestedName();
      stringBuilder.Append($"Il2CppClass* {NestedNameStr}il2cppClass = nullptr;\n".Indent(indent));
      stringBuilder.Append($"System::Type* {NestedNameStr}il2cppObjectType = nullptr;\n".Indent(indent));
      //code += $"Il2CppClass* {NestedNameStr}il2cppClass = nullptr;\n".Indent(indent);
      for (int i = 0; i < miMethods.Count; i++) {
        stringBuilder.Append($"::MethodInfo* {NestedNameStr}{miMethods[i].MI_Name} = nullptr;\n".Indent(indent));
      }

      stringBuilder.Append("// static fields\n".Indent(indent));
      for (int i = 0; i < staticFields.Count; i++) {
        stringBuilder.Append($"::FieldInfo* {NestedNameStr}FI_{staticFields[i].Name.CSharpToCppIdentifier()} = nullptr;\n".Indent(indent));
      }

      if (typeDef.genericContainerIndex == -1) {
        for (int i = 0; i < instanceProperties.Count; i++) {
          stringBuilder.Append(instanceProperties[i].ToCppCode(indent));
        }

        for (int i = 0; i < staticProperties.Count; i++) {
          stringBuilder.Append(staticProperties[i].ToCppCode(indent));
        }
        //code += "\n";

        // methods
        for (int i = 0; i < instanceMethods.Count; i++) {
          stringBuilder.Append(instanceMethods[i].ToCppCode(indent));
        }

        // methods
        for (int i = 0; i < staticMethods.Count; i++) {
          stringBuilder.Append(staticMethods[i].ToCppCode(indent));
        }

        for (int i = 0; i < instanceEvents.Count; i++) {
          stringBuilder.Append(instanceEvents[i].ToCppCode(indent));
        }
      }
      // TODO: nested types
      for (int i = 0; i < nestedTypes.Count; i++) {
        stringBuilder.Append(nestedTypes[i].ToCppCode(indent + 2));
        stringBuilder.Append("\n");
      }

      // il2cpp_init
      stringBuilder.Append($"void {NestedNameStr}il2cpp_init()\n".Indent(indent));
      stringBuilder.Append("{\n".Indent(indent));
      // TODO: grab right type from typedef
      stringBuilder.Append($"il2cppClass = il2cpp::class_from_il2cpp_type(il2cpp::m_pMetadataRegistration->types[{typeDef.byvalTypeIndex}]);\n".Indent(indent + 2));
      stringBuilder.Append("il2cpp::runtime_class_init(il2cppClass);\n".Indent(indent + 2));
      stringBuilder.Append("void* iter = nullptr;\n".Indent(indent + 2));
      stringBuilder.Append("il2cpp::class_get_methods(il2cppClass, &iter);\n".Indent(indent + 2));
      stringBuilder.Append("iter = nullptr;\n".Indent(indent + 2));
      stringBuilder.Append("il2cpp::class_get_fields(il2cppClass, &iter);\n".Indent(indent + 2));
      // Static Fields
      for (int i = 0; i < staticFields.Count; i++) {
        stringBuilder.Append($"FI_{staticFields[i].Name.CSharpToCppIdentifier()} = (::FieldInfo*)il2cppClass->fields + {staticFields[i].fieldIndex};\n".Indent(indent + 2));
      }
      // Methods
      for (int i = 0; i < miMethods.Count; i++) {
        stringBuilder.Append($"{miMethods[i].MI_Name} = (::MethodInfo*)il2cppClass->methods[{miMethods[i].methodIndex - typeDef.methodStart}];\n".Indent(indent + 2));
      }
      stringBuilder.Append("il2cppObjectType = (System::Type*)malloc(sizeof(Il2CppReflectionType));\n".Indent(indent + 2));
      stringBuilder.Append("memcpy(il2cppObjectType, il2cpp::class_get_object(il2cppClass), sizeof(Il2CppReflectionType));\n".Indent(indent + 2));

      stringBuilder.Append("}\n".Indent(indent));

      // end of Namespace
      if (Namespace != "" && !isNested) {
        indent -= 2;
        stringBuilder.Append("}\n".Indent(indent));
      }

      return stringBuilder.ToString();
    }
    //public override string ToCppCode(Int32 indent = 0)
    //{
    //    string code = "";
    //    if (!isNested)
    //    {
    //        code = code.Indent(indent);
    //        code += "#include \"pch.h\"\n\n";
    //        // Namespace
    //        if (Namespace != "")
    //        {
    //            code += $"namespace {CppNamespace()}\n".Indent(indent);
    //            code += "{\n".Indent(indent);
    //            indent += 2;
    //        }
    //    }
    //    // TODO: Init any static fields
    //    string NestedNameStr = NestedName();
    //    code += $"Il2CppClass* {NestedNameStr}il2cppClass = nullptr;\n".Indent(indent);
    //    for (int i = 0; i < miMethods.Count; i++)
    //    {
    //        code += $"::MethodInfo* {NestedNameStr}{miMethods[i].MI_Name} = nullptr;\n".Indent(indent);
    //    }

    //    if (typeDef.genericContainerIndex == -1)
    //    {
    //        // methods
    //        for (int i = 0; i < instanceMethods.Count; i++)
    //        {
    //            code += instanceMethods[i].ToCppCode(indent);
    //        }
    //    }
    //    // TODO: nested types
    //    for (int i = 0; i < nestedTypes.Count; i++)
    //    {
    //        code += nestedTypes[i].ToCppCode(indent + 2);
    //        code += "\n";
    //    }

    //    // il2cpp_init
    //    code += $"void {NestedNameStr}il2cpp_init()\n".Indent(indent);
    //    code += "{\n".Indent(indent);
    //    // TODO: grab right type from typedef
    //    code += $"il2cppClass = il2cpp::class_from_il2cpp_type(il2cpp::m_pMetadataRegistration->types[{typeDef.byvalTypeIndex}]);\n".Indent(indent + 2);
    //    code += "il2cpp::runtime_class_init(il2cppClass);\n".Indent(indent + 2);
    //    // Methods
    //    for (int i = 0; i < miMethods.Count; i++)
    //    {
    //        code += $"{miMethods[i].MI_Name} = (::MethodInfo*)il2cppClass->methods[{miMethods[i].methodIndex - typeDef.methodStart}];\n".Indent(indent + 2);
    //    }
    //    code += "}\n".Indent(indent);
    //    // end of Namespace
    //    if (Namespace != "" && !isNested)
    //    {
    //        indent -= 2;
    //        code += "}\n".Indent(indent);
    //    }

    //    return code;
    //}

    internal override void ResolveOverrides() {
      for (int i = 0; i < miMethods.Count; i++) {
        if (!miMethods[i].isOverride)
          continue;

        miMethods[i].ResolveOverride();
      }
    }

    public override string ForwardDeclaration(Int32 indent = 0) {
      return $"struct {Name};\n".Indent(indent);
    }

    public override string ReturnTypeString(bool nestedCall = false) {
      //if (isMangled)
      //    return "void";

      return base.ReturnTypeString(nestedCall);
    }

    public override string GetFullName() {
      //if (isMangled)
      //    return "void";

      return base.GetFullName();
    }

    async Task GameObjectExtensions(StreamWriter sw, Int32 indent) {
      await sw.WriteLineAsync("template <typename T>".Indent(indent));
      await sw.WriteLineAsync("T* AddComponent() {".Indent(indent));
      indent += 2;
      await sw.WriteLineAsync("return (T*)this->AddComponent(T::il2cppObjectType);".Indent(indent));
      indent -= 2;
      await sw.WriteLineAsync("}".Indent(indent));

      await sw.WriteLineAsync();
      await sw.WriteLineAsync("template <typename T>".Indent(indent));
      await sw.WriteLineAsync("T* GetComponent() {".Indent(indent));
      indent += 2;
      await sw.WriteLineAsync("return (T*)this->GetComponent(T::il2cppObjectType);".Indent(indent));
      indent -= 2;
      await sw.WriteLineAsync("}".Indent(indent));

      //GetComponentInChildren(System::Type * type, bool includeInactive)
      await sw.WriteLineAsync();
      await sw.WriteLineAsync("template <typename T>".Indent(indent));
      await sw.WriteLineAsync("T* GetComponentInChildren(bool includeInactive = false) {".Indent(indent));
      indent += 2;
      await sw.WriteLineAsync("return (T*)this->GetComponentInChildren(T::il2cppObjectType, includeInactive);".Indent(indent));
      indent -= 2;
      await sw.WriteLineAsync("}".Indent(indent));

      //::Array<UnityEngine::Component*>* GetComponentsInChildren(System::Type* type, bool includeInactive);
      await sw.WriteLineAsync();
      await sw.WriteLineAsync("template <typename T>".Indent(indent));
      await sw.WriteLineAsync("::Array<T*>* GetComponentsInChildren(bool includeInactive = false) {".Indent(indent));
      indent += 2;
      await sw.WriteLineAsync("return (::Array<T*>*)this->GetComponentsInChildren(T::il2cppObjectType, includeInactive);".Indent(indent));
      indent -= 2;
      await sw.WriteLineAsync("}".Indent(indent));
    }

    async Task ComponentExtensions(StreamWriter sw, Int32 indent) {
      await sw.WriteLineAsync("template <typename T>".Indent(indent));
      await sw.WriteLineAsync("T* GetComponent() {".Indent(indent));
      indent += 2;
      await sw.WriteLineAsync("return (T*)this->GetComponent(T::il2cppObjectType);".Indent(indent));
      indent -= 2;
      await sw.WriteLineAsync("}".Indent(indent));

      //GetComponentInChildren(System::Type * type, bool includeInactive)
      await sw.WriteLineAsync();
      await sw.WriteLineAsync("template <typename T>".Indent(indent));
      await sw.WriteLineAsync("T* GetComponentInChildren(bool includeInactive = false) {".Indent(indent));
      indent += 2;
      await sw.WriteLineAsync("return (T*)this->GetComponentInChildren(T::il2cppObjectType, includeInactive);".Indent(indent));
      indent -= 2;
      await sw.WriteLineAsync("}".Indent(indent));

      //::Array<UnityEngine::Component*>* GetComponentsInChildren(System::Type* type, bool includeInactive);
      await sw.WriteLineAsync();
      await sw.WriteLineAsync("template <typename T>".Indent(indent));
      await sw.WriteLineAsync("::Array<T*>* GetComponentsInChildren(bool includeInactive = false) {".Indent(indent));
      indent += 2;
      await sw.WriteLineAsync("return (::Array<T*>*)this->GetComponentsInChildren(T::il2cppObjectType, includeInactive);".Indent(indent));
      indent -= 2;
      await sw.WriteLineAsync("}".Indent(indent));
    }
  }
}
