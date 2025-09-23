using System;
using System.Windows;
using System.Windows.Threading;
using Controller;
using GUI.Services;
using GUI.ViewModels;
using GUI.Views;

namespace GUI;

public partial class App : Application
{
    // Singleton Services following Dependency Injection pattern
    public static IController? Controller { get; private set; }
    public static IToastService ToastService { get; private set; } = new ToastService();
    public static IThemeService ThemeService { get; private set; } = new ThemeService();
    public static INavigationService NavigationService { get; private set; } = new NavigationService();

    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize services
        Controller = new MockController();
        
        // Load saved theme preference
        ThemeService.LoadSavedTheme();

        // Create and show main window
        var mainView = new MainView
        {
            DataContext = new MainViewModel()
        };

        mainView.Show();
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        ReportException(e.Exception);
        e.Handled = true;
    }

    private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            ReportException(ex);
        }
    }

    private static void ReportException(Exception exception)
    {
        Console.Error.WriteLine(exception);
        MessageBox.Show(exception.ToString(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}