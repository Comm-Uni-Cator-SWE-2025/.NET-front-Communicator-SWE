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
using Communicator.Core.UX.Services;

namespace Communicator.App.Services;

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

    // Canvas Events
    public event EventHandler<RpcDataEventArgs>? CanvasUpdateReceived;

    public void TriggerFrameReceived(byte[] data)
    {
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

    public void TriggerCanvasUpdateReceived(byte[] data)
    {
        CanvasUpdateReceived?.Invoke(this, new RpcDataEventArgs(data));
    }
}


