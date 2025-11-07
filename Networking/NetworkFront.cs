// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;

namespace Networking;

/**
 * The frontend networking class to connect to the RPC and to the core
 * netwroking.
 */
public class NetworkFront : IController, INetworking
{
    /** Variable to store the function mappings. */
    private Dictionary<int, IMessageListener> _listeners;

    /** Variable to track the number of functions. */
    private int _functionCount = 1;

    public void SendData(byte[] data, ClientNode[] dest, int module, int priority)
    {
        int dataLength = data.Length;
        int destSize = 0;
        foreach (ClientNode record in dest)
        {
            byte[] hostName = Encoding.UTF8.GetBytes(record.HostName);
            destSize += 1 + hostName.Length + sizeof(int); // 1 byte length + host + port
        }
        // 1 - data length 1 - dest count 1 - module 1 - priority
        int bufferSize = dataLength + destSize + 4 * sizeof(int);
        MemoryStream buffer = new MemoryStream(bufferSize);
        BinaryWriter writer = new BinaryWriter(buffer);
        writer.Write(dest.Length);
        foreach (ClientNode record in dest)
        {
            byte[] hostName = Encoding.UTF8.GetBytes(record.HostName);
            writer.Write((byte)hostName.Length);
            writer.Write(hostName);
            writer.Write(record.Port);
        }
        writer.Write(dataLength);
        writer.Write(data);
        writer.Write(module);
        writer.Write(priority);
        byte[] args = buffer.ToArray();

        // TODO: Call RPC broadcast
    }

    public void Broadcast(byte[] data, int module, int priority)
    {
        int dataLength = data.Length;
        int bufferSize = dataLength + 3 * sizeof(int);
        MemoryStream buffer = new MemoryStream(bufferSize);
        BinaryWriter writer = new BinaryWriter(buffer);
        writer.Write(dataLength);
        writer.Write(data);
        writer.Write(module);
        writer.Write(priority);
        byte[] args = buffer.ToArray();

        // TODO: Call RPC broadcast
    }

    public void Subscribe(int name, IMessageListener function)
    {
        _listeners.Add(name, function);
        int bufferSize = sizeof(int);
        MemoryStream buffer = new MemoryStream(bufferSize);
        BinaryWriter writer = new BinaryWriter(buffer);
        writer.Write(name);
        byte[] args = buffer.ToArray();

        // TODO: Call RPC Add function
    }

    public void RemoveSubscription(int name)
    {
        int bufferSize = sizeof(int);
        MemoryStream buffer = new MemoryStream(bufferSize);
        BinaryWriter writer = new BinaryWriter(buffer);
        writer.Write(name);
        byte[] args = buffer.ToArray();

        // TODO: Call RPC remove
    }

    public void AddUser(ClientNode deviceAddress, ClientNode mainServerAddress)
    {
        int bufferSize = 2 + deviceAddress.HostName.Length + mainServerAddress.HostName.Length
                + 2 * sizeof(int);
        MemoryStream buffer = new MemoryStream(bufferSize);
        BinaryWriter writer = new BinaryWriter(buffer);
        byte[] hostName = Encoding.UTF8.GetBytes(deviceAddress.HostName);
        writer.Write((byte)hostName.Length);
        writer.Write(hostName);
        writer.Write(deviceAddress.Port);
        hostName = Encoding.UTF8.GetBytes(mainServerAddress.HostName);
        writer.Write((byte)hostName.Length);
        writer.Write(hostName);
        writer.Write(mainServerAddress.Port);
        byte[] args = buffer.ToArray();

        // TODO: Call RPC addUser
    }

    /**
     * Function to call the subscriber in frontend.
     *
     * @param data the data to send
     */
    public void NetworkFrontCallSubscriber(byte[] data)
    {
        int dataSize = data.Length - 1;
        MemoryStream buffer = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(buffer);
        int module = reader.ReadInt32();
        byte[] newData = new byte[dataSize];
        IMessageListener function = _listeners.GetValueOrDefault(module);
        function?.ReceiveData(newData);
    }
}
