using Newtonsoft.Json;

namespace Sinric.json
{
    public class SinrecHeader
    {
        [JsonProperty("payloadVersion")]
        public int PayloadVersion { get; set; } = 2;

        [JsonProperty("signatureVersion")]
        public int SignatureVersion { get; set; } = 1;
    }
}