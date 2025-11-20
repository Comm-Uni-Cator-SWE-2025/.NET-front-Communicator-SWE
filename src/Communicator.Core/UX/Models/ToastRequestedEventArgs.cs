/*
 * -----------------------------------------------------------------------------
 *  File: ToastRequestedEventArgs.cs
 *  Owner: UpdateNamesForEachModule
 *  Roll Number :
 *  Module : 
 *
 * -----------------------------------------------------------------------------
 */
using System;

namespace Communicator.Core.UX.Models;

/// <summary>
/// Event args for toast notification requests.
/// </summary>
public class ToastRequestedEventArgs : EventArgs
{
    public ToastMessage Message { get; }

    public ToastRequestedEventArgs(ToastMessage message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }
}

