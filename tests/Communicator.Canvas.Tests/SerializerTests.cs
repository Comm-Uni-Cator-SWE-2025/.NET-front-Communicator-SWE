using Communicator.Canvas;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Communicator.Canvas.Tests;

[TestClass]
public class SerializerTests
{
    [TestMethod]
    public void SerializeShapeManual_RoundTrip_PreservesData()
    {
        var shape = new RectangleShape(new List<Point> { new(0, 0), new(10, 20) }, Color.Green, 2, "u1");
        string json = CanvasSerializer.SerializeShapeManual(shape);
        IShape? back = CanvasSerializer.DeserializeShapeManual(json);

        Assert.IsNotNull(back);
        Assert.AreEqual(shape.ShapeId, back!.ShapeId);
        Assert.AreEqual(shape.Type, back.Type);
        Assert.AreEqual(shape.Points.Count, back.Points.Count);
    }

    [TestMethod]
    public void DeserializeShapeManual_InvalidJson_ReturnsNull()
    {
        IShape? shape = CanvasSerializer.DeserializeShapeManual("{ \"Type\": \"INVALID\" }");
        Assert.IsNull(shape);
    }

    [TestMethod]
    public void SerializeActionManual_RoundTrip_PreservesData()
    {
        var shape = new FreeHand(new List<Point> { new(0, 0), new(1, 1) }, Color.Red, 2, "u1");
        var action = new CanvasAction(CanvasActionType.Create, null, shape);

        string json = CanvasSerializer.SerializeActionManual(action);
        CanvasAction? back = CanvasSerializer.DeserializeActionManual(json);

        Assert.IsNotNull(back);
        Assert.AreEqual(action.ActionId, back!.ActionId);
        Assert.AreEqual(CanvasActionType.Create, back.ActionType);
    }

    [TestMethod]
    public void DeserializeActionManual_InvalidJson_ReturnsNull()
    {
        CanvasAction? action = CanvasSerializer.DeserializeActionManual("{ \"ActionType\": \"INVALID\" }");
        Assert.IsNull(action);
    }

    [TestMethod]
    public void SerializeActionStack_RoundTrip_PreservesStructure()
    {
        var shape = new FreeHand(new List<Point> { new(0, 0), new(1, 1) }, Color.Red, 2, "u1");
        var stack = new SerializedActionStack();
        stack.AllActions.Add(new CanvasAction(CanvasActionType.Initial, null, null));
        stack.AllActions.Add(new CanvasAction(CanvasActionType.Create, null, shape));
        stack.CurrentIndex = 1;

        string json = CanvasSerializer.SerializeActionStack(stack);
        SerializedActionStack? back = CanvasSerializer.DeserializeActionStack(json);

        Assert.IsNotNull(back);
        Assert.AreEqual(stack.CurrentIndex, back!.CurrentIndex);
        Assert.AreEqual(2, back.AllActions.Count);
    }

    [TestMethod]
    public void SerializeShapesDictionary_RoundTrip_PreservesKeysAndValues()
    {
        var dict = new Dictionary<string, IShape>();
        var r = new RectangleShape(new List<Point> { new(0, 0), new(5, 5) }, Color.Blue, 2, "u1");
        dict[r.ShapeId] = r;

        string json = CanvasSerializer.SerializeShapesDictionary(dict);
        Dictionary<string, IShape> back = CanvasSerializer.DeserializeShapesDictionary(json);

        Assert.AreEqual(1, back.Count);
        Assert.IsTrue(back.ContainsKey(r.ShapeId));
    }

    [TestMethod]
    public void SerializeNetworkMessage_RoundTrip_PreservesData()
    {
        var shape = new FreeHand(new List<Point> { new(0, 0), new(1, 1) }, Color.Red, 2, "u1");
        var act = new CanvasAction(CanvasActionType.Create, null, shape);
        var msg = new NetworkMessage(NetworkMessageType.NORMAL, act, "payload");

        string json = CanvasSerializer.SerializeNetworkMessage(msg);
        NetworkMessage? back = CanvasSerializer.DeserializeNetworkMessage(json);

        Assert.IsNotNull(back);
        Assert.AreEqual(NetworkMessageType.NORMAL, back!.MessageType);
        Assert.AreEqual(act.ActionId, back.Action!.ActionId);
        Assert.AreEqual("payload", back.Payload);
    }

    [TestMethod]
    public void JsonStringToBytes_RoundTrip_ReturnsOriginalString()
    {
        string json = "{\"A\":1}";
        byte[] bytes = CanvasSerializer.JsonStringToBytes(json);
        string str = CanvasSerializer.BytesToJsonString(bytes);
        Assert.AreEqual(json, str);
    }
}
