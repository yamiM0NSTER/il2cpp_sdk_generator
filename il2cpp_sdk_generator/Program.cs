using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace il2cpp_sdk_generator
{
    class Program
    {
        static string AssemblyPath = "";
        static string MetadataPath = "";

        [STAThread]
        static void Main(string[] args)
        {

            if (!GetFilePaths())
                return;
            
            // Metadata was selected
            byte[] metadataBytes = File.ReadAllBytes(MetadataPath);
            Console.WriteLine($"metadataBytes: {metadataBytes.Length}");

            MetadataReader metadataReader = new MetadataReader(new MemoryStream(metadataBytes));
            metadataReader.Read();

            // Select & Read GameAssembly.dll


            Console.ReadLine();
        }

        static bool GetFilePaths()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.Title = "Select il2cpp Game";
            fileDialog.Filter = "il2cpp Game|GameAssembly.dll;*.exe|All|*";
            if (fileDialog.ShowDialog() != DialogResult.OK)
                return false;

            AssemblyPath = fileDialog.FileName;
            if (Path.GetExtension(AssemblyPath) == ".exe")
            {
                string exeDir = Path.GetDirectoryName(AssemblyPath);
                string exeName = Path.GetFileNameWithoutExtension(AssemblyPath);
                string potentialMetadataPath = $"{exeDir}\\{exeName}_Data\\il2cpp_data\\Metadata\\global-metadata.dat";
                //Console.WriteLine(potentialMetadataPath);
                if (File.Exists(potentialMetadataPath))
                {
                    MetadataPath = potentialMetadataPath;
                    return true;
                }
            }
            else if(Path.GetExtension(AssemblyPath) == ".dll")
            {
                string assemblyDir = Path.GetDirectoryName(AssemblyPath);
                string potentialMetadataPath = $"{assemblyDir}\\global-metadata.dat";
                //Console.WriteLine(potentialMetadataPath);
                if (File.Exists(potentialMetadataPath))
                {
                    MetadataPath = potentialMetadataPath;
                    return true;
                }
            }

            fileDialog.Title = "Select global-metadata file";
            fileDialog.Filter = "global-metadata|global-metadata.dat|All|*";
            if (fileDialog.ShowDialog() != DialogResult.OK)
                return false;

            MetadataPath = fileDialog.FileName;

            return true;
        }
    }
}
