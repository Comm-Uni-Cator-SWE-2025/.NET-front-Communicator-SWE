using System;
using UX.Core.Models;

namespace UX.Core.Services;

/// <summary>
/// Emits toast notifications by invoking ToastRequested with preconfigured message types.
/// Thread-safe implementation ensures handlers don't get removed during invocation.
/// </summary>
public class ToastService : IToastService
{
    public event Action<ToastMessage>? ToastRequested;

    public void ShowSuccess(string message, int duration = 3000)
    {
        InvokeToastRequested(new ToastMessage(message, ToastType.Success, duration));
    }

    public void ShowError(string message, int duration = 3000)
    {
        InvokeToastRequested(new ToastMessage(message, ToastType.Error, duration));
    }

    public void ShowWarning(string message, int duration = 3000)
    {
        InvokeToastRequested(new ToastMessage(message, ToastType.Warning, duration));
    }

    public void ShowInfo(string message, int duration = 3000)
    {
        InvokeToastRequested(new ToastMessage(message, ToastType.Info, duration));
    }
    
    /// <summary>
    /// Thread-safe event invocation to prevent issues with concurrent subscription changes.
    /// </summary>
    private void InvokeToastRequested(ToastMessage message)
    {
        // Capture the event handler to prevent race conditions
        var handler = ToastRequested;
        handler?.Invoke(message);
    }
}
