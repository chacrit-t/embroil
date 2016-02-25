using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrc.Embroil.Stomp
{
    class Message
    {
        private string body = "";
        public Dictionary<string,string> Headers { get; set; } = new Dictionary<string, string>();

        public string Body
        {
            get { return body; }
            set
            {
                this.body = value;
                this.Headers["content-length"] = body.Length.ToString();
            } 
        }

        public string Frame { get; set; }
    }
}
