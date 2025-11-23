/*
 * -----------------------------------------------------------------------------
 *  File: App.xaml.cs
 *  Owner: Geetheswar V
 *  Roll Number : 142201025
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.Windows;
using System.Windows.Threading;
using Communicator.Controller;
using Communicator.Controller.Meeting;
using Communicator.Core.RPC;
using Communicator.Core.UX;
using Communicator.Core.UX.Services;
using Communicator.App.Services;
using Communicator.App.ViewModels;
using Communicator.App.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Communicator.App;

public sealed partial class MainApp : Application
{
    // Dependency Injection Service Provider
    public static IServiceProvider Services { get; private set; } = null!;

    public MainApp()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        // Load environment variables from .env file
        DotNetEnv.Env.TraversePath().Load();

        base.OnStartup(e);

        // Configure Dependency Injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Load saved theme preference
        IThemeService themeService = Services.GetRequiredService<IThemeService>();
        themeService.LoadSavedTheme();

        // Subscribe to UserLoggedIn to sync theme with cloud
        IAuthenticationService authService = Services.GetRequiredService<IAuthenticationService>();
        authService.UserLoggedIn += (s, args) => {
            if (args.User != null && !string.IsNullOrEmpty(args.User.Email))
            {
                themeService.SetUser(args.User.Email);
            }
        };

        // Subscribe to RPC methods BEFORE starting connection (like Java)
        IRPC rpc = Services.GetRequiredService<IRPC>();
        IRpcEventService rpcEventService = Services.GetRequiredService<IRpcEventService>();
        SubscribeRpcMethods(rpc, rpcEventService);

        // Start RPC connection in background thread (like Java does)
        // This allows UI to appear while waiting for backend to connect
        StartRpcConnectionInBackground(rpc, e.Args);
    }

    /// <summary>
    /// Subscribes to RPC methods that the backend may call.
    /// Must be called BEFORE Connect(), matching Java frontend pattern.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "RPC callbacks must not crash the app")]
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

                rpcEventService.TriggerParticipantJoined(viewerIP);

                return Array.Empty<byte>();
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Error in subscribeAsViewer: {ex.Message}");
                return Array.Empty<byte>();
            }
        });

        // Subscribe to UPDATE_UI to receive video/screen frames
        rpc.Subscribe(ScreenShare.Utils.UPDATE_UI, (byte[] data) => {
            try
            {
                Console.WriteLine($"[App] UPDATE_UI Received UPDATE_UI with {data.Length} bytes");
                System.Diagnostics.Debug.WriteLine("UPDATE UI : Triggering FrameReceived event");
                rpcEventService.TriggerFrameReceived(data);
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Error in UPDATE_UI: {ex.Message}");
            }
            return Array.Empty<byte>();
        });

        // Subscribe to STOP_SHARE to clear screen frames
        rpc.Subscribe(ScreenShare.Utils.STOP_SHARE, (byte[] data) => {
            try
            {
                rpcEventService.TriggerStopShareReceived(data);
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Error in STOP_SHARE: {ex.Message}");
            }
            return Array.Empty<byte>();
        });

        // Subscribe to "core/updateParticipants" - called when participant list updates
        rpc.Subscribe("core/updateParticipants", (byte[] data) => {
            try
            {
                string json = System.Text.Encoding.UTF8.GetString(data);
                System.Diagnostics.Debug.WriteLine($"[App] Participant list updated: {json}");

                rpcEventService.TriggerParticipantsListUpdated(json);

                return Array.Empty<byte>();
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Error in core/updateParticipants: {ex.Message}");
                return Array.Empty<byte>();
            }
            catch (System.Text.Json.JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Error parsing JSON in core/updateParticipants: {ex.Message}");
                return Array.Empty<byte>();
            }
        });

        // Subscribe to "core/logout"
        rpc.Subscribe("core/logout", (byte[] data) => {
            try
            {
                string message = System.Text.Encoding.UTF8.GetString(data);
                System.Diagnostics.Debug.WriteLine($"[App] Logout received: {message}");
                rpcEventService.TriggerLogout(message);
                return Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Error in core/logout: {ex.Message}");
                return Array.Empty<byte>();
            }
        });

        // Subscribe to "core/endMeeting"
        rpc.Subscribe("core/endMeeting", (byte[] data) => {
            try
            {
                string message = System.Text.Encoding.UTF8.GetString(data);
                System.Diagnostics.Debug.WriteLine($"[App] EndMeeting received: {message}");
                rpcEventService.TriggerEndMeeting(message);
                return Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Error in core/endMeeting: {ex.Message}");
                return Array.Empty<byte>();
            }
        });

        // Subscribe to "unSubscribeAsViewer" - called when a participant leaves
        rpc.Subscribe("unSubscribeAsViewer", (byte[] data) => {
            try
            {
                string viewerIP = System.Text.Encoding.UTF8.GetString(data);
                System.Diagnostics.Debug.WriteLine($"[App] Viewer left: {viewerIP}");

                rpcEventService.TriggerParticipantLeft(viewerIP);

                return Array.Empty<byte>();
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Error in unSubscribeAsViewer: {ex.Message}");
                return Array.Empty<byte>();
            }
        });

        // --- Chat Subscriptions ---

        rpc.Subscribe("chat:new-message", (byte[] data) => {
            try
            {
                rpcEventService.TriggerChatMessageReceived(data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Error in chat:new-message: {ex.Message}");
            }
            return Array.Empty<byte>();
        });

        rpc.Subscribe("chat:file-metadata-received", (byte[] data) => {
            try
            {
                rpcEventService.TriggerFileMetadataReceived(data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Error in chat:file-metadata-received: {ex.Message}");
            }
            return Array.Empty<byte>();
        });

        rpc.Subscribe("chat:file-saved-success", (byte[] data) => {
            try
            {
                rpcEventService.TriggerFileSaveSuccess(data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Error in chat:file-saved-success: {ex.Message}");
            }
            return Array.Empty<byte>();
        });

        rpc.Subscribe("chat:file-saved-error", (byte[] data) => {
            try
            {
                rpcEventService.TriggerFileSaveError(data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Error in chat:file-saved-error: {ex.Message}");
            }
            return Array.Empty<byte>();
        });

        rpc.Subscribe("chat:message-deleted", (byte[] data) => {
            try
            {
                rpcEventService.TriggerMessageDeleted(data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Error in chat:message-deleted: {ex.Message}");
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

        // Show loading screen
        var loadingViewModel = new ViewModels.Common.LoadingViewModel { Message = "Connecting to backend..." };
        var loadingView = new Views.Common.LoadingView { DataContext = loadingViewModel };
        loadingView.Show();

        // Start RPC server in background thread - matches Java: rpc.connect() returns Thread
        var rpcTask = Task.Run(() => {
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

                // Connection successful, switch to MainView on UI thread
                Application.Current?.Dispatcher.Invoke(() => {
                    // Create and show main window with DI
                    MainViewModel mainViewModel = Services.GetRequiredService<MainViewModel>();
                    var mainView = new MainView {
                        DataContext = mainViewModel
                    };
                    mainView.Show();

                    // Close loading view AFTER showing main view to prevent app shutdown
                    // (ShutdownMode defaults to OnLastWindowClose)
                    loadingView.Close();
                });

                // The rpcThread is now running listenLoop in background
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                HandleRpcConnectionError(ex, loadingViewModel, loadingView);
            }
            catch (System.IO.IOException ex)
            {
                HandleRpcConnectionError(ex, loadingViewModel, loadingView);
            }
        });
    }

    private static void HandleRpcConnectionError(Exception ex, ViewModels.Common.LoadingViewModel loadingViewModel, Views.Common.LoadingView loadingView)
    {
        System.Diagnostics.Debug.WriteLine($"[App] RPC thread error: {ex.Message}");
        Console.Error.WriteLine($"[App] RPC connection failed: {ex}");

        // Show error on UI thread
        Application.Current?.Dispatcher.Invoke(() => {
            loadingViewModel.Message = $"Connection Failed: {ex.Message}";
            loadingViewModel.IsBusy = false;

            MessageBox.Show(
                $"Failed to establish RPC connection: {ex.Message}\n\nSome features may not work.",
                "Connection Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            // Allow proceeding even if connection failed (for testing/offline)
            loadingView.Close();
            MainViewModel mainViewModel = Services.GetRequiredService<MainViewModel>();
            var mainView = new MainView {
                DataContext = mainViewModel
            };
            mainView.Show();
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

        // Register Communicator.App services
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

        // Register LoadingViewModel as Singleton
        services.AddSingleton<ViewModels.Common.LoadingViewModel>();

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
            (user, session) => ActivatorUtilities.CreateInstance<ViewModels.Meeting.MeetingSessionViewModel>(sp, user, session!));
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



