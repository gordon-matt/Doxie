// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

namespace Doxie.Core.Models;

/// <summary>The types from one namespace, split by kind.</summary>
public class NamespaceModel
{
    /// <summary>Creates a namespace whose type lists are empty.</summary>
    public NamespaceModel()
    {
        Classes = [];
        Interfaces = [];
        Structures = [];
        Enumerations = [];
        Delegates = [];
    }

    /// <summary>
    /// Every class, interface, structure, enumeration, and delegate, ordered by name. Each read
    /// builds a new list.
    /// </summary>
    public ICollection<TypeModel> AllTypes => Classes
        .Union(Interfaces)
        .Union(Structures)
        .Union(Enumerations)
        .Union(Delegates)
        .OrderBy(t => t.Name)
        .ToList();

    /// <summary>Classes, ordered by name.</summary>
    public ICollection<TypeModel> Classes { get; set; }

    /// <summary>The number of types in <see cref="AllTypes"/>.</summary>
    public int CountAlltype => AllTypes.Count;

    /// <summary>Delegates, ordered by name.</summary>
    public ICollection<TypeModel> Delegates { get; set; }

    /// <summary>Enumerations, ordered by name.</summary>
    public ICollection<TypeModel> Enumerations { get; set; }

    /// <summary>Interfaces, ordered by name.</summary>
    public ICollection<TypeModel> Interfaces { get; set; }

    /// <summary>Namespace name. A type with no namespace stores <see langword="null"/>.</summary>
    public string Name { get; set; }

    /// <summary>Structures, ordered by name. Enumerations are not included.</summary>
    public ICollection<TypeModel> Structures { get; set; }

    /// <summary>Compares namespaces by <see cref="Name"/> only.</summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns><see langword="true"/> when <paramref name="obj"/> is a namespace with the same name.</returns>
    public override bool Equals(object obj) => obj is NamespaceModel b && Name == b.Name;

    /// <summary>The namespace name.</summary>
    /// <returns><see cref="Name"/>.</returns>
    public override string ToString() => Name;
}