// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

namespace Doxie.Core.Models;

/// <summary>
/// One assembly in the generated help file. A failed load is still a model: its full name is
/// the path plus the exception message, and it has no namespaces.
/// </summary>
public class AssemblyModel
{
    /// <summary>Path that was parsed. Set when the help file is written.</summary>
    public string FileName { get; set; }

    /// <summary>The assembly's full name, or the path and exception message when loading failed.</summary>
    public string FullName { get; set; }

    /// <summary>The module version id, or a stable hash of the path when loading failed.</summary>
    public Guid Id { get; set; }

    /// <summary>The assembly's simple name, or the file name without its extension when loading failed.</summary>
    public string Name { get; set; }

    /// <summary>Namespaces, ordered by name. Empty when loading failed.</summary>
    public ICollection<NamespaceModel> Namespaces { get; set; }
}