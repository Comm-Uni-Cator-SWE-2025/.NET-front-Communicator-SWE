using System.IO;
using System.Windows.Controls;
using Communicator.UX.ViewModels.Meeting;
using Microsoft.Win32;

namespace Communicator.UX.Views.Meeting;

/// <summary>
/// Interaction logic for ChatView.xaml
/// </summary>
public partial class ChatView : UserControl
{
    public ChatView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        // Unsubscribe from old ViewModel if exists
        if (e.OldValue is ChatViewModel oldViewModel)
        {
            oldViewModel.RequestFileSelection -= OnRequestFileSelection;
        }

        // Subscribe to new ViewModel if exists
        if (e.NewValue is ChatViewModel newViewModel)
        {
            newViewModel.RequestFileSelection += OnRequestFileSelection;
        }
    }

    /// <summary>
    /// Shows a file selection dialog and returns the selected file
    /// </summary>
    private FileInfo? OnRequestFileSelection()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select a file to send",
            Filter = "All Files (*.*)|*.*|" +
                     "Images (*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|" +
                     "Documents (*.pdf;*.doc;*.docx;*.txt)|*.pdf;*.doc;*.docx;*.txt|" +
                     "Archives (*.zip;*.rar;*.7z)|*.zip;*.rar;*.7z",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog() == true)
        {
            return new FileInfo(dialog.FileName);
        }

        return null;
    }
}
