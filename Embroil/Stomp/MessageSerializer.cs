using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrc.Embroil.Stomp
{
    class MessageSerializer
    {
        public static Message Deserailize(string msg)
        {
            var msgObj = new Message();
            var reader = new StringReader(msg);

            msgObj.Frame = reader.ReadLine();
            
            var header = reader.ReadLine();

            while (!string.IsNullOrWhiteSpace(header))
            {
                var headerItem = ParseHeader(header);
                msgObj.Headers[headerItem.Name] = headerItem.Value;

                header = reader.ReadLine();
            }
            
            msgObj.Body = reader.ReadToEnd() ?? string.Empty;

            return msgObj;
        }

        private static HeaderItem ParseHeader(string header)
        {
            var splitHeader = header.Split(':');
            if (splitHeader.Length == 2)
            {
                return new HeaderItem()
                {
                    Name = splitHeader[0],
                    Value = splitHeader[1]
                };
            }

            return null;
        }

        class HeaderItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}
