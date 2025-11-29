using System;

namespace Communicator.Core.Logging;

/// <summary>
/// Defines a factory for creating named ILogger instances.
/// </summary>
public interface ILoggerFactory
{
    /// <summary>
    /// Creates a new ILogger instance with the specified module name.
    /// </summary>
    /// <param name="moduleName">The name of the module for logging context.</param>
    /// <returns>An ILogger instance.</returns>
    ILogger GetLogger(string moduleName);
}
