using Controller;
using GUI.Core;

namespace GUI.ViewModels.Meeting
{
    public class MeetingDashboardViewModel : ObservableObject
    {
        public MeetingDashboardViewModel(UserProfile user)
        {
            Title = "Dashboard";
            CurrentUser = user;
        }

        public string Title { get; }
        public UserProfile CurrentUser { get; }
    }
}
