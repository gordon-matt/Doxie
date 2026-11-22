// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

namespace Doxie.Core.Models;

/// <summary>Which kind of type a <see cref="TypeModel"/> describes.</summary>
public enum ObjectType : byte
{
    /// <summary>The kind was not set.</summary>
    Unknown = 0,

    /// <summary>A value type that is not an enumeration.</summary>
    Structure = 1,

    /// <summary>A class.</summary>
    Class = 2,

    /// <summary>An interface.</summary>
    Interface = 3,

    /// <summary>An enumeration.</summary>
    Enumeration = 4,

    /// <summary>A delegate.</summary>
    Delegate = 5
}