using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace il2cpp_sdk_generator
{
    class CppOutput
    {
        public static void Output()
        {
            ResetDirectory();

            string outputDirectory = Directory.GetCurrentDirectory();

            OutPutil2cppDefines();

            for (int i =0;i<Metadata.resolvedImages.Count;i++)
            {
                Directory.SetCurrentDirectory(outputDirectory);
                Metadata.resolvedImages[i].Output();
            }

            try
            {
                // Wait for all jobs to finish
                Task.WaitAll(ResolvedNamespace.jobs.ToArray());
            }
            catch(AggregateException ex)
            {

            }
            
        }






        const string OUTPUT_PATH = "Output";

        private static void ResetDirectory()
        {
            // Set .exe location
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            // Check if Output Folder exists & delete. Has to be recursive or else exception is thrown zzzz
            if (Directory.Exists(OUTPUT_PATH))
                Directory.Delete(OUTPUT_PATH, true);
            // Create Output Folder for content
            Directory.CreateDirectory(OUTPUT_PATH);
            // Set operating location for output
            Directory.SetCurrentDirectory($"{AppDomain.CurrentDomain.BaseDirectory}\\{OUTPUT_PATH}");
        }

        private static void OutPutil2cppDefines()
        {
            // TODO: Use StreamWriter
            string headerCode = "#pragma once\n\n";

            headerCode += $"#define GAMEASSEMBLY_FILE_SIZE {BinaryPattern.m_assemblySize}\n";
            headerCode += $"#define METADATA_FILE_SIZE {MetadataReader.stream.Length}\n";
            
            headerCode += $"#define CODE_REGISTRATION_RVA 0x{RVA.FromVA(il2cpp.CodeRegistrationAddress):X}\n";
            headerCode += $"#define METADATA_REGISTRATION_RVA 0x{RVA.FromVA(il2cpp.MetadataRegistrationAddress):X}\n";

            File.WriteAllText($"il2cpp-defines.h", headerCode);
        }
    }
}
