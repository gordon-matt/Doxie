using System.Collections.Generic;
using System.IO;
using System.Linq;
using Doxie.Core.Models;
using Doxie.Core.XmlComments;
using Extenso;

namespace Doxie.Core.Services
{
    public static class JsonHelpFileGenerator
    {
        private static DocParser docParser = new DocParser();

        public static void Generate(IEnumerable<string> selectedAssemblyPaths, string outputPath)
        {
            var assemblies = GetAssemblies(selectedAssemblyPaths);
            string outputFileName = Path.Combine(outputPath, "assemblies.json");
            assemblies.ToJson().ToFile(outputFileName);
        }

        private static IEnumerable<AssemblyModel> GetAssemblies(IEnumerable<string> selectedAssemblyPaths)
        {
            return selectedAssemblyPaths.Select(filePath => GetAssembly(filePath)).ToArray();
        }

        private static AssemblyModel GetAssembly(string filePath, bool parseNamespace = true)
        {
            var assembly = docParser.Parse(filePath, parseNamespace);
            assembly.FileName = filePath;
            return assembly;
        }
    }
}