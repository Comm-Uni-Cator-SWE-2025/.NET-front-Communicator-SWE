using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CanvasDataModel;

public class TriangleShape : IShape
{
    public string ShapeId { get; } // ADDED
    public ShapeType Type => ShapeType.Triangle;
    public List<Point> Points { get; } = new();
    public Color Color { get; }
    public double Thickness { get; }
    public string UserId { get; }

    public TriangleShape(List<Point> points, Color color, double thickness, string userId)
    {
        ShapeId = Guid.NewGuid().ToString(); // ADDED
        Points.AddRange(points);
        Color = color;
        Thickness = thickness;
        UserId = userId;
    }

    // --- ADDED ---
    private Point[] GetVertices()
    {
        if (Points.Count < 2) return new Point[0];

        Point p1 = Points[0]; // Start point
        Point p2 = Points[1]; // End point

        // Vertices as drawn by the renderer
        Point vertex1 = new Point(p1.X, p2.Y);
        Point vertex2 = new Point((p1.X + p2.X) / 2, p1.Y);
        Point vertex3 = new Point(p2.X, p2.Y);
        return new[] { vertex1, vertex2, vertex3 };
    }

    public Rectangle GetBoundingBox()
    {
        if (Points.Count < 2) return new Rectangle(0, 0, 0, 0);

        int minX = Math.Min(Points[0].X, Points[1].X);
        int minY = Math.Min(Points[0].Y, Points[1].Y);
        int width = Math.Abs(Points[0].X - Points[1].X);
        int height = Math.Abs(Points[0].Y - Points[1].Y);

        return new Rectangle(minX, minY, width, height);
    }

    public bool IsHit(Point clickPoint)
    {
        var vertices = GetVertices();
        if (vertices.Length == 0) return false;

        double tolerance = (Thickness / 2.0) + 2.0;

        // Check distance to each of the 3 line segments
        if (HitTestHelper.GetDistanceToLineSegment(clickPoint, vertices[0], vertices[1]) <= tolerance) return true;
        if (HitTestHelper.GetDistanceToLineSegment(clickPoint, vertices[1], vertices[2]) <= tolerance) return true;
        if (HitTestHelper.GetDistanceToLineSegment(clickPoint, vertices[2], vertices[0]) <= tolerance) return true;

        return false;
    }
    // --- END ADDED ---
}
