/*
 * -----------------------------------------------------------------------------
 *  File: CloudMessageService.cs
 *  Owner: Geetheswar V
 *  Roll Number : 142201025
 *  Module : UX
 *
 * ----------------------------------------------------------------------------- 
 */

using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections;

namespace Communicator.App.Services;

public sealed class CloudMessageService : ICloudMessageService, IDisposable
{
    private readonly ICloudConfigService _cloudConfig;
    private readonly HttpClient _httpClient;
    private HubConnection? _hubConnection;
    private string _currentMeetingId = string.Empty;
    private string _currentUsername = string.Empty;

    public bool IsConnected
    {
        get {
            if (_hubConnection == null)
            {
                return false;
            }
            return _hubConnection.State == HubConnectionState.Connected;
        }
    }

    public event EventHandler<CloudMessageEventArgs>? MessageReceived;

    public CloudMessageService(ICloudConfigService cloudConfig)
    {
        _cloudConfig = cloudConfig ?? throw new ArgumentNullException(nameof(cloudConfig));
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Connects to Azure SignalR hub via cloud function negotiate endpoint,
    /// with expanded diagnostics and robust handlers.
    /// </summary>
    public async Task ConnectAsync(string meetingId, string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be empty", nameof(username));
        }

        _currentMeetingId = meetingId;
        _currentUsername = username;

        try
        {
            string uri = $"{_cloudConfig.NegotiateUrl}?meetingId={Uri.EscapeDataString(meetingId)}";

            System.Diagnostics.Debug.WriteLine($"[CloudMessage] NEGOTIATE: Calling: {uri}");

            string negotiateJson = await _httpClient.GetStringAsync(new Uri(uri)).ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine($"[CloudMessage] NEGOTIATE RAW JSON: {negotiateJson}");

            JsonDocument doc = JsonDocument.Parse(negotiateJson);

            string? url = TryGetPropertyString(doc.RootElement, "url", "Url");
            string? accessToken = TryGetPropertyString(doc.RootElement, "accessToken", "AccessToken");

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new InvalidOperationException($"Negotiate response missing 'url'. Full response: {negotiateJson}");
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new InvalidOperationException($"Negotiate response missing 'accessToken'. Full response: {negotiateJson}");
            }

            if (accessToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Debug.WriteLine("[CloudMessage] TOKEN: Server returned 'Bearer ' prefix. Removing for SignalR.");
                accessToken = accessToken.Substring(7).Trim();
            }

            System.Diagnostics.Debug.WriteLine($"[CloudMessage] NEGOTIATE URL: {url}");
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] TOKEN LENGTH: {accessToken.Length}");

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(
                    url,
                    opts => {
                        opts.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
                        opts.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
                    }
                )
                .WithAutomaticReconnect()
                .Build();

            RegisterLifecycleDebugEvents();
            RegisterMessageHandlers();

            System.Diagnostics.Debug.WriteLine("[CloudMessage] CONNECTING...");
            await _hubConnection.StartAsync().ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine($"[CloudMessage] CONNECTED: ID={_hubConnection.ConnectionId}, STATE={_hubConnection.State}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] CONNECT ERROR: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Sends a message to all participants.
    /// </summary>
    public async Task SendMessageAsync(CloudMessageType messageType, string meetingId, string username, string message = "")
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

            string url =
                $"{_cloudConfig.MessageUrl}?meetingId={Uri.EscapeDataString(meetingId)}&message={encodedMessage}";

            // Debug output
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] URL: {url}");

            System.Diagnostics.Debug.WriteLine($"[CloudMessage] SEND: {messageType} -> {formattedMessage}");

            HttpResponseMessage response = await _httpClient.GetAsync(new Uri(url)).ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine($"[CloudMessage] SEND STATUS: {(int)response.StatusCode} {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] SEND ERROR: {ex}");
            throw;
        }
    }

    private static string FormatMessageForCloud(CloudMessageType messageType, string username, string message)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            username = string.Empty;
        }

        switch (messageType)
        {
            case CloudMessageType.UserJoined:
                {
                    return $"[USER_JOINED] {username}";
                }

            case CloudMessageType.QuickDoubt:
                {
                    return $"[{username}] {message}";
                }

            default:
                {
                    return $"[{username}] {message}";
                }
        }
    }


    /// <summary>
    /// Disconnects cleanly.
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[CloudMessage] DISCONNECTING...");
                await _hubConnection.StopAsync().ConfigureAwait(false);

                System.Diagnostics.Debug.WriteLine("[CloudMessage] DISPOSING...");
                await _hubConnection.DisposeAsync().ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CloudMessage] DISCONNECT CANCELED: {ex}");
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CloudMessage] DISCONNECT INVALID OP: {ex}");
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CloudMessage] DISCONNECT HTTP ERROR: {ex}");
            }
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

    #region Private Debug Helpers

    private void RegisterLifecycleDebugEvents()
    {
        if (_hubConnection == null)
        {
            return;
        }

        _hubConnection.Reconnecting += (ex) => {
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] LIFECYCLE: Reconnecting... ERROR={ex?.Message}");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += (id) => {
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] LIFECYCLE: Reconnected. ID={id}");
            return Task.CompletedTask;
        };

        _hubConnection.Closed += (ex) => {
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] LIFECYCLE: Closed. ERROR={ex?.Message}");
            return Task.CompletedTask;
        };
    }

    #endregion

    #region Message Handlers

    private void RegisterMessageHandlers()
    {
        if (_hubConnection == null)
        {
            return;
        }

        // ---- ReceiveDoubt ----

        _hubConnection.On<string>("ReceiveDoubt", (msg) => {
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] HDLR ReceiveDoubt(string): {msg}");
            OnReceiveDoubt(msg);
        });

        _hubConnection.On<string, string>("ReceiveDoubt", (sender, payload) => {
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] HDLR ReceiveDoubt(string,string): sender={sender}, payload={payload}");
            OnReceiveDoubt($"[{sender}] {payload}");
        });

        _hubConnection.On<JsonElement>("ReceiveDoubt", (json) => {
            string raw = json.GetRawText();
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] HDLR ReceiveDoubt(JsonElement): {raw}");

            if (json.ValueKind == JsonValueKind.Object)
            {
                if (json.TryGetProperty("sender", out JsonElement s) &&
                    json.TryGetProperty("message", out JsonElement m))
                {
                    OnReceiveDoubt($"[{s.GetString()}] {m.GetString()}");
                    return;
                }
            }

            OnReceiveDoubt(raw);
        });

        // ---- UserJoined ----

        _hubConnection.On<string>("UserJoined", (msg) => {
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] HDLR UserJoined(string): {msg}");
            OnUserJoined(msg);
        });

        _hubConnection.On<string, string>("UserJoined", (sender, _) => {
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] HDLR UserJoined(string,string): sender={sender}");
            OnUserJoined(sender);
        });

        _hubConnection.On<JsonElement>("UserJoined", (json) => {
            string raw = json.GetRawText();
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] HDLR UserJoined(JsonElement): {raw}");

            if (json.ValueKind == JsonValueKind.Object &&
                json.TryGetProperty("username", out JsonElement u))
            {
                OnUserJoined(u.GetString() ?? "");
            }
            else
            {
                OnUserJoined(raw);
            }
        });

        System.Diagnostics.Debug.WriteLine("[CloudMessage] HANDLERS REGISTERED (ReceiveDoubt & UserJoined) with multiple overloads.");
    }

    #endregion

    #region Message Parsing Logic

    private void OnReceiveDoubt(string msg)
    {
        System.Diagnostics.Debug.WriteLine($"[CloudMessage] RECV DOUBT RAW: {msg}");

        string decoded = System.Net.WebUtility.UrlDecode(msg);

        System.Diagnostics.Debug.WriteLine($"[CloudMessage] RECV DOUBT DECODED: {decoded}");

        if (decoded.StartsWith("[USER_JOINED]", StringComparison.OrdinalIgnoreCase))
        {
            string username = decoded.Substring("[USER_JOINED]".Length).Trim();

            if (!string.Equals(username, _currentUsername, StringComparison.OrdinalIgnoreCase))
            {
                MessageReceived?.Invoke(this, new CloudMessageEventArgs {
                    MessageType = CloudMessageType.UserJoined,
                    SenderName = username,
                    Message = string.Empty
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[CloudMessage] RECV: Skipped own user joined.");
            }

            return;
        }

        (string sender, string text) = ParseMessageFormat(decoded);

        if (string.Equals(sender, _currentUsername, StringComparison.OrdinalIgnoreCase))
        {
            System.Diagnostics.Debug.WriteLine("[CloudMessage] RECV: Skipped own doubt.");
            return;
        }

        if (string.IsNullOrWhiteSpace(sender))
        {
            sender = "Unknown";
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            text = "(no message)";
        }

        MessageReceived?.Invoke(this, new CloudMessageEventArgs {
            MessageType = CloudMessageType.QuickDoubt,
            SenderName = sender,
            Message = text
        });
    }

    private void OnUserJoined(string msg)
    {
        System.Diagnostics.Debug.WriteLine($"[CloudMessage] RECV USERJOIN RAW: {msg}");

        string decoded = System.Net.WebUtility.UrlDecode(msg);
        string username = ExtractUsernameFromUserJoined(decoded);

        if (!string.Equals(username, _currentUsername, StringComparison.OrdinalIgnoreCase))
        {
            MessageReceived?.Invoke(this, new CloudMessageEventArgs {
                MessageType = CloudMessageType.UserJoined,
                SenderName = username,
                Message = string.Empty
            });
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[CloudMessage] RECV: Skipped own user joined.");
        }
    }

    private static string ExtractUsernameFromUserJoined(string rawMessage)
    {
        if (string.IsNullOrWhiteSpace(rawMessage))
        {
            return string.Empty;
        }

        if (rawMessage.StartsWith("[USER_JOINED]", StringComparison.OrdinalIgnoreCase))
        {
            return rawMessage.Substring(13).Trim();
        }

        if (rawMessage.StartsWith('['))
        {
            int i = rawMessage.IndexOf(']', System.StringComparison.Ordinal);
            if (i > 0)
            {
                return rawMessage.Substring(1, i - 1).Trim();
            }
        }

        return rawMessage.Trim();
    }

    private static (string senderName, string message) ParseMessageFormat(string rawMessage)
    {
        if (string.IsNullOrWhiteSpace(rawMessage))
        {
            return (string.Empty, string.Empty);
        }

        if (rawMessage.StartsWith('['))
        {
            int i = rawMessage.IndexOf(']', System.StringComparison.Ordinal);
            if (i > 0)
            {
                string sender = rawMessage.Substring(1, i - 1).Trim();
                string msg = rawMessage.Length > i + 1
                    ? rawMessage.Substring(i + 1).Trim()
                    : string.Empty;

                return (sender, msg);
            }
        }

        return (string.Empty, rawMessage.Trim());
    }

    private static string? TryGetPropertyString(JsonElement element, params string[] names)
    {
        foreach (string name in names)
        {
            if (element.TryGetProperty(name, out JsonElement v))
            {
                return v.GetString();
            }
        }
        return null;
    }

    #endregion
}
