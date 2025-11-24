using Communicator.Core.UX;

namespace UX.Core.Tests;

public class RelayCommandTests
{
    [Fact]
    public void Execute_CallsExecuteDelegate()
    {
        bool executed = false;
        var command = new RelayCommand(_ => executed = true);

        command.Execute(null);

        Assert.True(executed);
    }

    [Fact]
    public void Execute_PassesParameterToDelegate()
    {
        object? receivedParameter = null;
        var command = new RelayCommand(param => receivedParameter = param);
        string testParameter = "test";

        command.Execute(testParameter);

        Assert.Equal(testParameter, receivedParameter);
    }

    [Fact]
    public void CanExecute_ReturnsTrue_WhenNoPredicateProvided()
    {
        var command = new RelayCommand(_ => { });

        var result = command.CanExecute(null);

        Assert.True(result);
    }

    [Fact]
    public void CanExecute_ReturnsPredicateResult_WhenPredicateProvided()
    {
        var command = new RelayCommand(_ => { }, _ => false);

        var result = command.CanExecute(null);

        Assert.False(result);
    }

    [Fact]
    public void CanExecute_PassesParameterToPredicate()
    {
        object? receivedParameter = null;
        var command = new RelayCommand(
            _ => { },
            param =>
            {
                receivedParameter = param;
                return true;
            });
        string testParameter = "test";

        command.CanExecute(testParameter);

        Assert.Equal(testParameter, receivedParameter);
    }

    [Fact]
    public void RaiseCanExecuteChanged_RaisesCanExecuteChangedEvent()
    {
        var command = new RelayCommand(_ => { });
        bool eventRaised = false;

        command.CanExecuteChanged += (sender, args) => eventRaised = true;

        command.RaiseCanExecuteChanged();

        Assert.True(eventRaised);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenExecuteIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new RelayCommand(null!));
    }
}
