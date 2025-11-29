using Microsoft.Extensions.DependencyInjection;
using Communicator.Core.Logging;

namespace Communicator.Core;

/// <summary>
/// Extension methods for registering Communicator.Core services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers core services (Logger, etc.) in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<ILoggerFactory, LoggerFactory>();
        return services;
    }
}
