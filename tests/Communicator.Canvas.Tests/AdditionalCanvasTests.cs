using System.Drawing;
using Communicator.Canvas;

namespace Communicator.Canvas.Tests;

public class AdditionalCanvasTests
{
    [Fact]
    public void StateManagerPeekUndoEmptyReturnsNull()
    {
        StateManager mgr = new StateManager();
        Assert.Null(mgr.PeekUndo());
    }

    [Fact]
    public void StateManagerPeekRedoEmptyReturnsNull()
    {
        StateManager mgr = new StateManager();
        Assert.Null(mgr.PeekRedo());
    }

    [Fact]
    public void StateManagerImportEmptyResets()
    {
        StateManager mgr = new StateManager();
        SerializedActionStack empty = new SerializedActionStack();
        mgr.ImportState(empty);
        Assert.Null(mgr.Undo());
        Assert.Null(mgr.Redo());
    }

    [Fact]
    public void HitTestHelperDegenerateSegment()
    {
        Point p = new Point(5, 5);
        Point a = new Point(5, 5);
        double d = HitTestHelper.GetDistanceToLineSegment(p, a, a);
        Assert.Equal(0, d, 3);
    }

    [Fact]
    public void StraightLineBoundingBoxSinglePointIsZero()
    {
        StraightLine line = new StraightLine(new() { new(10, 10) }, Color.Black, 1, "u1");
        Rectangle box = line.GetBoundingBox();
        Assert.Equal(0, box.Width);
        Assert.Equal(0, box.Height);
    }

    [Fact]
    public void FreeHandWithMoveZeroSizeReturnsSameInstance()
    {
        FreeHand fh = new FreeHand(new() { new(3, 3) }, Color.Black, 1, "u1");
        IShape moved = fh.WithMove(new Point(5, 5), new Rectangle(0, 0, 100, 100), "u2");
        Assert.Same(fh, moved); // method returns this for zero sized bounding box
    }

    [Fact]
    public void SerializerDeserializeShapeInvalidReturnsNull()
    {
        IShape? shape = CanvasSerializer.DeserializeShapeManual("{ \"Type\": \"INVALID\" }");
        Assert.Null(shape);
    }

    [Fact]
    public void SerializerDeserializeActionInvalidReturnsNull()
    {
        CanvasAction? action = CanvasSerializer.DeserializeActionManual("{ \"ActionType\": \"INVALID\" }");
        Assert.Null(action);
    }

    [Fact]
    public void NetworkMockSendToUnknownDoesNotThrow()
    {
        NetworkMock.SendMessage("9.9.9.9", "msg");
        Assert.True(true);
    }

    [Fact]
    public void ProcessingServiceThicknessUnchangedWhenNotTwo()
    {
        FreeHand fh = new FreeHand(new() { new(0, 0), new(1, 1) }, Color.Black, 3, "u1");
        string json = CanvasSerializer.SerializeShapeManual(fh);
        string processed = ProcessingService.RegularizeShape(json);
        Assert.DoesNotContain("\"Thickness\": 5", processed); // original did not have 2 so replacement should not occur
    }

    [Fact]
    public void ShapeFactoryCreatesNonDeleted()
    {
        IShape shape = ShapeFactory.CreateShape(ShapeType.RECTANGLE, new() { new(0, 0), new(5, 5) }, Color.Purple, 2, "u1");
        Assert.False(shape.IsDeleted);
        Assert.False(string.IsNullOrWhiteSpace(shape.ShapeId));
    }

    [Fact]
    public void VisitorOutputsContainPrefix()
    {
        TestVisitor visitor = new TestVisitor();
        FreeHand fh = new FreeHand(new() { new(0, 0) }, Color.Black, 1, "u1");
        string result = fh.Accept(visitor);
        Assert.StartsWith("FreeHand:", result);
    }
}
