// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

namespace Doxie.Core.Models;

/// <summary>A type named as a base type or interface, without its members.</summary>
public class TypeSummaryModel
{
    /// <summary>Namespace-qualified name.</summary>
    public string FullName { get; set; }

    /// <summary>Type name.</summary>
    public string Name { get; set; }
}