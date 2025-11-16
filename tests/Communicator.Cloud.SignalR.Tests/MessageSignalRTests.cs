using System.Collections.Specialized;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Communicator.Cloud.SignalR;
using Xunit;
using static Communicator.Cloud.SignalR.MessageSignalR;

namespace Communicator.Cloud.SignalR.Tests;

public class MessageSignalRTests
{
    private readonly Mock<ILogger<MessageSignalR>> _mockLogger;
    private readonly Mock<FunctionContext> _mockContext;
    private readonly Mock<HttpRequestData> _mockRequest;
    private readonly MessageSignalR _function;
    private readonly MemoryStream _responseStream;
    private readonly Mock<HttpResponseData> _mockResponse;

    public MessageSignalRTests()
    {
        _mockLogger = new Mock<ILogger<MessageSignalR>>();
        _mockContext = new Mock<FunctionContext>();

        IHost host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .Build();

        IServiceProvider serviceProvider = host.Services;
        _mockContext.Setup(c => c.InstanceServices).Returns(serviceProvider);

        _mockRequest = new Mock<HttpRequestData>(_mockContext.Object);
        _function = new MessageSignalR(_mockLogger.Object);

        _responseStream = new MemoryStream();
        _mockResponse = new Mock<HttpResponseData>(_mockContext.Object);
        _mockResponse.Setup(r => r.Body).Returns(_responseStream);
        _mockResponse.Setup(r => r.Headers).Returns(new HttpHeadersCollection());
        _mockResponse.SetupProperty(r => r.StatusCode);

        _mockRequest.Setup(r => r.CreateResponse()).Returns(_mockResponse.Object);
    }

    [Fact]
    public async Task BroadcastGivenMessageTest()
    {
        var query = new NameValueCollection { { "message", "Test Doubt" } };
        _mockRequest.Setup(r => r.Query).Returns(query);

        MessageResponse result = await _function.Run(_mockRequest.Object);

        Assert.NotNull(result.SignalRMessage);
        Assert.Equal("ReceiveDoubt", result.SignalRMessage.Target);
        Assert.Single(result.SignalRMessage.Arguments);
        Assert.Equal("Test Doubt", result.SignalRMessage.Arguments[0]);

        Assert.NotNull(result.HttpResponse);
        Assert.Equal(HttpStatusCode.OK, result.HttpResponse.StatusCode);

        _responseStream.Position = 0;
        using var reader = new StreamReader(_responseStream);
        string responseBody = await reader.ReadToEndAsync();

        Assert.Contains("ok", responseBody);
        Assert.Contains("Message sent to SignalR hub. Broadcasted: Test Doubt", responseBody);

        _mockLogger.Verify(
            log => log.Log(
                It.Is<LogLevel>(level => level == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("MessageSignalR trigger invoked.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task BroadcastDefaultMessageTest()
    {
        var query = new NameValueCollection();
        _mockRequest.Setup(r => r.Query).Returns(query);

        MessageResponse result = await _function.Run(_mockRequest.Object);

        Assert.NotNull(result.SignalRMessage);
        Assert.Equal("ReceiveDoubt", result.SignalRMessage.Target);
        Assert.Single(result.SignalRMessage.Arguments);
        Assert.Equal("New Doubt Raised!", result.SignalRMessage.Arguments[0]);

        Assert.NotNull(result.HttpResponse);
        Assert.Equal(HttpStatusCode.OK, result.HttpResponse.StatusCode);

        _responseStream.Position = 0;
        using var reader = new StreamReader(_responseStream);
        string responseBody = await reader.ReadToEndAsync();

        Assert.Contains("ok", responseBody);
        Assert.Contains("Message sent to SignalR hub. Broadcasted: New Doubt Raised!", responseBody);

        _mockLogger.Verify(
            log => log.Log(
                It.Is<LogLevel>(level => level == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("MessageSignalR trigger invoked.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }
}
