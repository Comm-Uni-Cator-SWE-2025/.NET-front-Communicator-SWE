using Controller;
using GUI.Core;

namespace GUI.ViewModels.Meeting
{
    public class WhiteboardViewModel : ObservableObject
    {
        public WhiteboardViewModel(UserProfile user)
        {
            Title = "Whiteboard";
            CurrentUser = user;
        }

        public string Title { get; }
        public UserProfile CurrentUser { get; }
    }
}
