using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LightExample
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

        //[OnSerializing]
        //private void OnSerializingMethod(StreamingContext context)
        //{
        //    RawPayload = new JRaw(JsonConvert.SerializeObject(Payload));
        //}

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            Payload = JsonConvert.DeserializeObject<SinrecPayload>(RawPayload?.Value as string ?? "");
        }

        [JsonProperty("signature")]
        public SinrecSignature Signature { get; set; } = new SinrecSignature();

    }

    public class SinrecHeader
    {
        [JsonProperty("payloadVersion")]
        public int PayloadVersion { get; set; } = 2;

        [JsonProperty("signatureVersion")]
        public int SignatureVersion { get; set; } = 1;
    }

    public class SinrecPayload
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("createdAt")]
        public uint CreatedAt { get; set; }

        [JsonIgnore]
        public DateTime CreatedAtUtc
        {
            get => DateTime.UnixEpoch.AddSeconds(CreatedAt);
            set => CreatedAt = (uint)value.Subtract(DateTime.UnixEpoch).TotalSeconds;
        }

        [JsonProperty("deviceAttributes")] 
        public List<object> DeviceAttributes { get; set; } = new List<object>();

        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("replyToken")]
        public string ReplyToken { get; set; }

        [JsonProperty("value")]
        public SinrecValue Value { get; set; } = new SinrecValue();

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        public T GetValue<T>(string key) where T:class
        {
            Value.Fields.TryGetValue(key, out var value);

            return value?.Value<T>();
        }

        public void SetValue(string key, object value)
        {
            if (value != null)
                Value.Fields[key] = JToken.FromObject(value);
            else
                Value.Fields[key] = null;
        }

    }

    public class SinrecSignature
    {
        [JsonProperty("HMAC")]
        public string Hmac { get; set; }
    }

    public class SinrecValue
    {
        // misc fields
        [JsonExtensionData]
        public IDictionary<string, JToken> Fields { get; set; } = new Dictionary<string, JToken>();
    }
}
