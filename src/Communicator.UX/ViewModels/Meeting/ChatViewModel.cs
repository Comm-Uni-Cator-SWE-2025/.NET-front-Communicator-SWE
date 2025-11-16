using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Communicator.Chat;
using Communicator.Core.UX;
using Communicator.Core.UX.Services;
using Controller;

namespace Communicator.UX.ViewModels.Meeting;

/// <summary>
/// ViewModel for chat functionality with file sharing support.
/// Manages chat messages, replies, file attachments, and communication with backend via RPC.
/// </summary>
public class ChatViewModel : ObservableObject
{
    // --- Constants ---
    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB

    // --- Dependencies ---
    private readonly User _currentUser;
    private readonly IToastService _toastService;
    // private readonly AbstractRPC _rpc; // TODO: Wire up when RPC is available
    
    // --- State ---
    private string _messageInput = string.Empty;
    private string _replyQuoteText = string.Empty;
    private string _attachmentText = string.Empty;
    private string? _currentReplyId;
    private FileInfo? _attachedFile;

    // --- Properties ---
    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public string MessageInput
    {
        get => _messageInput;
        set
        {
            if (SetProperty(ref _messageInput, value))
            {
                // Notify the SendMessageCommand to re-evaluate CanExecute
                (SendMessageCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string ReplyQuoteText
    {
        get => _replyQuoteText;
        set
        {
            if (SetProperty(ref _replyQuoteText, value))
            {
                OnPropertyChanged(nameof(IsReplying));
            }
        }
    }

    public string AttachmentText
    {
        get => _attachmentText;
        set
        {
            if (SetProperty(ref _attachmentText, value))
            {
                OnPropertyChanged(nameof(HasAttachment));
            }
        }
    }

    public bool IsReplying => !string.IsNullOrEmpty(ReplyQuoteText);
    public bool HasAttachment => !string.IsNullOrEmpty(AttachmentText);

    // --- Commands ---
    public ICommand SendMessageCommand { get; }
    public ICommand CancelReplyCommand { get; }
    public ICommand StartReplyCommand { get; }
    public ICommand TestRecvCommand { get; }
    public ICommand AttachFileCommand { get; }
    public ICommand CancelAttachmentCommand { get; }
    public ICommand DownloadFileCommand { get; }
    public ICommand DeleteMessageCommand { get; }

    // --- Events for View ---
    public event Func<FileInfo?>? RequestFileSelection;

    // --- Constructor ---
    public ChatViewModel(User user, IToastService toastService)
    {
        _currentUser = user ?? throw new ArgumentNullException(nameof(user));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));

        // Initialize commands
        SendMessageCommand = new RelayCommand(_ => OnSendMessage(), _ => CanSendMessage());
        CancelReplyCommand = new RelayCommand(_ => CancelReply());
        TestRecvCommand = new RelayCommand(_ => SimulateIncomingMessage());
        StartReplyCommand = new RelayCommand(param => StartReply(param as ChatMessage));
        AttachFileCommand = new RelayCommand(_ => OnAttachFile());
        CancelAttachmentCommand = new RelayCommand(_ => CancelAttachment());
        DownloadFileCommand = new RelayCommand(param => DownloadFile(param as ChatMessage));
        DeleteMessageCommand = new RelayCommand(param => DeleteMessage(param as ChatMessage));

        // TODO: Subscribe to RPC events when available
        // _rpc.Subscribe("chat:new-message", HandleBackendTextMessage);
        // _rpc.Subscribe("chat:file-metadata-received", HandleBackendFileMetadata);
        // etc.
    }

    // --- User Actions ---
    private bool CanSendMessage()
    {
        return _attachedFile != null || !string.IsNullOrWhiteSpace(MessageInput);
    }

    private void OnSendMessage()
    {
        if (_attachedFile != null)
        {
            SendFileMessage(_attachedFile, MessageInput);
        }
        else if (!string.IsNullOrWhiteSpace(MessageInput))
        {
            SendTextMessage(MessageInput);
        }

        MessageInput = string.Empty;
        CancelReply();
        CancelAttachment();
    }

    private void SendTextMessage(string messageText)
    {
        string messageId = Guid.NewGuid().ToString();
        string timestamp = DateTime.Now.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);

        // Create and add message optimistically
        ChatMessage message = new ChatMessage(
            messageId,
            _currentUser.DisplayName,
            messageText,
            string.Empty, // fileName
            0, // compressedFileSize
            Array.Empty<byte>(), // fileContent
            timestamp,
            true, // isSentByMe
            GetQuotedContent(_currentReplyId) ?? string.Empty
        );

        Messages.Add(message);

        // TODO: Send via RPC
        // var messageBytes = Encoding.UTF8.GetBytes(SerializeMessage(message));
        // await _rpc.CallAsync("chat:send-text", messageBytes);
    }

    private void SendFileMessage(FileInfo file, string caption)
    {
        if (!file.Exists)
        {
            _toastService.ShowError("File not found");
            return;
        }

        if (file.Length > MaxFileSizeBytes)
        {
            _toastService.ShowError("File is too large (Max 50MB)");
            return;
        }

        string messageId = Guid.NewGuid().ToString();
        string timestamp = DateTime.Now.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);

        // Create and add message optimistically
        ChatMessage message = new ChatMessage(
            messageId,
            _currentUser.DisplayName,
            caption ?? $"Sent {file.Name}",
            file.Name,
            file.Length,
            Array.Empty<byte>(), // We don't load file content here
            timestamp,
            true, // isSentByMe
            GetQuotedContent(_currentReplyId) ?? string.Empty
        );

        Messages.Add(message);

        // TODO: Send via RPC
        // var messageBytes = Encoding.UTF8.GetBytes(SerializeFileMessage(message, file.FullName));
        // await _rpc.CallAsync("chat:send-file", messageBytes);
    }

    public void StartReply(ChatMessage? messageToReply)
    {
        if (messageToReply == null)
        {
            return;
        }

        _currentReplyId = messageToReply.MessageId;

        string quoteText = messageToReply.IsFileMessage
            ? $"Replying to file: {messageToReply.FileName}"
            : $"Replying to {messageToReply.Username}: {TruncateText(messageToReply.Content, 30)}";

        ReplyQuoteText = quoteText;
    }

    public void CancelReply()
    {
        _currentReplyId = null;
        ReplyQuoteText = string.Empty;
    }

    public void OnAttachFile()
    {
        FileInfo? selectedFile = RequestFileSelection?.Invoke();
        if (selectedFile != null && selectedFile.Exists)
        {
            if (selectedFile.Length > MaxFileSizeBytes)
            {
                _toastService.ShowError("File is too large (Max 50MB)");
                return;
            }

            _attachedFile = selectedFile;
            AttachmentText = $"📎 {selectedFile.Name}";
        }
    }

    public void CancelAttachment()
    {
        _attachedFile = null;
        AttachmentText = string.Empty;
    }

    public void DownloadFile(ChatMessage? fileMessage)
    {
        if (fileMessage == null || !fileMessage.IsFileMessage)
        {
            _toastService.ShowError("Invalid file message");
            return;
        }

        // TODO: Implement file download via RPC
        // var messageIdBytes = Encoding.UTF8.GetBytes(fileMessage.MessageId);
        // await _rpc.CallAsync("chat:save-file-to-disk", messageIdBytes);
    }

    public void DeleteMessage(ChatMessage? messageToDelete)
    {
        if (messageToDelete == null || !messageToDelete.IsSentByMe)
        {
            return;
        }

        // Remove from local collection
        Messages.Remove(messageToDelete);

        // Clear reply if replying to deleted message
        if (_currentReplyId == messageToDelete.MessageId)
        {
            CancelReply();
        }

        // TODO: Send delete request via RPC
        // var messageIdBytes = Encoding.UTF8.GetBytes(messageToDelete.MessageId);
        // await _rpc.CallAsync("chat:delete-message", messageIdBytes);
    }

    public void SimulateIncomingMessage()
    {
        string messageId = Guid.NewGuid().ToString();
        string timestamp = DateTime.Now.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);

        ChatMessage message = new ChatMessage(
            messageId,
            "Test User",
            "Hello! This is a simulated incoming message.",
            string.Empty,
            0,
            Array.Empty<byte>(),
            timestamp,
            false, // not sent by me
            string.Empty
        );

        Messages.Add(message);
    }

    // --- Helper Methods ---
    private string? GetQuotedContent(string? replyToId)
    {
        if (string.IsNullOrEmpty(replyToId))
        {
            return null;
        }

        ChatMessage? repliedToMessage = Messages.FirstOrDefault(m => m.MessageId == replyToId);
        if (repliedToMessage == null)
        {
            return "Reply to unavailable message";
        }

        string sender = repliedToMessage.IsSentByMe ? "You" : repliedToMessage.Username;
        string contentSnippet = repliedToMessage.IsFileMessage
            ? $"File: {repliedToMessage.FileName}"
            : TruncateText(repliedToMessage.Content, 30);

        return $"↩ {sender}: {contentSnippet}";
    }

    private static string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
        {
            return text;
        }

        return string.Concat(text.AsSpan(0, maxLength), "...");
    }
}
