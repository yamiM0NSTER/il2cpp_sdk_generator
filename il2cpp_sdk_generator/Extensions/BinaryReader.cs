﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace il2cpp_sdk_generator
{
    static class BinaryReaderExtensions
    {
        public static T Read<T>(this BinaryReader reader) where T : new()
        {
            Type type = typeof(T);

            if (type.IsPrimitive)
                return (T)reader.ReadPrimitive(type);

            T retObj = new T();

            // TODO: investigate performance of GetFields vs cache
            // TODO: Proper union reading
            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                Type fieldType = fieldInfo.FieldType;
                // TODO: investigate performance of SetValue vs SetValueDirect
                if (fieldType.IsPrimitive)
                {
                    fieldInfo.SetValue(retObj, reader.ReadPrimitive(fieldType));
                }
                else if (fieldType.IsEnum)
                {
                    // TODO: Support uint64 enums somehow
                    object newEnumValue = Enum.ToObject(fieldType, reader.ReadPrimitive(typeof(System.UInt32)));
                    fieldInfo.SetValue(retObj, newEnumValue);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            
            return retObj;
        }

        // signed
        static Type int16_t = typeof(System.Int16);
        static Type int32_t = typeof(System.Int32);
        static Type int64_t = typeof(System.Int64);
        // unsigned
        static Type byte_t = typeof(System.Byte);
        static Type uint16_t = typeof(System.UInt16);
        static Type uint32_t = typeof(System.UInt32);
        static Type uint64_t = typeof(System.UInt64);

        public static object ReadPrimitive(this BinaryReader reader, Type t)
        {
            // TODO: figure out non-ifology solution
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

            throw new NotSupportedException();
        }
    }
}