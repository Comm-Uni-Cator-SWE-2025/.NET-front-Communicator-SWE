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

        // Load saved theme preference
        IThemeService themeService = Services.GetRequiredService<IThemeService>();
        themeService.LoadSavedTheme();

        // Subscribe to RPC methods BEFORE starting connection (like Java)
        IRPC rpc = Services.GetRequiredService<IRPC>();
        IRpcEventService rpcEventService = Services.GetRequiredService<IRpcEventService>();
        SubscribeRpcMethods(rpc, rpcEventService);
        
        // Start RPC connection in background thread (like Java does)
        // This allows UI to appear while waiting for backend to connect
        StartRpcConnectionInBackground(rpc, e.Args);

        // Create and show main window with DI
        MainViewModel mainViewModel = Services.GetRequiredService<MainViewModel>();
        var mainView = new MainView {
            DataContext = mainViewModel
        };
        mainView.Show();
    }

    /// <summary>
    /// Subscribes to RPC methods that the backend may call.
    /// Must be called BEFORE Connect(), matching Java frontend pattern.
    /// </summary>
    private static void SubscribeRpcMethods(IRPC rpc, IRpcEventService rpcEventService)
    {
        System.Diagnostics.Debug.WriteLine("[App] Subscribing to RPC methods...");
        
        // Subscribe to "subscribeAsViewer" - called when a new participant joins
        // Matches Java: rpc.subscribe(Utils.SUBSCRIBE_AS_VIEWER, ...)
        rpc.Subscribe("subscribeAsViewer", (byte[] data) => {
            try
            {
                string viewerIP = System.Text.Encoding.UTF8.GetString(data);
                System.Diagnostics.Debug.WriteLine($"[App] New viewer subscribed: {viewerIP}");
                
                rpcEventService.RaiseParticipantJoined(viewerIP);
                
                return Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Error in subscribeAsViewer: {ex.Message}");
                return Array.Empty<byte>();
            }
        });

        // Subscribe to UPDATE_UI to receive video/screen frames
        rpc.Subscribe(Communicator.ScreenShare.Utils.UPDATE_UI, (byte[] data) => {
            try
            {
                rpcEventService.RaiseFrameReceived(data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Error in UPDATE_UI: {ex.Message}");
            }
            return Array.Empty<byte>();
        });

        // Subscribe to STOP_SHARE to clear screen frames
        rpc.Subscribe(Communicator.ScreenShare.Utils.STOP_SHARE, (byte[] data) => {
            try
            {
                rpcEventService.RaiseStopShareReceived(data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Error in STOP_SHARE: {ex.Message}");
            }
            return Array.Empty<byte>();
        });
        
        System.Diagnostics.Debug.WriteLine("[App] RPC method subscriptions complete");
    }

    /// <summary>
    /// Starts RPC connection in a background thread.
    /// This matches Java frontend pattern where connect() is called and returns a Thread.
    /// The UI can appear while we wait for backend to connect.
    /// </summary>
    private static void StartRpcConnectionInBackground(IRPC rpc, string[] args)
    {
        System.Diagnostics.Debug.WriteLine("[App] Starting RPC connection in background thread...");
        
        // Start RPC server in background thread - matches Java: rpc.connect() returns Thread
        var rpcTask = Task.Run(() =>
        {
            try
            {
                int portNumber = 6942;
                if (args.Length > 0 && int.TryParse(args[0], out int parsedPort))
                {
                    portNumber = parsedPort;
                }

                System.Diagnostics.Debug.WriteLine($"[App] RPC thread: Connecting to port {portNumber}...");
                
                // This will BLOCK until backend connects and completes handshake
                // Just like Java: new SocketryServer(portNumber, methods)
                Thread rpcThread = rpc.Connect(portNumber);
                
                System.Diagnostics.Debug.WriteLine("[App] RPC thread: Backend connected, server running");
                
                // The rpcThread is now running listenLoop in background
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] RPC thread error: {ex.Message}");
                Console.Error.WriteLine($"[App] RPC connection failed: {ex}");
                
                // Show error on UI thread
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        $"Failed to establish RPC connection: {ex.Message}\n\nSome features may not work.",
                        "Connection Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                });
            }
        });
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

        // Register Cloud services (Cloud messaging for real-time features)
        services.AddSingleton<ICloudConfigService, CloudConfigService>();
        services.AddSingleton<ICloudMessageService, CloudMessageService>();

        // Register RPC Event Service
        services.AddSingleton<IRpcEventService, RpcEventService>();

        // Register RPC Service (for authentication via Controller backend)
        services.AddSingleton<IRPC, RPCService>();

        // Register ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<ViewModels.Auth.AuthViewModel>();
        services.AddTransient<ViewModels.Home.HomePageViewModel>();
        services.AddTransient<ViewModels.Settings.SettingsViewModel>();
        services.AddTransient<ViewModels.Meeting.MeetingSessionViewModel>();

        // Register ToastContainerViewModel as Singleton (single toast container for app)
        services.AddSingleton<ViewModels.Common.ToastContainerViewModel>();

        // Register ViewModel Factories
        // Factory for creating AuthViewModel instances (used in MainViewModel)
        services.AddTransient<Func<ViewModels.Auth.AuthViewModel>>(sp =>
            () => sp.GetRequiredService<ViewModels.Auth.AuthViewModel>());

        // Factory for creating ViewModels with UserProfile parameter
        // Using ActivatorUtilities.CreateInstance for automatic dependency resolution
        services.AddTransient<Func<UserProfile, ViewModels.Home.HomePageViewModel>>(sp =>
            user => ActivatorUtilities.CreateInstance<ViewModels.Home.HomePageViewModel>(sp, user));

        services.AddTransient<Func<UserProfile, ViewModels.Settings.SettingsViewModel>>(sp =>
            user => ActivatorUtilities.CreateInstance<ViewModels.Settings.SettingsViewModel>(sp, user));

        // Factory for creating MeetingSessionViewModel with UserProfile and optional MeetingSession
        services.AddTransient<Func<UserProfile, MeetingSession?, ViewModels.Meeting.MeetingSessionViewModel>>(sp =>
            (user, session) => ActivatorUtilities.CreateInstance<ViewModels.Meeting.MeetingSessionViewModel>(sp, user, session));
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

