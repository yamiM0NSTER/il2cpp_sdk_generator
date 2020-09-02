using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace il2cpp_sdk_generator
{
    class CppOutput
    {
        public static void Output()
        {
            ResetDirectory();

            for(int i =0;i<Metadata.resolvedImages.Count;i++)
            {
                Directory.CreateDirectory(Metadata.resolvedImages[i].Name);
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
    }
}
