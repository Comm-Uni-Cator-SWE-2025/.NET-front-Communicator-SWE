using System.Drawing;

namespace CanvasDataModel;

public interface IShape
{
    string ShapeId { get; } // ADDED
    ShapeType Type { get; }
    List<Point> Points { get; }
    Color Color { get; }
    double Thickness { get; }
    string UserId { get; }
    // --- MODIFIED ---
    /// <summary>
    /// Creates a new shape instance with updated properties (e.g., color, thickness).
    /// </summary>
    IShape WithUpdates(Color? newColor, double? newThickness);

    // --- NEW ---
    /// <summary>
    /// Creates a new shape instance with all points translated by an offset,
    /// constrained by the canvas bounds.
    /// </summary>
    IShape WithMove(Point offset, Rectangle canvasBounds);
    // --- END NEW ---
    Rectangle GetBoundingBox();
    bool IsHit(Point clickPoint);
}
