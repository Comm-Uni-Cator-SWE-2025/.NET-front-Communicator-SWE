using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CanvasDataModel;

public class RectangleShape : IShape
{
    public string ShapeId { get; }
    public ShapeType Type => ShapeType.Rectangle;
    public List<Point> Points { get; } = new();
    public Color Color { get; }
    public double Thickness { get; }
    public string UserId { get; }

    public RectangleShape(List<Point> points, Color color, double thickness, string userId)
    {
        ShapeId = Guid.NewGuid().ToString();
        Points.AddRange(points);
        Color = color;
        Thickness = thickness;
        UserId = userId;
    }

    // --- ADDED ---
    private Rectangle GetBoundsInternal()
    {
        if (Points.Count < 2) return new Rectangle(0, 0, 0, 0);

        int minX = Math.Min(Points[0].X, Points[1].X);
        int minY = Math.Min(Points[0].Y, Points[1].Y);
        int width = Math.Abs(Points[0].X - Points[1].X);
        int height = Math.Abs(Points[0].Y - Points[1].Y);

        return new Rectangle(minX, minY, width, height);
    }

    public Rectangle GetBoundingBox()
    {
        return GetBoundsInternal();
    }

    public bool IsHit(Point clickPoint)
    {
        if (Points.Count < 2) return false;

        Rectangle bounds = GetBoundsInternal();
        double tolerance = (Thickness / 2.0) + 2.0;

        // Check if point is inside the filled area
        if (HitTestHelper.IsPointInRectangle(clickPoint, bounds, 0))
        {
            // Check if it's NOT in the "inner" hollow part
            Rectangle innerBounds = new Rectangle(
                (int)(bounds.Left + tolerance),
                (int)(bounds.Top + tolerance),
                (int)(bounds.Width - 2 * tolerance),
                (int)(bounds.Height - 2 * tolerance)
            );
            return !HitTestHelper.IsPointInRectangle(clickPoint, innerBounds, 0);
        }
        return false;
    }
    // --- END ADDED ---
}
