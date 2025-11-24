using System;
using Communicator.App.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Communicator.App.Tests.Unit.Services
{
    public class CloudConfigServiceTests
    {
        [Fact]
        public void Properties_ReturnValuesFromConfig()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(x => x["CloudFunctions:NegotiateUrl"]).Returns("http://test.com/negotiate");
            mockConfig.Setup(x => x["CloudFunctions:MessageUrl"]).Returns("http://test.com/message");
            mockConfig.Setup(x => x["CloudFunctions:JoinGroupUrl"]).Returns("http://test.com/join");
            mockConfig.Setup(x => x["CloudFunctions:LeaveGroupUrl"]).Returns("http://test.com/leave");

            var service = new CloudConfigService(mockConfig.Object);

            Assert.Equal(new Uri("http://test.com/negotiate"), service.NegotiateUrl);
            Assert.Equal(new Uri("http://test.com/message"), service.MessageUrl);
            Assert.Equal(new Uri("http://test.com/join"), service.JoinGroupUrl);
            Assert.Equal(new Uri("http://test.com/leave"), service.LeaveGroupUrl);
        }

        [Fact]
        public void Properties_ThrowIfConfigMissing()
        {
            var mockConfig = new Mock<IConfiguration>();
            // Setup to return null
            
            var service = new CloudConfigService(mockConfig.Object);

            Assert.Throws<InvalidOperationException>(() => service.NegotiateUrl);
        }
    }
}
