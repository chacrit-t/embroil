using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using Vrc.Embroil.Converter;

namespace Vrc.Embroil.Connection
{
    public class WebSocketConnection : IConnection
    {
        private readonly Uri _uri;
        private ClientWebSocket _client;
        private IConverter<string, string> _converter;

        public WebSocketConnection(Uri uri, IConverter<string, string> converter = null)
        {
            this._uri = uri;
            this._converter = converter;
        }

        public event Action OnOpen;

        public event Action<string> OnMessage;

        public event Action OnClose;

        public async void Connect(string clientId)
        {
            var randomId = new Random().Next(1000);

            var uri = new Uri($"{_uri.AbsoluteUri}/{randomId}/{clientId}{_uri.PathAndQuery}");

            _client = new ClientWebSocket();

            await _client.ConnectAsync(uri, CancellationToken.None);

            StartListen();
            OnOpen?.Invoke();
        }

        public void Send(string message)
        {
            var convertedMsg = message;

            if (_converter != null)
                convertedMsg = _converter.CovertTo(convertedMsg);

            var encoded = Encoding.UTF8.GetBytes(convertedMsg);
            var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);

            _client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public void Close()
        {
            _client.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None);
            OnClose?.Invoke();
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
                            _client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        
                        OnClose?.Invoke();
                    }
                    else
                    {
                        var str = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        stringResult.Append(str);
                    }

                } while (!result.EndOfMessage);


                if (_converter != null)
                {
                    var convertedMsgs = _converter.ConvertFrom(stringResult.ToString());
                    foreach (var convertedMsg in convertedMsgs)
                    {
                        OnMessage?.Invoke(convertedMsg);
                    }
                }
                else
                {
                    OnMessage?.Invoke(stringResult.ToString());
                }
            }
        }
    }
}

