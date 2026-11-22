// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Doxie.Core.Models;

/// <summary>
/// A documented constructor. <see cref="BaseCommentsModel.Name"/> is the declaring type's short
/// name, and the id is a hash of the signature.
/// </summary>
public class ConstructorModel : BaseCodeComment
{
    /// <summary>The constructor's <see cref="System.Reflection.MethodAttributes"/>, as that enum's name. Not the custom attributes.</summary>
    [JsonProperty]
    public string Attributes { get; set; }
}