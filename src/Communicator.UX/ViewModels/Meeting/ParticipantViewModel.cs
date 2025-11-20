/*
 * -----------------------------------------------------------------------------
 *  File: ParticipantViewModel.cs
 *  Owner: UpdateNamesForEachModule
 *  Roll Number :
 *  Module : 
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.Windows.Media.Imaging;
using Communicator.Controller.Meeting;
using Communicator.Core.UX;

namespace Communicator.UX.ViewModels.Meeting;

/// <summary>
/// Represents a participant in the meeting with UI-specific state.
/// Wraps UserProfile with bindable properties for video frames, mute state, etc.
/// </summary>
public sealed class ParticipantViewModel : ObservableObject
{
    private BitmapSource? _videoFrame;
    private BitmapSource? _screenFrame;
    private bool _isMuted;
    private bool _isCameraOn;
    private bool _isScreenSharing;
    private bool _isHandRaised;

    /// <summary>
    /// Initializes a new participant view model with the given user profile.
    /// </summary>
    public ParticipantViewModel(UserProfile user)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
    }

    /// <summary>
    /// The underlying user profile data.
    /// </summary>
    public UserProfile User { get; }

    /// <summary>
    /// Current video frame from this participant's camera.
    /// </summary>
    public BitmapSource? VideoFrame
    {
        get => _videoFrame;
        set {
            if (SetProperty(ref _videoFrame, value))
            {
                OnPropertyChanged(nameof(HasVideoFrame));
            }
        }
    }

    /// <summary>
    /// Current screen share frame from this participant.
    /// </summary>
    public BitmapSource? ScreenFrame
    {
        get => _screenFrame;
        set {
            if (SetProperty(ref _screenFrame, value))
            {
                OnPropertyChanged(nameof(HasScreenFrame));
            }
        }
    }

    /// <summary>
    /// Whether this participant is muted.
    /// </summary>
    public bool IsMuted
    {
        get => _isMuted;
        set => SetProperty(ref _isMuted, value);
    }

    /// <summary>
    /// Whether this participant's camera is on.
    /// </summary>
    public bool IsCameraOn
    {
        get => _isCameraOn;
        set => SetProperty(ref _isCameraOn, value);
    }

    /// <summary>
    /// Whether this participant is sharing their screen.
    /// </summary>
    public bool IsScreenSharing
    {
        get => _isScreenSharing;
        set => SetProperty(ref _isScreenSharing, value);
    }

    /// <summary>
    /// Whether this participant has raised their hand.
    /// </summary>
    public bool IsHandRaised
    {
        get => _isHandRaised;
        set => SetProperty(ref _isHandRaised, value);
    }

    /// <summary>
    /// Display name for the participant (falls back to email if not set).
    /// </summary>
    public string DisplayName => User.DisplayName ?? User.Email ?? "Unknown";

    /// <summary>
    /// First letter of display name for avatar initial.
    /// </summary>
    public string Initial => string.IsNullOrEmpty(DisplayName) ? "?" : DisplayName.Substring(0, 1).ToUpper(System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// Whether this participant has a video frame to display.
    /// </summary>
    public bool HasVideoFrame => VideoFrame != null;

    /// <summary>
    /// Whether this participant has a screen share frame to display.
    /// </summary>
    public bool HasScreenFrame => ScreenFrame != null;

    /// <summary>
    /// Profile image URI (from user profile or null).
    /// </summary>
    public Uri? ProfileImageUri => User.LogoUrl;
}


