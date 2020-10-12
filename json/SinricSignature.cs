using Newtonsoft.Json;

namespace Sinric.json
{
    public class SinricSignature
    {
        [JsonProperty("HMAC")]
        public string Hmac { get; set; }
    }
}