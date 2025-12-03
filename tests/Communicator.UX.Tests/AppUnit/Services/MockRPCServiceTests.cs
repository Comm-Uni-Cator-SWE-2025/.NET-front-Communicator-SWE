using System;
using System.Threading.Tasks;
using Communicator.App.Services;
using Xunit;

namespace Communicator.App.Tests.Unit.Services;

/// <summary>
/// Unit tests for MockRPCService.
/// Note: Connect() tests are excluded because they create infinite-loop background threads
/// </summary>
public sealed class MockRPCServiceTests
{
    [Fact]
    public async Task CallReturnsEmptyByteArray()
    {
        MockRPCService service = new MockRPCService();
        byte[] result = await service.Call("test/method", [1, 2, 3]);

        Assert.Empty(result);
    }

    [Fact]
    public async Task CallWithAnyMethodNameReturnsEmptyByteArray()
    {
        MockRPCService service = new MockRPCService();

        byte[] result1 = await service.Call("core/register", Array.Empty<byte>());
        byte[] result2 = await service.Call("core/logout", Array.Empty<byte>());
        byte[] result3 = await service.Call("any/method", [1, 2, 3, 4]);

        Assert.Empty(result1);
        Assert.Empty(result2);
        Assert.Empty(result3);
    }

    [Fact]
    public void SubscribeDoesNotThrow()
    {
        MockRPCService service = new MockRPCService();

        // Should not throw
        service.Subscribe("test/method", data => Array.Empty<byte>());
        service.Subscribe("another/method", data => [1, 2, 3]);
    }

    [Fact]
    public void SubscribeWithNullMethodNameDoesNotThrow()
    {
        MockRPCService service = new MockRPCService();

        // Should not throw - mock just logs
        service.Subscribe(null!, data => Array.Empty<byte>());
    }

    [Fact]
    public void SubscribeWithNullCallbackDoesNotThrow()
    {
        MockRPCService service = new MockRPCService();

        // Should not throw - mock just logs
        service.Subscribe("test/method", null!);
    }
}
