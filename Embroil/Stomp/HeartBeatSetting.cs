using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrc.Embroil.Stomp
{
    public class HeartBeatSetting 
    {
        public int Outgoing { get; set; }
        public int Incoming { get; set; }

        public HeartBeatSetting Clone()
        {
            return (HeartBeatSetting) MemberwiseClone();
        }
    }
}
