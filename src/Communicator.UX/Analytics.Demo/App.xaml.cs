using System.Windows;
using Communicator.UX.Analytics.Views;

namespace Communicator.UX.Analytics.Demo;

public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // Window 1: Analytics View
        var window1 = new Window
        {
            Title = "Analytics Dashboard (Demo)",
            Content = new AnalyticsView(),
            Width = 1200,
            Height = 800
        };
        window1.Show();

        // Window 2: Meet Analytics View
        var window2 = new Window
        {
            Title = "Meet Analytics (Demo)",
            Content = new MeetAnalyticsView(),
            Width = 650,
            Height = 450
        };
        window2.Show();
    }
}
