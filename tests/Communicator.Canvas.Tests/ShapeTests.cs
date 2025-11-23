using System.Drawing;
using Communicator.Canvas;

namespace Communicator.Canvas.Tests;

public class ShapeTests
{
    [Fact]
    public void FreeHandBoundingBox()
    {
        FreeHand f = new FreeHand(new() { new(0, 0), new(10, 5) }, Color.Red, 2, "u1");
        Rectangle box = f.GetBoundingBox();
        Assert.Equal(0, box.X);
        Assert.Equal(0, box.Y);
        Assert.Equal(10, box.Width);
        Assert.Equal(5, box.Height);
    }

    [Fact]
    public void FreeHandIsHitOnSegment()
    {
        FreeHand f = new FreeHand(new() { new(0, 0), new(10, 10) }, Color.Red, 2, "u1");
        Assert.True(f.IsHit(new Point(5, 5)));
    }

    [Fact]
    public void StraightLineIsHit()
    {
        StraightLine l = new StraightLine(new() { new(0, 0), new(10, 0) }, Color.Black, 2, "u1");
        Assert.True(l.IsHit(new Point(5, 1)));
    }

    [Fact]
    public void RectangleIsHitOnBorder()
    {
        RectangleShape r = new RectangleShape(new() { new(0, 0), new(10, 10) }, Color.Black, 2, "u1");
        Assert.True(r.IsHit(new Point(0, 5)));
        Assert.False(r.IsHit(new Point(5, 5)));
    }

    [Fact]
    public void EllipseIsHitOnEdge()
    {
        EllipseShape e = new EllipseShape(new() { new(0, 0), new(10, 10) }, Color.Black, 2, "u1");
        Assert.True(e.IsHit(new Point(5, 0)));
    }

    [Fact]
    public void TriangleIsHitOnEdge()
    {
        TriangleShape t = new TriangleShape(new() { new(0, 0), new(10, 10) }, Color.Black, 2, "u1");
        Assert.True(t.IsHit(new Point(0, 5)));
    }

    [Fact]
    public void WithUpdatesChangesColorThickness()
    {
        FreeHand f = new FreeHand(new() { new(0, 0) }, Color.Red, 2, "u1");
        IShape f2 = f.WithUpdates(Color.Blue, 4, "u2");
        Assert.Equal(Color.Blue, f2.Color);
        Assert.Equal(4, f2.Thickness);
        Assert.Equal(f.ShapeId, f2.ShapeId);
    }

    [Fact]
    public void WithMoveClampsInsideBounds()
    {
        RectangleShape r = new RectangleShape(new() { new(5, 5), new(15, 15) }, Color.Black, 2, "u1");
        IShape moved = r.WithMove(new Point(-10, -10), new Rectangle(0, 0, 100, 100), "u2");
        Rectangle box = moved.GetBoundingBox();
        Assert.Equal(0, box.X);
        Assert.Equal(0, box.Y);
    }

    [Fact]
    public void DeleteResurrectFlags()
    {
        StraightLine l = new StraightLine(new() { new(0, 0), new(1, 1) }, Color.Black, 1, "u1");
        IShape deleted = l.WithDelete("u2");
        Assert.True(deleted.IsDeleted);
        IShape restored = deleted.WithResurrect("u3");
        Assert.False(restored.IsDeleted);
    }
}
