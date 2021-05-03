using System;
using System.Collections.Concurrent;
using SinricLibrary.json;

namespace SinricLibrary.Devices
{
    public abstract class SinricDeviceBase
    {
        internal ConcurrentQueue<SinricMessage> OutgoingMessages { get; } = new ConcurrentQueue<SinricMessage>();
        public string DeviceId { get; private set; }
        public abstract string Type { get; protected set; }
        public string Name { get; set; }
        internal abstract void MessageReceived(SinricClient client, SinricMessage message, SinricMessage reply);

        protected SinricDeviceBase(string name, string deviceId)
        {
            Name = name;
            DeviceId = deviceId;
        }
        
        /// <summary>
        /// Creates a new  message with base information filled in for contacting the server.
        /// The caller must add remaining info & sign the message for it to be valid.
        /// </summary>
        /// <returns>A newly generated message will be returned</returns>
        internal SinricMessage NewMessage(string messageType)
        {
            var message = new SinricMessage
            {
                TimestampUtc = DateTime.UtcNow,
                Payload =
                {
                    DeviceId = DeviceId,
                    CreatedAtUtc = DateTime.UtcNow,
                    ReplyToken = Guid.NewGuid().ToString(),
                    Type = messageType,
                    Success = SinricPayload.Result.Success
                }
            };

            return message;
        }
    }
}
