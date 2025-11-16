// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Communicator.ScreenShare;
public interface AbstractRPC
{
    void Subscribe(string name, Func<byte[], byte[]> func);

    Thread Connect();

    Task<byte[]> CallAsync(string name, byte[] args);
}
