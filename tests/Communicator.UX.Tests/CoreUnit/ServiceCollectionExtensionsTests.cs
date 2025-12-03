using Communicator.UX.Core;
using Communicator.UX.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Communicator.Core.Tests.Unit;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddUXCoreServices_RegistersServices()
    {
        var services = new ServiceCollection();
        
        services.AddUXCoreServices();
        
        var provider = services.BuildServiceProvider();
        
        Assert.NotNull(provider.GetService<IToastService>());
        Assert.IsType<ToastService>(provider.GetService<IToastService>());
        
        Assert.NotNull(provider.GetService<IThemeService>());
        Assert.IsType<ThemeService>(provider.GetService<IThemeService>());
    }
}
