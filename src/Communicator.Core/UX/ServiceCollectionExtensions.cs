using Microsoft.Extensions.DependencyInjection;
using Communicator.Core.UX.Services;

namespace Communicator.Core.UX;

/// <summary>
/// Extension methods for registering Communicator.Core.UX services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Communicator.Core.UX services (ToastService, ThemeService) as singletons.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUXCoreServices(this IServiceCollection services)
    {
        // Register core services as singletons (single instance for app lifetime)
        services.AddSingleton<IToastService, ToastService>();
        services.AddSingleton<IThemeService, ThemeService>();

        return services;
    }
}
