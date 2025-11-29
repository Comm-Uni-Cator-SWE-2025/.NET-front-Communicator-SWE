using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Communicator.UX.Canvas.ViewModels;
using Communicator.Controller.Meeting;
using Communicator.Core.RPC;
using Communicator.UX.Core.Services;
using Communicator.Canvas;
using Communicator.Controller.Serialization;
using System.Collections.Generic;
using System.Drawing;
using System;

namespace Communicator.Canvas.Tests;

[TestClass]
public class HostViewModelTests
{
    private HostViewModel _hostVm;

    [TestInitialize]
    public void Setup()
    {
        // FIX: Set the required environment variable
        Environment.SetEnvironmentVariable("CLOUD_BASE_URL", "http://localhost:8080");

        var user = new UserProfile { DisplayName = "HostUser" };
        _hostVm = new HostViewModel(user, new Mock<IRPC>().Object, new Mock<IRpcEventService>().Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Clean up env var
        Environment.SetEnvironmentVariable("CLOUD_BASE_URL", null);
    }

    [TestMethod]
    public void Constructor_SetsIsHost_True()
    {
        Assert.IsTrue(_hostVm.IsHost);
    }

    [TestMethod]
    public void ProcessIncomingMessage_DeleteNonExistentShape_DoesNotUpdateState()
    {
        var shapeToDelete = new RectangleShape(new List<Point>(), Color.Red, 1, "u1");
        var action = new CanvasAction(CanvasActionType.Delete, shapeToDelete, null);
        var msg = new NetworkMessage(NetworkMessageType.NORMAL, action);
        string json = CanvasSerializer.SerializeNetworkMessage(msg);

        _hostVm.ProcessIncomingMessage(json);

        Assert.AreEqual(0, _hostVm._shapes.Count);
    }

    [TestMethod]
    public void ApplyRestore_ValidJson_UpdatesHostShapes()
    {
        var shape = new FreeHand(new List<Point> { new(10, 10) }, Color.Black, 1, "u1");
        var dict = new Dictionary<string, IShape> { { shape.ShapeId, shape } };
        string json = CanvasSerializer.SerializeShapesDictionary(dict);

        _hostVm.ApplyRestore(json);

        Assert.AreEqual(1, _hostVm._shapes.Count);
        Assert.IsTrue(_hostVm._shapes.ContainsKey(shape.ShapeId));
    }

    [TestMethod]
    public void ProcessIncomingMessage_ModifyWithVersionMismatch_IsRejected()
    {
        var shapeId = "s1";
        var original = new RectangleShape(new List<Point>(), Color.Red, 1, "u1");
        _hostVm._shapes[shapeId] = original;

        var incomingPrev = new RectangleShape(new List<Point>(), Color.Red, 1, "u2");
        var incomingNew = new RectangleShape(new List<Point>(), Color.Blue, 1, "u2");

        var action = new CanvasAction(CanvasActionType.Modify, incomingPrev, incomingNew);
        var msg = new NetworkMessage(NetworkMessageType.NORMAL, action);
        string json = CanvasSerializer.SerializeNetworkMessage(msg);

        _hostVm.ProcessIncomingMessage(json);

        Assert.AreEqual(Color.Red, _hostVm._shapes[shapeId].Color);
    }

    [TestMethod]
    public void ProcessIncomingMessage_Create_UpdatesHostState()
    {
        var shape = new RectangleShape(new List<Point>(), Color.Green, 1, "u1");
        var action = new CanvasAction(CanvasActionType.Create, null, shape);
        var msg = new NetworkMessage(NetworkMessageType.NORMAL, action);
        string json = CanvasSerializer.SerializeNetworkMessage(msg);

        _hostVm.ProcessIncomingMessage(json);

        Assert.IsTrue(_hostVm._shapes.ContainsKey(shape.ShapeId));
    }
}
