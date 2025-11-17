namespace Communicator.UX.Services;

/// <summary>
/// Provides access to cloud function URLs stored securely in appsettings.json.
/// </summary>
public interface ICloudConfigService
{
    /// <summary>
    /// Gets the SignalR negotiate endpoint URL.
    /// </summary>
    string NegotiateUrl { get; }

    /// <summary>
    /// Gets the message broadcast endpoint URL.
    /// </summary>
    string MessageUrl { get; }
}
