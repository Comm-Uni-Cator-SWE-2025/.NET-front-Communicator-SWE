using packetparser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socketry
{
    public abstract class Socketry
    {
        Dictionary<String, Func<byte[], byte[]>> _procedures;
        String[] _procedureNames;
        String[] _remoteprocedureNames;

        public Tunnel[] _tunnels;

        public byte[] GetProcedures()
        {
            MemoryStream buffer = new MemoryStream(1024);
            BinaryWriter binaryWriter = new BinaryWriter(buffer);
            foreach (String procedureName in _procedureNames)
            {
                System.Diagnostics.Debug.WriteLine($"[Socketry] procedure name: {procedureName}");
                binaryWriter.Write(Encoding.UTF8.GetBytes(procedureName));
                binaryWriter.Write((byte)0);
            }
            buffer.Position = 0;
            byte[] result = buffer.ToArray();
            return result;
        }

        public void SetProcedures(Dictionary<String, Func<byte[], byte[]>> procedures)
        {
            _procedures = procedures;
            _procedures.Add("GetProcedures", i => GetProcedures());
            _procedureNames = new String[_procedures.Count];
            _procedureNames[0] = "GetProcedures";

            int index = 1;
            foreach(String key in _procedures.Keys)
            {
                if (!key.Equals("GetProcedures"))
                {
                    System.Diagnostics.Debug.WriteLine($"[Socketry] Function {key} set at {index}");
                    _procedureNames[index++] = key;
                }
            }
        }

        public void GetRemoteProceduresNames()
        {
            byte[] initResponse = null;
            try
            {
                initResponse = MakeRemoteCall((byte)0, new byte[0], 0).Task.Result;
            }
            catch (AggregateException ex) {
                System.Diagnostics.Debug.WriteLine($"Aggregate exception {ex.ToString()}");
            }
            System.Diagnostics.Debug.WriteLine($"init response {initResponse.Length}");
            List<String> remoteProceduresNameList = new List<String>();
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
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"Exception in ListenLoop: {e}");
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
                            System.Diagnostics.Debug.WriteLine($"Packet: {packet.ToString()}");
                            byte[] response = HandleRemoteCall(callPacket.fnId, callPacket.arguments);
                            responsePacket = new Packet.Result(callPacket.fnId, callPacket.callId, response);
                        }
                        catch (Exception e)
                        {
                            responsePacket = new Packet.Error(callPacket.fnId, callPacket.callId, Encoding.UTF8.GetBytes(e.Message));
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
                        System.Diagnostics.Debug.WriteLine($"Unhandled packet {packet}");
                        break;
                    }
            }
        }

        public byte[] HandleRemoteCall(byte fnId, byte[] data)
        {
            Func<byte[], byte[]> procedure = _procedures[_procedureNames[fnId]];
            System.Diagnostics.Debug.WriteLine($"Fn: {procedure.ToString()} {data}");
            if (procedure == null)
            {
                System.Diagnostics.Debug.WriteLine("Required procedure does not exists...");
            }
            return (byte[]) procedure(data);
        }

        public byte GetRemoteProceduresId(String name)
        {
            if (_remoteprocedureNames == null)
            {
                System.Diagnostics.Debug.WriteLine("Fetching remote procedures names...");
                try
                {
                    GetRemoteProceduresNames();
                }
                catch (Exception e) { }

                System.Diagnostics.Debug.WriteLine(_remoteprocedureNames[0]);
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
}
