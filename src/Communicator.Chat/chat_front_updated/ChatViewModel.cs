// ChatViewModel.cs

using chat_front_updated;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace chat_front
{
    // --- BASIC MVVM IMPLEMENTATION ---
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // --- SIMPLE COMMAND IMPLEMENTATION ---
    public class SimpleCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public SimpleCommand(Action execute)
            : this(execute, null)
        {
        }

        public SimpleCommand(Action execute, Func<bool> canExecute)
            : this(_ => execute(), canExecute != null ? _ => canExecute() : (Func<object, bool>)null)
        {
        }

        public SimpleCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter) => _execute(parameter);
    }

    // --- RPC AND MESSAGE CLASSES ---
    public class AbstractRPC
    {
        public virtual void Subscribe(string topic, Func<byte[], byte[]> handler) { }
        public virtual Task<byte[]> CallAsync(string method, byte[] data)
        {
            return Task.FromResult(new byte[0]);
        }
    }

    public class ChatMessage
    {
        public string MessageId { get; set; }
        public string UserId { get; set; }
        public string SenderDisplayName { get; set; }
        public string Content { get; set; }
        public string ReplyToMessageId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public ChatMessage(string id, string userId, string displayName, string content, string replyId)
        {
            MessageId = id;
            UserId = userId;
            SenderDisplayName = displayName;
            Content = content;
            ReplyToMessageId = replyId;
        }
    }

    public class FileMessage
    {
        public string MessageId { get; set; }
        public string UserId { get; set; }
        public string SenderDisplayName { get; set; }
        public string Caption { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public byte[] FileContent { get; set; }
        public string ReplyToMessageId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public FileMessage(string id, string userId, string displayName,
                          string caption, string fileName, string filePath, string replyId)
        {
            MessageId = id;
            UserId = userId;
            SenderDisplayName = displayName;
            Caption = caption;
            FileName = fileName;
            FilePath = filePath;
            ReplyToMessageId = replyId;
        }
    }

    public static class MessageParser
    {
        public static string SerializeChatMessage(ChatMessage message)
        {
            // Simplified - replace with actual JSON serialization
            return $"{{\"id\":\"{message.MessageId}\",\"userId\":\"{message.UserId}\"}}";
        }

        public static ChatMessage DeserializeChatMessage(string json)
        {
            // Simplified - replace with actual JSON deserialization
            return new ChatMessage(Guid.NewGuid().ToString(), "backend_user", "Backend", "Test message", null);
        }

        public static string SerializeFileMessage(FileMessage message)
        {
            // Simplified - replace with actual JSON serialization
            return $"{{\"id\":\"{message.MessageId}\",\"fileName\":\"{message.FileName}\"}}";
        }

        public static FileMessage DeserializeFileMessage(string json)
        {
            // Simplified - replace with actual JSON deserialization
            return new FileMessage(Guid.NewGuid().ToString(), "backend_user", "Backend",
                                  "File caption", "test.txt", "", null);
        }
    }

    /// <summary>
    /// The ViewModel holding chat logic and data with file sharing support.
    /// </summary>
    public class ChatViewModel : BaseViewModel
    {
        // --- Constants ---
        private const long MAX_FILE_SIZE_BYTES = 50 * 1024 * 1024; // 50 MB

        // --- Model / Backend Dependency ---
        private readonly AbstractRPC _rpc;
        private readonly string _currentUserId = "user-" + Guid.NewGuid().ToString().Substring(0, 8);
        private readonly string _currentDisplayName = "You";
        private readonly Dictionary<string, ChatMessage> _messageHistory = new Dictionary<string, ChatMessage>();

        // --- State for View ---
        public ObservableCollection<MessageVM> Messages { get; } = new ObservableCollection<MessageVM>();

        private string _messageInput = string.Empty;
        public string MessageInput
        {
            get => _messageInput;
            set
            {
                if (_messageInput != value)
                {
                    _messageInput = value;
                    OnPropertyChanged(nameof(MessageInput));
                }
            }
        }

        private string _replyQuoteText;
        public string ReplyQuoteText
        {
            get => _replyQuoteText;
            set
            {
                if (_replyQuoteText != value)
                {
                    _replyQuoteText = value;
                    OnPropertyChanged(nameof(ReplyQuoteText));
                    OnPropertyChanged(nameof(IsReplying));
                }
            }
        }

        private string _attachmentText;
        public string AttachmentText
        {
            get => _attachmentText;
            set
            {
                if (_attachmentText != value)
                {
                    _attachmentText = value;
                    OnPropertyChanged(nameof(AttachmentText));
                    OnPropertyChanged(nameof(HasAttachment));
                }
            }
        }

        public bool IsReplying => !string.IsNullOrEmpty(ReplyQuoteText);
        public bool HasAttachment => !string.IsNullOrEmpty(AttachmentText);

        private string _currentReplyId = null;
        private FileInfo _attachedFile = null;

        // --- Commands ---
        public ICommand SendMessageCommand { get; }
        public ICommand CancelReplyCommand { get; }
        public ICommand StartReplyCommand { get; }
        public ICommand TestRecvCommand { get; }
        public ICommand AttachFileCommand { get; }
        public ICommand CancelAttachmentCommand { get; }
        public ICommand DownloadFileCommand { get; }
        public ICommand DeleteMessageCommand { get; }

        // --- Events for View to Handle ---
        public event Action<string> ShowErrorDialog;
        public event Action<string> ShowSuccessDialog;
        public event Action<string> ShowDeleteConfirmation;
        public event Func<FileInfo> RequestFileSelection;

        // --- Constructor ---
        public ChatViewModel(AbstractRPC rpc)
        {
            _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));

            // Bind commands
            SendMessageCommand = new SimpleCommand(OnSendMessage, CanSendMessage);
            CancelReplyCommand = new SimpleCommand(CancelReply);
            TestRecvCommand = new SimpleCommand(SimulateIncomingMessage);
            StartReplyCommand = new SimpleCommand(p => StartReply(p as MessageVM));
            AttachFileCommand = new SimpleCommand(OnAttachFile);
            CancelAttachmentCommand = new SimpleCommand(CancelAttachment);
            DownloadFileCommand = new SimpleCommand(p => DownloadFile(p as MessageVM));
            DeleteMessageCommand = new SimpleCommand(p => DeleteMessage(p as MessageVM));

            // Subscribe to backend events
            _rpc.Subscribe("chat:new-message", HandleBackendTextMessage);
            _rpc.Subscribe("chat:file-metadata-received", HandleBackendFileMetadata);
            _rpc.Subscribe("chat:file-saved-success", HandleFileSaveSuccess);
            _rpc.Subscribe("chat:file-saved-error", HandleFileSaveError);
            _rpc.Subscribe("chat:message-deleted", HandleBackendDelete);
        }

        // --- RPC Handlers ---
        private byte[] HandleBackendTextMessage(byte[] data)
        {
            var messageJson = Encoding.UTF8.GetString(data);
            ChatMessage message = MessageParser.DeserializeChatMessage(messageJson);

            Application.Current?.Dispatcher.Invoke(() =>
            {
                HandleIncomingMessage(
                    message.MessageId,
                    message.UserId,
                    message.SenderDisplayName,
                    message.Timestamp.ToString("HH:mm"),
                    message.ReplyToMessageId,
                    message.Content,
                    null, 0, null
                );
            });

            return new byte[0];
        }

        private byte[] HandleBackendFileMetadata(byte[] data)
        {
            Console.WriteLine("[FRONT] Received file metadata (no data attached)");

            try
            {
                var messageJson = Encoding.UTF8.GetString(data);
                FileMessage message = MessageParser.DeserializeFileMessage(messageJson);

                if (message == null || string.IsNullOrEmpty(message.FileName))
                {
                    Console.WriteLine("[FRONT] Invalid file metadata");
                    return new byte[0];
                }

                long compressedSize = (message.FileContent != null) ? message.FileContent.Length : 0;

                Application.Current?.Dispatcher.Invoke(() =>
                {
                    HandleIncomingMessage(
                        message.MessageId,
                        message.UserId,
                        message.SenderDisplayName,
                        message.Timestamp.ToString("HH:mm"),
                        message.ReplyToMessageId,
                        message.Caption,
                        message.FileName,
                        compressedSize,
                        null
                    );
                });
            }
            catch (Exception e)
            {
                Console.WriteLine($"[FRONT] Failed to deserialize file metadata: {e.Message}");
            }

            return new byte[0];
        }

        private byte[] HandleFileSaveSuccess(byte[] data)
        {
            string message = Encoding.UTF8.GetString(data);
            Application.Current?.Dispatcher.Invoke(() =>
            {
                ShowSuccessDialog?.Invoke($"File saved successfully!\n{message}");
            });
            return new byte[0];
        }

        private byte[] HandleFileSaveError(byte[] data)
        {
            string message = Encoding.UTF8.GetString(data);
            Application.Current?.Dispatcher.Invoke(() =>
            {
                ShowErrorDialog?.Invoke($"Failed to save file: {message}");
            });
            return new byte[0];
        }

        private byte[] HandleBackendDelete(byte[] messageIdBytes)
        {
            string messageId = Encoding.UTF8.GetString(messageIdBytes);

            Application.Current?.Dispatcher.Invoke(() =>
            {
                _messageHistory.Remove(messageId);

                var messageToRemove = Messages.FirstOrDefault(m => m.MessageId == messageId);
                if (messageToRemove != null)
                {
                    Messages.Remove(messageToRemove);
                }
            });

            return new byte[0];
        }

        // --- User Actions ---
        private bool CanSendMessage()
        {
            return _attachedFile != null || !string.IsNullOrWhiteSpace(MessageInput);
        }

        public void OnSendMessage()
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
            var messageId = Guid.NewGuid().ToString();
            var messageToSend = new ChatMessage(
                messageId,
                _currentUserId,
                _currentDisplayName,
                messageText,
                _currentReplyId
            );

            byte[] messageBytes = Encoding.UTF8.GetBytes(MessageParser.SerializeChatMessage(messageToSend));
            SendRpcAsync("chat:send-text", messageBytes);

            // Optimistic update
            HandleIncomingMessage(
                messageToSend.MessageId,
                messageToSend.UserId,
                messageToSend.SenderDisplayName,
                messageToSend.Timestamp.ToString("HH:mm"),
                messageToSend.ReplyToMessageId,
                messageToSend.Content,
                null, 0, null
            );
        }

        private void SendFileMessage(FileInfo file, string caption)
        {
            string cleanPath = file.FullName.Trim();
            if (cleanPath.StartsWith("*"))
            {
                cleanPath = cleanPath.Substring(1).Trim();
            }

            if (string.IsNullOrEmpty(cleanPath))
            {
                ShowErrorDialog?.Invoke("File path is empty or invalid!");
                return;
            }

            var messageId = Guid.NewGuid().ToString();
            var messageToSend = new FileMessage(
                messageId,
                _currentUserId,
                _currentDisplayName,
                caption,
                file.Name,
                cleanPath,
                _currentReplyId
            );

            byte[] messageBytes = Encoding.UTF8.GetBytes(MessageParser.SerializeFileMessage(messageToSend));
            SendRpcAsync("chat:send-file", messageBytes);

            // Optimistic update
            HandleIncomingMessage(
                messageToSend.MessageId,
                messageToSend.UserId,
                messageToSend.SenderDisplayName,
                messageToSend.Timestamp.ToString("HH:mm"),
                messageToSend.ReplyToMessageId,
                caption,
                file.Name,
                file.Length,
                null
            );
        }

        public void DownloadFile(MessageVM fileMessage)
        {
            if (fileMessage == null || !fileMessage.IsFileMessage)
            {
                ShowErrorDialog?.Invoke("Invalid file message");
                return;
            }

            Console.WriteLine("[FRONT] User clicked 'Save'. Requesting backend to decompress and save.");

            byte[] messageIdBytes = Encoding.UTF8.GetBytes(fileMessage.MessageId);

            Task.Run(async () =>
            {
                try
                {
                    await _rpc.CallAsync("chat:save-file-to-disk", messageIdBytes);
                    Console.WriteLine("[FRONT] Backend finished saving file");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[FRONT] Failed to request save: {e.Message}");
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        ShowErrorDialog?.Invoke($"Save failed: {e.Message}");
                    });
                }
            });
        }

        public void OnAttachFile()
        {
            var selectedFile = RequestFileSelection?.Invoke();
            if (selectedFile != null)
            {
                UserSelectedFileToAttach(selectedFile);
            }
        }

        private void UserSelectedFileToAttach(FileInfo selectedFile)
        {
            if (selectedFile == null) return;

            if (selectedFile.Length > MAX_FILE_SIZE_BYTES)
            {
                ShowErrorDialog?.Invoke("File is too large (Max 50MB).");
                return;
            }

            _attachedFile = selectedFile;
            AttachmentText = $"Attached: {selectedFile.Name}";
        }

        public void DeleteMessage(MessageVM messageToDelete)
        {
            if (messageToDelete == null || !messageToDelete.IsSentByMe) return;

            // Request confirmation from view
            ShowDeleteConfirmation?.Invoke(messageToDelete.MessageId);
        }

        public void ConfirmDeleteMessage(string messageId)
        {
            byte[] messageIdBytes = Encoding.UTF8.GetBytes(messageId);
            SendRpcAsync("chat:delete-message", messageIdBytes);

            _messageHistory.Remove(messageId);
            var messageToRemove = Messages.FirstOrDefault(m => m.MessageId == messageId);
            if (messageToRemove != null)
            {
                Messages.Remove(messageToRemove);
            }

            if (_currentReplyId != null && _currentReplyId.Equals(messageId))
            {
                CancelReply();
            }
        }

        public void StartReply(MessageVM messageToReply)
        {
            if (messageToReply == null) return;

            _currentReplyId = messageToReply.MessageId;

            string quoteText = messageToReply.IsFileMessage
                ? $"Replying to file: {messageToReply.FileName}"
                : $"Replying to {messageToReply.Username}: {messageToReply.Content.Substring(0, Math.Min(messageToReply.Content.Length, 20))}...";

            ReplyQuoteText = quoteText;
        }

        public void CancelReply()
        {
            _currentReplyId = null;
            ReplyQuoteText = null;
        }

        public void CancelAttachment()
        {
            _attachedFile = null;
            AttachmentText = null;
        }

        public void SimulateIncomingMessage()
        {
            var messageId = Guid.NewGuid().ToString();
            var fakeMessage = new ChatMessage(
                messageId,
                "backend_user",
                "Test User",
                "Hello from the server test!",
                null
            );

            HandleIncomingMessage(
                fakeMessage.MessageId,
                fakeMessage.UserId,
                fakeMessage.SenderDisplayName,
                fakeMessage.Timestamp.ToString("HH:mm"),
                fakeMessage.ReplyToMessageId,
                fakeMessage.Content,
                null, 0, null
            );
        }

        // --- Core Logic ---
        private void HandleIncomingMessage(
            string messageId, string userId, string senderDisplayName,
            string formattedTime, string replyToId,
            string content,
            string fileName,
            long compressedFileSize,
            byte[] fileContent)
        {
            if (_messageHistory.ContainsKey(messageId))
            {
                Console.WriteLine($"[FRONT] Duplicate message ignored: {messageId}");
                return;
            }

            var isSentByMe = userId.Equals(_currentUserId, StringComparison.OrdinalIgnoreCase);
            var username = isSentByMe ? "You" : senderDisplayName;

            string quotedContent = null;
            if (!string.IsNullOrEmpty(replyToId))
            {
                var repliedToVM = Messages.FirstOrDefault(m => m.MessageId == replyToId);
                if (repliedToVM != null)
                {
                    string sender = repliedToVM.IsSentByMe ? "You" : repliedToVM.Username;
                    string contentSnippet = repliedToVM.IsFileMessage
                        ? $"File: {repliedToVM.FileName}"
                        : repliedToVM.Content.Substring(0, Math.Min(repliedToVM.Content.Length, 20)) + "...";
                    quotedContent = $"Replying to {sender}: {contentSnippet}";
                }
                else
                {
                    quotedContent = "Reply to unavailable message";
                }
            }

            var vm = new MessageVM(
                messageId,
                username,
                content,
                fileName,
                compressedFileSize,
                fileContent,
                formattedTime,
                isSentByMe,
                quotedContent
            );

            Messages.Add(vm);
        }

        // --- RPC Helper ---
        private async void SendRpcAsync(string endpoint, byte[] data)
        {
            try
            {
                var response = await _rpc.CallAsync(endpoint, data);
                if (response != null && response.Length > 0)
                {
                    Console.WriteLine("[FRONT] Received response from backend");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[FRONT] RPC call failed: {e.Message}");
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    ShowErrorDialog?.Invoke($"RPC call failed: {e.Message}");
                });
            }
        }
    }
}