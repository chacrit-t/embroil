using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrc.Embroil.Connection
{
    public interface IConnection
    {
        Action OnOpen { get; set; }
        Action<string> OnMessage { get; set; }
        Action OnClose { get; set; }

        void Connect();
        void Send(string message);
        void Close();
    }
}
