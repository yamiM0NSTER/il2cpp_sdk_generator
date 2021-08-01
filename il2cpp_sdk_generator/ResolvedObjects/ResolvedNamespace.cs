using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    public class ResolvedNamespace : ResolvedObject
    {

        public List<ResolvedType> Types = new List<ResolvedType>();
        public List<ResolvedType> Enums = new List<ResolvedType>();

        public static List<Task> jobs = new List<Task>();

        //const int BufferSize = 65536;  // 64 Kilobytes
        const int BufferSize = 8192;  // 64 Kilobytes

        private const int NumberOfRetries = 3;
        private const int DelayOnRetry = 100;

        async Task JobProcessor(int idx, string path)
        {
            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter($"{path}/{Types[idx].Name}.h", true, Encoding.UTF8, BufferSize))
                    {
                        await Types[idx].ToHeaderCode(sw);
                        await sw.FlushAsync();
                    }
                    //Console.WriteLine($"{Types[idx].Name}.h Attempts: {i}");
                    break; // When done we can break loop
                }
                catch (IOException e) //when(i <= NumberOfRetries)
                {
                    if (i == NumberOfRetries)
                        Console.WriteLine($"{Types[idx].Name}.h failed after 3 retries.");
                    // You may check error code to filter some exceptions, not every error
                    // can be recovered.
                    await Task.Delay(DelayOnRetry);
                }
            }

            if (Types[idx] is ResolvedInterface)
                return;

            string cppFile = $"{Types[idx].Name}.cpp";
            if (Name == "VRCSDK2")
                cppFile = $"{Types[idx].Name}2.cpp";
            else if (Name == "VRCSDK3")
                cppFile = $"{Types[idx].Name}3.cpp";

            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter($"{path}/{cppFile}", true, Encoding.UTF8, BufferSize))
                    {
                        await Types[idx].ToCppCode(sw);
                        await sw.FlushAsync();
                    }
                    //Console.WriteLine($"{Types[idx].Name}.h Attempts: {i}");
                    break; // When done we can break loop
                }
                catch (IOException e) //when(i <= NumberOfRetries)
                {
                    if (i == NumberOfRetries)
                        Console.WriteLine($"{cppFile} failed after 3 retries.");
                    // You may check error code to filter some exceptions, not every error
                    // can be recovered.
                    await Task.Delay(DelayOnRetry);
                }
            }
        }

        async Task EnumJobProcessor(string path)
        {
            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter($"{path}/il2cpp-enums.h", true, Encoding.UTF8, BufferSize))
                    {
                        await sw.WriteAsync("#pragma once\n\n");
                        int indent = 0;
                        // Start of namespace
                        if (Name != "")
                        {
                            await sw.WriteAsync($"namespace {Name.Replace(".", "::")}\n".Indent(indent));
                            await sw.WriteAsync("{\n".Indent(indent));
                            indent += 2;
                        }

                        // Output Enums
                        for (int k = 0; k < Enums.Count; k++)
                        {
                            if (!Enums[k].Name.isCppIdentifier() || Enums[k].Name == "MonoIOError")
                                continue;
                            await sw.WriteAsync(Enums[k].ToHeaderCodeGlobal(indent));
                        }

                        // end of Namespace
                        if (Name != "")
                        {
                            indent -= 2;
                            await sw.WriteAsync("}\n".Indent(indent));
                        }
                        await sw.FlushAsync();
                    }
                    //Console.WriteLine($"{Types[idx].Name}.h Attempts: {i}");
                    break; // When done we can break loop
                }
                catch (IOException e) //when(i <= NumberOfRetries)
                {
                    if (i == NumberOfRetries)
                        Console.WriteLine($"{path}/il2cpp-enums.h failed after 3 retries.");
                    // You may check error code to filter some exceptions, not every error
                    // can be recovered.
                    await Task.Delay(DelayOnRetry);
                }
            }
        }

        async Task ToCpp(int idx)
        {
            string cppFile = $"{Types[idx].Name}.cpp";
            if (Name == "VRCSDK2")
                cppFile = $"{Types[idx].Name}2.cpp";
            else if (Name == "VRCSDK3")
                cppFile = $"{Types[idx].Name}3.cpp";

            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(cppFile, true, Encoding.UTF8, BufferSize))
                    {
                        await Types[idx].ToCppCode(sw);
                        await sw.FlushAsync();
                    }
                    //Console.WriteLine($"{Types[idx].Name}.h Attempts: {i}");
                    break; // When done we can break loop
                }
                catch (IOException e) //when(i <= NumberOfRetries)
                {
                    if (i == NumberOfRetries)
                        Console.WriteLine($"{cppFile} failed after 3 retries.");
                    // You may check error code to filter some exceptions, not every error
                    // can be recovered.
                    await Task.Delay(DelayOnRetry);
                }
            }
        }



        public void Output()
        {
            string curDir = Directory.GetCurrentDirectory();
            //Console.WriteLine("Creating Headers");
            for (int i = 0; i < Types.Count; i++)
            {
                if (!Types[i].Name.isCppIdentifier())
                {
                    Console.WriteLine($"Not cpp {i} => {Types[i].Name}");
                    continue;
                }
                
                int idx = i;
                jobs.Add(Task.Run(() => JobProcessor(idx, curDir)));
            }

            jobs.Add(Task.Run(() => EnumJobProcessor(curDir)));
        }
    }
}
