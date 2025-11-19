using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http;
using System.Text.Json;

namespace Communicator.UX.Services;

/// <summary>
/// Implementation of cloud-based messaging using Azure SignalR.
/// Handles real-time communication for various message types via cloud functions.
/// </summary>
public class CloudMessageService : ICloudMessageService, IDisposable
{
    private readonly ICloudConfigService _cloudConfig;
    private readonly HttpClient _httpClient;
    private HubConnection? _hubConnection;
    private string _currentUsername = string.Empty;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public event EventHandler<CloudMessageEventArgs>? MessageReceived;

    public CloudMessageService(ICloudConfigService cloudConfig)
    {
        _cloudConfig = cloudConfig ?? throw new ArgumentNullException(nameof(cloudConfig));
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Connects to Azure SignalR hub via cloud function negotiate endpoint.
    /// </summary>
    public async Task ConnectAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be empty", nameof(username));
        }

        _currentUsername = username;

        try
        {
            // Get SignalR connection info from negotiate endpoint
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] Calling negotiate endpoint: {_cloudConfig.NegotiateUrl}");
            string negotiateJson = await _httpClient.GetStringAsync(_cloudConfig.NegotiateUrl).ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] Negotiate response: {negotiateJson}");
            
            JsonDocument doc = JsonDocument.Parse(negotiateJson);
            
            // Extract URL (try both lowercase and uppercase property names)
            string? url = TryGetPropertyString(doc.RootElement, "url", "Url");
            if (string.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException($"Response does not contain 'url' or 'Url' property. Response: {negotiateJson}");
            }
            
            // Extract access token (try both lowercase and uppercase property names)
            string? accessToken = TryGetPropertyString(doc.RootElement, "accessToken", "AccessToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new InvalidOperationException($"Response does not contain 'accessToken' or 'AccessToken' property. Response: {negotiateJson}");
            }

            System.Diagnostics.Debug.WriteLine($"[CloudMessage] SignalR URL: {url}");

            // Build SignalR connection
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url, opts => opts.AccessTokenProvider = () => Task.FromResult<string?>(accessToken))
                .WithAutomaticReconnect()
                .Build();

            // Subscribe to messages from SignalR hub
            RegisterMessageHandlers();

            await _hubConnection.StartAsync().ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine("[CloudMessage] Successfully connected to SignalR hub");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to connect to negotiate endpoint '{_cloudConfig.NegotiateUrl}': {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse negotiate response as JSON: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to connect to cloud message service: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Sends a message to all participants via cloud function endpoint.
    /// </summary>
    public async Task SendMessageAsync(CloudMessageType messageType, string username, string message = "")
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be empty", nameof(username));
        }

        if (!IsConnected)
        {
            throw new InvalidOperationException("Not connected to cloud message service");
        }

        try
        {
            string formattedMessage = FormatMessageForCloud(messageType, username, message);
            string encodedMessage = System.Net.WebUtility.UrlEncode(formattedMessage);
            string url = $"{_cloudConfig.MessageUrl}?message={encodedMessage}";

            System.Diagnostics.Debug.WriteLine($"[CloudMessage] Sending {messageType}: {formattedMessage}");
            await _httpClient.GetAsync(url).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to send {messageType} message: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Disconnects from SignalR hub.
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            try
            {
                await _hubConnection.StopAsync().ConfigureAwait(false);
                await _hubConnection.DisposeAsync().ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
            {
                // Ignore disposal errors
            }
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                _hubConnection = null;
            }
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _hubConnection?.DisposeAsync().AsTask().Wait();
        GC.SuppressFinalize(this);
    }

    #region Private Methods

    /// <summary>
    /// Registers SignalR message handlers for different message types.
    /// </summary>
    private void RegisterMessageHandlers()
    {
        if (_hubConnection == null)
        {
            return;
        }

        // Handle doubt messages from SignalR
        _hubConnection.On<string>("ReceiveDoubt", OnReceiveDoubt);

        // Handle user joined messages from SignalR
        _hubConnection.On<string>("UserJoined", OnUserJoined);
    }

    private void OnReceiveDoubt(string msg)
    {
        System.Diagnostics.Debug.WriteLine($"[CloudMessage] Received doubt: {msg}");
        
        // Decode URL-encoded messages
        string decoded = System.Net.WebUtility.UrlDecode(msg);
        
        // Cloud function sends everything to ReceiveDoubt channel
        // Check if it's actually a UserJoined message
        if (decoded.StartsWith("[USER_JOINED]", StringComparison.OrdinalIgnoreCase))
        {
            // Extract username and route to UserJoined handler
            string username = decoded.Substring("[USER_JOINED]".Length).Trim();
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] Routing UserJoined to correct handler: {username}");
            
            // Skip if this is our own message
            if (string.Equals(username, _currentUsername, StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Debug.WriteLine($"[CloudMessage] Skipping own user joined from {username}");
                return;
            }
            
            // Raise UserJoined event
            var userJoinedArgs = new CloudMessageEventArgs
            {
                MessageType = CloudMessageType.UserJoined,
                SenderName = username,
                Message = string.Empty
            };
            MessageReceived?.Invoke(this, userJoinedArgs);
            return;
        }
        
        // It's an actual quick doubt - parse normally
        (string senderName, string doubtMessage) = ParseMessageFormat(decoded);
        
        // Skip if this is our own message
        if (string.Equals(senderName, _currentUsername, StringComparison.OrdinalIgnoreCase))
        {
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] Skipping own doubt from {senderName}");
            return;
        }
        
        // Ensure we have valid sender and message
        if (string.IsNullOrWhiteSpace(senderName))
        {
            senderName = "Unknown";
        }
        
        if (string.IsNullOrWhiteSpace(doubtMessage))
        {
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] WARNING: Received empty doubt message from {senderName}. Raw: '{msg}', Decoded: '{decoded}'");
            doubtMessage = "(no message)";
        }
        
        // Raise the event
        var args = new CloudMessageEventArgs
        {
            MessageType = CloudMessageType.QuickDoubt,
            SenderName = senderName,
            Message = doubtMessage
        };
        MessageReceived?.Invoke(this, args);
    }

    private void OnUserJoined(string msg)
    {
        System.Diagnostics.Debug.WriteLine($"[CloudMessage] User joined: {msg}");
        
        // Decode URL-encoded messages
        string decoded = System.Net.WebUtility.UrlDecode(msg);
        
        // Parse format: "[USER_JOINED] Username" or just "Username"
        string username = ExtractUsernameFromUserJoined(decoded);
        
        // Skip if this is our own message
        if (string.Equals(username, _currentUsername, StringComparison.OrdinalIgnoreCase))
        {
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] Skipping own user joined from {username}");
            return;
        }
        
        // Raise the event with username in SenderName and empty message
        var args = new CloudMessageEventArgs
        {
            MessageType = CloudMessageType.UserJoined,
            SenderName = username,
            Message = string.Empty
        };
        MessageReceived?.Invoke(this, args);
    }

    /// <summary>
    /// Extracts username from UserJoined message format.
    /// Handles both "[USER_JOINED] Username" and just "Username" formats.
    /// </summary>
    private static string ExtractUsernameFromUserJoined(string rawMessage)
    {
        if (string.IsNullOrWhiteSpace(rawMessage))
        {
            return string.Empty;
        }

        // Check if it has the [USER_JOINED] prefix
        if (rawMessage.StartsWith("[USER_JOINED]", StringComparison.OrdinalIgnoreCase))
        {
            // Extract username after the prefix
            return rawMessage.Substring(13).Trim();
        }

        // Check if it has any bracket format like [Username]
        if (rawMessage.StartsWith('['))
        {
            int closeBracketIndex = rawMessage.IndexOf(']', StringComparison.Ordinal);
            if (closeBracketIndex > 0)
            {
                // If the bracket contains "USER_JOINED", get the text after
                string bracketContent = rawMessage.Substring(1, closeBracketIndex - 1).Trim();
                if (string.Equals(bracketContent, "USER_JOINED", StringComparison.OrdinalIgnoreCase))
                {
                    return closeBracketIndex + 1 < rawMessage.Length
                        ? rawMessage.Substring(closeBracketIndex + 1).Trim()
                        : string.Empty;
                }
                // Otherwise, the bracket itself contains the username
                return bracketContent;
            }
        }

        // No special format, just return the whole message as username
        return rawMessage.Trim();
    }

    /// <summary>
    /// Parses message format "[SenderName] Message" into components.
    /// </summary>
    private static (string senderName, string message) ParseMessageFormat(string rawMessage)
    {
        System.Diagnostics.Debug.WriteLine($"[CloudMessage] ParseMessageFormat - Input: '{rawMessage}'");
        
        if (string.IsNullOrWhiteSpace(rawMessage))
        {
            System.Diagnostics.Debug.WriteLine("[CloudMessage] ParseMessageFormat - Input is null/empty");
            return (string.Empty, string.Empty);
        }

        // Look for pattern: "[SenderName] Message"
        if (rawMessage.StartsWith('['))
        {
            int closeBracketIndex = rawMessage.IndexOf(']', StringComparison.Ordinal);
            if (closeBracketIndex > 0)
            {
                string senderName = rawMessage.Substring(1, closeBracketIndex - 1).Trim();
                string message = closeBracketIndex + 1 < rawMessage.Length
                    ? rawMessage.Substring(closeBracketIndex + 1).Trim()
                    : string.Empty;

                System.Diagnostics.Debug.WriteLine($"[CloudMessage] ParseMessageFormat - Parsed: Sender='{senderName}', Message='{message}'");
                return (senderName, message);
            }
        }
        
        // If no brackets found, treat entire message as content
        System.Diagnostics.Debug.WriteLine($"[CloudMessage] ParseMessageFormat - No brackets, treating as message: '{rawMessage}'");
        return (string.Empty, rawMessage.Trim());
    }

    /// <summary>
    /// Formats a message for sending to the cloud function.
    /// </summary>
    private static string FormatMessageForCloud(CloudMessageType messageType, string username, string message)
    {
        return messageType switch
        {
            CloudMessageType.UserJoined => $"[USER_JOINED] {username}",
            CloudMessageType.QuickDoubt => $"[{username}] {message}",
            _ => $"[{username}] {message}",
        };
    }

    /// <summary>
    /// Tries to get a string property from JsonElement, attempting multiple property names.
    /// </summary>
    private static string? TryGetPropertyString(JsonElement element, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            if (element.TryGetProperty(propertyName, out JsonElement propertyElement))
            {
                return propertyElement.GetString();
            }
        }
        return null;
    }

    #endregion
}
