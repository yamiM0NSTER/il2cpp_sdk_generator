using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    static class AssemblyExtensions
    {
        public static Type[] GetAllTypes(this Assembly assembly)
        {
            if (assembly == null)
                return new Type[0];

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null).ToArray();
            }
        }
    }
}
