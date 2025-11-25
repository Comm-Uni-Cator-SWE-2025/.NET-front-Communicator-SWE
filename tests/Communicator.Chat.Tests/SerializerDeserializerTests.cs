using System;
using System.IO;
using System.Text;
using Communicator.Chat.Serializer_deserializer;
using FluentAssertions;
using Xunit;

// Aliases
using ChatMessageDTO = Communicator.Chat.Serializer_deserializer.ChatMessage;
using FileMessageDTO = Communicator.Chat.Serializer_deserializer.FileMessage;

namespace Communicator.Chat.Tests;

// ============================================================================
// 1. Binary Utility Tests
// ============================================================================
public class BinarySerializerUtilsTests
{
    [Fact]
    public void WriteInt_ReadInt_RoundTrip()
    {
        using var stream = new MemoryStream();
        int originalValue = 12345;
        BinarySerializerUtils.WriteInt(stream, originalValue);
        stream.Position = 0;
        int readValue = BinarySerializerUtils.ReadInt(stream);
        readValue.Should().Be(originalValue);
    }

    [Fact]
    public void WriteString_ReadString_RoundTrip()
    {
        using var stream = new MemoryStream();
        string originalValue = "Hello World";
        BinarySerializerUtils.WriteString(stream, originalValue);
        stream.Position = 0;
        string readValue = BinarySerializerUtils.ReadString(stream);
        readValue.Should().Be(originalValue);
    }

    [Fact]
    public void WriteBytes_ReadBytes_RoundTrip()
    {
        using var stream = new MemoryStream();
        byte[] originalValue = new byte[] { 1, 2, 3, 4, 5 };
        BinarySerializerUtils.WriteBytes(stream, originalValue);
        stream.Position = 0;
        byte[] readValue = BinarySerializerUtils.ReadBytes(stream);
        readValue.Should().BeEquivalentTo(originalValue);
    }
}

// ============================================================================
// 2. DTO Serializer Tests
// ============================================================================
public class ChatMessageSerializerTests
{
    [Fact]
    public void Serialize_WithValidMessage_ProducesByteArray()
    {
        var message = new ChatMessageDTO("msg-1", "user-1", "Test User", "Hello World", 1234567890L, "reply-1");
        byte[] result = ChatMessageSerializer.Serialize(message);
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void Serialize_Deserialize_RoundTrip()
    {
        var original = new ChatMessageDTO("msg-1", "user-1", "Test User", "Hello World", 1234567890L, "reply-1");
        byte[] serialized = ChatMessageSerializer.Serialize(original);
        ChatMessageDTO deserialized = ChatMessageSerializer.Deserialize(serialized);

        deserialized.MessageId.Should().Be(original.MessageId);
        deserialized.Content.Should().Be(original.Content);
    }
}

public class FileMessageSerializerTests
{
    [Fact]
    public void Serialize_WithPathMode_ProducesByteArray()
    {
        var message = new FileMessageDTO("msg-1", "user-1", "Test User", "Caption", "file.txt", "/path/to/file.txt", "reply-1");
        byte[] result = FileMessageSerializer.Serialize(message);
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void Serialize_Deserialize_ContentMode_RoundTrip()
    {
        byte[] content = new byte[] { 1, 2, 3, 4, 5 };
        var original = new FileMessageDTO("msg-1", "user-1", "Test User", "Caption", "file.txt", content, 1234567890L, "reply-1");
        byte[] serialized = FileMessageSerializer.Serialize(original);
        FileMessageDTO deserialized = FileMessageSerializer.Deserialize(serialized);

        deserialized.MessageId.Should().Be(original.MessageId);
        deserialized.FileContent.Should().BeEquivalentTo(original.FileContent);
    }
}

// ============================================================================
// 3. Stream Extension Tests
// ============================================================================
public class StreamExtensionsTests
{
    [Fact]
    public void ReadExactly_WithExactData_ReadsAllBytes()
    {
        byte[] data = new byte[] { 1, 2, 3, 4, 5 };
        using var stream = new MemoryStream(data);
        byte[] buffer = new byte[5];
        stream.ReadExactly(buffer);
        buffer.Should().BeEquivalentTo(data);
    }

}