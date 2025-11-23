using System.Text.Json;
using Communicator.UX.Analytics.Models;

namespace Communicator.UX.Analytics.Services;

/// <summary>
/// Simulates API that returns canvas shape counts.
/// It cycles through predefined snapshots.
/// </summary>
public class CanvasDataService
{
    private int _index = 0;

    /// <summary>
    /// Pre-set JSON snapshots, same as your Java example.
    /// </summary>
    private readonly List<string> _shapeJsonList = new()
    {
        """
        { "freeHand": 12, "straightLine": 5, "rectangle": 3, "ellipse": 2, "triangle": 4 }
        """,
        """
        { "freeHand": 15, "straightLine": 8, "rectangle": 6, "ellipse": 4, "triangle": 7 }
        """,
        """
        { "freeHand": 10, "straightLine": 12, "rectangle": 5, "ellipse": 3, "triangle": 6 }
        """,
        """
        { "freeHand": 18, "straightLine": 6, "rectangle": 9, "ellipse": 5, "triangle": 8 }
        """,
        """
        { "freeHand": 14, "straightLine": 10, "rectangle": 7, "ellipse": 6, "triangle": 5 }
        """,
        """
        { "freeHand": 20, "straightLine": 9, "rectangle": 8, "ellipse": 7, "triangle": 10 }
        """
    };

    /// <summary>
    /// Fetch next snapshot, cycling through the list endlessly.
    /// </summary>
    public CanvasData FetchNext()
    {
        string json = _shapeJsonList[_index];
        _index = (_index + 1) % _shapeJsonList.Count;

        return ParseJson(json);
    }

    /// <summary>
    /// Parses JSON -> CanvasData object.
    /// Manual parsing replaced with safe JSON parsing.
    /// </summary>
    private CanvasData ParseJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        return new CanvasData
        {
            FreeHand = root.GetProperty("freeHand").GetInt32(),
            StraightLine = root.GetProperty("straightLine").GetInt32(),
            Rectangle = root.GetProperty("rectangle").GetInt32(),
            Ellipse = root.GetProperty("ellipse").GetInt32(),
            Triangle = root.GetProperty("triangle").GetInt32()
        };
    }
}
