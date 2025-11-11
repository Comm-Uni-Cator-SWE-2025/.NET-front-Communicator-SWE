using System;
using System.IO;
using System.Linq;
using System.Windows;
using UX.Core.Models;

namespace UX.Core.Services;

/// <summary>
/// Implementation of IThemeService managing dynamic theme switching at runtime using WPF ResourceDictionaries.
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
        _currentTheme = AppTheme.Light;
    }

    /// <summary>
    /// Sets the application theme by dynamically loading the appropriate ResourceDictionary.
    /// </summary>
    public void SetTheme(AppTheme theme)
    {
        if (_currentTheme == theme)
        {
            return;
        }

        _currentTheme = theme;

        var themeUri = new Uri($"/UX.Core;component/Themes/{theme}Theme.xaml", UriKind.Relative);

        try
        {
            var newTheme = new ResourceDictionary { Source = themeUri };

            ResourceDictionary? existingTheme = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null &&
                                    (d.Source.OriginalString.Contains("LightTheme.xaml") ||
                                     d.Source.OriginalString.Contains("DarkTheme.xaml")));

            if (existingTheme != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(existingTheme);
            }

            Application.Current.Resources.MergedDictionaries.Add(newTheme);

            SaveThemePreference();

            ThemeChanged?.Invoke(this, theme);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load theme: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads the saved theme preference from a simple text file.
    /// </summary>
    public void LoadSavedTheme()
    {
        try
        {
            string settingsPath = GetSettingsFilePath();

            if (File.Exists(settingsPath))
            {
                string content = File.ReadAllText(settingsPath);
                string[] lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    if (line.StartsWith($"{ThemePreferenceKey}="))
                    {
                        string value = line.Substring(ThemePreferenceKey.Length + 1);
                        if (Enum.TryParse<AppTheme>(value, out AppTheme savedTheme))
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

        SetTheme(AppTheme.Light);
    }

    /// <summary>
    /// Saves the current theme preference to a file.
    /// </summary>
    public void SaveThemePreference()
    {
        try
        {
            string settingsPath = GetSettingsFilePath();
            string? directory = Path.GetDirectoryName(settingsPath);

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

    private string GetSettingsFilePath()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        // Use the actual application name instead of library name
        string appFolder = Path.Combine(appDataPath, "Comm-Uni-Cate");
        return Path.Combine(appFolder, SettingsFileName);
    }
}
