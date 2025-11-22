using System.Windows;
using MeetAnalyticsApp.ViewModels;

namespace MeetAnalyticsApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
