using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQSDemo
{
    class Program
    {
        const string QUEUE_URL = "https://sqs.us-east-1.amazonaws.com/505383316967/test-queue";
        static readonly Amazon.RegionEndpoint ENDPOINT = Amazon.RegionEndpoint.USEast1;
        static readonly AmazonSQSClient client = new AmazonSQSClient(ENDPOINT);

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("SQS demo.  Enter command, or h for help.");
                Poll();
            }
            catch (Exception ex)
            {
                string msg = string.Format("{0} Exception: {1}", ex.GetType().Name, ex.Message);
                Console.WriteLine(msg);
                Console.WriteLine(ex.StackTrace);

                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
        }

        private static void Poll()
        {
            char command = new char();
            do
            {
                var consoleKeyInfo = Console.ReadKey(true);
                command = consoleKeyInfo.KeyChar;

                Console.WriteLine();

                switch (command)
                {
                    case 'h':
                        Console.WriteLine("s: send message to queue");
                        Console.WriteLine("r: receive from queue");
                        Console.WriteLine("d: receive and then delete from queue");
                        Console.WriteLine("i: queue info");
                        Console.WriteLine("x: exit");
                        break;
                    case 's': SendToQueue(); break;
                    case 'r': ReadFromQueue(); break;
                    case 'd': PopFromQueue(); break;
                    case 'i': QueueInfo(); break;
                    case 'x': break;
                    default:
                        Console.WriteLine(string.Format("Invalid command: {0} ", command));
                        break;
                }
            } while (command != 'x');
        }

        private static void SendToQueue()
        {
            Console.WriteLine("Type a message for the queue, <return> to submit.");
            string message = Console.ReadLine();

            var request = new SendMessageRequest()
            {
                QueueUrl = QUEUE_URL,
                MessageBody = message
            };
            var response = client.SendMessage(request);
            Console.WriteLine("Response code: " + response.HttpStatusCode.ToString());
        }

        private static void PopFromQueue()
        {
            string handle = ReadFromQueue();
            if (string.IsNullOrEmpty(handle))
            {
                Console.WriteLine("Nothing to delete.");
                return;
            }
            var request = new DeleteMessageRequest()
            {
                QueueUrl = QUEUE_URL,
                ReceiptHandle = handle
            };
            var response = client.DeleteMessage(request);
            Console.WriteLine("Response code: " + response.HttpStatusCode.ToString());
        }

        private static string ReadFromQueue()
        {
            var request = new ReceiveMessageRequest()
            {
                QueueUrl = QUEUE_URL,
                MaxNumberOfMessages = 1
            };
            var response = client.ReceiveMessage(request);
            if (response.Messages.Count > 0)
            {
                Console.WriteLine("Message " + response.Messages[0].Body);
                return response.Messages[0].ReceiptHandle;
            }
            else
            {
                Console.WriteLine("No messages available.");
                return null;
            }
        }

        private static void QueueInfo()
        {
            GetQueueAttributesRequest request = new GetQueueAttributesRequest
            {
                QueueUrl = QUEUE_URL,
                AttributeNames = new List<string>() { "All" }
            };

            var response = client.GetQueueAttributes(request);

            Console.WriteLine("Attributes for queue ARN '" + response.QueueARN + "':");
            Console.WriteLine("  Approximate number of messages:" + response.ApproximateNumberOfMessages);
            Console.WriteLine("  Approximate number of messages delayed: " + response.ApproximateNumberOfMessagesDelayed);
            Console.WriteLine("  Approximate number of messages not visible: " + response.ApproximateNumberOfMessagesNotVisible);
            Console.WriteLine("  Queue created on: " + response.CreatedTimestamp);
            Console.WriteLine("  Delay seconds: " + response.DelaySeconds);
            Console.WriteLine("  Queue last modified on: " + response.LastModifiedTimestamp);
            Console.WriteLine("  Maximum message size: " + response.MaximumMessageSize);
            Console.WriteLine("  Message retention period: " + response.MessageRetentionPeriod);
            Console.WriteLine("  Visibility timeout: " + response.VisibilityTimeout);
            Console.WriteLine("  Policy: " + response.Policy);
            Console.WriteLine("  Attributes:");

            foreach (var attr in response.Attributes)
            {
                Console.WriteLine("    " + attr.Key + ": " + attr.Value);
            }

        }
    }
}
