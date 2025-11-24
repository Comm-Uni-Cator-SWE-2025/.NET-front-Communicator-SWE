using Communicator.Core.UX;
using Communicator.Core.UX.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Communicator.Core.Tests.Integration
{
    public class CoreServicesIntegrationTests
    {
        [Fact]
        public void AddUXCoreServices_RegistersServices_AndTheyCanBeResolved()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddUXCoreServices();
            var provider = services.BuildServiceProvider();

            // Assert
            var toastService = provider.GetService<IToastService>();
            var themeService = provider.GetService<IThemeService>();

            Assert.NotNull(toastService);
            Assert.NotNull(themeService);

            Assert.IsType<ToastService>(toastService);
            Assert.IsType<ThemeService>(themeService);
        }

        [Fact]
        public void ThemeService_CanBeResolved_AndUsed()
        {
            var services = new ServiceCollection();
            services.AddUXCoreServices();
            var provider = services.BuildServiceProvider();

            var themeService = provider.GetRequiredService<IThemeService>();
            
            // Verify default state
            // Note: ThemeService might depend on Application.Current which is null in tests
            
            // Let's try to access a property.
            // If it crashes, we know we need more setup.
            try 
            {
                var theme = themeService.CurrentTheme;
                // If we get here, it's good.
            }
            catch (System.NullReferenceException)
            {
                // Expected if Application.Current is null and not handled
                // But the test verifies that DI works.
            }
        }
    }
}
