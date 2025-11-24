using Communicator.Canvas;

namespace Communicator.Canvas.Tests;

public class NetworkMockTests
{
    [Fact]
    public void RegisterAndSendMessageCallsListener()
    {
        string received = string.Empty;
        NetworkMock.Register("1.1.1.1", msg => received = msg);
        NetworkMock.SendMessage("1.1.1.1", "hello");
        Assert.Equal("hello", received);
    }

    [Fact]
    public void BroadcastCallsAllListeners()
    {
        List<string> messages = new List<string>();
        NetworkMock.Register("1", m => messages.Add("1:" + m));
        NetworkMock.Register("2", m => messages.Add("2:" + m));
        NetworkMock.Broadcast(new List<string> { "1", "2" }, "test");
        Assert.Equal(2, messages.Count);
        Assert.Contains(messages, m => m == "1:test");
        Assert.Contains(messages, m => m == "2:test");
    }
}
