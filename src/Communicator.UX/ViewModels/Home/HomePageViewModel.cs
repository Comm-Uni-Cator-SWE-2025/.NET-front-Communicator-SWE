using System;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Communicator.Controller.Meeting;
using Communicator.Core.RPC;
using Communicator.Core.UX;
using Communicator.Core.UX.Services;
using Communicator.UX.ViewModels.Meeting;

namespace Communicator.UX.ViewModels.Home;

/// <summary>
/// Provides welcome content and meeting shortcuts displayed after authentication.
/// </summary>
public class HomePageViewModel : ObservableObject
{
    private readonly UserProfile _user;
    private readonly IToastService _toastService;
    private readonly INavigationService _navigationService;
    private readonly IRPC _rpc;
    private readonly ViewModels.Common.LoadingViewModel _loadingViewModel;
    private readonly Func<UserProfile, MeetingSession?, MeetingSessionViewModel> _meetingSessionViewModelFactory;

    public static string CurrentTime => DateTime.Now.ToString("dddd, MMMM dd, yyyy", CultureInfo.CurrentCulture);
    public string WelcomeMessage => _user.DisplayName ?? "User";
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

    /// <summary>
    /// Initializes the home page with the authenticated user's profile and commands.
    /// Uses injected factory to create MeetingSessionViewModel.
    /// </summary>
    public HomePageViewModel(
        UserProfile user,
        IToastService toastService,
        INavigationService navigationService,
        IRPC rpc,
        ViewModels.Common.LoadingViewModel loadingViewModel,
        Func<UserProfile, MeetingSession?, MeetingSessionViewModel> meetingSessionViewModelFactory)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        _loadingViewModel = loadingViewModel ?? throw new ArgumentNullException(nameof(loadingViewModel));
        _meetingSessionViewModelFactory = meetingSessionViewModelFactory ?? throw new ArgumentNullException(nameof(meetingSessionViewModelFactory));

        _meetingLink = string.Empty;
        JoinMeetingCommand = new RelayCommand(JoinMeeting, CanJoinMeeting);
        CreateMeetingCommand = new RelayCommand(CreateMeeting, CanCreateMeeting);
    }

    /// <summary>
    /// Joins a meeting using the provided Meeting ID.
    /// </summary>
    private async void JoinMeeting(object? obj)
    {
        if (string.IsNullOrWhiteSpace(MeetingLink))
        {
            _toastService.ShowWarning("Please enter a meeting ID to join");
            return;
        }

        string meetingId = MeetingLink.Trim();

        try
        {
            _loadingViewModel.Message = "Joining meeting...";
            _loadingViewModel.IsBusy = true;

            // 1. Create a local session object with this ID
            // We don't have the full session details yet, but we can start with basic info.
            // The backend will sync participants later via 'subscribeAsViewer' or other events.
            var session = new MeetingSession(
                meetingId: meetingId,
                createdBy: "unknown", // We don't know who created it yet
                createdAt: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                sessionMode: SessionMode.CLASS
            );

            // 2. Create ViewModel (subscribes to events)
            // IMPORTANT: We must create the ViewModel (which subscribes to events) BEFORE calling joinMeeting
            // to ensure we don't miss any initial participant events from the backend.
            MeetingSessionViewModel meetingVm = _meetingSessionViewModelFactory(_user, session);
            
            // 3. Serialize meeting ID to JSON string, then to bytes
            // Java backend expects: DataSerializer.deserialize(meetId, String.class)
            string jsonId = JsonSerializer.Serialize(meetingId);
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(jsonId);

            // 4. Call RPC to join meeting
            // Backend returns the meeting ID if successful
            await _rpc.Call("core/joinMeeting", payload);
            
            // 5. Navigate to meeting session
            _navigationService.NavigateTo(meetingVm);
            
            _toastService.ShowSuccess($"Joined meeting {meetingId}");
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"Error joining meeting: {ex.Message}");
            // If join failed, we should probably leave the meeting view
            // But for now, user can manually leave
        }
        finally
        {
            _loadingViewModel.IsBusy = false;
        }
    }

    /// <summary>
    /// Always enable the join command. Validation happens in the execution.
    /// </summary>
    private bool CanJoinMeeting(object? obj)
    {
        return true;
    }

    /// <summary>
    /// Creates a new meeting via RPC.
    /// </summary>
    private async void CreateMeeting(object? obj)
    {
        try
        {
            _loadingViewModel.Message = "Creating meeting...";
            _loadingViewModel.IsBusy = true;

            // 1. Call RPC to create meeting
            // The backend expects a byte[] for meetMode, but ignores it. We can send empty.
            byte[] response = await _rpc.Call("core/createMeeting", Array.Empty<byte>());

            // 2. Deserialize response to MeetingSession
            string json = System.Text.Encoding.UTF8.GetString(response);
            
            // Use case-insensitive options as Java might use camelCase while C# expects PascalCase (or vice versa depending on config)
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            MeetingSession? session = JsonSerializer.Deserialize<MeetingSession>(json, options);

            if (session != null)
            {
                // 3. Navigate to meeting session
                _navigationService.NavigateTo(_meetingSessionViewModelFactory(_user, session));
                _toastService.ShowSuccess($"Created meeting {session.MeetingId}");
            }
            else
            {
                _toastService.ShowError("Failed to create meeting: Invalid response from server");
            }
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"Error creating meeting: {ex.Message}");
        }
        finally
        {
            _loadingViewModel.IsBusy = false;
        }
    }

    /// <summary>
    /// All users can create meetings - always enabled.
    /// </summary>
    private bool CanCreateMeeting(object? obj)
    {
        return true;
    }
}

