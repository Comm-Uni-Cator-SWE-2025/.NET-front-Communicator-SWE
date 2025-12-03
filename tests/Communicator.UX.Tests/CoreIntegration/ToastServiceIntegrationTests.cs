using System;
using Communicator.UX.Core;
using Communicator.UX.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Communicator.Core.Tests.Integration
{
    public class ToastServiceIntegrationTests
    {
        [Fact]
        public void ShowSuccess_Raises_ToastRequested_Event()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddUXCoreServices();
            var provider = services.BuildServiceProvider();

            var toastService = provider.GetRequiredService<IToastService>();
            bool eventRaised = false;
            string? receivedMessage = null;

            toastService.ToastRequested += (s, e) =>
            {
                eventRaised = true;
                receivedMessage = e.Message.Message;
            };

            // Act
            string expectedMessage = "Integration Test Success";
            toastService.ShowSuccess(expectedMessage);

            // Assert
            Assert.True(eventRaised);
            Assert.Equal(expectedMessage, receivedMessage);
        }
    }
}
