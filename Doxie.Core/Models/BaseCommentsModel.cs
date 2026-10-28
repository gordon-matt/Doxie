// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Doxie.Core.Models;

public class BaseCommentsModel
{
    public BaseCommentsModel()
    {
        FullName = string.Empty;
        Summary = string.Empty;
        Remarks = string.Empty;
        Returns = string.Empty;
        Example = string.Empty;
        Exceptions = string.Empty;
    }

    [JsonProperty]
    public string Example { get; set; }

    [JsonProperty]
    public string Exceptions { get; set; }

    [JsonProperty]
    public string FullName { get; set; }

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

    [JsonProperty]
    public bool LoadError { get; set; }

    [JsonProperty]
    public string Name { get; set; }

    [JsonProperty]
    public string Remarks { get; set; }

    [JsonProperty]
    public string Returns { get; set; }

    [JsonProperty]
    public string Summary { get; set; }

    [JsonIgnore]
    public bool UseHashCodeForId { get; set; }
}