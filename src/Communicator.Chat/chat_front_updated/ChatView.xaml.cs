// ChatView.xaml.cs
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Specialized;
using Microsoft.Win32;

namespace chat_front
{
    public partial class ChatView : Window
    {
        private ChatViewModel ViewModel => DataContext as ChatViewModel;

        public ChatView()
        {
            InitializeComponent();

            // Create the ViewModel and set it as the DataContext
            var rpcPlaceholder = new AbstractRPC();
            var viewModel = new ChatViewModel(rpcPlaceholder);
            DataContext = viewModel;

            // Hook into the Messages collection changing
            viewModel.Messages.CollectionChanged += OnMessagesCollectionChanged;

            // Subscribe to ViewModel events
            viewModel.ShowErrorDialog += ShowErrorDialog;
            viewModel.ShowSuccessDialog += ShowSuccessDialog;
            viewModel.ShowDeleteConfirmation += ShowDeleteConfirmation;
            viewModel.RequestFileSelection += RequestFileSelection;

            // Set focus to the message input box
            Loaded += (s, e) => MessageInputBox.Focus();
        }

        /// <summary>
        /// Shows a file selection dialog and returns the selected file
        /// </summary>
        private FileInfo RequestFileSelection()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select a file to send",
                Filter = "All Files (*.*)|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog(this) == true)
            {
                return new FileInfo(dialog.FileName);
            }

            return null;
        }

        /// <summary>
        /// Shows an error dialog
        /// </summary>
        private void ShowErrorDialog(string message)
        {
            MessageBox.Show(
                message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        /// <summary>
        /// Shows a success dialog
        /// </summary>
        private void ShowSuccessDialog(string message)
        {
            MessageBox.Show(
                message,
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        /// <summary>
        /// Shows a delete confirmation dialog
        /// </summary>
        private void ShowDeleteConfirmation(string messageId)
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete this message?",
                "Delete Message",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes && ViewModel != null)
            {
                ViewModel.ConfirmDeleteMessage(messageId);
            }
        }

        /// <summary>
        /// Scrolls to the bottom when a new message is added
        /// </summary>
        private void OnMessagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // Scroll to end after the UI updates
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageScrollViewer.ScrollToEnd();
                }), System.Windows.Threading.DispatcherPriority.ContextIdle);
            }
        }

        /// <summary>
        /// Clean up event handlers when the window closes
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.Messages.CollectionChanged -= OnMessagesCollectionChanged;
                ViewModel.ShowErrorDialog -= ShowErrorDialog;
                ViewModel.ShowSuccessDialog -= ShowSuccessDialog;
                ViewModel.ShowDeleteConfirmation -= ShowDeleteConfirmation;
                ViewModel.RequestFileSelection -= RequestFileSelection;
            }
            base.OnClosed(e);
        }
    }
}