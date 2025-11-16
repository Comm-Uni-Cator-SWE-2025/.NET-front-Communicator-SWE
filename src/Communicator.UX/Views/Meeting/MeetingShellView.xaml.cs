using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Communicator.UX.Views.Meeting;

/// <summary>
/// Container view that frames all meeting sub-pages and surfaces the active tab content.
/// </summary>
public partial class MeetingShellView : UserControl
{
    private const double DefaultSidePanelWidth = 320;
    private const double MinSidePanelWidth = 250;
    private const double MaxSidePanelWidth = 600;
    private GridLength _previousPanelWidth = new(DefaultSidePanelWidth);

    /// <summary>
    /// Initializes shell components declared in XAML.
    /// </summary>
    public MeetingShellView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        
        // Wire up Enter key handler for Quick Doubt TextBox
        if (FindName("QuickDoubtTextBox") is TextBox quickDoubtTextBox)
        {
            quickDoubtTextBox.PreviewKeyDown += QuickDoubtTextBox_PreviewKeyDown;
        }
    }

    private void QuickDoubtTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            e.Handled = true; // Prevent new line
            
            if (DataContext is ViewModels.Meeting.MeetingShellViewModel viewModel)
            {
                if (viewModel.SendQuickDoubtCommand.CanExecute(null))
                {
                    viewModel.SendQuickDoubtCommand.Execute(null);
                }
            }
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Meeting.MeetingShellViewModel viewModel)
        {
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModels.Meeting.MeetingShellViewModel.IsSidePanelOpen))
        {
            if (DataContext is ViewModels.Meeting.MeetingShellViewModel viewModel)
            {
                ColumnDefinition columnDefinition = ((Grid)Content).ColumnDefinitions[2];

                if (viewModel.IsSidePanelOpen)
                {
                    // Open: Set to previous width or default
                    columnDefinition.Width = _previousPanelWidth.Value > 0
                        ? _previousPanelWidth
                        : new GridLength(DefaultSidePanelWidth);
                    columnDefinition.MinWidth = MinSidePanelWidth;
                    columnDefinition.MaxWidth = MaxSidePanelWidth;
                }
                else
                {
                    // Close: Save current width and collapse
                    if (columnDefinition.Width.Value > 0)
                    {
                        _previousPanelWidth = columnDefinition.Width;
                    }
                    columnDefinition.Width = new GridLength(0);
                    columnDefinition.MinWidth = 0;
                    columnDefinition.MaxWidth = 0;
                }
            }
        }
    }
}
