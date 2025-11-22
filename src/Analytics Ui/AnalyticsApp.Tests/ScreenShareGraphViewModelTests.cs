using AnalyticsApp.ViewModels;
using Xunit;

namespace AnalyticsApp.Tests;

/// <summary>
/// Tests for ScreenShareGraphViewModel (Graph logic only).
/// </summary>
public class ScreenShareGraphViewModelTests
{
    [Fact]
    public void AddPoint_ShouldAddToCollection()
    {
        // Arrange
        var vm = new ScreenShareGraphViewModel();

        // Act
        vm.AddPoint(10, 5);

        // Assert
        Assert.Single(vm.Points);
        Assert.Equal(5, vm.Points[0].Y);
        Assert.Equal(10, vm.Points[0].X);
    }

    [Fact]
    public void Add_ShouldScrollWhenLimitExceeded()
    {
        // Arrange
        var vm = new ScreenShareGraphViewModel();
        vm.WindowSeconds = 50;

        // Act
        vm.Add(60, 4); // exceeds 50 second window

        // Assert
        Assert.Single(vm.Points);

        Assert.Equal(10, vm.XAxes[0].MinLimit);  // 60 - 50
        Assert.Equal(60, vm.XAxes[0].MaxLimit);
    }

    [Fact]
    public void AddMultiplePoints_ShouldIncreaseCollection()
    {
        // Arrange
        var vm = new ScreenShareGraphViewModel();

        // Act
        vm.AddPoint(0, 1);
        vm.AddPoint(5, 2);
        vm.AddPoint(10, 3);

        // Assert
        Assert.Equal(3, vm.Points.Count);
    }
}
