using Doxie.Core.Models;
using Doxie.Core.XmlComments;
using Extenso;

namespace Doxie.Core.Services;

public static class JsonHelpFileGenerator
{
    private static readonly DocParser docParser = new();

    public static void Generate(IEnumerable<string> selectedAssemblyPaths, string outputPath)
    {
        var assemblies = GetAssemblies(selectedAssemblyPaths);
        string outputFileName = Path.Combine(outputPath, "assemblies.json");
        assemblies.JsonSerialize().ToFile(outputFileName);
    }

    private static IEnumerable<AssemblyModel> GetAssemblies(IEnumerable<string> selectedAssemblyPaths) =>
        selectedAssemblyPaths.Select(GetAssembly).ToArray();

    private static AssemblyModel GetAssembly(string filePath)
    {
        var assembly = docParser.Parse(filePath);
        assembly.FileName = filePath;
        return assembly;
    }
}