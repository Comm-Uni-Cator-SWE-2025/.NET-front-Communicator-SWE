using System.Windows.Controls;
using Communicator.UX.Analytics.ViewModels;

namespace Communicator.UX.Analytics.Views;

public partial class AnalyticsView : UserControl
{
    public AnalyticsView()
    {
        InitializeComponent();
        DataContext = new AnalyticsViewModel();
    }
}
