using Communicator.UX.Core;
using Xunit;

namespace Communicator.Core.Tests.Unit;

public class RelayCommandTests
{
    [Fact]
    public void Execute_Invokes_Action_And_CanExecute_Respected()
    {
        bool executed = false;
        var command = new RelayCommand(_ => executed = true, _ => false);
        Assert.False(command.CanExecute(null));
        command.Execute(null);
        Assert.True(executed);
    }

    [Fact]
    public void RaiseCanExecuteChanged_DoesNotThrow()
    {
        var command = new RelayCommand(_ => { });
        command.RaiseCanExecuteChanged();
        Assert.True(command.CanExecute(null));
    }
}
