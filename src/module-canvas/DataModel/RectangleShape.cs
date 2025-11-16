using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CanvasApp.DataModel;

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

    // --- NEW ---
    /// <summary>
    /// Private constructor for cloning.
    /// </summary>
    internal RectangleShape(string shapeId, List<Point> points, Color color, double thickness, string userId)
    {
        ShapeId = shapeId;
        Points.AddRange(points);
        Color = color;
        Thickness = thickness;
        UserId = userId;
    }

    public IShape WithUpdates(Color? newColor, double? newThickness)
    {
        return new RectangleShape(
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
        return new RectangleShape(
            this.ShapeId,
            newPoints,
            this.Color,
            this.Thickness,
            this.UserId
        );
    }
    // --- END NEW ---
    // --- ADDED ---
    private Rectangle GetBoundsInternal()
    {
        if (Points.Count < 2) { return new Rectangle(0, 0, 0, 0); }

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
        if (Points.Count < 2) { return false; }

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
