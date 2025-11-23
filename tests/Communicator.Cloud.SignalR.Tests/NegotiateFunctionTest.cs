/******************************************************************************
* Filename    = MessageSignalRTests.cs
* Author      = Nikhil S Thomas
* Product     = Comm-Uni-Cator
* Project     = SignalR Function App
* Description = Unit test for NegotiateFunction Azure Function.
*****************************************************************************/

using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Communicator.Cloud.SignalR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.SignalRService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Communicator.Cloud.SignalR.Tests;

/// <summary>
/// Class containing unit tests for the NegotiateFunction Azure Function.
/// </summary>
public class NegotiateFunctionTests
{
    // Mock objects and function instance
    private readonly Mock<ILogger<NegotiateFunction>> _mockLogger;
    private readonly Mock<FunctionContext> _mockContext;
    private readonly Mock<HttpRequestData> _mockRequest;
    private readonly NegotiateFunction _function;

    /// <summary>
    /// Constructor to set up the test environment.
    /// </summary>
    public NegotiateFunctionTests()
    {
        _mockLogger = new Mock<ILogger<NegotiateFunction>>();
        _mockContext = new Mock<FunctionContext>();

        IHost host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .Build();

        IServiceProvider serviceProvider = host.Services;
        _mockContext.Setup(c => c.InstanceServices).Returns(serviceProvider);

        _mockRequest = new Mock<HttpRequestData>(_mockContext.Object);
        _function = new NegotiateFunction(_mockLogger.Object);
    }

    /// <summary>
    /// Test for the NegotiateFunction Run method.
    /// </summary>
    [Fact]
    public async Task NegotitateFunctionTest()
    {
        var fakeConnectionInfo = new SignalRConnectionInfo {
            Url = "https://fake.service.com",
            AccessToken = "fake_access_token"
        };

        var query = new NameValueCollection {
            { "meetingId", "Test123" }
        };
        _mockRequest.Setup(r => r.Query).Returns(query);

        _mockRequest.Setup(r => r.Url)
            .Returns(new Uri("https://localhost/api/negotiate?meetingId=Test123"));
        var responseStream = new MemoryStream();
        var mockResponse = new Mock<HttpResponseData>(_mockContext.Object);
        mockResponse.Setup(r => r.Body).Returns(responseStream);
        mockResponse.Setup(r => r.Headers).Returns(new HttpHeadersCollection());
        mockResponse.SetupProperty(r => r.StatusCode);

        _mockRequest.Setup(r => r.CreateResponse())
                    .Returns(mockResponse.Object);

        NegotiateFunction.NegotiateResponse result = await _function.Run(_mockRequest.Object, fakeConnectionInfo);

        Assert.Equal(HttpStatusCode.OK, result.HttpResponse!.StatusCode);
        HttpResponseData httpResponse = result.HttpResponse!;

        httpResponse.Body.Position = 0;
        using var reader = new StreamReader(httpResponse.Body);
        string responseBody = await reader.ReadToEndAsync();

        Dictionary<string, JsonElement> json = 
            JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseBody)!;

        // Validate connection info fields
        Assert.Equal(fakeConnectionInfo.Url, json["url"].GetString());
        Assert.Equal(fakeConnectionInfo.AccessToken, json["accessToken"].GetString());

        // Validate meetingId
        Assert.Equal("Test123", json["meetingId"].GetString());

        _mockLogger.Verify(
            log => log.Log(
                It.Is<LogLevel>(level => level == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Negotiation request received.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }
}
