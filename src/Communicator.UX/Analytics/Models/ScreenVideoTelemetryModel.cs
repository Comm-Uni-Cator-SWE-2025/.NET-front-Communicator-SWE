using System.Text.Json.Serialization;

namespace Communicator.UX.Analytics.Models;

/// <summary>
/// Represents screen/video telemetry data from the core module.
/// Mirrors the Java ScreenVideoTelemetryModel structure.
/// </summary>
public class ScreenVideoTelemetryModel
{
    /// <summary>
    /// Start time in milliseconds (epoch).
    /// </summary>
    [JsonPropertyName("startTime")]
    public long StartTime { get; set; }

    /// <summary>
    /// End time in milliseconds (epoch).
    /// </summary>
    [JsonPropertyName("endTime")]
    public long EndTime { get; set; }

    /// <summary>
    /// FPS values recorded every 3 seconds.
    /// </summary>
    [JsonPropertyName("fpsEvery3Seconds")]
    public List<double> FpsEvery3Seconds { get; set; } = new();

    /// <summary>
    /// Whether camera was active during this period.
    /// </summary>
    [JsonPropertyName("withCamera")]
    public bool WithCamera { get; set; }

    /// <summary>
    /// Whether screen share was active during this period.
    /// </summary>
    [JsonPropertyName("withScreen")]
    public bool WithScreen { get; set; }

    /// <summary>
    /// Average FPS during this period.
    /// </summary>
    [JsonPropertyName("avgFps")]
    public double AvgFps { get; set; }

    /// <summary>
    /// Maximum FPS during this period.
    /// </summary>
    [JsonPropertyName("maxFps")]
    public double MaxFps { get; set; }

    /// <summary>
    /// Minimum FPS during this period.
    /// </summary>
    [JsonPropertyName("minFps")]
    public double MinFps { get; set; }

    /// <summary>
    /// 95th percentile FPS (worst 5%).
    /// </summary>
    [JsonPropertyName("p95Fps")]
    public double P95Fps { get; set; }

    /// <summary>
    /// Gets the start time as DateTime.
    /// </summary>
    [JsonIgnore]
    public DateTime StartDateTime => DateTimeOffset.FromUnixTimeMilliseconds(StartTime).LocalDateTime;

    /// <summary>
    /// Gets the end time as DateTime.
    /// </summary>
    [JsonIgnore]
    public DateTime EndDateTime => DateTimeOffset.FromUnixTimeMilliseconds(EndTime).LocalDateTime;

    /// <summary>
    /// Gets a formatted time label for the start time (e.g., "10:01").
    /// </summary>
    [JsonIgnore]
    public string TimeLabel => StartDateTime.ToString("HH:mm:ss");
}
