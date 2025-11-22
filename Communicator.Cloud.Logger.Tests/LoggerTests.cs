using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using Xunit;

using Communicator.Cloud.Logger;

namespace Communicator.Cloud.Logger.Tests;
public class LoggerTests : IDisposable
{
    private const string LogFile = "application.log";

    public LoggerTests()
    {
        try
        {
            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
            }

            File.Create(LogFile).Dispose();
        }
        catch (IOException)
        {
            // Ignore locks from other tests if running in parallel
        }
    }

    public void Dispose()
    {
        try
        {
            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
            }
        }
        catch (IOException)
        {
            // Ignore locks if file is still in use by another process/test
        }
    }

    [Fact]
    public async Task InfoAsync_WritesToLocalFile()
    {
        var mockCloud = new Mock<CloudFunctionLibrary>();
        var logger = new CloudLogger("TestModule", mockCloud.Object);
        await logger.InfoAsync("Hello Info");

        string content = await File.ReadAllTextAsync(LogFile);

        Assert.Contains("INFO", content);
        Assert.Contains("TestModule", content);
        Assert.Contains("Hello Info", content);
    }

    [Fact]
    public async Task WarnAsync_WritesToFile_AndCallsCloud()
    {
        var mockCloud = new Mock<CloudFunctionLibrary>();
        mockCloud
            .Setup(m => m.SendLogAsync("TestModule", "WARNING", "Warn Test"))
            .Returns(Task.CompletedTask);

        var logger = new CloudLogger("TestModule", mockCloud.Object);

        await logger.WarnAsync("Warn Test");
        string content = await File.ReadAllTextAsync(LogFile);

        Assert.Contains("WARNING", content);
        Assert.Contains("Warn Test", content);

        mockCloud.Verify(
            m => m.SendLogAsync("TestModule", "WARNING", "Warn Test"),
            Times.Once);
    }

    [Fact]
    public async Task ErrorAsync_WithException_WritesToFile_AndCallsCloud()
    {
        var mockCloud = new Mock<CloudFunctionLibrary>();
        mockCloud
            .Setup(m => m.SendLogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var logger = new CloudLogger("TestModule", mockCloud.Object);
        Exception ex = new Exception("Something broke");

        await logger.ErrorAsync("Error happened", ex);
        string content = await File.ReadAllTextAsync(LogFile);

        Assert.Contains("SEVERE", content);
        Assert.Contains("Error happened", content);
        Assert.Contains("Something broke", content);

        mockCloud.Verify(m =>
            m.SendLogAsync("TestModule", "ERROR", It.Is<string>(msg => msg.Contains("Something broke"))),
            Times.Once);
    }
}
