using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace socket
{
    public class Selector
    {
        List<Socket> ReadSocketList;
        List<Socket> WritesocketList;
        List<Socket> ConnectsocketList;
        Dictionary<Socket, Object> ReadRegisteredSockets;
        Dictionary<Socket, Object> WriteRegisteredSockets;
        Dictionary<Socket, Object> ConnectRegisteredSockets;
        private Selector() {}
        public static Selector Open()
        {
            Selector sel = new Selector();
            sel.ReadSocketList = new List<Socket>();
            sel.WritesocketList = new List<Socket>();
            sel.ConnectsocketList = new List<Socket>();
            sel.ReadRegisteredSockets = new Dictionary<Socket, Object>();
            sel.WriteRegisteredSockets = new Dictionary<Socket, Object>();
            sel.ConnectRegisteredSockets = new Dictionary<Socket, Object>();
            return sel;
        }

        public SelectionKey register(Socket sock, int ops, Object att)
        {
            switch (ops)
            {
                case 1:
                    ReadSocketList.Add(sock);
                    ReadRegisteredSockets.Add(sock, att);
                    break;
                case 2:
                    WritesocketList.Add(sock);
                    WriteRegisteredSockets.Add(sock, att);
                    break;
                case 4:
                    ConnectsocketList.Add(sock);
                    ConnectRegisteredSockets.Add(sock, att);
                    break;
                default:
                    Console.WriteLine($"Unknown operation {ops}");
                    break;
            }
            // meh. is this really required?
            // its wrong but nowhere its used so well ignore it for now
            SelectionKey key = new SelectionKey();
            key.setAtt(att);
            return key;
        }

        public int Select()
        {
            // copy lists :(
            List<Socket> CheckRead = new List<Socket>(ReadSocketList);
            List<Socket> CheckWrite = new List<Socket>(WritesocketList);
            List<Socket> CheckConnect = new List<Socket>(ConnectsocketList);
            Socket.Select(CheckRead, CheckWrite, CheckConnect, 1000);
            return CheckRead.Count + CheckWrite.Count + CheckConnect.Count;
        }

        public ISet<SelectionKey> SelectedKeys()
        {
            // this is peak.
            List<Socket> CheckRead = new List<Socket>(ReadSocketList);
            List<Socket> CheckWrite = new List<Socket>(WritesocketList);
            List<Socket> CheckConnect = new List<Socket>(ConnectsocketList);
            Socket.Select(CheckRead, CheckWrite, CheckConnect, 1000);
            ISet<SelectionKey> keys = new HashSet<SelectionKey>();
            foreach (Socket s in CheckRead) {
                SelectionKey key = new SelectionKey();
                key.setReadable(true);
                key.setAtt(ReadRegisteredSockets[s]);
                keys.Add(key);
            }
            foreach (Socket s in CheckWrite) {
                SelectionKey key = new SelectionKey();
                key.setWritable(true);
                key.setAtt(WriteRegisteredSockets[s]);
                keys.Add(key);
            }
            foreach (Socket s in CheckConnect) {
                SelectionKey key = new SelectionKey();
                key.setConnectable(true);
                key.setAtt(ConnectRegisteredSockets[s]);
                keys.Add(key);
            }
            return keys;
        }
    }
}