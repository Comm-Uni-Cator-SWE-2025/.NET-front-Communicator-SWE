using System.Windows;
using AnalyticsApp.ViewModels;

namespace AnalyticsApp.Views;

/// <summary>
/// Interaction logic for the main application window.
/// Initializes the UI and assigns the primary view model.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Creates a new instance of <see cref="MainWindow"/> and sets up data binding.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        // Assign the main ViewModel to the DataContext
        DataContext = new MainPageViewModel();
    }
}
