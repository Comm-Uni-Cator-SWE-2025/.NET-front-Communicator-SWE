using System.Text.Json;
using Communicator.UX.Analytics.Models;

namespace Communicator.UX.Analytics.Services;

/// <summary>
/// Simulates API that returns canvas shape counts.
/// It cycles through predefined snapshots.
/// </summary>
public class CanvasDataService
{
    /// <summary>
    /// Event triggered when new canvas data is broadcasted.
    /// </summary>
    public static event Action<CanvasData>? CanvasDataChanged;

    /// <summary>
    /// Broadcasts the JSON data to all subscribers.
    /// </summary>
    public static void BroadcastData(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            var data = new CanvasData {
                FreeHand = root.GetProperty("freeHand").GetInt32(),
                StraightLine = root.GetProperty("straightLine").GetInt32(),
                Rectangle = root.GetProperty("rectangle").GetInt32(),
                Ellipse = root.GetProperty("ellipse").GetInt32(),
                Triangle = root.GetProperty("triangle").GetInt32()
            };

            CanvasDataChanged?.Invoke(data);
        }
        catch
        {
            // Ignore parsing errors
        }
    }
}
