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

    // --- NEW ---
    /// <summary>
    /// Private constructor for cloning.
    /// </summary>
    private FreeHand(string shapeId, List<Point> points, Color color, double thickness, string userId)
    {
        ShapeId = shapeId;
        Points.AddRange(points);
        Color = color;
        Thickness = thickness;
        UserId = userId;
    }

    public IShape WithUpdates(Color? newColor, double? newThickness)
    {
        return new FreeHand(
            this.ShapeId,
            this.Points,
            newColor ?? this.Color,
            newThickness ?? this.Thickness,
            this.UserId
        );
    }
    // --- END NEW ---
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
        return new FreeHand(
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
        if (Points.Count == 0) { return new Rectangle(0, 0, 0, 0); }

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
