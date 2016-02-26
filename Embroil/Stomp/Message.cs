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
                this._body = value;
                this.Headers["content-length"] = _body.Length.ToString();
            } 
        }

        public string FrameCommand { get; set; }
    }
}
