﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    static class StringExtensions
    {
        static string digits = "0123456789";
        static string alphanum = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";


        public static bool isCppIdentifier(this string str)
        {
            if (str.Length == 0)
                return false;

            if (digits.Contains(str[0]))
                return false;

            for(int i = 1; i< str.Length;i++)
            {
                if (!alphanum.Contains(str[i]))
                    return false;
            }

            return true;
        }

        public static string Indent(this string str, Int32 indent)
        {
            return "".PadLeft(indent) + str;
        }
    }
}
