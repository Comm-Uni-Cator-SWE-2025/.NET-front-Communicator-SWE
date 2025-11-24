using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communicator.Networking;
/*
 * -----------------------------------------------------------------------------
 *  File: CanvasNetworkBridge.cs
 *  Owner: Shanmukha Sri Krishna
 *  Roll Number : 112201013
 *  Module : Canvas
 *
 * -----------------------------------------------------------------------------
 */
//to be implemented
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
