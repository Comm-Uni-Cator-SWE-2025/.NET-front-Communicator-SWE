/******************************************************************************
* Filename    = Logger.cs
* Author      = Sidarth Prabhu
* Product     = Comm-Uni-Cator
* Project     = Cloud Logger
* Description = Methods to store info, warnings and errors in Azure Telemetry.
*****************************************************************************/
namespace Communicator.Cloud.Logger;

public class CloudLogger
{
    /// <summary>
    /// Library instance for sending log.
    /// </summary>
    private readonly CloudFunctionLibrary _cloudLib;

    /// <summary>
    /// Path of local log file.
    /// </summary>
    private static readonly string s_logFilePath = "application.log";

    /// <summary>
    /// object to lock when accessing the local file.
    /// </summary>
    private static readonly object s_fileLock = new object();

    /// <summary>
    /// Name of the module instantiating the Logger
    /// </summary>
    private readonly string _moduleName;

    /// <summary>
    /// Static constructor.
    /// Initializes the log file if it does not exist.
    /// </summary>
    static CloudLogger()
    {
        try
        {
            if (!File.Exists(s_logFilePath))
            {
                File.Create(s_logFilePath).Dispose();
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Logger setup failed: " + e.Message);
        }
    }

    /// <summary>
    /// Public constructor used by GetLogger().
    /// Chains to the other constructor with a real CloudFunctionLibrary.
    /// </summary>
    private CloudLogger(string name)
            : this(name, new CloudFunctionLibrary()) { }

    /// <summary>
    /// Internal constructor for unit testing.
    /// </summary>
    public CloudLogger(string name, CloudFunctionLibrary cloudLib)
    {
        _moduleName = name;
        _cloudLib = cloudLib;
    }

    /// <summary>
    /// Factory method to get a logger instance.
    /// </summary>
    public static CloudLogger GetLogger(string moduleName)
    {
        return new CloudLogger(moduleName);
    }

    // Helper to simulate the synchronized file writing of Java's FileHandler
    private void WriteLocalLog(string level, string message)
    {
        lock (s_fileLock)
        {
            string logEntry = $"{DateTime.Now:s} {level}: [{_moduleName}] {message}{Environment.NewLine}";
            File.AppendAllText(s_logFilePath, logEntry);
        }
    }

    /// <summary>
    /// Logs an INFO message to the local file asynchronously.
    /// </summary>
    /// <param name="message">The informational message to be logged.</param>
    /// <returns>
    /// A <see cref="Task"/> that completes when the local log entry has been successfully written to disk.
    /// </returns>
    public Task InfoAsync(string message)
    {
        return Task.Run(() => WriteLocalLog("INFO", message));
    }

    /// <summary>
    /// Logs a WARNING message to the local file and sends it to the cloud asynchronously.
    /// </summary>
    /// <param name="message">The warning message to be logged and sent.</param>
    /// <returns>
    /// A <see cref="Task"/> that completes only when BOTH the local file write 
    /// and the cloud transmission are finished.
    /// </returns>
    public Task WarnAsync(string message)
    {
        Task localLogTask = Task.Run(() => WriteLocalLog("WARNING", message));
        Task cloudLogTask = _cloudLib.SendLogAsync(_moduleName, "WARNING", message);
        return Task.WhenAll(localLogTask, cloudLogTask);
    }

    /// <summary>
    /// Logs an ERROR message to the local file and sends it to the cloud asynchronously.
    /// </summary>
    /// <param name="message">The error message to be logged and sent.</param>
    /// <returns>
    /// A <see cref="Task"/> that completes only when BOTH the local file write 
    /// and the cloud transmission are finished.
    /// </returns>
    public Task ErrorAsync(string message)
    {
        Task localLogTask = Task.Run(() => WriteLocalLog("SEVERE", message));
        Task cloudLogTask = _cloudLib.SendLogAsync(_moduleName, "ERROR", message);
        return Task.WhenAll(localLogTask, cloudLogTask);
    }

    /// <summary>
    /// Logs an ERROR message with exception details to the local file and sends it to the cloud asynchronously.
    /// </summary>
    /// <param name="message">The base error message.</param>
    /// <param name="e">The exception containing stack trace and details to append to the log.</param>
    /// <returns>
    /// A <see cref="Task"/> that completes only when the full error details have been 
    /// written locally and sent to the cloud.
    /// </returns>
    public Task ErrorAsync(string message, Exception e)
    {
        string exceptionDetails = e != null ? $" | Exception: {e}" : "";
        string fullMessage = message + exceptionDetails;
        Task localLogTask = Task.Run(() => WriteLocalLog("SEVERE", fullMessage));
        Task cloudLogTask = _cloudLib.SendLogAsync(_moduleName, "ERROR", fullMessage);
        return Task.WhenAll(localLogTask, cloudLogTask);
    }
}
