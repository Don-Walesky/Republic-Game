namespace Republic.Core.Tests.NationalYield;

using Republic.Core.Economy.Treasury;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class FoundingBuffTests
{
    private readonly RepublicClock _clock;
    private readonly FoundingBuffEvaluator _evaluator;

    public FoundingBuffTests()
    {
        _clock = RepublicClock.CreateControlled();
        _evaluator = new FoundingBuffEvaluator();
    }

    private Country CreateTestCountry(
        string name = "Arcadia",
        RepublicTime? foundingTime = null,
        double stability = 80.0,
        double industry = 60.0)
    {
        var yield = new Republic.Core.NationalYield.NationalYield
        {
            IndustrialCapacity = industry,
            AdministrativeCapacity = 60.0,
            InfrastructureCapacity = 60.0,
            HumanCapital = 60.0,
            ScienceCapacity = 60.0,
            SecurityCapacity = 60.0
        };

        var time = foundingTime ?? _clock.CurrentTime;
        return Country.Found(
            name: name,
            foundingTime: time,
            baselineStability: stability,
            yield: yield,
            treasury: new RepuTreasury(0.0));
    }

    [Fact]
    public void FoundingBuffs_AtFounding_AllSevenAreOne()
    {
        var country = CreateTestCountry(foundingTime: _clock.CurrentTime);

        var snapshot = _evaluator.Evaluate(country, _clock.CurrentTime);

        Assert.Equal(1.0, snapshot.AdministrativeEfficiency);
        Assert.Equal(1.0, snapshot.InnovationDrive);
        Assert.Equal(1.0, snapshot.DevelopmentInitiative);
        Assert.Equal(1.0, snapshot.InvestorConfidence);
        Assert.Equal(1.0, snapshot.DiplomaticRecognitionMomentum);
        Assert.Equal(1.0, snapshot.InstitutionBuilding);
        Assert.Equal(1.0, snapshot.TemporaryNationalUnity);
        Assert.Equal(_clock.CurrentTime, snapshot.AsOfTime);

        // Verify dictionary mapping contains all 7 canonical names
        var dict = snapshot.ToDictionary();
        Assert.Equal(7, dict.Count);
        Assert.All(dict.Values, v => Assert.Equal(1.0, v));

        // Verify percentage string formatting
        Assert.Equal("100%", snapshot.FormattedAdministrativeEfficiency);
        Assert.Equal("100%", snapshot.FormattedInnovationDrive);
    }

    [Fact]
    public void FoundingBuffs_AtDay7_AllSevenAreStillOne()
    {
        var foundingTime = _clock.CurrentTime;
        var country = CreateTestCountry(foundingTime: foundingTime);

        var day7 = foundingTime + TimeSpan.FromDays(7);
        var snapshot = _evaluator.Evaluate(country, day7);

        Assert.Equal(1.0, snapshot.AdministrativeEfficiency);
        Assert.Equal(1.0, snapshot.InnovationDrive);
        Assert.Equal(1.0, snapshot.DevelopmentInitiative);
        Assert.Equal(1.0, snapshot.InvestorConfidence);
        Assert.Equal(1.0, snapshot.DiplomaticRecognitionMomentum);
        Assert.Equal(1.0, snapshot.InstitutionBuilding);
        Assert.Equal(1.0, snapshot.TemporaryNationalUnity);
        Assert.Equal(day7, snapshot.AsOfTime);
    }

    [Fact]
    public void FoundingBuffs_AtDay17Point5_AllSevenAreHalfWithinSmallTolerance()
    {
        var foundingTime = _clock.CurrentTime;
        var country = CreateTestCountry(foundingTime: foundingTime);

        var day17Point5 = foundingTime + TimeSpan.FromDays(17.5);
        var snapshot = _evaluator.Evaluate(country, day17Point5);

        Assert.Equal(0.5, snapshot.AdministrativeEfficiency, precision: 4);
        Assert.Equal(0.5, snapshot.InnovationDrive, precision: 4);
        Assert.Equal(0.5, snapshot.DevelopmentInitiative, precision: 4);
        Assert.Equal(0.5, snapshot.InvestorConfidence, precision: 4);
        Assert.Equal(0.5, snapshot.DiplomaticRecognitionMomentum, precision: 4);
        Assert.Equal(0.5, snapshot.InstitutionBuilding, precision: 4);
        Assert.Equal(0.5, snapshot.TemporaryNationalUnity, precision: 4);
        Assert.Equal(day17Point5, snapshot.AsOfTime);

        Assert.Equal("50%", snapshot.FormattedAdministrativeEfficiency);
    }

    [Fact]
    public void FoundingBuffs_AtDay28AndAfter_AllSevenAreZero()
    {
        var foundingTime = _clock.CurrentTime;
        var country = CreateTestCountry(foundingTime: foundingTime);

        // At exact day 28
        var day28 = foundingTime + TimeSpan.FromDays(28);
        var snapshot28 = _evaluator.Evaluate(country, day28);

        Assert.Equal(0.0, snapshot28.AdministrativeEfficiency);
        Assert.Equal(0.0, snapshot28.InnovationDrive);
        Assert.Equal(0.0, snapshot28.DevelopmentInitiative);
        Assert.Equal(0.0, snapshot28.InvestorConfidence);
        Assert.Equal(0.0, snapshot28.DiplomaticRecognitionMomentum);
        Assert.Equal(0.0, snapshot28.InstitutionBuilding);
        Assert.Equal(0.0, snapshot28.TemporaryNationalUnity);

        // After day 28 (e.g. day 45)
        var day45 = foundingTime + TimeSpan.FromDays(45);
        var snapshot45 = _evaluator.Evaluate(country, day45);

        Assert.Equal(0.0, snapshot45.AdministrativeEfficiency);
        Assert.Equal(0.0, snapshot45.InnovationDrive);
        Assert.Equal(0.0, snapshot45.DevelopmentInitiative);
        Assert.Equal(0.0, snapshot45.InvestorConfidence);
        Assert.Equal(0.0, snapshot45.DiplomaticRecognitionMomentum);
        Assert.Equal(0.0, snapshot45.InstitutionBuilding);
        Assert.Equal(0.0, snapshot45.TemporaryNationalUnity);

        Assert.Equal("0%", snapshot45.FormattedAdministrativeEfficiency);
    }

    [Fact]
    public void FoundingBuffs_TwoCountriesFoundedOnDifferentDays_DoNotShareBuffStrength()
    {
        var epoch = _clock.CurrentTime;

        // Country A founded on Day 0
        var countryA = CreateTestCountry(name: "Country A", foundingTime: epoch);

        // Country B founded on Day 20
        var countryBFoundingTime = epoch + TimeSpan.FromDays(20);
        var countryB = CreateTestCountry(name: "Country B", foundingTime: countryBFoundingTime);

        // Evaluate both at Day 25
        var evaluationTime = epoch + TimeSpan.FromDays(25);

        var snapshotA = _evaluator.Evaluate(countryA, evaluationTime);
        var snapshotB = _evaluator.Evaluate(countryB, evaluationTime);

        // Country A has aged 25 days (linear decay between 7 and 28): (28 - 25) / 21 = 3 / 21 = 1 / 7 ~= 0.1429
        var expectedA = (28.0 - 25.0) / 21.0;
        Assert.Equal(expectedA, snapshotA.AdministrativeEfficiency, precision: 4);

        // Country B has aged 5 days (still within first 7 days): full strength 1.0
        Assert.Equal(1.0, snapshotB.AdministrativeEfficiency);

        // Verify Country A and Country B have distinct buff states
        Assert.NotEqual(snapshotA.AdministrativeEfficiency, snapshotB.AdministrativeEfficiency);

        // Storing on Country A does not mutate Country B
        countryA.EvaluateFoundingBuffs(evaluationTime, _evaluator);
        countryB.EvaluateFoundingBuffs(evaluationTime, _evaluator);

        Assert.NotNull(countryA.FoundingBuffs);
        Assert.NotNull(countryB.FoundingBuffs);
        Assert.Equal(snapshotA.AdministrativeEfficiency, countryA.FoundingBuffs.AdministrativeEfficiency);
        Assert.Equal(snapshotB.AdministrativeEfficiency, countryB.FoundingBuffs.AdministrativeEfficiency);
    }

    [Fact]
    public void FoundingDayYieldMultiplier_RemainsX10AndNotAppliedTwice()
    {
        var foundingTime = _clock.CurrentTime;
        var country = CreateTestCountry(foundingTime: foundingTime);

        var cycleService = new NationalYieldCycleService();

        // 1. Calculate normal yield directly using calculator
        var calculator = new NationalYieldCalculator();
        var inputs = NationalYieldCalculationInputs.FromCountry(country);
        var baseYield = calculator.Calculate(inputs);

        // 2. On founding day (Day 0), cycle service applies x10 founding day multiplier
        var snapshotDay0 = cycleService.ExecuteCycle(country, foundingTime);

        // Expected revenue is exactly baseYield * 10
        Assert.Equal(baseYield.RepuTreasuryRevenue * 10.0, snapshotDay0.Yield.RepuTreasuryRevenue);

        // 3. Evaluate founding buffs on country at founding day
        var buffs = country.EvaluateFoundingBuffs(foundingTime, _evaluator);
        Assert.Equal(1.0, buffs.AdministrativeEfficiency);

        // 4. Executing another cycle does not double-multiply by 10 or change the x10 rule
        // (FoundingDayYieldRule multiplies by 10 on founding day, not by any extra buff factor)
        var secondSnapshot = cycleService.ExecuteCycle(country, foundingTime);
        Assert.Equal(baseYield.RepuTreasuryRevenue * 10.0, secondSnapshot.Yield.RepuTreasuryRevenue);

        // 5. On Day 10 (still in founding buff window, but past founding day):
        var day10 = foundingTime + TimeSpan.FromDays(10);
        var snapshotDay10 = cycleService.ExecuteCycle(country, day10);

        // Base multiplier applies (1.0), not 10.0
        Assert.Equal(baseYield.RepuTreasuryRevenue, snapshotDay10.Yield.RepuTreasuryRevenue);
    }

    [Fact]
    public void FoundingBuffs_BeforeFoundingOrUnfounded_ReturnsZero()
    {
        var foundingTime = _clock.CurrentTime + TimeSpan.FromDays(5);
        var country = CreateTestCountry(foundingTime: foundingTime);

        // Evaluated before founding time
        var snapshotBefore = _evaluator.Evaluate(country, _clock.CurrentTime);
        Assert.Equal(0.0, snapshotBefore.AdministrativeEfficiency);

        // Unfounded country
        var unfoundedCountry = new Country { Name = "Unfounded Land" };
        var snapshotUnfounded = _evaluator.Evaluate(unfoundedCountry, _clock.CurrentTime);
        Assert.Equal(0.0, snapshotUnfounded.AdministrativeEfficiency);
    }

    [Fact]
    public void FoundingBuffs_ToModifiers_ProducesExplicitNamedModifiers()
    {
        var foundingTime = _clock.CurrentTime;
        var snapshot = _evaluator.Evaluate(foundingTime, foundingTime);

        var modifiers = snapshot.ToModifiers("test-country");

        Assert.Equal(7, modifiers.Count);
        Assert.Contains(modifiers, m => m.Name == "Administrative efficiency" && m.Value == 1.0);
        Assert.Contains(modifiers, m => m.Name == "Innovation drive" && m.Value == 1.0);
        Assert.Contains(modifiers, m => m.Name == "Development initiative" && m.Value == 1.0);
        Assert.Contains(modifiers, m => m.Name == "Investor confidence" && m.Value == 1.0);
        Assert.Contains(modifiers, m => m.Name == "Diplomatic recognition momentum" && m.Value == 1.0);
        Assert.Contains(modifiers, m => m.Name == "Institution-building" && m.Value == 1.0);
        Assert.Contains(modifiers, m => m.Name == "Temporary national unity" && m.Value == 1.0);
    }
}
