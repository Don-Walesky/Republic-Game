namespace Republic.Core.Tests.Diplomacy;

using System.Threading.Tasks;
using Xunit;
using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.World.Models;
using Republic.Core.World.Services;

public class GeopoliticalSummitTests
{
    [Fact]
    public void CalculateGeopoliticalTensionIndex_Returns_Baseline_When_No_Provinces()
    {
        var service = new GeographyService(new TestLogger());
        double tension = service.CalculateGeopoliticalTensionIndex();

        Assert.Equal(15.0, tension);
    }

    [Fact]
    public async Task HostBilateralSummitAsync_Increases_Provincial_Stability()
    {
        var service = new GeographyService(new TestLogger());
        var province = service.RegisterProvince(new ProvinceState
        {
            Id = "prov-01",
            Name = "Capital State",
            CountryId = "country-republic",
            LocalStability = 60.0
        });

        bool success = await service.HostBilateralSummitAsync("country-valoria", "Treaty of Harmony");

        Assert.True(success);
        Assert.Equal(65.0, province.LocalStability);
        Assert.Equal(35.0, province.RebellionRisk);
    }
}
