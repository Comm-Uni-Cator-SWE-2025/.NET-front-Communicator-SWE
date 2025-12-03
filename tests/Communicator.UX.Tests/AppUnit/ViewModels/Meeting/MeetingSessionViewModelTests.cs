// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Moq;
using Xunit;
using Communicator.App.ViewModels.Meeting;
using Communicator.App.Services;
using Communicator.Controller.Meeting;
using Communicator.Controller.RPC;
using Communicator.UX.Core.Services;

namespace Communicator.UX.Tests.AppUnit.ViewModels.Meeting;

public sealed class MeetingSessionViewModelTests : IDisposable
{
    private readonly Mock<IToastService> _mockToastService;
    private readonly Mock<ICloudMessageService> _mockCloudMessageService;
    private readonly Mock<ICloudConfigService> _mockCloudConfigService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly Mock<IThemeService> _mockThemeService;
    private readonly Mock<IRPC> _mockRpc;
    private readonly Mock<IRpcEventService> _mockRpcEventService;
    private readonly UserProfile _testUser;
    private readonly MeetingSession _testMeetingSession;
    private readonly List<MeetingSessionViewModel> _disposables = new();
    private bool _disposed;

    public MeetingSessionViewModelTests()
    {
        _mockToastService = new Mock<IToastService>();
        _mockCloudMessageService = new Mock<ICloudMessageService>();
        _mockCloudConfigService = new Mock<ICloudConfigService>();
        _mockNavigationService = new Mock<INavigationService>();
        _mockThemeService = new Mock<IThemeService>();
        _mockRpc = new Mock<IRPC>();
        _mockRpcEventService = new Mock<IRpcEventService>();

        _testUser = new UserProfile
        {
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        _testMeetingSession = new MeetingSession("test@example.com", SessionMode.CLASS);
    }

    void IDisposable.Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        foreach (var vm in _disposables)
        {
            vm.Dispose();
        }
        _disposables.Clear();
    }

    private MeetingSessionViewModel CreateViewModel(
        UserProfile? user = null,
        MeetingSession? meeting = null)
    {
        var vm = new MeetingSessionViewModel(
            user ?? _testUser,
            meeting ?? _testMeetingSession,
            _mockToastService.Object,
            _mockCloudMessageService.Object,
            _mockCloudConfigService.Object,
            _mockNavigationService.Object,
            _mockThemeService.Object,
            _mockRpc.Object,
            _mockRpcEventService.Object);
        _disposables.Add(vm);
        return vm;
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullUser_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new MeetingSessionViewModel(
                null!,
                _testMeetingSession,
                _mockToastService.Object,
                _mockCloudMessageService.Object,
                _mockCloudConfigService.Object,
                _mockNavigationService.Object,
                _mockThemeService.Object));
    }

    [Fact]
    public void Constructor_WithNullToastService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new MeetingSessionViewModel(
                _testUser,
                _testMeetingSession,
                null!,
                _mockCloudMessageService.Object,
                _mockCloudConfigService.Object,
                _mockNavigationService.Object,
                _mockThemeService.Object));
    }

    [Fact]
    public void Constructor_WithNullCloudMessageService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new MeetingSessionViewModel(
                _testUser,
                _testMeetingSession,
                _mockToastService.Object,
                null!,
                _mockCloudConfigService.Object,
                _mockNavigationService.Object,
                _mockThemeService.Object));
    }

    [Fact]
    public void Constructor_WithNullCloudConfigService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new MeetingSessionViewModel(
                _testUser,
                _testMeetingSession,
                _mockToastService.Object,
                _mockCloudMessageService.Object,
                null!,
                _mockNavigationService.Object,
                _mockThemeService.Object));
    }

    [Fact]
    public void Constructor_WithNullNavigationService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new MeetingSessionViewModel(
                _testUser,
                _testMeetingSession,
                _mockToastService.Object,
                _mockCloudMessageService.Object,
                _mockCloudConfigService.Object,
                null!,
                _mockThemeService.Object));
    }

    [Fact]
    public void Constructor_WithNullThemeService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new MeetingSessionViewModel(
                _testUser,
                _testMeetingSession,
                _mockToastService.Object,
                _mockCloudMessageService.Object,
                _mockCloudConfigService.Object,
                _mockNavigationService.Object,
                null!));
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesProperties()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.NotNull(viewModel.Toolbar);
        Assert.NotNull(viewModel.Participants);
        Assert.NotNull(viewModel.VideoSession);
        Assert.NotNull(viewModel.Chat);
        Assert.NotNull(viewModel.Whiteboard);
        Assert.NotNull(viewModel.AIInsights);
        Assert.NotNull(viewModel.ActiveQuickDoubts);
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesCommands()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.NotNull(viewModel.ToggleMuteCommand);
        Assert.NotNull(viewModel.ToggleCameraCommand);
        Assert.NotNull(viewModel.ToggleHandCommand);
        Assert.NotNull(viewModel.ToggleScreenShareCommand);
        Assert.NotNull(viewModel.LeaveMeetingCommand);
        Assert.NotNull(viewModel.ToggleChatPanelCommand);
        Assert.NotNull(viewModel.ToggleParticipantsPanelCommand);
        Assert.NotNull(viewModel.CloseSidePanelCommand);
        Assert.NotNull(viewModel.SendQuickDoubtCommand);
        Assert.NotNull(viewModel.DismissQuickDoubtCommand);
        Assert.NotNull(viewModel.CopyMeetingLinkCommand);
    }

    [Fact]
    public void Constructor_WithNullMeetingSession_InitializesSuccessfully()
    {
        // Arrange & Act (null meeting session should be handled)
        var viewModel = new MeetingSessionViewModel(
            _testUser,
            null, // null meeting session
            _mockToastService.Object,
            _mockCloudMessageService.Object,
            _mockCloudConfigService.Object,
            _mockNavigationService.Object,
            _mockThemeService.Object,
            _mockRpc.Object,
            _mockRpcEventService.Object);
        _disposables.Add(viewModel);

        // Assert
        Assert.NotNull(viewModel.Participants);
    }

    [Fact]
    public void Constructor_WithNullRpc_InitializesSuccessfully()
    {
        // Arrange & Act
        var viewModel = new MeetingSessionViewModel(
            _testUser,
            _testMeetingSession,
            _mockToastService.Object,
            _mockCloudMessageService.Object,
            _mockCloudConfigService.Object,
            _mockNavigationService.Object,
            _mockThemeService.Object,
            null, // null RPC
            null); // null RpcEventService
        _disposables.Add(viewModel);

        // Assert - should not throw
        Assert.NotNull(viewModel);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void IsMuted_DefaultValue_IsTrue()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.True(viewModel.IsMuted);
    }

    [Fact]
    public void IsCameraOn_DefaultValue_IsFalse()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.False(viewModel.IsCameraOn);
    }

    [Fact]
    public void IsHandRaised_DefaultValue_IsFalse()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.False(viewModel.IsHandRaised);
    }

    [Fact]
    public void IsScreenSharing_DefaultValue_IsFalse()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.False(viewModel.IsScreenSharing);
    }

    [Fact]
    public void IsSidePanelOpen_DefaultValue_IsFalse()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.False(viewModel.IsSidePanelOpen);
    }

    [Fact]
    public void SidePanelTitle_DefaultValue_IsEmpty()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.Equal(string.Empty, viewModel.SidePanelTitle);
    }

    [Fact]
    public void QuickDoubtMessage_DefaultValue_IsEmpty()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.Equal(string.Empty, viewModel.QuickDoubtMessage);
    }

    [Fact]
    public void QuickDoubtMessage_SetValue_UpdatesProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.QuickDoubtMessage = "Test message";

        // Assert
        Assert.Equal("Test message", viewModel.QuickDoubtMessage);
    }

    [Fact]
    public void QuickDoubtMessage_SetValue_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = CreateViewModel();
        bool propertyChanged = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.QuickDoubtMessage))
                propertyChanged = true;
        };

        // Act
        viewModel.QuickDoubtMessage = "New message";

        // Assert
        Assert.True(propertyChanged);
    }

    [Fact]
    public void IsQuickDoubtBubbleOpen_DefaultValue_IsFalse()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.False(viewModel.IsQuickDoubtBubbleOpen);
    }

    [Fact]
    public void MeetingLink_DefaultValue_IsEmpty()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.Equal(string.Empty, viewModel.MeetingLink);
    }

    [Fact]
    public void MeetingLink_SetValue_UpdatesProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.MeetingLink = "https://meeting.example.com/123";

        // Assert
        Assert.Equal("https://meeting.example.com/123", viewModel.MeetingLink);
    }

    [Fact]
    public void CurrentPage_DefaultValue_IsSet()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert - CurrentPage should be initialized from the selected tab
        Assert.NotNull(viewModel.CurrentPage);
    }

    [Fact]
    public void IpToMailMap_IsInitialized()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.NotNull(viewModel.IpToMailMap);
    }

    [Fact]
    public void IsMeetingActive_DefaultValue_IsTrue()
    {
        // Arrange & Act (StartMeeting is called in constructor)
        var viewModel = CreateViewModel();

        // Assert
        Assert.True(viewModel.IsMeetingActive);
    }

    #endregion

    #region ParseIpFromIpPort Tests

    [Theory]
    [InlineData("192.168.1.1:8080", "192.168.1.1")]
    [InlineData("10.0.0.1:443", "10.0.0.1")]
    [InlineData("127.0.0.1:5000", "127.0.0.1")]
    [InlineData("192.168.1.1", "192.168.1.1")]
    [InlineData("localhost:3000", "localhost")]
    [InlineData("host", "host")]
    public void ParseIpFromIpPort_ReturnsCorrectIp(string ipPort, string expectedIp)
    {
        // Act
        string result = MeetingSessionViewModel.ParseIpFromIpPort(ipPort);

        // Assert
        Assert.Equal(expectedIp, result);
    }

    [Fact]
    public void ParseIpFromIpPort_WithEmptyString_ReturnsEmptyString()
    {
        // Act
        string result = MeetingSessionViewModel.ParseIpFromIpPort(string.Empty);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ParseIpFromIpPort_WithNoPort_ReturnsFullString()
    {
        // Arrange
        string ipWithoutPort = "192.168.1.100";

        // Act
        string result = MeetingSessionViewModel.ParseIpFromIpPort(ipWithoutPort);

        // Assert
        Assert.Equal("192.168.1.100", result);
    }

    #endregion

    #region Command CanExecute Tests

    [Fact]
    public void SendQuickDoubtCommand_CanExecute_ReturnsFalseWhenMessageEmpty()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.QuickDoubtMessage = string.Empty;

        // Act
        bool canExecute = viewModel.SendQuickDoubtCommand.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void SendQuickDoubtCommand_CanExecute_ReturnsFalseWhenMessageIsWhitespace()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.QuickDoubtMessage = "   ";

        // Act
        bool canExecute = viewModel.SendQuickDoubtCommand.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void SendQuickDoubtCommand_CanExecute_ReturnsTrueWhenMessageHasContent()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.QuickDoubtMessage = "What is the topic?";

        // Act
        bool canExecute = viewModel.SendQuickDoubtCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void ToggleMuteCommand_CanExecute_ReturnsTrue()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        bool canExecute = viewModel.ToggleMuteCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void ToggleCameraCommand_CanExecute_ReturnsTrue()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        bool canExecute = viewModel.ToggleCameraCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void ToggleHandCommand_CanExecute_ReturnsTrue()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        bool canExecute = viewModel.ToggleHandCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void LeaveMeetingCommand_CanExecute_ReturnsTrue()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        bool canExecute = viewModel.LeaveMeetingCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void CopyMeetingLinkCommand_CanExecute_ReturnsTrue()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        bool canExecute = viewModel.CopyMeetingLinkCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    #endregion

    #region Participants Collection Tests

    [Fact]
    public void Participants_AfterConstruction_ContainsCurrentUser()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert - Current user should be added as a participant
        Assert.Single(viewModel.Participants);
        Assert.Equal(_testUser.Email, viewModel.Participants.First().User.Email);
    }

    [Fact]
    public void Participants_IsObservableCollection()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.IsType<System.Collections.ObjectModel.ObservableCollection<ParticipantViewModel>>(viewModel.Participants);
    }

    #endregion

    #region ActiveQuickDoubts Collection Tests

    [Fact]
    public void ActiveQuickDoubts_AfterConstruction_IsEmpty()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.Empty(viewModel.ActiveQuickDoubts);
    }

    [Fact]
    public void ActiveQuickDoubts_IsObservableCollection()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.IsType<System.Collections.ObjectModel.ObservableCollection<QuickDoubtItem>>(viewModel.ActiveQuickDoubts);
    }

    #endregion

    #region Sub-ViewModel Tests

    [Fact]
    public void VideoSession_IsInitialized()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.NotNull(viewModel.VideoSession);
    }

    [Fact]
    public void Chat_IsInitialized()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.NotNull(viewModel.Chat);
    }

    [Fact]
    public void Whiteboard_IsInitialized()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.NotNull(viewModel.Whiteboard);
    }

    [Fact]
    public void AIInsights_IsInitialized()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.NotNull(viewModel.AIInsights);
    }

    [Fact]
    public void Toolbar_IsInitialized()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.NotNull(viewModel.Toolbar);
    }

    #endregion

    #region InitializeCanvasAsync Tests

    [Fact]
    public async Task InitializeCanvasAsync_CompletesSuccessfully()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act & Assert - should complete without throwing
        await viewModel.InitializeCanvasAsync();
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var viewModel = new MeetingSessionViewModel(
            _testUser,
            _testMeetingSession,
            _mockToastService.Object,
            _mockCloudMessageService.Object,
            _mockCloudConfigService.Object,
            _mockNavigationService.Object,
            _mockThemeService.Object,
            _mockRpc.Object,
            _mockRpcEventService.Object);

        // Act & Assert - should not throw
        viewModel.Dispose();
        viewModel.Dispose();
    }

    [Fact]
    public void Dispose_DisconnectsCloudMessageService()
    {
        // Arrange
        var viewModel = new MeetingSessionViewModel(
            _testUser,
            _testMeetingSession,
            _mockToastService.Object,
            _mockCloudMessageService.Object,
            _mockCloudConfigService.Object,
            _mockNavigationService.Object,
            _mockThemeService.Object,
            _mockRpc.Object,
            _mockRpcEventService.Object);

        // Act
        viewModel.Dispose();

        // Assert
        _mockCloudMessageService.Verify(x => x.DisconnectAsync(), Times.AtLeastOnce());
    }

    #endregion

    #region PropertyChanged Tests

    [Fact]
    public void MeetingLink_WhenSet_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var changedProperties = new List<string>();
        viewModel.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        // Act
        viewModel.MeetingLink = "https://new-link.com";

        // Assert
        Assert.Contains(nameof(viewModel.MeetingLink), changedProperties);
    }

    #endregion
}
