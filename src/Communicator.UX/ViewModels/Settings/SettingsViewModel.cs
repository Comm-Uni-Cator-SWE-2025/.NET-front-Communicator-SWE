using System.Globalization;
using System.Windows.Input;
using Controller;
using UX.Core;
using UX.Core.Models;
using UX.Core.Services;

namespace GUI.ViewModels.Settings;

/// <summary>
/// ViewModel for the Settings Page
/// Manages user preferences including theme selection
/// </summary>
public class SettingsViewModel : ObservableObject
{
    private readonly UserProfile _user;
    private readonly IThemeService _themeService;

    // User Information
    public string DisplayName => _user.DisplayName;
    public string Email => _user.Email;
    public string Role => FormatRole(_user.Role);

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

    /// <summary>
    /// Normalizes the role string for display purposes.
    /// </summary>
    private static string FormatRole(string role)
    {
        if (string.IsNullOrEmpty(role))
        {
            return "User";
        }

        // Capitalize first letter - only first character is uppercased, rest stays as-is
        return char.ToUpper(role[0], CultureInfo.InvariantCulture) + role.Substring(1);
    }
}

