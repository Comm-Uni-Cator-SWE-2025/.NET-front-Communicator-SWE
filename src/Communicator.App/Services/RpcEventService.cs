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
    event EventHandler<RpcStringEventArgs>? ParticipantsListUpdated;
    event EventHandler<RpcStringEventArgs>? Logout;
    event EventHandler<RpcStringEventArgs>? EndMeeting;

    // Chat Events
    event EventHandler<RpcDataEventArgs>? ChatMessageReceived;
    event EventHandler<RpcDataEventArgs>? FileMetadataReceived;
    event EventHandler<RpcDataEventArgs>? FileSaveSuccess;
    event EventHandler<RpcDataEventArgs>? FileSaveError;
    event EventHandler<RpcDataEventArgs>? MessageDeleted;

    void TriggerFrameReceived(byte[] data);
    void TriggerStopShareReceived(byte[] data);
    void TriggerParticipantsListUpdated(string participantsJson);
    void TriggerLogout(string message);
    void TriggerEndMeeting(string message);

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
    public event EventHandler<RpcStringEventArgs>? ParticipantsListUpdated;
    public event EventHandler<RpcStringEventArgs>? Logout;
    public event EventHandler<RpcStringEventArgs>? EndMeeting;

    // Chat Events
    public event EventHandler<RpcDataEventArgs>? ChatMessageReceived;
    public event EventHandler<RpcDataEventArgs>? FileMetadataReceived;
    public event EventHandler<RpcDataEventArgs>? FileSaveSuccess;
    public event EventHandler<RpcDataEventArgs>? FileSaveError;
    public event EventHandler<RpcDataEventArgs>? MessageDeleted;

    public void TriggerFrameReceived(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);


        Console.WriteLine($"[App] UPDATE_UI 2 TriggerFrameReceived called Received UPDATE_UI with {data.Length} bytes" + FrameReceived);
        System.Diagnostics.Debug.WriteLine("UPDATE UI: Got FrameReceived event" + FrameReceived);
        FrameReceived?.Invoke(this, new RpcDataEventArgs(data));
    }

    public void TriggerStopShareReceived(byte[] data)
    {
        StopShareReceived?.Invoke(this, new RpcDataEventArgs(data));
    }

    public void TriggerParticipantsListUpdated(string participantsJson)
    {
        ParticipantsListUpdated?.Invoke(this, new RpcStringEventArgs(participantsJson));
    }

    public void TriggerLogout(string message)
    {
        Logout?.Invoke(this, new RpcStringEventArgs(message));
    }

    public void TriggerEndMeeting(string message)
    {
        EndMeeting?.Invoke(this, new RpcStringEventArgs(message));
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


