using System.Collections.ObjectModel;
using System.Windows;
using GUI.Core;
using GUI.Models;
using GUI.Services;

namespace GUI.ViewModels.Common
{
    public class ToastContainerViewModel : ObservableObject
    {
        private readonly IToastService _toastService;
        
        public ObservableCollection<ToastMessage> Toasts { get; } = new ObservableCollection<ToastMessage>();

        public ToastContainerViewModel(IToastService toastService)
        {
            _toastService = toastService;
            _toastService.ToastRequested += OnToastRequested;
        }

        private void OnToastRequested(ToastMessage toast)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Toasts.Add(toast);
            });
        }

        public void RemoveToast(ToastMessage toast)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Toasts.Remove(toast);
            });
        }
    }
}
