// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NewSocket;

public class SelectionKey
{
    public static int OP_READ = 1;
    public static int OP_WRITE = 2;
    public static int OP_CONNECT = 4;
    private object? _att;
    private bool _isValid = true;
    private bool _isReadable = false;
    private bool _isWritable = false;
    private bool _isConnectable = false;

    public void SetAtt(object att)
    {
        this._att = att;
    }

    public void SetReadable(bool val)
    {
        _isReadable = val;
    }
    public void SetWritable(bool val)
    {
        _isWritable = val;
    }
    public void SetConnectable(bool val)
    {
        _isConnectable = val;
    }

    public bool IsValid()
    {
        return _isValid;
    }

    public bool IsReadable()
    {
        return _isReadable;
    }
    public bool IsWritable()
    {
        return _isWritable;
    }
    public bool IsConnectable()
    {
        return _isConnectable;
    }

    public object Attachment()
    {
        return _att;
    }
}
