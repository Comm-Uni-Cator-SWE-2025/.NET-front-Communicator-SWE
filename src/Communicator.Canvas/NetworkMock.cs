/*
 * -----------------------------------------------------------------------------
 *  File: NetworkMock.cs
 *  Owner: Sriram Nangunoori
 *  Roll Number : 112201019
 *  Module : Canvas
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.Collections.Generic;

namespace Communicator.Canvas;

public static class NetworkMock
{
    // Registry of listeners
    private static Dictionary<string, Action<string>> s_listeners = new();

    public static void Register(string ip, Action<string> listener)
    {
        s_listeners[ip] = listener;
    }

    public static void SendMessage(string targetIp, string jsonMessage)
    {
        System.Diagnostics.Debug.WriteLine($"[Network] Sending to {targetIp}");
        if (s_listeners.ContainsKey(targetIp))
        {
            s_listeners[targetIp].Invoke(jsonMessage);
        }
    }

    public static void Broadcast(List<string> clientIps, string jsonMessage)
    {
        System.Diagnostics.Debug.WriteLine($"[Network] Broadcasting");
        foreach (string ip in clientIps)
        {
            SendMessage(ip, jsonMessage);
        }
    }
}
