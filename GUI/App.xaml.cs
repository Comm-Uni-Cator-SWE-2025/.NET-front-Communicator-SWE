using System;
using System.Windows;
using System.Windows.Threading;
using Controller;
using Microsoft.Extensions.DependencyInjection;
using UX.Core;
using UX.Core.Services;
using GUI.ViewModels;
using GUI.Views;

namespace GUI;

public partial class App : Application
{
    // Dependency Injection Service Provider
    public static IServiceProvider Services { get; private set; } = null!;

    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure Dependency Injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Load saved theme preference
        var themeService = Services.GetRequiredService<IThemeService>();
        themeService.LoadSavedTheme();

        // Create and show main window with DI
        var mainViewModel = Services.GetRequiredService<MainViewModel>();
        var mainView = new MainView
        {
            DataContext = mainViewModel
        };

        mainView.Show();
    }

    /// <summary>
    /// Configures all application services for dependency injection.
    /// </summary>
    private void ConfigureServices(IServiceCollection services)
    {
        // Register UX.Core services (Toast, Theme)
        services.AddUXCoreServices();

        // Register GUI-specific services
        services.AddSingleton<INavigationService, Services.NavigationService>();

        // Register Controller services
        services.AddSingleton<IController, MockController>();

        // Register ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<Controller.UI.ViewModels.AuthViewModel>();
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

