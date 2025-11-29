// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*
 * -----------------------------------------------------------------------------
 *  File: App.xaml.cs
 *  Owner: Geetheswar V
 *  Roll Number : 142201025
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System.Windows;
using System.Windows.Threading;
using System.Linq;
using Communicator.App.Services;
using Communicator.App.ViewModels;
using Communicator.App.Views;
using Communicator.Controller;
using Communicator.Controller.Meeting;
using Communicator.Core;
using Communicator.Core.Logging;
using Communicator.Core.RPC;
using Communicator.UX.Core;
using Communicator.UX.Core.Services;
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

        // Check for test mode
        bool isTestMode = e.Args.Contains("--test-mode");

        // Configure Dependency Injection
        var services = new ServiceCollection();
        ConfigureServices(services, isTestMode);
        Services = services.BuildServiceProvider();

        // RPC Service
        IRPC rpc = Services.GetRequiredService<IRPC>();

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

        // Get Logger
        ILoggerFactory loggerFactory = Services.GetRequiredService<ILoggerFactory>();
        ILogger logger = loggerFactory.GetLogger("UX");

        // Subscribe to RPC methods BEFORE starting connection (like Java)
        IRpcEventService rpcEventService = Services.GetRequiredService<IRpcEventService>();
        SubscribeRpcMethods(rpc, rpcEventService, logger);

        // Start RPC connection in background thread (like Java does)
        // This allows UI to appear while waiting for backend to connect

        // Debug args
        string argsStr = string.Join(", ", e.Args);

        if (e.Args.Contains("--test-mode"))
        {
            try
            {
                logger.LogInfo("[App] Test mode detected. Skipping RPC connection.");

                // Resolve services
                MainViewModel mainViewModel = Services.GetRequiredService<MainViewModel>();
                IAuthenticationService authenticationService = Services.GetRequiredService<IAuthenticationService>();
                INavigationService navService = Services.GetRequiredService<INavigationService>();
                Func<UserProfile, ViewModels.Home.HomePageViewModel> homeFactory = Services.GetRequiredService<Func<UserProfile, ViewModels.Home.HomePageViewModel>>();
                ViewModels.Common.LoadingViewModel loadingViewModel = Services.GetRequiredService<ViewModels.Common.LoadingViewModel>();

                // Ensure loading is off
                loadingViewModel.IsBusy = false;

                // Create dummy user
                UserProfile dummyUser = new UserProfile(
                    "test@example.com",
                    "Test User",
                    ParticipantRole.STUDENT,
                    new Uri("https://via.placeholder.com/150"));

                // Auto-login
                authenticationService.CompleteLogin(dummyUser);

                // Navigate to Home
                navService.NavigateTo(homeFactory(dummyUser));

                // Show Main Window
                MainView mainView = new MainView {
                    DataContext = mainViewModel
                };
                mainView.Show();
            }
            catch (Exception ex)
            {
                logger.LogError($"Test Mode Error: {ex.Message}", ex);
                MessageBox.Show($"Test Mode Error: {ex.Message}\n{ex.StackTrace}", "Test Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }
        else
        {
            StartRpcConnectionInBackground(rpc, e.Args, logger);
        }
    }

    /// <summary>
    /// Subscribes to RPC methods that the backend may call.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "RPC callbacks must not crash the app")]
    private static void SubscribeRpcMethods(IRPC rpc, IRpcEventService rpcEventService, ILogger logger)
    {

        // Subscribe to UPDATE_UI to receive video/screen frames
        rpc.Subscribe(ScreenShare.Utils.UPDATE_UI, (byte[] data) => {
            try
            {
                rpcEventService.TriggerFrameReceived(data);
            }
            catch (ArgumentException ex)
            {
                logger.LogError($"[App] Error in UPDATE_UI: {ex.Message}", ex);
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
                logger.LogError($"[App] Error in STOP_SHARE: {ex.Message}", ex);
            }
            return Array.Empty<byte>();
        });

        // Subscribe to "core/updateParticipants" - called when participant list updates
        rpc.Subscribe("core/updateParticipants", (byte[] data) => {
            try
            {
                string json = System.Text.Encoding.UTF8.GetString(data);
                rpcEventService.TriggerParticipantsListUpdated(json);

                return Array.Empty<byte>();
            }
            catch (ArgumentException ex)
            {
                logger.LogError($"[App] Error in core/updateParticipants: {ex.Message}", ex);
                return Array.Empty<byte>();
            }
            catch (System.Text.Json.JsonException ex)
            {
                logger.LogError($"[App] Error parsing JSON in core/updateParticipants: {ex.Message}", ex);
                return Array.Empty<byte>();
            }
        });

        // Subscribe to "core/logout"
        rpc.Subscribe("core/logout", (byte[] data) => {
            try
            {
                string message = System.Text.Encoding.UTF8.GetString(data);
                rpcEventService.TriggerLogout(message);
                return Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                logger.LogError($"[App] Error in core/logout: {ex.Message}", ex);
                return Array.Empty<byte>();
            }
        });

        // Subscribe to "core/endMeeting"
        rpc.Subscribe("core/endMeeting", (byte[] data) => {
            try
            {
                string message = System.Text.Encoding.UTF8.GetString(data);
                rpcEventService.TriggerEndMeeting(message);
                return Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                logger.LogError($"[App] Error in core/endMeeting: {ex.Message}", ex);
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
                logger.LogError($"[App] Error in chat:new-message: {ex.Message}", ex);
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
                logger.LogError($"[App] Error in chat:file-metadata-received: {ex.Message}", ex);
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
                logger.LogError($"[App] Error in chat:file-saved-success: {ex.Message}", ex);
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
                logger.LogError($"[App] Error in chat:file-saved-error: {ex.Message}", ex);
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
                logger.LogError($"[App] Error in chat:message-deleted: {ex.Message}", ex);
            }
            return Array.Empty<byte>();
        });

        // --- Canvas Subscriptions ---

        rpc.Subscribe("canvas:update", (byte[] data) => {
            try
            {
                rpcEventService.TriggerCanvasUpdateReceived(data);
            }
            catch (Exception ex)
            {
                logger.LogError($"[App] Error in canvas:update: {ex.Message}", ex);
            }
            return Array.Empty<byte>();
        });
    }

    /// <summary>
    /// Starts RPC connection in a background thread.
    /// This matches Java frontend pattern where connect() is called and returns a Thread.
    /// The UI can appear while we wait for backend to connect.
    /// </summary>
    private static void StartRpcConnectionInBackground(IRPC rpc, string[] args, ILogger logger)
    {
        logger.LogInfo("[App] Starting RPC connection in background thread...");

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

                logger.LogInfo($"[App] RPC thread: Connecting to port {portNumber}...");

                // This will BLOCK until backend connects and completes handshake
                // Just like Java: new SocketryServer(portNumber, methods)
                Thread rpcThread = rpc.Connect(portNumber);

                logger.LogInfo("[App] RPC thread: Backend connected, server running");

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
                HandleRpcConnectionError(ex, loadingViewModel, loadingView, logger);
            }
            catch (System.IO.IOException ex)
            {
                HandleRpcConnectionError(ex, loadingViewModel, loadingView, logger);
            }
        });
    }

    private static void HandleRpcConnectionError(Exception ex, ViewModels.Common.LoadingViewModel loadingViewModel, Views.Common.LoadingView loadingView, ILogger logger)
    {
        logger.LogError($"[App] RPC thread error: {ex.Message}", ex);
        logger.LogError($"[App] RPC connection failed: {ex}");

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
    internal static void ConfigureServices(IServiceCollection services, bool isTestMode = false)
    {
        // Register Configuration (loads appsettings.json)
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Register Communicator.Core services (Logger)
        services.AddCoreServices();

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
        if (isTestMode)
        {
            services.AddSingleton<IRPC, Services.MockRPCService>();
        }
        else
        {
            services.AddSingleton<IRPC, RPCService>();
        }

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
        try
        {
            ILoggerFactory? factory = Services?.GetService<ILoggerFactory>();
            ILogger? logger = factory?.GetLogger("App");
            logger?.LogError("Unhandled exception", exception);
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            // Ignore logging errors during crash reporting
        }

        Console.Error.WriteLine(exception);
        MessageBox.Show(exception.ToString(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}



