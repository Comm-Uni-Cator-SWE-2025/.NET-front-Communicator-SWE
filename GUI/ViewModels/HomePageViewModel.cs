using System;
using Controller;
using GUI.Core;
using System.Windows.Input;

namespace GUI.ViewModels
{
    public class HomePageViewModel : ObservableObject
    {
        private readonly UserProfile _user;

        public string CurrentTime => DateTime.Now.ToString("dddd, MMMM dd, yyyy");
        public string WelcomeMessage => _user.DisplayName;
        public string SubHeading => "Ready to connect and collaborate? Join an existing meeting or create a new one to get started.";

        private string _meetingLink;
        public string MeetingLink
        {
            get { return _meetingLink; }
            set
            {
                _meetingLink = value;
                OnPropertyChanged();
            }
        }

        public ICommand JoinMeetingCommand { get; }
        public ICommand CreateMeetingCommand { get; }

        public HomePageViewModel(UserProfile user)
        {
            _user = user;
            _meetingLink = string.Empty;
            JoinMeetingCommand = new RelayCommand(JoinMeeting, CanJoinMeeting);
            CreateMeetingCommand = new RelayCommand(CreateMeeting, CanCreateMeeting);
        }

        private void JoinMeeting(object? obj)
        {
            if (string.IsNullOrWhiteSpace(MeetingLink))
            {
                App.ToastService.ShowWarning("Please enter a meeting link to join");
                return;
            }

            // TODO: Implement Join Meeting logic
            App.ToastService.ShowInfo("Join meeting functionality will be implemented soon");
        }

        private bool CanJoinMeeting(object? obj)
        {
            return !string.IsNullOrWhiteSpace(MeetingLink);
        }

        private void CreateMeeting(object? obj)
        {
            // TODO: Implement Create Meeting logic
            if (string.Equals(_user.Role, "lecturer", StringComparison.OrdinalIgnoreCase))
            {
                App.ToastService.ShowInfo("Create meeting functionality will be implemented soon");
            }
            else
            {
                App.ToastService.ShowWarning("Only lecturers can create meetings");
            }
        }

        private bool CanCreateMeeting(object? obj)
        {
            return string.Equals(_user.Role, "lecturer", StringComparison.OrdinalIgnoreCase);
        }
    }
}
