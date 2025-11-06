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

    Rectangle GetBoundingBox();
    bool IsHit(Point clickPoint);
}
