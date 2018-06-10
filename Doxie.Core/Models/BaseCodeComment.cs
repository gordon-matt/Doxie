// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Doxie.Core.Models
{
    public abstract class BaseCodeComment : BaseCommentsModel
    {
        [JsonProperty]
        public string FormattedParams => ParameterTypes();

        [JsonProperty]
        public ICollection<ParameterModel> Parameters { get; set; }

        [JsonIgnore]
        public TypeModel Parent { get; set; }

        [JsonProperty]
        public string ParentClass { get; set; }

        /// <summary>
        /// Returns the parameters types, comma seperated.
        /// </summary>
        /// <returns></returns>
        public string ParameterTypes()
        {
            if (Parameters == null || !Parameters.Any())
            {
                return string.Empty;
            }

            var list = Parameters.Select(parameter => parameter.Type).ToList();

            return string.Join(",", list);
        }
    }
}