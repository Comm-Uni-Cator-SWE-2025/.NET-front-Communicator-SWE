using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows;
using Communicator.UX.Core.Models;
using Communicator.UX.Core.Services;
using Xunit;

namespace Communicator.Core.Tests.Unit;

public class ThemeServiceTests : IDisposable
{
    private readonly string _testSettingsPath;

    public ThemeServiceTests()
    {
        // Setup a test path for settings to avoid messing with real app data
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string appFolder = Path.Combine(appDataPath, "Comm-Uni-Cate");
        _testSettingsPath = Path.Combine(appFolder, "settings.txt");

        // Ensure clean state
        if (File.Exists(_testSettingsPath))
        {
            File.Delete(_testSettingsPath);
        }
    }

    [Fact]
    public void Constructor_SetsDefaultThemeToLight()
    {
        if (Application.Current == null)
        {
            new Application();
        }

        var service = new ThemeService();
        Assert.Equal(AppTheme.Light, service.CurrentTheme);
    }

    [Fact]
    public void SetUser_UpdatesUsername()
    {
        if (Application.Current == null)
        {
            _ = new Application();
        }

        var service = new ThemeService();

        service.SetUser("testuser");
    }

    #region TryParseThemeFromResponse Tests

    [Fact]
    public void TryParseThemeFromResponse_WithUndefinedData_ReturnsEmpty()
    {
        JsonElement undefinedData = default;
        string result = ThemeService.TryParseThemeFromResponse(undefinedData);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void TryParseThemeFromResponse_WithNullData_ReturnsEmpty()
    {
        JsonElement nullData = JsonDocument.Parse("null").RootElement;
        string result = ThemeService.TryParseThemeFromResponse(nullData);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void TryParseThemeFromResponse_WithObjectContainingColorProperty_ReturnsTheme()
    {
        JsonElement data = JsonDocument.Parse("{\"color\": \"dark\"}").RootElement;
        string result = ThemeService.TryParseThemeFromResponse(data);
        Assert.Equal("dark", result);
    }

    [Fact]
    public void TryParseThemeFromResponse_WithObjectMissingColorProperty_ReturnsEmpty()
    {
        JsonElement data = JsonDocument.Parse("{\"other\": \"value\"}").RootElement;
        string result = ThemeService.TryParseThemeFromResponse(data);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void TryParseThemeFromResponse_WithStringValue_ReturnsString()
    {
        JsonElement data = JsonDocument.Parse("\"light\"").RootElement;
        string result = ThemeService.TryParseThemeFromResponse(data);
        Assert.Equal("light", result);
    }

    [Fact]
    public void TryParseThemeFromResponse_WithEmptyObject_ReturnsEmpty()
    {
        JsonElement data = JsonDocument.Parse("{}").RootElement;
        string result = ThemeService.TryParseThemeFromResponse(data);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void TryParseThemeFromResponse_WithNullColorValue_ReturnsEmpty()
    {
        JsonElement data = JsonDocument.Parse("{\"color\": null}").RootElement;
        string result = ThemeService.TryParseThemeFromResponse(data);
        Assert.Equal(string.Empty, result);
    }

    #endregion

    public void Dispose()
    {
        if (File.Exists(_testSettingsPath))
        {
            File.Delete(_testSettingsPath);
        }
    }
}
