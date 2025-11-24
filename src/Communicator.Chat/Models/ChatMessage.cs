namespace Communicator.Chat
{
    /// <summary>
    /// Data object for a single chat message displayed in the View.
    /// Supports both text and file messages.
    /// </summary>
    public class ChatMessage
    {
        public string MessageId { get; }
        public string Username { get; }
        public string Content { get; }
        public string FileName { get; }
        public long CompressedFileSize { get; }
        public byte[] FileContent { get; }
        public string Timestamp { get; }
        public bool IsSentByMe { get; }
        public string QuotedContent { get; }

        // Helper properties
        public bool HasQuote => !string.IsNullOrWhiteSpace(QuotedContent);
        public bool IsFileMessage => !string.IsNullOrWhiteSpace(FileName);

        public ChatMessage(
            string messageId,
            string username,
            string content,
            string fileName,
            long compressedFileSize,
            byte[] fileContent,
            string timestamp,
            bool isSentByMe,
            string quotedContent)
        {
            MessageId = messageId;
            Username = username;
            Content = content;
            FileName = fileName;
            CompressedFileSize = compressedFileSize;
            FileContent = fileContent;
            Timestamp = timestamp;
            IsSentByMe = isSentByMe;
            QuotedContent = quotedContent;
        }
    }
}