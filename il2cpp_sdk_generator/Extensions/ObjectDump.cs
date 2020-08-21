using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace il2cpp_sdk_generator
{
    static class ObjectDumpExtensions
    {
        public static void DumpPrimitiveToConsole(object instance)
        {
            Type type = instance.GetType();
            Console.WriteLine($"{type.Name} : {instance}");
        }

        public static void DumpToConsole(this object instance)
        {
            // TODO: Dump Arrays
            // TODO: Support unions
            Type type = instance.GetType();
            if(type.IsPrimitive)
            {
                DumpPrimitiveToConsole(instance);
                return;
            }

            Console.WriteLine($"{type.Name}");
            Console.WriteLine("{");
            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                Console.WriteLine($" {fieldInfo.Name}: {fieldInfo.GetValue(instance).ToString()}");
            }
            Console.WriteLine("}");
        }
    }
}
