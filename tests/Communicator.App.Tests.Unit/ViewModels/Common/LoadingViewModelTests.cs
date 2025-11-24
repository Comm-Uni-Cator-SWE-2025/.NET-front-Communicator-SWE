using Communicator.App.ViewModels.Common;
using Xunit;

namespace Communicator.App.Tests.Unit.ViewModels.Common
{
    public class LoadingViewModelTests
    {
        [Fact]
        public void Properties_SetAndGet_RaisePropertyChanged()
        {
            var vm = new LoadingViewModel();

            Assert.Equal("Loading...", vm.Message);
            Assert.False(vm.IsBusy);

            var msgChanged = false;
            var busyChanged = false;
            vm.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == nameof(LoadingViewModel.Message)) msgChanged = true;
                if (e.PropertyName == nameof(LoadingViewModel.IsBusy)) busyChanged = true;
            };

            vm.Message = "Wait";
            vm.IsBusy = true;

            Assert.Equal("Wait", vm.Message);
            Assert.True(vm.IsBusy);
            Assert.True(msgChanged);
            Assert.True(busyChanged);
        }
    }
}
