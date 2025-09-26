using Controller;
using GUI.Core;

namespace GUI.ViewModels
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
