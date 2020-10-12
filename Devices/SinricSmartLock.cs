using System;
using System.Collections.Generic;
using System.Text;
using Sinric.json;

namespace Sinric.Devices
{
    public class SinricSmartLock : SinricDeviceBase
    {
        internal override void ProcessMessage(SinricClient client, SinricMessage message)
        {
            var reply = Utility.CreateReplyMessage(message, true);
            var state = message.Payload.GetValue<string>("state");

            switch (state)
            {
                case "lock":
                    reply.Payload.SetValue("state", "LOCKED");
                    break;
                case "unlock":
                    reply.Payload.SetValue("state", "UNLOCKED");
                    break;
                default:
                    Console.WriteLine("SinricSmartLock received unrecognized state: " + state);
                    reply.Payload.Success = false;
                    break;
            }

            client.SendMessage(reply);
        }
    }
}
