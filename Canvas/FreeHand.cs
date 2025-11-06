using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CanvasDataModel;

public class FreeHand : IShape
{
    public string ShapeId { get; } // ADDED
    public ShapeType Type => ShapeType.FreeHand;
    public List<Point> Points { get; } = new();
    public Color Color { get; }
    public double Thickness { get; }
    public string UserId { get; }
    public FreeHand(List<Point> points, Color color, double thickness, string userId)
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
        if (Points.Count == 0) return new Rectangle(0, 0, 0, 0);

        int minX = Points.Min(p => p.X);
        int minY = Points.Min(p => p.Y);
        int maxX = Points.Max(p => p.X);
        int maxY = Points.Max(p => p.Y);

        return new Rectangle(minX, minY, maxX - minX, maxY - minY);
    }

    public bool IsHit(Point clickPoint)
    {
        // Add a tolerance for easier clicking
        double tolerance = (Thickness / 2.0) + 2.0;

        // Check the distance against every segment in the freehand line
        for (int i = 0; i < Points.Count - 1; i++)
        {
            if (HitTestHelper.GetDistanceToLineSegment(clickPoint, Points[i], Points[i + 1]) <= tolerance)
            {
                return true;
            }
        }
        return false;
    }
    // --- END ADDED ---
}
