// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Doxie.Core.Models;

/// <summary>
/// A documented method. <see cref="BaseCommentsModel.Name"/> is the formatted name, so an operator
/// reads <c>operator +</c>. A generic method, an operator, and every overload after the first of
/// the same name use a hashed id.
/// </summary>
public class MethodModel : BaseCodeComment
{
    /// <summary>Creates a method with an empty parameter list.</summary>
    public MethodModel()
    {
        Parameters = [];
    }

    /// <summary>Short return type, including <c>dynamic</c> and a nullable annotation.</summary>
    [JsonProperty]
    public string ReturnType { get; set; }

    /// <summary>Return type with its namespace.</summary>
    [JsonProperty]
    public string ReturnTypeFullName { get; set; }
}