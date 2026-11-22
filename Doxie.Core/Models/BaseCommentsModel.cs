// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Doxie.Core.Models;

/// <summary>
/// Name, signature, and the rendered documentation shared by every documented member.
/// </summary>
public class BaseCommentsModel
{
    /// <summary>Creates a member whose comment fields are empty strings.</summary>
    public BaseCommentsModel()
    {
        FullName = string.Empty;
        Summary = string.Empty;
        Remarks = string.Empty;
        Returns = string.Empty;
        Example = string.Empty;
        Exceptions = string.Empty;
    }

    /// <summary>Rendered <c>example</c> text. Empty when the member has none.</summary>
    [JsonProperty]
    public string Example { get; set; }

    /// <summary>Rendered <c>exception</c> lines. Empty when the member has none.</summary>
    [JsonProperty]
    public string Exceptions { get; set; }

    /// <summary>
    /// Signature or type name. When reading a member throws, this is the exception message and
    /// <see cref="LoadError"/> is set.
    /// </summary>
    [JsonProperty]
    public string FullName { get; set; }

    /// <summary>
    /// <see cref="Name"/>, or the first 12 hexadecimal characters of the SHA-256 of
    /// <see cref="FullName"/> when <see cref="UseHashCodeForId"/> is set. This is not
    /// <see cref="object.GetHashCode"/>.
    /// </summary>
    [JsonProperty]
    public string Id => UseHashCodeForId ? StableId(FullName) : Name;

    /// <summary>
    /// String hash codes change between processes, so overload ids are derived from the signature.
    /// </summary>
    private static string StableId(string value)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty));
        return System.Convert.ToHexString(hash)[..12];
    }

    /// <summary>
    /// <see langword="true"/> when the member was kept after reading it threw. <see cref="FullName"/>
    /// then holds the exception message.
    /// </summary>
    [JsonProperty]
    public bool LoadError { get; set; }

    /// <summary>Member or type name, without its signature.</summary>
    [JsonProperty]
    public string Name { get; set; }

    /// <summary>
    /// Rendered remarks, then the type-parameter lines, then a "See also" sentence. A blank line
    /// separates the parts that are present. Empty when the member has none of them.
    /// </summary>
    [JsonProperty]
    public string Remarks { get; set; }

    /// <summary>Rendered <c>returns</c> text. Empty when the member has none.</summary>
    [JsonProperty]
    public string Returns { get; set; }

    /// <summary>Rendered <c>summary</c> text. Empty when the member has none.</summary>
    [JsonProperty]
    public string Summary { get; set; }

    /// <summary>
    /// When <see langword="true"/>, <see cref="Id"/> is a hash of <see cref="FullName"/>.
    /// Constructors set this. A method sets it when another method of the same name was already recorded.
    /// </summary>
    [JsonIgnore]
    public bool UseHashCodeForId { get; set; }
}