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

        public virtual string ToCode(Int32 indent = 0)
        {
            return "ResolvedType::ToCode";
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
    }
}
