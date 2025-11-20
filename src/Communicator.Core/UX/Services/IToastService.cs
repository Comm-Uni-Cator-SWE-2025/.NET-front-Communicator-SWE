/*
 * -----------------------------------------------------------------------------
 *  File: IToastService.cs
 *  Owner: UpdateNamesForEachModule
 *  Roll Number :
 *  Module : 
 *
 * -----------------------------------------------------------------------------
 */
using System;
using Communicator.Core.UX.Models;

namespace Communicator.Core.UX.Services;

/// <summary>
/// Publishes toast notification requests so the UI layer can surface transient feedback to the user.
/// </summary>
public interface IToastService
{
    /// <summary>
    /// Raised whenever a toast should be displayed; subscribers decide how to render the message.
    /// </summary>
    event EventHandler<ToastRequestedEventArgs>? ToastRequested;

    /// <summary>
    /// Shows a success toast conveying a positive outcome.
    /// </summary>
    void ShowSuccess(string message, int duration = 3000);

    /// <summary>
    /// Shows an error toast highlighting an operation failure.
    /// </summary>
    void ShowError(string message, int duration = 3000);

    /// <summary>
    /// Shows a warning toast for non-fatal issues that still require attention.
    /// </summary>
    void ShowWarning(string message, int duration = 3000);

    /// <summary>
    /// Shows an informational toast to deliver neutral status updates.
    /// </summary>
    void ShowInfo(string message, int duration = 3000);
}

