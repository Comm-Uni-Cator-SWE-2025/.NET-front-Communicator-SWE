/******************************************************************************
 * Filename    = TimeRange.cs
 * Author      = Kallepally Sai Kiran, Nikhil S Thomas
 * Product     = cloud-function-app
 * Project     = Comm-Uni-Cator
 * Description = Defines a helper data structure for Entity.cs.
 *****************************************************************************/

namespace Communicator.Cloud.CloudFunction.DataStructures;

/// <summary>
/// Represents a time range for querying data.
/// </summary>
/// <param name="FromTime">The start time.</param>
/// <param name="ToTime">The end time.</param>
public record TimeRange(double FromTime, double ToTime);
