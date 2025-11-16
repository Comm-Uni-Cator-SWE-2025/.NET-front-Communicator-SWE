using packetparser;
using socket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socketry
{
    public class Tunnel
    {
        private static byte _NO_CALL_IDS_AVAILABLE = 127;
        IDictionary<CallIdentifier, TaskCompletionSource<byte[]>> _packets;

        List<Link> _links;
        Selector _selector;

        private readonly object _lock = new object();

        private void Initialize()
        {
            _selector = Selector.Open();
            _packets = new ConcurrentDictionary<CallIdentifier, TaskCompletionSource<byte[]>>();
        }

        public Tunnel(short[] LinkPorts)
        {
            Initialize();
            this._links = new List<Link>();
            foreach (short linkPort in LinkPorts)
            {
                Link link = new Link(linkPort);
                link.Register(_selector);
                _links.Add(link);
            }
            CallIdentifier call = new CallIdentifier(2,3);
            TaskCompletionSource<byte[]> resFuture = new TaskCompletionSource<byte[]>();
            Console.WriteLine("packet adding...");
            _packets.Add(call, resFuture);
            Console.WriteLine("packet added...");
        }

        public Tunnel(ISocket[] sockets) { 
            Initialize();
            _links = new List<Link>();
            foreach (ISocket socket in sockets) { 
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
                case Packet.Result resPacket : {
                    byte[] result = resPacket.response;
                    var callIdentifier = new CallIdentifier(resPacket.callId, resPacket.fnId);
                    if(_packets.TryGetValue(callIdentifier, out var resFuture))
                        {
                            resFuture.TrySetResult(result);
                        }
                    return null;
                }
                case Packet.Error errorPacket:
                    {
                        byte[] error = errorPacket.error;
                        var callIdentifier = new CallIdentifier(errorPacket.callId, errorPacket.fnId);
                        if (_packets.TryGetValue(callIdentifier, out var resFuture))
                        {
                            resFuture.TrySetException(new Exception(error.ToString()));
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
                byte callId = _NO_CALL_IDS_AVAILABLE;
                while(true){
                sbyte i = -127;
                    for (; i < 127; i++)
                    {
                        if(!_packets.ContainsKey(new CallIdentifier((byte)i, fnId)))
                        {
                            callId = (byte)i;
                            break;
                        }
                    }
                    if (callId != _NO_CALL_IDS_AVAILABLE) {
                        break;
                    }
                    try
                    {
                        Thread.Sleep(1000);
                    }
                    catch(ThreadInterruptedException e)
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
                    if (!key.IsValid()) continue;

                    if (key.IsReadable())
                    {
                        Link link = (Link)key.Attachment();
                        foreach(Packet packet in link.GetPackets())
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
            Packet.Call packet = new Packet.Call(fnId, callIdentifier.callId,arguments);
            SendPacket(packet);
            _packets[callIdentifier] = resFuture;
            return resFuture;
        }
    }
}
