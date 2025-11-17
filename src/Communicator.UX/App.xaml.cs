using System;
using System.Windows;
using System.Windows.Threading;
using Communicator.Controller;
using Communicator.Controller.Meeting;
using Communicator.Core.RPC;
using Communicator.Core.UX;
using Communicator.Core.UX.Services;
using Communicator.UX.Services;
using Communicator.UX.ViewModels;
using Communicator.UX.Views;
using Microsoft.Extensions.Configuration;
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

        // Initialize RPC connection
        InitializeRpcConnection(e.Args);

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
    /// Initializes RPC connection on application startup.
    /// </summary>
    private static void InitializeRpcConnection(string[] args)
    {
        try
        {
            // Default port number
            int portNumber = 6942;

            // Parse port from command line args if provided
            if (args.Length > 0 && int.TryParse(args[0], out int parsedPort))
            {
                portNumber = parsedPort;
            }

            IRPC rpc = Services.GetRequiredService<IRPC>();
            System.Diagnostics.Debug.WriteLine($"[App] Connecting RPC on port {portNumber}...");
            
            System.Threading.Thread rpcThread = rpc.Connect(portNumber);
            System.Diagnostics.Debug.WriteLine("[App] RPC connected successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] Warning: Could not initialize RPC: {ex.Message}");
            // Don't crash the app if RPC fails
        }
    }

    /// <summary>
    /// Configures all application services for dependency injection.
    /// </summary>
    private static void ConfigureServices(IServiceCollection services)
    {
        // Register Configuration (loads appsettings.json)
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Register Communicator.Core.UX services (Toast, Theme)
        services.AddUXCoreServices();

        // Register Communicator.UX services
        services.AddSingleton<INavigationService, Services.NavigationService>();
        services.AddSingleton<IAuthenticationService, Services.AuthenticationService>();

        // Register Cloud services (HandWave feature)
        services.AddSingleton<ICloudConfigService, CloudConfigService>();
        services.AddSingleton<IHandWaveService, HandWaveService>();

        // Register RPC Service (for authentication via Controller backend)
        services.AddSingleton<IRPC, RPCService>();

        // Register ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<Communicator.UX.ViewModels.Auth.AuthViewModel>();
        services.AddTransient<Communicator.UX.ViewModels.Home.HomePageViewModel>();
        services.AddTransient<Communicator.UX.ViewModels.Settings.SettingsViewModel>();
        services.AddTransient<Communicator.UX.ViewModels.Meeting.MeetingSessionViewModel>();

        // Register ToastContainerViewModel as Singleton (single toast container for app)
        services.AddSingleton<Communicator.UX.ViewModels.Common.ToastContainerViewModel>();

        // Register ViewModel Factories
        // Factory for creating AuthViewModel instances (used in MainViewModel)
        services.AddTransient<Func<Communicator.UX.ViewModels.Auth.AuthViewModel>>(sp =>
            () => sp.GetRequiredService<Communicator.UX.ViewModels.Auth.AuthViewModel>());

        // Factory for creating ViewModels with UserProfile parameter
        // Using ActivatorUtilities.CreateInstance for automatic dependency resolution
        services.AddTransient<Func<UserProfile, Communicator.UX.ViewModels.Home.HomePageViewModel>>(sp =>
            user => ActivatorUtilities.CreateInstance<Communicator.UX.ViewModels.Home.HomePageViewModel>(sp, user));

        services.AddTransient<Func<UserProfile, Communicator.UX.ViewModels.Settings.SettingsViewModel>>(sp =>
            user => ActivatorUtilities.CreateInstance<Communicator.UX.ViewModels.Settings.SettingsViewModel>(sp, user));

        services.AddTransient<Func<UserProfile, Communicator.UX.ViewModels.Meeting.MeetingSessionViewModel>>(sp =>
            user => ActivatorUtilities.CreateInstance<Communicator.UX.ViewModels.Meeting.MeetingSessionViewModel>(sp, user));
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

