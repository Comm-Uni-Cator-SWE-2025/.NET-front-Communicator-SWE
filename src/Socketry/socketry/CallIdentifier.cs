using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socketry
{
    public record CallIdentifier(byte callId, byte fnId)
    {
        public int HashCode()
        {
            return callId << 8 | fnId;
        }
    }
}
