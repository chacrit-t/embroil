using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrc.Embroil.Stomp
{
    public class Message
    {
        private string _body = "";
        public Dictionary<string,string> Headers { get; set; } = new Dictionary<string, string>();

        public string Body
        {
            get { return _body; }
            set
            {
                _body = value;
                Headers["content-length"] = _body.Length.ToString();
                Headers["content-type"] = "text/plain;charset=utf-8";
            } 
        }

        public string FrameCommand { get; set; }
    }
}
