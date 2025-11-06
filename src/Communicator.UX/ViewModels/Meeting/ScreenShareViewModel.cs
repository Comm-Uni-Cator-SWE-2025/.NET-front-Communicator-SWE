using Controller;
using UX.Core;

namespace GUI.ViewModels.Meeting;

/// <summary>
/// Describes the screen share tab, holding the current user for access control or display.
/// </summary>
public class ScreenShareViewModel : ObservableObject
{
    /// <summary>
    /// Initializes screen share metadata with the active user context.
    /// </summary>
    public ScreenShareViewModel(UserProfile user)
    {
        Title = "ScreenShare";
        CurrentUser = user;
    }

    public string Title { get; }
    public UserProfile CurrentUser { get; }
}

