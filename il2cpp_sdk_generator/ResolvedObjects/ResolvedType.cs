using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    class ResolvedType : ResolvedObject
    {
        protected bool isResolved = false;
        public Int32 typeDefinitionIndex;
        public Il2CppTypeDefinition typeDef = null;
        public string Namespace = null;
        public ResolvedType parentType = null;
        public ResolvedType declaringType = null;

        public List<ResolvedType> nestedTypes = new List<ResolvedType>();

        public bool isNested
        {
            get
            {
                return typeDef.declaringTypeIndex != -1;
            }
        }

        public ResolvedType()
        {

        }

        public ResolvedType(Il2CppTypeDefinition type, Int32 idx)
        {
            typeDef = type;
            typeDefinitionIndex = idx;
        }

        public virtual void Resolve()
        {
            if (isResolved)
                return;
            Console.WriteLine("ResolvedType::Resolve()");
            isResolved = true;
        }

        public virtual string ToHeaderCode(Int32 indent = 0)
        {
            return "ResolvedType::ToHeaderCode";
        }

        public string CppNamespace()
        {
            return Namespace.Replace(".", "::");
        }

        public string GetFullName()
        {
            string str = "";
            if (isNested)
                str = declaringType.GetFullName();
            else
                str = CppNamespace();

            if (str != "")
                str += "::";
            str += $"{Name}";
            return str;
        }

        public virtual string DemangledPrefix()
        {
            string prefix = GetVisibility();

            UInt32 staticFlags = typeDef.flags & il2cpp_Constants.TYPE_ATTRIBUTE_STATIC;
            switch(staticFlags)
            {
                case il2cpp_Constants.TYPE_ATTRIBUTE_STATIC:
                    prefix += "Static";
                    break;
                case il2cpp_Constants.TYPE_ATTRIBUTE_ABSTRACT:
                    prefix += "Abstract";
                    break;
                case il2cpp_Constants.TYPE_ATTRIBUTE_SEALED:
                    prefix += "Sealed";
                    break;
            }

            return prefix;
        }

        public string GetVisibility()
        {
            UInt32 visFlags = typeDef.flags & il2cpp_Constants.TYPE_ATTRIBUTE_VISIBILITY_MASK;
            switch (visFlags)
            {
                case il2cpp_Constants.TYPE_ATTRIBUTE_PUBLIC:
                case il2cpp_Constants.TYPE_ATTRIBUTE_NESTED_PUBLIC:
                    return "Public";
                case il2cpp_Constants.TYPE_ATTRIBUTE_NOT_PUBLIC:
                case il2cpp_Constants.TYPE_ATTRIBUTE_NESTED_FAM_AND_ASSEM:
                case il2cpp_Constants.TYPE_ATTRIBUTE_NESTED_ASSEMBLY:
                    return "Internal";
                case il2cpp_Constants.TYPE_ATTRIBUTE_NESTED_PRIVATE:
                    return "Private";
                case il2cpp_Constants.TYPE_ATTRIBUTE_NESTED_FAMILY:
                    return "Protected";
                case il2cpp_Constants.TYPE_ATTRIBUTE_NESTED_FAM_OR_ASSEM:
                    return "ProtectedInternal";
            }
            return "";
        }

        public virtual Dictionary<string, Int32> Demangle(Dictionary<string, Int32> demangledPrefixesParent = null)
        {
            // Demangle type
            Dictionary<string, Int32> demangledPrefixes = new Dictionary<string, Int32>();
            if (demangledPrefixesParent != null)
                demangledPrefixes = new Dictionary<string, Int32>(demangledPrefixesParent);
            return demangledPrefixes;
        }

        protected bool isDemangled = false;

        public virtual void Demangle()
        {
            if (isDemangled)
                return;

            // Demangle Nested types.
            for (int i = 0; i < nestedTypes.Count; i++)
            {
                nestedTypes[i].Demangle();
            }
        }

        public virtual void DemangleNestedTypeNames()
        {
            Dictionary<string, Int32> demangledPrefixes = new Dictionary<string, Int32>();

            for(int i =0;i< nestedTypes.Count;i++)
            {
                ResolvedType resolvedType = nestedTypes[i];
                if (resolvedType.Name.isCSharpIdentifier())
                {
                    if (resolvedType.Name.isCppIdentifier())
                        continue;

                    resolvedType.Name = resolvedType.Name.Replace('<', '_').Replace('>', '_').Replace('.', '_');
                    continue;
                }

                string demangledPrefix = resolvedType.DemangledPrefix();
                if(!demangledPrefixes.TryGetValue(demangledPrefix, out var idx))
                {
                    idx = 0;
                    demangledPrefixes.Add(demangledPrefix, idx);
                }
                idx++;

                resolvedType.Name = $"{demangledPrefix}{idx}";
                demangledPrefixes[demangledPrefix] = idx;
                resolvedType.DemangleNestedTypeNames();
            }
        }

        public virtual string ToCppCode(Int32 indent = 0)
        {
            return "ResolvedType::ToCppCode";
        }

        public string NestedName(string name = "")
        {
            if (declaringType == null)
                return $"{Name}::{name}";

            name = $"{Name}::{name}";
            return declaringType.NestedName(name);
        }
    }
}
