using Microsoft.VisualStudio.TestTools.UnitTesting;
using Communicator.UX.Canvas;
using Communicator.Canvas;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Collections.Generic;
using Drawing = System.Drawing;
using System.Threading;
using System;

namespace Communicator.Canvas.Tests;

[TestClass]
public class ShapeRendererTests
{
    private System.Windows.Controls.Canvas _canvas;

    [TestInitialize]
    public void Setup()
    {
        // FIX: Removed _canvas instantiation here. 
        // It must be created inside the STA thread (RunInSta) to avoid InvalidOperationException.
    }

    [TestMethod]
    public void Render_StraightLine_SetsCoordinatesCorrectly()
    {
        RunInSta(() => {
            // FIX: Instantiate Canvas here on the STA thread
            _canvas = new System.Windows.Controls.Canvas();

            var shape = new StraightLine(
                new List<Drawing.Point> { new(0, 0), new(100, 100) },
                Drawing.Color.Red, 2.0, "u1");

            var result = ShapeRenderer.Render(_canvas, shape);

            Assert.IsInstanceOfType(result, typeof(Line));
            var line = (Line)result;
            Assert.AreEqual(0, line.X1);
            Assert.AreEqual(100, line.X2);
        });
    }

    [TestMethod]
    public void Render_Rectangle_SetsDimensionsCorrectly()
    {
        RunInSta(() => {
            // FIX: Instantiate Canvas here
            _canvas = new System.Windows.Controls.Canvas();

            var shape = new RectangleShape(
                new List<Drawing.Point> { new(0, 0), new(50, 60) },
                Drawing.Color.Blue, 1.0, "u1");

            var result = ShapeRenderer.Render(_canvas, shape);

            Assert.IsInstanceOfType(result, typeof(Rectangle));
            var rect = (Rectangle)result;
            Assert.AreEqual(50, rect.Width);
            Assert.AreEqual(60, rect.Height);
        });
    }

    [TestMethod]
    public void Render_Ellipse_SetsDimensionsCorrectly()
    {
        RunInSta(() => {
            // FIX: Instantiate Canvas here
            _canvas = new System.Windows.Controls.Canvas();

            var shape = new EllipseShape(
                new List<Drawing.Point> { new(0, 0), new(20, 40) },
                Drawing.Color.Green, 1.0, "u1");

            var result = ShapeRenderer.Render(_canvas, shape);

            Assert.IsInstanceOfType(result, typeof(Ellipse));
            var ellipse = (Ellipse)result;
            Assert.AreEqual(20, ellipse.Width);
            Assert.AreEqual(40, ellipse.Height);
        });
    }

    [TestMethod]
    public void Createselectionbox_ReturnsDashedRectangle()
    {
        RunInSta(() => {
            // No _canvas needed here, but RunInSta is still good for Brush access
            var bounds = new Drawing.Rectangle(10, 10, 100, 50);

            var box = ShapeRenderer.Createselectionbox(bounds);

            Assert.IsNotNull(box.StrokeDashArray);
            Assert.IsTrue(box.StrokeDashArray.Count > 0);
            Assert.AreEqual(Brushes.DeepSkyBlue.ToString(), box.Stroke.ToString());
        });
    }

    [TestMethod]
    public void Render_FreeHand_InsufficientPoints_ReturnsEmpty()
    {
        RunInSta(() => {
            // FIX: Instantiate Canvas here
            _canvas = new System.Windows.Controls.Canvas();

            var shape = new FreeHand(new List<Drawing.Point> { new(0, 0) }, Drawing.Color.Black, 1, "u1");

            var result = ShapeRenderer.Render(_canvas, shape);

            Assert.IsNotInstanceOfType(result, typeof(Polyline));
        });
    }

    // Helper to force STA execution
    private void RunInSta(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() => {
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
            throw exception;
    }
}
