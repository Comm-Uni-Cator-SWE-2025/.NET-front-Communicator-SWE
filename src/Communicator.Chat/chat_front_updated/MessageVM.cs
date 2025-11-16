// MessageVM.cs
namespace chat_front
{
    /// <summary>
    /// Data object for a single chat message displayed in the View.
    /// Supports both text and file messages.
    /// </summary>
    public class MessageVM
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
        public bool HasQuote => QuotedContent != null;
        public bool IsFileMessage => !string.IsNullOrEmpty(FileName);

        public MessageVM(
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