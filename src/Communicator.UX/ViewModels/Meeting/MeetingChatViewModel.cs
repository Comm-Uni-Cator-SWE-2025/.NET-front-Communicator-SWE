using Controller;
using Communicator.Core.UX;

namespace GUI.ViewModels.Meeting;

/// <summary>
/// Represents the meeting chat pane, exposing metadata about the current user.
/// </summary>
public class MeetingChatViewModel : ObservableObject
{
    /// <summary>
    /// Initializes the chat view model with the active user context.
    /// </summary>
    public MeetingChatViewModel(User user)
    {
        Title = "Chat";
        CurrentUser = user;
    }

    public string Title { get; }
    public User CurrentUser { get; }
}

