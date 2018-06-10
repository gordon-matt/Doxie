// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Doxie.Core.Models
{
    public class ParameterModel
    {
        [JsonProperty]
        public string Attributes { get; set; }

        [JsonProperty]
        public string Description { get; set; }

        [JsonProperty]
        public bool IsOut { get; set; }

        [JsonProperty]
        public bool IsRetval { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string Type { get; set; }

        [JsonProperty]
        public string TypeFullName { get; set; }

        public override string ToString()
        {
            return $"{Type} {Name}";
        }
    }
}