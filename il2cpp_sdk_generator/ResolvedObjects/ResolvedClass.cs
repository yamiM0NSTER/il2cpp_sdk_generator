using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    public class ResolvedClass : ResolvedType
    {
        bool isGeneric = false;
        string genericTemplate = "";
        // Fields
        public List<ResolvedField> instanceFields = new List<ResolvedField>();
        public List<ResolvedField> staticFields = new List<ResolvedField>();
        public List<ResolvedField> constFields = new List<ResolvedField>();
        // Properties
        public List<ResolvedProperty> instanceProperties = new List<ResolvedProperty>();
        public List<ResolvedProperty> staticProperties = new List<ResolvedProperty>();
        // Events
        public List<ResolvedEvent> instanceEvents = new List<ResolvedEvent>();
        public List<ResolvedEvent> staticEvents = new List<ResolvedEvent>();
        // Methods
        public List<ResolvedMethod> instanceMethods = new List<ResolvedMethod>();
        public List<ResolvedMethod> staticMethods = new List<ResolvedMethod>();
        Dictionary<Int32, ResolvedMethod> slottedMethods = new Dictionary<Int32, ResolvedMethod>();
        public List<ResolvedMethod> miMethods = new List<ResolvedMethod>();

        public ResolvedClass(Il2CppTypeDefinition type, Int32 idx)
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

            // TODO: Just generate all existing generics as diffferent structs
            // Resolve generic name
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
            // Resolve constats (if needed)
            for (int i = 0; i < typeDef.field_count; i++)
            {
                ResolvedField resolvedField = new ResolvedField(Metadata.fieldDefinitions[i + typeDef.fieldStart], this.typeDefinitionIndex, i);

                if (resolvedField.isStatic)
                    staticFields.Add(resolvedField);
                else if(resolvedField.isConst)
                    constFields.Add(resolvedField);
                else
                    instanceFields.Add(resolvedField);
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
                // For now skip generic methods
                if (methodDef.genericContainerIndex >= 0)
                    continue;

                ResolvedMethod resolvedMethod = new ResolvedMethod(methodDef, typeDef.methodStart + i);
                resolvedMethod.declaringType = this;
                miMethods.Add(resolvedMethod);
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

                if(methodDef.slot != il2cpp_Constants.kInvalidIl2CppMethodSlot)
                    slottedMethods.Add(methodDef.slot, resolvedMethod);

                if (!resolvedMethod.isStatic)
                    instanceMethods.Add(resolvedMethod);
                else
                    staticMethods.Add(resolvedMethod);
                //if (methodDef.genericContainerIndex >= 0 && name == "GetComponent")
                //    Console.WriteLine();
            }

            // Split properties into instanced and static
            for(int i = 0;i< properties.Length;i++)
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

            Console.WriteLine("ResolvedClass::Resolve()");
            isResolved = true;
        }

        public override string ToHeaderCode(Int32 indent = 0)
        {
            GenerateMINames();
            string code = "";
            
            if(!isNested)
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

            code += $"// Class TypeDefinitionIndex: {typeDefinitionIndex}\n".Indent(indent);

            if (typeDef.genericContainerIndex >= 0)
                code += $"{genericTemplate}\n".Indent(indent);
            code += $"struct {Name}".Indent(indent);
            if (parentType != null)
                code += $" : {parentType.GetFullName()}";
            else
                code += $" : Il2CppObject";
            code += "\n";
            code += "{\n".Indent(indent);

            if(instanceFields.Count > 0)
            {
                for (int i = 0; i < instanceFields.Count; i++)
                {
                    code += instanceFields[i].ToCode(indent + 2);
                }
                code += "\n";
            }
            if(instanceProperties.Count > 0)
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
                    code += instanceMethods[i].ToHeaderCode(indent + 2);
                }
                code += "\n";
            }
            if(staticMethods.Count > 0)
            {
                code += "// Static Methods\n".Indent(indent + 2);
                for (int i = 0; i < staticMethods.Count; i++)
                {
                    code += staticMethods[i].ToHeaderCode(indent + 2);
                }
                code += "\n";
            }
                
            // TODO: Fields, static fields, const fields, properties, methods, (generics), nested types
            if(nestedTypes.Count > 0)
            {
                for (int i = 0; i < nestedTypes.Count; i++)
                {
                    code += nestedTypes[i].ToHeaderCode(indent + 2);
                    code += "\n";
                }
            }

            // il2cpp stuff
            code += "\n";
            code += "// il2cpp stuff\n".Indent(indent+2);
            code += "static Il2CppClass* il2cppClass;\n".Indent(indent+2);
            code += "// methods\n".Indent(indent + 2);
            for (int i=0;i<miMethods.Count;i++)
            {
                code += $"static MethodInfo* {miMethods[i].MI_Name};\n".Indent(indent+2);
            }
            code += "static void il2cpp_init();\n".Indent(indent + 2);

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
            if (parentType == Demangler.multicastDelegateType)
            {
                return base.DemangledPrefix() + "Callback";
            }
            else
            {
                bool hasCoroutineProperty = false;
                bool hasMoveNextMethod = false;

                if(instanceProperties.Count != 2)
                    goto CLASS;

                for (int i = 0; i < instanceProperties.Count; i++)
                {
                    if(instanceProperties[i].Name == "System.Collections.IEnumerator.Current")
                    {
                        hasCoroutineProperty = true;
                        break;
                    }
                }

                if(!hasCoroutineProperty)
                    goto CLASS;

                for (int i = 0; i < instanceMethods.Count; i++)
                {
                    if (instanceMethods[i].Name == "MoveNext")
                    {
                        hasMoveNextMethod = true;
                        break;
                    }
                }

                if (!hasMoveNextMethod)
                    goto CLASS;

                return base.DemangledPrefix() + "Coroutine";
            }

            CLASS:
            return base.DemangledPrefix() + "Class";
        }

        public void GenerateMINames()
        {
            Dictionary<string, List<ResolvedMethod>> mapMIMethods = new Dictionary<string, List<ResolvedMethod>>();
            for(int i =0;i<miMethods.Count;i++)
            {
                if (!mapMIMethods.TryGetValue(miMethods[i].Name, out var methodList))
                {
                    methodList = new List<ResolvedMethod>();
                    mapMIMethods.Add(miMethods[i].Name, methodList);
                }

                methodList.Add(miMethods[i]);
            }

            foreach(var pair in mapMIMethods)
            {
                List<ResolvedMethod> methods = pair.Value;
                string prefix = pair.Key;

                if(methods.Count == 1)
                {
                    methods[0].MI_Name = $"MI_{methods[0].Name}";
                    continue;
                }

                for(int i =0;i<methods.Count;i++)
                {
                    methods[i].MI_Name = $"MI_{methods[i].Name}_{i+1}";
                }
            }
        }

        private static int SortMethods(ResolvedMethod m1, ResolvedMethod m2)
        {
            if (m1 == null)
            {
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

        public override void Demangle()
        {
            if (isDemangled)
                return;

            base.Demangle();

            Dictionary<string, Int32> demangledPrefixes = new Dictionary<string, Int32>();
            if (parentType != null)
            {
                // If type inherits make sure parent is demangled first
                if (!Demangler.demangledTypes.TryGetValue(parentType, out var demangledPrefixesParent))
                {
                    if (!Demangler.demangleQueue.TryGetValue(parentType, out var list))
                    {
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
            for (int i = 0; i < instanceFields.Count; i++)
            {
                ResolvedField resolvedField = instanceFields[i];
                if (resolvedField.Name.isCSharpIdentifier())
                {
                    if (resolvedField.Name.isCppIdentifier())
                        continue;

                    resolvedField.Name = resolvedField.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
                    continue;
                }

                string demangledPrefix = instanceFields[i].DemangledPrefix();
                if (!demangledPrefix.EndsWith("_"))
                    demangledPrefix += "_";
                if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx))
                {
                    idx = 0;
                    demangledPrefixes.Add(demangledPrefix, idx);
                }
                idx++;
                resolvedField.Name = $"{demangledPrefix}{idx}";
                demangledPrefixes[demangledPrefix] = idx;
                //code += instanceFields[i].ToCode(indent + 2);
            }
            
            // Demangle properties
            for (int i =0;i<instanceProperties.Count;i++)
            {
                ResolvedProperty resolvedProperty = instanceProperties[i];
                if (resolvedProperty.Name.isCSharpIdentifier())
                {
                    if (resolvedProperty.Name.isCppIdentifier())
                        continue;

                    resolvedProperty.EnsureCppMethodNames();
                    resolvedProperty.Name = resolvedProperty.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
                    continue;
                }

                string demangledPrefix = resolvedProperty.DemangledPrefix();
                if (!demangledPrefix.EndsWith("_"))
                    demangledPrefix += "_";
                if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx))
                {
                    idx = 0;
                    demangledPrefixes.Add(demangledPrefix, idx);
                }
                idx++;
                resolvedProperty.Name = $"{demangledPrefix}{idx}";
                demangledPrefixes[demangledPrefix] = idx;
                resolvedProperty.DemangleMethods();
            }

            for (int i = 0; i < staticProperties.Count; i++)
            {
                ResolvedProperty resolvedProperty = staticProperties[i];
                if (resolvedProperty.Name.isCSharpIdentifier())
                {
                    if (resolvedProperty.Name.isCppIdentifier())
                        continue;

                    resolvedProperty.EnsureCppMethodNames();
                    resolvedProperty.Name = resolvedProperty.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
                    continue;
                }

                string demangledPrefix = resolvedProperty.DemangledPrefix();
                if (!demangledPrefix.EndsWith("_"))
                    demangledPrefix += "_";
                if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx))
                {
                    idx = 0;
                    demangledPrefixes.Add(demangledPrefix, idx);
                }
                idx++;
                resolvedProperty.Name = $"{demangledPrefix}{idx}";
                demangledPrefixes[demangledPrefix] = idx;
                resolvedProperty.DemangleMethods();
            }
            // Demangle Events
            for (int i = 0; i < instanceEvents.Count; i++)
            {
                ResolvedEvent resolvedEvent = instanceEvents[i];
                if (resolvedEvent.Name.isCSharpIdentifier())
                {
                    if (resolvedEvent.Name.isCppIdentifier())
                        continue;

                    resolvedEvent.Name = resolvedEvent.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
                    continue;
                }

                string demangledPrefix = resolvedEvent.DemangledPrefix();
                if (!demangledPrefix.EndsWith("_"))
                    demangledPrefix += "_";
                if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx))
                {
                    idx = 0;
                    demangledPrefixes.Add(demangledPrefix, idx);
                }
                idx++;
                resolvedEvent.Name = $"{demangledPrefix}{idx}";
                demangledPrefixes[demangledPrefix] = idx;
                resolvedEvent.DemangleMethods();
            }
            for (int i = 0; i < staticEvents.Count; i++)
            {
                ResolvedEvent resolvedEvent = staticEvents[i];
                if (resolvedEvent.Name.isCSharpIdentifier())
                {
                    if (resolvedEvent.Name.isCppIdentifier())
                        continue;

                    resolvedEvent.Name = resolvedEvent.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
                    continue;
                }

                string demangledPrefix = resolvedEvent.DemangledPrefix();
                if (!demangledPrefix.EndsWith("_"))
                    demangledPrefix += "_";
                if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx))
                {
                    idx = 0;
                    demangledPrefixes.Add(demangledPrefix, idx);
                }
                idx++;
                resolvedEvent.Name = $"{demangledPrefix}{idx}";
                demangledPrefixes[demangledPrefix] = idx;
                resolvedEvent.DemangleMethods();
            }

            // Demangle Methods
            for (int i = 0;i<instanceMethods.Count;i++)
            {
                ResolvedMethod resolvedMethod = instanceMethods[i];
                resolvedMethod.DemangleParams();
                if (resolvedMethod.Name.isCSharpIdentifier())
                {
                    if (resolvedMethod.Name.isCppIdentifier())
                        continue;
                    
                    resolvedMethod.Name = resolvedMethod.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
                    continue;
                }

                string demangledPrefix = resolvedMethod.DemangledPrefix();
                if (!demangledPrefix.EndsWith("_"))
                    demangledPrefix += "_";
                if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx))
                {
                    idx = 0;
                    demangledPrefixes.Add(demangledPrefix, idx);
                }
                idx++;
                resolvedMethod.Name = $"{demangledPrefix}{idx}";
                demangledPrefixes[demangledPrefix] = idx;
            }

            for (int i = 0; i < staticMethods.Count; i++)
            {
                ResolvedMethod resolvedMethod = staticMethods[i];
                resolvedMethod.DemangleParams();
                if (resolvedMethod.Name.isCSharpIdentifier())
                {
                    if (resolvedMethod.Name.isCppIdentifier())
                        continue;

                    resolvedMethod.Name = resolvedMethod.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
                    continue;
                }

                string demangledPrefix = resolvedMethod.DemangledPrefix();
                if (!demangledPrefix.EndsWith("_"))
                    demangledPrefix += "_";
                if (!demangledPrefixes.TryGetValue(demangledPrefix, out var idx))
                {
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

            for (int i =0;i< demangleQueue.Count;i++)
            {
                demangleQueue[i].Demangle();
            }
        }

        public override string ToCppCode(Int32 indent = 0)
        {
            string code = "";
            if (!isNested)
            {
                code = code.Indent(indent);
                code += "#include \"stdafx.h\"\n\n";
                // Namespace
                if (Namespace != "")
                {
                    code += $"namespace {CppNamespace()}\n".Indent(indent);
                    code += "{\n".Indent(indent);
                    indent += 2;
                }
            }
            // TODO: Init any static fields
            string NestedNameStr = NestedName();
            code += $"Il2CppClass* {NestedNameStr}il2cppClass = nullptr;\n".Indent(indent);
            for(int i =0;i<miMethods.Count; i++)
            {
                code += $"MethodInfo* {NestedNameStr}{miMethods[i].MI_Name} = nullptr;\n".Indent(indent);
            }
            // methods
            for (int i=0;i<instanceMethods.Count;i++)
            {
                code += instanceMethods[i].ToCppCode(indent);
            }

            // TODO: nested types
            for (int i = 0; i < nestedTypes.Count; i++)
            {
                code += nestedTypes[i].ToCppCode(indent + 2);
                code += "\n";
            }

            // il2cpp_init
            code += $"void {NestedNameStr}il2cpp_init()\n".Indent(indent);
            code += "{\n".Indent(indent);
            // TODO: grab right type from typedef
            code += $"il2cppClass = il2cpp::class_from_il2cpp_type(il2cpp::m_pMetadataRegistration->types[{typeDef.byvalTypeIndex}]);\n".Indent(indent+2);
            code += "il2cpp::runtime_class_init(il2cppClass);\n".Indent(indent + 2);
            // Methods
            for(int i =0;i<miMethods.Count;i++)
            {
                code += $"{miMethods[i].MI_Name} = (MethodInfo*)il2cppClass->methods[{miMethods[i].methodIndex - typeDef.methodStart}];\n".Indent(indent + 2);
            }
            code += "}\n".Indent(indent);
            // end of Namespace
            if (Namespace != "" && !isNested)
            {
                indent -= 2;
                code += "}\n".Indent(indent);
            }

            return code;
        }
    }
}
