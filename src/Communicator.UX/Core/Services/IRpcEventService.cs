/*
 * -----------------------------------------------------------------------------
 *  File: IRpcEventService.cs
 *  Owner: Geetheswar V
 *  Roll Number : 142201025
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System;

namespace Communicator.UX.Core.Services;

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

    // Canvas Events
    event EventHandler<RpcDataEventArgs>? CanvasUpdateReceived;

    event EventHandler<RpcDataEventArgs>? CanvasAnalyticsUpdateReceived;

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

    // Canvas Triggers
    void TriggerCanvasUpdateReceived(byte[] data);

    void TriggerCanvasAnalyticsUpdateReceived(byte[] data);
}
