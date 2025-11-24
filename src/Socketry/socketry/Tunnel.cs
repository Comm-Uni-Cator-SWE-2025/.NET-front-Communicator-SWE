// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using System.Text;
using NewSocket;
using Packetparser;
using NewSocket;

namespace Socketry;

public class Tunnel
{
    private static byte s_nO_CALL_IDS_AVAILABLE = 127;
    IDictionary<CallIdentifier, TaskCompletionSource<byte[]>> _packets;

    List<Link> _links;
    Selector _selector;
    private ISocket[] _sockets;
    private readonly object _lock = new object();

    private void Initialize()
    {
        _selector = Selector.Open();
        _packets = new ConcurrentDictionary<CallIdentifier, TaskCompletionSource<byte[]>>();
    }

    public Tunnel(short[] linkPorts)
    {
        Initialize();
        this._links = new List<Link>();
        foreach (short linkPort in linkPorts)
        {
            Link link = new Link(linkPort);
            link.Register(_selector);
            _links.Add(link);
        }
        CallIdentifier call = new CallIdentifier(2, 3);
        TaskCompletionSource<byte[]> resFuture = new TaskCompletionSource<byte[]>();
        Console.WriteLine("packet adding...");
        _packets.Add(call, resFuture);
        Console.WriteLine("packet added...");
    }

    public Tunnel(ISocket[] sockets)
    {
        Initialize();
        _links = new List<Link>();
        foreach (ISocket socket in sockets)
        {
            Link link = new Link(socket);
            link.Register(_selector);
            _links.Add(link);
        }
    }

    private Packet FeedPacket(Packet packet)
    {
        Console.WriteLine($"Feeding packet {packet.ToString()}");
        switch (packet)
        {
            case Packet.Result resPacket:
                {
                    byte[] result = resPacket.Response;
                    var callIdentifier = new CallIdentifier(resPacket.CallId, resPacket.FnId);
                    if (_packets.TryGetValue(callIdentifier, out TaskCompletionSource<byte[]>? resFuture))
                    {
                        resFuture?.TrySetResult(result);
                        _packets.Remove(callIdentifier);
                    }
                    return null;
                }
            case Packet.Error errorPacket:
                {
                    byte[] error = errorPacket.ErrorResp;
                    var callIdentifier = new CallIdentifier(errorPacket.CallId, errorPacket.FnId);
                    if (_packets.TryGetValue(callIdentifier, out TaskCompletionSource<byte[]>? resFuture))
                    {
                        resFuture?.TrySetException(new Exception(Encoding.UTF8.GetString(error)));
                        _packets.Remove(callIdentifier);
                    }
                    return null;
                }

            default:
                {
                    return packet;
                }
        }
    }

    private Link SelectLink()
    {
        var rand = new Random();
        int linkId = Math.Max(0, (int)(rand.NextDouble() * _links.Count) - 1);
        return _links[linkId];
    }

    CallIdentifier GetCallIdentifier(byte fnId)
    {
        lock (_lock)
        {
            byte callId = s_nO_CALL_IDS_AVAILABLE;
            while (true)
            {
                sbyte i = -127;
                for (; i < 127; i++)
                {
                    if (!_packets.ContainsKey(new CallIdentifier((byte)i, fnId)))
                    {
                        callId = (byte)i;
                        break;
                    }
                }
                if (callId != s_nO_CALL_IDS_AVAILABLE)
                {
                    break;
                }
                try
                {
                    Thread.Sleep(1000);
                }
                catch (ThreadInterruptedException e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            CallIdentifier callIdentifier = new CallIdentifier(callId, fnId);
            _packets.Add(callIdentifier, null);
            return callIdentifier;
        }
    }

    public void SendPacket(Packet packet)
    {
        Link link = SelectLink();
        link.SendPacket(packet);
    }

    public List<Packet> Listen()
    {
        List<Packet> packets = new List<Packet>();
        try
        {
            int readyChannels = _selector.Select();
            if (readyChannels == 0)
            {
                return new List<Packet>();
            }

            ISet<SelectionKey> selectedKeys = _selector.SelectedKeys();
            foreach (SelectionKey selectedkey in selectedKeys)
            {
                SelectionKey key = selectedkey;
                selectedKeys.Remove(selectedkey);
                if (!key.IsValid())
                {
                    continue;
                }

                if (key.IsReadable())
                {
                    Link link = (Link)key.Attachment();
                    foreach (Packet packet in link.GetPackets())
                    {
                        packets.Add(packet);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to listen {e.ToString()}");
        }

        List<Packet> packetsToReturn = new List<Packet>();
        foreach (Packet packet in packets)
        {
            Packet feededPacket = FeedPacket(packet);
            if (feededPacket != null)
            {
                packetsToReturn.Add(feededPacket);
            }
        }

        return packetsToReturn;
    }

    public TaskCompletionSource<byte[]> CallFn(byte fnId, byte[] arguments)
    {
        CallIdentifier callIdentifier = GetCallIdentifier(fnId);
        TaskCompletionSource<byte[]> resFuture = new TaskCompletionSource<byte[]>();
        Packet.Call packet = new Packet.Call(fnId, callIdentifier.CallId, arguments);
        _packets[callIdentifier] = resFuture;
        SendPacket(packet);
        return resFuture;
    }
}
