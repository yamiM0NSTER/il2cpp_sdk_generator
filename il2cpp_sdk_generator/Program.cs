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
        [STAThread]
        static void Main(string[] args)
        {

            var fileDialog = new OpenFileDialog();

            fileDialog.Filter = "global-metadata|global-metadata.dat";
            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            // Metadata was selected
            byte[] metadataBytes = File.ReadAllBytes(fileDialog.FileName);

            MetadataReader metadataReader = new MetadataReader(new BinaryReader(new MemoryStream(metadataBytes), Encoding.UTF8));
            metadataReader.Read();



            Console.ReadLine();
        }
    }
}
