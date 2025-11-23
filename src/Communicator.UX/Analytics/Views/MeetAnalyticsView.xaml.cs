using System.Windows.Controls;
using Communicator.UX.Analytics.ViewModels;

namespace Communicator.UX.Analytics.Views;

public partial class MeetAnalyticsView : UserControl
{
    public MeetAnalyticsView()
    {
        InitializeComponent();
        DataContext = new MeetAnalyticsViewModel();
    }
}
