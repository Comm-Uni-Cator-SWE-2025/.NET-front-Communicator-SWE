/*
 * -----------------------------------------------------------------------------
 *  File: ThemeChangedEventArgs.cs
 *  Owner: UpdateNamesForEachModule
 *  Roll Number :
 *  Module : 
 *
 * -----------------------------------------------------------------------------
 */
using System;

namespace Communicator.Core.UX.Models;

/// <summary>
/// Event args for theme change events.
/// </summary>
public class ThemeChangedEventArgs : EventArgs
{
    public AppTheme Theme { get; }

    public ThemeChangedEventArgs(AppTheme theme)
    {
        Theme = theme;
    }
}

