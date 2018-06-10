// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Doxie.Core.Models
{
    public class ConstructorModel : BaseCodeComment
    {
        [JsonProperty]
        public string Attributes { get; set; }
    }
}