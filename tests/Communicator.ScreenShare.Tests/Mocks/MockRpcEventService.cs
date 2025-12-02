/*
 * -----------------------------------------------------------------------------
 *  File: MockRpcEventService.cs
 *  Owner: Devansh Manoj Kesan
 *  Roll Number :142201017
 *  Module : ScreenShare
 *
 * -----------------------------------------------------------------------------
 */

using System;
using Communicator.App.Services;

namespace Communicator.ScreenShare.Tests.Mocks;

public class MockRpcEventService : IRpcEventService
{
    public event EventHandler<RpcDataEventArgs>? FrameReceived;
    public event EventHandler<RpcDataEventArgs>? StopShareReceived;
    public event EventHandler<RpcStringEventArgs>? Logout;
    public event EventHandler<RpcStringEventArgs>? EndMeeting;

    // Extra convenience events used in some tests (not part of IRpcEventService).
    public event EventHandler<RpcStringEventArgs>? ParticipantJoined;
    public event EventHandler<RpcStringEventArgs>? ParticipantLeft;
    public event EventHandler<RpcStringEventArgs>? ParticipantsListUpdated;
    public event EventHandler<RpcDataEventArgs>? ChatMessageReceived;
    public event EventHandler<RpcDataEventArgs>? FileMetadataReceived;
    public event EventHandler<RpcDataEventArgs>? FileSaveSuccess;
    public event EventHandler<RpcDataEventArgs>? FileSaveError;
    public event EventHandler<RpcDataEventArgs>? MessageDeleted;

    // Each helper below simply raises the corresponding event so tests can simulate inbound RPC activity.
    public void TriggerFrameReceived(byte[] data) => FrameReceived?.Invoke(this, new RpcDataEventArgs(data));
    public void TriggerStopShareReceived(byte[] data) => StopShareReceived?.Invoke(this, new RpcDataEventArgs(data));
    public void TriggerLogout(string message) => Logout?.Invoke(this, new RpcStringEventArgs(message));
    public void TriggerEndMeeting(string message) => EndMeeting?.Invoke(this, new RpcStringEventArgs(message));

    // Helpers for additional test-only events.
    public void TriggerParticipantJoined(string ip) => ParticipantJoined?.Invoke(this, new RpcStringEventArgs(ip));
    public void TriggerParticipantLeft(string ip) => ParticipantLeft?.Invoke(this, new RpcStringEventArgs(ip));
    public void TriggerParticipantsListUpdated(string json) => ParticipantsListUpdated?.Invoke(this, new RpcStringEventArgs(json));
    public void TriggerChatMessageReceived(byte[] data) => ChatMessageReceived?.Invoke(this, new RpcDataEventArgs(data));
    public void TriggerFileMetadataReceived(byte[] data) => FileMetadataReceived?.Invoke(this, new RpcDataEventArgs(data));
    public void TriggerFileSaveSuccess(byte[] data) => FileSaveSuccess?.Invoke(this, new RpcDataEventArgs(data));
    public void TriggerFileSaveError(byte[] data) => FileSaveError?.Invoke(this, new RpcDataEventArgs(data));
    public void TriggerMessageDeleted(byte[] data) => MessageDeleted?.Invoke(this, new RpcDataEventArgs(data));
}

