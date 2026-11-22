// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Doxie.Core.Models;

/// <summary>A documented field. Enumeration values and compiler-generated fields are not included.</summary>
public class FieldModel : BaseCommentsModel
{
    /// <summary><see langword="true"/> for a <c>const</c> field. A <c>readonly</c> field is <see langword="false"/>.</summary>
    [JsonProperty]
    public bool IsConst { get; set; }

    /// <summary><see langword="true"/> when the field is static. A <c>const</c> field is static.</summary>
    [JsonProperty]
    public bool IsStatic { get; set; }

    /// <summary>Short field type.</summary>
    [JsonProperty]
    public string Type { get; set; }

    /// <summary>Field type with its namespace.</summary>
    [JsonProperty]
    public string TypeFullName { get; set; }
}