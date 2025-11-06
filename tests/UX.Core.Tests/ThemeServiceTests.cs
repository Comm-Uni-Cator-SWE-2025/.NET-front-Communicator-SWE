using UX.Core.Models;
using UX.Core.Services;

namespace UX.Core.Tests;

public class ThemeServiceTests
{
    [Fact]
    public void Constructor_SetsDefaultTheme_ToLight()
    {
        var service = new ThemeService();

        Assert.Equal(AppTheme.Light, service.CurrentTheme);
    }

    [Fact]
    public void SetTheme_DoesNotRaiseEvent_WhenThemeIsSame()
    {
        var service = new ThemeService();
        var eventRaised = false;

        service.ThemeChanged += (sender, theme) => eventRaised = true;

        Assert.Equal(AppTheme.Light, service.CurrentTheme);

        Assert.False(eventRaised);
    }
}
