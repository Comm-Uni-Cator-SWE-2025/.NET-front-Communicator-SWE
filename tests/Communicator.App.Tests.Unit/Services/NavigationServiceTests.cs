using System;
using Communicator.App.Services;
using Xunit;

namespace Communicator.App.Tests.Unit.Services
{
    public class NavigationServiceTests
    {
        private readonly NavigationService _service;

        public NavigationServiceTests()
        {
            _service = new NavigationService();
        }

        [Fact]
        public void NavigateTo_SetsCurrentView()
        {
            var view1 = new object();
            var eventRaised = false;
            _service.NavigationChanged += (s, e) => eventRaised = true;

            _service.NavigateTo(view1);

            Assert.Same(view1, _service.CurrentView);
            Assert.True(eventRaised);
            Assert.False(_service.CanGoBack);
        }

        [Fact]
        public void NavigateTo_PushesToBackStack()
        {
            var view1 = new object();
            var view2 = new object();

            _service.NavigateTo(view1);
            _service.NavigateTo(view2);

            Assert.Same(view2, _service.CurrentView);
            Assert.True(_service.CanGoBack);
        }

        [Fact]
        public void GoBack_RestoresPreviousView()
        {
            var view1 = new object();
            var view2 = new object();

            _service.NavigateTo(view1);
            _service.NavigateTo(view2);
            
            _service.GoBack();

            Assert.Same(view1, _service.CurrentView);
            Assert.False(_service.CanGoBack);
        }

        [Fact]
        public void ClearHistory_ClearsBackStack()
        {
            var view1 = new object();
            var view2 = new object();

            _service.NavigateTo(view1);
            _service.NavigateTo(view2);
            
            _service.ClearHistory();

            Assert.Same(view2, _service.CurrentView); // Current view remains
            Assert.False(_service.CanGoBack); // History is gone
        }
    }
}
