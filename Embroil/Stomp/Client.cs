using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading;
using Vrc.Embroil.Connection;
using Vrc.Embroil.Extension;

namespace Vrc.Embroil.Stomp
{
    /// <summary>
    /// TODO : Improve it to support heart-beating
    /// </summary>
    public sealed class Client
    {
        private const int TimerInterval = 500;
        private const int MarginTime = 500;
        private readonly IConnection _connection;
        private readonly Queue<Message> _messageQueue = new Queue<Message>(); 
        private string _id;
        private readonly object _lockObj = new object();
        private readonly Timer _outgoingTimer;
        private readonly Timer _incomingTimer;
        private DateTime _latestMessageTime;
        private readonly HeartBeatSetting _originalHeartBeatSetting;
        private HeartBeatSetting _heartbeat = new HeartBeatSetting()
        {
            Outgoing = 60000,
            Incoming = 60000,
        };

        public Client(IConnection connection, HeartBeatSetting heartBeat = null)
        {
            this._connection = connection;
            if (heartBeat != null)
            {
                _heartbeat = heartBeat;
            }

            _originalHeartBeatSetting = _heartbeat.Clone();
            _outgoingTimer = new Timer(OutgoingTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
            _incomingTimer = new Timer(IncomingTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }


        #region Public Methods
        public void Connect(string clientId)
        {
            this._heartbeat = _originalHeartBeatSetting.Clone();

            _id = clientId;
            _connection.OnOpen += OnOpen;
            _connection.OnMessage += OnMessage;
            _connection.OnClose += OnClose;

            _connection.Connect(_id);
        }

        public bool IsConnected => _connection?.State == ConnectionState.Connected;

        public void Send(string destination, string body, string transaction = "", Dictionary<string, string> additionalHeader = null)
        {
            var message = new Message()
            {
                FrameCommand = "SEND",
                Headers = { ["destination"] = destination },
                Body = body
            };
            if (!string.IsNullOrWhiteSpace(transaction))
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
                    Headers =
                    {
                        ["id"] = message.Headers["ack"]
                    }
                };

                if (!string.IsNullOrWhiteSpace(transaction))
                {
                    ackMessage.Headers["transaction"] = transaction;
                }
                SendMessage(ackMessage);
            }
        }

        public void Nack(Message message, string transaction = "")
        {
            if (message.Headers.ContainsKey("ack"))
            {
                var nackMessage = new Message
                {
                    FrameCommand = "NACK",
                    Headers = { ["id"] = message.Headers["ack"] }
                };

                if (!string.IsNullOrWhiteSpace(transaction))
                {
                    nackMessage.Headers["transaction"] = transaction;
                }
                SendMessage(nackMessage);
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
            
            CleanUp();
        }

        #endregion

        #region Public Event

        public event EventHandler OnConntected;
        public event EventHandler<Message> OnMessageReceived;
        public event EventHandler<string> OnError;
        public event EventHandler<Message> OnReceipt;
        #endregion

        #region Private Method
        private void OnOpen(object sender, EventArgs e)
        {
            var message = new Message
            {
                FrameCommand = "CONNECT",
                Headers =
                {
                    ["accept-version"] = "1.2",
                    ["heart-beat"] = $"{_heartbeat.Outgoing},{_heartbeat.Incoming}"
                }
            };

            SendMessage(message);
        }

        private void OnClose(object sender, EventArgs e)
        {
            CleanUp();
            OnError?.Invoke(this, "Lost connection.");
        }

        private void OnMessage(object sender, string message)
        {;
            _latestMessageTime = DateTime.Now;
            var msgObj = MessageSerializer.Deserailize(Regex.Unescape(message));

            switch (msgObj.FrameCommand)
            {
                case "CONNECTED":
                    SendMessageQueue();
                    OnConntected?.Invoke(this, null);

                    var heartbeats = msgObj.Headers["heart-beat"]?.Split(',');

                    if (heartbeats?.Length == 2)
                    {
                        var serverHeartbeat = new HeartBeatSetting()
                        {
                            Outgoing = int.Parse(heartbeats[0]),
                            Incoming = int.Parse(heartbeats[1])
                        };
                            
                        _heartbeat.Incoming = CalculateHeartbeatTime(
                            _heartbeat.Incoming,
                            serverHeartbeat.Outgoing);
                        _heartbeat.Outgoing = CalculateHeartbeatTime(
                            _heartbeat.Outgoing,
                            serverHeartbeat.Incoming);
                        
                        StartTimer();
                    }

                    break;
                case "MESSAGE":
                    OnMessageReceived?.Invoke(this, msgObj);
                    break;
                case "RECEIPT":
                    OnReceipt?.Invoke(this, msgObj);
                    break;
                case "ERROR":
                    CleanUp();
                    OnError?.Invoke(this, msgObj.Headers["message"]);
                    break;
            }
        }

        private int CalculateHeartbeatTime(int firstHeartbeatTime, int secondHeartbeatTime)
        {
            if (firstHeartbeatTime == 0 ||
                secondHeartbeatTime == 0)
            {
                return Timeout.Infinite;
            }
            return Math.Max(firstHeartbeatTime, secondHeartbeatTime) - MarginTime;
        }

        private void SendMessage(Message message)
        {
            RefreshTimer();
            _messageQueue.Enqueue(message);
            SendMessageQueue();   
        }

        private void SendMessageQueue()
        {
            lock (_lockObj)
            {
                while (IsConnected && _messageQueue.Count > 0)
                {
                    var msg = _messageQueue.Dequeue();
                    _connection.Send(MessageSerializer.Serialize(msg));
                }
            }
        }

        private void StopTimer()
        {
            _incomingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _outgoingTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void StartTimer()
        {
            RefreshTimer();
            _incomingTimer.Change(0, TimerInterval);
            _outgoingTimer.Change(0, TimerInterval);
        }

        private void RefreshTimer()
        {
            _latestMessageTime = DateTime.Now;
        }

        private void CleanUp()
        {
            _connection.OnClose -= OnClose;
            _connection.OnMessage -= OnMessage;
            _connection.OnOpen -= OnOpen;
            _connection.Dispose();
            StopTimer();
        }
        
        private void IncomingTimerCallback(object state)
        {
            if ((DateTime.Now - _latestMessageTime).TotalMilliseconds >= (_heartbeat.Incoming * 2))
            {
                CleanUp();
                OnError?.Invoke(this, "Lost Connection.");
            }
        }

        private void OutgoingTimerCallback(object state)
        {
            if ((DateTime.Now - _latestMessageTime).TotalMilliseconds >= _heartbeat.Outgoing)
            {
                SendMessage(new Message()
                {
                    FrameCommand = "HEARTBEAT"
                });
            }
        }
        #endregion


    }
}