using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
  public class ResolvedInterface : ResolvedType
  {
    bool isGeneric = false;
    string genericTemplate = "";

    public ResolvedInterface(Il2CppTypeDefinition type, Int32 idx)
    {
      typeDef = type;
      typeDefinitionIndex = idx;
      Name = MetadataReader.GetString(typeDef.nameIndex);
      Namespace = MetadataReader.GetString(typeDef.namespaceIndex);
      isMangled = !Name.isCSharpIdentifier();
    }

    public override void Resolve()
    {
      if (isResolved)
        return;

      if (typeDef.genericContainerIndex >= 0)
      {
        isGeneric = true;
        if (Name.Contains('`'))
        {
          int idx = Name.IndexOf('`');
          Name = Name.Remove(idx, Name.Length - idx);
        }

        MakeGenericTemplate();
      }

      Dictionary<Int32, ResolvedProperty> propertyMethods = new Dictionary<int, ResolvedProperty>();
      ResolvedProperty[] properties = new ResolvedProperty[typeDef.property_count];
      // Resolve properties (and exclude getters/setters from methods?)
      for (int i = 0; i < typeDef.property_count; i++)
      {
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
      for (int i = 0; i < typeDef.event_count; i++)
      {
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
      for (int i = 0; i < typeDef.method_count; i++)
      {
        Il2CppMethodDefinition methodDef = Metadata.methodDefinitions[typeDef.methodStart + i];

        if (typeDef.genericContainerIndex < 0 && methodDef.genericContainerIndex >= 0)
        {

        }

        // For now skip generic methods
        if (methodDef.genericContainerIndex >= 0)
          continue;

        ResolvedMethod resolvedMethod = new ResolvedMethod(methodDef, typeDef.methodStart + i, this);
        miMethods.Add(resolvedMethod);
        if (methodDef.slot != il2cpp_Constants.kInvalidIl2CppMethodSlot)
          slottedMethods.Add(methodDef.slot, resolvedMethod);
        if (propertyMethods.TryGetValue(i, out var resolvedProperty))
        {
          resolvedMethod.isReferenced = true;
          resolvedProperty.AssignMethod(i, resolvedMethod);
          continue;
        }

        if (eventMethods.TryGetValue(i, out var resolvedEvent))
        {
          resolvedMethod.isReferenced = true;
          resolvedEvent.AssignMethod(i, resolvedMethod);
          continue;
        }

        if (!resolvedMethod.isStatic)
          instanceMethods.Add(resolvedMethod);
        else
        {
          staticMethods.Add(resolvedMethod);
        }

        //if (methodDef.genericContainerIndex >= 0 && name == "GetComponent")
        //    Console.WriteLine();
      }

      // Split properties into instanced and static
      for (int i = 0; i < properties.Length; i++)
      {
        if (properties[i].isStatic)
          staticProperties.Add(properties[i]);
        else
          instanceProperties.Add(properties[i]);
      }

      isResolved = true;
    }

    public override async Task ToHeaderCode(StreamWriter sw, Int32 indent = 0)
    {
      if (!isNested)
      {
        await sw.WriteAsync("#pragma once\n\n".Indent(indent));
        // Namespace
        if (Namespace != "")
        {
          await sw.WriteAsync($"namespace {CppNamespace()}\n".Indent(indent));
          await sw.WriteAsync("{\n".Indent(indent));
          indent += 2;
        }
      }

      await sw.WriteAsync($"// Interface TypeDefinitionIndex: {typeDefinitionIndex}\n".Indent(indent));

      //if (typeDef.genericContainerIndex >= 0)
      //    code += $"{genericTemplate}\n".Indent(indent);
      await sw.WriteAsync($"struct {Name}".Indent(indent));
      if (parentType != null)
        await sw.WriteAsync($" : {parentType.GetFullName()}");
      else
        await sw.WriteAsync($" : Il2CppObject");
      await sw.WriteAsync("\n");
      await sw.WriteAsync("{\n".Indent(indent));
      indent += 2;

      if (instanceProperties.Count > 0)
      {
        await sw.WriteLineAsync("// Instance Properties".Indent(indent));
        for (int i = 0; i < instanceProperties.Count; i++)
        {
          await instanceProperties[i].ToHeaderCode(sw, indent);
        }
        await sw.WriteLineAsync();
      }
      if (instanceMethods.Count > 0)
      {
        await sw.WriteLineAsync("// Instance Methods".Indent(indent));
        for (int i = 0; i < instanceMethods.Count; i++)
        {
          await instanceMethods[i].ToHeaderCode(sw, indent);
        }
        await sw.WriteLineAsync();
      }
      indent -= 2;
      await sw.WriteAsync("};\n".Indent(indent));

      // Namespace
      if (Namespace != "" && !isNested)
      {
        indent -= 2;
        await sw.WriteAsync("}\n".Indent(indent));
      }
    }

    public override string ToHeaderCode(Int32 indent = 0)
    {
      string code = "";

      if (!isNested)
      {
        code += "#pragma once\n\n".Indent(indent);
        // Namespace
        if (Namespace != "")
        {
          code += $"namespace {CppNamespace()}\n".Indent(indent);
          code += "{\n".Indent(indent);
          indent += 2;
        }
      }

      code += $"// Interface TypeDefinitionIndex: {typeDefinitionIndex}\n".Indent(indent);

      //if (typeDef.genericContainerIndex >= 0)
      //    code += $"{genericTemplate}\n".Indent(indent);
      code += $"struct {Name}".Indent(indent);
      if (parentType != null)
        code += $" : {parentType.GetFullName()}";
      else
        code += $" : Il2CppObject";
      code += "\n";
      code += "{\n".Indent(indent);

      code += "};\n".Indent(indent);

      // Namespace
      if (Namespace != "" && !isNested)
      {
        indent -= 2;
        code += "}\n".Indent(indent);
      }

      return code;
    }

    public override async Task ToCppCode(StreamWriter sw, Int32 indent = 0)
    {
      await Task.Delay(1);
    }

    private void MakeGenericTemplate()
    {
      Il2CppGenericContainer generic_container = Metadata.genericContainers[typeDef.genericContainerIndex];

      genericTemplate = "template <";
      for (int i = 0; i < generic_container.type_argc; i++)
      {
        Il2CppGenericParameter generic_parameter = Metadata.genericParameters[generic_container.genericParameterStart + i];
        genericTemplate += $"typename {MetadataReader.GetString(generic_parameter.nameIndex)}";
        if (i < generic_container.type_argc - 1)
          genericTemplate += ", ";
      }
      genericTemplate += ">";
    }

    public override string DemangledPrefix()
    {
      return GetVisibility() + "Interface";
    }

    public override string ToCppCode(Int32 indent = 0)
    {
      return "";
    }
  }
}
