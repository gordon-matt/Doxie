using Doxie.Core.Models;

namespace Doxie.BlazorClient;

public static class Context
{
    public static ICollection<AssemblyModel> Assemblies { get; set; } = [];
}