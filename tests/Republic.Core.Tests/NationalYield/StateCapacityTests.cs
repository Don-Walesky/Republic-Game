namespace Republic.Core.Tests.NationalYield;

using Republic.Core.Economy.Treasury;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class StateCapacityTests
{
    private readonly RepublicClock _clock;
    private readonly NationalYieldCycleService _cycleService;
    private readonly TreasuryRevenueService _revenueService;
    private readonly StateCapacityEvaluator _evaluator;

    public StateCapacityTests()
    {
        _clock = RepublicClock.CreateControlled();
        _revenueService = new TreasuryRevenueService();
        _cycleService = new NationalYieldCycleService(treasuryRevenueService: _revenueService);
        _evaluator = new StateCapacityEvaluator();
    }

    private Country CreateCountryWithCapacity(
        string name,
        StateCapacityInputs inputs,
        double stability = 75.0,
        double industry = 60.0,
        RepublicTime? foundingTime = null)
    {
        var yield = new Republic.Core.NationalYield.NationalYield
        {
            IndustrialCapacity = industry,
            InfrastructureCapacity = 60.0,
            HumanCapital = 70.0,
            AdministrativeCapacity = 70.0,
            ScienceCapacity = 65.0,
            SecurityCapacity = 60.0
        };

        var time = foundingTime ?? _clock.CurrentTime;
        var country = Country.Found(
            name: name,
            foundingTime: time,
            baselineStability: stability,
            yield: yield,
            stateCapacity: inputs);

        return country;
    }

    [Fact]
    public void Test1_AllInputsOne_ProducesScoreOne_AndFullDeposit()
    {
        // Given a sovereign country with all seven State Capacity inputs at 1.0
        var inputs = StateCapacityInputs.All(1.0);
        var country = CreateCountryWithCapacity("Ideal State", inputs);

        // Score evaluates to exactly 1.0 (100%)
        var snapshot = country.EvaluateStateCapacity();
        Assert.Equal(1.0, snapshot.Score);
        Assert.Equal(1.0, country.StateCapacityScore);
        Assert.Equal("100%", snapshot.FormattedScore);
        Assert.Equal(100.0, snapshot.ScorePercentage);

        // When a cycle executes
        var normalTime = _clock.FromDayAndTime(5, new TimeOnly(12, 0));
        var yieldSnapshot = _cycleService.ExecuteCycle(country, normalTime);
        var unscaledRevenue = yieldSnapshot.Yield.RepuTreasuryRevenue;
        Assert.True(unscaledRevenue > 0.0);

        // Then full (100%) deposit is made into the treasury
        Assert.Equal(unscaledRevenue, country.Treasury.Balance);
    }

    [Fact]
    public void Test2_OneInputAtPoint25_ProducesScorePoint25_And25PercentDeposit()
    {
        // Given a sovereign country where six inputs are 1.0 and one input (administrative competence) is 0.25
        var inputs = new StateCapacityInputs
        {
            GovernmentEffectiveness = 1.0,
            AdministrativeCompetence = 0.25,
            InstitutionQuality = 1.0,
            CorruptionControl = 1.0,
            Stability = 1.0,
            InfrastructureCondition = 1.0,
            CivilServiceQuality = 1.0
        };
        var country = CreateCountryWithCapacity("Bottleneck Nation", inputs);

        // Score evaluates to 0.25 (the minimum / bottleneck)
        var snapshot = country.EvaluateStateCapacity();
        Assert.Equal(0.25, snapshot.Score);
        Assert.Equal(0.25, country.StateCapacityScore);
        Assert.Equal("25%", snapshot.FormattedScore);
        Assert.Equal("Administrative competence", snapshot.BottleneckFactor);

        // When a cycle executes
        var normalTime = _clock.FromDayAndTime(5, new TimeOnly(12, 0));
        var yieldSnapshot = _cycleService.ExecuteCycle(country, normalTime);
        var unscaledRevenue = yieldSnapshot.Yield.RepuTreasuryRevenue;
        Assert.True(unscaledRevenue > 0.0);

        // Unscaled revenue in the yield snapshot remains unmodified
        Assert.Equal(unscaledRevenue, yieldSnapshot.Yield.RepuTreasuryRevenue);

        // Exactly 25% of that cycle's RepuTreasuryRevenue is deposited into the treasury
        var expectedDeposit = unscaledRevenue * 0.25;
        Assert.Equal(expectedDeposit, country.Treasury.Balance, precision: 4);
    }

    [Fact]
    public void Test3_MissingInput_ProducesScoreZero_AndNoDeposit()
    {
        // Given a country with a missing input (CivilServiceQuality is null)
        var inputs = new StateCapacityInputs
        {
            GovernmentEffectiveness = 0.9,
            AdministrativeCompetence = 0.8,
            InstitutionQuality = 0.9,
            CorruptionControl = 0.8,
            Stability = 0.9,
            InfrastructureCondition = 0.8,
            CivilServiceQuality = null // missing input
        };
        var country = CreateCountryWithCapacity("Missing Input Nation", inputs);

        // Score evaluates to exactly 0.0
        var snapshot = country.EvaluateStateCapacity();
        Assert.Equal(0.0, snapshot.Score);
        Assert.Equal(0.0, country.StateCapacityScore);
        Assert.Equal("0%", snapshot.FormattedScore);
        Assert.Contains("Civil service quality", snapshot.BottleneckFactor);

        // When a cycle executes
        var normalTime = _clock.FromDayAndTime(5, new TimeOnly(12, 0));
        var yieldSnapshot = _cycleService.ExecuteCycle(country, normalTime);
        Assert.True(yieldSnapshot.Yield.RepuTreasuryRevenue > 0.0);

        // Zero deposit is made into the treasury
        Assert.Equal(0.0, country.Treasury.Balance);

        // But the snapshot ID was still recorded as applied (idempotency preserved)
        Assert.True(country.Treasury.HasAppliedSnapshot(yieldSnapshot.Id));
    }

    [Fact]
    public void Test4_NullInputsObject_ProducesScoreZero_AndNoDeposit()
    {
        // Given a null inputs object
        var snapshot = _evaluator.Evaluate((StateCapacityInputs?)null);
        Assert.Equal(0.0, snapshot.Score);
        Assert.Equal("Missing inputs", snapshot.BottleneckFactor);
        Assert.Equal(0.0, snapshot.GovernmentEffectiveness);
        Assert.Equal(0.0, snapshot.CorruptionControl);
    }

    [Fact]
    public void Test5_CountryIsolation_CountryBDoesNotUseCountryAScore()
    {
        // Given four sovereign nations with different capacity configurations
        var countryA = CreateCountryWithCapacity("Country A", StateCapacityInputs.All(1.0));
        var countryB = CreateCountryWithCapacity("Country B", new StateCapacityInputs { AdministrativeCompetence = 0.25 });
        var countryC = CreateCountryWithCapacity("Country C", new StateCapacityInputs()); // defaults to 0.6
        var countryD = CreateCountryWithCapacity("Country D", new StateCapacityInputs { CorruptionControl = null }); // missing

        // Assert score isolation
        Assert.Equal(1.0, countryA.StateCapacityScore);
        Assert.Equal(0.25, countryB.StateCapacityScore);
        Assert.Equal(0.6, countryC.StateCapacityScore);
        Assert.Equal(0.0, countryD.StateCapacityScore);

        // Mutating Country A does not affect Country B, C, or D
        countryA.StateCapacity.Stability = 0.1;
        Assert.Equal(0.1, countryA.StateCapacityScore);
        Assert.Equal(0.25, countryB.StateCapacityScore);
        Assert.Equal(0.6, countryC.StateCapacityScore);
        Assert.Equal(0.0, countryD.StateCapacityScore);

        // When revenue is credited to Country B, Country A's treasury is strictly untouched
        var evalTime = _clock.FromDayAndTime(3, new TimeOnly(6, 0));
        var snapshotB = _cycleService.ExecuteCycle(countryB, evalTime);

        Assert.Equal(snapshotB.Yield.RepuTreasuryRevenue * 0.25, countryB.Treasury.Balance, precision: 4);
        Assert.Equal(0.0, countryA.Treasury.Balance);
        Assert.Equal(0.0, countryC.Treasury.Balance);
    }

    [Fact]
    public void Test6_CreditingSameSnapshotTwice_StillDepositsOnce()
    {
        // Given a country with State Capacity 0.5
        var country = CreateCountryWithCapacity("Half Capacity", StateCapacityInputs.All(0.5));
        var evalTime = _clock.FromDayAndTime(2, new TimeOnly(12, 0));
        var snapshot = new NationalYieldSnapshot(
            country.Id,
            evalTime,
            new Republic.Core.NationalYield.NationalYield { RepuTreasuryRevenue = 600.0 });

        // First credit deposits 50% (300.0)
        _revenueService.ApplyRevenue(country, snapshot);
        Assert.Equal(300.0, country.Treasury.Balance);

        // Second credit with identical snapshot ID deposits nothing further
        _revenueService.ApplyRevenue(country, snapshot);
        Assert.Equal(300.0, country.Treasury.Balance);

        // Snapshot identity was not altered or recreated by scaling
        Assert.True(country.Treasury.HasAppliedSnapshot(snapshot.Id));
    }

    [Fact]
    public void Test7_FoundingDayX10_ScaledOnceByStateCapacity_AfterMultiplier()
    {
        // Given a country in its founding day with State Capacity 0.5
        var foundingTime = _clock.FromDayAndTime(10, new TimeOnly(8, 0));
        var country = CreateCountryWithCapacity("Founding Nation", StateCapacityInputs.All(0.5), foundingTime: foundingTime);

        var calculator = new NationalYieldCalculator();
        var baseRevenue = calculator.Calculate(NationalYieldCalculationInputs.FromCountry(country)).RepuTreasuryRevenue;
        Assert.True(baseRevenue > 0.0);

        // When cycle executes on founding day
        var snapshot = _cycleService.ExecuteCycle(country, foundingTime);

        // Snapshot recorded yield has the founding-day x10 multiplier
        Assert.Equal(baseRevenue * 10.0, snapshot.Yield.RepuTreasuryRevenue);

        // Treasury deposit is scaled once after x10 by State Capacity (0.5)
        var expectedDeposit = (baseRevenue * 10.0) * 0.5;
        Assert.Equal(expectedDeposit, country.Treasury.Balance, precision: 4);
    }

    [Fact]
    public void Test8_DefaultInputs_ArePointSixEach_EnsuresExistingCountriesNotAtZero()
    {
        // Newly instantiated inputs default to 0.6 across all seven factors
        var inputs = new StateCapacityInputs();
        Assert.Equal(0.6, inputs.GovernmentEffectiveness);
        Assert.Equal(0.6, inputs.AdministrativeCompetence);
        Assert.Equal(0.6, inputs.InstitutionQuality);
        Assert.Equal(0.6, inputs.CorruptionControl);
        Assert.Equal(0.6, inputs.Stability);
        Assert.Equal(0.6, inputs.InfrastructureCondition);
        Assert.Equal(0.6, inputs.CivilServiceQuality);

        // Newly instantiated country defaults to 0.6 State Capacity score
        var country = new Country();
        Assert.NotNull(country.StateCapacity);
        Assert.Equal(0.6, country.StateCapacityScore);
        Assert.Equal("60%", country.EvaluateStateCapacity().FormattedScore);
    }

    [Fact]
    public void Test9_WeakestInstitutionIsBottleneck_DoesNotAverage()
    {
        // Six inputs at 1.0, one input at 0.1
        var inputs = new StateCapacityInputs
        {
            GovernmentEffectiveness = 1.0,
            AdministrativeCompetence = 1.0,
            InstitutionQuality = 1.0,
            CorruptionControl = 0.1, // weakest institution
            Stability = 1.0,
            InfrastructureCondition = 1.0,
            CivilServiceQuality = 1.0
        };

        var snapshot = _evaluator.Evaluate(inputs);

        // Average would be (6.1 / 7) ~= 0.871
        // Bottleneck minimum is strictly 0.1
        Assert.Equal(0.1, snapshot.Score);
        Assert.NotEqual(6.1 / 7.0, snapshot.Score);
        Assert.Equal("Corruption control", snapshot.BottleneckFactor);
    }

    [Fact]
    public void Test10_Clamping_InputsOutsideZeroToOneAreClamped()
    {
        var inputs = new StateCapacityInputs
        {
            GovernmentEffectiveness = 1.5,
            Stability = -0.5
        };

        // Clamping on setters
        Assert.Equal(1.0, inputs.GovernmentEffectiveness);
        Assert.Equal(0.0, inputs.Stability);

        var snapshot = _evaluator.Evaluate(inputs);
        Assert.Equal(0.0, snapshot.Score);
        Assert.Equal(1.0, snapshot.GovernmentEffectiveness);
        Assert.Equal(0.0, snapshot.Stability);
    }
}
