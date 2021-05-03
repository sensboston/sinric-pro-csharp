using System;
using System.Diagnostics;
using SinricLibrary.json;

namespace SinricLibrary.Devices
{
    public class SinricSmartLock : SinricDeviceBase
    {
        public class State
        {
            // when reporting state
            public const string Locked = "Locked";
            public const string Unlocked = "Unlocked";
            public const string Jammed = "Jammed";
        }

        internal class Receive
        {
            // when receiving state
            public const string Lock = "lock";
            public const string Unlock = "unlock";
        }
        internal class Actions
        {
            public const string SetLockState = "setLockState";
        }

        /// <summary>
        /// Called when the service requests to lock the device. Should return true if able to comply.
        /// </summary>
        public Func<bool> LockedAction;

        /// <summary>
        /// Called when the service requests to unlock the device. Should return true if able to comply.
        /// </summary>
        public Func<bool> UnlockedAction;

        public string CurrentState { get; private set; }

        public override string DeviceType { get; protected set; } = "SmartLock";

        public SinricSmartLock(string deviceId) : base(deviceId)
        {
        }

        /// <summary>
        /// Called when receiving a message from the server
        /// </summary>
        /// <param name="message">Message from server</param>
        /// <param name="reply">Pre-generated reply message template. Modify it to indicate the result.</param>
        internal override void MessageReceived(SinricClient client, SinricMessage message, SinricMessage reply)
        {
            switch (message.Payload.Action)
            {
                case Actions.SetLockState:
                    ReceiveNewLockState(message, reply);
                    break;

                default:
                    Debug.Print("SinricSmartLock received unrecognized action: " + message.Payload.Action);
                    break;
            }
        }

        /// <summary>
        /// Called when the server is asking to set the state of the lock
        /// </summary>
        /// <param name="message">Message from server</param>
        /// <param name="reply">Pre-generated reply message template. Modify it to indicate the result.</param>
        private void ReceiveNewLockState(SinricMessage message, SinricMessage reply)
        {
            var requestedState = message.Payload.GetValue<string>(SinricValue.State);

            // The service is requesting to change the state of the lock
            switch (requestedState)
            {
                case Receive.Lock:
                    if (LockedAction?.Invoke() ?? true)
                    {
                        // lock was successful
                        // confirm new state by replying with upper case "LOCKED"
                        reply.Payload.Success = SinricPayload.Result.Success;
                        CurrentState = State.Locked;
                    }

                    break;

                case Receive.Unlock:
                    if (UnlockedAction?.Invoke() ?? true)
                    {
                        // unlock was successful
                        // confirm new state by replying with upper case "UNLOCKED"
                        reply.Payload.Success = SinricPayload.Result.Success;
                        CurrentState = State.Unlocked;
                    }

                    break;

                default:
                    Debug.Print("SinricSmartLock received unrecognized state: " + requestedState);
                    break;
            }

            reply.Payload.SetValue(SinricValue.State, CurrentState.ToUpper());
        }

        public void SetNewState(string state)
        {
            CurrentState = state;

            var message = NewMessage(SinricPayload.MessageType.Event);
            message.Payload.SetCause(SinricCause.CauseType, SinricCause.PhysicalInteraction);
            message.Payload.SetValue(SinricValue.State, state.ToUpper());
            message.Payload.Action = Actions.SetLockState;
            OutgoingMessages.Enqueue(message);
        }
    }
}
