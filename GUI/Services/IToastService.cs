using System;
using GUI.Models;

namespace GUI.Services
{
    public interface IToastService
    {
        event Action<ToastMessage>? ToastRequested;
        
        void ShowSuccess(string message, int duration = 3000);
        void ShowError(string message, int duration = 3000);
        void ShowWarning(string message, int duration = 3000);
        void ShowInfo(string message, int duration = 3000);
    }
}
