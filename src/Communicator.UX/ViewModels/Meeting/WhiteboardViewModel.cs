using Controller;
using Communicator.Core.UX;

namespace Communicator.UX.ViewModels.Meeting;

/// <summary>
/// Represents the collaborative whiteboard tab state for the active meeting.
/// </summary>
public class WhiteboardViewModel : ObservableObject
{
    /// <summary>
    /// Initializes the whiteboard model with the given user context.
    /// </summary>
    public WhiteboardViewModel(User user)
    {
        Title = "Whiteboard";
        CurrentUser = user;
    }

    public string Title { get; }
    public User CurrentUser { get; }
}

