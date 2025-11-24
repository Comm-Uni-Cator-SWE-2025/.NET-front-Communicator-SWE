// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net.Sockets;

namespace NewSocket;

public class Selector
{
    List<Socket> _readSocketList;
    List<Socket> _writesocketList;
    List<Socket> _connectsocketList;
    Dictionary<Socket, object> _readRegisteredSockets;
    Dictionary<Socket, object> _writeRegisteredSockets;
    Dictionary<Socket, object> _connectRegisteredSockets;
    private Selector() { }
    public static Selector Open()
    {
        Selector sel = new Selector {
            _readSocketList = new List<Socket>(),
            _writesocketList = new List<Socket>(),
            _connectsocketList = new List<Socket>(),
            _readRegisteredSockets = new Dictionary<Socket, object>(),
            _writeRegisteredSockets = new Dictionary<Socket, object>(),
            _connectRegisteredSockets = new Dictionary<Socket, object>()
        };
        return sel;
    }

    public SelectionKey Register(Socket sock, int ops, object att)
    {
        switch (ops)
        {
            case 1:
                _readSocketList.Add(sock);
                _readRegisteredSockets.Add(sock, att);
                break;
            case 2:
                _writesocketList.Add(sock);
                _writeRegisteredSockets.Add(sock, att);
                break;
            case 4:
                _connectsocketList.Add(sock);
                _connectRegisteredSockets.Add(sock, att);
                break;
            default:
                Console.WriteLine($"Unknown operation {ops}");
                break;
        }
        // meh. is this really required?
        // its wrong but nowhere its used so well ignore it for now
        SelectionKey key = new SelectionKey();
        key.SetAtt(att);
        return key;
    }

    public int Select()
    {
        // copy lists :(
        List<Socket> checkRead = new List<Socket>(_readSocketList);
        List<Socket> checkWrite = new List<Socket>(_writesocketList);
        List<Socket> checkConnect = new List<Socket>(_connectsocketList);
        Socket.Select(checkRead, checkWrite, checkConnect, 1000);
        return checkRead.Count + checkWrite.Count + checkConnect.Count;
    }

    public ISet<SelectionKey> SelectedKeys()
    {
        // this is peak.
        List<Socket> checkRead = new List<Socket>(_readSocketList);
        List<Socket> checkWrite = new List<Socket>(_writesocketList);
        List<Socket> checkConnect = new List<Socket>(_connectsocketList);
        Socket.Select(checkRead, checkWrite, checkConnect, 1000);
        ISet<SelectionKey> keys = new HashSet<SelectionKey>();
        foreach (Socket s in checkRead)
        {
            SelectionKey key = new SelectionKey();
            key.SetReadable(true);
            key.SetAtt(_readRegisteredSockets[s]);
            keys.Add(key);
        }
        foreach (Socket s in checkWrite)
        {
            SelectionKey key = new SelectionKey();
            key.SetWritable(true);
            key.SetAtt(_writeRegisteredSockets[s]);
            keys.Add(key);
        }
        foreach (Socket s in checkConnect)
        {
            SelectionKey key = new SelectionKey();
            key.SetConnectable(true);
            key.SetAtt(_connectRegisteredSockets[s]);
            keys.Add(key);
        }
        return keys;
    }

    internal SelectionKey Register(Socket osSocket, int ops, object att)
    {
        throw new NotImplementedException();
    }
}
