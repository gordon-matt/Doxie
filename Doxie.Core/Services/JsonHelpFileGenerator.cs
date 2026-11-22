using Doxie.Core.Models;
using Doxie.Core.XmlComments;
using Extenso;

namespace Doxie.Core.Services;

/// <summary>
/// Writes <c>assemblies.json</c> for the selected assemblies.
/// </summary>
public static class JsonHelpFileGenerator
{
    private static readonly DocParser docParser = new();

    /// <summary>
    /// Parses the selected assemblies and writes <c>assemblies.json</c> into
    /// <paramref name="outputPath"/>. An assembly that fails to load is still included, with
    /// the exception message in its full name and no namespaces. The directory must already exist.
    /// </summary>
    /// <param name="selectedAssemblyPaths">Assembly files to document.</param>
    /// <param name="outputPath">Directory that receives <c>assemblies.json</c>.</param>
    /// <param name="probeDirectories">
    /// Extra directories searched when a dependency of those assemblies has to be loaded.
    /// <see langword="null"/> adds none.
    /// </param>
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