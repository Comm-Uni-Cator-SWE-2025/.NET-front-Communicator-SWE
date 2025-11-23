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
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Collections.Specialized;

namespace Communicator.Cloud.SignalR;

public class NegotiateFunction
{
    private readonly ILogger<NegotiateFunction> _logger;

    public NegotiateFunction(ILogger<NegotiateFunction> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Combined response: SignalR connection info + group add + HTTP output
    /// </summary>
    public class NegotiateResponse
    {
        [SignalROutput(HubName = "meetingHub")]
        public object? SignalROutput { get; set; }  // holds group action

        [HttpResult]
        public HttpResponseData? HttpResponse { get; set; }
    }

    [Function("negotiate")]
    public async Task<NegotiateResponse> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        [SignalRConnectionInfoInput(HubName = "meetingHub", UserId = "{meetingId}")]
        SignalRConnectionInfo connectionInfo)
    {
        NameValueCollection query = HttpUtility.ParseQueryString(req.Url.Query);
        string? meetingId = query["meetingId"];

        _logger.LogInformation($"Negotiation request received. MeetingId={meetingId}");

        // Create HTTP response
        HttpResponseData httpResponse = req.CreateResponse(HttpStatusCode.OK);
        await httpResponse.WriteAsJsonAsync(new {
            status = "ok",
            meetingId,
            info = "Negotiation successful"
        });

        // Group add action
        var groupAddAction = new SignalRGroupAction(SignalRGroupActionType.Add) {
            UserId = meetingId,
            GroupName = meetingId
        };

        // Output: return BOTH connection info and group add
        return new NegotiateResponse {
            SignalROutput = new {
                connectionInfo,
                groupAddAction
            },
            HttpResponse = httpResponse
        };
    }
}
