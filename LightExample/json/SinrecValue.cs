using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sinric.json
{
    public class SinrecValue
    {
        // misc fields
        [JsonExtensionData]
        public IDictionary<string, JToken> Fields { get; set; } = new Dictionary<string, JToken>();
    }
}