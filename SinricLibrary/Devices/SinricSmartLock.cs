using System;
using System.Diagnostics;
using SinricLibrary.json;

namespace SinricLibrary.Devices
{
    public class SinricSmartLock : SinricDeviceBase
    {
        public Action LockedAction;
        public Action UnlockedAction;

        internal override void ProcessMessage(SinricClient client, SinricMessage message)
        {
            var reply = Utility.CreateReplyMessage(message, true);
            var state = message.Payload.GetValue<string>("state");

            switch (state)
            {
                case "lock":
                    reply.Payload.SetValue("state", "LOCKED");
                    LockedAction?.Invoke();
                    break;

                case "unlock":
                    reply.Payload.SetValue("state", "UNLOCKED");
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
