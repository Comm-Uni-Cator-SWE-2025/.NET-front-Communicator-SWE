using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.SignalRService;
using System.Threading.Tasks;
using System.Net;

namespace Communicator.Cloud.SignalR;

public class NegotiateFunction
{
    private readonly ILogger<NegotiateFunction> _logger;

    public NegotiateFunction(ILogger<NegotiateFunction> logger)
    {
        _logger = logger;
    }

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
