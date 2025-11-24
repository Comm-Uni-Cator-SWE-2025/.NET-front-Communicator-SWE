// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net;
using System.Net.Sockets;

namespace NewSocket;

public interface IServerSocket
{
    Socket Bind(EndPoint local)
    {
        return Bind(local, 0);
    }

    Socket Bind(EndPoint local, int backlog);

    Socket ConfigureBlocking(bool block);

    ISocket Accept();
}
