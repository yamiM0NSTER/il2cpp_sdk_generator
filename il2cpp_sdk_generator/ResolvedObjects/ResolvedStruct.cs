using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    class ResolvedStruct : ResolvedType
    {
        bool isGeneric = false;
        string genericTemplate = "";
        // Fields
        List<ResolvedField> instanceFields = new List<ResolvedField>();
        List<ResolvedField> staticFields = new List<ResolvedField>();
        List<ResolvedField> constFields = new List<ResolvedField>();
        // Properties
        List<ResolvedProperty> instanceProperties = new List<ResolvedProperty>();
        List<ResolvedProperty> staticProperties = new List<ResolvedProperty>();
        // Events
        List<ResolvedEvent> instanceEvents = new List<ResolvedEvent>();
        List<ResolvedEvent> staticEvents = new List<ResolvedEvent>();
        // Methods
        List<ResolvedMethod> instanceMethods = new List<ResolvedMethod>();
        List<ResolvedMethod> staticMethods = new List<ResolvedMethod>();

        public ResolvedStruct(Il2CppTypeDefinition type, Int32 idx)
        {
            typeDef = type;
            typeDefinitionIndex = idx;
            Name = MetadataReader.GetString(typeDef.nameIndex);
            Namespace = MetadataReader.GetString(typeDef.namespaceIndex);
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


            // Resolve fields
            // Resolve static fields
            // Resolve constants (if needed)
            for (int i = 0; i < typeDef.field_count; i++)
            {
                ResolvedField resolvedField = new ResolvedField(Metadata.fieldDefinitions[i + typeDef.fieldStart], this.typeDefinitionIndex, i);

                if (resolvedField.isStatic)
                    staticFields.Add(resolvedField);
                else if (resolvedField.isConst)
                    constFields.Add(resolvedField);
                else
                    instanceFields.Add(resolvedField);
            }

            // TODO: Detect unions

            // Resolve methods
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
                // For now skip generic methods
                if (methodDef.genericContainerIndex >= 0)
                    continue;

                ResolvedMethod resolvedMethod = new ResolvedMethod(methodDef, typeDef.methodStart + i);

                if (propertyMethods.TryGetValue(i, out var resolvedProperty))
                {
                    resolvedProperty.AssignMethod(i, resolvedMethod);
                    continue;
                }

                if (eventMethods.TryGetValue(i, out var resolvedEvent))
                {
                    resolvedEvent.AssignMethod(i, resolvedMethod);
                    continue;
                }

                if (!resolvedMethod.isStatic)
                    instanceMethods.Add(resolvedMethod);
                else
                    staticMethods.Add(resolvedMethod);
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

            // Split events into instanced and static
            for (int i = 0; i < events.Length; i++)
            {
                if (events[i].isStatic)
                    staticEvents.Add(events[i]);
                else
                    instanceEvents.Add(events[i]);
            }

            Console.WriteLine("ResolvedStruct::Resolve()");
            isResolved = true;
        }

        public override string ToCode(Int32 indent = 0)
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

            code += $"// Struct TypeDefinitionIndex: {typeDefinitionIndex}\n".Indent(indent);

            if (typeDef.genericContainerIndex >= 0)
                code += $"{genericTemplate}\n".Indent(indent);
            code += $"struct {Name}".Indent(indent);
            if (parentType != null && !(parentType is ResolvedClass))
                code += $" : {parentType.GetFullName()}";
            code += "\n";
            code += "{\n".Indent(indent);

            if (instanceFields.Count > 0)
            {
                for (int i = 0; i < instanceFields.Count; i++)
                {
                    code += instanceFields[i].ToCode(indent + 2);
                }
                code += "\n";
            }
            if (instanceProperties.Count > 0)
            {
                code += "// Instance Properties\n".Indent(indent + 2);
                for (int i = 0; i < instanceProperties.Count; i++)
                {
                    code += instanceProperties[i].ToCode(indent + 2);
                }
                code += "\n";
            }
            if (staticProperties.Count > 0)
            {
                code += "// Static Properties\n".Indent(indent + 2);
                for (int i = 0; i < staticProperties.Count; i++)
                {
                    code += staticProperties[i].ToCode(indent + 2);
                }
                code += "\n";
            }
            if (instanceEvents.Count > 0)
            {
                code += "// Instance Events\n".Indent(indent + 2);
                for (int i = 0; i < instanceEvents.Count; i++)
                {
                    code += instanceEvents[i].ToCode(indent + 2);
                }
                code += "\n";
            }
            if (staticEvents.Count > 0)
            {
                code += "// Static Events\n".Indent(indent + 2);
                for (int i = 0; i < staticEvents.Count; i++)
                {
                    code += staticEvents[i].ToCode(indent + 2);
                }
                code += "\n";
            }
            if (instanceMethods.Count > 0)
            {
                code += "// Instance Methods\n".Indent(indent + 2);
                for (int i = 0; i < instanceMethods.Count; i++)
                {
                    code += instanceMethods[i].ToCode(indent + 2);
                }
                code += "\n";
            }
            if (staticMethods.Count > 0)
            {
                code += "// Static Methods\n".Indent(indent + 2);
                for (int i = 0; i < staticMethods.Count; i++)
                {
                    code += staticMethods[i].ToCode(indent + 2);
                }
                code += "\n";
            }

            // TODO: Fields, static fields, const fields, properties, methods, (generics), nested types
            if (nestedTypes.Count > 0)
            {
                for (int i = 0; i < nestedTypes.Count; i++)
                {
                    code += nestedTypes[i].ToCode(indent + 2);
                    code += "\n";
                }
            }


            code += "};\n".Indent(indent);

            // Namespace
            if (Namespace != "" && !isNested)
            {
                indent -= 2;
                code += "}\n".Indent(indent);
            }

            return code;
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
            return GetVisibility() + "Struct";
        }
    }
}
