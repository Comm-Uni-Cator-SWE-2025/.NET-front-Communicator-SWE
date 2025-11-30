using System.Threading;
using Communicator.App;
using Xunit;

namespace Communicator.App.Tests.Integration;

public class MainWindowIntegrationTests
{
    [Fact]
    public void MainWindow_Constructs_OnSTA()
    {
        bool created = false;
        var t = new Thread(() =>
        {
            var w = new MainWindow();
            Assert.NotNull(w);
            created = true;
        });
        t.SetApartmentState(ApartmentState.STA);
        t.Start();
        t.Join();
        Assert.True(created);
    }
}
