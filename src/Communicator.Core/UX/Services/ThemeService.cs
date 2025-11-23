/*
 * -----------------------------------------------------------------------------
 *  File: ThemeService.cs
 *  Owner: Geetheswar V
 *  Roll Number : 142201025
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Communicator.Cloud.CloudFunction.DataStructures;
using Communicator.Cloud.CloudFunction.FunctionLibrary;
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
    private string? _currentUsername;
    private CloudFunctionLibrary? _cloudLibrary;
    private const string ThemeContainer = "UX";
    private const string ThemeType = "Theme";
    private const string ThemeKey = "color";

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    public AppTheme CurrentTheme => _currentTheme;

    public ThemeService()
    {
        _currentTheme = AppTheme.Light;
        InitializeCloudLibrary();
    }

    private void InitializeCloudLibrary()
    {
        try
        {
            string? cloudUrl = Environment.GetEnvironmentVariable("CLOUD_BASE_URL");
            System.Diagnostics.Debug.WriteLine($"[ThemeService] Initializing CloudFunctionLibrary. CLOUD_BASE_URL: {cloudUrl}");

            // Only initialize if environment variable is set to avoid crashes
            if (cloudUrl != null)
            {
                _cloudLibrary = new CloudFunctionLibrary();
                System.Diagnostics.Debug.WriteLine("[ThemeService] CloudFunctionLibrary initialized successfully.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ThemeService] CLOUD_BASE_URL is missing. Cloud sync disabled.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ThemeService] Failed to initialize CloudFunctionLibrary: {ex.Message}");
        }
    }

    public void SetUser(string? username)
    {
        System.Diagnostics.Debug.WriteLine($"[ThemeService] SetUser called with: {username}");
        _currentUsername = username;
        if (!string.IsNullOrEmpty(username))
        {
            LoadThemeFromCloud();
        }
    }

    private async void LoadThemeFromCloud()
    {
        if (string.IsNullOrEmpty(_currentUsername))
        {
            System.Diagnostics.Debug.WriteLine("[ThemeService] LoadThemeFromCloud: Username is empty.");
            return;
        }
        if (_cloudLibrary == null)
        {
            System.Diagnostics.Debug.WriteLine("[ThemeService] LoadThemeFromCloud: CloudLibrary is null.");
            return;
        }

        try
        {
            System.Diagnostics.Debug.WriteLine($"[ThemeService] Loading theme from cloud for user: {_currentUsername}");
            // Use JsonDocument.Parse("{}").RootElement instead of default to ensure valid JSON object
            var emptyData = JsonDocument.Parse("{}").RootElement;
            var req = new Entity(ThemeContainer, ThemeType, _currentUsername, ThemeKey, -1, new TimeRange(0, 0), emptyData);
            var res = await _cloudLibrary.CloudGetAsync(req);
            System.Diagnostics.Debug.WriteLine($"[ThemeService] CloudGetAsync response: {res.StatusCode} {res.Message}");

            if (res.Data.ValueKind != JsonValueKind.Undefined && res.Data.ValueKind != JsonValueKind.Null)
            {
                string themeStr = "";
                if (res.Data.ValueKind == JsonValueKind.Object && res.Data.TryGetProperty(ThemeKey, out var val))
                {
                    themeStr = val.GetString() ?? "";
                }
                else if (res.Data.ValueKind == JsonValueKind.String)
                {
                    themeStr = res.Data.GetString() ?? "";
                }

                System.Diagnostics.Debug.WriteLine($"[ThemeService] Theme from cloud: {themeStr}");

                if (!string.IsNullOrEmpty(themeStr))
                {
                    var theme = themeStr.Equals("dark", StringComparison.OrdinalIgnoreCase) ? AppTheme.Dark : AppTheme.Light;
                    Application.Current.Dispatcher.Invoke(() => SetTheme(theme));
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ThemeService] No data in cloud response.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ThemeService] Failed to load theme from cloud: {ex.Message}");
        }
    }

    private async void SaveThemeToCloud()
    {
        if (string.IsNullOrEmpty(_currentUsername)) return;
        if (_cloudLibrary == null) return;

        try
        {
            string themeValue = _currentTheme == AppTheme.Dark ? "dark" : "light";
            System.Diagnostics.Debug.WriteLine($"[ThemeService] Saving theme to cloud: {themeValue} for user: {_currentUsername}");
            var data = new { color = themeValue };
            var jsonData = JsonSerializer.SerializeToElement(data);

            var req = new Entity(ThemeContainer, ThemeType, _currentUsername, ThemeKey, -1, new TimeRange(0, 0), jsonData);
            await _cloudLibrary.CloudPostAsync(req);
            System.Diagnostics.Debug.WriteLine($"[ThemeService] Theme saved to cloud successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ThemeService] Failed to save theme to cloud: {ex.Message}");
        }
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
            SaveThemeToCloud();

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

