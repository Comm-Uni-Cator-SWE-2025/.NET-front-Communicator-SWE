using System;

namespace Communicator.Core.Logging;

/// <summary>
/// Defines a contract for logging messages within the application.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogInfo(string message);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogWarning(string message);

    /// <summary>
    /// Logs an error message, optionally with an exception.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception associated with the error.</param>
    void LogError(string message, Exception? exception = null);

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogDebug(string message);
}
