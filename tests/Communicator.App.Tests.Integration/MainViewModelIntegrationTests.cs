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

            // Act - Navigate to first view
            var view1 = new object();
            navigationService.NavigateTo(view1);
            Assert.Same(view1, mainViewModel.CurrentView);

            // Act - Navigate to second view
            var view2 = new object();
            navigationService.NavigateTo(view2);
            Assert.Same(view2, mainViewModel.CurrentView);
            
            // Assert - Can Go Back
            Assert.True(mainViewModel.CanGoBack);

            // Act - Go Back via Command
            if (mainViewModel.GoBackCommand.CanExecute(null))
            {
                mainViewModel.GoBackCommand.Execute(null);
            }

            // Assert - Returned to View 1
            Assert.Same(view1, mainViewModel.CurrentView);
        }
    }
}
