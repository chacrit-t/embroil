using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Vrc.Embroil.Converter
{
    public class SockJsConverter : IConverter<string, string>
    {
        public string CovertTo(string input)
        {
            return $"[\"{input}\"]";
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
    }
}
