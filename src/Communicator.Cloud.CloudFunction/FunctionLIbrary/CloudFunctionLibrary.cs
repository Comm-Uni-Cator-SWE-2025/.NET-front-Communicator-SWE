/******************************************************************************
 * Filename    = CloudFunctionLibrary.cs
 * Author      = kallepally sai kiran
 * Product     = cloud-function-app
 * Project     = Comm-Uni-Cator
 * Description = ASYNC Function Library for calling Azure Function APIs
 *****************************************************************************/

namespace Communicator.Cloud.CloudFunction.FunctionLibrary;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Communicator.Cloud.CloudFunction.DataStructures;

/// <summary>
/// Function Library for calling Azure Cloud Function APIs asynchronously.
/// </summary>
public sealed class CloudFunctionLibrary : IDisposable
{
    /// <summary>Base URL of the Cloud Functions.</summary>
    private readonly string baseUrl;

    /// <summary>HTTP client for requests.</summary>
    private readonly HttpClient httpClient;

    /// <summary>JSON serializer options.</summary>
    private readonly JsonSerializerOptions jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudFunctionLibrary"/> class.
    /// Constructor loads base URL from environment and initializes client/serializer.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when CLOUD_BASE_URL environment variable is not set.</exception>
    public CloudFunctionLibrary()
    {
        baseUrl = Environment.GetEnvironmentVariable("CLOUD_BASE_URL") ??
                  throw new InvalidOperationException("CLOUD_BASE_URL environment variable is not set");

        httpClient = new HttpClient();
        jsonOptions = new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };
    }

    /// <summary>
    /// Generic function to make HTTP calls.
    /// </summary>
    /// <param name="api">Endpoint after base URL.</param>
    /// <param name="method">HTTP method ("POST" or "PUT").</param>
    /// <param name="payload">JSON payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>CloudResponse body as string.</returns>
    /// <exception cref="ArgumentException">Thrown when unsupported HTTP method is provided.</exception>
    private async Task<string> CallApiAsync(string api, string method, string payload, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(api);
        ArgumentException.ThrowIfNullOrEmpty(method);
        ArgumentException.ThrowIfNullOrEmpty(payload);

        using var request = new HttpRequestMessage {
            RequestUri = new Uri(baseUrl + api),
            Content = new StringContent(payload, Encoding.UTF8, "application/json"),
        };

        request.Method = method.ToUpper(CultureInfo.InvariantCulture) switch {
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            _ => throw new ArgumentException($"Unsupported HTTP method: {method}", nameof(method)),
        };

        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Convert JSON to CloudResponse.
    /// </summary>
    /// <param name="json">Contains the Response with the type string.</param>
    /// <returns>Convert the json into type CloudResponse.</returns>
    /// <exception cref="InvalidOperationException">Thrown when JSON parsing fails.</exception>
    private CloudResponse ConvertToResponse(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<CloudResponse>(json, jsonOptions) ??
                   throw new InvalidOperationException("Deserialized response is null");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse CloudResponse JSON: {json}", ex);
        }
    }

    /// <summary>
    /// Calls /cloudcreate endpoint.
    /// </summary>
    /// <param name="request">Contains the request with type Entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response from cloud function with type CloudResponse.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    public async Task<CloudResponse> CloudCreateAsync(Entity request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            string payload = JsonSerializer.Serialize(request, jsonOptions);
            string response = await CallApiAsync("/cloudcreate", "POST", payload, cancellationToken).ConfigureAwait(false);
            return ConvertToResponse(response);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException("CloudCreate operation failed", ex);
        }
    }

    /// <summary>
    /// Calls /clouddelete endpoint.
    /// </summary>
    /// <param name="request">Contains the request with type Entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response from cloud function with type CloudResponse.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    public async Task<CloudResponse> CloudDeleteAsync(Entity request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            string payload = JsonSerializer.Serialize(request, jsonOptions);
            string response = await CallApiAsync("/clouddelete", "POST", payload, cancellationToken).ConfigureAwait(false);
            return ConvertToResponse(response);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException("CloudDelete operation failed", ex);
        }
    }

    /// <summary>
    /// Calls /cloudget endpoint.
    /// </summary>
    /// <param name="request">Contains the request with type Entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response from cloud function with type CloudResponse.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    public async Task<CloudResponse> CloudGetAsync(Entity request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            string payload = JsonSerializer.Serialize(request, jsonOptions);
            string response = await CallApiAsync("/cloudget", "POST", payload, cancellationToken).ConfigureAwait(false);
            return ConvertToResponse(response);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException("CloudGet operation failed", ex);
        }
    }

    /// <summary>
    /// Calls /cloudpost endpoint.
    /// </summary>
    /// <param name="request">Contains the request with type Entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response from cloud function with type CloudResponse.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    public async Task<CloudResponse> CloudPostAsync(Entity request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            string payload = JsonSerializer.Serialize(request, jsonOptions);
            string response = await CallApiAsync("/cloudpost", "POST", payload, cancellationToken).ConfigureAwait(false);
            return ConvertToResponse(response);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException("CloudPost operation failed", ex);
        }
    }

    /// <summary>
    /// Calls /cloudupdate endpoint.
    /// </summary>
    /// <param name="request">Contains the request with type Entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response from cloud function with type CloudResponse.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    public async Task<CloudResponse> CloudUpdateAsync(Entity request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            string payload = JsonSerializer.Serialize(request, jsonOptions);
            string response = await CallApiAsync("/cloudupdate", "PUT", payload, cancellationToken).ConfigureAwait(false);
            return ConvertToResponse(response);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException("CloudUpdate operation failed", ex);
        }
    }

    /// <summary>
    /// Sends a telemetry log to the cloud asynchronously.
    /// </summary>
    /// <param name="moduleName">The name of the module sending the log.</param>
    /// <param name="severity">The severity level of the log (e.g., INFO, ERROR).</param>
    /// <param name="message">The log message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Task that completes when the log is sent.</returns>
    /// <exception cref="ArgumentException">Thrown when parameters are null or empty.</exception>
    public async Task SendLogAsync(string moduleName, string severity, string message, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(moduleName);
        ArgumentException.ThrowIfNullOrEmpty(severity);
        ArgumentException.ThrowIfNullOrEmpty(message);

        try
        {
            var logData = new Dictionary<string, string> {
                ["moduleName"] = moduleName,
                ["severity"] = severity,
                ["message"] = message,
            };

            string payload = JsonSerializer.Serialize(logData, jsonOptions);
            await CallApiAsync("/telemetry/log", "POST", payload, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException("SendLog operation failed", ex);
        }
    }

    /// <summary>
    /// Disposes the HttpClient instance.
    /// </summary>
    public void Dispose()
    {
        httpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
