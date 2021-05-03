using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using SinricLibrary;
using SinricLibrary.Devices;

namespace ConsoleExampleCore
{
    class Program
    {
        // identifies the account
        private static string AppKey { get; set; } = "your app key";

        // for validating messages sent to and from sinric
        private static string SecretKey { get; set; } = "your secret key";

        // for identifying a specific device in the account
        private static string DeviceId { get; set; } = "your device id";

        public static void Main(string[] args)
        {

            var smartLock = new SinricSmartLock(DeviceId)
            {
                LockedAction = () =>
                {
                    Console.WriteLine("Locked!");
                    return true;
                },
                UnlockedAction = () =>
                {
                    Console.WriteLine("Unlocked!");
                    return true;
                }
            };
            
            SinricDeviceBase [] devices =
            {
                smartLock
            };

            var client = new SinricClient(AppKey, SecretKey, devices);

            client.Start();

            //smartLock.SetNewState(SinricSmartLock.State.Jammed);
            //smartLock.SetNewState(SinricSmartLock.State.Locked);
            //smartLock.SetNewState(SinricSmartLock.State.Unlocked);

            while (true)
            {
                client.ProcessIncomingMessages();

                Thread.Sleep(100);
            }

            // example runs perpetually
            // client.Stop();
        }
    }
}
