// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Socketry;

public record CallIdentifier(byte CallId, byte FnId)
{
    public int HashCode()
    {
        return CallId << 8 | FnId;
    }
}
