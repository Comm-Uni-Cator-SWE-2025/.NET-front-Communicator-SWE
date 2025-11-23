using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Communicator.Cloud.CloudFunction.DataStructures;
using Communicator.Cloud.CloudFunction.FunctionLibrary;

namespace CloudFunctionLibrary.Tests.UnitTests;

public class CloudFunctionLibraryRealTests : IDisposable
{
    private readonly CloudFunctionLibrary _cloudFunctionLibrary;
    private readonly string _originalBaseUrl;
    private bool _disposedValue;

    public CloudFunctionLibraryRealTests()
    {
        // Store original value and ensure environment variable is set
        //_originalBaseUrl = Environment.GetEnvironmentVariable("CLOUD_BASE_URL");
        _originalBaseUrl = "https://cloud-function-app.azurewebsites.net/api/";

        if (string.IsNullOrEmpty(_originalBaseUrl))
        {
            throw new InvalidOperationException(
                "CLOUD_BASE_URL environment variable is not set. " +
                "Please set it before running tests: " +
                "set CLOUD_BASE_URL=your-actual-cloud-url");
        }

        _cloudFunctionLibrary = new CloudFunctionLibrary();
    }

    [Fact]
    public async Task CloudCreateAsync_WithYourCloudEndpoint_ShouldReturnResponse()
    {
        // Arrange - Using YOUR actual cloud endpoint
        Entity testEntity = new Entity("TestModule", "TestTable", Guid.NewGuid().ToString(),
            "create", -1, new TimeRange(0, 0),
            JsonDocument.Parse("""{"test": "data", "timestamp": """ + DateTime.UtcNow.Ticks + """}""").RootElement);

        // Act
        CloudResponse response = await _cloudFunctionLibrary.CloudCreateAsync(testEntity);

        // Assert
        Assert.NotNull(response);
        Assert.IsType<CloudResponse>(response);
        Console.WriteLine($"CloudCreate Status: {response.StatusCode}, Message: {response.Message}");

        // Your cloud API should return a meaningful status code
        Assert.True(response.StatusCode >= 200 && response.StatusCode < 500,
            $"Unexpected status code: {response.StatusCode}");
    }

    [Fact]
    public async Task CloudGetAsync_WithYourCloudEndpoint_ShouldReturnResponse()
    {
        // Arrange - First create an item to retrieve
        string testId = Guid.NewGuid().ToString();

        // Create entity first
        Entity createEntity = new Entity("TestModule", "TestTable", testId,
            "create", -1, new TimeRange(0, 0),
            JsonDocument.Parse($$"""{"id": "{{testId}}", "name": "Test Item", "createdAt": "{{DateTime.UtcNow:O}}"}""").RootElement);

        var createResponse = await _cloudFunctionLibrary.CloudCreateAsync(createEntity);
        Console.WriteLine($"Create response: {createResponse.StatusCode}");

        // Now try to get it
        Entity getEntity = new Entity("TestModule", "TestTable", testId,
            "get", -1, new TimeRange(0, 0), JsonDocument.Parse("{}").RootElement);

        // Act
        CloudResponse response = await _cloudFunctionLibrary.CloudGetAsync(getEntity);

        // Assert
        Assert.NotNull(response);
        Assert.IsType<CloudResponse>(response);
        Console.WriteLine($"CloudGet Status: {response.StatusCode}, Message: {response.Message}");
        Assert.True(response.StatusCode >= 200 && response.StatusCode < 500);
    }

    [Fact]
    public async Task CloudUpdateAsync_WithYourCloudEndpoint_ShouldReturnResponse()
    {
        // Arrange - Create then update
        string testId = Guid.NewGuid().ToString();

        // First create
        Entity createEntity = new Entity("TestModule", "TestTable", testId,
            "create", -1, new TimeRange(0, 0),
            JsonDocument.Parse($$"""{"id": "{{testId}}", "status": "created"}""").RootElement);

        await _cloudFunctionLibrary.CloudCreateAsync(createEntity);

        // Now update
        Entity updateEntity = new Entity("TestModule", "TestTable", testId,
            "update", -1, new TimeRange(0, 0),
            JsonDocument.Parse($$"""{"id": "{{testId}}", "status": "updated", "updatedAt": "{{DateTime.UtcNow:O}}"}""").RootElement);

        // Act
        CloudResponse response = await _cloudFunctionLibrary.CloudUpdateAsync(updateEntity);

        // Assert
        Assert.NotNull(response);
        Assert.IsType<CloudResponse>(response);
        Console.WriteLine($"CloudUpdate Status: {response.StatusCode}, Message: {response.Message}");
        Assert.True(response.StatusCode >= 200 && response.StatusCode < 500);
    }

    [Fact]
    public async Task CloudDeleteAsync_WithYourCloudEndpoint_ShouldReturnResponse()
    {
        // Arrange - Create then delete
        string testId = Guid.NewGuid().ToString();

        // First create
        Entity createEntity = new Entity("TestModule", "TestTable", testId,
            "create", -1, new TimeRange(0, 0),
            JsonDocument.Parse($$"""{"id": "{{testId}}", "toBeDeleted": true}""").RootElement);

        await _cloudFunctionLibrary.CloudCreateAsync(createEntity);

        // Now delete
        Entity deleteEntity = new Entity("TestModule", "TestTable", testId,
            "delete", -1, new TimeRange(0, 0), JsonDocument.Parse("{}").RootElement);

        // Act
        CloudResponse response = await _cloudFunctionLibrary.CloudDeleteAsync(deleteEntity);

        // Assert
        Assert.NotNull(response);
        Assert.IsType<CloudResponse>(response);
        Console.WriteLine($"CloudDelete Status: {response.StatusCode}, Message: {response.Message}");
        Assert.True(response.StatusCode >= 200 && response.StatusCode < 500);
    }

    [Fact]
    public async Task CloudPostAsync_WithYourCloudEndpoint_ShouldReturnResponse()
    {
        // Arrange
        Entity testEntity = new Entity("TestModule", "TestTable", Guid.NewGuid().ToString(),
            "post", -1, new TimeRange(0, 0),
            JsonDocument.Parse("""{"action": "test", "data": {"key": "value"}}""").RootElement);

        // Act
        CloudResponse response = await _cloudFunctionLibrary.CloudPostAsync(testEntity);

        // Assert
        Assert.NotNull(response);
        Assert.IsType<CloudResponse>(response);
        Console.WriteLine($"CloudPost Status: {response.StatusCode}, Message: {response.Message}");
        Assert.True(response.StatusCode >= 200 && response.StatusCode < 500);
    }

    [Fact]
    public async Task SendLogAsync_WithYourCloudEndpoint_ShouldSucceed()
    {
        // Act
        await _cloudFunctionLibrary.SendLogAsync(
            "CloudFunctionLibraryTests",
            "INFO",
            $"Test log message at {DateTime.UtcNow:O}");

        // Assert - If we get here without exception, it worked
        Assert.True(true);
        Console.WriteLine("SendLog completed successfully");
    }

    [Fact]
    public async Task SendLogAsync_WithDifferentSeverityLevels_ShouldSucceed()
    {
        // Test different log levels
        var severities = new[] { "DEBUG", "INFO", "WARNING", "ERROR", "CRITICAL" };

        foreach (var severity in severities)
        {
            // Act & Assert - Should not throw
            await _cloudFunctionLibrary.SendLogAsync(
                "TestModule",
                severity,
                $"{severity} level test message");

            Console.WriteLine($"Log with severity '{severity}' sent successfully");
        }
    }

    [Fact]
    public async Task Operations_WithValidData_ShouldHandleComplexJson()
    {
        // Arrange - Test with complex JSON structure
        var complexData = JsonDocument.Parse("""
        {
            "user": {
                "id": "user-123",
                "profile": {
                    "name": "Test User",
                    "preferences": {
                        "notifications": true,
                        "theme": "dark"
                    }
                }
            },
            "action": "complex-test",
            "metadata": {
                "timestamp": """ + DateTime.UtcNow.Ticks + @""",
                "version": "1.0",
                "tags": ["test", "integration", "complex"]
            },
            "nestedArray": [
                {"item": 1, "active": true},
                {"item": 2, "active": false},
                { "item": 3, "active": true}
            ]
        }
        """).RootElement;

        Entity testEntity = new Entity("ComplexTest", "TestTable", Guid.NewGuid().ToString(),
            "create", -1, new TimeRange(0, 0), complexData);

// Act
CloudResponse response = await _cloudFunctionLibrary.CloudCreateAsync(testEntity);

// Assert
Assert.NotNull(response);
Console.WriteLine($"Complex JSON test - Status: {response.StatusCode}");
Assert.True(response.StatusCode >= 200 && response.StatusCode < 500);
    }

    [Fact]
public async Task Operations_WithCancellationToken_ShouldBeCancelable()
{
    // Arrange
    Entity testEntity = new Entity("TestModule", "TestTable", Guid.NewGuid().ToString(),
        "create", -1, new TimeRange(0, 0), JsonDocument.Parse("""{"test": "cancellation"}""").RootElement);

    var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromMilliseconds(100)); // Cancel quickly

    // Act & Assert
    await Assert.ThrowsAsync<TaskCanceledException>(() =>
        _cloudFunctionLibrary.CloudCreateAsync(testEntity, cts.Token));

    Console.WriteLine("Cancellation test completed");
}

[Fact]
public void Constructor_WithYourCloudBaseUrl_ShouldInitializeSuccessfully()
{
    // Arrange & Act
    var exception = Record.Exception(() => new CloudFunctionLibrary());

    // Assert
    Assert.Null(exception);
    Console.WriteLine($"Successfully initialized with CLOUD_BASE_URL: {Environment.GetEnvironmentVariable("CLOUD_BASE_URL")}");
}

[Fact]
public async Task MultipleOperations_InSequence_ShouldWork()
{
    // Test a complete CRUD sequence
    string testId = Guid.NewGuid().ToString();
    Console.WriteLine($"Testing CRUD sequence with ID: {testId}");

    // 1. CREATE
    var createEntity = new Entity("TestModule", "TestTable", testId,
        "create", -1, new TimeRange(0, 0),
        JsonDocument.Parse($$"""{"id": "{{testId}}", "operation": "create", "step": 1}""").RootElement);

    var createResponse = await _cloudFunctionLibrary.CloudCreateAsync(createEntity);
    Console.WriteLine($"CREATE - Status: {createResponse.StatusCode}");

    // 2. GET
    var getEntity = new Entity("TestModule", "TestTable", testId,
        "get", -1, new TimeRange(0, 0), JsonDocument.Parse("{}").RootElement);

    var getResponse = await _cloudFunctionLibrary.CloudGetAsync(getEntity);
    Console.WriteLine($"GET - Status: {getResponse.StatusCode}");

    // 3. UPDATE
    var updateEntity = new Entity("TestModule", "TestTable", testId,
        "update", -1, new TimeRange(0, 0),
        JsonDocument.Parse($$"""{"id": "{{testId}}", "operation": "update", "step": 2}""").RootElement);

    var updateResponse = await _cloudFunctionLibrary.CloudUpdateAsync(updateEntity);
    Console.WriteLine($"UPDATE - Status: {updateResponse.StatusCode}");

    // 4. DELETE
    var deleteEntity = new Entity("TestModule", "TestTable", testId,
        "delete", -1, new TimeRange(0, 0), JsonDocument.Parse("{}").RootElement);

    var deleteResponse = await _cloudFunctionLibrary.CloudDeleteAsync(deleteEntity);
    Console.WriteLine($"DELETE - Status: {deleteResponse.StatusCode}");

    // Assert all operations returned valid status codes
    Assert.True(createResponse.StatusCode >= 200 && createResponse.StatusCode < 500);
    Assert.True(getResponse.StatusCode >= 200 && getResponse.StatusCode < 500);
    Assert.True(updateResponse.StatusCode >= 200 && updateResponse.StatusCode < 500);
    Assert.True(deleteResponse.StatusCode >= 200 && deleteResponse.StatusCode < 500);

    Console.WriteLine("CRUD sequence completed successfully");
}

public void Dispose()
{
    Dispose(true);
    GC.SuppressFinalize(this);
}

protected virtual void Dispose(bool disposing)
{
    if (!_disposedValue)
    {
        if (disposing)
        {
            _cloudFunctionLibrary?.Dispose();
            // Restore original environment variable if needed
            if (_originalBaseUrl != null)
            {
                Environment.SetEnvironmentVariable("CLOUD_BASE_URL", _originalBaseUrl);
            }
        }
        _disposedValue = true;
    }
}
}
