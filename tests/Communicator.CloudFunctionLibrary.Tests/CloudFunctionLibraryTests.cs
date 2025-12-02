/*****************************************************************************/
/* Filename    = CloudFunctionLibraryTest.cs                                 */
/* Author      = kallepally sai kiran                                        */
/* Product     = cloud-function-app                                          */
/* Project     = Comm-Uni-Cator                                              */
/* Description = ASYNC Function Library tests for Azure Function APIs        */
/*****************************************************************************/

using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Communicator.Cloud.CloudFunction.DataStructures;
using Xunit;

namespace CloudFunctionLibrary.Tests.UnitTests;

/// <summary>
/// Basic tests converted from original Java test suite for CloudFunctionLibrary async operations.
/// </summary>
public sealed class CloudFunctionLibraryBasicTests : IDisposable
{
    private readonly Communicator.Cloud.CloudFunction.FunctionLibrary.CloudFunctionLibrary _cloudFunctionLibrary;
    private readonly string? _originalBaseUrl;
    private bool _disposed;

    public CloudFunctionLibraryBasicTests()
    {
        _originalBaseUrl = Environment.GetEnvironmentVariable("CLOUD_BASE_URL");
        LoadEnv();
        _cloudFunctionLibrary = new Communicator.Cloud.CloudFunction.FunctionLibrary.CloudFunctionLibrary();
    }

    private void LoadEnv()
    {
        var root = Directory.GetCurrentDirectory();
        var dotenv = Path.Combine(root, ".env");
        while (!File.Exists(dotenv))
        {
            var parent = Directory.GetParent(root);
            if (parent == null) break;
            root = parent.FullName;
            dotenv = Path.Combine(root, ".env");
        }

        if (File.Exists(dotenv))
        {
            foreach (var line in File.ReadAllLines(dotenv))
            {
                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                }
            }
        }
        else
        {
            Environment.SetEnvironmentVariable("CLOUD_BASE_URL", "http://localhost:7071/api/");
        }
    }

    private static Entity CreateTestEntity(string operation)
    {
        return new Entity(
            "TestModule",
            "TestTable",
            Guid.NewGuid().ToString(),
            operation,
            -1,
            new TimeRange(0, 0),
            JsonDocument.Parse("{}").RootElement);
    }

    [Fact]
    public async Task CloudCreateAsyncBasicShouldReturnResponse()
    {
        Entity entity = CreateTestEntity("create");
        CloudResponse response = await _cloudFunctionLibrary.CloudCreateAsync(entity).ConfigureAwait(true);
        Assert.NotNull(response);
        Assert.IsType<CloudResponse>(response);
    }

    [Fact]
    public async Task CloudDeleteAsyncBasicShouldReturnResponse()
    {
        Entity entity = CreateTestEntity("delete");
        CloudResponse response = await _cloudFunctionLibrary.CloudDeleteAsync(entity).ConfigureAwait(true);
        Assert.NotNull(response);
        Assert.IsType<CloudResponse>(response);
    }

    [Fact]
    public async Task CloudGetAsyncBasicShouldReturnResponse()
    {
        Entity entity = CreateTestEntity("get");
        CloudResponse response = await _cloudFunctionLibrary.CloudGetAsync(entity).ConfigureAwait(true);
        Assert.NotNull(response);
        Assert.IsType<CloudResponse>(response);
    }

    [Fact]
    public async Task CloudPostAsyncBasicShouldReturnResponse()
    {
        Entity entity = CreateTestEntity("post");
        CloudResponse response = await _cloudFunctionLibrary.CloudPostAsync(entity).ConfigureAwait(true);
        Assert.NotNull(response);
        Assert.IsType<CloudResponse>(response);
    }

    [Fact]
    public async Task CloudUpdateAsyncBasicShouldReturnResponse()
    {
        Entity entity = CreateTestEntity("update");
        CloudResponse response = await _cloudFunctionLibrary.CloudUpdateAsync(entity).ConfigureAwait(true);
        Assert.NotNull(response);
        Assert.IsType<CloudResponse>(response);
    }

    [Fact]
    public async Task CallApiAsyncUnsupportedMethodShouldThrowArgumentException()
    {
        MethodInfo? methodInfo = typeof(Communicator.Cloud.CloudFunction.FunctionLibrary.CloudFunctionLibrary)
            .GetMethod("CallApiAsync", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(methodInfo);

        object? taskObj = methodInfo!.Invoke(_cloudFunctionLibrary, new object[] { "/invalid", "GET", "{}", CancellationToken.None });
        Assert.NotNull(taskObj);

        Task<string> task = (Task<string>)taskObj!;
        ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(async () => await task.ConfigureAwait(true)).ConfigureAwait(true);
        Assert.Contains("Unsupported HTTP method", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _cloudFunctionLibrary.Dispose();
        Environment.SetEnvironmentVariable("CLOUD_BASE_URL", _originalBaseUrl);
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
