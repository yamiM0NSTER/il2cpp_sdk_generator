using System;

namespace il2cpp_sdk_generator
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class ArraySizeAttribute : Attribute
    {
        public ArraySizeAttribute(int size)
        {
            Value = size;
        }

        public int Value { get; private set; }
    }
}