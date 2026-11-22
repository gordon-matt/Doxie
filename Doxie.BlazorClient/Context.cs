using Doxie.Core.Models;

namespace Doxie.BlazorClient;

/// <summary>The assemblies the Blazor help viewer is showing.</summary>
public static class Context
{
    /// <summary>Loaded help models. Empty until a file is opened.</summary>
    public static ICollection<AssemblyModel> Assemblies { get; set; } = [];
}