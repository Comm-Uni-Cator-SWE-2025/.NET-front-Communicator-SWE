/*
 * -----------------------------------------------------------------------------
 *  File: ICloudMessageService.cs
 *  Owner: Dhruvadeep
 *  Roll Number : 142201026
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
namespace Communicator.App.Services;

/// <summary>
/// Message types for cloud messaging system.
/// </summary>
public enum CloudMessageType
{
    QuickDoubt
}

/// <summary>
/// Event args for cloud message received events.
/// </summary>
public sealed class CloudMessageEventArgs : EventArgs
{
    public CloudMessageType MessageType { get; init; }
    public string SenderName { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Manages cloud-based real-time messaging using Azure SignalR for communication features.
/// This is a cloud-managed service separate from the RPC backend.
/// </summary>
public interface ICloudMessageService
{
    /// <summary>
    /// Gets whether the service is connected to SignalR hub.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Event raised when any cloud message is received from SignalR.
    /// </summary>
    event EventHandler<CloudMessageEventArgs>? MessageReceived;

    /// <summary>
    /// Connects to the SignalR hub for cloud messaging.
    /// </summary>
    /// <param name="meetingId">The meeting ID to join the appropriate hub group.</param>
    /// <param name="username">The username to identify the user.</param>
    Task ConnectAsync(string meetingId, string username);

    /// <summary>
    /// Sends a message to all participants via cloud function.
    /// The cloud function determines how to broadcast based on message type.
    /// </summary>
    /// <param name="messageType">The type of message being sent.</param>
    /// <param name="meetingId">The meeting ID to send the message to the correct group.</param>
    /// <param name="username">The username of the sender.</param>
    /// <param name="message">The message content (optional for some message types like UserJoined).</param>
    Task SendMessageAsync(CloudMessageType messageType, string meetingId, string username, string message = "");

    /// <summary>
    /// Disconnects from the SignalR hub.
    /// </summary>
    Task DisconnectAsync();
}


