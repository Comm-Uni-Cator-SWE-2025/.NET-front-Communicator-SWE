using Microsoft.Extensions.DependencyInjection;
using Communicator.Controller.Logging;

namespace Communicator.Controller;

/// <summary>
/// Extension methods for registering Communicator.Controller services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers core services (Logger, etc.) in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddControllerServices(this IServiceCollection services)
    {
        services.AddSingleton<ILoggerFactory, LoggerFactory>();
        return services;
    }
}
