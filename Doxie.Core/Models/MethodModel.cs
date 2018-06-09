using System.Collections.Generic;
using Newtonsoft.Json;

namespace Doxie.Core.Models
{
    public class MethodModel : BaseCodeComment
    {
        public MethodModel()
        {
            Parameters = new List<ParameterModel>();
        }

        [JsonProperty]
        public string ReturnType { get; set; }

        [JsonProperty]
        public string ReturnTypeFullName { get; set; }
    }
}