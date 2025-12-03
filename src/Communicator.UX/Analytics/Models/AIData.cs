using System.Text.Json.Serialization;

namespace Communicator.UX.Analytics.Models;

/// <summary>
/// Represents a single AI data point from the API.
/// </summary>
public class AIData
{
    /// <summary>
    /// The timestamp of the data point.
    /// </summary>
    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;

    /// <summary>
    /// The sentiment value from the API (mapped to "sentiment").
    /// </summary>
    [JsonPropertyName("sentiment")]
    public double Value { get; set; }

    /// <summary>
    /// Gets the formatted time label for display (e.g., "10:01").
    /// </summary>
    [JsonIgnore]
    public string TimeLabel
    {
        get {
            if (DateTime.TryParse(Time, out DateTime dt))
            {
                return dt.ToString("HH:mm");
            }
            return Time;
        }
    }
}
