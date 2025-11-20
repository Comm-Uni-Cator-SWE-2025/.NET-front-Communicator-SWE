/*
 * -----------------------------------------------------------------------------
 *  File: RpcEventService.cs
 *  Owner: Geetheswar V
 *  Roll Number : 142201025
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System;

namespace Communicator.UX.Services;

public class RpcDataEventArgs : EventArgs
{
    public ReadOnlyMemory<byte> Data { get; }
    public RpcDataEventArgs(ReadOnlyMemory<byte> data)
    {
        Data = data;
    }
}

public class RpcStringEventArgs : EventArgs
{
    public string Value { get; }
    public RpcStringEventArgs(string value)
    {
        Value = value;
    }
}

public interface IRpcEventService
{
    event EventHandler<RpcDataEventArgs>? FrameReceived;
    event EventHandler<RpcDataEventArgs>? StopShareReceived;
    event EventHandler<RpcStringEventArgs>? ParticipantJoined;
    event EventHandler<RpcStringEventArgs>? ParticipantLeft;
    event EventHandler<RpcStringEventArgs>? ParticipantsListUpdated;

    void TriggerFrameReceived(byte[] data);
    void TriggerStopShareReceived(byte[] data);
    void TriggerParticipantJoined(string viewerIp);
    void TriggerParticipantLeft(string viewerIp);
    void TriggerParticipantsListUpdated(string participantsJson);
}

public sealed class RpcEventService : IRpcEventService
{
    public event EventHandler<RpcDataEventArgs>? FrameReceived;
    public event EventHandler<RpcDataEventArgs>? StopShareReceived;
    public event EventHandler<RpcStringEventArgs>? ParticipantJoined;
    public event EventHandler<RpcStringEventArgs>? ParticipantLeft;
    public event EventHandler<RpcStringEventArgs>? ParticipantsListUpdated;

    public void TriggerFrameReceived(byte[] data)
    {
        FrameReceived?.Invoke(this, new RpcDataEventArgs(data));
    }

    public void TriggerStopShareReceived(byte[] data)
    {
        StopShareReceived?.Invoke(this, new RpcDataEventArgs(data));
    }

    public void TriggerParticipantJoined(string viewerIp)
    {
        ParticipantJoined?.Invoke(this, new RpcStringEventArgs(viewerIp));
    }

    public void TriggerParticipantLeft(string viewerIp)
    {
        ParticipantLeft?.Invoke(this, new RpcStringEventArgs(viewerIp));
    }

    public void TriggerParticipantsListUpdated(string participantsJson)
    {
        ParticipantsListUpdated?.Invoke(this, new RpcStringEventArgs(participantsJson));
    }
}


