#nullable enable
using System.Text.Json;
using System.Xml.Linq;

namespace Doxie.Core.Solutions;

public static class ProjectInspector
{
    private static readonly string[] TestPackages =
    [
        "Microsoft.NET.Test.Sdk",
        "xunit",
        "xunit.v3",
        "nunit",
        "MSTest.TestFramework",
        "TUnit"
    ];

    public static DocumentationTarget Inspect(string projectPath)
    {
        string name = Path.GetFileNameWithoutExtension(projectPath);
        if (!File.Exists(projectPath))
        {
            return Missing(name, projectPath, "Project file was not found.");
        }

        XDocument document;
        try
        {
            document = XDocument.Load(projectPath);
        }
        catch (Exception)
        {
            return Missing(name, projectPath, "Project file could not be read.");
        }

        string assemblyName = Property(document, "AssemblyName");
        if (string.IsNullOrEmpty(assemblyName))
        {
            assemblyName = name;
        }

        string sdk = document.Root?.Attribute("Sdk")?.Value ?? string.Empty;
        string outputType = Property(document, "OutputType");
        if (string.IsNullOrEmpty(outputType))
        {
            outputType = sdk.Contains(".Web", StringComparison.OrdinalIgnoreCase)
                || sdk.Contains(".Worker", StringComparison.OrdinalIgnoreCase)
                || sdk.Contains("Blazor", StringComparison.OrdinalIgnoreCase)
                ? "Exe"
                : "Library";
        }

        List<string> frameworks = Frameworks(document);
        bool isTest = IsTest(name, document);
        string projectDirectory = Path.GetDirectoryName(projectPath) ?? Environment.CurrentDirectory;
        LocatedAssembly located = Locate(projectDirectory, assemblyName, frameworks, Property(document, "OutputPath"));
        string framework = string.IsNullOrEmpty(located.TargetFramework)
            ? frameworks.LastOrDefault() ?? string.Empty
            : located.TargetFramework;

        var probes = new List<string>();
        if (!string.IsNullOrEmpty(located.AssemblyPath))
        {
            string outputDirectory = Path.GetDirectoryName(located.AssemblyPath) ?? string.Empty;
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                probes.Add(outputDirectory);
            }
        }

        probes.AddRange(PackageDirectories(projectDirectory, framework));

        string status = located.AssemblyPath == null
            ? "Not built"
            : located.HasXml ? "Ready" : "No XML documentation";

        return new DocumentationTarget
        {
            Name = assemblyName,
            ProjectPath = projectPath,
            AssemblyPath = located.AssemblyPath,
            OutputType = outputType,
            TargetFramework = framework,
            IsTestProject = isTest,
            HasXmlDocumentation = located.HasXml,
            SuggestInclude = !isTest && outputType.Equals("Library", StringComparison.OrdinalIgnoreCase),
            Status = status,
            ProbeDirectories = probes
        };
    }

    public static IReadOnlyList<DocumentationTarget> InspectSolution(string solutionPath) =>
        SolutionFile.GetProjects(solutionPath).Select(Inspect).ToList();

    private static DocumentationTarget Missing(string name, string projectPath, string status) =>
        new()
        {
            Name = name,
            ProjectPath = projectPath,
            Status = status
        };

    private static List<string> Frameworks(XDocument document)
    {
        string many = Property(document, "TargetFrameworks");
        if (!string.IsNullOrEmpty(many))
        {
            return many.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }

        string one = Property(document, "TargetFramework");
        return string.IsNullOrEmpty(one) ? [] : [one];
    }

    private static bool IsTest(string projectName, XDocument document)
    {
        string flag = Property(document, "IsTestProject");
        if (flag.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (projectName.EndsWith(".Tests", StringComparison.OrdinalIgnoreCase)
            || projectName.EndsWith(".Test", StringComparison.OrdinalIgnoreCase)
            || projectName.EndsWith("Tests", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return document.Descendants()
            .Where(element => element.Name.LocalName == "PackageReference")
            .Select(element => element.Attribute("Include")?.Value ?? element.Attribute("Update")?.Value)
            .Any(package => TestPackages.Contains(package, StringComparer.OrdinalIgnoreCase));
    }

    private static LocatedAssembly Locate(string projectDirectory, string assemblyName, IReadOnlyList<string> frameworks, string outputPath)
    {
        var candidates = new List<LocatedAssembly>();
        string[] configurations = ["Release", "Debug"];
        IReadOnlyList<string> tfms = frameworks.Count == 0 ? [string.Empty] : frameworks;

        foreach (string tfm in tfms.Reverse())
        {
            foreach (string configuration in configurations)
            {
                AddCandidate(candidates, DefaultOutput(projectDirectory, configuration, tfm), assemblyName, tfm);
            }
        }

        if (!string.IsNullOrEmpty(outputPath))
        {
            string folder = Path.IsPathRooted(outputPath) ? outputPath : Path.Combine(projectDirectory, outputPath);
            AddCandidate(candidates, folder, assemblyName, tfms.LastOrDefault() ?? string.Empty);
        }

        LocatedAssembly documented = candidates.FirstOrDefault(candidate => candidate.HasXml);
        if (documented.AssemblyPath != null)
        {
            return documented;
        }

        return candidates.FirstOrDefault();
    }

    private static string DefaultOutput(string projectDirectory, string configuration, string tfm)
    {
        string folder = Path.Combine(projectDirectory, "bin", configuration);
        return string.IsNullOrEmpty(tfm) ? folder : Path.Combine(folder, tfm);
    }

    private static void AddCandidate(List<LocatedAssembly> candidates, string folder, string assemblyName, string tfm)
    {
        string dll = Path.Combine(folder, assemblyName + ".dll");
        if (!File.Exists(dll))
        {
            return;
        }

        candidates.Add(new LocatedAssembly(dll, tfm, File.Exists(Path.ChangeExtension(dll, ".xml"))));
    }

    private static IEnumerable<string> PackageDirectories(string projectDirectory, string targetFramework)
    {
        string assetsPath = Path.Combine(projectDirectory, "obj", "project.assets.json");
        if (!File.Exists(assetsPath))
        {
            yield break;
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(File.ReadAllText(assetsPath));
        }
        catch (Exception)
        {
            yield break;
        }

        using (document)
        {
            JsonElement root = document.RootElement;
            if (!root.TryGetProperty("packageFolders", out JsonElement folders)
                || !root.TryGetProperty("libraries", out JsonElement libraries)
                || !root.TryGetProperty("targets", out JsonElement targets))
            {
                yield break;
            }

            string? packageRoot = folders.EnumerateObject().Select(folder => folder.Name).FirstOrDefault();
            if (string.IsNullOrEmpty(packageRoot) || !TryGetTarget(targets, targetFramework, out JsonElement target))
            {
                yield break;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (JsonProperty package in target.EnumerateObject())
            {
                if (!TryGetAssets(package.Value, out JsonElement assets)
                    || !libraries.TryGetProperty(package.Name, out JsonElement library)
                    || !library.TryGetProperty("path", out JsonElement libraryPath))
                {
                    continue;
                }

                foreach (JsonProperty asset in assets.EnumerateObject())
                {
                    if (!asset.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string relative = asset.Name.Replace('/', Path.DirectorySeparatorChar);
                    string fullPath = Path.GetFullPath(Path.Combine(packageRoot, libraryPath.GetString() ?? string.Empty, relative));
                    string? directory = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(directory) && seen.Add(directory))
                    {
                        yield return directory;
                    }
                }
            }
        }
    }

    private static bool TryGetTarget(JsonElement targets, string targetFramework, out JsonElement target)
    {
        if (!string.IsNullOrEmpty(targetFramework) && targets.TryGetProperty(targetFramework, out target))
        {
            return true;
        }

        foreach (JsonProperty candidate in targets.EnumerateObject())
        {
            target = candidate.Value;
            return true;
        }

        target = default;
        return false;
    }

    private static bool TryGetAssets(JsonElement package, out JsonElement assets)
    {
        if (package.TryGetProperty("runtime", out assets) && assets.EnumerateObject().Any())
        {
            return true;
        }

        return package.TryGetProperty("compile", out assets);
    }

    private static string Property(XDocument document, string name)
    {
        List<XElement> matches = document.Descendants()
            .Where(element => element.Name.LocalName == name && element.Parent?.Name.LocalName == "PropertyGroup")
            .ToList();
        List<XElement> unconditional = matches.Where(element => !IsConditional(element)).ToList();
        List<XElement> chosen = unconditional.Count > 0 ? unconditional : matches;
        return chosen.LastOrDefault()?.Value.Trim() ?? string.Empty;
    }

    private static bool IsConditional(XElement element) =>
        element.Attribute("Condition") != null || element.Parent?.Attribute("Condition") != null;

    private readonly record struct LocatedAssembly(string? AssemblyPath, string TargetFramework, bool HasXml);
}
