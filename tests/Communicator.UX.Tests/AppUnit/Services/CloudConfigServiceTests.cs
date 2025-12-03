using System;
using Communicator.App.Services;
using Xunit;

namespace Communicator.App.Tests.Unit.Services
{
    public class CloudConfigServiceTests
    {
        [Fact]
        public void Properties_ReturnValuesFromEnvironment()
        {
            // Environment variables should already be loaded by TestStartup via EnvLoader
            var service = new CloudConfigService();

            // Verify that the service can read the URLs (they should be set from .env)
            Assert.NotNull(service.NegotiateUrl);
            Assert.NotNull(service.MessageUrl);
            Assert.NotNull(service.JoinGroupUrl);
            Assert.NotNull(service.LeaveGroupUrl);
            Assert.Contains("negotiate", service.NegotiateUrl.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Properties_ThrowIfEnvVarMissing()
        {
            // Temporarily unset the env var to test error case
            string? original = Environment.GetEnvironmentVariable("SIGNALR_NEGOTIATE_URL");
            try
            {
                Environment.SetEnvironmentVariable("SIGNALR_NEGOTIATE_URL", null);
                var service = new CloudConfigService();
                Assert.Throws<InvalidOperationException>(() => service.NegotiateUrl);
            }
            finally
            {
                // Restore
                Environment.SetEnvironmentVariable("SIGNALR_NEGOTIATE_URL", original);
            }
        }
    }
}
