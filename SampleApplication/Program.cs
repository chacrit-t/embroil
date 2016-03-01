using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vrc.Embroil.Connection;
using Vrc.Embroil.MessageConverter;
using Vrc.Embroil.Stomp;

namespace SampleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run().Wait();
            while (true){}
        }

        public Program()
        {
            
        }


        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public Task Run()
        {
            return Task.Run(() =>
            {
                var uri = new Uri("ws://localhost:8080/websocket");

                var client = new Client(new WebSocketConnection(uri, new SockJsMessageConverter()));
                var random = RandomString(7);

                client.OnConntected += (sender, eventArg) =>
                {
                    Console.WriteLine("Connected");
                    client.Subscribe("/topic/table-updates", additionalHeader: new Dictionary<string, string>()
                    {
                        ["activemq.subscriptionName"] = "/topic/table-updates"
                    });
                };

                client.OnMessageReceived += (sender, message) =>
                {
                    Console.WriteLine("Message ->");
                    Console.WriteLine(message.Body);
                    client.Ack(message);
                };

                client.OnError += (s, message) =>
                {
                    Console.WriteLine("Error ->");
                    Console.WriteLine(message);

                    Console.WriteLine("Reconnect ->");
                    client.Connect(random);
                };

                client.OnReceipt += (sender, message) =>
                {
                    Console.WriteLine("Receipt ->");
                    Console.WriteLine(message.Body);
                };

                client.Connect(random);

                
            });
        }
    }
}
