using Controller;
using UX.Core;

namespace GUI.ViewModels.Meeting
{
    /// <summary>
    /// Represents the primary meeting session surface, exposing the current user context.
    /// </summary>
    public class VideoSessionViewModel : ObservableObject
    {
        /// <summary>
        /// Initializes the session view model with the supplied user context.
        /// </summary>
        public VideoSessionViewModel(UserProfile user)
        {
            Title = "Meeting";
            CurrentUser = user;
        }

        public string Title { get; }
        public UserProfile CurrentUser { get; }
    }
}

