using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Bluegrams.Application;
using GHPCControl.Properties;
using SinricLibrary;
using SinricLibrary.Devices;
using SuperSocket.ClientEngine;
using WebSocket4Net;

namespace GHPCControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SinricClient client;
        public MainWindow()
        {
            PortableSettingsProvider.ApplyProvider(Settings.Default);
            InitializeComponent();
            this.Closed += (_, __) => { Settings.Default.Save(); };
            this.Loaded += (_, __) => { startStopButton.IsEnabled = AllSettingsAreSet(); };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Content.Equals("Start"))
            {
                var devices = new List<SinricDeviceBase>();
                devices.Add(new SinricThermostat(Settings.Default.DEVICE_NAME, Settings.Default.DEVICE_KEY));
                client = new SinricClient(Settings.Default.APP_KEY, Settings.Default.APP_SECRET, devices);
                client.Thermostats(Settings.Default.DEVICE_NAME).SetHandler<StateEnums.TargetTemperatureState>(info =>
                {
                    var t = info.NewState;
                });

                client.Start();
                client.Thermostats(Settings.Default.DEVICE_NAME).SendNewState(StateEnums.PowerState.On);

                (sender as Button).Content = "Stop";
            }
            else
            {
                if (client != null)
                {
                    client.Stop();
                    client = null;
                    (sender as Button).Content = "Start";
                }
            }
        }

        private bool AllSettingsAreSet()
        {
            return !(string.IsNullOrEmpty(Settings.Default.APP_KEY) || string.IsNullOrEmpty(Settings.Default.APP_SECRET) ||
                     string.IsNullOrEmpty(Settings.Default.DEVICE_KEY) || string.IsNullOrEmpty(Settings.Default.DEVICE_NAME));
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (startStopButton != null) startStopButton.IsEnabled = AllSettingsAreSet();
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (startStopButton != null) startStopButton.IsEnabled = AllSettingsAreSet();
        }
    }

    public static class WebSocketExtensions
    {
        public static async Task OpenAsync(
            this WebSocket webSocket,
            int retryCount = 5,
            CancellationToken cancelToken = default(CancellationToken))
        {
            var failCount = 0;
            var exceptions = new List<Exception>(retryCount);

            var openCompletionSource = new TaskCompletionSource<bool>();
            cancelToken.Register(() => openCompletionSource.TrySetCanceled());

            EventHandler openHandler = (s, e) => openCompletionSource.TrySetResult(true);

            EventHandler<ErrorEventArgs> errorHandler = (s, e) =>
            {
                if (exceptions.All(ex => ex.Message != e.Exception.Message))
                {
                    exceptions.Add(e.Exception);
                }
            };

            EventHandler closeHandler = (s, e) =>
            {
                if (cancelToken.IsCancellationRequested)
                {
                    openCompletionSource.TrySetCanceled();
                }
                else if (++failCount < retryCount)
                {
                    webSocket.Open();
                }
                else
                {
                    var exception = exceptions.Count == 1
                        ? exceptions.Single()
                        : new AggregateException(exceptions);

                    var webSocketException = new Exception(
                        "Unable to connect",
                        exception);

                    openCompletionSource.TrySetException(webSocketException);
                }
            };

            try
            {
                webSocket.Opened += openHandler;
                webSocket.Error += errorHandler;
                webSocket.Closed += closeHandler;

                webSocket.Open();

                await openCompletionSource.Task.ConfigureAwait(false);
            }
            finally
            {
                webSocket.Opened -= openHandler;
                webSocket.Error -= errorHandler;
                webSocket.Closed -= closeHandler;
            }
        }
    }
}
