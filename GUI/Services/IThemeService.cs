using System;
using GUI.Models;

namespace GUI.Services
{
    /// <summary>
    /// Service interface for managing application themes
    /// Follows the Service pattern for theme management
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Event raised when the theme changes
        /// </summary>
        event EventHandler<AppTheme>? ThemeChanged;

        /// <summary>
        /// Gets the current active theme
        /// </summary>
        AppTheme CurrentTheme { get; }

        /// <summary>
        /// Sets the application theme
        /// </summary>
        /// <param name="theme">The theme to apply</param>
        void SetTheme(AppTheme theme);

        /// <summary>
        /// Loads the saved theme preference from user settings
        /// </summary>
        void LoadSavedTheme();

        /// <summary>
        /// Saves the current theme preference to user settings
        /// </summary>
        void SaveThemePreference();
    }
}
