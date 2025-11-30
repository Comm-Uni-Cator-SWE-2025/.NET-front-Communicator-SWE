using System.Collections.Generic;
using System.Drawing;
using Communicator.Canvas;
using Communicator.Controller.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Communicator.Canvas.Tests;

[TestClass]
public class SerializerTests
{
    [TestMethod]
    public void SerializeShapeManual_RoundTrip_PreservesData()
    {
        // FIX: Explicit type instead of 'var'
        RectangleShape shape = new RectangleShape(new List<Point> { new(0, 0), new(10, 20) }, Color.Green, 2, "u1");
        string json = CanvasSerializer.SerializeShapeManual(shape);
        IShape? back = CanvasSerializer.DeserializeShapeManual(json);

        Assert.IsNotNull(back);
        Assert.AreEqual(shape.ShapeId, back!.ShapeId);
        Assert.AreEqual(shape.Type, back.Type);
        Assert.AreEqual(shape.Points.Count, back.Points.Count);
        // Verify Color Roundtrip
        Assert.AreEqual(shape.Color.ToArgb(), back.Color.ToArgb());
    }

    [TestMethod]
    public void SerializeShapeManual_AllShapeTypes_RoundTrip()
    {
        // This test covers the switch cases for TRIANGLE, ELLIPSE, and LINE which were previously missed.
        List<IShape> shapesToTest = new List<IShape>
        {
            new TriangleShape(new List<Point> { new(0,0), new(10,10) }, Color.Yellow, 1, "u1"),
            new EllipseShape(new List<Point> { new(0,0), new(20,20) }, Color.Blue, 1, "u1"),
            new StraightLine(new List<Point> { new(0,0), new(50,50) }, Color.Black, 1, "u1")
        };

        foreach (IShape shape in shapesToTest)
        {
            string json = CanvasSerializer.SerializeShapeManual(shape);
            IShape? back = CanvasSerializer.DeserializeShapeManual(json);

            Assert.IsNotNull(back, $"Failed to deserialize {shape.Type}");
            Assert.AreEqual(shape.Type, back!.Type);
            Assert.AreEqual(shape.ShapeId, back.ShapeId);
        }
    }

    [TestMethod]
    public void DeserializeShapeManual_InvalidJson_ReturnsNull()
    {
        IShape? shape = CanvasSerializer.DeserializeShapeManual("{ \"Type\": \"INVALID\" }");
        Assert.IsNull(shape);
    }

    [TestMethod]
    public void DeserializeShapeManual_EmptyJson_ReturnsNull()
    {
        IShape? shape = CanvasSerializer.DeserializeShapeManual("");
        Assert.IsNull(shape);
    }

    [TestMethod]
    public void SerializeActionManual_CreateAction_PreservesData()
    {
        // Case: Prev is Null, Next is Set
        FreeHand shape = new FreeHand(new List<Point> { new(0, 0), new(1, 1) }, Color.Red, 2, "u1");
        CanvasAction action = new CanvasAction(CanvasActionType.Create, null, shape);

        string json = CanvasSerializer.SerializeActionManual(action);
        CanvasAction? back = CanvasSerializer.DeserializeActionManual(json);

        Assert.IsNotNull(back);
        Assert.AreEqual(action.ActionId, back!.ActionId);
        Assert.AreEqual(CanvasActionType.Create, back.ActionType);
        Assert.IsNull(back.PrevShape);
        Assert.IsNotNull(back.NewShape);
    }

    [TestMethod]
    public void SerializeActionManual_DeleteAction_PreservesData()
    {
        // Case: Prev is Set, Next is Null (Covers the 'else { WriteNull("Next") }' branch)
        RectangleShape shape = new RectangleShape(new List<Point> { new(0, 0) }, Color.Blue, 1, "u1");
        CanvasAction action = new CanvasAction(CanvasActionType.Delete, shape, null);

        string json = CanvasSerializer.SerializeActionManual(action);
        CanvasAction? back = CanvasSerializer.DeserializeActionManual(json);

        Assert.IsNotNull(back);
        Assert.AreEqual(CanvasActionType.Delete, back!.ActionType);
        Assert.IsNotNull(back.PrevShape);
        Assert.IsNull(back.NewShape);
    }

    [TestMethod]
    public void SerializeActionManual_ModifyAction_PreservesData()
    {
        // Case: Both Prev and Next are Set
        RectangleShape prev = new RectangleShape(new List<Point> { new(0, 0) }, Color.Blue, 1, "u1");
        RectangleShape next = new RectangleShape(new List<Point> { new(0, 0) }, Color.Red, 1, "u1");
        CanvasAction action = new CanvasAction(CanvasActionType.Modify, prev, next);

        string json = CanvasSerializer.SerializeActionManual(action);
        CanvasAction? back = CanvasSerializer.DeserializeActionManual(json);

        Assert.IsNotNull(back);
        Assert.AreEqual(CanvasActionType.Modify, back!.ActionType);
        Assert.IsNotNull(back.PrevShape);
        Assert.IsNotNull(back.NewShape);
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
        FreeHand shape = new FreeHand(new List<Point> { new(0, 0), new(1, 1) }, Color.Red, 2, "u1");
        SerializedActionStack stack = new SerializedActionStack();
        stack.AllActions.Add(new CanvasAction(CanvasActionType.Initial, null, null));
        stack.AllActions.Add(new CanvasAction(CanvasActionType.Create, null, shape));
        stack.CurrentIndex = 1;

        string json = CanvasSerializer.SerializeActionStack(stack);
        SerializedActionStack? back = CanvasSerializer.DeserializeActionStack(json);

        Assert.IsNotNull(back);
        Assert.AreEqual(stack.CurrentIndex, back!.CurrentIndex);
        Assert.AreEqual(2, back.AllActions.Count);
        // Verify nested action deserialization
        Assert.AreEqual(CanvasActionType.Create, back.AllActions[1].ActionType);
    }

    [TestMethod]
    public void SerializeShapesDictionary_RoundTrip_PreservesKeysAndValues()
    {
        Dictionary<string, IShape> dict = new Dictionary<string, IShape>();
        RectangleShape r = new RectangleShape(new List<Point> { new(0, 0), new(5, 5) }, Color.Blue, 2, "u1");
        dict[r.ShapeId] = r;

        string json = CanvasSerializer.SerializeShapesDictionary(dict);
        Dictionary<string, IShape> back = CanvasSerializer.DeserializeShapesDictionary(json);

        Assert.AreEqual(1, back.Count);
        Assert.IsTrue(back.ContainsKey(r.ShapeId));
        Assert.AreEqual(r.Type, back[r.ShapeId].Type);
    }

    [TestMethod]
    public void SerializeNetworkMessage_RoundTrip_PreservesData()
    {
        FreeHand shape = new FreeHand(new List<Point> { new(0, 0), new(1, 1) }, Color.Red, 2, "u1");
        CanvasAction act = new CanvasAction(CanvasActionType.Create, null, shape);
        NetworkMessage msg = new NetworkMessage(NetworkMessageType.NORMAL, act, "payload");

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
