/******************************************************************************
 * Filename    = Entity.cs
 * Author      = Kallepally Sai Kiran, Nikhil S Thomas
 * Product     = cloud-function-app
 * Project     = Comm-Uni-Cator
 * Description = Defines a common data structure for the cloud API requests.
 *****************************************************************************/

namespace Communicator.Cloud.CloudFunction.DataStructures;

using System.Text.Json;

/// <summary>
/// Represents a common data structure for cloud API requests.
/// </summary>
/// <param name="Module">The module identifier.</param>
/// <param name="Table">The table name.</param>
/// <param name="Id">The entity identifier.</param>
/// <param name="Type">The entity type.</param>
/// <param name="LastN">The number of recent items to retrieve.</param>
/// <param name="TimeRange">The time range for queries.</param>
/// <param name="Data">The JSON data payload.</param>
public record Entity(
    string Module,
    string Table,
    string Id,
    string Type,
    int LastN,
    TimeRange TimeRange,
    JsonElement Data);
