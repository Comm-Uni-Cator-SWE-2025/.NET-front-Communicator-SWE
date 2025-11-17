using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http;
using System.Text.Json;

namespace Communicator.UX.Services;

/// <summary>
/// Implementation of HandWave feature using Azure SignalR.
/// Handles real-time quick doubt messaging via cloud functions.
/// </summary>
public class HandWaveService : IHandWaveService, IDisposable
{
    private readonly ICloudConfigService _cloudConfig;
    private readonly HttpClient _httpClient;
    private HubConnection? _hubConnection;
    private string _currentUsername = string.Empty;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public event Action<string>? QuickDoubtReceived;

    public HandWaveService(ICloudConfigService cloudConfig)
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
            System.Diagnostics.Debug.WriteLine($"[HandWave] Calling negotiate endpoint: {_cloudConfig.NegotiateUrl}");
            string negotiateJson = await _httpClient.GetStringAsync(_cloudConfig.NegotiateUrl).ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine($"[HandWave] Negotiate response: {negotiateJson}");
            JsonDocument doc = JsonDocument.Parse(negotiateJson);
            // Try both lowercase and uppercase property names (cloud team uses "Url" instead of "url")
            string? url = null;
            string? accessToken = null;
            
            // Try lowercase first (standard SignalR convention)
            if (doc.RootElement.TryGetProperty("url", out JsonElement urlElement))
            {
                url = urlElement.GetString();
            }
            else if (doc.RootElement.TryGetProperty("Url", out urlElement))
            {
                url = urlElement.GetString();
            }
            else
            {
                throw new InvalidOperationException("Response does not contain 'url' or 'Url' property. Response: " + negotiateJson);
            }
            
            // Try lowercase first (standard SignalR convention)
            if (doc.RootElement.TryGetProperty("accessToken", out JsonElement tokenElement))
            {
                accessToken = tokenElement.GetString();
            }
            // Try uppercase
            else if (doc.RootElement.TryGetProperty("AccessToken", out tokenElement))
            {
                accessToken = tokenElement.GetString();
            }
            else
            {
                throw new InvalidOperationException("Response does not contain 'accessToken' or 'AccessToken' property. Response: " + negotiateJson);
            }
            
            if (string.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException("URL property is null or empty in negotiate response");
            }
            
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new InvalidOperationException("Access token property is null or empty in negotiate response");
            }

            System.Diagnostics.Debug.WriteLine($"[HandWave] SignalR URL: {url}");

            // Build SignalR connection
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url, opts => opts.AccessTokenProvider = () => Task.FromResult<string?>(accessToken))
                .WithAutomaticReconnect()
                .Build();

            // Subscribe to ReceiveDoubt messages from cloud
            _hubConnection.On<string>("ReceiveDoubt", (msg) =>
            {
                System.Diagnostics.Debug.WriteLine($"[HandWave] Received doubt: {msg}");
                QuickDoubtReceived?.Invoke(msg);
            });

            await _hubConnection.StartAsync().ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine("[HandWave] Successfully connected to SignalR hub");
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
            throw new InvalidOperationException($"Failed to connect to HandWave service: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Sends a quick doubt message via cloud function endpoint.
    /// </summary>
    public async Task SendQuickDoubtAsync(string username, string message)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be empty", nameof(username));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be empty", nameof(message));
        }

        if (!IsConnected)
        {
            throw new InvalidOperationException("Not connected to HandWave service");
        }

        try
        {
            string formattedMessage = $"[{username}] {message}";
            string encodedMessage = System.Net.WebUtility.UrlEncode(formattedMessage);
            string url = $"{_cloudConfig.MessageUrl}?message={encodedMessage}";

            await _httpClient.GetAsync(url).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to send quick doubt: {ex.Message}", ex);
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
}
