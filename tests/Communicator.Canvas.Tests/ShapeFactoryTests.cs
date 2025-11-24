using Communicator.Canvas;
using System.Drawing;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Communicator.Canvas.Tests;

[TestClass]
public class ShapeFactoryTests
{
    [TestMethod]
    public void CreateShape_FreeHand_ReturnsCorrectType()
    {
        IShape shape = ShapeFactory.CreateShape(ShapeType.FREEHAND, new List<Point> { new(0, 0) }, Color.Red, 3, "u1");
        Assert.IsInstanceOfType(shape, typeof(FreeHand));
        Assert.AreEqual(ShapeType.FREEHAND, shape.Type);
    }

    [TestMethod]
    public void CreateShape_Line_ReturnsCorrectType()
    {
        IShape shape = ShapeFactory.CreateShape(ShapeType.LINE, new List<Point> { new(0, 0), new(1, 1) }, Color.Black, 2, "u1");
        Assert.IsInstanceOfType(shape, typeof(StraightLine));
    }

    [TestMethod]
    public void CreateShape_Rectangle_ReturnsCorrectType()
    {
        IShape shape = ShapeFactory.CreateShape(ShapeType.RECTANGLE, new List<Point> { new(0, 0), new(3, 4) }, Color.Blue, 1, "u1");
        Assert.IsInstanceOfType(shape, typeof(RectangleShape));
    }

    [TestMethod]
    public void CreateShape_Ellipse_ReturnsCorrectType()
    {
        IShape shape = ShapeFactory.CreateShape(ShapeType.ELLIPSE, new List<Point> { new(0, 0), new(3, 4) }, Color.Blue, 1, "u1");
        Assert.IsInstanceOfType(shape, typeof(EllipseShape));
    }

    [TestMethod]
    public void CreateShape_Triangle_ReturnsCorrectType()
    {
        IShape shape = ShapeFactory.CreateShape(ShapeType.TRIANGLE, new List<Point> { new(0, 0), new(3, 4) }, Color.Blue, 1, "u1");
        Assert.IsInstanceOfType(shape, typeof(TriangleShape));
    }

    [TestMethod]
    public void CreateShape_InvalidType_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            ShapeFactory.CreateShape((ShapeType)999, new List<Point> { new(0, 0) }, Color.White, 1, "u1"));
    }

    [TestMethod]
    public void CreateShape_Default_IsDeletedFalse()
    {
        IShape shape = ShapeFactory.CreateShape(ShapeType.RECTANGLE, new List<Point> { new(0, 0), new(5, 5) }, Color.Purple, 2, "u1");
        Assert.IsFalse(shape.IsDeleted);
        Assert.IsFalse(string.IsNullOrWhiteSpace(shape.ShapeId));
    }
}
