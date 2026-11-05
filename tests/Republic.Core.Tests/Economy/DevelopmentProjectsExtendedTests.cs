namespace Republic.Core.Tests.Economy;

using System;
using System.IO;
using System.Linq;
using Republic.Core.Cabinet.Models;
using Republic.Core.Client;
using Republic.Core.Economy.Projects;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Xunit;

public sealed class DevelopmentProjectsExtendedTests
{
    private static RepublicTime CreateTestTime(int hour = 6)
    {
        var clock = RepublicClock.CreateControlled();
        return clock.FromDayAndTime(0, new TimeOnly(hour, 0));
    }

    private static Minister CreateMinister(string name, CabinetPortfolio portfolio)
    {
        return new Minister
        {
            Name = name,
            Portfolio = portfolio,
            IsAppointed = true,
            Competence = 0.85,
            Loyalty = 0.80
        };
    }

    [Fact]
    public void HighSpeedRail_Requires_InfrastructureMinister_And_Boosts_Trade_And_Transport()
    {
        var time = CreateTestTime();
        var country = Country.Found("Arcadia", time, id: "arcadia");
        country.Treasury.Deposit(5_000_000.0);

        // Without minister
        var outcomeNoMin = PresidentialWorkspaceHandler.InitiateHighSpeedRailProject(country, time);
        Assert.False(outcomeNoMin.IsSuccess);
        Assert.Equal(ExecutiveActionResult.PrerequisitesNotMet, outcomeNoMin.Result);

        // Appoint infrastructure minister
        country.AppointMinister(CreateMinister("Dr. Clara Sterling", CabinetPortfolio.Infrastructure));
        var outcome = PresidentialWorkspaceHandler.InitiateHighSpeedRailProject(country, time);
        Assert.True(outcome.IsSuccess);
        Assert.NotNull(outcome.Project);
        Assert.IsType<NationalHighSpeedRailProject>(outcome.Project);

        // Complete project
        var rail = (NationalHighSpeedRailProject)outcome.Project;
        var finishTime = time.Add(NationalHighSpeedRailProject.Duration);
        bool completed = country.CompleteProject(rail, finishTime);
        Assert.True(completed);
        Assert.True(country.TradeBalance >= 350_000_000.0);
        Assert.True(country.Yield.InfrastructureCapacity >= 8.0);
    }

    [Fact]
    public void DeepwaterPort_Expands_Trade_And_IndustrialCapacity()
    {
        var time = CreateTestTime();
        var country = Country.Found("Arcadia", time, id: "arcadia");
        country.Treasury.Deposit(5_000_000.0);
        country.AppointMinister(CreateMinister("Admiral Zhao", CabinetPortfolio.Infrastructure));

        var outcome = PresidentialWorkspaceHandler.InitiateDeepwaterPortProject(country, time);
        Assert.True(outcome.IsSuccess);
        var port = (NationalDeepwaterPortProject)outcome.Project!;

        country.CompleteProject(port, time.Add(NationalDeepwaterPortProject.Duration));
        Assert.True(country.TradeBalance >= 600_000_000.0);
        Assert.True(country.GrossDomesticProduct >= 10_000_000_000.0);
        Assert.True(country.Yield.IndustrialCapacity >= 4.0);
    }

    [Fact]
    public void TeachingHospital_Requires_HealthMinister_And_Elevates_Approval()
    {
        var time = CreateTestTime();
        var country = Country.Found("Arcadia", time, id: "arcadia");
        country.Treasury.Deposit(5_000_000.0);
        country.AppointMinister(CreateMinister("Dr. Julian Mercer", CabinetPortfolio.Health));

        var initialApproval = country.HappinessRating;
        var outcome = PresidentialWorkspaceHandler.InitiateHospitalProject(country, time);
        Assert.True(outcome.IsSuccess);

        var hosp = (NationalHospitalProject)outcome.Project!;
        country.CompleteProject(hosp, time.Add(NationalHospitalProject.Duration));
        Assert.True(country.HappinessRating >= initialApproval + 5.0);
    }

    [Fact]
    public void Supercomputer_Requires_EducationMinister_And_Elevates_AI_Capacities()
    {
        var time = CreateTestTime();
        var country = Country.Found("Arcadia", time, id: "arcadia");
        country.Treasury.Deposit(5_000_000.0);
        country.AppointMinister(CreateMinister("Prof. Amara Diallo", CabinetPortfolio.Education));

        var outcome = PresidentialWorkspaceHandler.InitiateSupercomputerProject(country, time);
        Assert.True(outcome.IsSuccess);

        var sc = (NationalSupercomputerProject)outcome.Project!;
        country.CompleteProject(sc, time.Add(NationalSupercomputerProject.Duration));
        Assert.True(country.Yield.ScienceCapacity >= 6.0);
        Assert.True(country.Yield.InnovationCapacity >= 8.0);
        Assert.True(country.Yield.IntelligenceCapacity >= 6.0);
    }

    [Fact]
    public void IrrigationDam_Requires_ResourcesMinister_And_Expands_Resources()
    {
        var time = CreateTestTime();
        var country = Country.Found("Arcadia", time, id: "arcadia");
        country.Treasury.Deposit(5_000_000.0);
        country.AppointMinister(CreateMinister("Hon. Marcus Thorne", CabinetPortfolio.Resources));

        var outcome = PresidentialWorkspaceHandler.InitiateIrrigationDamProject(country, time);
        Assert.True(outcome.IsSuccess);

        var dam = (NationalIrrigationDamProject)outcome.Project!;
        country.CompleteProject(dam, time.Add(NationalIrrigationDamProject.Duration));
        Assert.True(country.Yield.NaturalResourceOutput >= 6.0);
        Assert.True(country.Yield.EnergyCapacity >= 4.0);
    }
}
