using System.Drawing;
using System.Text.Json.Nodes;
using Communicator.Canvas;

namespace Communicator.Canvas.Tests;

public class SerializerTests
{
    [Fact]
    public void SerializeDeserializeShape()
    {
        RectangleShape shape = new RectangleShape(new() { new(0, 0), new(10, 20) }, Color.Green, 2, "u1");
        string json = CanvasSerializer.SerializeShapeManual(shape);
        IShape? back = CanvasSerializer.DeserializeShapeManual(json);
        Assert.NotNull(back);
        Assert.Equal(shape.ShapeId, back!.ShapeId);
        Assert.Equal(shape.Type, back.Type);
        Assert.Equal(shape.Points.Count, back.Points.Count);
    }

    [Fact]
    public void SerializeDeserializeAction()
    {
        FreeHand shape = new FreeHand(new() { new(0, 0), new(1, 1) }, Color.Red, 2, "u1");
        CanvasAction action = new CanvasAction(CanvasActionType.Create, null, shape);
        string json = CanvasSerializer.SerializeActionManual(action);
        CanvasAction? back = CanvasSerializer.DeserializeActionManual(json);
        Assert.NotNull(back);
        Assert.Equal(action.ActionId, back!.ActionId);
        Assert.Equal(CanvasActionType.Create, back.ActionType);
    }

    [Fact]
    public void SerializeDeserializeActionStack()
    {
        FreeHand shape = new FreeHand(new() { new(0, 0), new(1, 1) }, Color.Red, 2, "u1");
        SerializedActionStack stack = new SerializedActionStack();
        stack.AllActions.Add(new CanvasAction(CanvasActionType.Initial, null, null));
        stack.AllActions.Add(new CanvasAction(CanvasActionType.Create, null, shape));
        stack.CurrentIndex = 1;
        string json = CanvasSerializer.SerializeActionStack(stack);
        SerializedActionStack? back = CanvasSerializer.DeserializeActionStack(json);
        Assert.NotNull(back);
        Assert.Equal(stack.CurrentIndex, back!.CurrentIndex);
        Assert.Equal(2, back.AllActions.Count);
    }

    [Fact]
    public void SerializeDeserializeShapesDictionary()
    {
        Dictionary<string, IShape> dict = new Dictionary<string, IShape>();
        RectangleShape r = new RectangleShape(new() { new(0, 0), new(5, 5) }, Color.Blue, 2, "u1");
        dict[r.ShapeId] = r;
        string json = CanvasSerializer.SerializeShapesDictionary(dict);
        Dictionary<string, IShape> back = CanvasSerializer.DeserializeShapesDictionary(json);
        Assert.Single(back);
        Assert.True(back.ContainsKey(r.ShapeId));
    }

    [Fact]
    public void SerializeDeserializeNetworkMessage()
    {
        FreeHand shape = new FreeHand(new() { new(0, 0), new(1, 1) }, Color.Red, 2, "u1");
        CanvasAction act = new CanvasAction(CanvasActionType.Create, null, shape);
        NetworkMessage msg = new NetworkMessage(NetworkMessageType.NORMAL, act, "payload");
        string json = CanvasSerializer.SerializeNetworkMessage(msg);
        NetworkMessage? back = CanvasSerializer.DeserializeNetworkMessage(json);
        Assert.NotNull(back);
        Assert.Equal(NetworkMessageType.NORMAL, back!.MessageType);
        Assert.Equal(act.ActionId, back.Action!.ActionId);
        Assert.Equal("payload", back.Payload);
    }

    [Fact]
    public void BytesRoundTrip()
    {
        string json = "{\"A\":1}";
        byte[] bytes = CanvasSerializer.JsonStringToBytes(json);
        string str = CanvasSerializer.BytesToJsonString(bytes);
        Assert.Equal(json, str);
    }
}
