using System.Windows.Controls;

namespace Communicator.UX.Views.Meeting;

/// <summary>
/// Container view that frames all meeting sub-pages and surfaces the active tab content.
/// </summary>
public partial class MeetingShellView : UserControl
{
    /// <summary>
    /// Initializes shell components declared in XAML.
    /// </summary>
    public MeetingShellView()
    {
        InitializeComponent();
    }
}

