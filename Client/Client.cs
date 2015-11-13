using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Threading;

namespace ClientNamespace
{
    class Client
    {
        static string iotHubUri = "{IoTHubName}.azure-devices.net";
        static string deviceKey = "{DeviceKey}";
        static string deviceID = "{DeviceName}";
        static DeviceClient deviceClient;
        static void Main(string[] args)
        {
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceID, deviceKey), TransportType.Amqp);   //Or: TransportType.Http1
            ReceiveC2dAsync();
            Console.ReadLine();
        }
        private static async void ReceiveC2dAsync()
        {
            Console.WriteLine("\nReceiving cloud to device messages from service");
            while (true)
            {
                Message receivedMessage = await deviceClient.ReceiveAsync();    //Wait to get a message from cloud
                if (receivedMessage == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received message: {0}", Encoding.ASCII.GetString(receivedMessage.GetBytes()));
                Console.ResetColor();

                await deviceClient.CompleteAsync(receivedMessage);  //Send response (success) to cloud
                Console.WriteLine("Sent feedback back to Cloud");
            }
        }
    }
}
