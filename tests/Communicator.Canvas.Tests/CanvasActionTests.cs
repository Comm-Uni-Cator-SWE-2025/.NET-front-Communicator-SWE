using Communicator.Canvas;
using System.Drawing;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Communicator.Canvas.Tests;

[TestClass]
public class CanvasActionTests
{
    [TestMethod]
    public void Constructor_NoId_GeneratesNewGuid()
    {
        var shape = new FreeHand(new List<Point> { new(0, 0) }, Color.Black, 1, "u1");
        var action = new CanvasAction(CanvasActionType.Create, null, shape);

        Assert.IsFalse(string.IsNullOrWhiteSpace(action.ActionId));
        Assert.IsTrue(Guid.TryParse(action.ActionId, out _));
    }

    [TestMethod]
    public void Constructor_WithId_SetsCorrectId()
    {
        var shape = new FreeHand(new List<Point> { new(0, 0) }, Color.Black, 1, "u1");
        string id = Guid.NewGuid().ToString();
        var action = new CanvasAction(id, CanvasActionType.Modify, shape, shape);

        Assert.AreEqual(id, action.ActionId);
        Assert.AreEqual(CanvasActionType.Modify, action.ActionType);
    }
}
