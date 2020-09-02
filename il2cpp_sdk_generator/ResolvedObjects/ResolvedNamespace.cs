using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace il2cpp_sdk_generator
{
    class ResolvedNamespace : ResolvedObject
    {
        public List<ResolvedType> Types = new List<ResolvedType>();

        public void Output()
        {

        }
    }
}
