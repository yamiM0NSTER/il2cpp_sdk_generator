using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace il2cpp_sdk_generator
{
    public class ResolvedImage : ResolvedObject
    {
        public Dictionary<string, ResolvedNamespace> Namespaces = new Dictionary<string, ResolvedNamespace>();
        public ResolvedModule resolvedModule = null;

        public void Output()
        {
            string outputDirectory = Directory.GetCurrentDirectory();
            Directory.CreateDirectory(Name);
            string imageDirectory = Path.Combine(outputDirectory, Name);

            foreach (var pair in Namespaces)
            {
                Directory.SetCurrentDirectory(imageDirectory);
                if (pair.Key != "")
                {
                    Directory.CreateDirectory(pair.Key);
                    Directory.SetCurrentDirectory(Path.Combine(imageDirectory, pair.Key));
                }
                pair.Value.Output();
            }
        }
    }
}
