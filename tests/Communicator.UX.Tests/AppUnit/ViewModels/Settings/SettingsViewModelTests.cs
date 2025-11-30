using System;
using System.Threading.Tasks;
using Communicator.App.Services;
using Communicator.App.ViewModels.Settings;
using Communicator.Controller.Meeting;
using Communicator.UX.Core;
using Communicator.UX.Core.Services;
using Communicator.UX.Core.Models;
using Moq;
using Xunit;

namespace Communicator.App.Tests.Unit.ViewModels.Settings
{
    public class SettingsViewModelTests
    {
        private readonly Mock<IThemeService> _mockThemeService;
        private readonly Mock<IAuthenticationService> _mockAuthService;
        private readonly UserProfile _user;

        public SettingsViewModelTests()
        {
            _mockThemeService = new Mock<IThemeService>();
            _mockAuthService = new Mock<IAuthenticationService>();
            _user = new UserProfile("test@example.com", "Test User", ParticipantRole.STUDENT, new Uri("http://photo.url"));
        }

        private SettingsViewModel CreateViewModel()
        {
            return new SettingsViewModel(_user, _mockThemeService.Object, _mockAuthService.Object);
        }

        [Fact]
        public void Constructor_InitializesProperties()
        {
            _mockThemeService.Setup(x => x.CurrentTheme).Returns(AppTheme.Light);
            var vm = CreateViewModel();

            Assert.Equal("Test User", vm.DisplayName);
            Assert.Equal("test@example.com", vm.Email);
            Assert.False(vm.IsDarkMode);
            Assert.Equal("Light", vm.CurrentThemeText);
        }

        [Fact]
        public void Constructor_InitializesDarkMode_WhenThemeIsDark()
        {
            _mockThemeService.Setup(x => x.CurrentTheme).Returns(AppTheme.Dark);
            var vm = CreateViewModel();

            Assert.True(vm.IsDarkMode);
            Assert.Equal("Dark", vm.CurrentThemeText);
        }

        [Fact]
        public void IsDarkMode_SetTrue_UpdatesThemeAndText()
        {
            _mockThemeService.Setup(x => x.CurrentTheme).Returns(AppTheme.Light);
            var vm = CreateViewModel();

            vm.IsDarkMode = true;

            Assert.True(vm.IsDarkMode);
            Assert.Equal("Dark", vm.CurrentThemeText);
            _mockThemeService.Verify(x => x.SetTheme(AppTheme.Dark), Times.Once);
        }

        [Fact]
        public void IsDarkMode_SetFalse_UpdatesThemeAndText()
        {
            _mockThemeService.Setup(x => x.CurrentTheme).Returns(AppTheme.Dark);
            var vm = CreateViewModel();

            vm.IsDarkMode = false;

            Assert.False(vm.IsDarkMode);
            Assert.Equal("Light", vm.CurrentThemeText);
            _mockThemeService.Verify(x => x.SetTheme(AppTheme.Light), Times.Once);
        }

        [Fact]
        public async Task LogoutCommand_CallsAuthService()
        {
            var vm = CreateViewModel();
            
            if (vm.LogoutCommand is Communicator.UX.Core.RelayCommand command)
            {
                command.Execute(null);
                _mockAuthService.Verify(x => x.LogoutAsync(), Times.Once);
            }
        }
    }
}
