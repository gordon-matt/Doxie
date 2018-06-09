using Newtonsoft.Json;

namespace Doxie.Core.Models
{
    public class ConstructorModel : BaseCodeComment
    {
        [JsonProperty]
        public string Attributes { get; set; }
    }
}