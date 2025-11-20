using System;

namespace Communicator.UX.Services;

public interface IRpcEventService
{
    event EventHandler<byte[]>? FrameReceived;
    event EventHandler<byte[]>? StopShareReceived;
    event EventHandler<string>? ParticipantJoined;
    event EventHandler<string>? ParticipantLeft;
    event EventHandler<string>? ParticipantsListUpdated;

    void RaiseFrameReceived(byte[] data);
    void RaiseStopShareReceived(byte[] data);
    void RaiseParticipantJoined(string viewerIp);
    void RaiseParticipantLeft(string viewerIp);
    void RaiseParticipantsListUpdated(string participantsJson);
}

public class RpcEventService : IRpcEventService
{
    public event EventHandler<byte[]>? FrameReceived;
    public event EventHandler<byte[]>? StopShareReceived;
    public event EventHandler<string>? ParticipantJoined;
    public event EventHandler<string>? ParticipantLeft;
    public event EventHandler<string>? ParticipantsListUpdated;

    public void RaiseFrameReceived(byte[] data)
    {
        FrameReceived?.Invoke(this, data);
    }

    public void RaiseStopShareReceived(byte[] data)
    {
        StopShareReceived?.Invoke(this, data);
    }

    public void RaiseParticipantJoined(string viewerIp)
    {
        ParticipantJoined?.Invoke(this, viewerIp);
    }

    public void RaiseParticipantLeft(string viewerIp)
    {
        ParticipantLeft?.Invoke(this, viewerIp);
    }

    public void RaiseParticipantsListUpdated(string participantsJson)
    {
        ParticipantsListUpdated?.Invoke(this, participantsJson);
    }
}
