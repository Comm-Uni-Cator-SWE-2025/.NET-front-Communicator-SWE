/*
 * -----------------------------------------------------------------------------
 *  File: CloudConfigService.cs
 *  Owner: Dhruvadeep
 *  Roll Number : 142201026
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System;

namespace Communicator.App.Services;

/// <summary>
/// Loads cloud function URLs from environment variables.
/// The .env file at the solution root is loaded by EnvLoader at startup.
/// </summary>
public sealed class CloudConfigService : ICloudConfigService
{
    public CloudConfigService()
    {
    }

    public Uri NegotiateUrl =>
        new(Environment.GetEnvironmentVariable("SIGNALR_NEGOTIATE_URL")
        ?? throw new InvalidOperationException("SIGNALR_NEGOTIATE_URL environment variable is not set"));

    public Uri MessageUrl =>
        new(Environment.GetEnvironmentVariable("SIGNALR_MESSAGE_URL")
        ?? throw new InvalidOperationException("SIGNALR_MESSAGE_URL environment variable is not set"));

    public Uri JoinGroupUrl =>
        new(Environment.GetEnvironmentVariable("SIGNALR_JOIN_GROUP_URL")
        ?? throw new InvalidOperationException("SIGNALR_JOIN_GROUP_URL environment variable is not set"));

    public Uri LeaveGroupUrl =>
        new(Environment.GetEnvironmentVariable("SIGNALR_LEAVE_GROUP_URL")
        ?? throw new InvalidOperationException("SIGNALR_LEAVE_GROUP_URL environment variable is not set"));
}


