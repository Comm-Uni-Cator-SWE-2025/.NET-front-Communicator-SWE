using System;
using System.IO;
using System.Linq;
using Communicator.App.Tests.UI.Pages;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using Xunit;

namespace Communicator.App.Tests.UI;

public class MainWindowUiTests : IDisposable
{
    private readonly Application _app;
    private readonly UIA3Automation _automation;
    private readonly Window _window;

    public MainWindowUiTests()
    {
        var exe = Path.GetFullPath(Path.Combine("..", "..", "..", "..", "..", "src", "Communicator.App", "bin", "Debug", "net8.0-windows", "Communicator.App.exe"));
        
        // Ensure the app is built before running tests
        if (!File.Exists(exe))
        {
            throw new FileNotFoundException("Communicator.App.exe not found. Please build the project first.", exe);
        }

        // Launch with --test-mode to skip RPC connection and loading screen
        _app = Application.Launch(exe, "--test-mode");
        _automation = new UIA3Automation();
        
        // Wait for the main window with title "Comm-Uni-Cate" to appear
        // The app might show a loading window first, so we wait for the real main window
        _window = Retry.WhileNull(() =>
        {
            var allWindows = _app.GetAllTopLevelWindows(_automation);
            foreach (var w in allWindows)
            {
                // System.Diagnostics.Debug.WriteLine($"[UI Test] Found Window: '{w.Title}' ({w.AutomationId})");
            }
            return allWindows.FirstOrDefault(w => w.Title == "Comm-Uni-Cate");
        }, TimeSpan.FromSeconds(15), TimeSpan.FromMilliseconds(500)).Result;

        if (_window == null)
        {
            var allWindows = _app.GetAllTopLevelWindows(_automation);
            var windowNames = string.Join(", ", allWindows.Select(w => $"'{w.Title}'"));
            throw new Exception($"Main window 'Comm-Uni-Cate' not found within timeout. Found windows: {windowNames}");
        }
        
        // Wait for window to be visible/interactive
        _window.WaitUntilClickable(TimeSpan.FromSeconds(5));
    }

    public void Dispose()
    {
        _automation?.Dispose();
        _app?.Close();
        _app?.Dispose();
    }

    [Fact]
    public void LaunchApp_ShowsHomePage_InTestMode()
    {
        var homePage = new HomePage(_window);
        Assert.True(homePage.IsVisible(), "Home page should be visible on launch in test mode");
        Assert.NotNull(homePage.JoinMeetingButton);
    }

    [Fact]
    public void JoinMeeting_NavigatesToMeetingPage()
    {
        var homePage = new HomePage(_window);
        
        // Join a meeting
        var meetingPage = homePage.JoinMeeting("test-meeting-id");

        Assert.True(meetingPage.IsVisible(), "Meeting page should be visible after joining");
        Assert.NotNull(meetingPage.LeaveMeetingButton);
        Assert.NotNull(meetingPage.ToggleMuteButton);
    }

    [Fact]
    public void MeetingControls_ToggleState()
    {
        var homePage = new HomePage(_window);
        var meetingPage = homePage.JoinMeeting("test-meeting-controls");

        // Verify controls exist
        Assert.NotNull(meetingPage.ToggleMuteButton);
        Assert.NotNull(meetingPage.ToggleCameraButton);
        Assert.NotNull(meetingPage.RaiseHandButton);

        // Click buttons (just verifying no crash and interaction works)
        meetingPage.ToggleMute();
        meetingPage.ToggleCamera();
        
        // Leave meeting
        meetingPage.LeaveMeeting();
    }
}
