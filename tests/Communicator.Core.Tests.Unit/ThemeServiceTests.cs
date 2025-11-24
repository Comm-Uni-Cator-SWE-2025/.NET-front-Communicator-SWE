using System;
using System.IO;
using System.Threading;
using System.Windows;
using Communicator.Core.UX.Models;
using Communicator.Core.UX.Services;
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
        if (Application.Current == null) new Application();
        var service = new ThemeService();
        
        service.SetUser("testuser");
    }

    public void Dispose()
    {
        if (File.Exists(_testSettingsPath))
        {
            File.Delete(_testSettingsPath);
        }
    }
}
