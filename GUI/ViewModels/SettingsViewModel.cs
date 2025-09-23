using System.Windows.Input;
using Controller;
using GUI.Core;
using GUI.Models;
using GUI.Services;

namespace GUI.ViewModels
{
    /// <summary>
    /// ViewModel for the Settings Page
    /// Manages user preferences including theme selection
    /// Follows MVVM pattern with INotifyPropertyChanged
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
            get { return _isDarkMode; }
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CurrentThemeText));
                    
                    // Apply theme change
                    _themeService.SetTheme(value ? AppTheme.Dark : AppTheme.Light);
                    App.ToastService.ShowSuccess($"{CurrentThemeText} theme applied successfully");
                }
            }
        }

        public string CurrentThemeText => _isDarkMode ? "Dark" : "Light";

        // Commands
        public ICommand BackCommand { get; }

        public SettingsViewModel(UserProfile user, IThemeService themeService, ICommand backCommand)
        {
            _user = user;
            _themeService = themeService;
            BackCommand = backCommand;

            // Initialize theme toggle based on current theme
            _isDarkMode = _themeService.CurrentTheme == AppTheme.Dark;
        }

        private string FormatRole(string role)
        {
            if (string.IsNullOrEmpty(role))
                return "User";

            // Capitalize first letter
            return char.ToUpper(role[0]) + role.Substring(1).ToLower();
        }
    }
}
