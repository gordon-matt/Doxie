#nullable enable
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Doxie.Core.Solutions;

public static partial class SolutionFile
{
    private const string SolutionFolderType = "2150E333-8FDC-42A3-9474-1A3956D46DE8";

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
