using System;
using Communicator.App.Services;
using Communicator.App.ViewModels;
using Communicator.App.ViewModels.Auth;
using Communicator.App.ViewModels.Common;
using Communicator.App.ViewModels.Home;
using Communicator.App.ViewModels.Meeting;
using Communicator.App.ViewModels.Settings;
using Communicator.Controller.Meeting;
using Communicator.Controller.RPC;
using Communicator.UX.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Communicator.App.Tests.Integration;

public class DependencyInjectionTests
{
    [Fact]
    public void ConfigureServices_RegistersAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        // We are calling the internal static method from App.xaml.cs
        // This requires InternalsVisibleTo in App assembly
        MainApp.ConfigureServices(services);
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert - Verify Core Services
        // Note: IConfiguration is no longer registered - we use environment variables instead
        Assert.NotNull(provider.GetService<IToastService>());
        Assert.NotNull(provider.GetService<IThemeService>());

        // Assert - Verify App Services
        Assert.NotNull(provider.GetService<INavigationService>());
        Assert.NotNull(provider.GetService<IAuthenticationService>());
        Assert.NotNull(provider.GetService<ICloudConfigService>());
        Assert.NotNull(provider.GetService<ICloudMessageService>());
        Assert.NotNull(provider.GetService<IRpcEventService>());
        Assert.NotNull(provider.GetService<IRPC>());

        // Assert - Verify ViewModels
        Assert.NotNull(provider.GetService<MainViewModel>());
        Assert.NotNull(provider.GetService<AuthViewModel>());
        // HomePageViewModel, SettingsViewModel, MeetingSessionViewModel require UserProfile, 
        // so they cannot be resolved directly from DI without it being registered.
        // They are intended to be created via factories.

        Assert.NotNull(provider.GetService<ToastContainerViewModel>());
        Assert.NotNull(provider.GetService<LoadingViewModel>());

        // Assert - Verify Factories
        Func<AuthViewModel>? authFactory = provider.GetService<Func<AuthViewModel>>();
        Assert.NotNull(authFactory);
        Assert.NotNull(authFactory());

        Func<UserProfile, HomePageViewModel>? homeFactory = provider.GetService<Func<UserProfile, HomePageViewModel>>();
        Assert.NotNull(homeFactory);
        var user = new UserProfile("test@example.com", "Test User", ParticipantRole.STUDENT, new Uri("http://url"));
        Assert.NotNull(homeFactory(user));

        Func<UserProfile, SettingsViewModel>? settingsFactory = provider.GetService<Func<UserProfile, SettingsViewModel>>();
        Assert.NotNull(settingsFactory);
        Assert.NotNull(settingsFactory(user));

        Func<UserProfile, MeetingSession?, MeetingSessionViewModel>? meetingFactory = provider.GetService<Func<UserProfile, MeetingSession?, MeetingSessionViewModel>>();
        Assert.NotNull(meetingFactory);
    }

    [Fact]
    public void ViewModels_CanBeResolvedDirectly_WhenRuntimeDependenciesAreRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        MainApp.ConfigureServices(services);

        // Register runtime dependencies that are usually passed via factories.
        // This allows us to test direct resolution of the ViewModels to ensure their 
        // static dependencies (Services, Config, etc.) are correctly wired up.
        UserProfile user = new UserProfile("test@example.com", "Test User", ParticipantRole.STUDENT, new Uri("http://url"));
        services.AddSingleton(user);

        MeetingSession session = new MeetingSession("test-meeting", "host", 0, SessionMode.CLASS);
        services.AddSingleton(session);

        ServiceProvider provider = services.BuildServiceProvider();

        // Act & Assert
        // Now we can resolve them directly because all dependencies (static + runtime) are in the container
        Assert.NotNull(provider.GetService<HomePageViewModel>());
        Assert.NotNull(provider.GetService<SettingsViewModel>());
        Assert.NotNull(provider.GetService<MeetingSessionViewModel>());
    }
}
