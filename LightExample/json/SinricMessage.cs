using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sinric.json
{
    public class SinricMessage
    {
        public SinricMessage CreateReply(bool result)
        {
            var reply = new SinricMessage
            {
                TimestampUtc = DateTime.UtcNow,
                Payload =
                {
                    DeviceId = Payload.DeviceId,
                    ReplyToken = Payload.ReplyToken,
                    Type = "response",
                    Message = "OK",
                    Success = result,
                    ClientId = "csharp",
                    CreatedAtUtc = DateTime.UtcNow,
                    Action = Payload.Action,
                }
            };

            return reply;
        }

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
        public SinrecHeader Header { get; set; } = new SinrecHeader();

        // Raw payload needed for computing signature
        [JsonProperty("payload")]
        public JRaw RawPayload { get; set; }

        // Deserialized version of payload
        [JsonIgnore]
        public SinrecPayload Payload { get; set; } = new SinrecPayload();

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            Payload = JsonConvert.DeserializeObject<SinrecPayload>(RawPayload?.Value as string ?? "");
        }

        [JsonProperty("signature")]
        public SinrecSignature Signature { get; set; } = new SinrecSignature();

    }
}
