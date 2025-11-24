using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Communicator.App.ViewModels.Auth;
using Communicator.Controller.Serialization;
using Communicator.Core.RPC;
using Communicator.Core.UX.Services;
using Moq;
using Xunit;
using Communicator.Controller.Meeting;

namespace Communicator.App.Tests.Unit;

public class AuthViewModelTests
{
    private readonly Mock<IRPC> _mockRpc;
    private readonly Mock<IToastService> _mockToastService;

    public AuthViewModelTests()
    {
        _mockRpc = new Mock<IRPC>();
        _mockToastService = new Mock<IToastService>();
    }

    [Fact]
    public void SignInWithGoogle_Success_SetsCurrentUser()
    {
        // Arrange
        var vm = new AuthViewModel(_mockRpc.Object, _mockToastService.Object);
        var expectedUser = new UserProfile { DisplayName = "Test User", Email = "test@example.com" };
        var serializedUser = DataSerializer.Serialize(expectedUser);

        _mockRpc.Setup(r => r.Call("core/register", It.IsAny<byte[]>()))
                .ReturnsAsync(serializedUser);
    }

    [Fact]
    public void Reset_ClearsState()
    {
        var vm = new AuthViewModel(_mockRpc.Object, _mockToastService.Object);
        vm.ErrorMessage = "Error";
        vm.IsLoading = true;
        
        vm.Reset();

        Assert.Empty(vm.ErrorMessage);
        Assert.False(vm.IsLoading);
        Assert.Null(vm.CurrentUser);
    }
}
