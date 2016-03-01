using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Vrc.Embroil.MessageConverter
{
    public class SockJsMessageConverter : IMessageConverter<string, string>
    {
        public string CovertTo(params string[] input)
        {
            return $"[{string.Join(",", input.Select(x => $"\"{EscapeString(x)}\"" ))}]";
        }

        // TODO : Handle case 'm' & 'c'
        public IEnumerable<string> ConvertFrom(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new List<string>() { string.Empty };
            }

            switch (input[0])
            {
                case 'o':
                    return new List<string>() { "OPEN" };
                case 'a':
                    var payload = input.Substring(1);
                    // remove '[' and ']' from input
                    var output = payload.Substring(2, payload.Length - 4);

                    return new List<string>() { output };
                case 'h':
                    return new List<string>() { "HEARTBEAT" };
                default:
                    return new List<string>() { string.Empty };
            }

        }

        private string EscapeString(string input)
        {
            var escape = input.Replace(@"\c",@"\\c");
#if DEBUG
            Debug.WriteLine($"{DateTime.Now} :: SockJSMessageConverter :: EscapeString :: {escape}");
#endif
            return escape;
        }
    }
}
