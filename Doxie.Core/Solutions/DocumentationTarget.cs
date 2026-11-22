#nullable enable

namespace Doxie.Core.Solutions;

/// <summary>
/// A project from a solution, and the built assembly Doxie can document.
/// </summary>
public sealed record DocumentationTarget
{
    /// <summary>Assembly or project name shown in the list.</summary>
    public required string Name { get; init; }

    /// <summary>Path of the project file.</summary>
    public required string ProjectPath { get; init; }

    /// <summary>
    /// Built assembly, or <see langword="null"/> when no <c>.dll</c> was found. A documented
    /// build is preferred over one that has no XML file. Release is preferred over Debug, and
    /// the last target framework is searched first.
    /// </summary>
    public string? AssemblyPath { get; init; }

    /// <summary>
    /// <c>Library</c> or <c>Exe</c>. A missing value is <c>Exe</c> for a web, worker, or Blazor
    /// SDK, and <c>Library</c> otherwise.
    /// </summary>
    public string OutputType { get; init; } = "Library";

    /// <summary>Target framework of the located assembly, or the last framework the project names.</summary>
    public string TargetFramework { get; init; } = string.Empty;

    /// <summary>
    /// <see langword="true"/> when the project sets <c>IsTestProject</c>, its name ends with
    /// <c>Tests</c> or <c>.Test</c>, or it references Microsoft.NET.Test.Sdk, xunit, xunit.v3,
    /// nunit, MSTest.TestFramework, or TUnit.
    /// </summary>
    public bool IsTestProject { get; init; }

    /// <summary><see langword="true"/> when an XML file sits beside <see cref="AssemblyPath"/>.</summary>
    public bool HasXmlDocumentation { get; init; }

    /// <summary>
    /// Libraries are included by default. Tests and applications stay listed so they can be added.
    /// </summary>
    public bool SuggestInclude { get; init; }

    /// <summary>
    /// <c>Ready</c>, <c>No XML documentation</c>, <c>Not built</c>, or why the project file
    /// could not be read.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Directories that hold the built assembly and the NuGet assemblies named in
    /// <c>project.assets.json</c>. Empty when the project could not be read.
    /// </summary>
    public IReadOnlyList<string> ProbeDirectories { get; init; } = [];
}
