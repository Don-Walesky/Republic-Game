namespace Republic.Core.Tests.NationalStrategy;

using System;
using System.IO;
using System.Linq;
using Republic.Core.Client;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Economy.PrivateSector.Models;
using Republic.Core.Economy.PrivateSector.Services;
using Republic.Core.Economy.Treasury;
using Republic.Core.Government.Officials.Services;
using Republic.Core.Media.Models;
using Republic.Core.Media.Services;
using Republic.Core.NationalStrategy.Models;
using Republic.Core.NationalStrategy.Services;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Xunit;

public sealed class NationalStrategyTests
{
    private static readonly DateTimeOffset LaunchEpoch = new(1980, 1, 1, 0, 0, 0, RepublicTime.WatOffset);

    private static RepublicTime CreateTime(int day = 1)
    {
        return new RepublicTime(LaunchEpoch.AddDays(day), LaunchEpoch);
    }

    private static (Country Country, RepublicTime Time) CreateTestEnvironment(int day = 1)
    {
        var time = CreateTime(day);
        var country = new Country
        {
            Id = "country_veridia",
            Name = "Republic of Veridia",
            CapitalCity = "Veridia City",
            Treasury = new RepuTreasury(500_000_000.0, "country_veridia"),
            BaselineStability = 75.0,
            HappinessRating = 60.0,
            MinistryBudget = new MinistryBudget
            {
                DefenseAllocation = 500_000_000.0,
                HealthcareAllocation = 400_000_000.0,
                EducationAllocation = 350_000_000.0,
                InfrastructureAllocation = 600_000_000.0,
                ScienceAllocation = 250_000_000.0
            }
        };
        return (country, time);
    }

    [Fact]
    public void SetStrategicDirection_InitialAdoption_AppliesRetoolingExpenseAndFriction()
    {
        var (country, time) = CreateTestEnvironment(1);
        var service = new NationalStrategyService();

        double initialTreasury = country.Treasury.Balance;
        double initialStability = country.BaselineStability;
        double initialApproval = country.ApprovalRating;

        var (success, message, profile) = service.SetStrategicDirection(
            country,
            NationalStrategicDirection.ScienceAndTechnology,
            time);

        Assert.True(success);
        Assert.NotNull(profile);
        Assert.Equal(NationalStrategicDirection.ScienceAndTechnology, country.StrategicDirection);
        Assert.Equal(NationalStrategicDirection.ScienceAndTechnology, profile.Direction);
        Assert.Equal(1, profile.AdoptedDay);
        Assert.True(profile.IsTransitioning);
        Assert.Equal(14, profile.TransitionDaysRemaining);

        // Treasury deducted R 25M
        Assert.Equal(initialTreasury - 25_000_000.0, country.Treasury.Balance);

        // Stability -5%
        Assert.Equal(initialStability - 5.0, country.BaselineStability);

        // Approval -4%
        Assert.Equal(initialApproval - 4.0, country.ApprovalRating);
    }

    [Fact]
    public void SetStrategicDirection_WithinCooldown_IsRejected()
    {
        var (country, time) = CreateTestEnvironment(1);
        var service = new NationalStrategyService();

        var (success1, _, _) = service.SetStrategicDirection(
            country,
            NationalStrategicDirection.EnergyIndependence,
            time);
        Assert.True(success1);

        // Advance 10 days (cooldown is 30 days)
        var laterTime = CreateTime(11);
        var (success2, message2, _) = service.SetStrategicDirection(
            country,
            NationalStrategicDirection.Industrialisation,
            laterTime);

        Assert.False(success2);
        Assert.Contains("Cooldown active", message2);
        Assert.Equal(NationalStrategicDirection.EnergyIndependence, country.StrategicDirection);
    }

    [Fact]
    public void SetStrategicDirection_AfterCooldown_Succeeds()
    {
        var (country, time) = CreateTestEnvironment(1);
        var service = new NationalStrategyService();

        service.SetStrategicDirection(country, NationalStrategicDirection.EnergyIndependence, time);

        // Advance 35 days (past 30-day cooldown)
        var afterCooldownTime = CreateTime(36);
        var (success, message, profile) = service.SetStrategicDirection(
            country,
            NationalStrategicDirection.InfrastructureDevelopment,
            afterCooldownTime);

        Assert.True(success);
        Assert.Equal(NationalStrategicDirection.InfrastructureDevelopment, country.StrategicDirection);
        Assert.Equal(36, profile.AdoptedDay);
    }

    [Fact]
    public void EvaluateEmpiricalProgress_ReflectsRealCountryMetrics()
    {
        var (country, time) = CreateTestEnvironment(1);
        var service = new NationalStrategyService();

        // Science and Tech evaluation
        country.StrategicDirection = NationalStrategicDirection.ScienceAndTechnology;
        var progressInitial = service.UpdateStrategicProgress(country, time);
        Assert.True(progressInitial > 10.0 && progressInitial <= 100.0);

        // Boost science budget
        country.MinistryBudget!.ScienceAllocation = 500_000_000.0;
        var progressBoosted = service.UpdateStrategicProgress(country, time);
        Assert.True(progressBoosted > progressInitial);
    }

    [Fact]
    public void PrivateSectorProposals_AlignWithNationalStrategy()
    {
        var (country, time) = CreateTestEnvironment(1);
        country.StrategicDirection = NationalStrategicDirection.ScienceAndTechnology;

        var privateSectorService = new PrivateSectorService();
        var proposals = privateSectorService.GenerateInvestmentProposals(country, time);

        Assert.NotEmpty(proposals);

        // Proposals matching Technology sector receive alignment bonuses
        var techProposal = proposals.FirstOrDefault(p => p.Sector.Contains("Technology", StringComparison.OrdinalIgnoreCase) ||
                                                         p.Sector.Contains("Transport", StringComparison.OrdinalIgnoreCase) ||
                                                         p.Sector.Contains("Energy", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(techProposal);
    }

    [Fact]
    public void ChiefOfStaff_RecommendsPriorityMatchingStrategicDirection()
    {
        var (country, time) = CreateTestEnvironment(1);
        country.StrategicDirection = NationalStrategicDirection.EnergyIndependence;

        var officialsService = new ActiveOfficialsService();
        var recs = officialsService.EvaluateChiefOfStaffRecommendations(country, time);

        Assert.NotEmpty(recs);
        var stratRec = recs.FirstOrDefault(r => r.Category == "NationalStrategy" || r.Title.Contains("Strategic"));
        Assert.NotNull(stratRec);
        Assert.Contains("Energy", stratRec.Summary);
    }

    [Fact]
    public void MediaSimulation_GeneratesCoverageReflectingNationalStrategy()
    {
        var (country, time) = CreateTestEnvironment(1);
        country.StrategicDirection = NationalStrategicDirection.InfrastructureDevelopment;

        var mediaService = new MediaSimulationService();
        var reports = mediaService.GenerateMediaReports(country, time);

        Assert.NotEmpty(reports);
        var strategyReport = reports.FirstOrDefault(r => r.Category == "NationalStrategy" ||
                                                         r.Headline.Contains("Infrastructure") ||
                                                         r.Headline.Contains("Strategic"));
        Assert.NotNull(strategyReport);
    }

    [Fact]
    public void PresidentialDeskBridge_ApplyStrategicDirectionDecision_EndToEnd()
    {
        var (country, time) = CreateTestEnvironment(1);
        var tempState = Path.Combine(Path.GetTempPath(), $"state_strat_{Guid.NewGuid():N}.json");
        var tempSave = Path.Combine(Path.GetTempPath(), $"save_strat_{Guid.NewGuid():N}.sav");

        try
        {
            PresidentialDeskBridge.SaveAuthoritativeState(country, time, 1, tempSave);
            var snapshot = PresidentialDeskSnapshot.Create(country, time, 1, tempSave);
            File.WriteAllText(tempState, PresidentialDeskBridge.Serialize(snapshot));

            // 1. Dry run confirmation quote
            var quoteResult = PresidentialDeskBridge.ApplyStrategicDirectionDecision(
                "EnergyIndependence",
                confirmed: false,
                stateFilePath: tempState,
                saveFilePath: tempSave,
                requestId: "req-strat-01");

            Assert.False(quoteResult.Success);
            Assert.Equal("AwaitingConfirmation", quoteResult.Result);
            Assert.Contains("Energy Independence", quoteResult.Message);

            // 2. Confirmed adoption
            var actionResult = PresidentialDeskBridge.ApplyStrategicDirectionDecision(
                "EnergyIndependence",
                confirmed: true,
                stateFilePath: tempState,
                saveFilePath: tempSave,
                requestId: "req-strat-02");

            Assert.True(actionResult.Success);
            Assert.Equal("Success", actionResult.Result);

            // 3. Verify state file was re-exported with updated strategic direction
            var reloaded = PresidentialDeskBridge.LoadState(tempState);
            Assert.NotNull(reloaded);
            Assert.Equal("EnergyIndependence", reloaded.StrategicDirection);
            Assert.Equal("Energy Independence & Power Security", reloaded.StrategicDirectionDisplay);
            Assert.True(reloaded.StrategicTransitionRemaining > 0);
        }
        finally
        {
            if (File.Exists(tempState)) File.Delete(tempState);
            if (File.Exists(tempSave)) File.Delete(tempSave);
        }
    }
}
