using System.Collections.Generic;
using Communicator.Canvas;
using Communicator.Controller.Serialization;
using Communicator.Core.RPC;
using Communicator.Core.UX.Services;
using Communicator.UX.Canvas.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// Alias to distinguish between System.Drawing (Data) and System.Windows.Media (WPF)
using Drawing = System.Drawing;

namespace Communicator.Canvas.Tests;

[TestClass]
public class CanvasViewModelTests
{
    private CanvasViewModel _vm = null!;

    [TestInitialize]
    public void Setup()
    {
        // Initialize with Mocks to satisfy the constructor
        _vm = new CanvasViewModel(new Mock<IRPC>().Object, new Mock<IRpcEventService>().Object) {
            CanvasBounds = new Drawing.Rectangle(0, 0, 1000, 1000)
        };
    }

    [TestMethod]
    public void ChangingCurrentColor_UpdatesSelectedShapeColor()
    {
        // Arrange
        RectangleShape shape = new RectangleShape(new List<Drawing.Point> { new(0, 0), new(10, 10) }, Drawing.Color.Black, 1, "u1");
        _vm._shapes[shape.ShapeId] = shape;
        _vm.SelectedShape = shape;

        // Act
        _vm.CurrentColor = Drawing.Color.Red;

        // Assert
        Assert.IsNotNull(_vm.SelectedShape);
        Assert.AreEqual(Drawing.Color.Red, _vm.SelectedShape.Color);
    }

    [TestMethod]
    public void ChangingDrawingMode_ToFreehand_ClearsSelection()
    {
        // Arrange
        IShape shape = new Mock<IShape>().Object;
        _vm.SelectedShape = shape;
        _vm.CurrentMode = CanvasViewModel.DrawingMode.Select;

        // Act
        _vm.CurrentMode = CanvasViewModel.DrawingMode.FreeHand;

        // Assert
        Assert.IsNull(_vm.SelectedShape);
    }

    [TestMethod]
    public void SelectShapeAt_FindsShape_WhenPointIsInside()
    {
        // Arrange
        RectangleShape shape = new RectangleShape(new List<Drawing.Point> { new(0, 0), new(100, 100) }, Drawing.Color.Black, 1, "u1");
        _vm._shapes[shape.ShapeId] = shape;

        // Act
        _vm.SelectShapeAt(new Drawing.Point(0, 0));

        // Assert
        Assert.IsNotNull(_vm.SelectedShape, "Shape should be selected when clicking on its border");
        Assert.AreEqual(shape.ShapeId, _vm.SelectedShape.ShapeId);
    }

    [TestMethod]
    public void CurrentPreviewShape_ReturnsNull_WhenNotTracking()
    {
        // Act
        _vm._isTracking = false;

        // Assert
        Assert.IsNull(_vm.CurrentPreviewShape);
    }

    [TestMethod]
    public void StartTracking_ResetsLastCreatedShape()
    {
        // Arrange
        _vm.CurrentMode = CanvasViewModel.DrawingMode.Rectangle;

        // Act
        _vm.StartTracking(new Drawing.Point(10, 10));

        // Assert
        Assert.IsNull(_vm.LastCreatedShape);
        Assert.IsTrue(_vm._isTracking);
    }

    [TestMethod]
    public void CurrentThickness_Change_UpdatesSelectedShapeAndTriggerUndo()
    {
        // Arrange
        RectangleShape shape = new RectangleShape(new List<Drawing.Point> { new(0, 0), new(10, 10) }, Drawing.Color.Black, 2.0, "u1");
        _vm._shapes[shape.ShapeId] = shape;
        _vm.SelectedShape = shape;

        // Act
        _vm.CurrentThickness = 5.0;

        // Assert
        Assert.AreEqual(5.0, _vm.SelectedShape.Thickness);
        Assert.IsNotNull(_vm._originalShapeForUndo, "Should store original shape for Undo before modification");
    }

    [TestMethod]
    public void DeleteSelectedShape_RemovesShapeAndAddsToUndoStack()
    {
        // Arrange
        RectangleShape shape = new RectangleShape(new List<Drawing.Point> { new(0, 0), new(10, 10) }, Drawing.Color.Black, 1.0, "u1");
        _vm._shapes[shape.ShapeId] = shape;
        _vm.SelectedShape = shape;

        // Act
        _vm.DeleteSelectedShape();

        // Assert
        Assert.IsNull(_vm.SelectedShape);

        // Verify Undo works
        _vm.Undo();
        Assert.IsTrue(_vm._shapes.ContainsKey(shape.ShapeId));
        Assert.IsFalse(_vm._shapes[shape.ShapeId].IsDeleted);
    }

    [TestMethod]
    public void Undo_RevertsLastCreation()
    {
        // Arrange: Create a shape via tracking
        _vm.CurrentMode = CanvasViewModel.DrawingMode.Rectangle;
        _vm.StartTracking(new Drawing.Point(0, 0));
        _vm.TrackPoint(new Drawing.Point(10, 10));
        _vm.StopTracking();

        string createdId = _vm.LastCreatedShape!.ShapeId;
        Assert.IsTrue(_vm._shapes.ContainsKey(createdId));

        // Act
        _vm.Undo();

        // Assert
        IShape shape = _vm._shapes[createdId];
        Assert.IsTrue(shape.IsDeleted, "Undo of Create should mark shape as deleted");
    }

    [TestMethod]
    public void MovingShape_UpdatesCoordinates()
    {
        // Arrange
        Drawing.Point startP = new Drawing.Point(0, 0);
        Drawing.Point endP = new Drawing.Point(50, 50);
        RectangleShape shape = new RectangleShape(new List<Drawing.Point> { startP, endP }, Drawing.Color.Black, 1, "u1");
        _vm._shapes[shape.ShapeId] = shape;

        // Select the shape
        _vm.CurrentMode = CanvasViewModel.DrawingMode.Select;
        _vm.SelectShapeAt(startP);

        // Act: Simulate Drag
        _vm.StartTracking(startP);
        _vm.TrackPoint(new Drawing.Point(10, 10)); // Move by +10, +10

        // Assert
        IShape? movedShape = _vm.SelectedShape;
        Assert.IsNotNull(movedShape);
        Assert.AreEqual(10, movedShape.Points[0].X);
        Assert.AreEqual(10, movedShape.Points[0].Y);
    }

    [TestMethod]
    public void TrackPoint_ClampsToCanvasBounds()
    {
        // Arrange
        _vm.CanvasBounds = new Drawing.Rectangle(0, 0, 100, 100);
        _vm.CurrentMode = CanvasViewModel.DrawingMode.FreeHand;
        _vm.StartTracking(new Drawing.Point(50, 50));

        // Act: Move mouse WAY outside bounds
        _vm.TrackPoint(new Drawing.Point(200, 200));

        // Assert
        _vm.StopTracking();
        FreeHand shape = (FreeHand)_vm.LastCreatedShape!;
        Drawing.Point lastPoint = shape.Points[shape.Points.Count - 1];

        Assert.AreEqual(100, lastPoint.X, "Should clamp X to 100");
        Assert.AreEqual(100, lastPoint.Y, "Should clamp Y to 100");
    }
}
