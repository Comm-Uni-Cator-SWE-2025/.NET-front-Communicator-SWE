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
    // --- NEW ---
    /// <summary>
    /// Private constructor for cloning.
    /// </summary>
    private StraightLine(string shapeId, List<Point> points, Color color, double thickness, string userId)
    {
        ShapeId = shapeId;
        Points.AddRange(points);
        Color = color;
        Thickness = thickness;
        UserId = userId;
    }

    public IShape WithUpdates(Color? newColor, double? newThickness)
    {
        return new StraightLine(
            this.ShapeId,
            this.Points,
            newColor ?? this.Color,
            newThickness ?? this.Thickness,
            this.UserId
        );
    }
    // --- NEW ---
    public IShape WithMove(Point offset, Rectangle canvasBounds)
    {
        Rectangle oldBounds = GetBoundingBox();
        if (oldBounds.Width == 0 && oldBounds.Height == 0) { return this; }

        // Calculate target new position
        int newLeft = oldBounds.Left + offset.X;
        int newTop = oldBounds.Top + offset.Y;

        // Clamp the offset based on canvas bounds
        if (newLeft < canvasBounds.Left)
        {
            offset.X = canvasBounds.Left - oldBounds.Left;
        }
        if (newTop < canvasBounds.Top)
        {
            offset.Y = canvasBounds.Top - oldBounds.Top;
        }
        if (newLeft + oldBounds.Width > canvasBounds.Right)
        {
            offset.X = canvasBounds.Right - oldBounds.Right;
        }
        if (newTop + oldBounds.Height > canvasBounds.Bottom)
        {
            offset.Y = canvasBounds.Bottom - oldBounds.Bottom;
        }

        // Create new points list with clamped offset
        List<Point> newPoints = new List<Point>();
        foreach (Point p in this.Points)
        {
            newPoints.Add(new Point(p.X + offset.X, p.Y + offset.Y));
        }

        // Return a new shape with the same ID but new points
        return new StraightLine(
            this.ShapeId,
            newPoints,
            this.Color,
            this.Thickness,
            this.UserId
        );
    }
    // --- END NEW ---
    public Rectangle GetBoundingBox()
    {
        if (Points.Count < 2) { return new Rectangle(0, 0, 0, 0); }

        int minX = Math.Min(Points[0].X, Points[1].X);
        int minY = Math.Min(Points[0].Y, Points[1].Y);
        int maxX = Math.Max(Points[0].X, Points[1].X);
        int maxY = Math.Max(Points[0].Y, Points[1].Y);

        return new Rectangle(minX, minY, maxX - minX, maxY - minY);
    }

    public bool IsHit(Point clickPoint)
    {
        if (Points.Count < 2) { return false; }

        // Add a tolerance for easier clicking
        double tolerance = (Thickness / 2.0) + 2.0;

        return HitTestHelper.GetDistanceToLineSegment(clickPoint, Points[0], Points[1]) <= tolerance;
    }
    // --- END ADDED ---
}
