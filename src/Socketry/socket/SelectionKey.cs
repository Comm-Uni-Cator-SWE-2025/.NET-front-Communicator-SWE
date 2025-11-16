using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socket
{
    public class SelectionKey
    {
        public static int OP_READ = 1;
        public static int OP_WRITE = 2;
        public static int OP_CONNECT = 4;
        private Object att;
        private bool isValid = true;
        private bool isReadable = false;
        private bool isWritable = false;
        private bool isConnectable = false;

        public void setAtt(Object att)
        {
            this.att = att;
        }

        public void setReadable(bool val)
        {
            isReadable = val;
        }
        public void setWritable(bool val)
        {
            isWritable = val;
        }
        public void setConnectable(bool val)
        {
            isConnectable = val;
        }

        public bool IsValid()
        {
            return isValid;
        }

        public bool IsReadable()
        {
            return isReadable;
        }
        public bool IsWritable()
        {
            return isWritable;
        }
        public bool IsConnectable()
        {
            return isConnectable;
        }

        public Object Attachment()
        {
            return att;
        }
    }
}
