using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using Vrc.Embroil.Connection;
using Vrc.Embroil.Extension;

namespace Vrc.Embroil.Stomp
{
    /// <summary>
    /// TODO : Improve it to support heart-beating
    /// </summary>
    public sealed class Client
    {
        private readonly IConnection _connection;
        private Queue<Message> _messageQueue = new Queue<Message>(); 
        private string _id;

        public Client(IConnection connection)
        {
            this._connection = connection;

        }
        
        #region Public Methods
        public void Connect(string clientId)
        {
            _id = clientId;
            _connection.OnOpen += OnOpen;
            _connection.OnMessage += OnMessage;
            _connection.OnClose += OnClose;

            _connection.Connect(clientId);
        }

        public bool IsConnected { get; private set; }
        
        public void Send(string destination, string body, string transaction = "", Dictionary<string, string> additionalHeader = null)
        {
            var message = new Message()
            {
                FrameCommand = "SEND",
                Headers = { ["destination"] = destination },
                Body = body
            };
            if (string.IsNullOrWhiteSpace(transaction))
                message.Headers["transaction"] = transaction;

            if(additionalHeader != null)
                message.Headers = message.Headers.Merge(additionalHeader);

            SendMessage(message);
        }

        public void Subscribe(string destination, string ack = "client", Dictionary<string, string> additionalHeader = null)
        {
            var message = new Message()
            {
                FrameCommand = "SUBSCRIBE",
                Headers =
                {
                    ["destination"] = destination,
                    ["id"] = _id,
                    ["ack"] = ack
                }
            };

            if (additionalHeader != null)
                message.Headers = message.Headers.Merge(additionalHeader);

            SendMessage(message);
        }

        public void Unsubscribe()
        {
            var message = new Message()
            {
                FrameCommand = "UNSUBSCRIBE",
                Headers = { ["id"] = _id }
            };

            SendMessage(message);
        }

        public void Ack(Message message, string transaction = "")
        {
            if (message.Headers.ContainsKey("ack"))
            {
                var ackMessage = new Message
                {
                    FrameCommand = "ACK",
                    Headers = { ["id"] = message.Headers["ack"] }
                };

                if (!string.IsNullOrWhiteSpace(transaction))
                {
                    ackMessage.Headers["transaction"] = transaction;
                }
                SendMessage(message);
            }
        }

        public void Nack(Message message, string transaction = "")
        {
            if (message.Headers.ContainsKey("ack"))
            {
                var ackMessage = new Message
                {
                    FrameCommand = "NACK",
                    Headers = { ["id"] = message.Headers["ack"] }
                };

                if (!string.IsNullOrWhiteSpace(transaction))
                {
                    ackMessage.Headers["transaction"] = transaction;
                }
                SendMessage(message);
            }
        }

        public void Begin(string transaction)
        {
            var message = new Message()
            {
                FrameCommand = "BEGIN",
                Headers = { ["transaction"] = transaction }
            };
            SendMessage(message);
        }

        public void Commit(string transaction)
        {
            var message = new Message()
            {
                FrameCommand = "COMMIT",
                Headers = { ["transaction"] = transaction }
            };
            SendMessage(message);
        }

        public void Abort(string transaction)
        {
            var message = new Message()
            {
                FrameCommand = "ABORT",
                Headers = { ["transaction"] = transaction }
            };
            SendMessage(message);
        }

        public void Disconnect(string receipt)
        {
            var message = new Message()
            {
                FrameCommand = "DISCONNECT",
                Headers = { ["receipt"] = receipt }
            };
            SendMessage(message);

            _connection.OnClose -= OnClose;
            _connection.Close();

            CleanUp();
        }

        #endregion

        #region Public Event

        public event Action OnConntected;
        public event Action<Message> OnMessageReceived;
        public event Action<string, Message> OnError;
        public event Action<Message> OnReceipt;
        #endregion

        #region Private Method
        private void OnOpen()
        {
            var message = new Message
            {
                FrameCommand = "CONNECT",
                Headers = { ["accept-version"] = "1.2" }
            };

            _connection.Send(MessageSerializer.Serialize(message));
        }

        private void OnClose()
        {
            CleanUp();
            OnError?.Invoke("Lost connection!.", null);
        }

        private void OnMessage(string message)
        {;
            var msgObj = MessageSerializer.Deserailize(Regex.Unescape(message));

            switch (msgObj.FrameCommand)
            {
                case "CONNECTED":
                    this.IsConnected = true;
                    SendMessageQueue();
                    OnConntected?.Invoke();
                    break;
                case "MESSAGE":
                    OnMessageReceived?.Invoke(msgObj);
                    break;
                case "RECEIPT":
                    OnReceipt?.Invoke(msgObj);
                    break;
                case "ERROR":
                    OnError?.Invoke(msgObj.Headers["message"], msgObj);
                    this.IsConnected = false;
                    break;
            }
        }

        private void SendMessage(Message message)
        {
            _messageQueue.Enqueue(message);
            SendMessageQueue();   
        }

        private void SendMessageQueue()
        {
            while (_messageQueue.Count > 0 && IsConnected)
            {
                var msg = _messageQueue.Dequeue();
                _connection.Send(MessageSerializer.Serialize(msg));
            }
        }

        private void CleanUp()
        {
            this.IsConnected = false;
        }
        
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        #endregion


    }
}