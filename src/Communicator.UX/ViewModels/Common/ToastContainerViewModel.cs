using System.Collections.ObjectModel;
using System.Windows;
using UX.Core;
using UX.Core.Models;
using UX.Core.Services;

namespace GUI.ViewModels.Common
{
    /// <summary>
    /// Collects toast messages from <see cref="IToastService"/> and exposes them to the view for rendering.
    /// </summary>
    public class ToastContainerViewModel : ObservableObject
    {
        private readonly IToastService _toastService;
        
        public ObservableCollection<ToastMessage> Toasts { get; } = new ObservableCollection<ToastMessage>();

        /// <summary>
        /// Subscribes to toast requests and primes the container collection.
        /// </summary>
        public ToastContainerViewModel(IToastService toastService)
        {
            _toastService = toastService;
            _toastService.ToastRequested += OnToastRequested;
        }

        /// <summary>
        /// Adds incoming toast notifications on the UI dispatcher thread.
        /// </summary>
        private void OnToastRequested(ToastMessage toast)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Toasts.Add(toast);
            });
        }

        /// <summary>
        /// Removes a toast once dismissed.
        /// </summary>
        public void RemoveToast(ToastMessage toast)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Toasts.Remove(toast);
            });
        }
    }
}

