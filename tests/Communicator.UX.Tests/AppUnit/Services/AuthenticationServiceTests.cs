using System;
using System.Threading.Tasks;
using Communicator.App.Services;
using Communicator.Controller.Meeting;
using Communicator.Controller.RPC;
using Moq;
using Xunit;

namespace Communicator.App.Tests.Unit.Services;

public sealed class AuthenticationServiceTests
{
    private readonly Mock<IRPC> _mockRpc;
    private readonly AuthenticationService _service;

    public AuthenticationServiceTests()
    {
        _mockRpc = new Mock<IRPC>();
        _service = new AuthenticationService(_mockRpc.Object);
    }

    [Fact]
    public void ConstructorWithNullRpcThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AuthenticationService(null!));
    }

    [Fact]
    public void InitialStateIsNotAuthenticated()
    {
        Assert.Null(_service.CurrentUser);
        Assert.False(_service.IsAuthenticated);
    }

    [Fact]
    public void CompleteLoginSetsCurrentUserAndRaisesEvent()
    {
        UserProfile user = new UserProfile("email@test.com", "User", ParticipantRole.STUDENT, new Uri("http://photo.com"));
        bool eventRaised = false;
        UserProfileEventArgs? eventArgs = null;

        _service.UserLoggedIn += (s, e) => {
            Assert.Equal(user, e.User);
            eventRaised = true;
            eventArgs = e;
        };

        _service.CompleteLogin(user);

        Assert.Equal(user, _service.CurrentUser);
        Assert.True(_service.IsAuthenticated);
        Assert.True(eventRaised);
        Assert.NotNull(eventArgs);
    }

    [Fact]
    public void CompleteLoginWithNullUserThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _service.CompleteLogin(null!));
    }

    [Fact]
    public async Task LogoutAsyncCallsRpcAndClearsUser()
    {
        UserProfile user = new UserProfile("email@test.com", "User", ParticipantRole.STUDENT, new Uri("http://photo.com"));
        _service.CompleteLogin(user);

        bool eventRaised = false;
        _service.UserLoggedOut += (s, e) => eventRaised = true;

        _mockRpc.Setup(r => r.Call("core/logout", It.IsAny<byte[]>()))
                .ReturnsAsync(Array.Empty<byte>());

        await _service.LogoutAsync();

        _mockRpc.Verify(x => x.Call("core/logout", It.IsAny<byte[]>()), Times.Once);
        Assert.Null(_service.CurrentUser);
        Assert.False(_service.IsAuthenticated);
        Assert.True(eventRaised);
    }

    [Fact]
    public async Task LogoutAsyncRaisesEventEvenWhenRpcFails()
    {
        UserProfile user = new UserProfile("email@test.com", "User", ParticipantRole.STUDENT, new Uri("http://photo.com"));
        _service.CompleteLogin(user);

        bool eventRaised = false;
        _service.UserLoggedOut += (s, e) => eventRaised = true;

        _mockRpc.Setup(r => r.Call("core/logout", It.IsAny<byte[]>()))
                .ThrowsAsync(new ArgumentException("RPC error"));

        await _service.LogoutAsync();

        Assert.Null(_service.CurrentUser);
        Assert.False(_service.IsAuthenticated);
        Assert.True(eventRaised);
    }

    [Fact]
    public async Task LogoutAsyncRaisesEventWhenRpcThrowsInvalidOperationException()
    {
        UserProfile user = new UserProfile("email@test.com", "User", ParticipantRole.STUDENT, new Uri("http://photo.com"));
        _service.CompleteLogin(user);

        bool eventRaised = false;
        _service.UserLoggedOut += (s, e) => eventRaised = true;

        _mockRpc.Setup(r => r.Call("core/logout", It.IsAny<byte[]>()))
                .ThrowsAsync(new InvalidOperationException("Connection lost"));

        await _service.LogoutAsync();

        Assert.Null(_service.CurrentUser);
        Assert.True(eventRaised);
    }

    [Fact]
    public async Task LogoutAsyncHandlesGenericException()
    {
        UserProfile user = new UserProfile("email@test.com", "User", ParticipantRole.STUDENT, new Uri("http://photo.com"));
        _service.CompleteLogin(user);

        _mockRpc.Setup(r => r.Call("core/logout", It.IsAny<byte[]>()))
                .ThrowsAsync(new Exception("Unexpected error"));

        // Should not throw
        await _service.LogoutAsync();

        Assert.Null(_service.CurrentUser);
    }

    [Fact]
    public void IsAuthenticatedReturnsTrueWhenUserIsSet()
    {
        UserProfile user = new UserProfile("email@test.com", "User", ParticipantRole.STUDENT, new Uri("http://photo.com"));

        Assert.False(_service.IsAuthenticated);

        _service.CompleteLogin(user);

        Assert.True(_service.IsAuthenticated);
    }

    [Fact]
    public void PropertyChangedIsRaisedWhenCurrentUserChanges()
    {
        UserProfile user = new UserProfile("email@test.com", "User", ParticipantRole.STUDENT, new Uri("http://photo.com"));
        int propertyChangedCount = 0;

        _service.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(_service.CurrentUser) ||
                e.PropertyName == nameof(_service.IsAuthenticated))
            {
                propertyChangedCount++;
            }
        };

        _service.CompleteLogin(user);

        Assert.True(propertyChangedCount >= 1);
    }
}
