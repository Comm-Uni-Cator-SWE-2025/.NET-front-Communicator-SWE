using System;
using System.Threading.Tasks;
using Communicator.App.Services;
using Communicator.Controller.Meeting;
using Communicator.Controller.RPC;
using Moq;
using Xunit;

namespace Communicator.App.Tests.Unit.Services
{
    public class AuthenticationServiceTests
    {
        private readonly Mock<IRPC> _mockRpc;
        private readonly AuthenticationService _service;

        public AuthenticationServiceTests()
        {
            _mockRpc = new Mock<IRPC>();
            _service = new AuthenticationService(_mockRpc.Object);
        }

        [Fact]
        public void CompleteLogin_SetsCurrentUser_AndRaisesEvent()
        {
            var user = new UserProfile("email", "User", ParticipantRole.STUDENT, new Uri("http://photo"));
            var eventRaised = false;
            _service.UserLoggedIn += (s, e) => 
            {
                Assert.Equal(user, e.User);
                eventRaised = true;
            };

            _service.CompleteLogin(user);

            Assert.Equal(user, _service.CurrentUser);
            Assert.True(_service.IsAuthenticated);
            Assert.True(eventRaised);
        }

        [Fact]
        public async Task LogoutAsync_CallsRpc_AndClearsUser()
        {
            var user = new UserProfile("email", "User", ParticipantRole.STUDENT, new Uri("http://photo"));
            _service.CompleteLogin(user);
            
            var eventRaised = false;
            _service.UserLoggedOut += (s, e) => eventRaised = true;

            await _service.LogoutAsync();

            _mockRpc.Verify(x => x.Call("core/logout", It.IsAny<byte[]>()), Times.Once);
            Assert.Null(_service.CurrentUser);
            Assert.False(_service.IsAuthenticated);
            Assert.True(eventRaised);
        }
    }
}
