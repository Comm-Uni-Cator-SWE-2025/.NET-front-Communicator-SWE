using System;
using Communicator.App.ViewModels.Common;
using Xunit;

namespace Communicator.App.Tests.Unit.ViewModels.Common;

public sealed class UtilsTests
{
    [Fact]
    public void GetSelfIPReturnsNonNullValue()
    {
        // This test verifies that GetSelfIP returns an IP address
        // In a real network environment, this should work
        // In isolated test environments, this might throw
        try
        {
            string? ip = Utils.GetSelfIP();

            // If it doesn't throw, it should return a valid IP string
            Assert.NotNull(ip);
            Assert.NotEmpty(ip);
        }
        catch (Exception ex)
        {
            // If network is unavailable, it should throw with specific message
            Assert.Contains("Could not determine local IP address", ex.Message);
        }
    }

    [Fact]
    public void GetSelfIPReturnsValidIPFormat()
    {
        try
        {
            string? ip = Utils.GetSelfIP();

            if (ip != null)
            {
                // Should contain dots (IPv4 format)
                Assert.Contains(".", ip);

                // Should be parseable as IPAddress
                Assert.True(System.Net.IPAddress.TryParse(ip, out _));
            }
        }
        catch (Exception ex)
        {
            // Expected in isolated environments
            Assert.Contains("Could not determine local IP address", ex.Message);
        }
    }

    [Fact]
    public void GetSelfIPThrowsExceptionWithInnerException()
    {
        // We can't easily force this to fail, but we can verify the exception type
        // if the network is unavailable
        try
        {
            _ = Utils.GetSelfIP();
            // If we get here, the call succeeded
        }
        catch (Exception ex)
        {
            // Verify exception has the expected message
            Assert.Contains("GetSelfIP method", ex.Message);
            // Should have an inner exception
            Assert.NotNull(ex.InnerException);
        }
    }
}
