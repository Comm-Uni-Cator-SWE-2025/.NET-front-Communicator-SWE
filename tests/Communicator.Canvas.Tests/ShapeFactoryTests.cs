using System.Drawing;
using Communicator.Canvas;

namespace Communicator.Canvas.Tests;

public class ShapeFactoryTests
{
    [Fact]
    public void CreateFreeHand()
    {
        IShape shape = ShapeFactory.CreateShape(ShapeType.FREEHAND, new() { new(0, 0) }, Color.Red, 3, "u1");
        Assert.IsType<FreeHand>(shape);
        Assert.Equal(ShapeType.FREEHAND, shape.Type);
    }

    [Fact]
    public void CreateLine()
    {
        IShape shape = ShapeFactory.CreateShape(ShapeType.LINE, new() { new(0, 0), new(1, 1) }, Color.Black, 2, "u1");
        Assert.IsType<StraightLine>(shape);
    }

    [Fact]
    public void CreateRectangle()
    {
        IShape shape = ShapeFactory.CreateShape(ShapeType.RECTANGLE, new() { new(0, 0), new(3, 4) }, Color.Blue, 1, "u1");
        Assert.IsType<RectangleShape>(shape);
    }

    [Fact]
    public void CreateEllipse()
    {
        IShape shape = ShapeFactory.CreateShape(ShapeType.ELLIPSE, new() { new(0, 0), new(3, 4) }, Color.Blue, 1, "u1");
        Assert.IsType<EllipseShape>(shape);
    }

    [Fact]
    public void CreateTriangle()
    {
        IShape shape = ShapeFactory.CreateShape(ShapeType.TRIANGLE, new() { new(0, 0), new(3, 4) }, Color.Blue, 1, "u1");
        Assert.IsType<TriangleShape>(shape);
    }

    [Fact]
    public void UnsupportedTypeThrows()
    {
        Assert.Throws<ArgumentException>(() => ShapeFactory.CreateShape((ShapeType)999, new() { new(0, 0) }, Color.White, 1, "u1"));
    }
}
