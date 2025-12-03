using Communicator.UX.Analytics.ViewModels;

namespace Communicator.UX.Tests.AnalyticsUnit;

/// <summary>
/// Unit tests for the GraphViewModel class.
/// </summary>
public class GraphViewModelTests
{
    [Fact]
    public void GraphViewModel_DefaultConstructor_CreatesInstance()
    {
        // Arrange & Act
        var viewModel = new GraphViewModel();

        // Assert
        Assert.NotNull(viewModel);
    }

    [Fact]
    public void GraphViewModel_DefaultConstructor_HasSentimentYAxis()
    {
        // Arrange & Act
        var viewModel = new GraphViewModel();

        // Assert
        Assert.NotNull(viewModel.YAxes);
        Assert.Single(viewModel.YAxes);
        Assert.Equal("Sentiment", viewModel.YAxes[0].Name);
    }

    [Fact]
    public void GraphViewModel_WithCustomYAxisName_SetsCorrectName()
    {
        // Arrange & Act
        var viewModel = new GraphViewModel("FPS");

        // Assert
        Assert.NotNull(viewModel.YAxes);
        Assert.Single(viewModel.YAxes);
        Assert.Equal("FPS", viewModel.YAxes[0].Name);
    }

    [Fact]
    public void GraphViewModel_Points_IsInitiallyEmpty()
    {
        // Arrange & Act
        var viewModel = new GraphViewModel();

        // Assert
        Assert.NotNull(viewModel.Points);
        Assert.Empty(viewModel.Points);
    }

    [Fact]
    public void GraphViewModel_Series_IsInitialized()
    {
        // Arrange & Act
        var viewModel = new GraphViewModel();

        // Assert
        Assert.NotNull(viewModel.Series);
        Assert.Single(viewModel.Series);
    }

    [Fact]
    public void GraphViewModel_XAxes_IsInitialized()
    {
        // Arrange & Act
        var viewModel = new GraphViewModel();

        // Assert
        Assert.NotNull(viewModel.XAxes);
        Assert.Single(viewModel.XAxes);
        Assert.Equal("Time", viewModel.XAxes[0].Name);
    }

    [Fact]
    public void GraphViewModel_WindowSize_DefaultIsCorrect()
    {
        // Arrange & Act
        var viewModel = new GraphViewModel();

        // Assert
        Assert.Equal(10, viewModel.WindowSize);
    }

    [Fact]
    public void GraphViewModel_WindowSize_CanBeModified()
    {
        // Arrange
        var viewModel = new GraphViewModel();

        // Act
        viewModel.WindowSize = 20;

        // Assert
        Assert.Equal(20, viewModel.WindowSize);
    }

    [Fact]
    public void AddPointWithLabel_AddsPointToCollection()
    {
        // Arrange
        var viewModel = new GraphViewModel();

        // Act
        viewModel.AddPointWithLabel("10:01", 0.75);

        // Assert
        Assert.Single(viewModel.Points);
        Assert.Equal(0, viewModel.Points[0].X);
        Assert.Equal(0.75, viewModel.Points[0].Y);
    }

    [Fact]
    public void AddPointWithLabel_MultiplePoints_IncreasesIndex()
    {
        // Arrange
        var viewModel = new GraphViewModel();

        // Act
        viewModel.AddPointWithLabel("10:01", 0.5);
        viewModel.AddPointWithLabel("10:02", 0.6);
        viewModel.AddPointWithLabel("10:03", 0.7);

        // Assert
        Assert.Equal(3, viewModel.Points.Count);
        Assert.Equal(0, viewModel.Points[0].X);
        Assert.Equal(1, viewModel.Points[1].X);
        Assert.Equal(2, viewModel.Points[2].X);
    }

    [Fact]
    public void AddPointWithLabel_PreservesYValues()
    {
        // Arrange
        var viewModel = new GraphViewModel();
        double[] expectedValues = { 0.1, 0.5, 0.9 };

        // Act
        viewModel.AddPointWithLabel("10:01", expectedValues[0]);
        viewModel.AddPointWithLabel("10:02", expectedValues[1]);
        viewModel.AddPointWithLabel("10:03", expectedValues[2]);

        // Assert
        for (int i = 0; i < expectedValues.Length; i++)
        {
            Assert.Equal(expectedValues[i], viewModel.Points[i].Y);
        }
    }

    [Fact]
    public void Clear_RemovesAllPoints()
    {
        // Arrange
        var viewModel = new GraphViewModel();
        viewModel.AddPointWithLabel("10:01", 0.5);
        viewModel.AddPointWithLabel("10:02", 0.6);

        // Act
        viewModel.Clear();

        // Assert
        Assert.Empty(viewModel.Points);
    }

    [Fact]
    public void GraphViewModel_FpsYAxis_CanBeCreated()
    {
        // Arrange & Act
        var fpsGraph = new GraphViewModel("FPS");
        var sentimentGraph = new GraphViewModel("Sentiment");

        // Assert
        Assert.Equal("FPS", fpsGraph.YAxes[0].Name);
        Assert.Equal("Sentiment", sentimentGraph.YAxes[0].Name);
    }

    [Fact]
    public void AddPointWithLabel_NegativeValue_IsAllowed()
    {
        // Arrange
        var viewModel = new GraphViewModel();

        // Act
        viewModel.AddPointWithLabel("10:01", -0.5);

        // Assert
        Assert.Single(viewModel.Points);
        Assert.Equal(-0.5, viewModel.Points[0].Y);
    }

    [Fact]
    public void AddPointWithLabel_ZeroValue_IsAllowed()
    {
        // Arrange
        var viewModel = new GraphViewModel();

        // Act
        viewModel.AddPointWithLabel("10:01", 0.0);

        // Assert
        Assert.Single(viewModel.Points);
        Assert.Equal(0.0, viewModel.Points[0].Y);
    }

    [Fact]
    public void AddPointWithLabel_LargeValue_IsAllowed()
    {
        // Arrange
        var viewModel = new GraphViewModel();

        // Act
        viewModel.AddPointWithLabel("10:01", 1000.0);

        // Assert
        Assert.Single(viewModel.Points);
        Assert.Equal(1000.0, viewModel.Points[0].Y);
    }
}
