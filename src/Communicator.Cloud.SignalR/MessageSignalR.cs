/******************************************************************************
* Filename    = MessageSignalR.cs
* Author      = Nikhil S Thomas
* Product     = Comm-Uni-Cator
* Project     = SignalR Function App
* Description = Azure Function to send messages to SignalR hub.
*****************************************************************************/

using System.Net;
using System.Collections.Specialized;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.SignalRService;

namespace Communicator.Cloud.SignalR;

/// <summary>
/// Class to send messages to SignalR hub.
/// </summary>
public class MessageSignalR
{
    /// <summary>
    /// Logger instance for logging information.
    /// </summary>
    private readonly ILogger<MessageSignalR> _logger;

    /// <summary>
    /// Constructor to initialize the logger.
    /// </summary>
    /// <param name="logger">Used to instantiate logger</param>
    public MessageSignalR(ILogger<MessageSignalR> logger)
    {
        _logger = logger;
    }
    /// <summary>
    /// Represents a response datastructure that contains optional SignalR and HTTP output data.
    /// </summary>
    public class MessageResponse
    {
        [SignalROutput(HubName = "meetingHub")]
        public SignalRMessageAction? SignalRMessage { get; set; }
        [HttpResult]
        public HttpResponseData? HttpResponse { get; set; }
    }

    /// <summary>
    /// Function app endpoint to send messages to SignalR hub.
    /// </summary>
    /// <param name="req">HTTP Request</param>
    /// <returns>
    /// A <see cref="MessageResponse"/> that contains the broadcast action 
    /// and HTTP response acknowledging the send operation.
    /// </returns>
    [Function("MessageSignalR")]
    public async Task<MessageResponse> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("MessageSignalR trigger invoked.");

        // Extract message from query parameters
        NameValueCollection query = req.Query;
        string? rawMessage = query["message"];
        string message = string.IsNullOrEmpty(rawMessage) ? "New Doubt Raised!" : rawMessage!;

        // Create SignalR message action to broadcast the message
        var signalRMessage = new SignalRMessageAction(
            target: "ReceiveDoubt",
            arguments: new object[] { message }
        );

        // Create HTTP response acknowledging the send operation
        HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new {
            status = "ok",
            message = $"Message sent to SignalR hub. Broadcasted: {message}"
        });

        // Return both SignalR message action and HTTP response
        return new MessageResponse {
            SignalRMessage = signalRMessage,
            HttpResponse = response
        };
    }
}
