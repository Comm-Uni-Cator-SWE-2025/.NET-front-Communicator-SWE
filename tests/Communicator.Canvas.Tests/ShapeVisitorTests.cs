using System.Collections.Generic;
using System.Drawing;
using Communicator.Canvas;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Communicator.Canvas.Tests;

[TestClass]
public class ShapeVisitorTests
{
    private sealed class TestVisitor : IShapeVisitor<string>
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

    [TestMethod]
    public void Accept_DispatchesToCorrectVisitorMethod()
    {
        TestVisitor visitor = new TestVisitor();

        // Ensure every shape type is present to cover all Accept/Visit paths
        IShape[] shapes = new IShape[]
        {
            new FreeHand(new List<Point>{ new(0,0) }, Color.Black, 1, "u"),
            new RectangleShape(new List<Point>{ new(0,0), new(1,1) }, Color.Black, 1, "u"),
            new TriangleShape(new List<Point>{ new(0,0), new(2,2) }, Color.Black, 1, "u"),
            new StraightLine(new List<Point>{ new(0,0), new(3,3) }, Color.Black, 1, "u"),
            new EllipseShape(new List<Point>{ new(0,0), new(4,4) }, Color.Black, 1, "u"),
        };

        foreach (IShape s in shapes)
        {
            string result = s.Accept(visitor);
            Assert.IsTrue(result.Contains(s.ShapeId));
        }
    }

    [TestMethod]
    public void Visit_AllShapes_ReturnsCorrectPrefix()
    {
        TestVisitor visitor = new TestVisitor();
        FreeHand fh = new FreeHand(new List<Point> { new(0, 0) }, Color.Black, 1, "u1");

        string result = fh.Accept(visitor);

        StringAssert.StartsWith(result, "FreeHand:");
    }
}
