using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SinricLibrary.json
{
    internal class SinricPayload
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
        public SinricValue Value { get; set; } = new SinricValue();

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        public T GetValue<T>(string key) where T:class
        {
            Value.Fields.TryGetValue(key, out var value);

            return value?.Value<T>();
        }

        public SinricPayload SetValue(string key, object value)
        {
            if (value != null)
                Value.Fields[key] = JToken.FromObject(value);
            else
                Value.Fields[key] = null;

            return this;
        }
    }
}