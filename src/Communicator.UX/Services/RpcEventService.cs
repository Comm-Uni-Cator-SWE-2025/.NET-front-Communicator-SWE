using System;

namespace Communicator.UX.Services;

public interface IRpcEventService
{
    event EventHandler<byte[]>? FrameReceived;
    event EventHandler<byte[]>? StopShareReceived;
    event EventHandler<string>? ParticipantJoined;

    void RaiseFrameReceived(byte[] data);
    void RaiseStopShareReceived(byte[] data);
    void RaiseParticipantJoined(string viewerIp);
}

public class RpcEventService : IRpcEventService
{
    public event EventHandler<byte[]>? FrameReceived;
    public event EventHandler<byte[]>? StopShareReceived;
    public event EventHandler<string>? ParticipantJoined;

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
}
