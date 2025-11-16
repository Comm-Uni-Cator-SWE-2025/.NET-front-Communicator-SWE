using Controller;
using Communicator.Core.UX;

namespace Communicator.UX.ViewModels.Meeting;

/// <summary>
/// Describes the screen share tab, holding the current user for access control or display.
/// </summary>
public class ScreenShareViewModel : ObservableObject
{
    /// <summary>
    /// Initializes screen share metadata with the active user context.
    /// </summary>
    public ScreenShareViewModel(User user)
    {
        Title = "ScreenShare";
        CurrentUser = user;
    }

    public string Title { get; }
    public User CurrentUser { get; }
}

