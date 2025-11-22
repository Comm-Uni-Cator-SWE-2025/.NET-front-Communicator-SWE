/*
 * -----------------------------------------------------------------------------
 *  File: LoadingViewModel.cs
 *  Owner: Geetheswar V
 *  Roll Number : 142201025
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using Communicator.Core.UX;

namespace Communicator.App.ViewModels.Common;

public sealed class LoadingViewModel : ObservableObject
{
    private string _message = "Loading...";
    private bool _isBusy;

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }
}


