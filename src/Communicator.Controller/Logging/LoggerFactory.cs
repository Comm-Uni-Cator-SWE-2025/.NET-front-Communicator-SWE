using System;

namespace Communicator.Controller.Logging;

/// <summary>
/// Implementation of ILoggerFactory.
/// </summary>
public class LoggerFactory : ILoggerFactory
{
    /// <inheritdoc />
    public ILogger GetLogger(string moduleName)
    {
        return new Logger(moduleName);
    }
}
