using System.Text.Json.Serialization;

namespace AnalyticsApp.Models;

/// <summary>
/// Represents a single AI data point from the API.
/// </summary>
public class AIData
{
    /// <summary>
    /// The timestamp of the data point.
    /// </summary>
    [JsonPropertyName("time")]
    public DateTime Time { get; set; }

    /// <summary>
    /// The sentiment value from the API (mapped to "sentiment").
    /// </summary>
    [JsonPropertyName("sentiment")]
    public double Value { get; set; }
}
