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
    /// Pre-set JSON snapshots, same as your Java example.
    /// </summary>
    private readonly List<string> _shapeJsonList = new();
    //_shapeJsonList must be empty

    /// <summary>
    /// Fetch next snapshot, cycling through the list endlessly.
    /// </summary>
    public CanvasData FetchNext()
    {
        string json = _shapeJsonList[_shapeJsonList.Count - 1];

        return ParseJson(json); //return the right most entry.
    }

    public void AddShapeJson(string json)
    {
        _shapeJsonList.Add(json);
    }

    //Fetch next snapshot not required
    /// <summary>
    /// Parses JSON -> CanvasData object.
    /// Manual parsing replaced with safe JSON parsing.
    /// </summary>
    private CanvasData ParseJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        return new CanvasData {
            FreeHand = root.GetProperty("freeHand").GetInt32(),
            StraightLine = root.GetProperty("straightLine").GetInt32(),
            Rectangle = root.GetProperty("rectangle").GetInt32(),
            Ellipse = root.GetProperty("ellipse").GetInt32(),
            Triangle = root.GetProperty("triangle").GetInt32()
        };
    }
}
