using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrc.Embroil.Connection
{
    public interface IConnection : IDisposable
    {
        event EventHandler OnOpen;
        event EventHandler<string> OnMessage;
        event EventHandler OnClose;

        void Connect(string connectionId);
        void Send(string message);
        void Close();

        ConnectionState State { get; }
    }
}
