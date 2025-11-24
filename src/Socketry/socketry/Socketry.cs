// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using Packetparser;

namespace Socketry;

public abstract class Socketry
{
    Dictionary<string, Func<byte[], byte[]>> _procedures;
    string[]? _procedureNames;
    string[] _remoteprocedureNames;

    public Tunnel[] _tunnels;

    public byte[] GetProcedures()
    {
        MemoryStream buffer = new MemoryStream(1024);
        BinaryWriter binaryWriter = new BinaryWriter(buffer);
        foreach (string procedureName in _procedureNames)
        {
            Console.WriteLine(procedureName);
            binaryWriter.Write(Encoding.UTF8.GetBytes(procedureName));
            binaryWriter.Write((byte)0);
        }
        buffer.Position = 0;
        byte[] result = buffer.ToArray();
        return result;
    }

    public void SetProcedures(Dictionary<string, Func<byte[], byte[]>> procedures)
    {
        _procedures = procedures;
        _procedures.Add("GetProcedures", i => GetProcedures());
        _procedureNames = new string[_procedures.Count];
        _procedureNames[0] = "GetProcedures";

        int index = 1;
        foreach (string key in _procedures.Keys)
        {
            if (!key.Equals("GetProcedures"))
            {
                Console.WriteLine($"Function {key} set at {index}");
                _procedureNames[index++] = key;
            }
        }
    }

    public void GetRemoteProceduresNames()
    {
        byte[] initResponse = null;
        try
        {
            Console.WriteLine($"1");
            initResponse = MakeRemoteCall((byte)0, new byte[0], 0).Task.Result;
            Console.WriteLine($"2");
        }
        catch (AggregateException ex)
        {
            Console.WriteLine($"3");
            Console.WriteLine($"Aggregrate exception {ex.ToString()}");
            Console.WriteLine($"4");
        }
        Console.WriteLine($"init response {initResponse.Length}");
        List<string> remoteProceduresNameList = new List<string>();
        StringBuilder currentName = new StringBuilder();
        foreach (byte b in initResponse)
        {
            if (b == 0)
            {
                remoteProceduresNameList.Add(currentName.ToString());
                currentName = new StringBuilder();
            }
            else
            {
                currentName.Append((char)b);
            }
        }
        _remoteprocedureNames = remoteProceduresNameList.ToArray();
    }

    public void ListenLoop()
    {
        try
        {
            StartListening();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception in ListenLoop: {e}");
        }
    }

    public void StartListening()
    {
        if (_tunnels == null)
        {
            throw new InvalidOperationException("Tunnels not initialized. Cannot start listening.");
        }

        while (true)
        {
            foreach (Tunnel tunnel in _tunnels)
            {
                List<Packet> unhandledPackets = tunnel.Listen();
                unhandledPackets.ForEach(packet => {
                    HandlePacket(packet, tunnel);
                });
            }
        }
    }

    public void HandlePacket(Packet packet, Tunnel tunnel)
    {
        switch (packet)
        {
            case Packet.Call callPacket:
                {
                    Packet responsePacket;
                    try
                    {
                        Console.WriteLine($"Packet: {packet.ToString()}");
                        byte[] response = HandleRemoteCall(callPacket.FnId, callPacket.Arguments);
                        responsePacket = new Packet.Result(callPacket.FnId, callPacket.CallId, response);
                    }
                    catch (Exception e)
                    {
                        responsePacket = new Packet.Error(callPacket.FnId, callPacket.CallId, Encoding.UTF8.GetBytes(e.Message));
                    }
                    tunnel.SendPacket(responsePacket);
                    break;
                }
            case Packet.Ping ignored:
                {
                    tunnel.SendPacket(Packet.Ping.Instance);
                    break;
                }
            default:
                {
                    Console.WriteLine($"Unhandled packet {packet}");
                    break;
                }
        }
    }

    public byte[] HandleRemoteCall(byte fnId, byte[] data)
    {
        Func<byte[], byte[]> procedure = _procedures[_procedureNames[fnId]];
        Console.WriteLine($"Fn: {procedure.ToString()} {data}");
        if (procedure == null)
        {
            Console.WriteLine("Required procedure does not exists...");
        }
        return (byte[])procedure(data);
    }

    public byte GetRemoteProceduresId(string name)
    {
        if (_remoteprocedureNames == null)
        {
            Console.WriteLine("Fetching remote procedures names...");
            try
            {
                GetRemoteProceduresNames();
            }
            catch (Exception e) { }

            Console.WriteLine(_remoteprocedureNames[0]);
        }

        for (byte i = 0; i < _remoteprocedureNames.Length; i++)
        {
            if (_remoteprocedureNames[i].Equals(name))
            {
                return i;
            }
        }

        return 0;
        // in java throws error
    }
    public TaskCompletionSource<byte[]> MakeRemoteCall(byte fnId, byte[] data, int tunnnelId)
    {
        if (tunnnelId < 0 || tunnnelId >= _tunnels.Length)
        {
            // Throw error
        }

        Tunnel tnl = _tunnels[tunnnelId];
        return tnl.CallFn(fnId, data);
    }
}
