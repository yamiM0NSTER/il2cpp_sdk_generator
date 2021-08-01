using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace il2cpp_sdk_generator
{
    public class BinaryPattern
    {
        static private byte[] m_assemblyData;
        static public long m_assemblySize;

        public static void SetAssemblyData(byte[] assemblyData)
        {
            m_assemblyData = assemblyData;
            m_assemblySize = assemblyData.LongLength;
        }

        public static long FindPattern(long offset, string pattern)
        {
            Dictionary<int, byte> patternMap = new Dictionary<int, byte>();

            byte start = GetByte(pattern[0], pattern[1]);

            int pattern_len = pattern.Length / 3 + (pattern.Length % 3 > 0 ? 1 : 0);

            for (int k = 3, m = 1; m < pattern_len; k += 3, m++)
            {
                // Allow wildcards
                if (pattern[k] == '?')
                    continue;

                patternMap.Add(m, GetByte(pattern[k], pattern[k + 1]));
            }

            //foreach(var pair in patternMap)
            //{
            //    Console.WriteLine($"pos: {pair.Key} val: {pair.Value}");
            //}

            bool bFound = false;
            //Console.WriteLine($"Start: {start}");
            //Console.WriteLine($"Max Length:{m_assemblyData.Length - pattern.Length}");

            for (long i = offset; i < m_assemblyData.Length - pattern_len + 1; i++)
            {
                if (m_assemblyData[i] != start)
                    continue;

                bFound = true;

                foreach (var pair in patternMap)
                {
                    if (pair.Value != m_assemblyData[i + pair.Key])
                    {
                        //if(pair.Key > 6)
                        //    Console.WriteLine($"cur offset: {i} cur key: {pair.Key} cur val: {pair.Value} assembly val: {m_assemblyData[i + pair.Key]}");
                        bFound = false;
                        break;
                    }
                }

                if (bFound)
                {
                    //Console.WriteLine("Apparently found");
                    return i;
                }
            }

            return 0;
        }

        public static long FindPattern(long offset, long max_length, string pattern)
        {
            Dictionary<int, byte> patternMap = new Dictionary<int, byte>();

            byte start = GetByte(pattern[0], pattern[1]);

            int pattern_len = pattern.Length / 3 + (pattern.Length % 3 > 0 ? 1 : 0);

            for (int k = 3, m = 1; m < pattern_len; k += 3, m++)
            {
                // Allow wildcards
                if (pattern[k] == '?')
                    continue;

                patternMap.Add(m, GetByte(pattern[k], pattern[k + 1]));
            }


            //Console.WriteLine($"Pattern length: {pattern_len}");
            //Console.WriteLine($"Pattern map length: {patternMap.Count + 1}");
            //foreach(var pair in patternMap)
            //{
            //    Console.WriteLine($"pos: {pair.Key} val: {pair.Value}");
            //}

            bool bFound = false;
            //Console.WriteLine($"Start: {start}");
            //Console.WriteLine($"Max Length: {m_assemblyData.Length - pattern_len}");
            //Console.WriteLine($"Start offset: {offset} offset + length - len: {offset + max_length - pattern_len}");
            for (long i = offset; i < offset + max_length - pattern_len + 1; i++)
            {
                if (m_assemblyData[i] != start)
                    continue;

                bFound = true;

                foreach (var pair in patternMap)
                {
                    if (pair.Value != m_assemblyData[i + pair.Key])
                    {
                        //if(pair.Key > 6)
                        //    Console.WriteLine($"cur offset: {i} cur key: {pair.Key} cur val: {pair.Value} assembly val: {m_assemblyData[i + pair.Key]}");
                        bFound = false;
                        break;
                    }
                }

                if (bFound)
                {
                    //Console.WriteLine("Apparently found");
                    return i;
                }
            }

            //Console.WriteLine($"Not found?");

            return 0;
        }

        static byte GetByte(char sign1, char sign2)
        {
            return Convert.ToByte(GetBits(sign1) << 4 | GetBits(sign2));
        }

        static byte GetBits(char x)
        {
            return Convert.ToByte(InRange(Convert.ToChar(x & (~0x20)), 'A', 'F') ? ((x & (~0x20)) - 'A' + 0xA) : (InRange(x, '0', '9') ? x - '0' : 0));
        }

        static bool InRange(char x, char a, char b)
        {
            return (x >= a && x <= b);
        }

        public static Int32 GetInt32_LE(long offset)
        {
            //Console.WriteLine("GetInt32:");
            //Console.WriteLine($"=> {(int)m_assemblyData[offset]} HEX: 0x{(int)m_assemblyData[offset]:X}");
            //Console.WriteLine($"=> {(int)m_assemblyData[offset + 1] << 8 } HEX: 0x{(int)m_assemblyData[offset + 1] << 8:X}");
            //Console.WriteLine($"=> {(int)m_assemblyData[offset + 2] << 16} HEX: 0x{(int)m_assemblyData[offset + 2] << 16:X}");
            //Console.WriteLine($"=> {(int)m_assemblyData[offset + 3] << 24} HEX: 0x{(int)m_assemblyData[offset + 3] << 24:X}");

            return (m_assemblyData[offset + 3] << 24) + (m_assemblyData[offset + 2] << 16) + (m_assemblyData[offset + 1] << 8) + m_assemblyData[offset];
        }
    }
}
