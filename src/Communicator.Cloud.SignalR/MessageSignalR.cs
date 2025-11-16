using System.Net;
using System.Collections.Specialized;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.SignalRService;

namespace Communicator.Cloud.SignalR;

public class MessageSignalR
{
    private readonly ILogger<MessageSignalR> _logger;

    public MessageSignalR(ILogger<MessageSignalR> logger)
    {
        _logger = logger;
    }

    public class MessageResponse
    {
        [SignalROutput(HubName = "meetingHub")]
        public SignalRMessageAction? SignalRMessage { get; set; }
        [HttpResult]
        public HttpResponseData? HttpResponse { get; set; }
    }

    [Function("MessageSignalR")]
    public async Task<MessageResponse> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("MessageSignalR trigger invoked.");

        NameValueCollection query = req.Query;
        string? rawMessage = query["message"];
        string message = string.IsNullOrEmpty(rawMessage) ? "New Doubt Raised!" : rawMessage!;

        var signalRMessage = new SignalRMessageAction(
            target: "ReceiveDoubt",
            arguments: new object[] { message }
        );

        HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new {
            status = "ok",
            message = $"Message sent to SignalR hub. Broadcasted: {message}"
        });

        return new MessageResponse {
            SignalRMessage = signalRMessage,
            HttpResponse = response
        };
    }
}
