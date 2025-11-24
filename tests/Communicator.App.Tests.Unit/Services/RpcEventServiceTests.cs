using System;
using Communicator.App.Services;
using Communicator.Core.UX.Services;
using Xunit;

namespace Communicator.App.Tests.Unit.Services
{
    public class RpcEventServiceTests
    {
        [Fact]
        public void TriggerFrameReceived_RaisesEvent()
        {
            var service = new RpcEventService();
            var data = new byte[] { 1, 2, 3 };
            RpcDataEventArgs? args = null;

            service.FrameReceived += (s, e) => args = e;

            service.TriggerFrameReceived(data);

            Assert.NotNull(args);
            Assert.Equal(data, args.Data);
        }

        [Fact]
        public void TriggerParticipantsListUpdated_RaisesEvent()
        {
            var service = new RpcEventService();
            var json = "{}";
            RpcStringEventArgs? args = null;

            service.ParticipantsListUpdated += (s, e) => args = e;

            service.TriggerParticipantsListUpdated(json);

            Assert.NotNull(args);
            Assert.Equal(json, args.Value);
        }
    }
}
