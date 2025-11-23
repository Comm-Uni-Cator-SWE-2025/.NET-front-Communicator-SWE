using System.Text.Json;
using Communicator.UX.Analytics.Models;

namespace Communicator.UX.Analytics.Services;

public class ScreenShareService
/// <summary>
/// Provides Screenshare data fetched from an API endpoint.
/// Currently uses static sample data for demo.
/// </summary>
/// 
{
    /// <summary>
    /// Fetches Screenshare sentiment data asynchronously.
    /// </summary>
    public async Task<List<ScreenShareData>> ScreenShareDatasAsync()
    {
        await Task.Delay(200); // simulate delay

        string json = """
        [
          { "time": "2025-11-07T10:00:00Z", "sentiment": 7.0},
          { "time": "2025-11-07T10:01:45Z", "sentiment": 3.0},
          { "time": "2025-11-07T10:03:20Z", "sentiment": -3.0},
          { "time": "2025-11-07T10:04:50Z", "sentiment": 2.0},
          { "time": "2025-11-07T10:06:10Z", "sentiment": 6.0},
          { "time": "2025-11-07T10:07:30Z", "sentiment": 4.0},
          { "time": "2025-11-07T10:08:55Z", "sentiment": 7.0},
          { "time": "2025-11-07T10:10:22Z", "sentiment": 8.0},
          { "time": "2025-11-07T10:12:40Z", "sentiment": -2.0},
          { "time": "2025-11-07T10:14:00Z", "sentiment": 3.0}
        ]
        """;

        return JsonSerializer.Deserialize<List<ScreenShareData>>(json)!;
    }
}
