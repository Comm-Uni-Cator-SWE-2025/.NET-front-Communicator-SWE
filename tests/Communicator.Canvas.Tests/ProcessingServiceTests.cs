using System;
using System.Drawing;
using Communicator.Canvas;

namespace Communicator.Canvas.Tests;

public class ProcessingServiceTests
{
    [Fact]
    public void RegularizeShapeModifiesThickness()
    {
        FreeHand shape = new FreeHand(new() { new(0, 0), new(1, 1) }, Color.Black, 2, "u1");
        string json = CanvasSerializer.SerializeShapeManual(shape);
        string regularized = ProcessingService.RegularizeShape(json);
        Assert.Contains("\"Thickness\": 5", regularized);
    }

    [Fact]
    public void AnalyzeCanvasImageFileMissing()
    {
        string result = ProcessingService.AnalyzeCanvasImage("nonexistent.png");
        Assert.StartsWith("Error", result, StringComparison.Ordinal);
    }
}
