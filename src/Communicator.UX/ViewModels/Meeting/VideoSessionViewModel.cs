using Controller;
using Communicator.Core.UX;

namespace Communicator.UX.ViewModels.Meeting;

/// <summary>
/// Represents the primary meeting session surface, exposing the current user context.
/// </summary>
public class VideoSessionViewModel : ObservableObject
{
    /// <summary>
    /// Initializes the session view model with the supplied user context.
    /// </summary>
    public VideoSessionViewModel(User user)
    {
        Title = "Meeting";
        CurrentUser = user;
    }

    public string Title { get; }
    public User CurrentUser { get; }
}

