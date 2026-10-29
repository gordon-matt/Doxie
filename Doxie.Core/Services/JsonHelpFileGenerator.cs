using Doxie.Core.Models;
using Doxie.Core.XmlComments;
using Extenso;

namespace Doxie.Core.Services;

public static class JsonHelpFileGenerator
{
    private static readonly DocParser docParser = new();

    public static void Generate(IEnumerable<string> selectedAssemblyPaths, string outputPath, IEnumerable<string> probeDirectories = null)
    {
        docParser.PrepareDocumentation(selectedAssemblyPaths, probeDirectories);
        var assemblies = GetAssemblies(selectedAssemblyPaths);
        string outputFileName = Path.Combine(outputPath, "assemblies.json");
        assemblies.JsonSerialize().ToFile(outputFileName);
    }

    private static IEnumerable<AssemblyModel> GetAssemblies(IEnumerable<string> selectedAssemblyPaths) =>
        selectedAssemblyPaths.Select(GetAssembly).Where(assembly => assembly != null).ToArray();

    private static AssemblyModel GetAssembly(string filePath)
    {
        var assembly = docParser.Parse(filePath);
        if (assembly == null)
        {
            return null;
        }

        assembly.FileName = filePath;
        return assembly;
    }
}