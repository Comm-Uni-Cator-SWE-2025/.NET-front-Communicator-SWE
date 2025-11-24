using System;
using System.IO;
using System.Text;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace Communicator.Chat.Serializer_deserializer
{
    // ====================================================================
    // 1. DATA MODELS (To match the fields used by the Java Serializers)
    // ====================================================================

    /// <summary>
    /// Represents a standard text chat message.
    /// </summary>
    public class ChatMessage
    {
        public string MessageId { get; }
        public string UserId { get; }
        public string SenderDisplayName { get; }
        public string Content { get; }
        public long TimestampEpochSeconds { get; }
        public string ReplyToMessageId { get; }

        public ChatMessage(string messageId, string userId, string senderDisplayName, string content, long timestampEpochSeconds, string replyToMessageId)
        {
            MessageId = messageId;
            UserId = userId;
            SenderDisplayName = senderDisplayName;
            Content = content;
            TimestampEpochSeconds = timestampEpochSeconds;
            ReplyToMessageId = replyToMessageId;
        }
    }

    /// <summary>
    /// Represents a message containing file data or a file path reference.
    /// </summary>
    public class FileMessage
    {
        public string MessageId { get; }
        public string UserId { get; }
        public string SenderDisplayName { get; }
        public string Caption { get; }
        public string FileName { get; }

        // Path-Mode fields (used when sending a reference)
        public string FilePath { get; }

        // Content-Mode fields (used when sending the actual bytes)
        public byte[] FileContent { get; }
        public long TimestampEpochSeconds { get; }

        public string ReplyToMessageId { get; }

        // Constructor for Path-Mode (used when deserializing if filePath is present)
        public FileMessage(string messageId, string userId, string senderDisplayName, string caption, string fileName, string filePath, string replyToMessageId)
        {
            MessageId = messageId;
            UserId = userId;
            SenderDisplayName = senderDisplayName;
            Caption = caption;
            FileName = fileName;
            FilePath = filePath;
            FileContent = null; // Path mode, no content bytes
            TimestampEpochSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // Placeholder
            ReplyToMessageId = replyToMessageId;
        }

        // Constructor for Content-Mode (used when deserializing if filePath is null)
        public FileMessage(string messageId, string userId, string senderDisplayName, string caption, string fileName, byte[] fileContent, long timestampEpochSeconds, string replyToMessageId)
        {
            MessageId = messageId;
            UserId = userId;
            SenderDisplayName = senderDisplayName;
            Caption = caption;
            FileName = fileName;
            FilePath = null; // Content mode, no path reference
            FileContent = fileContent;
            TimestampEpochSeconds = timestampEpochSeconds;
            ReplyToMessageId = replyToMessageId;
        }
    }

    
    // 2. BIG ENDIAN SERIALIZATION UTILITIES (Crucial for Java compatibility)

    /// <summary>
    /// Provides utility methods for writing and reading primitive types and length-prefixed data
    /// using Big Endian byte order for compatibility with Java's ByteBuffer defaults.
    /// </summary>
    public static class BinarySerializerUtils
    {
        private static readonly Encoding Utf8 = Encoding.UTF8;

        // --- Big Endian Primitive Writers/Readers (using BinaryPrimitives for safe Big Endian handling) ---

        public static void WriteInt(Stream stream, int value)
        {
            Span<byte> bytes = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(bytes, value);
            stream.Write(bytes);
        }

        public static int ReadInt(Stream stream)
        {
            Span<byte> bytes = stackalloc byte[4];
            stream.ReadExactly(bytes);
            return BinaryPrimitives.ReadInt32BigEndian(bytes);
        }

        public static void WriteLong(Stream stream, long value)
        {
            Span<byte> bytes = stackalloc byte[8];
            BinaryPrimitives.WriteInt64BigEndian(bytes, value);
            stream.Write(bytes);
        }

        public static long ReadLong(Stream stream)
        {
            Span<byte> bytes = stackalloc byte[8];
            stream.ReadExactly(bytes);
            return BinaryPrimitives.ReadInt64BigEndian(bytes);
        }

        // --- Length-Prefixed String/Bytes Writers/Readers ---

        /// <summary>
        /// Writes a length-prefixed string. Null strings are written as length 0.
        /// This mirrors Java's `writeString` behavior.
        /// </summary>
        public static void WriteString(Stream stream, string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                WriteInt(stream, 0);
            }
            else
            {
                byte[] bytes = Utf8.GetBytes(s);
                WriteInt(stream, bytes.Length);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        /// <summary>
        /// Reads a length-prefixed string. Length 0 is read as null.
        /// This mirrors Java's `readString` behavior.
        /// </summary>
        public static string ReadString(Stream stream)
        {
            int len = ReadInt(stream);
            if (len <= 0)
            {
                return null;
            }

            byte[] bytes = new byte[len];
            stream.ReadExactly(bytes);
            return Utf8.GetString(bytes);
        }

        /// <summary>
        /// Writes a length-prefixed byte array. Null arrays are written as length 0.
        /// This mirrors Java's `writeBytes` behavior.
        /// </summary>
        public static void WriteBytes(Stream stream, byte[] data)
        {
            if (data == null)
            {
                WriteInt(stream, 0);
            }
            else
            {
                WriteInt(stream, data.Length);
                stream.Write(data, 0, data.Length);
            }
        }

        /// <summary>
        /// Reads a length-prefixed byte array. Length 0 is read as null.
        /// This mirrors Java's `readBytes` behavior.
        /// </summary>
        public static byte[] ReadBytes(Stream stream)
        {
            int len = ReadInt(stream);
            if (len <= 0)
            {
                return null;
            }

            byte[] bytes = new byte[len];
            stream.ReadExactly(bytes);
            return bytes;
        }
    }

    
    // 3. CHATMESSAGE SERIALIZER (.NET Implementation)
    

    /// <summary>
    /// Serializer and Deserializer for ChatMessage, directly corresponding to the Java ChatMessageSerializer.
    /// </summary>
    public static class ChatMessageSerializer
    {
        /// <summary>
        /// Serializes a ChatMessage object into a Big Endian byte array using the Java format.
        /// </summary>
        public static byte[] Serialize(ChatMessage message)
        {
            using (var ms = new MemoryStream())
            {
                // Sequence: ID, User ID, Display Name, Content, Timestamp, Reply ID
                BinarySerializerUtils.WriteString(ms, message.MessageId);
                BinarySerializerUtils.WriteString(ms, message.UserId);
                BinarySerializerUtils.WriteString(ms, message.SenderDisplayName);
                BinarySerializerUtils.WriteString(ms, message.Content);

                // Timestamp is written as a raw 8-byte long (Big Endian)
                BinarySerializerUtils.WriteLong(ms, message.TimestampEpochSeconds);

                BinarySerializerUtils.WriteString(ms, message.ReplyToMessageId);

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Deserializes a Big Endian byte array into a ChatMessage object.
        /// </summary>
        public static ChatMessage Deserialize(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                string id = BinarySerializerUtils.ReadString(ms);
                string user = BinarySerializerUtils.ReadString(ms);
                string name = BinarySerializerUtils.ReadString(ms);
                string content = BinarySerializerUtils.ReadString(ms);

                long timestamp = BinarySerializerUtils.ReadLong(ms);

                string replyId = BinarySerializerUtils.ReadString(ms);

                return new ChatMessage(id, user, name, content, timestamp, replyId);
            }
        }
    }

    // ====================================================================
    // 4. FILEMESSAGE SERIALIZER (.NET Implementation)
    // ====================================================================

    /// <summary>
    /// Serializer and Deserializer for FileMessage, directly corresponding to the Java FileMessageSerializer.
    /// Note: The Java file serializer uses the byte array helpers (ReadBytes/WriteBytes) for strings internally.
    /// </summary>
    public static class FileMessageSerializer
    {
        /// <summary>
        /// Serializes a FileMessage object into a Big Endian byte array using the Java format.
        /// </summary>
        public static byte[] Serialize(FileMessage message)
        {
            using (var ms = new MemoryStream())
            {
                // FileMessage serializer uses WriteBytes helper for all length-prefixed data (including strings)

                // Strings -> Byte[]
                BinarySerializerUtils.WriteBytes(ms, message.MessageId == null ? null : Encoding.UTF8.GetBytes(message.MessageId));
                BinarySerializerUtils.WriteBytes(ms, message.UserId == null ? null : Encoding.UTF8.GetBytes(message.UserId));
                BinarySerializerUtils.WriteBytes(ms, message.SenderDisplayName == null ? null : Encoding.UTF8.GetBytes(message.SenderDisplayName));
                BinarySerializerUtils.WriteBytes(ms, message.Caption == null ? null : Encoding.UTF8.GetBytes(message.Caption));
                BinarySerializerUtils.WriteBytes(ms, message.FileName == null ? null : Encoding.UTF8.GetBytes(message.FileName));
                BinarySerializerUtils.WriteBytes(ms, message.FilePath == null ? null : Encoding.UTF8.GetBytes(message.FilePath));

                // File content (byte[])
                BinarySerializerUtils.WriteBytes(ms, message.FileContent);

                // Timestamp (long)
                BinarySerializerUtils.WriteLong(ms, message.TimestampEpochSeconds);

                // Reply ID
                BinarySerializerUtils.WriteBytes(ms, message.ReplyToMessageId == null ? null : Encoding.UTF8.GetBytes(message.ReplyToMessageId));

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Deserializes a Big Endian byte array into a FileMessage object, including the path/content mode logic.
        /// </summary>
        public static FileMessage Deserialize(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                // The Java readString helper calls readBytes and then converts it, which is matched by C#'s ReadString.
                string messageId = BinarySerializerUtils.ReadString(ms);
                string userId = BinarySerializerUtils.ReadString(ms);
                string senderName = BinarySerializerUtils.ReadString(ms);
                string caption = BinarySerializerUtils.ReadString(ms);
                string fileName = BinarySerializerUtils.ReadString(ms);
                string filePath = BinarySerializerUtils.ReadString(ms);
                byte[] fileContent = BinarySerializerUtils.ReadBytes(ms); // Note: readBytes is used directly for byte[] data
                long timestampEpoch = BinarySerializerUtils.ReadLong(ms);
                string replyToId = BinarySerializerUtils.ReadString(ms);

                // --- Defensive cleaning logic (matching Java code) ---
                if (filePath != null)
                {
                    filePath = filePath.Trim();
                    // Remove leading '*' if present
                    if (filePath.StartsWith("*"))
                    {
                        filePath = filePath.Substring(1).Trim();
                    }
                    if (filePath.Length == 0)
                    {
                        filePath = null;
                    }
                }

                // --- Prioritize filePath detection (Path-Mode) over fileContent (Content-Mode) ---
                if (filePath != null)
                {
                    // Path-Mode: filePath is present
                    return new FileMessage(messageId, userId, senderName, caption, fileName, filePath, replyToId);
                }
                else
                {
                    // Content-Mode: fileContent is present (or null if it was an empty file)
                    return new FileMessage(messageId, userId, senderName, caption, fileName, fileContent, timestampEpoch, replyToId);
                }
            }
        }
    }
}

// ====================================================================
// EXTENSION METHOD (Required for robust Stream reading)
// ====================================================================

// This is a polyfill for the modern .NET ReadExactly method, ensuring that we always
// read the exact number of bytes expected from the stream without short reads.
namespace System.IO
{
    public static class StreamExtensions
    {
        public static void ReadExactly(this Stream stream, Span<byte> buffer)
        {
            int totalRead = 0;
            while (totalRead < buffer.Length)
            {
                // Read into the remaining slice of the buffer
                int bytesRead = stream.Read(buffer.Slice(totalRead));
                if (bytesRead == 0)
                {
                    // Reached end of stream prematurely
                    throw new EndOfStreamException($"Premature end of stream. Expected {buffer.Length} bytes, but only read {totalRead}.");
                }
                totalRead += bytesRead;
            }
        }
    }
}
