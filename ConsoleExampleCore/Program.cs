using System;
using System.Collections.Generic;
using System.IO;
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

            var client = new SinricClient(AppKey, SecretKey, devices);

            client.SmartLocks("DemoLock").LockedAction = () =>
            {
                Console.WriteLine("Locked!");
                return true;
            };

            client.SmartLocks("DemoLock").UnlockedAction = () =>
            {
                Console.WriteLine("Unlocked!");
                return true;
            };

            client.Start();

            //client.SmartLocks("DemoLock").SetNewState(SinricSmartLock.State.Jammed);
            //client.SmartLocks("DemoLock").SetNewState(SinricSmartLock.State.Locked);
            //client.SmartLocks("DemoLock").SetNewState(SinricSmartLock.State.Unlocked);

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
