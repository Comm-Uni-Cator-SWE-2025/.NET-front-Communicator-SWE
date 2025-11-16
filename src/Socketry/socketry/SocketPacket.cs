using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socketry
{
    public class SocketPacket
    {
        public int BytesLeft;
        public MemoryStream Content;

        public SocketPacket(int contentLength, MemoryStream content)
        {
            this.BytesLeft = contentLength;
            this.Content = content;
        }
    }
}
