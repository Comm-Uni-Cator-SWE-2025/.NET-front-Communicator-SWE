using System.Text.Json;
using Communicator.Controller.RPC;
using Communicator.Controller.Serialization;
using Communicator.UX.Analytics.Models;

namespace Communicator.UX.Analytics.Services;

/// <summary>
/// Service responsible for fetching AI action items from core/AiAction RPC endpoint.
/// </summary>
public class AIMessageService
{
    private readonly IRPC? _rpc;
    private readonly List<string> _allMessages = new();

    /// <summary>
    /// Creates a new AIMessageService without RPC (for testing).
    /// </summary>
    public AIMessageService()
    {
        _rpc = null;
    }

    /// <summary>
    /// Creates a new AIMessageService with RPC support.
    /// </summary>
    /// <param name="rpc">The RPC interface to communicate with core</param>
    public AIMessageService(IRPC rpc)
    {
        _rpc = rpc;
    }

    /// <summary>
    /// Fetches the next set of AI action items from the core module.
    /// Only returns new messages that haven't been fetched before.
    /// </summary>
    /// <returns>A list of AIMessageData items containing timestamped messages.</returns>
    public async Task<List<AIMessageData>> FetchNextAsync()
    {
        if (_rpc == null)
        {
            System.Diagnostics.Debug.WriteLine("AI Message Service not initialized - RPC not available");
            return new List<AIMessageData>();
        }

        try
        {
            System.Diagnostics.Debug.WriteLine("Fetching Action Items from Core Module...");
            byte[] response = await _rpc.Call("core/AiAction", Array.Empty<byte>()).ConfigureAwait(false);

            if (response == null || response.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("No action items received from core");
                return new List<AIMessageData>();
            }

            // Deserialize the response using DataSerializer (returns a string JSON)
            string json = DataSerializer.Deserialize<string>(response);
            System.Diagnostics.Debug.WriteLine($"Received Action Items: {json}");

            // Clean up the JSON if it has markdown formatting
            json = CleanJson(json);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<AIMessageData>();
            }

            List<string> newMessages = ParseMessages(json);
            List<AIMessageData> addedMessages = new();

            // Only add messages that we haven't seen before (case-insensitive comparison)
            foreach (string message in newMessages)
            {
                bool alreadyExists = _allMessages.Any(existing =>
                    string.Equals(existing, message, StringComparison.OrdinalIgnoreCase));

                if (!alreadyExists)
                {
                    _allMessages.Add(message);
                    addedMessages.Add(new AIMessageData {
                        Time = DateTime.Now,
                        Message = message
                    });
                }
            }

            System.Diagnostics.Debug.WriteLine($"Added {addedMessages.Count} new action items");
            return addedMessages;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching action items: {ex.Message}");
            return new List<AIMessageData>();
        }
    }

    /// <summary>
    /// Gets all collected messages.
    /// </summary>
    public List<string> GetAllMessages()
    {
        return _allMessages;
    }

    /// <summary>
    /// Cleans JSON string by removing markdown formatting.
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
    /// Parses the action items JSON response (expected format: ["message1", "message2", ...]).
    /// </summary>
    private static List<string> ParseMessages(string json)
    {
        List<string> messages = new();

        try
        {
            // Try to parse as JSON array first
            if (json.TrimStart().StartsWith("["))
            {
                List<string>? parsed = JsonSerializer.Deserialize<List<string>>(json);
                if (parsed != null)
                {
                    messages.AddRange(parsed.Where(m => !string.IsNullOrWhiteSpace(m)));
                }
            }
            else
            {
                // Parse manually if not a clean JSON array
                string cleaned = json.Trim();
                if (cleaned.StartsWith("["))
                {
                    cleaned = cleaned[1..];
                }
                if (cleaned.EndsWith("]"))
                {
                    cleaned = cleaned[..^1];
                }

                // Split by "," pattern (quote-comma-quote)
                string[] parts = cleaned.Split("\",");

                foreach (string part in parts)
                {
                    string message = part.Trim();
                    // Remove leading/trailing quotes
                    if (message.StartsWith("\""))
                    {
                        message = message[1..];
                    }
                    if (message.EndsWith("\""))
                    {
                        message = message[..^1];
                    }

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        messages.Add(message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error parsing action items: {ex.Message}");
        }

        return messages;
    }
}
