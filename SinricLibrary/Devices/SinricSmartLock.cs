using System;
using System.Diagnostics;
using SinricLibrary.json;

namespace SinricLibrary.Devices
{
    public class SinricSmartLock : SinricDeviceBase
    {
        public const string SmartLock = "SmartLock";
        public const string LockedState = "Locked";
        public const string UnlockedState = "Unlocked";

        public Action LockedAction;
        public Action UnlockedAction;

        public override string DeviceType { get; protected set; } = SmartLock;

        public SinricSmartLock(string deviceId) : base(deviceId)
        {
        }

        internal override void ProcessMessage(SinricClient client, SinricMessage message)
        {
            var reply = Utility.CreateReplyMessage(message, true);
            var state = message.Payload.GetValue<string>("state");

            switch (state)
            {
                case "lock":
                    // reply with upper case "LOCKED"
                    reply.Payload.SetValue("state", LockedState.ToUpper());
                    LockedAction?.Invoke();
                    break;

                case "unlock":
                    // reply with upper case "UNLOCKED"
                    reply.Payload.SetValue("state", UnlockedState.ToUpper());
                    UnlockedAction?.Invoke();
                    break;

                default:
                    Debug.Print("SinricSmartLock received unrecognized state: " + state);
                    reply.Payload.Success = false;
                    break;
            }

            client.AddMessageToQueue(reply);
        }

        public SinricSmartLock Lock()
        {
            //var message = Utility.NewMessage()
            return this;
        }

        public SinricSmartLock Unlock()
        {
            //var message = Utility.NewMessage()
            return this;
        }
    }
}
