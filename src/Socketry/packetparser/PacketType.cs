// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Packetparser;

public class PacketType
{
    public const byte CALL = 1;
    public const byte RESULT = 2;
    public const byte ERROR = 3;
    public const byte INIT = 4;
    public const byte ACCEPT = 5;
    public const byte PING = 6;
    public const byte PONG = 7;
}
