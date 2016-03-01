using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using Vrc.Embroil.MessageConverter;

namespace Vrc.Embroil.Connection
{
    public class WebSocketConnection : IConnection
    {
        private readonly Uri _uri;
        private ClientWebSocket _client;
        private readonly IMessageConverter<string, string> _messageConverter;

        public WebSocketConnection(Uri uri, IMessageConverter<string, string> messageConverter = null)
        {
            this._uri = uri;
            this._messageConverter = messageConverter;
        }

        public event EventHandler OnOpen;

        public event EventHandler<string> OnMessage;

        public event EventHandler OnClose;

        public async void Connect(string clientId)
        {
            var randomId = new Random().Next(1000);

            var uri = new Uri($"{_uri.AbsoluteUri}/{randomId}/{clientId}{_uri.PathAndQuery}");

            _client = new ClientWebSocket();

            await _client.ConnectAsync(uri, CancellationToken.None);

            StartListen();
            OnOpen?.Invoke(this, null);
        }

        public void Send(string message)
        {
            var convertedMsg = message;

            if (_messageConverter != null)
                convertedMsg = _messageConverter.CovertTo(convertedMsg);

            var encoded = Encoding.UTF8.GetBytes(convertedMsg);
            var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);
#if DEBUG
            Debug.WriteLine($"{DateTime.Now} :: WebSocket :: Send :: {convertedMsg}");
#endif
            _client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
        }

        public void Close()
        {
            if (_client.State == WebSocketState.Open)
            {
                _client.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None);
                OnClose?.Invoke(this, null);
            }
        }

        public ConnectionState State {
            get
            {
                if (_client.State == WebSocketState.Open)
                {
                    return ConnectionState.Connected;
                }
                else
                {
                    return ConnectionState.Closed;
                }
            }
        }

        private async void StartListen()
        {
            var buffer = new byte[1024];
            while (_client.State == WebSocketState.Open)
            {
                var stringResult = new StringBuilder();
            
                WebSocketReceiveResult result;
                do
                {
                    result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await
                            _client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty,
                                CancellationToken.None);
                        
                        OnClose?.Invoke(this, null);
                    }
                    else
                    {
                        var str = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        stringResult.Append(str);
                    }
            
                } while (!result.EndOfMessage);
#if DEBUG
                Debug.WriteLine($"{DateTime.Now} :: WebSocket :: Received :: {stringResult}");
#endif
                if (_messageConverter != null)
                {
                    var convertedMsgs = _messageConverter.ConvertFrom(stringResult.ToString());
                    foreach (var convertedMsg in convertedMsgs)
                    {
                        OnMessage?.Invoke(this,
                            convertedMsg);
                    }
                }
                else
                {
                    OnMessage?.Invoke(this, stringResult.ToString());
                }
            }

        }

        public void Dispose()
        {
            _client.Dispose();
            _client = null;
        }
    }
}

