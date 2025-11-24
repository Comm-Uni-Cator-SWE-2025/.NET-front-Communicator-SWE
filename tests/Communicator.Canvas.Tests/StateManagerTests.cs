using System.Drawing;
using Communicator.Canvas;

namespace Communicator.Canvas.Tests;

public class StateManagerTests
{
    [Fact]
    public void AddActionShouldAdvanceCurrent()
    {
        StateManager mgr = new StateManager();
        FreeHand shape = new FreeHand(new List<Point> { new(0, 0), new(1, 1) }, Color.Red, 2, "u1");
        CanvasAction action = new CanvasAction(CanvasActionType.Create, null, shape);

        mgr.AddAction(action);
        SerializedActionStack exported = mgr.ExportState();

        Assert.Equal(1, exported.CurrentIndex); // initial + action
        Assert.Equal(action.ActionId, exported.AllActions[1].ActionId);
    }

    [Fact]
    public void UndoRedoWorkflow()
    {
        StateManager mgr = new StateManager();
        FreeHand s1 = new FreeHand(new List<Point> { new(0, 0), new(5, 5) }, Color.Blue, 2, "u1");
        CanvasAction a1 = new CanvasAction(CanvasActionType.Create, null, s1);
        mgr.AddAction(a1);
        IShape s2 = s1.WithUpdates(Color.Green, null, "u2");
        CanvasAction a2 = new CanvasAction(CanvasActionType.Modify, s1, s2);
        mgr.AddAction(a2);

        CanvasAction? undone = mgr.Undo();
        Assert.Equal(a2.ActionId, undone!.ActionId);
        Assert.Equal(CanvasActionType.Modify, undone.ActionType);

        CanvasAction? redone = mgr.Redo();
        Assert.Equal(a2.ActionId, redone!.ActionId);
    }

    [Fact]
    public void UndoAtInitialReturnsNull()
    {
        StateManager mgr = new StateManager();
        Assert.Null(mgr.Undo());
    }

    [Fact]
    public void RedoAtEndReturnsNull()
    {
        StateManager mgr = new StateManager();
        FreeHand s1 = new FreeHand(new List<Point> { new(0, 0), new(5, 5) }, Color.Blue, 2, "u1");
        mgr.AddAction(new CanvasAction(CanvasActionType.Create, null, s1));
        Assert.Null(mgr.Redo());
    }

    [Fact]
    public void PeekUndoPeeksCurrentAction()
    {
        StateManager mgr = new StateManager();
        FreeHand s1 = new FreeHand(new List<Point> { new(0, 0), new(5, 5) }, Color.Blue, 2, "u1");
        CanvasAction a1 = new CanvasAction(CanvasActionType.Create, null, s1);
        mgr.AddAction(a1);
        Assert.Equal(a1.ActionId, mgr.PeekUndo()!.ActionId);
    }

    [Fact]
    public void PeekRedoPeeksNextAction()
    {
        StateManager mgr = new StateManager();
        FreeHand s1 = new FreeHand(new List<Point> { new(0, 0), new(5, 5) }, Color.Blue, 2, "u1");
        CanvasAction a1 = new CanvasAction(CanvasActionType.Create, null, s1);
        mgr.AddAction(a1);
        mgr.Undo();
        Assert.Equal(a1.ActionId, mgr.PeekRedo()!.ActionId);
    }

    [Fact]
    public void ExportImportRoundTrip()
    {
        StateManager mgr = new StateManager();
        FreeHand s1 = new FreeHand(new List<Point> { new(0, 0), new(5, 5) }, Color.Blue, 2, "u1");
        CanvasAction a1 = new CanvasAction(CanvasActionType.Create, null, s1);
        mgr.AddAction(a1);
        SerializedActionStack dto = mgr.ExportState();

        StateManager mgr2 = new StateManager();
        mgr2.ImportState(dto);
        SerializedActionStack dto2 = mgr2.ExportState();

        Assert.Equal(dto.CurrentIndex, dto2.CurrentIndex);
        Assert.Equal(dto.AllActions.Count, dto2.AllActions.Count);
        Assert.Equal(dto.AllActions[1].ActionId, dto2.AllActions[1].ActionId);
    }
}
