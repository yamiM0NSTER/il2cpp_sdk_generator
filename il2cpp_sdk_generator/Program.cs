using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace il2cpp_sdk_generator
{
  class Program
  {
    static string AssemblyPath = "";
    static string MetadataPath = "";

    [STAThread]
    static void Main(string[] args)
    {
      var types = Assembly.GetEntryAssembly().GetAllTypes().Where(mytype => mytype.IsSubclassOf(typeof(RuleBaseClass))).ToArray();

      var test = (RuleBaseClass)Activator.CreateInstance(types[0]);


      if (!GetFilePaths())
        return;

      // Get all rules
      //Rules.Initialize();
      RulesManager.Initialize();

      // Metadata was selected
      byte[] metadataBytes = File.ReadAllBytes(MetadataPath);
      Console.WriteLine($"metadataBytes: {metadataBytes.Length}");

      MetadataReader metadataReader = new MetadataReader(new MemoryStream(metadataBytes));
      metadataReader.Read();
      metadataReader.Process();

      // Select & Read GameAssembly.dll
      byte[] peBytes = File.ReadAllBytes(AssemblyPath);
      Console.WriteLine($"peBytes: {peBytes.Length}");
      BinaryPattern.SetAssemblyData(peBytes);
      MemoryStream memStream = new MemoryStream(peBytes);

      PortableExecutableReader peReader = new PortableExecutableReader(memStream);
      peReader.Read();
      peReader.Process();

      il2cppReader.Init(memStream);
      il2cppReader.Read();
      il2cppReader.Process();

      // TODO: Trusted references
      CodeScanner.Scan(peBytes);

      Demangler.Demangle();

      Console.WriteLine("Applying rules");
      // Rules 
      RulesManager.Apply();
      //Rules.Apply();

      // Demangle again to clean up?
      //Demangler.Demangle();

      Console.WriteLine("Rules done");
      Console.ReadLine();

      Console.WriteLine("Outputting files");
      CppOutput.Output();

      Console.WriteLine("Finito");
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
        AssemblyPath = $"{Path.GetDirectoryName(AssemblyPath)}\\GameAssembly.dll";
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
      else if (Path.GetExtension(AssemblyPath) == ".dll")
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
