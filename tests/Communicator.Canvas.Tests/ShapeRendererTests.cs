using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Communicator.Canvas;
using Communicator.UX.Canvas;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// Alias to distinguish between System.Drawing (Data) and System.Windows.Media (WPF)
using Drawing = System.Drawing;

namespace Communicator.Canvas.Tests;

[TestClass]
public class ShapeRendererTests
{
    private System.Windows.Controls.Canvas _canvas = null!;

    [TestInitialize]
    public void Setup()
    {
        // Canvas instantiation is handled inside RunInSta for each test
    }

    [TestMethod]
    public void Render_StraightLine_SetsCoordinatesCorrectly()
    {
        RunInSta(() => {
            _canvas = new System.Windows.Controls.Canvas();

            StraightLine shape = new StraightLine(
                new List<Drawing.Point> { new(0, 0), new(100, 100) },
                Drawing.Color.Red, 2.0, "u1");

            System.Windows.UIElement? result = ShapeRenderer.Render(_canvas, shape);

            Assert.IsInstanceOfType(result, typeof(Line));
            Line line = (Line)result!;
            Assert.AreEqual(0, line.X1);
            Assert.AreEqual(100, line.X2);
        });
    }

    [TestMethod]
    public void Render_Rectangle_SetsDimensionsCorrectly()
    {
        RunInSta(() => {
            _canvas = new System.Windows.Controls.Canvas();

            RectangleShape shape = new RectangleShape(
                new List<Drawing.Point> { new(0, 0), new(50, 60) },
                Drawing.Color.Blue, 1.0, "u1");

            System.Windows.UIElement? result = ShapeRenderer.Render(_canvas, shape);

            Assert.IsInstanceOfType(result, typeof(Rectangle));
            Rectangle rect = (Rectangle)result!;
            Assert.AreEqual(50, rect.Width);
            Assert.AreEqual(60, rect.Height);
        });
    }

    [TestMethod]
    public void Render_Ellipse_SetsDimensionsCorrectly()
    {
        RunInSta(() => {
            _canvas = new System.Windows.Controls.Canvas();

            EllipseShape shape = new EllipseShape(
                new List<Drawing.Point> { new(0, 0), new(20, 40) },
                Drawing.Color.Green, 1.0, "u1");

            System.Windows.UIElement? result = ShapeRenderer.Render(_canvas, shape);

            Assert.IsInstanceOfType(result, typeof(Ellipse));
            Ellipse ellipse = (Ellipse)result!;
            Assert.AreEqual(20, ellipse.Width);
            Assert.AreEqual(40, ellipse.Height);
        });
    }

    [TestMethod]
    public void Createselectionbox_ReturnsDashedRectangle()
    {
        RunInSta(() => {
            Drawing.Rectangle bounds = new Drawing.Rectangle(10, 10, 100, 50);

            Rectangle box = ShapeRenderer.Createselectionbox(bounds);

            Assert.IsNotNull(box.StrokeDashArray);
            Assert.IsTrue(box.StrokeDashArray.Count > 0);
            Assert.AreEqual(Brushes.DeepSkyBlue.ToString(), box.Stroke.ToString());
        });
    }

    [TestMethod]
    public void Render_FreeHand_InsufficientPoints_ReturnsEmpty()
    {
        RunInSta(() => {
            _canvas = new System.Windows.Controls.Canvas();

            FreeHand shape = new FreeHand(new List<Drawing.Point> { new(0, 0) }, Drawing.Color.Black, 1, "u1");

            System.Windows.UIElement? result = ShapeRenderer.Render(_canvas, shape);

            Assert.IsNotInstanceOfType(result, typeof(Polyline));
        });
    }

    [TestMethod]
    public void Render_Triangle_CalculatesVerticesCorrectly()
    {
        RunInSta(() => {
            _canvas = new System.Windows.Controls.Canvas();
            TriangleShape shape = new TriangleShape(
                new List<Drawing.Point> { new(0, 0), new(100, 100) },
                Drawing.Color.Yellow, 2.0, "u1");

            System.Windows.UIElement? result = ShapeRenderer.Render(_canvas, shape);

            Assert.IsInstanceOfType(result, typeof(Polygon));
            Polygon poly = (Polygon)result!;

            Assert.AreEqual(3, poly.Points.Count);
            Assert.AreEqual(50, poly.Points[1].X);
            Assert.AreEqual(0, poly.Points[1].Y);
        });
    }

    [TestMethod]
    public void Render_FreeHand_ValidPoints_CreatesPolyline()
    {
        RunInSta(() => {
            _canvas = new System.Windows.Controls.Canvas();
            FreeHand shape = new FreeHand(
                new List<Drawing.Point> { new(0, 0), new(10, 10), new(20, 20) },
                Drawing.Color.Black, 2.0, "u1");

            System.Windows.UIElement? result = ShapeRenderer.Render(_canvas, shape);

            Assert.IsInstanceOfType(result, typeof(Polyline));
            Polyline poly = (Polyline)result!;
            Assert.AreEqual(3, poly.Points.Count);
        });
    }

    [TestMethod]
    public void RenderAll_AddsMultipleChildrenToCanvas()
    {
        RunInSta(() => {
            _canvas = new System.Windows.Controls.Canvas();
            List<IShape> shapes = new List<IShape>
            {
                new RectangleShape(new List<Drawing.Point> { new(0,0), new(10,10) }, Drawing.Color.Red, 1, "u1"),
                new EllipseShape(new List<Drawing.Point> { new(20,20), new(30,30) }, Drawing.Color.Blue, 1, "u2")
            };

            ShapeRenderer.RenderAll(_canvas, shapes);

            Assert.AreEqual(2, _canvas.Children.Count);
        });
    }

    [TestMethod]
    public void CreateSelectionInfo_CreatesBorderWithText()
    {
        RunInSta(() => {
            Mock<IShape> shape = new Mock<IShape>();
            shape.Setup(s => s.CreatedBy).Returns("Alice");
            shape.Setup(s => s.LastModifiedBy).Returns("Bob");
            Drawing.Rectangle bounds = new Drawing.Rectangle(10, 10, 100, 100);

            System.Windows.UIElement result = ShapeRenderer.CreateSelectionInfo(shape.Object, bounds);

            Assert.IsInstanceOfType(result, typeof(Border));
            Border border = (Border)result;
            Assert.IsInstanceOfType(border.Child, typeof(TextBlock));

            TextBlock textBlock = (TextBlock)border.Child;
            Assert.IsTrue(textBlock.Text.Contains("Alice"));
            Assert.IsTrue(textBlock.Text.Contains("Bob"));
        });
    }

    // Helper to force STA execution for WPF controls
    private void RunInSta(Action action)
    {
        Exception? exception = null;
        Thread thread = new Thread(() => {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception != null)
        {
            throw exception;
        }
    }
}
