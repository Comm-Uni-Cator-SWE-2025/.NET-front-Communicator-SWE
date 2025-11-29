using System;
using Communicator.Cloud.Logger;

namespace Communicator.Core.Logging;

/// <summary>
/// Implementation of ILogger using Communicator.Cloud.Logger.
/// </summary>
public class Logger : ILogger
{
    private readonly CloudLogger _cloudLogger;

    /// <summary>
    /// Initializes a new instance of the Logger class.
    /// </summary>
    public Logger(string moduleName)
    {
        _cloudLogger = CloudLogger.GetLogger(moduleName);
    }

    /// <inheritdoc />
    public void LogInfo(string message)
    {
        // Fire and forget
        _ = _cloudLogger.InfoAsync(message);
    }

    /// <inheritdoc />
    public void LogWarning(string message)
    {
        _ = _cloudLogger.WarnAsync(message);
    }

    /// <inheritdoc />
    public void LogError(string message, Exception? exception = null)
    {
        if (exception != null)
        {
            _ = _cloudLogger.ErrorAsync(message, exception);
        }
        else
        {
            _ = _cloudLogger.ErrorAsync(message);
        }
    }

    /// <inheritdoc />
    public void LogDebug(string message)
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] {message}");
    }
}
