using packetparser;
using socket;
using System.Collections;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;

namespace socketry
{
    public class Link
    {
        ISocket _clientChannel;
        private MemoryStream _leftOverBuffer;

        public SocketPacket _currentSocketPacket;

        private ConcurrentQueue<Packet> _packets;

        public Link(int port)
        {
            _clientChannel = new SocketTCP();
            _clientChannel.ConfigureBlocking(true); // block till connection is established
            _clientChannel.Connect(new IPEndPoint(IPAddress.Loopback, port));
            _clientChannel.ConfigureBlocking(false);
            _packets = new ConcurrentQueue<Packet>();
        }

        public Link(ISocket _connectedChannel)
        {
            _clientChannel = _connectedChannel;
            _clientChannel.ConfigureBlocking(false);
            _packets = new ConcurrentQueue<Packet>();
        }

        public void ConfigureBlocking(bool to_block)
        {
            _clientChannel.ConfigureBlocking(to_block);
        }

        public void Register(Selector selector)
        {
            if (_clientChannel != null)
            {
                _clientChannel.Register(selector, SelectionKey.OP_READ, this);
            }
        }

        private MemoryStream ReadData()
        {
            int bufferLength = 1024;
            if (_leftOverBuffer != null)
            {
                bufferLength = Math.Max(1024, _leftOverBuffer.Capacity + 1024);
            }
            MemoryStream readBuffer = new MemoryStream(bufferLength);
            if (_leftOverBuffer != null)
            {
                readBuffer.Write(_leftOverBuffer.ToArray());
            }
            int dataRead = _clientChannel.Read(readBuffer);
            if (dataRead == 0)
            {
                // Raise runtime Exception
                // Console.WriteLine("no data received...");
            }
            readBuffer.Position = 0;
            return readBuffer;
        }

        public List<Packet> GetPackets()
        {
            try
            {
                ReadAndParseAllPackets();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                return new List<Packet>();
            }
            List<Packet> packets = new List<Packet>(_packets);
            _packets.Clear();
            return packets;
        }

        void ReadAndParseAllPackets()
        {
            bool initialState = _clientChannel.IsBlocking();
            int dataRead = ReadAndParsePackets();
            Console.WriteLine("Read packets...");
            _clientChannel.ConfigureBlocking(false);

            while (dataRead > 0)
            {
                dataRead = ReadAndParsePackets();
            }
            _clientChannel.ConfigureBlocking(initialState);
        }

        static int ReadInt(byte[] data, int pos)
        {
            if (data.Length < pos)
            {
                // TODO: Throw an exception
            }
            return ((data[pos] & 0xFF) << 24) |
                ((data[pos + 1] & 0xFF) << 16) |
                ((data[pos + 2] & 0xFF) << 8) |
                (data[pos + 3] & 0xFF);
        }

        int ReadAndParsePackets()
        {
            MemoryStream buffer = ReadData();
            Console.WriteLine("Read from socket...");
            int remaining = (int)(buffer.Length - buffer.Position);
            byte[] data = new byte[remaining];
            buffer.Read(data,0,remaining);

            int currDatapos = 0;
            int len = data.Length;
            while (currDatapos < len)
            {
                if (_currentSocketPacket == null)
                {
                    int left = len - currDatapos;
                    if (left < 4)
                    {
                        _leftOverBuffer = new MemoryStream(left);
                        _leftOverBuffer.Write(data, currDatapos, left);
                        _leftOverBuffer.Position = 0;
                        break;
                    }
                    int contentLength = ReadInt(data, currDatapos);
                    _currentSocketPacket = new SocketPacket(contentLength, new MemoryStream());
                    currDatapos += 4;
                }

                int toRead = Math.Min(_currentSocketPacket.BytesLeft, data.Length - currDatapos);
                _currentSocketPacket.Content.Write(data, currDatapos, toRead);
                _currentSocketPacket.BytesLeft -= toRead;
                currDatapos += toRead;

                if (_currentSocketPacket.BytesLeft == 0)
                {
                    _packets.Enqueue(Packet.Parse(_currentSocketPacket.Content.ToArray()));
                    _currentSocketPacket = null;
                }

            }
            return data.Length;
        }

        public Packet GetPacket()
        {
            try
            {
                ReadAndParseAllPackets();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
            Packet packet = _packets.TryDequeue(out var p) ? p : null;
            Console.WriteLine($"packet {packet.ToString()}");
            return packet;
        }

        public bool SendPacket(Packet packet)
        {
            byte[] packetData = Packet.Serialize(packet).ToArray();
            MemoryStream socketData = new MemoryStream(packetData.Length + 4);
            BinaryWriter writer = new BinaryWriter(socketData);
            writer.Write((byte)((packetData.Length >> 24) & 0xFF));
            writer.Write((byte)((packetData.Length >> 16) & 0xFF));
            writer.Write((byte)((packetData.Length >> 8) & 0xFF));
            writer.Write((byte)(packetData.Length & 0xFF));
            writer.Write(packetData);
            socketData.Position = 0;
            Console.WriteLine($"Sent packet {socketData.Length}");
            if (_clientChannel != null)
            {
                try
                {
                    Console.WriteLine($"Writing packet of length {socketData.Length} to client channel.");
                    _clientChannel.Write(socketData);
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            return false;
        }
    }
}
