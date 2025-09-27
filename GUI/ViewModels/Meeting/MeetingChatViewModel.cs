using Controller;
using GUI.Core;

namespace GUI.ViewModels
{
    public class MeetingChatViewModel : ObservableObject
    {
        public MeetingChatViewModel(UserProfile user)
        {
            Title = "Chat";
            CurrentUser = user;
        }

        public string Title { get; }
        public UserProfile CurrentUser { get; }
    }
}
