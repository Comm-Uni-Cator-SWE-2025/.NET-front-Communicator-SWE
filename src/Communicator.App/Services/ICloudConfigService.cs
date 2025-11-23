/*
 * -----------------------------------------------------------------------------
 *  File: ICloudConfigService.cs
 *  Owner: Dhruvadeep
 *  Roll Number : 142201026
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
namespace Communicator.App.Services;

/// <summary>
/// Provides access to cloud function URLs stored securely in appsettings.json.
/// </summary>
public interface ICloudConfigService
{
    /// <summary>
    /// Gets the SignalR negotiate endpoint URL.
    /// </summary>
    Uri NegotiateUrl { get; }

    /// <summary>
    /// Gets the message broadcast endpoint URL.
    /// </summary>
    Uri MessageUrl { get; }

    /// <summary>
    /// Gets the join group endpoint URL.
    /// </summary>
    Uri JoinGroupUrl { get; }

    /// <summary>
    /// Gets the leave group endpoint URL.
    /// </summary>
    Uri LeaveGroupUrl { get; }
}

