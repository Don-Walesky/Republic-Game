namespace Republic.Core.Tests.Client;

using System;
using System.IO;
using System.Linq;
using Republic.Core.Cabinet.Models;
using Republic.Core.Client;
using Republic.Core.Economy.Projects;
using Republic.Core.Economy.Treasury;
using Republic.Core.Politics;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Xunit;

public sealed class ExecutiveOfficeRevisionTests
{
    private static RepublicTime CreateTestTime(int hour = 0, long day = 1)
    {
        var epoch = new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));
        var current = epoch.AddDays(day - 1).AddHours(hour);
        return new RepublicTime(current, epoch);
    }

    [Fact]
    public void InitiateUniversityProject_RequiresAppointedMinisterOfEducation()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Azuria", time);
        country.Treasury.Deposit(5_000_000.0);

        // Without Education Minister appointed
        var outcomeWithoutMinister = PresidentialWorkspaceHandler.InitiateUniversityProject(country, time, confirmed: true);
        Assert.False(outcomeWithoutMinister.IsSuccess);
        Assert.Equal(ExecutiveActionResult.PrerequisitesNotMet, outcomeWithoutMinister.Result);
        Assert.Contains("Minister of Education", outcomeWithoutMinister.Message);

        // Appoint Minister of Education
        country.AppointMinister(new Minister
        {
            Name = "Dr. Amina Bello",
            Portfolio = CabinetPortfolio.Education,
            Competence = 0.85,
            Loyalty = 0.9
        }, CabinetPortfolio.Education);

        var outcomeWithMinister = PresidentialWorkspaceHandler.InitiateUniversityProject(country, time, confirmed: true);
        Assert.True(outcomeWithMinister.IsSuccess);
        Assert.Equal(ExecutiveActionResult.Success, outcomeWithMinister.Result);
        Assert.Contains("National University construction commenced", outcomeWithMinister.Message);
    }

    [Fact]
    public void InitiateRefineryProject_RequiresAppointedMinisterOfOilAndGas()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Azuria", time);
        country.Treasury.Deposit(5_000_000.0);

        // Without Oil & Gas Minister
        var outcomeWithout = PresidentialWorkspaceHandler.InitiateRefineryProject(country, time, confirmed: true);
        Assert.False(outcomeWithout.IsSuccess);
        Assert.Equal(ExecutiveActionResult.PrerequisitesNotMet, outcomeWithout.Result);
        Assert.Contains("Minister of Oil & Gas", outcomeWithout.Message);

        // Appoint Minister of Oil & Gas
        country.AppointMinister(new Minister
        {
            Name = "Tariq Al-Mansoor",
            Portfolio = CabinetPortfolio.OilAndGas,
            Competence = 0.8,
            Loyalty = 0.85
        }, CabinetPortfolio.OilAndGas);

        var outcomeWith = PresidentialWorkspaceHandler.InitiateRefineryProject(country, time, confirmed: true);
        Assert.True(outcomeWith.IsSuccess);
        Assert.Equal(ExecutiveActionResult.Success, outcomeWith.Result);
        Assert.Contains("National Refinery construction commenced", outcomeWith.Message);
    }

    [Fact]
    public void InitiateMineProject_RequiresAppointedMinisterOfResources()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Azuria", time);
        country.Treasury.Deposit(5_000_000.0);

        // Without Resources Minister
        var outcomeWithout = PresidentialWorkspaceHandler.InitiateMineProject(country, time, confirmed: true);
        Assert.False(outcomeWithout.IsSuccess);
        Assert.Equal(ExecutiveActionResult.PrerequisitesNotMet, outcomeWithout.Result);
        Assert.Contains("Minister of Resources", outcomeWithout.Message);

        // Appoint Minister of Resources
        country.AppointMinister(new Minister
        {
            Name = "Gabriel Vance",
            Portfolio = CabinetPortfolio.Resources,
            Competence = 0.82,
            Loyalty = 0.88
        }, CabinetPortfolio.Resources);

        var outcomeWith = PresidentialWorkspaceHandler.InitiateMineProject(country, time, confirmed: true);
        Assert.True(outcomeWith.IsSuccess);
        Assert.Equal(ExecutiveActionResult.Success, outcomeWith.Result);
        Assert.Contains("National Mine construction commenced", outcomeWith.Message);
    }

    [Fact]
    public void LegislativeDifficultyEngine_Generates5050TradeoffsAndAppliesConsequences()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Azuria", time);
        country.FoundParty("Civic Alliance", time);
        country.Treasury.Deposit(2_000_000.0);

        var bill = country.ProposeBill("National Infrastructure Development Act");
        Assert.NotNull(bill);
        Assert.NotEmpty(bill!.PositiveConsequence);
        Assert.NotEmpty(bill.NegativeConsequence);
        Assert.NotEmpty(bill.GovernancePrinciple);

        // Vote on bill
        var voted = country.VoteBill(bill, time);
        Assert.NotNull(voted);
        Assert.True(voted!.IsPassed);
        Assert.True(voted.AyesCount >= 50);
        Assert.Equal(100, voted.TotalVotes);

        // Veto
        bool vetoed = country.VetoBill("National Infrastructure Development Act", out var vetoMessage);
        Assert.NotEmpty(vetoMessage);
    }

    [Fact]
    public void PresidentialDeskSnapshot_IncludesEmailsAndBriefings()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Azuria", time);
        country.Treasury.Deposit(10_000_000.0);

        var snapshot = PresidentialDeskSnapshot.Create(country, time);

        Assert.NotNull(snapshot.Emails);
        Assert.NotEmpty(snapshot.Emails);
        Assert.True(snapshot.UnreadEmailCount >= 0);

        Assert.NotEmpty(snapshot.PressSecretaryName);
        Assert.NotEmpty(snapshot.PressBriefingHeadline);
        Assert.NotEmpty(snapshot.CentralBankGovernorName);
        Assert.True(snapshot.MilitaryPersonnel > 0);
    }
}

