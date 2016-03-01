using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrc.Embroil.Stomp
{
    internal class MessageSerializer
    {
        internal static string Serialize(Message msgObj)
        {
            if (msgObj.FrameCommand == "HEARTBEAT")
            {
                return "\\n";
            }
            else
            {
                var builder = new StringBuilder();
                builder.Append($"{msgObj.FrameCommand}\\n");

                foreach (var header in msgObj.Headers)
                {
                    builder.Append($"{header.Key}:{header.Value}\\n");
                }

                builder.Append("\\n");
                if (!string.IsNullOrWhiteSpace(msgObj.Body))
                    builder.Append($"{msgObj.Body}\\n");
                builder.Append("\\u0000");

                return builder.ToString();
            }
        }

        internal static Message Deserailize(string msg)
        {
            var msgObj = new Message();
            var reader = new StringReader(msg);

            msgObj.FrameCommand = reader.ReadLine();
            
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
            var splitHeader = header.Split(new char[] {':'}, 2);
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

        private class HeaderItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}
