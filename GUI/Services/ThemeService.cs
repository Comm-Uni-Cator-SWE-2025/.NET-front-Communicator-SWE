using System;
using System.IO;
using System.Linq;
using System.Windows;
using GUI.Models;

namespace GUI.Services
{
    /// <summary>
    /// Implementation of IThemeService following Singleton pattern
    /// Manages dynamic theme switching at runtime using WPF ResourceDictionaries
    /// </summary>
    public class ThemeService : IThemeService
    {
        private const string ThemePreferenceKey = "AppTheme";
        private const string SettingsFileName = "settings.txt";
        private AppTheme _currentTheme;

        public event EventHandler<AppTheme>? ThemeChanged;

        public AppTheme CurrentTheme => _currentTheme;

        public ThemeService()
        {
            // Default to Light theme
            _currentTheme = AppTheme.Light;
        }

        /// <summary>
        /// Sets the application theme by dynamically loading the appropriate ResourceDictionary
        /// This follows WPF best practices for runtime theme switching
        /// </summary>
        public void SetTheme(AppTheme theme)
        {
            if (_currentTheme == theme)
                return;

            _currentTheme = theme;

            // Get the theme resource dictionary URI
            var themeUri = new Uri($"/GUI;component/Themes/{theme}Theme.xaml", UriKind.Relative);

            try
            {
                // Load the new theme ResourceDictionary
                var newTheme = new ResourceDictionary { Source = themeUri };

                // Find and remove existing theme dictionaries
                var existingTheme = Application.Current.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Source != null && 
                                        (d.Source.OriginalString.Contains("LightTheme.xaml") || 
                                         d.Source.OriginalString.Contains("DarkTheme.xaml")));

                if (existingTheme != null)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(existingTheme);
                }

                // Add the new theme dictionary
                Application.Current.Resources.MergedDictionaries.Add(newTheme);

                // Save preference
                SaveThemePreference();

                // Notify listeners of theme change
                ThemeChanged?.Invoke(this, theme);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to load theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the saved theme preference from a simple text file
        /// Uses file-based storage as a lightweight alternative to Settings.settings
        /// </summary>
        public void LoadSavedTheme()
        {
            try
            {
                var settingsPath = GetSettingsFilePath();
                
                if (File.Exists(settingsPath))
                {
                    var content = File.ReadAllText(settingsPath);
                    var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var line in lines)
                    {
                        if (line.StartsWith($"{ThemePreferenceKey}="))
                        {
                            var value = line.Substring(ThemePreferenceKey.Length + 1);
                            if (Enum.TryParse<AppTheme>(value, out var savedTheme))
                            {
                                SetTheme(savedTheme);
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to load theme preference: {ex.Message}");
            }

            // Default to Light theme if no preference found
            SetTheme(AppTheme.Light);
        }

        /// <summary>
        /// Saves the current theme preference to a file
        /// </summary>
        public void SaveThemePreference()
        {
            try
            {
                var settingsPath = GetSettingsFilePath();
                var directory = Path.GetDirectoryName(settingsPath);
                
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(settingsPath, $"{ThemePreferenceKey}={_currentTheme}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to save theme preference: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the path to the settings file in user's AppData folder
        /// </summary>
        private string GetSettingsFilePath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "GUI");
            return Path.Combine(appFolder, SettingsFileName);
        }
    }
}
