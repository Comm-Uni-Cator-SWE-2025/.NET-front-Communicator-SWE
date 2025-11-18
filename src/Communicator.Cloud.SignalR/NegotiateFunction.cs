/******************************************************************************
* Filename    = NegotiateFunction.cs
* Author      = Nikhil S Thomas
* Product     = Comm-Uni-Cator
* Project     = SignalR Function App
* Description = Azure Function to handle SignalR negotiation requests.
*****************************************************************************/

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.SignalRService;
using System.Threading.Tasks;
using System.Net;

namespace Communicator.Cloud.SignalR;

/// <summary>
/// Class to handle SignalR negotiation requests.
/// </summary>
public class NegotiateFunction
{
    /// <summary>
    /// Logger instance for logging information.
    /// </summary>
    private readonly ILogger<NegotiateFunction> _logger;

    /// <summary>
    /// Constructor to initialize the logger.
    /// </summary>
    /// <param name="logger">Used to instantiate logger</param>
    public NegotiateFunction(ILogger<NegotiateFunction> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Function app endpoint to handle negotiation requests.
    /// </summary>
    /// <param name="req">HTTP Request</param>
    /// <param name="connectionInfo">Auto-generated SignalR Connection Info</param>
    /// <returns>HTTP response with connection info (URL and Access Token)</returns>
    [Function("negotiate")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        [SignalRConnectionInfoInput(HubName = "meetingHub")] SignalRConnectionInfo connectionInfo)
    {
        _logger.LogInformation("Negotiation request received.");

        HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(connectionInfo);

        return response;
    }
}
