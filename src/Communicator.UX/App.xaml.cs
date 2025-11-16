using System;
using System.Windows;
using System.Windows.Threading;
using Communicator.Controller;
using Communicator.Core.RPC;
using Communicator.Core.UX;
using Communicator.Core.UX.Services;
using Communicator.UX.Services;
using Communicator.UX.ViewModels;
using Communicator.UX.Views;
using Controller;
using Microsoft.Extensions.DependencyInjection;

namespace Communicator.UX;

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
        IThemeService themeService = Services.GetRequiredService<IThemeService>();
        themeService.LoadSavedTheme();

        // Create and show main window with DI
        MainViewModel mainViewModel = Services.GetRequiredService<MainViewModel>();
        var mainView = new MainView {
            DataContext = mainViewModel
        };

        mainView.Show();
    }

    /// <summary>
    /// Configures all application services for dependency injection.
    /// </summary>
    private static void ConfigureServices(IServiceCollection services)
    {
        // Register Communicator.Core.UX services (Toast, Theme)
        services.AddUXCoreServices();

        // Register Communicator.UX services
        services.AddSingleton<INavigationService, Services.NavigationService>();
        services.AddSingleton<IAuthenticationService, Services.AuthenticationService>();

        // Register RPC Service (for authentication via Controller backend)
        services.AddSingleton<IRPC, RPCService>();

        // Register Controller services (Mock for demo purposes - will be replaced by RPC)
        services.AddSingleton<IController, MockController>();

        // Register ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<Communicator.UX.ViewModels.Auth.AuthViewModel>();
        services.AddTransient<Communicator.UX.ViewModels.Home.HomePageViewModel>();
        services.AddTransient<Communicator.UX.ViewModels.Settings.SettingsViewModel>();
        services.AddTransient<Communicator.UX.ViewModels.Meeting.MeetingShellViewModel>();

        // Register ToastContainerViewModel as Singleton (single toast container for app)
        services.AddSingleton<Communicator.UX.ViewModels.Common.ToastContainerViewModel>();

        // Register ViewModel Factories
        // Factory for creating AuthViewModel instances (used in MainViewModel)
        services.AddTransient<Func<Communicator.UX.ViewModels.Auth.AuthViewModel>>(sp =>
            () => sp.GetRequiredService<Communicator.UX.ViewModels.Auth.AuthViewModel>());

        // Factory for creating ViewModels with User parameter
        // Using ActivatorUtilities.CreateInstance for automatic dependency resolution
        services.AddTransient<Func<User, Communicator.UX.ViewModels.Home.HomePageViewModel>>(sp =>
            user => ActivatorUtilities.CreateInstance<Communicator.UX.ViewModels.Home.HomePageViewModel>(sp, user));

        services.AddTransient<Func<User, Communicator.UX.ViewModels.Settings.SettingsViewModel>>(sp =>
            user => ActivatorUtilities.CreateInstance<Communicator.UX.ViewModels.Settings.SettingsViewModel>(sp, user));

        services.AddTransient<Func<User, Communicator.UX.ViewModels.Meeting.MeetingShellViewModel>>(sp =>
            user => ActivatorUtilities.CreateInstance<Communicator.UX.ViewModels.Meeting.MeetingShellViewModel>(sp, user));
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

