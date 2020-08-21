using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace il2cpp_sdk_generator
{
    static class TypeExtensions
    {
        private static Dictionary<Type, MethodInfo> genericMethods = new Dictionary<Type, MethodInfo>();

        // signed
        static Type int16_t = typeof(System.Int16);
        static Type int32_t = typeof(System.Int32);
        static Type int64_t = typeof(System.Int64);
        // unsigned
        static Type byte_t = typeof(System.Byte);
        static Type uint16_t = typeof(System.UInt16);
        static Type uint32_t = typeof(System.UInt32);
        static Type uint64_t = typeof(System.UInt64);

        public static int GetPrimitiveSizeOf(Type instance)
        {
            // TODO: figure out non-ifology solution
            if (instance == int16_t)
                return 2;
            if (instance == int32_t)
                return 4;
            if (instance == int64_t)
                return 8;

            if (instance == byte_t)
                return 1;
            if (instance == uint16_t)
                return 2;
            if (instance == uint32_t)
                return 4;
            if (instance == uint64_t)
                return 8;

            throw new NotSupportedException();
        }

        public static int GetSizeOf(this Type instance)
        {
            // TODO: maybe cache?
            int nSize = 0;
            foreach (FieldInfo fieldInfo in instance.GetFields())
            {
                Type fieldType = fieldInfo.FieldType;
                if (fieldType.IsPrimitive)
                {
                    nSize += GetPrimitiveSizeOf(fieldType);
                }
                else if (fieldType.IsEnum)
                {
                    // TODO: Support uint64 enums somehow
                    nSize += 4;
                }
                else if(fieldType.IsArray)
                {
                    // Get Array size from attribute
                    // All array fields have to contain attribute
                    var arraySizeAttribute = (ArraySizeAttribute)fieldType.GetCustomAttributes(typeof(ArraySizeAttribute), false)[0];
                    nSize += fieldType.GetElementType().GetSizeOf() * arraySizeAttribute.Value;
                }
                else
                {
                    nSize += fieldType.GetSizeOf();

                    //throw new NotSupportedException();
                }
            }
            return nSize;
        }
    }
}
