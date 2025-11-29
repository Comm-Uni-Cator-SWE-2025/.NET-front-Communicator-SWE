/*
 * -----------------------------------------------------------------------------
 *  File: SettingsViewModel.cs
 *  Owner: Geetheswar V
 *  Roll Number : 142201025
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System.Globalization;
using System.Windows.Input;
using System.Threading.Tasks;
using Communicator.Controller.Meeting;
using Communicator.UX.Core;
using Communicator.UX.Core.Models;
using Communicator.UX.Core.Services;
using Communicator.App.Services;

namespace Communicator.App.ViewModels.Settings;

/// <summary>
/// ViewModel for the Settings Page
/// Manages user preferences including theme selection
/// </summary>
public sealed class SettingsViewModel : ObservableObject
{
    private readonly UserProfile _user;
    private readonly IThemeService _themeService;
    private readonly IAuthenticationService _authenticationService;

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

    public ICommand LogoutCommand { get; }

    public SettingsViewModel(UserProfile user, IThemeService themeService, IAuthenticationService authenticationService)
    {
        _user = user;
        _themeService = themeService;
        _authenticationService = authenticationService;

        // Initialize theme toggle based on current theme
        _isDarkMode = _themeService.CurrentTheme == AppTheme.Dark;

        LogoutCommand = new RelayCommand(async _ => await LogoutAsync().ConfigureAwait(true));
    }

    private async Task LogoutAsync()
    {
        await _authenticationService.LogoutAsync().ConfigureAwait(true);
    }
}



