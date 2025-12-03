// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;
using Communicator.Cloud.CloudFunction.FunctionLibrary;
using Communicator.Cloud.CloudFunction.DataStructures;

namespace Communicator.Cloud.CrashHandler;

public class CrashHandler : ICrashHandler
{

    private static bool s_isCreated = false;

    private CloudFunctionLibrary _cloudFunctionLibrary;

    private static string s_collection = "Exception";

    private static int s_exceptionId = 1;

    private static int s_successCode = 200;

    public CrashHandler(CloudFunctionLibrary cloudFunctionLibrary)
    {
        _cloudFunctionLibrary = cloudFunctionLibrary;
    }

    public void StartCrashHandler()
    {
        if (s_isCreated)
        {
            return;
        }

        s_isCreated = true;

        InsightProvider insightProvider = new InsightProvider();

        try
        {
            // Use an empty JSON object for Data where the original code passed null
            var emptyData = JsonDocument.Parse("{}").RootElement;

            CloudResponse responseCreate = _cloudFunctionLibrary
                .CloudCreateAsync(new Entity("CLOUD", s_collection, string.Empty, string.Empty, -1, null, emptyData))
                .GetAwaiter().GetResult();

            CloudResponse responseGet = _cloudFunctionLibrary
                .CloudGetAsync(new Entity("CLOUD", s_collection, string.Empty, string.Empty, 1, null, emptyData))
                .GetAwaiter().GetResult();

            if (responseCreate.StatusCode != s_successCode || responseGet.StatusCode != s_successCode)
            {
                throw new Exception("Cloud Error...");
            }

            // Expecting the response data to be a JSON array and extracting the first element's id
            s_exceptionId = responseGet.Data.EnumerateArray().First().GetProperty("id").GetInt32();
        }
        catch (Exception)
        {
            // Do nothing...
        }

        AppDomain.CurrentDomain.UnhandledException += async (sender, e) => {
            Exception? exception = e.ExceptionObject as Exception;
            if (exception == null)
            {
                return;
            }

            string exceptionName = exception.GetType().FullName ?? string.Empty;
            string timeStamp = DateTime.UtcNow.ToString();
            string exceptionMessage = exception.Message ?? string.Empty;
            string exceptionString = exception.ToString() ?? string.Empty;
            string exceptionStackTrace = exception.StackTrace ?? string.Empty;

            JsonNode? exceptionJsonNode = ToJsonNode(exceptionName, timeStamp, exceptionMessage, exceptionString, exceptionStackTrace);
            if (exceptionJsonNode == null)
            {
                return;
            }

            try
            {
                string aiResponse = insightProvider.GetInsights(exceptionJsonNode.ToJsonString());
                exceptionJsonNode["AIResponse"] = aiResponse;
                StoreDataToFile(exceptionJsonNode.ToJsonString());

                // Convert JsonNode to JsonElement expected by Entity
                var dataElement = JsonDocument.Parse(exceptionJsonNode.ToJsonString()).RootElement;

                var exceptionEntity = new Entity(
                    "CLOUD",
                    s_collection,
                    (++s_exceptionId).ToString(),
                    string.Empty,
                    -1,
                    null,
                    dataElement
                );

                await _cloudFunctionLibrary.CloudPostAsync(exceptionEntity).ConfigureAwait(false);
            }
            catch
            {
                // swallow exceptions in crash handler
            }
        };
    }

    private static JsonNode? ToJsonNode(string eName, string timeStamp, string eMsg, string eDetails, string eTrace)
    {
        var payload = new {
            ExceptionName = eName,
            TimestampUtc = timeStamp,
            ExceptionMessage = eMsg,
            ExceptionDetails = eDetails,
            StackTrace = eTrace
        };

        // Serialize directly to JsonNode
        return JsonSerializer.SerializeToNode(payload);
    }

    private void StoreDataToFile(string data)
    {
        File.AppendAllText("exception_log.jsonl", data + Environment.NewLine);
    }
}
