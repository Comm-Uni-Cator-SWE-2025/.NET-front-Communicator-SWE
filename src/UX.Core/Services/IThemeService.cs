using System;
using UX.Core.Models;

namespace UX.Core.Services;

/// <summary>
/// Service interface for managing application themes.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Event raised when the theme changes.
    /// </summary>
    event EventHandler<AppTheme>? ThemeChanged;

    /// <summary>
    /// Gets the current active theme.
    /// </summary>
    AppTheme CurrentTheme { get; }

    /// <summary>
    /// Sets the application theme.
    /// </summary>
    void SetTheme(AppTheme theme);

    /// <summary>
    /// Loads the saved theme preference from user settings.
    /// </summary>
    void LoadSavedTheme();

    /// <summary>
    /// Saves the current theme preference to user settings.
    /// </summary>
    void SaveThemePreference();
}
