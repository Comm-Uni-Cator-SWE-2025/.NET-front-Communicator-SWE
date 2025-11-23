using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Communicator.Networking;
using System;

namespace Communicator.Canvas.Services;

public class CanvasNetworkBridge : IMessageListener
{
    private IMessageListener? _activeListener;

    public void SetListener(IMessageListener listener)
    {
        _activeListener = listener;
    }

    public void ReceiveData(byte[] data)
    {
        _activeListener?.ReceiveData(data);
    }
}
