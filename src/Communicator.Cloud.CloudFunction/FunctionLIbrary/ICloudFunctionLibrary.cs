namespace Communicator.Cloud.CloudFunction.FunctionLibrary;

using System.Threading;
using System.Threading.Tasks;
using Communicator.Cloud.CloudFunction.DataStructures;

/// <summary>
/// Interface for calling Azure Cloud Function APIs asynchronously.
/// </summary>
public interface ICloudFunctionLibrary
{
    /// <summary>
    /// Calls /cloudcreate endpoint.
    /// </summary>
    /// <param name="request">Contains the request with type Entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response from cloud function with type CloudResponse.</returns>
    Task<CloudResponse> CloudCreateAsync(Entity request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls /clouddelete endpoint.
    /// </summary>
    /// <param name="request">Contains the request with type Entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response from cloud function with type CloudResponse.</returns>
    Task<CloudResponse> CloudDeleteAsync(Entity request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls /cloudget endpoint.
    /// </summary>
    /// <param name="request">Contains the request with type Entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response from cloud function with type CloudResponse.</returns>
    Task<CloudResponse> CloudGetAsync(Entity request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls /cloudpost endpoint.
    /// </summary>
    /// <param name="request">Contains the request with type Entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response from cloud function with type CloudResponse.</returns>
    Task<CloudResponse> CloudPostAsync(Entity request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls /cloudupdate endpoint.
    /// </summary>
    /// <param name="request">Contains the request with type Entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response from cloud function with type CloudResponse.</returns>
    Task<CloudResponse> CloudUpdateAsync(Entity request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a telemetry log to the cloud asynchronously.
    /// </summary>
    /// <param name="moduleName">The name of the module sending the log.</param>
    /// <param name="severity">The severity level of the log (e.g., INFO, ERROR).</param>
    /// <param name="message">The log message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Task that completes when the log is sent.</returns>
    Task SendLogAsync(string moduleName, string severity, string message, CancellationToken cancellationToken = default);
}
