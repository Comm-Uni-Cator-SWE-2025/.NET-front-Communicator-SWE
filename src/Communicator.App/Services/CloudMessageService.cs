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

    public bool IsConnected =>
        _hubConnection != null &&
        _hubConnection.State == HubConnectionState.Connected;

    public event EventHandler<CloudMessageEventArgs>? MessageReceived;

    public CloudMessageService(ICloudConfigService cloudConfig)
    {
        _cloudConfig = cloudConfig ?? throw new ArgumentNullException(nameof(cloudConfig));
        _httpClient = new HttpClient();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Logging for debug")]
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
            string negotiateUrl =
                $"{_cloudConfig.NegotiateUrl}?userId={Uri.EscapeDataString(username)}";

            Console.WriteLine($"[CloudMessage] NEGOTIATE: Calling: {negotiateUrl}");
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] NEGOTIATE: Calling: {negotiateUrl}");

            string negotiateJson =
                await _httpClient.GetStringAsync(new Uri(negotiateUrl)).ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine($"[CloudMessage] NEGOTIATE RAW JSON: {negotiateJson}");

            JsonDocument doc = JsonDocument.Parse(negotiateJson);

            string? url = TryGetPropertyString(doc.RootElement, "url", "Url");
            string? accessToken = TryGetPropertyString(doc.RootElement, "accessToken", "AccessToken");

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new InvalidOperationException("Negotiate response missing URL");
            }
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new InvalidOperationException("Negotiate response missing accessToken");
            }

            if (accessToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Debug.WriteLine("[CloudMessage] TOKEN: Server returned 'Bearer ' prefix. Removing for SignalR.");
                accessToken = accessToken.Substring(7).Trim();
            }

            System.Diagnostics.Debug.WriteLine($"[CloudMessage] NEGOTIATE URL: {url}");
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] TOKEN LENGTH: {accessToken.Length}");

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url, opts => {
                    opts.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
                    opts.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
                })
                .WithAutomaticReconnect()
                .Build();

            RegisterLifecycleEvents();
            RegisterMessageHandlers();

            System.Diagnostics.Debug.WriteLine("[CloudMessage] CONNECTING...");
            await _hubConnection.StartAsync().ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine($"[CloudMessage] CONNECTED: ID={_hubConnection.ConnectionId}, STATE={_hubConnection.State}");

            await JoinGroupAsync(meetingId, username).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CloudMessage] Connect ERROR: {ex}");
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] Connect ERROR: {ex}");
            throw;
        }
    }

    private async Task JoinGroupAsync(string meetingId, string username)
    {
        string joinUrl =
            $"{_cloudConfig.JoinGroupUrl}?meetingId={Uri.EscapeDataString(meetingId)}&userId={Uri.EscapeDataString(username)}";

        await _httpClient.PostAsync(new Uri(joinUrl), null).ConfigureAwait(false);

        System.Diagnostics.Debug.WriteLine($"[CloudMessage] JOINED GROUP: {meetingId}");
    }

    public async Task SendMessageAsync(CloudMessageType messageType, string meetingId, string username, string message = "")
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("Not connected to cloud message service");
        }

        string formatted = FormatMessageForCloud(username, message);
        string encoded = System.Net.WebUtility.UrlEncode(formatted);

        string url =
            $"{_cloudConfig.MessageUrl}?meetingId={Uri.EscapeDataString(meetingId)}&message={encoded}";

        System.Diagnostics.Debug.WriteLine($"[CloudMessage] URL: {url}");
        System.Diagnostics.Debug.WriteLine($"[CloudMessage] SEND: {messageType} -> {formatted}");

        HttpResponseMessage response = await _httpClient.GetAsync(new Uri(url)).ConfigureAwait(false);

        System.Diagnostics.Debug.WriteLine($"[CloudMessage] SEND STATUS: {(int)response.StatusCode} {response.ReasonPhrase}");
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection == null)
        {
            return;
        }

        try
        {
            // Leave group first
            if (!string.IsNullOrEmpty(_currentMeetingId) && !string.IsNullOrEmpty(_currentUsername))
            {
                string leaveUrl =
                    $"{_cloudConfig.LeaveGroupUrl}?meetingId={Uri.EscapeDataString(_currentMeetingId)}&userId={Uri.EscapeDataString(_currentUsername)}";

                await _httpClient.PostAsync(new Uri(leaveUrl), null).ConfigureAwait(false);
            }

            System.Diagnostics.Debug.WriteLine("[CloudMessage] DISCONNECTING...");
            await _hubConnection.StopAsync().ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine("[CloudMessage] DISPOSING...");
            await _hubConnection.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is InvalidOperationException || ex is HttpRequestException)
        {
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] DISCONNECT ERROR: {ex.Message}");
        }
        finally
        {
            _hubConnection = null;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _hubConnection?.DisposeAsync().AsTask().Wait();
        GC.SuppressFinalize(this);
    }

    private void RegisterLifecycleEvents()
    {
        if (_hubConnection == null)
        {
            return;
        }

        _hubConnection.Reconnecting += (ex) => {
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] LIFECYCLE: Reconnecting... ERROR={ex?.Message}");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += async (connectionId) => {
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] LIFECYCLE: Reconnected. ID={connectionId}");
            // Re-join group
            await JoinGroupAsync(_currentMeetingId, _currentUsername).ConfigureAwait(false);
        };

        _hubConnection.Closed += (ex) => {
            System.Diagnostics.Debug.WriteLine($"[CloudMessage] LIFECYCLE: Closed. ERROR={ex?.Message}");
            return Task.CompletedTask;
        };
    }

    private void RegisterMessageHandlers()
    {
        if (_hubConnection == null)
        {
            return;
        }

        _hubConnection.On<string>("ReceiveDoubt", OnReceiveMessage);
    }

    private void OnReceiveMessage(string msg)
    {
        System.Diagnostics.Debug.WriteLine($"[CloudMessage] RECV RAW: {msg}");

        string decoded = System.Net.WebUtility.UrlDecode(msg);

        System.Diagnostics.Debug.WriteLine($"[CloudMessage] RECV DECODED: {decoded}");

        (string sender, string text) = ParseMessageFormat(decoded);

        // Handle Quick Doubt (or other chat messages)
        if (string.Equals(sender, _currentUsername, StringComparison.OrdinalIgnoreCase))
        {
            System.Diagnostics.Debug.WriteLine("[CloudMessage] RECV: Skipped own message.");
            return;
        }

        MessageReceived?.Invoke(this, new CloudMessageEventArgs {
            MessageType = CloudMessageType.QuickDoubt,
            SenderName = sender,
            Message = text
        });
    }

    private static string FormatMessageForCloud(string username, string message)
    {
        return $"[{username}] {message}";
    }

    private static (string sender, string text) ParseMessageFormat(string rawMessage)
    {
        if (rawMessage.StartsWith('['))
        {
            int i = rawMessage.IndexOf(']', StringComparison.Ordinal);
            if (i > 0)
            {
                string sender = rawMessage.Substring(1, i - 1).Trim();
                string msg = rawMessage.Length > i + 1 ?
                    rawMessage.Substring(i + 1).Trim() :
                    "";

                return (sender, msg);
            }
        }
        return ("", rawMessage);
    }

    private static string? TryGetPropertyString(JsonElement e, params string[] names)
    {
        foreach (string n in names)
        {
            if (e.TryGetProperty(n, out JsonElement v))
            {
                return v.GetString();
            }
        }

        return null;
    }
}
