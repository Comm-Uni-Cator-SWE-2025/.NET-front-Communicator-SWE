using Communicator.Canvas;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Communicator.Canvas.Tests;

[TestClass]
public class ProcessingServiceTests
{
    [TestMethod]
    public void RegularizeShape_UpdatesThicknessToFive()
    {
        var shape = new FreeHand(new List<Point> { new(0, 0), new(1, 1) }, Color.Black, 2, "u1");
        string json = CanvasSerializer.SerializeShapeManual(shape);

        string regularized = ProcessingService.RegularizeShape(json);

        StringAssert.Contains(regularized, "\"Thickness\": 5");
    }

    [TestMethod]
    public void RegularizeShape_ThicknessNotTwo_RemainsUnchanged()
    {
        var fh = new FreeHand(new List<Point> { new(0, 0) }, Color.Black, 3, "u1");
        string json = CanvasSerializer.SerializeShapeManual(fh);

        string processed = ProcessingService.RegularizeShape(json);

        Assert.IsFalse(processed.Contains("\"Thickness\": 5"), "Should not force thickness to 5 if it wasn't 2");
    }

    [TestMethod]
    public void AnalyzeCanvasImage_MissingFile_ReturnsErrorMessage()
    {
        string result = ProcessingService.AnalyzeCanvasImage("nonexistent.png");
        StringAssert.StartsWith(result, "Error");
    }
}
