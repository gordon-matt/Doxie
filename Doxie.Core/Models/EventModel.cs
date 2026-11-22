// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Doxie.Core.Models;

/// <summary>A documented event.</summary>
public class EventModel : BaseCommentsModel
{
    /// <summary>Short delegate type.</summary>
    [JsonProperty]
    public string Type { get; set; }

    /// <summary>Delegate type with its namespace.</summary>
    [JsonProperty]
    public string TypeFullName { get; set; }
}