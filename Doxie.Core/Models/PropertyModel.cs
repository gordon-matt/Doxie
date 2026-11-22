// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Doxie.Core.Models;

/// <summary>
/// A documented property or indexer. An indexer's <see cref="BaseCommentsModel.Name"/> is
/// <c>this[]</c>, and that name is also its id.
/// </summary>
public class PropertyModel : BaseCommentsModel
{
    /// <summary>The property's <see cref="System.Reflection.PropertyAttributes"/>, as that enum's name. Not the custom attributes.</summary>
    [JsonProperty]
    public string Attributes { get; set; }

    /// <summary>The type that declares this property. Not written to the JSON file.</summary>
    [JsonIgnore]
    public TypeModel Parent { get; set; }

    /// <summary>The declaring type's <see cref="BaseCommentsModel.Id"/>.</summary>
    [JsonProperty]
    public string ParentClass { get; set; }

    /// <summary>Short property type.</summary>
    [JsonProperty]
    public string Type { get; set; }

    /// <summary>Property type with its namespace.</summary>
    [JsonProperty]
    public string TypeFullName { get; set; }
}