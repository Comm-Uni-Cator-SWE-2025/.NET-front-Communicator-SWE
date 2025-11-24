using System.ComponentModel;
using System.Threading;
using Communicator.Core.UX;
using Xunit;

namespace Communicator.Core.Tests.Integration;

class SampleViewModel : ObservableObject
{
    private int _count;
    public int Count
    {
        get => _count;
        set => SetProperty(ref _count, value);
    }

    public RelayCommand IncrementCommand { get; }

    public SampleViewModel()
    {
        IncrementCommand = new RelayCommand(_ => Count++);
    }
}

public class ObservableAndRelayIntegrationTests
{
    [Fact]
    public void PropertyChanged_Raised_When_SetProperty_ChangesValue()
    {
        var vm = new SampleViewModel();
        var raised = false;
        vm.PropertyChanged += (s,e) => { if (e.PropertyName == nameof(SampleViewModel.Count)) raised = true; };
        vm.Count = 5;
        Assert.True(raised);
    }

    [Fact]
    public void RelayCommand_Updates_Property_When_Executed_OnSTA()
    {
        var done = false;
        var t = new Thread(() =>
        {
            var vm = new SampleViewModel();
            vm.IncrementCommand.Execute(null);
            Assert.Equal(1, vm.Count);
            done = true;
        });
        t.SetApartmentState(ApartmentState.STA);
        t.Start();
        t.Join();
        Assert.True(done);
    }
}
