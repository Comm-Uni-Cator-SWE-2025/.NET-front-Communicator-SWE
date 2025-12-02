using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Communicator.App.Services;
using Communicator.ScreenShare;
using Communicator.App.ViewModels.Meeting;
using Communicator.Controller.Meeting;
using Communicator.Controller.RPC;
using Communicator.UX.Core.Services;
using Communicator.Networking;
using Moq;
using Xunit;

namespace Communicator.App.Tests.Unit;

public class MeetingSessionViewModelTests
{
    private readonly Mock<IToastService> _mockToastService;
    private readonly Mock<ICloudMessageService> _mockCloudMessageService;
    private readonly Mock<ICloudConfigService> _mockCloudConfig;
    private readonly Mock<INavigationService> _mockNavService;
    private readonly Mock<IThemeService> _mockThemeService;
    private readonly Mock<IRPC> _mockRpc;
    private readonly Mock<IRpcEventService> _mockRpcEventService;
    private readonly UserProfile _currentUser;

    public MeetingSessionViewModelTests()
    {
        _mockToastService = new Mock<IToastService>();
        _mockCloudMessageService = new Mock<ICloudMessageService>();
        _mockCloudConfig = new Mock<ICloudConfigService>();
        _mockNavService = new Mock<INavigationService>();
        _mockThemeService = new Mock<IThemeService>();
        _mockRpc = new Mock<IRPC>();
        _mockRpcEventService = new Mock<IRpcEventService>();
        _currentUser = new UserProfile { DisplayName = "Test User", Email = "test@example.com" };
    }

    [Fact]
    public void Constructor_Initializes_Participants()
    {
        MeetingSessionViewModel vm = CreateViewModel();
        Assert.Contains(vm.Participants, p => p.User.Email == _currentUser.Email);
    }

    [Fact]
    public void ToggleMuteCommand_TogglesIsMuted()
    {
        MeetingSessionViewModel vm = CreateViewModel();

        // Initial state: muted by default
        Assert.True(vm.IsMuted);

        // Execute toggle
        vm.ToggleMuteCommand.Execute(null);

        // Assert: should now be unmuted
        Assert.False(vm.IsMuted);
    }

    [Fact]
    public void ToggleCameraCommand_TogglesIsCameraOn()
    {
        var vm = CreateViewModel();
        
        // Initial state: camera off by default
        Assert.False(vm.IsCameraOn);

        // Execute toggle
        vm.ToggleCameraCommand.Execute(null);

        // Assert: should now be on
        Assert.True(vm.IsCameraOn);
    }

    private MeetingSessionViewModel CreateViewModel()
    {
        return new MeetingSessionViewModel(
            _currentUser,
            null,
            _mockToastService.Object,
            _mockCloudMessageService.Object,
            _mockCloudConfig.Object,
            _mockNavService.Object,
            _mockThemeService.Object,
            _mockRpc.Object,
            _mockRpcEventService.Object
        );
    }
}
