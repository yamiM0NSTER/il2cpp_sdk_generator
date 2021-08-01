using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    class Demangler
    {
        public static ResolvedType multicastDelegateType = null;

        public static void Demangle()
        {
            // Get Callback parent type
            for(int i =0;i<Metadata.resolvedTypes.Length;i++)
            {
                if (Metadata.resolvedTypes[i].isNested)
                    continue;

                if (Metadata.resolvedTypes[i].Name != "MulticastDelegate")
                    continue;

                if (Metadata.resolvedTypes[i].Namespace != "System")
                    continue;
                
                multicastDelegateType = Metadata.resolvedTypes[i];
                break;
            }

            Dictionary<string, Dictionary<string, Int32>> mangledTypes = new Dictionary<string, Dictionary<string, Int32>>();
            // Run through main classes in each image in each namespace
            for (int i = 0; i < Metadata.resolvedImages.Count; i++)
            {
                foreach(var resolvedNamespace in Metadata.resolvedImages[i].Namespaces)
                {
                    if (!mangledTypes.TryGetValue(resolvedNamespace.Key, out var dict))
                    {
                        dict = new Dictionary<string, Int32>();
                        mangledTypes.Add(resolvedNamespace.Key, dict);
                    }

                    DemangleTypeNames(resolvedNamespace.Value, dict);
                    DemangleTypes(resolvedNamespace.Value);
                }
            }

            MetadataReader.mapSimpleTypeStringCache.Clear();
            MetadataReader.mapTypeStringCache.Clear();
            
            //foreach (var genericClass in MetadataReader.mapResolvedGenericClasses.Values)
            //{
            //    genericClass.TestInsideTypes();
            //}
        }

        static void DemangleTypeNames(ResolvedNamespace resolvedNamespace, Dictionary<string, Int32> dict)
        {
            for (int k = 0; k < resolvedNamespace.Types.Count; k++)
            {
                ResolvedType resolvedType = resolvedNamespace.Types[k];
                resolvedType.DemangleNestedTypeNames();
                if (resolvedType.Name.isCSharpIdentifier())
                {
                    if (resolvedType.Name.isCppIdentifier())
                        continue;

                    resolvedType.Name = resolvedType.Name.Replace('<', '_').Replace('>', '_');
                    continue;
                }

                // Get Demangled type prefix
                string demangledPrefix = resolvedType.DemangledPrefix();
                if (!dict.TryGetValue(demangledPrefix, out var idx))
                {
                    dict.Add(demangledPrefix, 0);
                    idx = 0;
                }
                idx++;

                resolvedType.Name = $"{demangledPrefix}{idx}";
                dict[demangledPrefix] = idx;
            }

            for (int k = 0; k < resolvedNamespace.Enums.Count; k++)
            {
                ResolvedType resolvedType = resolvedNamespace.Enums[k];
                resolvedType.DemangleNestedTypeNames();
                if (resolvedType.Name.isCSharpIdentifier())
                {
                    if (resolvedType.Name.isCppIdentifier())
                        continue;

                    resolvedType.Name = resolvedType.Name.Replace('<', '_').Replace('>', '_');
                    continue;
                }

                // Get Demangled type prefix
                string demangledPrefix = resolvedType.DemangledPrefix();
                if (!dict.TryGetValue(demangledPrefix, out var idx))
                {
                    dict.Add(demangledPrefix, 0);
                    idx = 0;
                }
                idx++;

                resolvedType.Name = $"{demangledPrefix}{idx}";
                dict[demangledPrefix] = idx;
            }
        }

        public static Dictionary<ResolvedType, List<ResolvedType>> demangleQueue = new Dictionary<ResolvedType, List<ResolvedType>>();
        public static Dictionary<ResolvedType, Dictionary<string, Int32>> demangledTypes = new Dictionary<ResolvedType, Dictionary<string, Int32>>();

        static void DemangleTypes(ResolvedNamespace resolvedNamespace)
        {
            for (int k = 0; k < resolvedNamespace.Types.Count; k++)
            {
                resolvedNamespace.Types[k].Demangle(false);
            }

            for (int k = 0; k < resolvedNamespace.Enums.Count; k++)
            {
                resolvedNamespace.Enums[k].Demangle(false);
            }
        }

        static void DemangleType(ResolvedType resolvedType)
        {
            Dictionary<string, Int32> demangledPrefixes = new Dictionary<string, Int32>();

            // Resolve type here
            if (resolvedType is ResolvedEnum || resolvedType is ResolvedClass)
            {
                resolvedType.Demangle(false);
            }

            if (resolvedType.parentType != null)
            {
                // If type inherits make sure parent is demangled first
                if (!demangledTypes.TryGetValue(resolvedType.parentType, out var demangledPrefixesParent))
                {
                    if (!demangleQueue.TryGetValue(resolvedType.parentType, out var list))
                    {
                        list = new List<ResolvedType>();
                        demangleQueue.Add(resolvedType.parentType, list);
                    }
                    list.Add(resolvedType);
                    return;
                }

                demangledPrefixes = new Dictionary<string, int>(demangledPrefixesParent);
            }

            

            // After resolving check queue and resolve
        }
    }
}
