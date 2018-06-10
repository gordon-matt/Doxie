// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using System.Globalization;
using Newtonsoft.Json;

namespace Doxie.Core.Models
{
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
        public string Id => UseHashCodeForId ? FullName.GetHashCode().ToString(CultureInfo.InvariantCulture) : Name;

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
}