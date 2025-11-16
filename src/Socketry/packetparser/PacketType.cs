using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace packetparser
{
    public class PacketType
    {
        public const byte CALL = 1;
        public const byte RESULT = 2;
        public const byte ERROR = 3;
        public const byte INIT = 4;
        public const byte ACCEPT = 5;
        public const byte PING = 6;
        public const byte PONG = 7;
    }
}
