using Communicator.Canvas;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Communicator.Canvas.Tests;

[TestClass]
public class HitTestHelperTests
{
    [TestMethod]
    public void GetDistance_PointOnLine_ReturnsZero()
    {
        Point a = new Point(0, 0);
        Point b = new Point(10, 0);
        Point p = new Point(5, 0);

        double d = HitTestHelper.GetDistanceToLineSegment(p, a, b);

        Assert.AreEqual(0, d, 0.001);
    }

    [TestMethod]
    public void GetDistance_PointOffLine_ReturnsPositiveValue()
    {
        Point a = new Point(0, 0);
        Point b = new Point(10, 0);
        Point p = new Point(5, 5);

        double d = HitTestHelper.GetDistanceToLineSegment(p, a, b);

        Assert.IsTrue(d > 0);
    }

    [TestMethod]
    public void GetDistance_DegenerateSegment_ReturnsDistanceToPoint()
    {
        // Case where A == B
        Point p = new Point(5, 5);
        Point a = new Point(5, 5); // Point is exactly on the degenerate segment

        double d = HitTestHelper.GetDistanceToLineSegment(p, a, a);

        Assert.AreEqual(0, d, 0.001);
    }

    [TestMethod]
    public void IsPointInRectangle_WithTolerance_ReturnsCorrectly()
    {
        Rectangle rect = new Rectangle(10, 10, 20, 20);

        Assert.IsTrue(HitTestHelper.IsPointInRectangle(new Point(9, 10), rect, 1), "Point within tolerance should be inside");
        Assert.IsFalse(HitTestHelper.IsPointInRectangle(new Point(5, 5), rect, 1), "Point outside tolerance should be outside");
    }
}
