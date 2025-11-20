using Communicator.Core.UX;

namespace Communicator.UX.ViewModels.Common;

public class LoadingViewModel : ObservableObject
{
    private string _message = "Loading...";
    private bool _isBusy = false;

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
