using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CanvasApp.Services;

using CanvasApp.DataModel;

/// <summary>
/// Helper "tool" that mirrors the selection behavior (finding the top-most shape at a point)
/// and provides a simple "regularize" operation which converts an irregular shape (e.g. FreeHand)
/// into a regular shape approximation (using the bounding box).
/// 
/// This class is intentionally stateless and pure so the ViewModel can call it from the UI/tool
/// layer and decide how to commit the replacement into the canvas dictionary / undo stack.
/// </summary>
public static class AiService
{
    /// <summary>
    /// Find the top-most visible shape at the given point.
    /// The shapes collection is treated as an ordered sequence (insertion order). To emulate
    /// top-most semantics callers should pass shapes in natural drawing order; this method
    /// inspects them in reverse so later (top-most) shapes are found first.
    /// </summary>
    public static IShape? FindTopMostHit(IDictionary<string, (IShape Shape, bool IsVisible)> shapes, Point point)
    {
        if (shapes == null)
        {
            throw new ArgumentNullException(nameof(shapes));
        }

        // iterate in reverse order (top-most = last added)
        foreach ((IShape Shape, bool IsVisible) kv in shapes.Values.Reverse())
        {
            if (kv.IsVisible)
            {
                if (kv.Shape.IsHit(point))
                {
                    return kv.Shape;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Returns a "regularized" replacement for the provided shape.
    /// - FreeHand => RectangleShape (bounding box)
    /// - Triangle => RectangleShape (bounding box)
    /// - Square / Rectangle / Ellipse / StraightLine => returned as-is (no-op)
    /// 
    /// The returned IShape is a new instance; callers are responsible for replacing it in the
    /// canvas dictionary and for creating the appropriate undo/redo CanvasAction.
    /// </summary>
    public static IShape Regularize(IShape shape, string currentUserId)
    {
        if (shape == null)
        {
            throw new ArgumentNullException(nameof(shape));
        }
        if (currentUserId == null)
        {
            throw new ArgumentNullException(nameof(currentUserId));
        }

        // If it's already a regular shape we simply return the same shape (no change).
        if (shape.Type != ShapeType.FreeHand)
        {
            return shape;
        }

        // Use the bounding box of the shape to create a regularized rectangle.
        Rectangle bbox = shape.GetBoundingBox();
        Point topLeft = new Point(bbox.Left, bbox.Top);
        Point bottomRight = new Point(bbox.Right, bbox.Bottom);

        List<Point> pts = new List<Point> { topLeft, bottomRight };

        // Prefer Rectangle as a general "regularized" replacement.
        // Preserve color and thickness and reuse the original ShapeId so the dictionary key remains valid.
        return new RectangleShape(pts, shape.Color, shape.Thickness, currentUserId);
    }

    /// <summary>
    /// Creates a very small rectangle (at least 1x1) centered on the provided shape's bounding box.
    /// The returned RectangleShape reuses the original ShapeId so it can replace the entry in the
    /// canvas dictionary directly.
    /// </summary>
    public static IShape SmallReplacement(IShape shape, string currentUserId, int width = 1, int height = 1)
    {
        if (shape == null)
        {
            throw new ArgumentNullException(nameof(shape));
        }
        if (currentUserId == null)
        {
            throw new ArgumentNullException(nameof(currentUserId));
        }

        Rectangle bbox = shape.GetBoundingBox();
        int centerX = bbox.Left + Math.Max(0, bbox.Width / 2);
        int centerY = bbox.Top + Math.Max(0, bbox.Height / 2);

        // Ensure width/height are at least 1
        int w = Math.Max(1, width);
        int h = Math.Max(1, height);

        Point topLeft = new Point(centerX - w / 2, centerY - h / 2);
        Point bottomRight = new Point(topLeft.X + w, topLeft.Y + h);

        var pts = new List<Point> { topLeft, bottomRight };
        return new RectangleShape(pts, shape.Color, shape.Thickness, currentUserId);
    }
}
