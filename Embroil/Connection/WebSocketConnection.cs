using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace Vrc.Embroil.Connection
{
    public class WebSocketConnection : IConnection
    {
        private readonly Uri uri;
        private ClientWebSocket client;
        public WebSocketConnection(Uri uri)
        {
            this.uri = uri;
        }

        public Action OnOpen { get; set; }
        public Action<string> OnMessage { get; set; }
        public Action OnClose { get; set; }
        public void Connect()
        {

        }

        public void Send(string message)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }
    }
}
