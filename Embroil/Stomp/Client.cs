using System;
using System.Collections.Generic;
using Vrc.Embroil.Connection;

namespace Vrc.Embroil.Stomp
{
    public sealed class Client
    {
        private readonly IConnection _connection;
        private Queue<Message> messageQueue = new Queue<Message>(); 

        public Client(IConnection connection)
        {
            this._connection = connection;
        }

        /// <summary>
        ///     Request Message
        ///     CONNECT
        ///     [REQ] accept-version:{version=1.2}, host:{host-name}
        ///     [OPT] login, passcode, heart-beat
        ///     Response Message
        ///     CONNECTED
        ///     [REQ] version:{version=1.2}
        ///     [OPT] session, server, heart-beat
        /// </summary>
        public void Connect()
        {
            _connection.OnOpen += OnOpen;
            _connection.OnMessage += OnMessage;
            _connection.OnClose += OnClose;

            _connection.Connect();
        }

        public bool IsConnected { get; private set; }

        private void OnOpen()
        {
            var message = new Message
            {
                Frame = "CONNECT",
                Headers = {["accept-version"] = "1.2"}
            };

            _connection.Send(MessageSerializer.Serialize(message));
        }

        private void OnClose()
        {
            throw new NotImplementedException();
        }
        

        private void OnMessage(string message)
        {
            var msgObj = MessageSerializer.Deserailize(message);

            if (msgObj.Frame == "CONNECTED")
            {
                this.IsConnected = true;
            }
            else if (msgObj.Frame == "MESSAGE")
            {
                
            }
            else if(msgObj.Frame == "RECEIPT")
            {
            }
            else if (msgObj.Frame == "ERROR")
            {
                this.IsConnected = false;
            }
        }

        /// <summary>
        ///     Request Message
        ///     SEND
        ///     [REQ] destination
        ///     [OPT] transaction
        ///     Body
        /// </summary>
        private void Send(string destination, string message, string transaction="")
        {
        }

        private void Subscribe(string destination, string id, string ack = "client")
        {
        }

        private void Unsubscribe(string id)
        {
            var message = new Message()
            {
                Frame = "UNSUBSCRIBE",
                Headers = {["id"] = id}
            };

            _connection.Send( MessageSerializer.Serialize(message));
        }

        public void Ack(Message message, string transaction = "")
        {
            if (message.Headers.ContainsKey("ack"))
            {
                var ackMessage = new Message
                {
                    Frame = "ACK",
                    Headers = {["id"] = message.Headers["ack"]}
                };

                if (!string.IsNullOrWhiteSpace(transaction))
                {
                    ackMessage.Headers["transaction"] = transaction;
                }
            }
        }

        private void Nack(Message message, string transaction = "")
        {
            if (message.Headers.ContainsKey("ack"))
            {
                var ackMessage = new Message
                {
                    Frame = "NACK",
                    Headers = { ["id"] = message.Headers["ack"] }
                };

                if (!string.IsNullOrWhiteSpace(transaction))
                {
                    ackMessage.Headers["transaction"] = transaction;
                }
            }
        }

        private void Begin()
        {
        }

        private void Commit()
        {
        }

        private void Abort()
        {
        }

        private void Disconnect()
        {
        }
    }
}