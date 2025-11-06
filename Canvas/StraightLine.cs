using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CanvasDataModel;

public class StraightLine : IShape
{
    public string ShapeId { get; } // ADDED
    public ShapeType Type => ShapeType.StraightLine;
    public List<Point> Points { get; } = new();
    public Color Color { get; }
    public double Thickness { get; }
    public string UserId { get; }
    public StraightLine(List<Point> points, Color color, double thickness, string userId)
    {
        ShapeId = Guid.NewGuid().ToString(); // ADDED
        Points.AddRange(points);
        Color = color;
        Thickness = thickness;
        UserId = userId;
    }

    // --- ADDED ---
    public Rectangle GetBoundingBox()
    {
        if (Points.Count < 2) return new Rectangle(0, 0, 0, 0);

        int minX = Math.Min(Points[0].X, Points[1].X);
        int minY = Math.Min(Points[0].Y, Points[1].Y);
        int maxX = Math.Max(Points[0].X, Points[1].X);
        int maxY = Math.Max(Points[0].Y, Points[1].Y);

        return new Rectangle(minX, minY, maxX - minX, maxY - minY);
    }

    public bool IsHit(Point clickPoint)
    {
        if (Points.Count < 2) return false;

        // Add a tolerance for easier clicking
        double tolerance = (Thickness / 2.0) + 2.0;

        return HitTestHelper.GetDistanceToLineSegment(clickPoint, Points[0], Points[1]) <= tolerance;
    }
    // --- END ADDED ---
}
