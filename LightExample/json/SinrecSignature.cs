using Newtonsoft.Json;

namespace Sinric.json
{
    public class SinrecSignature
    {
        [JsonProperty("HMAC")]
        public string Hmac { get; set; }
    }
}