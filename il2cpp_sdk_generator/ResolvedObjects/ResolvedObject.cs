using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace il2cpp_sdk_generator
{
    public class ResolvedObject
    {
        public virtual string Name { get; set; }

        public bool isMangled { get; set; }
    }
}
