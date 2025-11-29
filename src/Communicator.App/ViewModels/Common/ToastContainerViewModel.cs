/*
 * -----------------------------------------------------------------------------
 *  File: ToastContainerViewModel.cs
 *  Owner: Geetheswar V
 *  Roll Number : 142201025
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System.Collections.ObjectModel;
using System.Windows;
using Communicator.UX.Core;
using Communicator.UX.Core.Models;
using Communicator.UX.Core.Services;

namespace Communicator.App.ViewModels.Common;

/// <summary>
/// Collects toast messages from <see cref="IToastService"/> and exposes them to the view for rendering.
/// </summary>
public sealed class ToastContainerViewModel : ObservableObject
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
    private void OnToastRequested(object? sender, ToastRequestedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() => {
            Toasts.Add(e.Message);
        });
    }

    /// <summary>
    /// Removes a toast once dismissed.
    /// </summary>
    public void RemoveToast(ToastMessage toast)
    {
        Application.Current.Dispatcher.Invoke(() => {
            Toasts.Remove(toast);
        });
    }
}



