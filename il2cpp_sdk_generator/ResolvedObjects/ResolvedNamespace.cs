﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace il2cpp_sdk_generator
{
    class ResolvedNamespace : ResolvedObject
    {
        public List<ResolvedType> Types = new List<ResolvedType>();

        public void Output()
        {
            for(int i = 0; i<Types.Count;i++)
            {
                if (!Types[i].Name.isCppIdentifier())
                    continue;
                File.WriteAllText($"{Types[i].Name}.h", Types[i].Name);
            }
            //File.WriteAllText
        }
    }
}
