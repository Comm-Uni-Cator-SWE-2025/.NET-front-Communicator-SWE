using System.Text.Json;
using Communicator.Controller.RPC;
using Communicator.Controller.Serialization;
using Communicator.UX.Analytics.Models;

namespace Communicator.UX.Analytics.Services;

/// <summary>
/// Provides AI sentiment data fetched from the core module via RPC.
/// </summary>
public class ApiService
{
    private readonly IRPC _rpc;
    private readonly List<AIData> _allData = new();
    private string _lastTime = string.Empty;

    /// <summary>
    /// Creates a new ApiService with RPC support.
    /// </summary>
    /// <param name="rpc">The RPC interface to communicate with core</param>
    public ApiService(IRPC rpc)
    {
        _rpc = rpc;
    }

    /// <summary>
    /// Fetches AI sentiment data asynchronously from RPC endpoint.
    /// Only returns new data points that haven't been fetched before.
    /// </summary>
    public async Task<List<AIData>> FetchAIDataAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Fetching Sentiment Data from Core Module...");
            byte[] response = await _rpc.Call("core/AiSentiment", Array.Empty<byte>()).ConfigureAwait(false);

            if (response == null || response.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("No sentiment data received from core");
                return new List<AIData>();
            }

            // Deserialize the response using DataSerializer (returns a string JSON)
            string json = DataSerializer.Deserialize<string>(response);
            System.Diagnostics.Debug.WriteLine($"Received Sentiment Data: {json}");

            // Clean up the JSON if it has markdown formatting
            json = CleanJson(json);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<AIData>();
            }

            List<AIData> newPoints = ParseSentimentJson(json);
            List<AIData> addedPoints = new();

            // Only add points that are newer than the last time we fetched
            foreach (AIData point in newPoints)
            {
                if (string.IsNullOrEmpty(_lastTime) || string.Compare(point.Time, _lastTime, StringComparison.Ordinal) > 0)
                {
                    _allData.Add(point);
                    addedPoints.Add(point);
                }
            }

            // Update the last time marker
            if (_allData.Count > 0)
            {
                _lastTime = _allData[^1].Time;
            }

            return addedPoints;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching sentiment data: {ex.Message}");
            return new List<AIData>();
        }
    }

    /// <summary>
    /// Gets all collected sentiment data.
    /// </summary>
    public List<AIData> GetAllData()
    {
        return _allData;
    }

    /// <summary>
    /// Cleans JSON string by removing markdown formatting and extra quotes.
    /// </summary>
    private static string CleanJson(string raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return string.Empty;
        }

        string cleaned = raw.Trim();

        // Remove markdown code block markers
        cleaned = cleaned.Replace("```json", "").Replace("```", "").Trim();

        // Unescape escaped quotes
        cleaned = cleaned.Replace("\\\"", "\"");

        return cleaned;
    }

    /// <summary>
    /// Parses the sentiment JSON response.
    /// </summary>
    private static List<AIData> ParseSentimentJson(string json)
    {
        List<AIData> points = new();

        try
        {
            // Try to parse as JSON array first
            if (json.TrimStart().StartsWith("["))
            {
                List<AIData>? parsed = JsonSerializer.Deserialize<List<AIData>>(json);
                if (parsed != null)
                {
                    points.AddRange(parsed);
                }
            }
            else
            {
                // Parse manually if not a clean JSON array (similar to Java implementation)
                string[] parts = json.Split('{');
                foreach (string part in parts)
                {
                    if (part.Contains("sentiment"))
                    {
                        try
                        {
                            string timePart = part.Split("\"time\":")[1].Split(",")[0].Trim();
                            string time = timePart.Replace("\"", "").Replace("Z", "").Trim();

                            string sentimentPart = part.Split("\"sentiment\":")[1].Split("}")[0].Trim();
                            double sentiment = double.Parse(sentimentPart.Replace(",", ""));

                            points.Add(new AIData { Time = time, Value = sentiment });
                        }
                        catch
                        {
                            System.Diagnostics.Debug.WriteLine($"Error parsing sentiment part: {part}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error parsing sentiment JSON: {ex.Message}");
        }

        return points;
    }
}
