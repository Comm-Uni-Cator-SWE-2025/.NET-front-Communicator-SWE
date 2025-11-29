using System;

namespace Communicator.Core.Logging;

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
