// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Socketry;

public class SocketPacket
{
    public int _bytesLeft;
    public MemoryStream _content;

    public SocketPacket(int contentLength, MemoryStream content)
    {
        this._bytesLeft = contentLength;
        this._content = content;
    }
}
