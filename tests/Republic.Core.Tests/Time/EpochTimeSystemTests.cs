namespace Republic.Core.Tests.Time;

using Republic.Core.Events;
using Republic.Core.Time;
using Xunit;

public sealed class EpochTimeSystemTests
{
    [Fact]
    public void CurrentSimulatedDateTime_CalculatesCorrectDateFromEpoch()
    {
        var logger = new TestLogger();
        var bus = new EventBus(new EventBusOptions(), logger);
        var epoch = new DateTime(2027, 2, 10, 0, 0, 0, DateTimeKind.Utc);

        var config = new TimeSystemConfiguration
        {
            TickRate = 60,
            EpochStartDate = epoch
        };

        var timeSystem = new TimeSystem(config, bus, logger);

        Assert.Equal(epoch, timeSystem.CurrentSimulatedDateTime);
    }
}
