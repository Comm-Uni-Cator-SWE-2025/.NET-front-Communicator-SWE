using System;
using System.Threading;
using Communicator.App.Services;
using Communicator.App.ViewModels;
using Communicator.App.ViewModels.Auth;
using Communicator.Core.UX.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Communicator.App.Tests.Integration
{
    public class MainViewModelIntegrationTests
    {
        [Fact]
        public void Navigation_Updates_MainViewModel_CurrentView()
        {
            // Arrange
            var services = new ServiceCollection();
            MainApp.ConfigureServices(services);
            var provider = services.BuildServiceProvider();

            var mainViewModel = provider.GetRequiredService<MainViewModel>();
            var navigationService = provider.GetRequiredService<INavigationService>();

            // Act - Initial State
            // MainViewModel constructor sets CurrentView to AuthViewModel
            Assert.IsType<AuthViewModel>(mainViewModel.CurrentView);
            Assert.False(mainViewModel.CanGoBack);

            // Act - Navigate
            var dummyView = new object();
            navigationService.NavigateTo(dummyView);

            // Assert - Navigation Reflected
            Assert.Same(dummyView, mainViewModel.CurrentView);
            Assert.True(mainViewModel.CanGoBack);

            // Act - Go Back via Command
            if (mainViewModel.GoBackCommand.CanExecute(null))
            {
                mainViewModel.GoBackCommand.Execute(null);
            }

            // Assert - Returned to Auth
            Assert.IsType<AuthViewModel>(mainViewModel.CurrentView);
            Assert.False(mainViewModel.CanGoBack);
        }
    }
}
