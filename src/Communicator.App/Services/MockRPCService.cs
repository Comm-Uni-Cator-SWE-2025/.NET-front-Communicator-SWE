using System;
using System.Threading;
using System.Threading.Tasks;
using Communicator.Core.RPC;

namespace Communicator.App.Services;

public class MockRPCService : IRPC
{
    public Task<byte[]> Call(string methodName, byte[] data)
    {
        System.Diagnostics.Debug.WriteLine($"[MockRPC] Call: {methodName}");
        // Return empty byte array or mock response
        return Task.FromResult(Array.Empty<byte>());
    }

    public Thread Connect(int portNumber)
    {
        System.Diagnostics.Debug.WriteLine($"[MockRPC] Connect: {portNumber}");
        // Return a dummy thread that does nothing
        var thread = new Thread(() => {
            while (true)
            {
                Thread.Sleep(1000);
            }
        }) {
            IsBackground = true
        };
        thread.Start();
        return thread;
    }

    public void Subscribe(string methodName, Func<byte[], byte[]> method)
    {
        System.Diagnostics.Debug.WriteLine($"[MockRPC] Subscribe: {methodName}");
    }
}
