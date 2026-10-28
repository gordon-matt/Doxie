// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Doxie.Core.Models;

public class EventModel : BaseCommentsModel
{
    [JsonProperty]
    public string Type { get; set; }

    [JsonProperty]
    public string TypeFullName { get; set; }
}