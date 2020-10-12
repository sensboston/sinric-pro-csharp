using System;
using System.Diagnostics;
using SinricLibrary.json;

namespace SinricLibrary.Devices
{
    public class SinricSmartLock : SinricDeviceBase
    {
        public const string SmartLock = "SmartLock";
        public const string Locked = "Locked";
        public const string Unlocked = "Unlocked";

        public Func<bool> LockedAction;
        public Func<bool> UnlockedAction;

        public string CurrentState { get; private set; }

        public override string DeviceType { get; protected set; } = SmartLock;

        public SinricSmartLock(string deviceId) : base(deviceId)
        {
        }

        internal override void ProcessMessage(SinricClient client, SinricMessage message)
        {
            var reply = Utility.CreateReplyMessage(message, true);
            var requestedState = message.Payload.GetValue<string>("state");
            var newState = CurrentState;

            switch (requestedState)
            {
                case "lock":
                    // reply with upper case "LOCKED"
                    newState = Locked;
                    reply.Payload.Success = LockedAction?.Invoke() ?? true;
                   
                    break;

                case "unlock":
                    // reply with upper case "UNLOCKED"
                    newState = Unlocked;
                    reply.Payload.Success = UnlockedAction?.Invoke() ?? true;

                    break;

                default:
                    Debug.Print("SinricSmartLock received unrecognized state: " + requestedState);
                    reply.Payload.Success = false;
                    break;
            }

            if (reply.Payload.Success)
            {
                CurrentState = newState;
            }

            reply.Payload.SetValue("state", CurrentState.ToUpper());
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
