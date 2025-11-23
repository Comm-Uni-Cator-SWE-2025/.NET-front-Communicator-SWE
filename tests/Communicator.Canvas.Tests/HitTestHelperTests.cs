using System.Drawing;
using Communicator.Canvas;

namespace Communicator.Canvas.Tests;

public class HitTestHelperTests
{
    [Fact]
    public void DistancePointOnLineIsZero()
    {
        Point a = new Point(0, 0); Point b = new Point(10, 0); Point p = new Point(5, 0);
        double d = HitTestHelper.GetDistanceToLineSegment(p, a, b);
        Assert.Equal(0, d, 3);
    }

    [Fact]
    public void DistancePointOffLine()
    {
        Point a = new Point(0, 0); Point b = new Point(10, 0); Point p = new Point(5, 5);
        double d = HitTestHelper.GetDistanceToLineSegment(p, a, b);
        Assert.True(d > 0);
    }

    [Fact]
    public void PointInRectangleWithTolerance()
    {
        Rectangle rect = new Rectangle(10, 10, 20, 20);
        Assert.True(HitTestHelper.IsPointInRectangle(new Point(9, 10), rect, 1));
        Assert.False(HitTestHelper.IsPointInRectangle(new Point(5, 5), rect, 1));
    }
}
