using System;
using System.Globalization;
using System.Windows.Input;
using Controller;
using GUI.ViewModels.Meeting;
using Communicator.Core.UX;
using Communicator.Core.UX.Services;

namespace GUI.ViewModels.Home;

/// <summary>
/// Provides welcome content and meeting shortcuts displayed after authentication.
/// </summary>
public class HomePageViewModel : ObservableObject
{
    private readonly User _user;
    private readonly IToastService _toastService;
    private readonly INavigationService _navigationService;
    private readonly Func<User, MeetingShellViewModel> _meetingShellViewModelFactory;

    public static string CurrentTime => DateTime.Now.ToString("dddd, MMMM dd, yyyy", CultureInfo.CurrentCulture);
    public string WelcomeMessage => _user.DisplayName;
    public static string SubHeading => "Ready to connect and collaborate? Join an existing meeting or create a new one to get started.";

    private string _meetingLink;
    public string MeetingLink
    {
        get => _meetingLink;
        set {
            _meetingLink = value;
            OnPropertyChanged();
        }
    }

    public ICommand JoinMeetingCommand { get; }
    public ICommand CreateMeetingCommand { get; }
    public ICommand OpenMeetingCommand { get; }

    /// <summary>
    /// Initializes the home page with the authenticated user's profile and commands.
    /// Uses injected factory to create MeetingShellViewModel.
    /// </summary>
    public HomePageViewModel(
        User user,
        IToastService toastService,
        INavigationService navigationService,
        Func<User, MeetingShellViewModel> meetingShellViewModelFactory)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _meetingShellViewModelFactory = meetingShellViewModelFactory ?? throw new ArgumentNullException(nameof(meetingShellViewModelFactory));

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
            _toastService.ShowWarning("Please enter a meeting link to join");
            return;
        }

        _toastService.ShowInfo("Join meeting functionality will be implemented soon");
    }

    /// <summary>
    /// Ensures a meeting link was supplied before enabling the join command.
    /// </summary>
    private bool CanJoinMeeting(object? obj)
    {
        return !string.IsNullOrWhiteSpace(MeetingLink);
    }

    /// <summary>
    /// Placeholder for meeting creation.
    /// No role restrictions - all users can create meetings.
    /// </summary>
    private void CreateMeeting(object? obj)
    {
        // TODO: Implement Create Meeting logic
        _toastService.ShowInfo("Create meeting functionality will be implemented soon");
    }

    /// <summary>
    /// All users can create meetings - always enabled.
    /// </summary>
    private bool CanCreateMeeting(object? obj)
    {
        return true;
    }

    /// <summary>
    /// Initiates the meeting workspace by navigating to the meeting shell.
    /// Uses injected factory to create MeetingShellViewModel with all dependencies.
    /// </summary>
    private void OpenMeeting(object? obj)
    {
        _navigationService.NavigateTo(_meetingShellViewModelFactory(_user));
    }
}

