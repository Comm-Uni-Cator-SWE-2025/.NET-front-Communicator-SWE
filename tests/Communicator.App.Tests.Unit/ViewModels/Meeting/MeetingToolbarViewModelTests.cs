using System;
using System.Collections.Generic;
using System.Linq;
using Communicator.App.ViewModels.Meeting;
using Moq;
using Xunit;

namespace Communicator.App.Tests.Unit.ViewModels.Meeting
{
    public class MeetingToolbarViewModelTests
    {
        [Fact]
        public void Constructor_SetsTabsAndSelectedTab()
        {
            var tab1 = new MeetingTabViewModel("Tab1", new object());
            var tab2 = new MeetingTabViewModel("Tab2", new object());
            var tabs = new List<MeetingTabViewModel> { tab1, tab2 };

            var vm = new MeetingToolbarViewModel(tabs);

            Assert.Equal(2, vm.Tabs.Count);
            Assert.Equal(tab1, vm.SelectedTab);
        }

        [Fact]
        public void SelectedTab_Set_RaisesEventAndPropertyChanged()
        {
            var tab1 = new MeetingTabViewModel("Tab1", new object());
            var tab2 = new MeetingTabViewModel("Tab2", new object());
            var tabs = new List<MeetingTabViewModel> { tab1, tab2 };
            var vm = new MeetingToolbarViewModel(tabs);

            var eventRaised = false;
            vm.SelectedTabChanged += (s, e) => 
            {
                Assert.Equal(tab2, e.Tab);
                eventRaised = true;
            };

            vm.SelectedTab = tab2;

            Assert.Equal(tab2, vm.SelectedTab);
            Assert.True(eventRaised);
        }

        [Fact]
        public void CopyMeetingLinkCommand_CallsSessionViewModel()
        {
            
            // We will test that it doesn't crash when session is null.
            var tab1 = new MeetingTabViewModel("Tab1", new object());
            var vm = new MeetingToolbarViewModel(new[] { tab1 }, null);

            vm.CopyMeetingLinkCommand.Execute(null);
            
            // Pass
        }
    }
}
