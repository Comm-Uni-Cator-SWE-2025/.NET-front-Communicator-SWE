using System.Collections.Generic;
using System.Drawing;
using Communicator.Canvas;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Communicator.Canvas.Tests;

[TestClass]
public class ShapeTests
{
    // ==========================================
    // Bounding Box Tests
    // ==========================================

    [TestMethod]
    public void GetBoundingBox_FreeHand_ReturnsCorrectDimensions()
    {
        FreeHand f = new FreeHand(new List<Point> { new(0, 0), new(10, 5) }, Color.Red, 2, "u1");
        Rectangle box = f.GetBoundingBox();

        Assert.AreEqual(0, box.X);
        Assert.AreEqual(0, box.Y);
        Assert.AreEqual(10, box.Width);
        Assert.AreEqual(5, box.Height);
    }

    [TestMethod]
    public void GetBoundingBox_StraightLine_SinglePoint_ReturnsZeroSize()
    {
        StraightLine line = new StraightLine(new List<Point> { new(10, 10) }, Color.Black, 1, "u1");
        Rectangle box = line.GetBoundingBox();

        Assert.AreEqual(0, box.Width);
        Assert.AreEqual(0, box.Height);
    }

    // ==========================================
    // Hit Test Logic (IsHit)
    // ==========================================

    [TestMethod]
    public void IsHit_FreeHand_ReturnsTrueOnSegment()
    {
        FreeHand f = new FreeHand(new List<Point> { new(0, 0), new(10, 10) }, Color.Red, 2, "u1");
        Assert.IsTrue(f.IsHit(new Point(5, 5)));
    }

    [TestMethod]
    public void IsHit_StraightLine_ReturnsTrueOnLine()
    {
        StraightLine l = new StraightLine(new List<Point> { new(0, 0), new(10, 0) }, Color.Black, 2, "u1");
        Assert.IsTrue(l.IsHit(new Point(5, 1)));
    }

    [TestMethod]
    public void IsHit_Rectangle_ReturnsTrueOnBorderOnly()
    {
        RectangleShape r = new RectangleShape(new List<Point> { new(0, 0), new(10, 10) }, Color.Black, 2, "u1");
        Assert.IsTrue(r.IsHit(new Point(0, 5)), "Should hit border");
        Assert.IsFalse(r.IsHit(new Point(5, 5)), "Should not hit center");
    }

    // --- ELLIPSE SPECIFIC TESTS (Coverage for EllipseShape.cs) ---

    [TestMethod]
    public void IsHit_Ellipse_ReturnsTrueOnEdge()
    {
        // Ellipse from (0,0) to (10,10). Center is (5,5). Radius is 5.
        EllipseShape e = new EllipseShape(new List<Point> { new(0, 0), new(10, 10) }, Color.Black, 2, "u1");
        // Point (5, 0) is exactly top center edge
        Assert.IsTrue(e.IsHit(new Point(5, 0)));
    }

    [TestMethod]
    public void IsHit_Ellipse_ReturnsFalseInsideAndOutside()
    {
        EllipseShape e = new EllipseShape(new List<Point> { new(0, 0), new(10, 10) }, Color.Black, 2, "u1");

        // Inside (Center)
        Assert.IsFalse(e.IsHit(new Point(5, 5)), "Should return false for center (hollow)");

        // Way Outside
        Assert.IsFalse(e.IsHit(new Point(20, 20)), "Should return false for outside");
    }

    [TestMethod]
    public void IsHit_Ellipse_DegenerateSize_ReturnsFalse()
    {
        // 0 width/height ellipse
        EllipseShape e = new EllipseShape(new List<Point> { new(0, 0), new(0, 0) }, Color.Black, 2, "u1");
        Assert.IsFalse(e.IsHit(new Point(0, 0)));
    }

    [TestMethod]
    public void IsHit_Triangle_ReturnsTrueOnEdge()
    {
        TriangleShape t = new TriangleShape(new List<Point> { new(0, 0), new(10, 10) }, Color.Black, 2, "u1");
        Assert.IsTrue(t.IsHit(new Point(0, 5)));
    }

    // ==========================================
    // Immutable Update Logic (With...)
    // ==========================================

    [TestMethod]
    public void WithUpdates_ReturnsCloneWithNewProperties()
    {
        FreeHand f = new FreeHand(new List<Point> { new(0, 0) }, Color.Red, 2, "u1");
        IShape f2 = f.WithUpdates(Color.Blue, 4, "u2");

        Assert.AreEqual(Color.Blue, f2.Color);
        Assert.AreEqual(4, f2.Thickness);
        Assert.AreEqual(f.ShapeId, f2.ShapeId);
    }

    [TestMethod]
    public void WithMove_ClampsShapeInsideBounds_TopLeft()
    {
        RectangleShape r = new RectangleShape(new List<Point> { new(5, 5), new(15, 15) }, Color.Black, 2, "u1");
        // Try to move to -5, -5 (outside 0,0)
        IShape moved = r.WithMove(new Point(-10, -10), new Rectangle(0, 0, 100, 100), "u2");
        Rectangle box = moved.GetBoundingBox();

        Assert.AreEqual(0, box.X);
        Assert.AreEqual(0, box.Y);
    }

    [TestMethod]
    public void WithMove_ClampsShapeInsideBounds_BottomRight()
    {
        // Shape size 10x10. Canvas 100x100.
        RectangleShape r = new RectangleShape(new List<Point> { new(0, 0), new(10, 10) }, Color.Black, 2, "u1");

        // Try to move way past 100,100
        IShape moved = r.WithMove(new Point(200, 200), new Rectangle(0, 0, 100, 100), "u2");
        Rectangle box = moved.GetBoundingBox();

        // Should be clamped to 90 (because 90+10 = 100)
        Assert.AreEqual(90, box.X);
        Assert.AreEqual(90, box.Y);
    }

    [TestMethod]
    public void WithMove_ZeroSizeShape_ReturnsSameInstance()
    {
        // Logic optimization check: if bounds are empty, WithMove returns 'this'
        FreeHand fh = new FreeHand(new List<Point> { new(3, 3) }, Color.Black, 1, "u1");
        IShape moved = fh.WithMove(new Point(5, 5), new Rectangle(0, 0, 100, 100), "u2");

        Assert.AreSame(fh, moved);
    }

    [TestMethod]
    public void WithDelete_And_WithResurrect_ToggleIsDeletedFlag()
    {
        StraightLine l = new StraightLine(new List<Point> { new(0, 0), new(1, 1) }, Color.Black, 1, "u1");

        IShape deleted = l.WithDelete("u2");
        Assert.IsTrue(deleted.IsDeleted);

        IShape restored = deleted.WithResurrect("u3");
        Assert.IsFalse(restored.IsDeleted);
    }
}
