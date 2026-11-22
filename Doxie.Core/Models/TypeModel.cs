// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Doxie.Core.Models;

/// <summary>
/// A visible type and the members declared on it. Inherited members are not copied onto the type.
/// </summary>
public class TypeModel : BaseCommentsModel
{
    /// <summary>Creates a type whose member lists are empty.</summary>
    public TypeModel()
    {
        Constructors = [];
        Methods = [];
        Properties = [];
        Events = [];
        Fields = [];
        Members = [];
        Parents = [];
        Interfaces = [];
    }

    /// <summary>Public and protected constructors declared on this type. Each id is a hash of its signature. Static constructors are omitted.</summary>
    [JsonProperty]
    public ICollection<ConstructorModel> Constructors { get; set; }

    /// <summary>Public and protected events declared on this type, ordered by name.</summary>
    [JsonProperty]
    public ICollection<EventModel> Events { get; set; }

    /// <summary>Public fields declared on this type, ordered by name. Empty for an enumeration.</summary>
    [JsonProperty]
    public ICollection<FieldModel> Fields { get; set; }

    /// <summary>
    /// <see langword="true"/> when the type is abstract. Interfaces and static classes are abstract.
    /// </summary>
    [JsonProperty]
    public bool IsAbstract { get; set; }

    /// <summary><see langword="true"/> when the type is nested.</summary>
    [JsonProperty]
    public bool IsNested { get; set; }

    /// <summary><see langword="true"/> for a primitive type.</summary>
    [JsonProperty]
    public bool IsPrimitive { get; set; }

    /// <summary>
    /// <see langword="true"/> when the type is public, or nested and public. A protected nested
    /// type is <see langword="false"/>.
    /// </summary>
    [JsonProperty]
    public bool IsPublic { get; set; }

    /// <summary>
    /// <see langword="true"/> when the type is sealed. Enumerations, structures, and static classes are sealed.
    /// </summary>
    [JsonProperty]
    public bool IsSealed { get; set; }

    /// <summary>
    /// Interfaces this type declares itself, as full names, ordered ordinally. Interfaces inherited
    /// through the base type are omitted. A failure leaves the list empty.
    /// </summary>
    [JsonProperty]
    public ICollection<string> Interfaces { get; set; }

    /// <summary>
    /// Enumeration members, ordered by name, excluding <c>value__</c>. Empty for every other kind of type.
    /// The description is the member's summary.
    /// </summary>
    [JsonProperty]
    public ICollection<MemberSummaryModel> Members { get; set; }

    /// <summary>Public and protected methods declared on this type, including operators and excluding other special names. Ordered by name.</summary>
    [JsonProperty]
    public ICollection<MethodModel> Methods { get; set; }

    /// <summary>The namespace this type was filed under. Not written to the JSON file.</summary>
    [JsonIgnore]
    public NamespaceModel Namespace { get; set; }

    /// <summary>Which kind of type this is.</summary>
    [JsonProperty]
    public ObjectType ObjectType { get; set; }

    /// <summary>Full name of the immediate base type, or an empty string when there is none.</summary>
    [JsonProperty]
    public string ParentClass { get; set; }

    /// <summary>
    /// The base-type chain. The furthest ancestor is first, so <c>object</c> leads a class chain.
    /// </summary>
    [JsonProperty]
    public ICollection<TypeSummaryModel> Parents { get; set; }

    /// <summary>Public and protected properties declared on this type, including indexers. Ordered by name.</summary>
    [JsonProperty]
    public ICollection<PropertyModel> Properties { get; set; }
}