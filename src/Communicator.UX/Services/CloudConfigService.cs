using Microsoft.Extensions.Configuration;

namespace Communicator.UX.Services;

/// <summary>
/// Loads cloud function URLs from appsettings.json.
/// This keeps sensitive URLs out of source control.
/// </summary>
public class CloudConfigService : ICloudConfigService
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
}
