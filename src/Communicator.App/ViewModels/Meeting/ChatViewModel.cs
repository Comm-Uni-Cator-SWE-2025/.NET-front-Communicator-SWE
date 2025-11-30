/*
 * -----------------------------------------------------------------------------
 *  File: ChatViewModel.cs
 *  Owner: UpdateNamesForEachModule
 *  Roll Number :
 *  Module : 
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Communicator.Chat;
using Communicator.Controller.Meeting;
using Communicator.Controller.RPC;
using Communicator.UX.Core;
using Communicator.UX.Core.Services;
using Communicator.App.Services;
using ChatSerializer = Communicator.Chat.Serializer_deserializer.ChatMessageSerializer;
using FileSerializer = Communicator.Chat.Serializer_deserializer.FileMessageSerializer;
using SerializedChatMessage = Communicator.Chat.Serializer_deserializer.ChatMessage;
using SerializedFileMessage = Communicator.Chat.Serializer_deserializer.FileMessage;

namespace Communicator.App.ViewModels.Meeting;

public class RequestFileSelectionEventArgs : EventArgs
{
    public FileInfo? SelectedFile { get; set; }
}

/// <summary>
/// ViewModel for chat functionality with file sharing support.
/// Manages chat messages, replies, file attachments, and communication with backend via RPC.
/// </summary>
public sealed class ChatViewModel : ObservableObject
{
    // --- Constants ---
    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB

    // --- Dependencies ---
    private readonly UserProfile _currentUser;
    private readonly IToastService _toastService;
    private readonly IRPC? _rpc;
    private readonly IRpcEventService? _rpcEventService;

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
        set {
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
        set {
            if (SetProperty(ref _replyQuoteText, value))
            {
                OnPropertyChanged(nameof(IsReplying));
            }
        }
    }

    public string AttachmentText
    {
        get => _attachmentText;
        set {
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
    public event EventHandler<RequestFileSelectionEventArgs>? RequestFileSelection;

    // --- Constructor ---
    public ChatViewModel(
        UserProfile user,
        IToastService toastService,
        IRPC? rpc = null,
        IRpcEventService? rpcEventService = null)
    {
        _currentUser = user ?? throw new ArgumentNullException(nameof(user));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
        _rpc = rpc;
        _rpcEventService = rpcEventService;

        // Initialize commands
        SendMessageCommand = new RelayCommand(_ => OnSendMessage(), _ => CanSendMessage());
        CancelReplyCommand = new RelayCommand(_ => CancelReply());
        TestRecvCommand = new RelayCommand(_ => SimulateIncomingMessage());
        StartReplyCommand = new RelayCommand(param => StartReply(param as ChatMessage));
        AttachFileCommand = new RelayCommand(_ => OnAttachFile());
        CancelAttachmentCommand = new RelayCommand(_ => CancelAttachment());
        DownloadFileCommand = new RelayCommand(param => DownloadFile(param as ChatMessage));
        DeleteMessageCommand = new RelayCommand(param => DeleteMessage(param as ChatMessage));

        if (_rpcEventService != null)
        {
            SubscribeToRpcEvents();
        }
    }

    private void SubscribeToRpcEvents()
    {
        if (_rpcEventService == null)
        {
            return;
        }

        _rpcEventService.ChatMessageReceived += OnChatMessageReceived;
        _rpcEventService.FileMetadataReceived += OnFileMetadataReceived;
        _rpcEventService.FileSaveSuccess += OnFileSaveSuccess;
        _rpcEventService.FileSaveError += OnFileSaveError;
        _rpcEventService.MessageDeleted += OnMessageDeleted;
    }

    // --- RPC Handlers ---

    private void OnChatMessageReceived(object? sender, RpcDataEventArgs e)
    {
        HandleBackendTextMessage(e.Data.ToArray());
    }

    private void OnFileMetadataReceived(object? sender, RpcDataEventArgs e)
    {
        HandleBackendFileMetadata(e.Data.ToArray());
    }

    private void OnFileSaveSuccess(object? sender, RpcDataEventArgs e)
    {
        HandleFileSaveSuccess(e.Data.ToArray());
    }

    private void OnFileSaveError(object? sender, RpcDataEventArgs e)
    {
        HandleFileSaveError(e.Data.ToArray());
    }

    private void OnMessageDeleted(object? sender, RpcDataEventArgs e)
    {
        HandleBackendDelete(e.Data.ToArray());
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "RPC callbacks must not crash the app")]
    private void HandleBackendTextMessage(byte[] data)
    {
        try
        {
            SerializedChatMessage messageDto = ChatSerializer.Deserialize(data);

            if (messageDto != null)
            {
                Application.Current.Dispatcher.Invoke(() => {
                    HandleIncomingMessage(
                        messageDto.MessageId,
                        messageDto.UserId,
                        messageDto.SenderDisplayName,
                        DateTimeOffset.FromUnixTimeSeconds(messageDto.TimestampEpochSeconds).LocalDateTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
                        messageDto.ReplyToMessageId,
                        messageDto.Content,
                        null, 0, null
                    );
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling text message: {ex.Message}");
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "RPC callbacks must not crash the app")]
    private void HandleBackendFileMetadata(byte[] data)
    {
        try
        {
            SerializedFileMessage messageDto = FileSerializer.Deserialize(data);

            if (messageDto != null)
            {
                long compressedSize = messageDto.FileContent?.Length ?? 0;

                Application.Current.Dispatcher.Invoke(() => {
                    HandleIncomingMessage(
                        messageDto.MessageId,
                        messageDto.UserId,
                        messageDto.SenderDisplayName,
                        DateTimeOffset.FromUnixTimeSeconds(messageDto.TimestampEpochSeconds).LocalDateTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
                        messageDto.ReplyToMessageId,
                        messageDto.Caption,
                        messageDto.FileName,
                        compressedSize,
                        messageDto.FileContent
                    );
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling file metadata: {ex.Message}");
        }
    }

    private void HandleFileSaveSuccess(byte[] data)
    {
        string message = Encoding.UTF8.GetString(data);
        Application.Current.Dispatcher.Invoke(() => _toastService.ShowSuccess($"File saved: {message}"));
    }

    private void HandleFileSaveError(byte[] data)
    {
        string message = Encoding.UTF8.GetString(data);
        Application.Current.Dispatcher.Invoke(() => _toastService.ShowError($"File save error: {message}"));
    }

    private void HandleBackendDelete(byte[] data)
    {
        string messageId = Encoding.UTF8.GetString(data);
        Application.Current.Dispatcher.Invoke(() => {
            ChatMessage? msg = Messages.FirstOrDefault(m => m.MessageId == messageId);
            if (msg != null)
            {
                Messages.Remove(msg);
            }
        });
    }

    private static string FormatTimestamp(string timestamp)
    {
        if (DateTime.TryParse(timestamp, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime dt))
        {
            return dt.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);
        }
        return DateTime.Now.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);
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

    private async void SendTextMessage(string messageText)
    {
        string messageId = Guid.NewGuid().ToString();
        long timestampEpoch = DateTimeOffset.Now.ToUnixTimeSeconds();
        string timestamp = DateTime.Now.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);

        // Optimistic update
        HandleIncomingMessage(
            messageId,
            _currentUser.Email ?? "unknown", // Using Email as ID for now
            _currentUser.DisplayName ?? "User",
            timestamp,
            _currentReplyId,
            messageText,
            null, 0, null
        );

        if (_rpc != null)
        {
            var dto = new SerializedChatMessage(
                messageId,
                _currentUser.Email ?? "unknown",
                _currentUser.DisplayName ?? "User",
                messageText,
                timestampEpoch,
                _currentReplyId ?? ""
            );

            await _rpc.Call("chat:send-text", ChatSerializer.Serialize(dto)).ConfigureAwait(true);
        }
    }

    private async void SendFileMessage(FileInfo file, string caption)
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
        long timestampEpoch = DateTimeOffset.Now.ToUnixTimeSeconds();
        string timestamp = DateTime.Now.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);

        // Optimistic update
        HandleIncomingMessage(
            messageId,
            _currentUser.Email ?? "unknown",
            _currentUser.DisplayName ?? "User",
            timestamp,
            _currentReplyId,
            caption ?? $"Sent {file.Name}",
            file.Name,
            file.Length,
            null
        );

        if (_rpc != null)
        {
            var dto = new SerializedFileMessage(
                messageId,
                _currentUser.Email ?? "unknown",
                _currentUser.DisplayName ?? "User",
                caption ?? "",
                file.Name,
                file.FullName, // Sending path as per Java impl
                _currentReplyId ?? ""
            );

            await _rpc.Call("chat:send-file", FileSerializer.Serialize(dto)).ConfigureAwait(true);
        }
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
        var args = new RequestFileSelectionEventArgs();
        RequestFileSelection?.Invoke(this, args);
        FileInfo? selectedFile = args.SelectedFile;

        if (selectedFile != null && selectedFile.Exists)
        {
            if (selectedFile.Length > MaxFileSizeBytes)
            {
                _toastService.ShowError("File is too large (Max 50MB)");
                return;
            }

            _attachedFile = selectedFile;
            AttachmentText = selectedFile.Name;
        }
    }

    public void CancelAttachment()
    {
        _attachedFile = null;
        AttachmentText = string.Empty;
    }

    public async void DownloadFile(ChatMessage? fileMessage)
    {
        if (fileMessage == null || !fileMessage.IsFileMessage)
        {
            _toastService.ShowError("Invalid file message");
            return;
        }

        if (_rpc != null)
        {
            _toastService.ShowInfo("Requesting file download...");
            await _rpc.Call("chat:save-file-to-disk", Encoding.UTF8.GetBytes(fileMessage.MessageId)).ConfigureAwait(true);
        }
    }

    public async void DeleteMessage(ChatMessage? messageToDelete)
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

        if (_rpc != null)
        {
            await _rpc.Call("chat:delete-message", Encoding.UTF8.GetBytes(messageToDelete.MessageId)).ConfigureAwait(true);
        }
    }

    public void SimulateIncomingMessage()
    {
        string messageId = Guid.NewGuid().ToString();
        string timestamp = DateTime.Now.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);

        HandleIncomingMessage(
            messageId,
            "test-user",
            "Test User",
            timestamp,
            null,
            "Hello! This is a simulated incoming message.",
            null, 0, null
        );
    }

    // --- Helper Methods ---

    private void HandleIncomingMessage(
        string messageId, string userId, string senderDisplayName,
        string formattedTime, string? replyToId,
        string content, string? fileName, long compressedFileSize, byte[]? fileContent)
    {
        if (Messages.Any(m => m.MessageId == messageId))
        {
            return;
        }

        bool isSentByMe = userId == (_currentUser.Email ?? "unknown"); // Or check ID if available
        string username = isSentByMe ? "You" : senderDisplayName;

        string quotedContent = GetQuotedContent(replyToId) ?? string.Empty;

        ChatMessage message = new ChatMessage(
            messageId,
            username,
            content,
            fileName ?? string.Empty,
            compressedFileSize,
            fileContent ?? Array.Empty<byte>(),
            formattedTime,
            isSentByMe,
            quotedContent
        );

        Messages.Add(message);
    }

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

        return $"{sender}: {contentSnippet}";
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


