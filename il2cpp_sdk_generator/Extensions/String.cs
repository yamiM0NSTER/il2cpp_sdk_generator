using System;
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
        static string csharp = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_<>.";

        public static bool isCppIdentifier(this string str)
        {
            if (str.Length == 0)
                return false;

            if (digits.Contains(str[0]) || str[0] == '.')
                return false;

            for(int i = 1; i< str.Length;i++)
            {
                if (!alphanum.Contains(str[i]))
                    return false;
            }

            return true;
        }

        public static bool isCSharpIdentifier(this string str)
        {
            if (str.Length == 0)
                return false;

            for (int i = 0; i < str.Length; i++)
            {
                if (!csharp.Contains(str[i]))
                    return false;
            }

            return true;
        }


        public static string Indent(this string str, Int32 indent)
        {
            return "".PadLeft(indent) + str;
        }

        public static string CSharpToCppIdentifier(this string str)
        {
            return str.Replace('<', '_').Replace('>', '_').Replace('.', '_');
        }
    }
}
