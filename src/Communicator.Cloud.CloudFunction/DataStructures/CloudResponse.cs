/******************************************************************************
 * Filename    = CloudResponse.cs
 * Author      = Kallepally Sai Kiran, Nikhil S Thomas
 * Product     = cloud-function-app
 * Project     = Comm-Uni-Cator
 * Description = Defines a common data structure for the cloud API responses.
 *****************************************************************************/

namespace Communicator.Cloud.CloudFunction.DataStructures;

using System.Text.Json;

/// <summary>
/// Represents a common data structure for cloud API responses.
/// </summary>
/// <param name="StatusCode">The HTTP status code.</param>
/// <param name="Message">The response message.</param>
/// <param name="Data">The JSON data payload.</param>
public record CloudResponse(int StatusCode, string Message, JsonElement Data);
