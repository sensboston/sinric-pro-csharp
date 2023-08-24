using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using SinricLibrary;
using SinricLibrary.Devices;

namespace gh4pc
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);

        // See all virtual key codes at https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
        const int KEYEVENTF_EXTENTEDKEY = 1;
        const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        const int VK_VOLUME_MUTE = 0xAD;
        const int VK_VOLUME_DOWN = 0xAE;
        const int VK_VOLUME_UP = 0xAF;

        const string APP_KEY = "YOUR_APP_KEY_HERE";             // Should look like "de0bxxxx-1x3x-4x3x-ax2x-5dabxxxxxxxx"
        const string APP_SECRET = "YOUR_APP_SECRET_HERE";       // Should look like "5f36xxxx-x3x7-4x3x-xexe-e86724a9xxxx-4c4axxxx-3x3x-x5xe-x9x3-333d65xxxxxx"
        const string DEVICE_ID = "YOUR_DEVICE_ID_HERE";         // Should look like "5dc1564130xxxxxxxxxxxxxx"
        const string DEVICE_NAME = "My PC";                     // Should be the same as thermostat device name on SinricPro dashboard
        const string SERVER_URL = "ws://ws.sinric.pro";

        static SinricClient client;

        [STAThreadAttribute]
        static void Main()
        {
            NetworkChange.NetworkAddressChanged += (s, e) =>
            {
                client?.Stop();
                client = null;  
                ConnectToSinric();
            };

            ConnectToSinric();
            while (true)
            {
                client?.ProcessIncomingMessages();
                Thread.Sleep(500);
            }
        }

        static void ConnectToSinric()
        { 
            var devices = new List<SinricDeviceBase>
            {
                new SinricThermostat(DEVICE_NAME, DEVICE_ID)
            };
            client = new SinricClient(APP_KEY, APP_SECRET, devices) { SinricAddress = SERVER_URL };
            client.Thermostats(DEVICE_NAME).SetHandler<StateEnums.TargetTemperatureState>(info =>
            {
                // Play / pause
                switch (info.NewState)
                {
                    case "10": case "11":
                        keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
                        break;

                    // Mute
                    case "12":
                        keybd_event(VK_VOLUME_MUTE, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
                        break;

                    // Volume up
                    case "13":
                        for (int i = 0; i < 10; i++) keybd_event(VK_VOLUME_UP, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
                        break;

                    // Volume down
                    case "14":
                        for (int i = 0; i < 10; i++) keybd_event(VK_VOLUME_DOWN, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
                        break;

                    // F key for fullscreen toggle
                    case "15":
                        keybd_event(0x46, 0, 0, IntPtr.Zero);
                        break;
                }
            });

            client.Start();
            client.Thermostats(DEVICE_NAME).SendNewState(StateEnums.PowerState.On);
        }
    }
}
