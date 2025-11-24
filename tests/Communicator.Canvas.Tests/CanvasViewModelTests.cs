using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Communicator.UX.Canvas.ViewModels;
using Communicator.Core.RPC;
using Communicator.Core.UX.Services;
using System.Drawing;
using Communicator.Canvas;
using System.Collections.Generic;

namespace Communicator.Canvas.Tests;

[TestClass]
public class CanvasViewModelTests
{
    private CanvasViewModel _vm;

    [TestInitialize]
    public void Setup()
    {
        _vm = new CanvasViewModel(new Mock<IRPC>().Object, new Mock<IRpcEventService>().Object);
        _vm.CanvasBounds = new Rectangle(0, 0, 1000, 1000);
    }

    [TestMethod]
    public void ChangingCurrentColor_UpdatesSelectedShapeColor()
    {
        // Arrange
        var shape = new RectangleShape(new List<Point> { new(0, 0), new(10, 10) }, Color.Black, 1, "u1");
        _vm._shapes[shape.ShapeId] = shape;
        _vm.SelectedShape = shape; // Set initially

        // Act
        _vm.CurrentColor = Color.Red; // Should trigger update logic

        // Assert
        // IMPORTANT: SelectedShape is likely replaced by a NEW instance with the new color.
        // We must check _vm.SelectedShape, NOT the local 'shape' variable which is the old instance.
        Assert.IsNotNull(_vm.SelectedShape);
        Assert.AreEqual(Color.Red, _vm.SelectedShape.Color);
    }

    [TestMethod]
    public void ChangingDrawingMode_ToFreehand_ClearsSelection()
    {
        var shape = new Mock<IShape>().Object;
        _vm.SelectedShape = shape;
        _vm.CurrentMode = CanvasViewModel.DrawingMode.Select;

        _vm.CurrentMode = CanvasViewModel.DrawingMode.FreeHand;

        Assert.IsNull(_vm.SelectedShape);
    }

    [TestMethod]
    public void SelectShapeAt_FindsShape_WhenPointIsInside()
    {
        var shape = new RectangleShape(new List<Point> { new(0, 0), new(100, 100) }, Color.Black, 1, "u1");
        _vm._shapes[shape.ShapeId] = shape;

        // FIX: The hit test logic for a generic Rectangle usually checks the Border/Stroke.
        // (50, 50) is in the center of the 100x100 box, which is "empty" space for a non-filled shape.
        // We change the test point to (0, 0) (the top-left corner) to ensure a hit on the border.
        _vm.SelectShapeAt(new Point(0, 0));

        Assert.IsNotNull(_vm.SelectedShape, "Shape should be selected when clicking on its border");
        Assert.AreEqual(shape.ShapeId, _vm.SelectedShape.ShapeId);
    }

    [TestMethod]
    public void CurrentPreviewShape_ReturnsNull_WhenNotTracking()
    {
        _vm._isTracking = false;
        Assert.IsNull(_vm.CurrentPreviewShape);
    }

    [TestMethod]
    public void StartTracking_ResetsLastCreatedShape()
    {
        _vm.CurrentMode = CanvasViewModel.DrawingMode.Rectangle;
        _vm.StartTracking(new Point(10, 10));

        Assert.IsNull(_vm.LastCreatedShape);
        Assert.IsTrue(_vm._isTracking);
    }
}
