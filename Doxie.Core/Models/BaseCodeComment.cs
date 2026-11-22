// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Doxie.Core.Models;

/// <summary>A constructor or method, including its parameters.</summary>
public abstract class BaseCodeComment : BaseCommentsModel
{
    /// <summary>The parameter types from <see cref="ParameterTypes"/>, joined with commas and no spaces.</summary>
    [JsonProperty]
    public string FormattedParams => ParameterTypes();

    /// <summary>Parameters, in declaration order. The type text does not include the parameter name.</summary>
    [JsonProperty]
    public ICollection<ParameterModel> Parameters { get; set; }

    /// <summary>The type that declares this member. Not written to the JSON file.</summary>
    [JsonIgnore]
    public TypeModel Parent { get; set; }

    /// <summary>The declaring type's <see cref="BaseCommentsModel.Id"/>.</summary>
    [JsonProperty]
    public string ParentClass { get; set; }

    /// <summary>The parameter types, joined with commas and no spaces.</summary>
    /// <returns>The types, or an empty string when there are no parameters.</returns>
    public string ParameterTypes()
    {
        if (Parameters == null || !Parameters.Any())
        {
            return string.Empty;
        }

        var list = Parameters.Select(parameter => parameter.Type).ToList();

        return string.Join(",", list);
    }
}