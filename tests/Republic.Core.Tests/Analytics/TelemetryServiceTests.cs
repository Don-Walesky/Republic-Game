namespace Republic.Core.Tests.Analytics;

using Republic.Core.Analytics.Services;
using Xunit;

public sealed class TelemetryServiceTests
{
    [Fact]
    public void RecordSnapshot_StoresTelemetryPoints()
    {
        var service = new TelemetryService();
        service.RecordSnapshot(1, 10_000_000, 50_000_000, 75.0, 80.0);
        service.RecordSnapshot(2, 12_000_000, 52_000_000, 85.0, 82.0);

        var history = service.GetHistory();

        Assert.Equal(2, history.Count);
        Assert.Equal(80.0, service.CalculateAverageHappiness());
    }
}
