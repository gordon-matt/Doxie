using Newtonsoft.Json;

namespace Doxie.Core.Models
{
    public class PropertyModel : BaseCommentsModel
    {
        [JsonProperty]
        public string Attributes { get; set; }

        [JsonIgnore]
        public TypeModel Parent { get; set; }

        [JsonProperty]
        public string ParentClass { get; set; }

        [JsonProperty]
        public string Type { get; set; }

        [JsonProperty]
        public string TypeFullName { get; set; }
    }
}