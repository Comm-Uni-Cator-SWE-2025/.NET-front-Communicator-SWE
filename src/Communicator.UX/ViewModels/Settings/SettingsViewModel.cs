using System.Globalization;
using System.Windows.Input;
using Communicator.Controller.Meeting;
using Communicator.Core.UX;
using Communicator.Core.UX.Models;
using Communicator.Core.UX.Services;

namespace Communicator.UX.ViewModels.Settings;

/// <summary>
/// ViewModel for the Settings Page
/// Manages user preferences including theme selection
/// </summary>
public class SettingsViewModel : ObservableObject
{
    private readonly UserProfile _user;
    private readonly IThemeService _themeService;

    // User Information
    public string DisplayName => _user.DisplayName ?? "User";
    public string Email => _user.Email ?? string.Empty;

    // Theme Settings
    private bool _isDarkMode;
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set {
            if (_isDarkMode != value)
            {
                _isDarkMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentThemeText));

                // Apply theme change
                _themeService.SetTheme(value ? AppTheme.Dark : AppTheme.Light);
            }
        }
    }

    public string CurrentThemeText => _isDarkMode ? "Dark" : "Light";

    public SettingsViewModel(UserProfile user, IThemeService themeService)
    {
        _user = user;
        _themeService = themeService;

        // Initialize theme toggle based on current theme
        _isDarkMode = _themeService.CurrentTheme == AppTheme.Dark;
    }
}

