using Controller;
using GUI.Core;

namespace GUI.ViewModels.Meeting
{
    public class MeetingSessionViewModel : ObservableObject
    {
        public MeetingSessionViewModel(UserProfile user)
        {
            Title = "Meeting";
            CurrentUser = user;
        }

        public string Title { get; }
        public UserProfile CurrentUser { get; }
    }
}
