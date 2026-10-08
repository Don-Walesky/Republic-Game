namespace Republic.Core.Tests.World;

using Republic.Core.Economy.Treasury;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class NationalStrengthTests
{
    private static RepublicTime CreateTestTime()
    {
        var clock = RepublicClock.CreateControlled();
        return clock.CurrentTime;
    }

    [Fact]
    public void EconomicStrength_R0Treasury_GivesZero()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Test", time, treasury: new RepuTreasury(0.0));

        var strength = country.GetNationalStrength();

        Assert.Equal(0.0, strength.Economic);
        Assert.Equal("0%", strength.FormattedEconomic);
        Assert.Contains("Economic: 0%", strength.Lines);
    }

    [Fact]
    public void EconomicStrength_R2BillionTreasury_GivesOne()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Test", time, treasury: new RepuTreasury(2_000_000_000.0));

        var strength = country.GetNationalStrength();

        Assert.Equal(1.0, strength.Economic);
        Assert.Equal("100%", strength.FormattedEconomic);
        Assert.Contains("Economic: 100%", strength.Lines);
    }

    [Fact]
    public void EconomicStrength_PartialBillion_ClampsAndScalesProportionally()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Test", time, treasury: new RepuTreasury(500_000_000.0));

        var strength = country.GetNationalStrength();

        Assert.Equal(0.5, strength.Economic);
        Assert.Equal("50%", strength.FormattedEconomic);
    }

    [Fact]
    public void DiplomaticAndMilitaryStrength_AreZero()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Test", time);

        var strength = country.GetNationalStrength();

        Assert.Equal(0.0, strength.Diplomatic);
        Assert.Equal(0.0, strength.Military);
        Assert.Equal("0%", strength.FormattedDiplomatic);
        Assert.Equal("0%", strength.FormattedMilitary);
        Assert.Contains("Diplomatic: 0%", strength.Lines);
        Assert.Contains("Military: 0%", strength.Lines);
    }

    [Fact]
    public void Total_IsAverageOfSixScores()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Test", time, treasury: new RepuTreasury(1_000_000_000.0));
        country.StateCapacity.InfrastructureCondition = 0.6;
        country.StateCapacity.CivilServiceQuality = 0.4;
        country.FoundingBuffs = null;

        var strength = country.GetNationalStrength();

        var expectedAverage = (strength.Economic + strength.Institutional + strength.Infrastructure +
                               strength.HumanCapital + strength.Diplomatic + strength.Military) / 6.0;

        Assert.Equal(expectedAverage, strength.Total, precision: 6);
    }

    [Fact]
    public void BuildingScore_DoesNotChangeTreasury()
    {
        var time = CreateTestTime();
        var initialBalance = 15_000_000_000.0;
        var country = Country.Found("Republic of Test", time, treasury: new RepuTreasury(initialBalance));

        var strength = country.GetNationalStrength();

        Assert.Equal(initialBalance, country.Treasury.Balance);
        Assert.Equal(1.0, strength.Economic);
    }

    [Fact]
    public void CountryIsolation_ScoringCountryA_DoesNotChangeCountryB()
    {
        var time = CreateTestTime();
        var countryA = Country.Found("Republic of Alpha", time, treasury: new RepuTreasury(3_000_000_000.0));
        var countryB = Country.Found("Republic of Beta", time, treasury: new RepuTreasury(250_000_000.0));
        countryB.StateCapacity.InfrastructureCondition = 0.75;

        var initialTreasuryB = countryB.Treasury.Balance;
        var initialInfraB = countryB.StateCapacity.InfrastructureCondition;
        var initialProjectsB = countryB.Projects.Count;

        var strengthA = countryA.GetNationalStrength();

        Assert.Equal(initialTreasuryB, countryB.Treasury.Balance);
        Assert.Equal(initialInfraB, countryB.StateCapacity.InfrastructureCondition);
        Assert.Equal(initialProjectsB, countryB.Projects.Count);
        Assert.NotEqual(strengthA.CountryId, countryB.Id);
    }

    [Fact]
    public void HumanCapital_UsesFoundingBuff_WhenAboveZero()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Test", time);
        country.StateCapacity.CivilServiceQuality = 0.3;
        country.FoundingBuffs = new FoundingBuffSnapshot(
            asOfTime: time,
            administrativeEfficiency: 1.0,
            innovationDrive: 1.0,
            developmentInitiative: 1.0,
            investorConfidence: 1.0,
            diplomaticRecognitionMomentum: 1.0,
            institutionBuilding: 0.85,
            temporaryNationalUnity: 1.0);

        var strength = country.GetNationalStrength();

        Assert.Equal(0.85, strength.HumanCapital);
        Assert.Equal("85%", strength.FormattedHumanCapital);
    }

    [Fact]
    public void HumanCapital_FallsBackToCivilServiceQuality_WhenFoundingBuffZeroOrNull()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Test", time);
        country.StateCapacity.CivilServiceQuality = 0.45;
        country.FoundingBuffs = null;

        var strength = country.GetNationalStrength();

        Assert.Equal(0.45, strength.HumanCapital);
        Assert.Equal("45%", strength.FormattedHumanCapital);
    }

    [Fact]
    public void Infrastructure_MatchesExistingInfrastructureInput()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Test", time);
        country.StateCapacity.InfrastructureCondition = 0.7;

        var strength = country.GetNationalStrength();

        Assert.Equal(0.7, strength.Infrastructure);
        Assert.Equal("70%", strength.FormattedInfrastructure);
    }

    [Fact]
    public void Institutional_MatchesExistingStateCapacityScore()
    {
        var time = CreateTestTime();
        var country = Country.Found("Republic of Test", time);

        var strength = country.GetNationalStrength();

        Assert.Equal(country.StateCapacityScore, strength.Institutional, precision: 6);
    }
}
