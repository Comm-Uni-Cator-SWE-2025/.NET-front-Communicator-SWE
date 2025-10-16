using System;
using Controller;
using UX.Core;
using System.Windows.Input;
using GUI.ViewModels.Meeting;

namespace GUI.ViewModels.Home
{
    /// <summary>
    /// Provides welcome content and meeting shortcuts displayed after authentication.
    /// </summary>
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
        public ICommand OpenMeetingCommand { get; }

        /// <summary>
        /// Initializes the home page with the authenticated user's profile and commands.
        /// </summary>
        public HomePageViewModel(UserProfile user)
        {
            _user = user;
            _meetingLink = string.Empty;
            JoinMeetingCommand = new RelayCommand(JoinMeeting, CanJoinMeeting);
            CreateMeetingCommand = new RelayCommand(CreateMeeting, CanCreateMeeting);
            OpenMeetingCommand = new RelayCommand(OpenMeeting);
        }

        /// <summary>
        /// Placeholder for joining a meeting via link with basic input validation.
        /// </summary>
        private void JoinMeeting(object? obj)
        {
            if (string.IsNullOrWhiteSpace(MeetingLink))
            {
                App.ToastService.ShowWarning("Please enter a meeting link to join");
                return;
            }

            App.ToastService.ShowInfo("Join meeting functionality will be implemented soon");
        }

        /// <summary>
        /// Ensures a meeting link was supplied before enabling the join command.
        /// </summary>
        private bool CanJoinMeeting(object? obj)
        {
            return !string.IsNullOrWhiteSpace(MeetingLink);
        }

        /// <summary>
        /// Placeholder for meeting creation with role-based restriction messaging.
        /// </summary>
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

        /// <summary>
        /// Lecturer-only guard for the create meeting command.
        /// </summary>
        private bool CanCreateMeeting(object? obj)
        {
            return string.Equals(_user.Role, "lecturer", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Initiates the meeting workspace by navigating to the meeting shell.
        /// </summary>
        private void OpenMeeting(object? obj)
        {
            App.NavigationService.NavigateTo(new MeetingShellViewModel(_user));
        }
    }
}

