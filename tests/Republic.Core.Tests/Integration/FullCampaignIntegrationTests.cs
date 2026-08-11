namespace Republic.Core.Tests.Integration;

using System;
using System.Threading.Tasks;
using Republic.App;
using Republic.Core.Cabinet.Models;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Engine;
using Republic.Core.Government;
using Republic.Core.Military.Models;
using Republic.Core.World.Models;
using Xunit;

public sealed class FullCampaignIntegrationTests
{
    [Fact]
    public async Task FullCampaign_Simulates50Ticks_ExecutesDirectivesAndEvaluatesEndState()
    {
        var bootstrapper = new ApplicationBootstrapper();
        var app = bootstrapper.Bootstrap();

        // 1. Register main country and starting state
        var playerCountry = app.WorldManager.Countries.RegisterCountry(new Country
        {
            Id = "player-country",
            Name = "Republic of Arcadia",
            BaselineStability = 85.0
        });

        app.WorldManager.Economic.DepositTreasury(15_000_000.0);

        // 2. Perform Cabinet Appointment
        await app.CabinetService.AppointMinisterAsync(new Minister
        {
            Name = "General Pendelton",
            CompetenceRating = 90.0,
            LoyaltyRating = 95.0
        }, CabinetPortfolio.Defense);

        Assert.NotNull(app.CabinetService.GetAppointedMinister(CabinetPortfolio.Defense));

        // 3. Update Tax Policy
        await app.BudgetService.UpdateTaxPolicyAsync(new TaxPolicy
        {
            IncomeTaxRate = 0.25,
            CorporateTaxRate = 0.20
        });

        // 4. Register Regional Province & Invest Infrastructure
        var province = app.WorldManager.Geography.RegisterProvince(new ProvinceState
        {
            Name = "Aethel Basin",
            CountryId = playerCountry.Id,
            InfrastructureIndex = 50.0
        });

        bool invested = await app.WorldManager.Geography.InvestInRegionalInfrastructureAsync(province.Id, 100_000m);
        Assert.True(invested);

        // 5. Set DEFCON Alert Level
        var state = new GovernmentState { CountryName = "Arcadia", TreasuryBalance = 15_000_000m };
        await app.MilitaryService.SetDefconLevelAsync(state, DefconLevel.Defcon3_AirForceStandby);

        // 6. Initialize Engine and Run Ticks
        await app.Engine.InitializeAsync();
        await app.Engine.RunAsync(10, TimeSpan.FromMilliseconds(10));
        Assert.True(app.TimeSystem.CurrentTick > 0);

        // 7. Evaluate End-State Condition
        var evaluator = new GameEndConditionEvaluator();
        var demographics = app.WorldManager.Demographics.GetDemographics();
        var readiness = app.MilitaryService.GetReadinessReport(state);
        var result = evaluator.EvaluateGameState(app.WorldManager.Economic.GetIndicators().TreasuryBalance, demographics, readiness, app.TimeSystem.CurrentTick);

        Assert.NotNull(result);
        Assert.Equal(GameEndStatus.Ongoing, result.Status);
    }
}
