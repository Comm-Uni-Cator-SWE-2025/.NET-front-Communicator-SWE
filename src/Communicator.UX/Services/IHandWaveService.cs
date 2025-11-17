namespace Communicator.UX.Services;

/// <summary>
/// Manages HandWave cloud feature using Azure SignalR for real-time communication.
/// This is a cloud-managed feature separate from the RPC backend.
/// </summary>
public interface IHandWaveService
{
    /// <summary>
    /// Gets whether the service is connected to SignalR hub.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Event raised when a quick doubt message is received from SignalR.
    /// </summary>
    event Action<string>? QuickDoubtReceived;

    /// <summary>
    /// Connects to the SignalR hub for HandWave feature.
    /// </summary>
    /// <param name="username">The username to identify the user.</param>
    Task ConnectAsync(string username);

    /// <summary>
    /// Sends a quick doubt message to all participants via cloud function.
    /// </summary>
    /// <param name="username">The username of the sender.</param>
    /// <param name="message">The doubt message to send.</param>
    Task SendQuickDoubtAsync(string username, string message);

    /// <summary>
    /// Disconnects from the SignalR hub.
    /// </summary>
    Task DisconnectAsync();
}
