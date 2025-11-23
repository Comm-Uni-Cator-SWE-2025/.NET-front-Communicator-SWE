using System.Text.Json.Serialization;

namespace Communicator.UX.Analytics.Models;

public class ScreenShareData
{
    /// <summary>
    /// The timestamp of the data point.
    /// </summary>
    [JsonPropertyName("time")]
    public DateTime Time { get; set; }

    /// <summary>
    /// The sentiment value from the API (mapped to "Value").
    /// </summary>
    [JsonPropertyName("sentiment")]
    public double Sentiment { get; set; }
}
