using Communicator.Canvas;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO; // Required for Path and File

namespace Communicator.Canvas.Tests;

[TestClass]
public class ProcessingServiceTests
{
    [TestMethod]
    public void RegularizeShape_UpdatesThicknessToFive()
    {
        // FIX: Explicit type instead of var
        FreeHand shape = new FreeHand(new List<Point> { new(0, 0), new(1, 1) }, Color.Black, 2, "u1");
        string json = CanvasSerializer.SerializeShapeManual(shape);

        string regularized = ProcessingService.RegularizeShape(json);

        StringAssert.Contains(regularized, "\"Thickness\": 5");
    }

    [TestMethod]
    public void RegularizeShape_ThicknessNotTwo_RemainsUnchanged()
    {
        // FIX: Explicit type instead of var
        FreeHand fh = new FreeHand(new List<Point> { new(0, 0) }, Color.Black, 3, "u1");
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

    // --- NEW TEST FOR 100% COVERAGE ---
    [TestMethod]
    public void AnalyzeCanvasImage_ExistingFile_ReturnsAnalysisString()
    {
        // 1. Create a temporary dummy file so the test always passes regardless of computer
        string tempFile = Path.GetTempFileName();

        try
        {
            // 2. Call the method with a file that actually exists
            string result = ProcessingService.AnalyzeCanvasImage(tempFile);

            // 3. Assert that we got the success message, not the error
            StringAssert.Contains(result, "[Analysis Result]");
            StringAssert.Contains(result, "KB");
        }
        finally
        {
            // 4. Cleanup: Delete the temp file to be clean
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
