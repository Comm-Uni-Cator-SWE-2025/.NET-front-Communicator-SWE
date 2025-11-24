namespace Communicator.UX.Analytics.Models;

/// <summary>
/// Holds the count of each shape type from a snapshot of the canvas.
/// </summary>
public class CanvasData
{
    /// <summary>Number of free-hand drawn shapes.</summary>
    public int FreeHand { get; set; }

    /// <summary>Number of straight lines.</summary>
    public int StraightLine { get; set; }

    /// <summary>Total rectangles drawn.</summary>
    public int Rectangle { get; set; }

    /// <summary>Ellipse count.</summary>
    public int Ellipse { get; set; }

    /// <summary>Triangle count.</summary>
    public int Triangle { get; set; }

    /// <summary>Snapshot label (T1, T2, T3, ...).</summary>
    public string Label { get; set; } = "";
}
