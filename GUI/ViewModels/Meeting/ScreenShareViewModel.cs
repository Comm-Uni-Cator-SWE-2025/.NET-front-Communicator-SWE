using Controller;
using GUI.Core;

namespace GUI.ViewModels
{
    public class ScreenShareViewModel : ObservableObject
    {
        public ScreenShareViewModel(UserProfile user)
        {
            Title = "ScreenShare";
            CurrentUser = user;
        }

        public string Title { get; }
        public UserProfile CurrentUser { get; }
    }
}
