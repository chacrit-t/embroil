using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrc.Embroil.Connection
{
    public interface IConnection
    {
        event Action OnOpen;
        event Action<string> OnMessage;
        event Action OnClose;

        void Connect(string connectionId);
        void Send(string message);
        void Close();
    }
}
