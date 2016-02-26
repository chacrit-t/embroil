using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vrc.Embroil.Connection;
using Vrc.Embroil.Converter;
using Vrc.Embroil.Stomp;

namespace SampleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run().Wait();
            while (true)
            {
                
            }
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
                var connection = new WebSocketConnection(uri, new SockJsConverter());

                var client = new Client(connection);
                var random = RandomString(7);

                client.OnConntected += () =>
                {
                    Console.WriteLine("Connected");
                    client.Subscribe("description", random, additionalHeader: new Dictionary<string, string>()
                    {
                        ["activemq.subscriptionName"] = "description"
                    });
                };

                client.OnMessageReceived += message =>
                {
                    Console.WriteLine("Message ->");
                    Console.WriteLine(message.Body);
                };

                client.OnError += (s, message) =>
                {
                    Console.WriteLine(s);
                };

                client.OnReceipt += message =>
                {
                    Console.WriteLine("Receipt ->");
                    Console.WriteLine(message.Body);
                };

                client.Connect(random);

                
            });
        }
    }
}
