/******************************************************************************
* Filename    = Program.cs
* Author      = Nikhil S Thomas
* Product     = Comm-Uni-Cator
* Project     = SignalR Function App
* Description = Auto-generated Program entry point for the Azure Functions application.
*****************************************************************************/

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Auto-generated Program entry point for the Azure Functions application.
/// </summary>
internal class Program
{
    /// <summary>
    /// Main method to configure and run the Functions application.
    /// </summary>
    /// <param name="args"></param>
    private static void Main(string[] args)
    {
        FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

        builder.ConfigureFunctionsWebApplication();

        builder.Services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights();

        builder.Build().Run();
    }
}
