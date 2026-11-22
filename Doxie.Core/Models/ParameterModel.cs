// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Doxie.Core.Models;

/// <summary>One parameter of a constructor or method.</summary>
public class ParameterModel
{
    /// <summary>Not filled in. Modifiers such as <c>ref</c> and <c>this</c> are part of <see cref="Type"/>.</summary>
    [JsonProperty]
    public string Attributes { get; set; }

    /// <summary>Rendered <c>param</c> text. Matched by name when any documented name matches, otherwise by position.</summary>
    [JsonProperty]
    public string Description { get; set; }

    /// <summary><see langword="true"/> for an <c>out</c> parameter. A <c>ref</c> parameter is <see langword="false"/>.</summary>
    [JsonProperty]
    public bool IsOut { get; set; }

    /// <summary>The compiler's return-value parameter flag. Ordinary parameters are <see langword="false"/>.</summary>
    [JsonProperty]
    public bool IsRetval { get; set; }

    /// <summary>Parameter name.</summary>
    [JsonProperty]
    public string Name { get; set; }

    /// <summary>Type text, including modifiers, without the parameter name or a default value.</summary>
    [JsonProperty]
    public string Type { get; set; }

    /// <summary>The same type text with the full type name, and a default value when the parameter has one.</summary>
    [JsonProperty]
    public string TypeFullName { get; set; }

    /// <summary>The type text and the parameter name.</summary>
    /// <returns><c>Type Name</c>.</returns>
    public override string ToString() => $"{Type} {Name}";
}