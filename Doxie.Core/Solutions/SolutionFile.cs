#nullable enable
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Doxie.Core.Solutions;

/// <summary>Lists the projects named by a <c>.sln</c> or <c>.slnx</c> file.</summary>
public static partial class SolutionFile
{
    private const string SolutionFolderType = "2150E333-8FDC-42A3-9474-1A3956D46DE8";

    /// <summary>
    /// Project paths in the solution. A <c>.slnx</c> file is read as XML. A <c>.sln</c> file is
    /// read from its <c>Project</c> lines, and solution folders are skipped. Only
    /// <c>.csproj</c>, <c>.vbproj</c>, and <c>.fsproj</c> paths are returned. Comparison is
    /// ordinal and case-insensitive, and the list is ordered that way.
    /// </summary>
    /// <param name="solutionPath">Path of the solution. A missing or blank path yields an empty list.</param>
    /// <returns>Full paths of the projects.</returns>
    /// <exception cref="System.Xml.XmlException">A <c>.slnx</c> file is not XML.</exception>
    public static IReadOnlyList<string> GetProjects(string solutionPath)
    {
        if (string.IsNullOrWhiteSpace(solutionPath) || !File.Exists(solutionPath))
        {
            return [];
        }

        string extension = Path.GetExtension(solutionPath);
        IEnumerable<string> relative = extension.Equals(".slnx", StringComparison.OrdinalIgnoreCase)
            ? ReadSlnx(solutionPath)
            : ReadSln(solutionPath);

        string directory = Path.GetDirectoryName(solutionPath) ?? Environment.CurrentDirectory;
        return relative
            .Select(path => Path.GetFullPath(Path.Combine(directory, path)))
            .Where(path => IsProjectFile(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool IsProjectFile(string path)
    {
        string extension = Path.GetExtension(path);
        return extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".vbproj", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".fsproj", StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> ReadSlnx(string solutionPath)
    {
        XDocument document = XDocument.Load(solutionPath);
        return document.Descendants()
            .Where(element => element.Name.LocalName == "Project")
            .Select(element => element.Attribute("Path")?.Value)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Cast<string>();
    }

    private static IEnumerable<string> ReadSln(string solutionPath)
    {
        foreach (string line in File.ReadLines(solutionPath))
        {
            Match match = ProjectLine().Match(line);
            if (!match.Success)
            {
                continue;
            }

            if (string.Equals(match.Groups["type"].Value, SolutionFolderType, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            yield return match.Groups["path"].Value;
        }
    }

    [GeneratedRegex("""^Project\("\{(?<type>[^}]+)\}"\)\s*=\s*"(?<name>[^"]*)"\s*,\s*"(?<path>[^"]+)"\s*,""", RegexOptions.IgnoreCase)]
    private static partial Regex ProjectLine();
}
