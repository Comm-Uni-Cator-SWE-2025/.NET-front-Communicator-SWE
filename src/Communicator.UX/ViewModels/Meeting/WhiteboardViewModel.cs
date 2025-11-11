using Controller;
using UX.Core;

namespace GUI.ViewModels.Meeting;

/// <summary>
/// Represents the collaborative whiteboard tab state for the active meeting.
/// </summary>
public class WhiteboardViewModel : ObservableObject
{
    /// <summary>
    /// Initializes the whiteboard model with the given user context.
    /// </summary>
    public WhiteboardViewModel(UserProfile user)
    {
        Title = "Whiteboard";
        CurrentUser = user;
    }

    public string Title { get; }
    public UserProfile CurrentUser { get; }
}

