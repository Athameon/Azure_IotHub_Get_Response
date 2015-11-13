using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using System.Diagnostics;

namespace cloudNamespace
{
    class Cloud
    {
        static string connectionString = "HostName={IotHubName}.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey={IotHub Key}";
        static ServiceClient serviceClient;
        static void Main(string[] args)
        {
            Console.WriteLine("Send Cloud-to-Device message\n");
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);

            Console.WriteLine("Press any key to send a C2D message.");
            while (true)
            {
                Console.ReadLine();
                string guid = Guid.NewGuid().ToString();
                for (int i = 0; i < 3; i++)     //Send 3 messages to the device and receive the feedback
                {
                    SendCloudToDeviceMessageAsync(guid).Wait();
                    ReceiveFeedbackAsync(guid).Wait();
                }
                Console.WriteLine("Finished sending all messages");
            }
        }
        private async static Task SendCloudToDeviceMessageAsync(string guid)
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes("Cloud to device message."));
            commandMessage.Ack = DeliveryAcknowledgement.Full;
            commandMessage.MessageId = guid;
            await serviceClient.SendAsync("{DeviceID}", commandMessage);
            Console.WriteLine("Message is sent");
        }
        private async static Task ReceiveFeedbackAsync(string guid)
        {
            var startTime = DateTime.Now;
            var feedbackReceiver = serviceClient.GetFeedbackReceiver();
            bool receivedFeetback = false;
            Console.WriteLine("\nReceiving c2d feedback from service");
            while (!receivedFeetback)
            {
                FeedbackBatch feedbackBatch = await feedbackReceiver.ReceiveAsync(TimeSpan.FromSeconds(0.5));
                if (feedbackBatch == null)
                {
                    Trace.TraceInformation("nothing");
                    continue;
                }
                Trace.TraceInformation("-----something");
                foreach (var item in feedbackBatch.Records)
                {
                    Console.WriteLine("GUID from sent message: " + guid);
                    Console.WriteLine("GUID from recived message: " + item.OriginalMessageId);
                    if (guid == item.OriginalMessageId)
                    {
                        Console.WriteLine("Time: " + (DateTime.Now - startTime).Seconds.ToString());
                        receivedFeetback = true;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Received feedback: {0}", string.Join(", ", feedbackBatch.Records.Select(f => f.StatusCode)));
                        Console.ResetColor();
                        await feedbackReceiver.CompleteAsync(feedbackBatch);    //Delete responsemessage from client
                    }
                }
            }
        }
    }
}
