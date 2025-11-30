using Communicator.App.ViewModels.Meeting;
using Xunit;

namespace Communicator.App.Tests.Unit.ViewModels.Meeting
{
    public class MeetingTabViewModelTests
    {
        [Fact]
        public void Constructor_SetsProperties()
        {
            var content = new object();
            var vm = new MeetingTabViewModel("Header", content);

            Assert.Equal("Header", vm.Header);
            Assert.Same(content, vm.ContentViewModel);
        }

        [Fact]
        public void Header_Set_RaisesPropertyChanged()
        {
            var vm = new MeetingTabViewModel("Header", new object());
            var propertyChanged = false;
            vm.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(MeetingTabViewModel.Header)) propertyChanged = true; };

            vm.Header = "New Header";

            Assert.Equal("New Header", vm.Header);
            Assert.True(propertyChanged);
        }
    }
}
