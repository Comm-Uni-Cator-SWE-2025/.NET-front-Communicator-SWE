using System.Text.Json;
using Communicator.Controller.RPC;
using Communicator.Controller.Serialization;
using Communicator.UX.Analytics.Models;

namespace Communicator.UX.Analytics.Services;

/// <summary>
/// Service responsible for fetching screen/video telemetry from core/ScreenTelemetry RPC endpoint.
/// </summary>
public class ScreenShareService
{
    private readonly IRPC? _rpc;
    private readonly List<ScreenVideoTelemetryModel> _allTelemetry = new();
    private long _lastEndTime = 0;

    /// <summary>
    /// Creates a new ScreenShareService without RPC (for testing).
    /// </summary>
    public ScreenShareService()
    {
        _rpc = null;
    }

    /// <summary>
    /// Creates a new ScreenShareService with RPC support.
    /// </summary>
    /// <param name="rpc">The RPC interface to communicate with core</param>
    public ScreenShareService(IRPC rpc)
    {
        _rpc = rpc;
    }

    /// <summary>
    /// Fetches screen telemetry data asynchronously from RPC endpoint.
    /// Only returns new telemetry entries that haven't been fetched before.
    /// </summary>
    public async Task<List<ScreenVideoTelemetryModel>> FetchTelemetryAsync()
    {
        if (_rpc == null)
        {
            System.Diagnostics.Debug.WriteLine("ScreenShare Service not initialized - RPC not available");
            return new List<ScreenVideoTelemetryModel>();
        }

        try
        {
            System.Diagnostics.Debug.WriteLine("Fetching Screen Telemetry from Core Module...");
            byte[] response = await _rpc.Call("core/ScreenTelemetry", Array.Empty<byte>()).ConfigureAwait(false);

            if (response == null || response.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("No screen telemetry received from core");
                return new List<ScreenVideoTelemetryModel>();
            }

            // Deserialize the response - it's a List<ScreenVideoTelemetryModel>
            List<ScreenVideoTelemetryModel> telemetryList = DataSerializer.Deserialize<List<ScreenVideoTelemetryModel>>(response);
            System.Diagnostics.Debug.WriteLine($"Received {telemetryList.Count} telemetry entries");

            List<ScreenVideoTelemetryModel> newEntries = new();

            // Only add entries that are newer than what we've seen
            foreach (ScreenVideoTelemetryModel entry in telemetryList)
            {
                if (entry.StartTime > _lastEndTime)
                {
                    _allTelemetry.Add(entry);
                    newEntries.Add(entry);
                    if (entry.EndTime > _lastEndTime)
                    {
                        _lastEndTime = entry.EndTime;
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"Added {newEntries.Count} new telemetry entries");
            return newEntries;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching screen telemetry: {ex.Message}");
            return new List<ScreenVideoTelemetryModel>();
        }
    }

    /// <summary>
    /// Gets all collected telemetry data.
    /// </summary>
    public List<ScreenVideoTelemetryModel> GetAllTelemetry()
    {
        return _allTelemetry;
    }

    /// <summary>
    /// Extracts all FPS data points with their timestamps from the telemetry.
    /// </summary>
    public List<(string TimeLabel, double Fps)> GetAllFpsDataPoints()
    {
        List<(string TimeLabel, double Fps)> dataPoints = new();

        foreach (ScreenVideoTelemetryModel entry in _allTelemetry)
        {
            DateTime startTime = entry.StartDateTime;

            for (int i = 0; i < entry.FpsEvery3Seconds.Count; i++)
            {
                // Each FPS reading is 3 seconds apart
                DateTime pointTime = startTime.AddSeconds(i * 3);
                string timeLabel = pointTime.ToString("HH:mm:ss");
                dataPoints.Add((timeLabel, entry.FpsEvery3Seconds[i]));
            }
        }

        return dataPoints;
    }
}
