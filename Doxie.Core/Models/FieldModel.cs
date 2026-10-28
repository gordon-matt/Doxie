// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Doxie.Core.Models;

public class FieldModel : BaseCommentsModel
{
    [JsonProperty]
    public bool IsConst { get; set; }

    [JsonProperty]
    public bool IsStatic { get; set; }

    [JsonProperty]
    public string Type { get; set; }

    [JsonProperty]
    public string TypeFullName { get; set; }
}