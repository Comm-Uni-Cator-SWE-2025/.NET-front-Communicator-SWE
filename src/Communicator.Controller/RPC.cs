// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Communicator.Core.RPC;
using socketry;

namespace Communicator.Controller;

/// <summary>
/// RPC implementation for Communicator.Controller using Socketry networking layer.
/// Wraps SocketryServer to provide RPC functionality over sockets.
/// </summary>
public class RPCService : IRPC
{
    private Dictionary<string, Func<byte[], byte[]>> _methods;
    private SocketryServer? _socketryServer;
    private bool _isConnected;

    /// <summary>
    /// Initializes a new instance of the RPCService class.
    /// </summary>
    public RPCService()
    {
        _methods = new Dictionary<string, Func<byte[], byte[]>>();
        _isConnected = false;
    }

    /// <summary>
    /// Registers a procedure on the server.
    /// </summary>
    /// <param name="methodName">The string name of the procedure.</param>
    /// <param name="method">The function to execute when the procedure is called.</param>
    public void Subscribe(string methodName, Func<byte[], byte[]> method)
    {
        if (_isConnected)
        {
            throw new InvalidOperationException("Cannot subscribe methods after Connect() has been called. Subscribe all methods before calling Connect().");
        }

        if (_methods.ContainsKey(methodName))
        {
            throw new ArgumentException($"Method '{methodName}' is already subscribed.");
        }

        _methods[methodName] = method;
        Console.WriteLine($"[RPC] Subscribed method: {methodName}");
    }

    /// <summary>
    /// Connects the RPC server and starts its listener thread.
    /// </summary>
    /// <param name="portNumber">The port to listen on.</param>
    /// <returns>The thread that the server is running on.</returns>
    public Thread Connect(int portNumber)
    {
        if (_isConnected)
        {
            throw new InvalidOperationException("RPC is already connected. Cannot call Connect() multiple times.");
        }

        Console.WriteLine($"[RPC] Connecting to port: {portNumber} with {_methods.Count} method(s)");

        // Create SocketryServer with the registered methods
        _socketryServer = new SocketryServer(portNumber, _methods);
        _isConnected = true;

        // Start the listener thread
        Thread rpcThread = new(() => {
            try
            {
                Console.WriteLine("[RPC] Starting listen loop...");
                _socketryServer.ListenLoop();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Console.WriteLine($"[RPC] Error in listen loop: {ex.Message}");
            }
        }) {
            IsBackground = true,
            Name = "RPC-Listener-Thread",
        };

        rpcThread.Start();

        // Give the server a moment to start
        Thread.Sleep(100);

        // Discover remote procedures
        try
        {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            Console.WriteLine("[RPC] Discovering remote procedures...");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
            _socketryServer.GetRemoteProceduresNames();
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            Console.WriteLine("[RPC] Remote procedures discovered successfully");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            Console.WriteLine($"[RPC] Warning: Could not discover remote procedures: {ex.Message}");
        }

        return rpcThread;
    }

    /// <summary>
    /// Calls a remote procedure on the server.
    /// </summary>
    /// <param name="methodName">The string name of the procedure to call.</param>
    /// <param name="data">The byte array argument.</param>
    /// <returns>A Task that will eventually contain the byte[] response.</returns>
    public Task<byte[]> Call(string methodName, byte[] data)
    {
        if (!_isConnected || _socketryServer == null)
        {
            throw new InvalidOperationException("RPC is not connected. Call Connect() before making remote calls.");
        }

        Console.WriteLine($"[RPC] Calling remote method: {methodName}");

        // Get the remote procedure ID for the method name
        byte methodId = _socketryServer.GetRemoteProceduresId(methodName);
        Console.WriteLine($"[RPC] Method '{methodName}' mapped to ID: {methodId}");

        // Make the remote call (using tunnel 0 by default)
        TaskCompletionSource<byte[]> taskCompletionSource = _socketryServer.MakeRemoteCall(methodId, data, 0);

        // Return the Task from TaskCompletionSource
        return taskCompletionSource.Task;
    }
}
