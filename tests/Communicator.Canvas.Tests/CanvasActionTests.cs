using System.Drawing;
using Communicator.Canvas;

namespace Communicator.Canvas.Tests;

public class CanvasActionTests
{
    [Fact]
    public void Constructor_GeneratesActionId()
    {
        var shape = new FreeHand(new() { new(0, 0) }, Color.Black, 1, "u1");
        var action = new CanvasAction(CanvasActionType.Create, null, shape);
        Assert.False(string.IsNullOrWhiteSpace(action.ActionId));
    }

    [Fact]
    public void Constructor_WithProvidedId_UsesIt()
    {
        var shape = new FreeHand(new() { new(0, 0) }, Color.Black, 1, "u1");
        string id = Guid.NewGuid().ToString();
        var action = new CanvasAction(id, CanvasActionType.Modify, shape, shape);
        Assert.Equal(id, action.ActionId);
        Assert.Equal(CanvasActionType.Modify, action.ActionType);
    }
}
