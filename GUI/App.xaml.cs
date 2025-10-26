using System;
using System.Windows;
using System.Windows.Threading;
using Controller;
using Microsoft.Extensions.DependencyInjection;
using UX.Core;
using UX.Core.Services;
using GUI.ViewModels;
using GUI.Views;
using GUI.Services;

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
        services.AddSingleton<IAuthenticationService, Services.AuthenticationService>();

        // Register Controller services
        services.AddSingleton<IController, MockController>();

        // Register ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<GUI.ViewModels.Auth.AuthViewModel>();
        services.AddTransient<GUI.ViewModels.Home.HomePageViewModel>();
        services.AddTransient<GUI.ViewModels.Settings.SettingsViewModel>();
        services.AddTransient<GUI.ViewModels.Meeting.MeetingShellViewModel>();
        
        // Register ToastContainerViewModel as Singleton (single toast container for app)
        services.AddSingleton<GUI.ViewModels.Common.ToastContainerViewModel>();

        // Register ViewModel Factories
        // Factory for creating AuthViewModel instances (used in MainViewModel)
        services.AddTransient<Func<GUI.ViewModels.Auth.AuthViewModel>>(sp => 
            () => sp.GetRequiredService<GUI.ViewModels.Auth.AuthViewModel>());

        // Factory for creating ViewModels with UserProfile parameter
        // Using ActivatorUtilities.CreateInstance for automatic dependency resolution
        services.AddTransient<Func<UserProfile, GUI.ViewModels.Home.HomePageViewModel>>(sp =>
            user => ActivatorUtilities.CreateInstance<GUI.ViewModels.Home.HomePageViewModel>(sp, user));

        services.AddTransient<Func<UserProfile, GUI.ViewModels.Settings.SettingsViewModel>>(sp =>
            user => ActivatorUtilities.CreateInstance<GUI.ViewModels.Settings.SettingsViewModel>(sp, user));

        services.AddTransient<Func<UserProfile, GUI.ViewModels.Meeting.MeetingShellViewModel>>(sp =>
            user => ActivatorUtilities.CreateInstance<GUI.ViewModels.Meeting.MeetingShellViewModel>(sp, user));
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

