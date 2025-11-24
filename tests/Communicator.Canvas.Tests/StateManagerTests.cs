using Communicator.Canvas;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Communicator.Canvas.Tests;

[TestClass]
public class StateManagerTests
{
    [TestMethod]
    public void AddAction_AdvancesCurrentIndex()
    {
        var mgr = new StateManager();
        var shape = new FreeHand(new List<Point> { new(0, 0), new(1, 1) }, Color.Red, 2, "u1");
        var action = new CanvasAction(CanvasActionType.Create, null, shape);

        mgr.AddAction(action);
        SerializedActionStack exported = mgr.ExportState();

        Assert.AreEqual(1, exported.CurrentIndex);
        Assert.AreEqual(action.ActionId, exported.AllActions[1].ActionId);
    }

    [TestMethod]
    public void Undo_ThenRedo_RestoresState()
    {
        var mgr = new StateManager();
        var s1 = new FreeHand(new List<Point> { new(0, 0), new(5, 5) }, Color.Blue, 2, "u1");
        var a1 = new CanvasAction(CanvasActionType.Create, null, s1);
        mgr.AddAction(a1);

        var s2 = s1.WithUpdates(Color.Green, null, "u2");
        var a2 = new CanvasAction(CanvasActionType.Modify, s1, s2);
        mgr.AddAction(a2);

        CanvasAction? undone = mgr.Undo();
        Assert.AreEqual(a2.ActionId, undone!.ActionId);
        Assert.AreEqual(CanvasActionType.Modify, undone.ActionType);

        CanvasAction? redone = mgr.Redo();
        Assert.AreEqual(a2.ActionId, redone!.ActionId);
    }

    [TestMethod]
    public void Undo_AtInitialState_ReturnsNull()
    {
        var mgr = new StateManager();
        Assert.IsNull(mgr.Undo());
    }

    [TestMethod]
    public void Redo_AtEndOfStack_ReturnsNull()
    {
        var mgr = new StateManager();
        var s1 = new FreeHand(new List<Point> { new(0, 0) }, Color.Blue, 2, "u1");
        mgr.AddAction(new CanvasAction(CanvasActionType.Create, null, s1));

        Assert.IsNull(mgr.Redo());
    }

    [TestMethod]
    public void PeekUndo_ReturnsCurrentActionWithoutMoving()
    {
        var mgr = new StateManager();
        var s1 = new FreeHand(new List<Point> { new(0, 0) }, Color.Blue, 2, "u1");
        var a1 = new CanvasAction(CanvasActionType.Create, null, s1);
        mgr.AddAction(a1);

        Assert.AreEqual(a1.ActionId, mgr.PeekUndo()!.ActionId);
        // Verify we can still undo it
        Assert.IsNotNull(mgr.Undo());
    }

    [TestMethod]
    public void PeekUndo_EmptyHistory_ReturnsNull()
    {
        var mgr = new StateManager();
        Assert.IsNull(mgr.PeekUndo());
    }

    [TestMethod]
    public void PeekRedo_PeeksNextActionWithoutMoving()
    {
        var mgr = new StateManager();
        var s1 = new FreeHand(new List<Point> { new(0, 0) }, Color.Blue, 2, "u1");
        var a1 = new CanvasAction(CanvasActionType.Create, null, s1);
        mgr.AddAction(a1);
        mgr.Undo();

        Assert.AreEqual(a1.ActionId, mgr.PeekRedo()!.ActionId);
        // Verify we can still redo it
        Assert.IsNotNull(mgr.Redo());
    }

    [TestMethod]
    public void PeekRedo_EmptyHistory_ReturnsNull()
    {
        var mgr = new StateManager();
        Assert.IsNull(mgr.PeekRedo());
    }

    [TestMethod]
    public void ImportState_RoundTrip_PreservesData()
    {
        var mgr = new StateManager();
        var s1 = new FreeHand(new List<Point> { new(0, 0) }, Color.Blue, 2, "u1");
        var a1 = new CanvasAction(CanvasActionType.Create, null, s1);
        mgr.AddAction(a1);

        SerializedActionStack dto = mgr.ExportState();

        var mgr2 = new StateManager();
        mgr2.ImportState(dto);
        SerializedActionStack dto2 = mgr2.ExportState();

        Assert.AreEqual(dto.CurrentIndex, dto2.CurrentIndex);
        Assert.AreEqual(dto.AllActions.Count, dto2.AllActions.Count);
        Assert.AreEqual(dto.AllActions[1].ActionId, dto2.AllActions[1].ActionId);
    }

    [TestMethod]
    public void ImportState_EmptyStack_ResetsManager()
    {
        var mgr = new StateManager();
        // Add dummy action to ensure it gets cleared
        var s1 = new FreeHand(new List<Point> { new(0, 0) }, Color.Black, 1, "u");
        mgr.AddAction(new CanvasAction(CanvasActionType.Create, null, s1));

        SerializedActionStack empty = new SerializedActionStack();
        mgr.ImportState(empty);

        Assert.IsNull(mgr.Undo());
        Assert.IsNull(mgr.Redo());
    }
}
