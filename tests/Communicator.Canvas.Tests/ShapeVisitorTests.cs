using System.Drawing;
using Communicator.Canvas;

namespace Communicator.Canvas.Tests;

sealed class TestVisitor : IShapeVisitor<string>
{
    public string Visit(FreeHand freeHand)
    {
        return $"FreeHand:{freeHand.ShapeId}";
    }

    public string Visit(RectangleShape rectangle)
    {
        return $"Rectangle:{rectangle.ShapeId}";
    }

    public string Visit(TriangleShape triangle)
    {
        return $"Triangle:{triangle.ShapeId}";
    }

    public string Visit(StraightLine line)
    {
        return $"Line:{line.ShapeId}";
    }

    public string Visit(EllipseShape ellipse)
    {
        return $"Ellipse:{ellipse.ShapeId}";
    }
}

public class ShapeVisitorTests
{
    [Fact]
    public void Visitor_DispatchesPerType()
    {
        TestVisitor visitor = new TestVisitor();
        IShape[] shapes = new IShape[]
        {
            new FreeHand(new(){ new(0,0) }, Color.Black, 1, "u"),
            new RectangleShape(new(){ new(0,0), new(1,1) }, Color.Black, 1, "u"),
            new TriangleShape(new(){ new(0,0), new(2,2) }, Color.Black, 1, "u"),
            new StraightLine(new(){ new(0,0), new(3,3) }, Color.Black, 1, "u"),
            new EllipseShape(new(){ new(0,0), new(4,4) }, Color.Black, 1, "u"),
        };

        foreach (IShape s in shapes)
        {
            string result = s.Accept(visitor);
            Assert.Contains(s.ShapeId, result);
        }
    }
}
