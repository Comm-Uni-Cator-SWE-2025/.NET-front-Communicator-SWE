using System;
using GUI.Models;

namespace GUI.Services
{
    /// <summary>
    /// Emits toast notifications by invoking <see cref="ToastRequested"/> with preconfigured message types.
    /// </summary>
    public class ToastService : IToastService
    {
        /// <inheritdoc />
        public event Action<ToastMessage>? ToastRequested;

        /// <inheritdoc />
        public void ShowSuccess(string message, int duration = 3000)
        {
            ToastRequested?.Invoke(new ToastMessage(message, ToastType.Success, duration));
        }

        /// <inheritdoc />
        public void ShowError(string message, int duration = 3000)
        {
            ToastRequested?.Invoke(new ToastMessage(message, ToastType.Error, duration));
        }

        /// <inheritdoc />
        public void ShowWarning(string message, int duration = 3000)
        {
            ToastRequested?.Invoke(new ToastMessage(message, ToastType.Warning, duration));
        }

        /// <inheritdoc />
        public void ShowInfo(string message, int duration = 3000)
        {
            ToastRequested?.Invoke(new ToastMessage(message, ToastType.Info, duration));
        }
    }
}
