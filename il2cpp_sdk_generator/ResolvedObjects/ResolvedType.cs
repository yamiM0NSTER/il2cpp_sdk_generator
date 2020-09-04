using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    class ResolvedType : ResolvedObject
    {
        public Il2CppTypeDefinition typeDef = null;
        public string Namespace = null;
        public ResolvedType parentType = null;

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

        public ResolvedType(Il2CppTypeDefinition type)
        {
            typeDef = type;
        }

    }
}
