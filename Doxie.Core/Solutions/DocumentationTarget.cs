#nullable enable

namespace Doxie.Core.Solutions;

/// <summary>
/// A project from a solution, and the built assembly Doxie can document.
/// </summary>
public sealed record DocumentationTarget
{
    public required string Name { get; init; }

    public required string ProjectPath { get; init; }

    public string? AssemblyPath { get; init; }

    public string OutputType { get; init; } = "Library";

    public string TargetFramework { get; init; } = string.Empty;

    public bool IsTestProject { get; init; }

    public bool HasXmlDocumentation { get; init; }

    /// <summary>
    /// Libraries are included by default. Tests and applications stay listed so they can be added.
    /// </summary>
    public bool SuggestInclude { get; init; }

    public string Status { get; init; } = string.Empty;

    public IReadOnlyList<string> ProbeDirectories { get; init; } = [];
}
