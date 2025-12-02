/*
 * Unit tests for CloudMessageService
 * Target: 90%+ line and branch coverage
 */
using System;
using System.Text.Json;
using Communicator.App.Services;
using Moq;
using Xunit;

namespace Communicator.App.Tests.Unit.Services;

public sealed class CloudMessageServiceTests : IDisposable
{
    private readonly Mock<ICloudConfigService> _mockCloudConfig;
    private readonly CloudMessageService _service;

    public CloudMessageServiceTests()
    {
        _mockCloudConfig = new Mock<ICloudConfigService>();
        _mockCloudConfig.Setup(c => c.NegotiateUrl).Returns(new Uri("https://example.com/negotiate"));
        _mockCloudConfig.Setup(c => c.JoinGroupUrl).Returns(new Uri("https://example.com/joinGroup"));
        _mockCloudConfig.Setup(c => c.LeaveGroupUrl).Returns(new Uri("https://example.com/leaveGroup"));
        _mockCloudConfig.Setup(c => c.MessageUrl).Returns(new Uri("https://example.com/message"));

        _service = new CloudMessageService(_mockCloudConfig.Object);
    }

    [Fact]
    public void ConstructorWithNullCloudConfigThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CloudMessageService(null!));
    }

    [Fact]
    public void ConstructorInitializesWithNotConnected()
    {
        Assert.False(_service.IsConnected);
    }

    [Fact]
    public async Task ConnectAsyncWithEmptyUsernameThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ConnectAsync("meeting-id", ""));
    }

    [Fact]
    public async Task ConnectAsyncWithWhitespaceUsernameThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ConnectAsync("meeting-id", "   "));
    }

    [Fact]
    public async Task SendMessageAsyncWhenNotConnectedThrowsInvalidOperationException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SendMessageAsync(CloudMessageType.QuickDoubt, "meeting", "user", "message"));
    }

    [Fact]
    public async Task DisconnectAsyncWhenNotConnectedDoesNotThrow()
    {
        // Should complete without throwing
        await _service.DisconnectAsync();
    }

    [Fact]
    public void DisposeCanBeCalledMultipleTimes()
    {
        _service.Dispose();
        _service.Dispose(); // Should not throw
    }

    [Fact]
    public void IsConnectedWhenHubConnectionNullReturnsFalse()
    {
        Assert.False(_service.IsConnected);
    }

    public void Dispose()
    {
        _service.Dispose();
    }
}
