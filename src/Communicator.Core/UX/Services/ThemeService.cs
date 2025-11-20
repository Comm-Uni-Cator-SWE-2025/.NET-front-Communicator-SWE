/*
 * -----------------------------------------------------------------------------
 *  File: ThemeService.cs
 *  Owner: UpdateNamesForEachModule
 *  Roll Number :
 *  Module : 
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.IO;
using System.Linq;
using System.Windows;
using Communicator.Core.UX.Models;

namespace Communicator.Core.UX.Services;

/// <summary>
/// Implementation of IThemeService managing dynamic theme switching at runtime using WPF ResourceDictionaries.
/// </summary>
public class ThemeService : IThemeService
{
    private const string ThemePreferenceKey = "AppTheme";
    private const string SettingsFileName = "settings.txt";
    private static readonly char[] s_lineSeparators = ['\n', '\r'];
    private AppTheme _currentTheme;

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

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

        var themeUri = new Uri($"/Communicator.Core;component/UX/Themes/{theme}Theme.xaml", UriKind.Relative);

        try
        {
            var newTheme = new ResourceDictionary { Source = themeUri };

            ResourceDictionary? existingTheme = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null &&
                                    (d.Source.OriginalString.Contains("LightTheme.xaml", StringComparison.OrdinalIgnoreCase) ||
                                     d.Source.OriginalString.Contains("DarkTheme.xaml", StringComparison.OrdinalIgnoreCase)));

            if (existingTheme != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(existingTheme);
            }

            Application.Current.Resources.MergedDictionaries.Add(newTheme);

            SaveThemePreference();

            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(theme));
        }
        catch (IOException ioEx)
        {
            Console.Error.WriteLine($"Failed to load theme due to IO error: {ioEx.Message}");
        }
        catch (UnauthorizedAccessException uaEx)
        {
            Console.Error.WriteLine($"Failed to load theme due to access denied: {uaEx.Message}");
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
                string[] lines = content.Split(s_lineSeparators, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    if (line.StartsWith($"{ThemePreferenceKey}=", StringComparison.Ordinal))
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
        catch (IOException ioEx)
        {
            Console.Error.WriteLine($"Failed to load theme preference due to IO error: {ioEx.Message}");
        }
        catch (UnauthorizedAccessException uaEx)
        {
            Console.Error.WriteLine($"Failed to load theme preference due to access denied: {uaEx.Message}");
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
        catch (IOException ioEx)
        {
            Console.Error.WriteLine($"Failed to save theme preference due to IO error: {ioEx.Message}");
        }
        catch (UnauthorizedAccessException uaEx)
        {
            Console.Error.WriteLine($"Failed to save theme preference due to access denied: {uaEx.Message}");
        }
    }

    private static string GetSettingsFilePath()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        // Use the actual application name instead of library name
        string appFolder = Path.Combine(appDataPath, "Comm-Uni-Cate");
        return Path.Combine(appFolder, SettingsFileName);
    }
}

