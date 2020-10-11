using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperSocket.ClientEngine;
using WebSocket4Net;

namespace LightExample
{
    class SinricClient
    {
        //private const string SinricAddress = "ws://iot.sinric.com";
        private const string SinricAddress = "ws://ws.sinric.pro";
        private string SecretKey { get; set; }

        private WebSocket WebSocket { get; set; } 
        private Thread MainLoop { get; set; }
        private bool Running { get; set; }

        public ConcurrentQueue<SinricMessage> IncomingMessages { get; } = new ConcurrentQueue<SinricMessage>();

        public SinricClient(string apiKey, string secretKey, string deviceId)
        {
            SecretKey = secretKey;

            var headers = new List<KeyValuePair<string, string>>
            {
                //new KeyValuePair<string, string>("Authorization", ("apikey:" + apiKey).Base64Encode())
                new KeyValuePair<string, string>("appkey", apiKey),
                new KeyValuePair<string, string>("deviceids", deviceId),
                new KeyValuePair<string, string>("platform", "csharp"),
                new KeyValuePair<string, string>("restoredevicestates", "true"),
            };

            WebSocket = new WebSocket(SinricAddress, customHeaderItems: headers)
            {
                EnableAutoSendPing = true, 
                AutoSendPingInterval = 60,
            };
            
            WebSocket.Opened += WebSocketOnOpened;
            WebSocket.Error += WebSocketOnError;
            WebSocket.Closed += WebSocketOnClosed;
            WebSocket.MessageReceived += WebSocketOnMessageReceived;

        }

        public void Start()
        {
            if (MainLoop == null)
            {
                Console.WriteLine("SinricClient is starting");

                Running = true;
                MainLoop = new Thread(MainLoopThread);
                MainLoop.Start();
            }
            else
            {
                Console.WriteLine("SinricClient is already running");
            }
        }

        public void Stop()
        {
            if (MainLoop == null)
            {
                Console.WriteLine("SinricClient is already stopped");
            }
            else
            {
                Console.WriteLine("SinricClient is stopping");

                Running = false;
                while (MainLoop != null)
                {
                    Thread.Sleep(100);
                }
                
                if (WebSocket.State == WebSocketState.Open)
                    WebSocket.Close();
            }
        }

        private void MainLoopThread(object obj)
        {
            while (Running)
            {
                switch (WebSocket.State)
                {
                    case WebSocketState.Closed:
                    case WebSocketState.None:
                        WebSocket.OpenAsync();
                        Console.WriteLine($"Websocket connecting to {SinricAddress}");
                        break;

                    case WebSocketState.Open:
                        break;

                    case WebSocketState.Closing:
                        Console.WriteLine("Websocket is closing ...");
                        break;

                    case WebSocketState.Connecting:
                        Console.WriteLine("Websocket is connecting ...");
                        break;
                }

                // give a few seconds grace time between attempts
                Thread.Sleep(3000);
            }

            MainLoop = null;
            Running = false;
        }

        public void SendMessage(SinricMessage message)
        {
            var payloadJson = JsonConvert.SerializeObject(message.Payload);
            message.RawPayload = new JRaw(payloadJson);

            // compute the signature using our secret key so that the service can verify authenticity
            message.Signature.Hmac = Utility.Signature(payloadJson, SecretKey);
            
            // serialize the message to json
            var json = JsonConvert.SerializeObject(message);

            WebSocket.Send(json);
            Console.WriteLine("Websocket message sent:\n" + json + "\n");
        }

        private void WebSocketOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Console.WriteLine("Websocket message received:\n" + e.Message + "\n");

            try
            {
                var obj = JsonConvert.DeserializeObject<SinricMessage>(e.Message);
                var payloadString = obj.RawPayload?.Value as string;

                if (!string.IsNullOrEmpty(payloadString))
                {
                    // if the message contains a payload then we need to validate its signature

                    // compute a local signature from the raw payload using our secret key:
                    var signature = Utility.Signature(payloadString, SecretKey);

                    // compare the locally computed signature with the one supplied in the message:
                    if (signature != obj.Signature.Hmac)
                        throw new Exception("Computed signature for the payload does not match the signature supplied in the message. Message may have been tampered with.");
                }

                // add to the incoming message queue. caller will retrieve the messages on their own thread
                IncomingMessages.Enqueue(obj);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing message from sinrec:\n" + ex + "\n");
            }
        }

        private void WebSocketOnClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Websocket connection closed");
        }

        private void WebSocketOnOpened(object sender, EventArgs e)
        {
            Console.WriteLine("Websocket connection opened");
        }

        private void WebSocketOnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("Websocket connection error:\n" + e.Exception + "\n");

            if (WebSocket.State == WebSocketState.Open)
                WebSocket.Close();
        }

    }
}
