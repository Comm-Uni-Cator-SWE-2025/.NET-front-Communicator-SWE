/*
 * -----------------------------------------------------------------------------
 *  File: CloudConfigService.cs
 *  Owner: Dhruvadeep
 *  Roll Number : 142201026
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using Microsoft.Extensions.Configuration;

namespace Communicator.App.Services;

/// <summary>
/// Loads cloud function URLs from appsettings.json.
/// This keeps sensitive URLs out of source control.
/// </summary>
public sealed class CloudConfigService : ICloudConfigService
{
    private readonly IConfiguration _configuration;

    public CloudConfigService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public Uri NegotiateUrl =>
        new(_configuration["CloudFunctions:NegotiateUrl"]
        ?? throw new InvalidOperationException("NegotiateUrl not configured in appsettings.json"));

    public Uri MessageUrl =>
        new(_configuration["CloudFunctions:MessageUrl"]
        ?? throw new InvalidOperationException("MessageUrl not configured in appsettings.json"));

    public Uri JoinGroupUrl =>
        new(_configuration["CloudFunctions:JoinGroupUrl"]
        ?? throw new InvalidOperationException("JoinGroupUrl not configured in appsettings.json"));

    public Uri LeaveGroupUrl =>
        new(_configuration["CloudFunctions:LeaveGroupUrl"]
        ?? throw new InvalidOperationException("LeaveGroupUrl not configured in appsettings.json"));
}


