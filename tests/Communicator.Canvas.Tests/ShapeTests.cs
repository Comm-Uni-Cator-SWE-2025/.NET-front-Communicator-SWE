using Communicator.Canvas;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Communicator.Canvas.Tests;

[TestClass]
public class ShapeTests
{
    [TestMethod]
    public void GetBoundingBox_FreeHand_ReturnsCorrectDimensions()
    {
        var f = new FreeHand(new List<Point> { new(0, 0), new(10, 5) }, Color.Red, 2, "u1");
        Rectangle box = f.GetBoundingBox();

        Assert.AreEqual(0, box.X);
        Assert.AreEqual(0, box.Y);
        Assert.AreEqual(10, box.Width);
        Assert.AreEqual(5, box.Height);
    }

    [TestMethod]
    public void GetBoundingBox_StraightLine_SinglePoint_ReturnsZeroSize()
    {
        var line = new StraightLine(new List<Point> { new(10, 10) }, Color.Black, 1, "u1");
        Rectangle box = line.GetBoundingBox();

        Assert.AreEqual(0, box.Width);
        Assert.AreEqual(0, box.Height);
    }

    [TestMethod]
    public void IsHit_FreeHand_ReturnsTrueOnSegment()
    {
        var f = new FreeHand(new List<Point> { new(0, 0), new(10, 10) }, Color.Red, 2, "u1");
        Assert.IsTrue(f.IsHit(new Point(5, 5)));
    }

    [TestMethod]
    public void IsHit_StraightLine_ReturnsTrueOnLine()
    {
        var l = new StraightLine(new List<Point> { new(0, 0), new(10, 0) }, Color.Black, 2, "u1");
        Assert.IsTrue(l.IsHit(new Point(5, 1)));
    }

    [TestMethod]
    public void IsHit_Rectangle_ReturnsTrueOnBorderOnly()
    {
        var r = new RectangleShape(new List<Point> { new(0, 0), new(10, 10) }, Color.Black, 2, "u1");
        Assert.IsTrue(r.IsHit(new Point(0, 5)), "Should hit border");
        Assert.IsFalse(r.IsHit(new Point(5, 5)), "Should not hit center");
    }

    [TestMethod]
    public void IsHit_Ellipse_ReturnsTrueOnEdge()
    {
        var e = new EllipseShape(new List<Point> { new(0, 0), new(10, 10) }, Color.Black, 2, "u1");
        Assert.IsTrue(e.IsHit(new Point(5, 0)));
    }

    [TestMethod]
    public void IsHit_Triangle_ReturnsTrueOnEdge()
    {
        var t = new TriangleShape(new List<Point> { new(0, 0), new(10, 10) }, Color.Black, 2, "u1");
        Assert.IsTrue(t.IsHit(new Point(0, 5)));
    }

    [TestMethod]
    public void WithUpdates_ReturnsCloneWithNewProperties()
    {
        var f = new FreeHand(new List<Point> { new(0, 0) }, Color.Red, 2, "u1");
        IShape f2 = f.WithUpdates(Color.Blue, 4, "u2");

        Assert.AreEqual(Color.Blue, f2.Color);
        Assert.AreEqual(4, f2.Thickness);
        Assert.AreEqual(f.ShapeId, f2.ShapeId);
    }

    [TestMethod]
    public void WithMove_ClampsShapeInsideBounds()
    {
        var r = new RectangleShape(new List<Point> { new(5, 5), new(15, 15) }, Color.Black, 2, "u1");
        IShape moved = r.WithMove(new Point(-10, -10), new Rectangle(0, 0, 100, 100), "u2");
        Rectangle box = moved.GetBoundingBox();

        Assert.AreEqual(0, box.X);
        Assert.AreEqual(0, box.Y);
    }

    [TestMethod]
    public void WithMove_ZeroSizeShape_ReturnsSameInstance()
    {
        // Logic optimization check: if bounds are empty, WithMove returns 'this'
        var fh = new FreeHand(new List<Point> { new(3, 3) }, Color.Black, 1, "u1");
        IShape moved = fh.WithMove(new Point(5, 5), new Rectangle(0, 0, 100, 100), "u2");

        Assert.AreSame(fh, moved);
    }

    [TestMethod]
    public void WithDelete_And_WithResurrect_ToggleIsDeletedFlag()
    {
        var l = new StraightLine(new List<Point> { new(0, 0), new(1, 1) }, Color.Black, 1, "u1");

        IShape deleted = l.WithDelete("u2");
        Assert.IsTrue(deleted.IsDeleted);

        IShape restored = deleted.WithResurrect("u3");
        Assert.IsFalse(restored.IsDeleted);
    }
}
