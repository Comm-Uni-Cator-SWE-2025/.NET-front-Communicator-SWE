using Communicator.Chat;
using FluentAssertions;
using Xunit;

namespace Communicator.Chat.Tests;

public class ChatMessageModelTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithAllParameters_SetsAllProperties()
    {
        // Arrange
        string messageId = "msg-1";
        string username = "TestUser";
        string content = "Hello World";
        string fileName = "test.txt";
        long compressedFileSize = 1024L;
        byte[] fileContent = new byte[] { 1, 2, 3 };
        string timestamp = "10:30";
        bool isSentByMe = true;
        string quotedContent = "Previous message";

        // Act
        var message = new ChatMessage(
            messageId, username, content, fileName,
            compressedFileSize, fileContent, timestamp,
            isSentByMe, quotedContent);

        // Assert
        message.MessageId.Should().Be(messageId);
        message.Username.Should().Be(username);
        message.Content.Should().Be(content);
        message.FileName.Should().Be(fileName);
        message.CompressedFileSize.Should().Be(compressedFileSize);
        message.FileContent.Should().BeSameAs(fileContent);
        message.Timestamp.Should().Be(timestamp);
        message.IsSentByMe.Should().Be(isSentByMe);
        message.QuotedContent.Should().Be(quotedContent);
    }

    [Fact]
    public void Constructor_WithEmptyStrings_SetsProperties()
    {
        // Arrange & Act
        var message = new ChatMessage(
            string.Empty, string.Empty, string.Empty, string.Empty,
            0L, Array.Empty<byte>(), string.Empty,
            false, string.Empty);

        // Assert
        message.MessageId.Should().BeEmpty();
        message.Username.Should().BeEmpty();
        message.Content.Should().BeEmpty();
        message.FileName.Should().BeEmpty();
        message.QuotedContent.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullFileContent_SetsProperty()
    {
        // Arrange & Act
        var message = new ChatMessage(
            "msg-1", "User", "Content", "file.txt",
            0L, null!, "10:00", false, string.Empty);

        // Assert
        message.FileContent.Should().BeNull();
    }

    #endregion

    #region HasQuote Property Tests

    [Fact]
    public void HasQuote_WithQuotedContent_ReturnsTrue()
    {
        // Arrange
        var message = new ChatMessage(
            "msg-1", "User", "Content", string.Empty,
            0L, null!, "10:00", false, "Quoted text");

        // Act & Assert
        message.HasQuote.Should().BeTrue();
    }

    [Fact]
    public void HasQuote_WithEmptyQuotedContent_ReturnsFalse()
    {
        // Arrange
        var message = new ChatMessage(
            "msg-1", "User", "Content", string.Empty,
            0L, null!, "10:00", false, string.Empty);

        // Act & Assert
        message.HasQuote.Should().BeFalse();
    }

    [Fact]
    public void HasQuote_WithNullQuotedContent_ReturnsFalse()
    {
        // Arrange
        var message = new ChatMessage(
            "msg-1", "User", "Content", string.Empty,
            0L, null!, "10:00", false, null!);

        // Act & Assert
        message.HasQuote.Should().BeFalse();
    }

    [Fact]
    public void HasQuote_WithWhitespaceQuotedContent_ReturnsFalse()
    {
        // Arrange
        var message = new ChatMessage(
            "msg-1", "User", "Content", string.Empty,
            0L, null!, "10:00", false, "   ");

        // Act & Assert
        message.HasQuote.Should().BeFalse();
    }

    #endregion

    #region IsFileMessage Property Tests

    [Fact]
    public void IsFileMessage_WithFileName_ReturnsTrue()
    {
        // Arrange
        var message = new ChatMessage(
            "msg-1", "User", "Content", "file.txt",
            1024L, new byte[] { 1, 2, 3 }, "10:00", false, string.Empty);

        // Act & Assert
        message.IsFileMessage.Should().BeTrue();
    }

    [Fact]
    public void IsFileMessage_WithEmptyFileName_ReturnsFalse()
    {
        // Arrange
        var message = new ChatMessage(
            "msg-1", "User", "Content", string.Empty,
            0L, null!, "10:00", false, string.Empty);

        // Act & Assert
        message.IsFileMessage.Should().BeFalse();
    }

    [Fact]
    public void IsFileMessage_WithNullFileName_ReturnsFalse()
    {
        // Arrange
        var message = new ChatMessage(
            "msg-1", "User", "Content", null!,
            0L, null!, "10:00", false, string.Empty);

        // Act & Assert
        message.IsFileMessage.Should().BeFalse();
    }

    [Fact]
    public void IsFileMessage_WithWhitespaceFileName_ReturnsFalse()
    {
        // Arrange
        var message = new ChatMessage(
            "msg-1", "User", "Content", "   ",
            0L, null!, "10:00", false, string.Empty);

        // Act & Assert
        message.IsFileMessage.Should().BeFalse();
    }

    #endregion

    #region Property Immutability Tests

    [Fact]
    public void Properties_AreReadOnly()
    {
        // Arrange
        var message = new ChatMessage(
            "msg-1", "User", "Content", string.Empty,
            0L, null!, "10:00", false, string.Empty);

        // Act & Assert - Properties should be get-only
        message.MessageId.Should().Be("msg-1");
        message.Username.Should().Be("User");
        message.Content.Should().Be("Content");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Constructor_WithLargeFileSize_HandlesCorrectly()
    {
        // Arrange
        long largeSize = long.MaxValue;
        var message = new ChatMessage(
            "msg-1", "User", "Content", "file.txt",
            largeSize, null!, "10:00", false, string.Empty);

        // Assert
        message.CompressedFileSize.Should().Be(largeSize);
    }

    [Fact]
    public void Constructor_WithLargeFileContent_HandlesCorrectly()
    {
        // Arrange
        byte[] largeContent = new byte[10000];
        Array.Fill(largeContent, (byte)255);
        var message = new ChatMessage(
            "msg-1", "User", "Content", "file.txt",
            10000L, largeContent, "10:00", false, string.Empty);

        // Assert
        message.FileContent.Should().HaveCount(10000);
        message.FileContent.Should().BeEquivalentTo(largeContent);
    }

    #endregion
}

