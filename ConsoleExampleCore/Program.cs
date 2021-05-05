using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SinricLibrary;
using SinricLibrary.Devices;

namespace ConsoleExampleCore
{
    internal class Program
    {
        internal static IConfigurationRoot Configuration;

        // identifies the account
        private static string AppKey { get; set; }

        // for validating messages sent to and from sinric
        private static string SecretKey { get; set; }

        public static void Main(string[] args)
        {

            Setup(args);

            AppKey = Configuration["AppKey"];
            SecretKey = Configuration["SecretKey"];

            // you can put your settings and devices directly in appsettings.json, or make an appsettings.private.json (and exclude from git)
            var devices = LoadDevices();

            var client = new SinricClient(AppKey, SecretKey, devices)
            {
                SinricAddress = Configuration["SinricAddress"]
            };
            
            client.Start();

            Console.WriteLine("First, you will need a valid Sinric account. Create a fake 'Smart Lock' device in the Sinric Dashboard.");
            Console.WriteLine(); 
            Console.WriteLine("Next, you will need to copy appsettings.json to appsettings.private.json");
            Console.WriteLine("  Set the build action: 'Content', copy to output directory: 'Copy if newer'");
            Console.WriteLine();
            Console.WriteLine("Follow the instructions to link the Sinric smart skill to your Alexa account.");
            Console.WriteLine("Go into your Alexa app and perform 'Discover Devices' -- then you can create routines using the fake Smart Lock."); 
            Console.WriteLine("To see something happen, open the Sinric dashboard and click 'Lock' or 'Unlock'");
            Console.WriteLine("To set off an event, uncomment one of the 'SetNewState' lines, or hook into some other event on your PC ...");

            //client.SmartLocks("DemoLock").SetNewState(SinricSmartLock.State.Jammed);
            //client.SmartLocks("DemoLock").SetNewState(SinricSmartLock.State.Locked);
            //client.SmartLocks("DemoLock").SetNewState(SinricSmartLock.State.Unlocked);
            client.ContactSensors("Kitchen Door").SetHandler(StateEnums.PowerState.On, info =>
            {
                Debug.Print($"Power state for kitchen door changed to {info.NewState}");
                info.Device.SendNewState(StateEnums.ContactState.Open);
            });

            client.ContactSensors("Kitchen Door").SetHandler(StateEnums.PowerState.Off, info =>
            {
                Debug.Print($"Power state for kitchen door changed to {info.NewState}");
                info.Device.SendNewState(StateEnums.ContactState.Closed);
            });

            client.SmartLocks("DemoLock").SetHandler<StateEnums.LockState>(info =>
            {
                // fix / translate to proper state reply
                if (info.NewState == "lock")
                    info.NewState = SinricMessageAttribute.Get(StateEnums.LockState.Lock).SendValue;

                if (info.NewState == "unlock")
                    info.NewState = SinricMessageAttribute.Get(StateEnums.LockState.Unlock).SendValue;
            });


            client.SmartLocks("DemoLock").SetHandler(StateEnums.LockState.Lock, info =>
            {
                Debug.Print($"Better lock your doors!");
            });

            client.SmartLocks("DemoLock").SetHandler(StateEnums.LockState.Unlock, info =>
            {
                Debug.Print($"Unlocked!");
            });

            client.SmartLocks("DemoLock").SendNewState(StateEnums.LockState.Lock);

            while (true)
            {
                client.ProcessIncomingMessages();

                Thread.Sleep(100);
            }

            // example runs perpetually
            // client.Stop();
        }

        private static List<SinricDeviceBase> LoadDevices()
        {
            var devices = new List<SinricDeviceBase>();

            foreach (var entry in Configuration.GetSection("Devices").Get<List<DeviceEntry>>())
            {
                switch (entry.Type)
                {
                    case SinricDeviceTypes.SmartLock:

                        devices.Add(new SinricSmartLock(entry.Name, entry.DeviceId));
                        break;

                    case SinricDeviceTypes.ContactSensor:
                        devices.Add(new SinricContactSensor(entry.Name, entry.DeviceId));
                        break;

                    default:
                        throw new Exception($"Unrecognized device type in configuration: {entry.Type}");
                }
            }

            return devices;
        }

        private static void Setup(string[] args)
        {
            // Create service collection
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // Create service provider
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Build configuration
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("appsettings.private.json", true)
                .Build();

            // Add access to generic IConfigurationRoot
            serviceCollection.AddSingleton<IConfigurationRoot>(Configuration);

            // Add app
            //serviceCollection.AddTransient<App>();
        }
    }
}
