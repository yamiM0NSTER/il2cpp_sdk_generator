using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace il2cpp_sdk_generator
{
    static class BinaryReaderExtensions
    {
        private static Dictionary<Type, MethodInfo> genericMethods = new Dictionary<Type, MethodInfo>();
        private static Dictionary<Type, FieldInfo[]> fieldCache = new Dictionary<Type, FieldInfo[]>();

        // TODO: custom struct for cache 
        // TODO: delegate for faster execution of generic methods?
        public static FieldInfo[] GetFields(Type type)
        {
            FieldInfo[] fields;
            if (fieldCache.TryGetValue(type, out fields))
                return fields;

            if (type.StructLayoutAttribute.Value == LayoutKind.Explicit)
            {
                List<FieldInfo> fieldInfos = new List<FieldInfo>();
                Dictionary<System.Int32, System.Int32> fieldOffsets = new Dictionary<System.Int32, System.Int32>();
                var typeFields = type.GetFields();
                for (int i = 0; i < typeFields.Length; i++)
                {
                    // All fields have to contain attribute
                    var fieldOffsetAttribute = (FieldOffsetAttribute)typeFields[i].GetCustomAttributes(typeof(FieldOffsetAttribute), false)[0];
                    if (fieldOffsets.ContainsKey(fieldOffsetAttribute.Value))
                        continue;

                    fieldOffsets.Add(fieldOffsetAttribute.Value, typeFields[i].GetType().GetSizeOf());
                    fieldInfos.Add(typeFields[i]);
                }
                fields = fieldInfos.ToArray();
            }
            else
            {
                fields = type.GetFields();
            }

            fieldCache.Add(type, fields);

            return fields;
        }


        public static T Read<T>(this BinaryReader reader) where T : new()
        {
            Type type = typeof(T);

            if (type.IsPrimitive)
                return (T)reader.ReadPrimitive(type);

            T retObj = new T();

            FieldInfo[] fields = GetFields(type);

            for (int i=0;i<fields.Length;i++)
            {
                Type fieldType = fields[i].FieldType;
                // TODO: investigate performance of SetValue vs SetValueDirect
                if (fieldType.IsPrimitive)
                {
                    fields[i].SetValue(retObj, reader.ReadPrimitive(fieldType));
                }
                else if(fieldType == typeof(string))
                {
                    fields[i].SetValue(retObj, reader.ReadNullTerminatedString());
                }
                else if (fieldType.IsEnum)
                {
                    // TODO: Support uint64 enums somehow
                    object newEnumValue = Enum.ToObject(fieldType, reader.ReadPrimitive(typeof(System.UInt32)));
                    fields[i].SetValue(retObj, newEnumValue);
                }
                else if (fieldType.IsArray)
                {
                    // Get Array size from attribute
                    // All array fields have to contain attribute
                    var arraySizeAttribute = (ArraySizeAttribute)fields[i].GetCustomAttributes(typeof(ArraySizeAttribute), false)[0];

                    MethodInfo methodInfo = null;

                    if (!genericMethods.TryGetValue(fieldType, out methodInfo))
                    {
                        // if generic method was not generated yet, generate it
                        MethodInfo readMethod = typeof(BinaryReaderExtensions).GetMethod("ReadArray");
                        methodInfo = readMethod.MakeGenericMethod(fieldType.GetElementType());

                        var test = methodInfo.GetParameters().Length;
                        genericMethods.Add(fieldType, methodInfo);
                    }

                    fields[i].SetValue(retObj, methodInfo.Invoke(null, new object[] { reader, arraySizeAttribute.Value }));
                }
                else
                {
                    MethodInfo methodInfo = null;

                    if (!genericMethods.TryGetValue(fieldType, out methodInfo))
                    {
                        // if generic method was not generated yet, generate it
                        MethodInfo readMethod = typeof(BinaryReaderExtensions).GetMethod("Read");
                        methodInfo = readMethod.MakeGenericMethod(fieldType);

                        var test = methodInfo.GetParameters().Length;
                        genericMethods.Add(fieldType, methodInfo);
                    }


                    fields[i].SetValue(retObj, methodInfo.Invoke(null, new object[] { reader }));
                    
                }
            }
            
            return retObj;
        }

        public static T[] ReadArray<T>(this BinaryReader reader, int arrSize) where T : new()
        {
            //Console.WriteLine($"arrSize: {arrSize} * {typeof(T).Name}");
            T[] retArr = new T[arrSize];

            for (var i = 0; i < arrSize; i++)
            {
                retArr[i] = reader.Read<T>();
            }

            return retArr;
        }

        public static string ReadNullTerminatedString(this BinaryReader reader)
        {
            ArrayBuilder arrayBuilder = new ArrayBuilder();
            byte uc;
            while ((uc = reader.ReadByte()) > 0)
                arrayBuilder.Append(uc);
            return System.Text.Encoding.UTF8.GetString(arrayBuilder.ToArray());
        }

        public static string ReadString(this BinaryReader reader, UInt32 len)
        {
            ArrayBuilder arrayBuilder = new ArrayBuilder();
            byte uc;
            for(int i=0;i<len;i++)
            {
                uc = reader.ReadByte();
                arrayBuilder.Append(uc);
            }
                
            return System.Text.Encoding.UTF8.GetString(arrayBuilder.ToArray());
        }

        // signed
        static Type sbyte_t = typeof(System.SByte);
        static Type int16_t = typeof(System.Int16);
        static Type int32_t = typeof(System.Int32);
        static Type int64_t = typeof(System.Int64);
        // unsigned
        static Type byte_t = typeof(System.Byte);
        static Type uint16_t = typeof(System.UInt16);
        static Type uint32_t = typeof(System.UInt32);
        static Type uint64_t = typeof(System.UInt64);

        static Type bool_t = typeof(System.Boolean);
        static Type float_t = typeof(System.Single);
        static Type double_t = typeof(System.Double);

        public static object ReadPrimitive(this BinaryReader reader, Type t)
        {
            // TODO: figure out non-ifology solution
            if (t == sbyte_t)
                return reader.ReadSByte();
            if (t == int16_t)
                return reader.ReadInt16();
            if(t == int32_t)
                return reader.ReadInt32();
            if (t == int64_t)
                return reader.ReadInt64();

            if (t == byte_t)
                return reader.ReadByte();
            if (t == uint16_t)
                return reader.ReadUInt16();
            if (t == uint32_t)
                return reader.ReadUInt32();
            if (t == uint64_t)
                return reader.ReadUInt64();

            if (t == bool_t)
                return reader.ReadBoolean();

            if (t == float_t)
                return reader.ReadSingle();
            if (t == double_t)
                return reader.ReadDouble();

            throw new NotSupportedException();
        }
    }
}
