// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Packetparser;

record CREPacket(byte FnId, byte CallId, byte[] Arguments)
{
    public static CREPacket Parse(byte[] data)
    {
        byte fnId = data[1];
        byte callId = data[2];
        int argumentsLength = data.Length - 3;
        byte[] arguments = new byte[argumentsLength];
        Array.Copy(data, 3, arguments, 0, argumentsLength);
        return new CREPacket(fnId, callId, arguments);
    }
}
public interface Packet
{
    static Packet Parse(byte[] data)
    {
        byte type = data[0];
        switch (type)
        {
            case PacketType.CALL:
                return Call.Parse(data);
            case PacketType.RESULT:
                return Result.Parse(data);
            case PacketType.ERROR:
                return Error.Parse(data);
            case PacketType.INIT:
                int length = data.Length - 1;
                byte[] packetData = new byte[length];
                Array.Copy(data, 1, packetData, 0, length);
                return new Init(packetData);
            case PacketType.ACCEPT:
                return Accept.Parse(data);
            case PacketType.PING:
                return Ping.Instance;
            case PacketType.PONG:
                return Pong.Instance;
            default:
                throw new ArgumentException($"Unknown packet type {type}");
        }
    }

    static MemoryStream Serialize(Packet packet)
    {
        int size = 1 + packet switch {
            Packet.Call call => 2 + call.Arguments.Length,
            Packet.Result result => 2 + result.Response.Length,
            Packet.Error error => 2 + error.ErrorResp.Length,
            Packet.Init init => init.Channels.Length,
            Packet.Accept accept => 2 * accept.Ports.Length,
            Packet.Ping => 0,
            Packet.Pong => 0,
            _ => throw new ArgumentOutOfRangeException()
        };
        MemoryStream buffer = new MemoryStream(size);
        BinaryWriter writer = new BinaryWriter(buffer);

        switch (packet)
        {
            case Packet.Call call:
                writer.Write(PacketType.CALL);
                writer.Write(call.FnId);
                writer.Write(call.CallId);
                writer.Write(call.Arguments);
                break;
            case Packet.Result result:
                writer.Write(PacketType.RESULT);
                writer.Write(result.FnId);
                writer.Write(result.CallId);
                writer.Write(result.Response);
                break;
            case Packet.Error error:
                writer.Write(PacketType.ERROR);
                writer.Write(error.FnId);
                writer.Write(error.CallId);
                writer.Write(error.ErrorResp);
                break;
            case Packet.Init init:
                writer.Write(PacketType.INIT);
                writer.Write(init.Channels);
                break;
            case Packet.Accept accept:
                writer.Write(PacketType.ACCEPT);
                foreach (short port in accept.Ports)
                {
                    writer.Write((byte)((port >> 8) & 0xFF));
                    writer.Write((byte)(port & 0xFF));
                }
                break;
            case Packet.Ping ping:
                writer.Write(PacketType.PING);
                break;
            case Packet.Pong pong:
                writer.Write(PacketType.PONG);
                break;

        }
        return buffer;
    }

    static ThreadLocal<MemoryStream> BUFFER_CACHE =
new ThreadLocal<MemoryStream>(() => new MemoryStream(1024 * 1024 * 1024));

    static MemoryStream SerializeFast(Packet packet)
    {
        int size = 1 + packet switch {
            Packet.Call call => 2 + call.Arguments.Length,
            Packet.Result result => 2 + result.Response.Length,
            Packet.Error error => 2 + error.ErrorResp.Length,
            Packet.Init init => init.Channels.Length,
            Packet.Accept accept => 2 * accept.Ports.Length,
            Packet.Ping => 0,
            Packet.Pong => 0,
            _ => throw new ArgumentOutOfRangeException()
        };
        MemoryStream buffer = BUFFER_CACHE.Value!;
        BinaryWriter writer = new BinaryWriter(buffer);
        buffer.Position = 0;
        switch (packet)
        {
            case Packet.Call call:
                writer.Write(PacketType.CALL);
                writer.Write(call.FnId);
                writer.Write(call.CallId);
                writer.Write(call.Arguments);
                break;
            case Packet.Result result:
                writer.Write(PacketType.RESULT);
                writer.Write(result.FnId);
                writer.Write(result.CallId);
                writer.Write(result.Response);
                break;
            case Packet.Error error:
                writer.Write(PacketType.ERROR);
                writer.Write(error.FnId);
                writer.Write(error.CallId);
                writer.Write(error.ErrorResp);
                break;
            case Packet.Init init:
                writer.Write(PacketType.INIT);
                writer.Write(init.Channels);
                break;
            case Packet.Accept accept:
                writer.Write(PacketType.ACCEPT);
                foreach (short port in accept.Ports)
                {
                    writer.Write((byte)((port >> 8) & 0xFF));
                    writer.Write((byte)(port & 0xFF));
                }
                break;
            case Packet.Ping ping:
                writer.Write(PacketType.PING);
                break;
            case Packet.Pong pong:
                writer.Write(PacketType.PONG);
                break;
        }
        buffer.SetLength(buffer.Position);
        buffer.Position = 0;
        return buffer;
    }

    record Call(byte FnId, byte CallId, byte[] Arguments) : Packet
    {
        public static Call Parse(byte[] data)
        {
            CREPacket crepacket = CREPacket.Parse(data);
            return new Call(crepacket.FnId, crepacket.CallId, crepacket.Arguments);
        }
    }

    record Result(byte FnId, byte CallId, byte[] Response) : Packet
    {
        public static Result Parse(byte[] data)
        {
            CREPacket crepacket = CREPacket.Parse(data);
            return new Result(crepacket.FnId, crepacket.CallId, crepacket.Arguments);
        }
    }

    record Error(byte FnId, byte CallId, byte[] ErrorResp) : Packet
    {
        public static Error Parse(byte[] data)
        {
            CREPacket crepacket = CREPacket.Parse(data);
            return new Error(crepacket.FnId, crepacket.CallId, crepacket.Arguments);
        }
    }

    record Init(byte[] Channels) : Packet { }

    record Accept(short[] Ports) : Packet
    {
        public static Accept Parse(byte[] data)
        {
            short[] ports = new short[(data.Length - 1) / 2]; // assume length is odd (even + 1 for type byte)
            for (int i = 1; i < data.Length; i += 2)
            {
                ports[i / 2] = (short)(((data[i] & 0xFF) << 8) | (data[i + 1] & 0xFF)); // BIG ENDIAN
            }
            return new Accept(ports);
        }
    }

    class Ping : Packet
    {
        public static Ping Instance = new Ping();
        private Ping() { }
    }

    class Pong : Packet
    {
        public static Pong Instance = new Pong();
        private Pong() { }
    }
}
