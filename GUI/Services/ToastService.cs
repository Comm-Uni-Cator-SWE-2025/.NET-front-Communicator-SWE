using System;
using GUI.Models;

namespace GUI.Services
{
    public class ToastService : IToastService
    {
        public event Action<ToastMessage>? ToastRequested;

        public void ShowSuccess(string message, int duration = 3000)
        {
            ToastRequested?.Invoke(new ToastMessage(message, ToastType.Success, duration));
        }

        public void ShowError(string message, int duration = 3000)
        {
            ToastRequested?.Invoke(new ToastMessage(message, ToastType.Error, duration));
        }

        public void ShowWarning(string message, int duration = 3000)
        {
            ToastRequested?.Invoke(new ToastMessage(message, ToastType.Warning, duration));
        }

        public void ShowInfo(string message, int duration = 3000)
        {
            ToastRequested?.Invoke(new ToastMessage(message, ToastType.Info, duration));
        }
    }
}
