﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace il2cpp_sdk_generator
{
    static class ObjectDumpExtensions
    {
        public static string Indent(int count)
        {
            return "".PadLeft(count);
        }

        public static void DumpPrimitiveToConsole(object instance, int indent = 0)
        {
            Type type = instance.GetType();
            Console.WriteLine($"{Indent(indent)}{instance}");
        }

        public static void DumpToConsole(this object instance, int indent = 0)
        {
            // TODO: Support unions/or just let them display same value @_@
            // TODO: Support dumping arrays
            Type type = instance.GetType();
            if(type.IsPrimitive)
            {
                if (indent == 0)
                    Console.Write($"{Indent(indent)}{type.Name} : ");
                DumpPrimitiveToConsole(instance, indent);
                return;
            }

            Console.WriteLine($"{Indent(indent)}{type.Name}:");
            Console.WriteLine(Indent(indent) + "{");
            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                Type fieldType = fieldInfo.FieldType;
                if (fieldType.IsPrimitive)
                {
                    Console.WriteLine($"{Indent(indent + 2)}{fieldInfo.Name}: {fieldInfo.GetValue(instance).ToString()}");
                }
                else if (fieldType == typeof(string))
                {
                    Console.WriteLine($"{Indent(indent + 2)}{fieldInfo.Name}: {fieldInfo.GetValue(instance).ToString()}");
                }
                else if (fieldType.IsArray)
                {
                    Console.WriteLine($"{Indent(indent + 2)}{fieldInfo.Name}:");
                    Console.WriteLine($"{Indent(indent + 2)}[");
                    Array arr = (Array)fieldInfo.GetValue(instance);
                    for (int i = 0; i < arr.GetLength(0); i++)
                        arr.GetValue(i).DumpToConsole(indent + 4);
                    Console.WriteLine($"{Indent(indent + 2)}]");
                }
                else
                {
                    fieldInfo.GetValue(instance).DumpToConsole(indent + 2);
                }
            }
            Console.WriteLine(Indent(indent) + "}");
        }
    }
}
