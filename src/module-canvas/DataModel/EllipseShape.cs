using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CanvasApp.DataModel;

public class EllipseShape : IShape
{
    public string ShapeId { get; } // ADDED
    public ShapeType Type => ShapeType.EllipseShape;
    public List<Point> Points { get; } = new();
    public Color Color { get; }
    public double Thickness { get; }
    public string UserId { get; }

    public EllipseShape(List<Point> points, Color color, double thickness, string userId)
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
    private EllipseShape(string shapeId, List<Point> points, Color color, double thickness, string userId)
    {
        ShapeId = shapeId;
        Points.AddRange(points);
        Color = color;
        Thickness = thickness;
        UserId = userId;
    }

    public IShape WithUpdates(Color? newColor, double? newThickness)
    {
        return new EllipseShape(
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
        return new EllipseShape(
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
        int width = Math.Abs(Points[0].X - Points[1].X);
        int height = Math.Abs(Points[0].Y - Points[1].Y);

        return new Rectangle(minX, minY, width, height);
    }

    public bool IsHit(Point clickPoint)
    {
        if (Points.Count < 2) { return false; }

        double centerX = (Points[0].X + Points[1].X) / 2.0;
        double centerY = (Points[0].Y + Points[1].Y) / 2.0;
        double radiusX = Math.Abs(Points[0].X - Points[1].X) / 2.0;
        double radiusY = Math.Abs(Points[0].Y - Points[1].Y) / 2.0;

        if (radiusX == 0 || radiusY == 0) { return false; }

        // Tolerance for easy clicking
        double tolerance = (Thickness / 2.0) + 2.0;

        // Check if the point is on the "outer" ellipse (radius + tolerance)
        double valOuter = Math.Pow(clickPoint.X - centerX, 2) / Math.Pow(radiusX + tolerance, 2) +
                          Math.Pow(clickPoint.Y - centerY, 2) / Math.Pow(radiusY + tolerance, 2);

        // Check if the point is on the "inner" ellipse (radius - tolerance)
        double valInner = Math.Pow(clickPoint.X - centerX, 2) / Math.Pow(radiusX - tolerance, 2) +
                          Math.Pow(clickPoint.Y - centerY, 2) / Math.Pow(radiusY - tolerance, 2);

        // The point is a "hit" if it's between the inner and outer tolerance ellipses
        return (valOuter <= 1.0 && valInner >= 1.0);
    }
    // --- END ADDED ---
}
