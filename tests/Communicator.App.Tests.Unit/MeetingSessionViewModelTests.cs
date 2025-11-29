using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Communicator.App.Services;
using Communicator.ScreenShare;
using Communicator.App.ViewModels.Meeting;
using Communicator.Controller.Meeting;
using Communicator.Core.RPC;
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
        var vm = CreateViewModel();
        Assert.Contains(vm.Participants, p => p.User.Email == _currentUser.Email);
    }

    [Fact]
    public void ToggleMuteCommand_TogglesIsMuted_AndCallsRpc()
    {
        var vm = CreateViewModel();
        
        // Initial state
        Assert.False(vm.IsMuted);

        // Execute
        vm.ToggleMuteCommand.Execute(null);

        // Assert
        Assert.True(vm.IsMuted);
        _mockRpc.Verify(r => r.Call(Utils.STOP_AUDIO_CAPTURE, It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public void ToggleCameraCommand_TogglesIsCameraOn_AndCallsRpc()
    {
        var vm = CreateViewModel();
        
        // Initial state (Camera is on by default)
        Assert.True(vm.IsCameraOn);

        // Execute
        vm.ToggleCameraCommand.Execute(null);

        // Assert
        Assert.False(vm.IsCameraOn);
        _mockRpc.Verify(r => r.Call(Utils.STOP_VIDEO_CAPTURE, It.IsAny<byte[]>()), Times.Once);
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
