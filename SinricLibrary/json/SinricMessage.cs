using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SinricLibrary.json
{
    internal class SinricMessage
    {

        [JsonProperty("timestamp")]
        public uint Timestamp { get; set; }

        [JsonIgnore]
        public DateTime TimestampUtc
        {
            get => DateTime.UnixEpoch.AddSeconds(Timestamp);
            set => Timestamp = (uint)value.Subtract(DateTime.UnixEpoch).TotalSeconds;
        }

        // other fields
        [JsonExtensionData] 
        public IDictionary<string, JToken> Fields { get; set; } = new Dictionary<string, JToken>();

        [JsonProperty("header")]
        public SinricHeader Header { get; set; } = new SinricHeader();

        // Raw payload needed for computing signature
        [JsonProperty("payload")]
        public JRaw RawPayload { get; set; }

        // Deserialized version of payload
        [JsonIgnore]
        public SinricPayload Payload { get; set; } = new SinricPayload();

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            Payload = JsonConvert.DeserializeObject<SinricPayload>(RawPayload?.Value as string ?? "");
        }

        [JsonProperty("signature")]
        public SinricSignature Signature { get; set; } = new SinricSignature();
    }
}
