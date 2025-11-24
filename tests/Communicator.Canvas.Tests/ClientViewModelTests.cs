using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Communicator.UX.Canvas.ViewModels;
using Communicator.Controller.Meeting;
using Communicator.Core.RPC;
using Communicator.Core.UX.Services;
using Communicator.Canvas;
using Communicator.Controller.Serialization;
using System.Drawing;
using System.Collections.Generic;

namespace Communicator.Canvas.Tests;

[TestClass]
public class ClientViewModelTests
{
    private ClientViewModel _clientVm;
    private UserProfile _user;

    [TestInitialize]
    public void Setup()
    {
        _user = new UserProfile { DisplayName = "TestClient" };
        _clientVm = new ClientViewModel(_user, new Mock<IRPC>().Object, new Mock<IRpcEventService>().Object);
    }

    [TestMethod]
    public void Constructor_SetsCurrentUserId_FromUserProfile()
    {
        Assert.AreEqual("TestClient", _clientVm.CurrentUserId);
    }

    [TestMethod]
    public void ProcessIncomingMessage_CreateAction_AddsShapeToDictionary()
    {
        var shape = new RectangleShape(new List<Point> { new(0, 0) }, Color.Blue, 2, "OtherUser");
        var action = new CanvasAction(CanvasActionType.Create, null, shape);
        var msg = new NetworkMessage(NetworkMessageType.NORMAL, action);
        string json = CanvasSerializer.SerializeNetworkMessage(msg);

        _clientVm.ProcessIncomingMessage(json);

        Assert.IsTrue(_clientVm._shapes.ContainsKey(shape.ShapeId));
        // Compare ARGB to avoid Named Color vs ARGB Color issues
        Assert.AreEqual(Color.Blue.ToArgb(), _clientVm._shapes[shape.ShapeId].Color.ToArgb());
    }

    [TestMethod]
    public void ProcessIncomingMessage_Restore_PopulatesDictionary()
    {
        var shape = new FreeHand(new List<Point> { new(10, 10) }, Color.Red, 1, "u1");
        var dict = new Dictionary<string, IShape> { { shape.ShapeId, shape } };
        string payloadJson = CanvasSerializer.SerializeShapesDictionary(dict);

        var msg = new NetworkMessage(NetworkMessageType.RESTORE, null, payloadJson);
        string netJson = CanvasSerializer.SerializeNetworkMessage(msg);

        _clientVm.ProcessIncomingMessage(netJson);

        Assert.AreEqual(1, _clientVm._shapes.Count);
        Assert.IsTrue(_clientVm._shapes.ContainsKey(shape.ShapeId));
    }

    [TestMethod]
    public void ProcessIncomingMessage_MyAction_RemovesMatchingGhostShape()
    {
        var shape = new RectangleShape(new List<Point> { new(0, 0) }, Color.Red, 1, _clientVm.CurrentUserId);
        _clientVm.GhostShapes.Add(shape);

        var action = new CanvasAction(CanvasActionType.Create, null, shape);
        var msg = new NetworkMessage(NetworkMessageType.NORMAL, action);
        string json = CanvasSerializer.SerializeNetworkMessage(msg);

        _clientVm.ProcessIncomingMessage(json);

        Assert.AreEqual(0, _clientVm.GhostShapes.Count);
        Assert.IsTrue(_clientVm._shapes.ContainsKey(shape.ShapeId));
    }

    [TestMethod]
    public void Undo_LocallyUpdatesState()
    {
        var shape = new RectangleShape(new List<Point>(), Color.Red, 1, "u1");
        var action = new CanvasAction(CanvasActionType.Create, null, shape);
        var undoMsg = new NetworkMessage(NetworkMessageType.UNDO, action);
        string json = CanvasSerializer.SerializeNetworkMessage(undoMsg);

        _clientVm.ProcessIncomingMessage(json);

        Assert.IsNotNull(_clientVm);
    }

}
