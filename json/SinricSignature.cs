using Newtonsoft.Json;

namespace Sinric.json
{
    internal class SinricSignature
    {
        [JsonProperty("HMAC")]
        public string Hmac { get; set; }
    }
}