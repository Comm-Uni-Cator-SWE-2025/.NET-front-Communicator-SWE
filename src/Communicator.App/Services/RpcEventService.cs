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

namespace Communicator.App.Services;

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

    // Chat Events
    event EventHandler<RpcDataEventArgs>? ChatMessageReceived;
    event EventHandler<RpcDataEventArgs>? FileMetadataReceived;
    event EventHandler<RpcDataEventArgs>? FileSaveSuccess;
    event EventHandler<RpcDataEventArgs>? FileSaveError;
    event EventHandler<RpcDataEventArgs>? MessageDeleted;

    void TriggerFrameReceived(byte[] data);
    void TriggerStopShareReceived(byte[] data);
    void TriggerParticipantJoined(string viewerIp);
    void TriggerParticipantLeft(string viewerIp);
    void TriggerParticipantsListUpdated(string participantsJson);

    // Chat Triggers
    void TriggerChatMessageReceived(byte[] data);
    void TriggerFileMetadataReceived(byte[] data);
    void TriggerFileSaveSuccess(byte[] data);
    void TriggerFileSaveError(byte[] data);
    void TriggerMessageDeleted(byte[] data);
}

public sealed class RpcEventService : IRpcEventService
{
    public event EventHandler<RpcDataEventArgs>? FrameReceived;
    public event EventHandler<RpcDataEventArgs>? StopShareReceived;
    public event EventHandler<RpcStringEventArgs>? ParticipantJoined;
    public event EventHandler<RpcStringEventArgs>? ParticipantLeft;
    public event EventHandler<RpcStringEventArgs>? ParticipantsListUpdated;

    // Chat Events
    public event EventHandler<RpcDataEventArgs>? ChatMessageReceived;
    public event EventHandler<RpcDataEventArgs>? FileMetadataReceived;
    public event EventHandler<RpcDataEventArgs>? FileSaveSuccess;
    public event EventHandler<RpcDataEventArgs>? FileSaveError;
    public event EventHandler<RpcDataEventArgs>? MessageDeleted;

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

    // Chat Triggers
    public void TriggerChatMessageReceived(byte[] data)
    {
        ChatMessageReceived?.Invoke(this, new RpcDataEventArgs(data));
    }

    public void TriggerFileMetadataReceived(byte[] data)
    {
        FileMetadataReceived?.Invoke(this, new RpcDataEventArgs(data));
    }

    public void TriggerFileSaveSuccess(byte[] data)
    {
        FileSaveSuccess?.Invoke(this, new RpcDataEventArgs(data));
    }

    public void TriggerFileSaveError(byte[] data)
    {
        FileSaveError?.Invoke(this, new RpcDataEventArgs(data));
    }

    public void TriggerMessageDeleted(byte[] data)
    {
        MessageDeleted?.Invoke(this, new RpcDataEventArgs(data));
    }
}


