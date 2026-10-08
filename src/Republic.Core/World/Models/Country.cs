namespace Republic.Core.World.Models;

using System.Text.Json.Serialization;
using Republic.Core.Cabinet.Models;
using Republic.Core.Economy.Treasury;
using Republic.Core.NationalYield;
using Republic.Core.Time;

/// <summary>
/// Domain model representing a sovereign nation entity.
/// Holds the authoritative national identity, constitution, and historical founding moment.
/// </summary>
public sealed class Country
{
    private RepublicTime _foundingTime;
    private bool _isFoundingSealed;

    /// <summary>
    /// Gets the unique identifier of the sovereign country.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets the official name of the sovereign nation.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the capital city.
    /// </summary>
    public string CapitalCity { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the constitutional system of governance.
    /// </summary>
    public string GovernmentType { get; set; } = "Democratic Republic";

    /// <summary>
    /// Gets or sets the sovereign land area in square kilometers.
    /// </summary>
    public double TerritorySizeSqKm { get; init; } = 500000;

    /// <summary>
    /// Gets the national traits and cultural characteristics.
    /// </summary>
    public List<string> NationalTraits { get; init; } = new();

    /// <summary>
    /// Gets or sets the baseline political and societal stability (0.0 to 100.0).
    /// </summary>
    public double BaselineStability { get; set; } = 75.0;

    /// <summary>
    /// Gets the authoritative National Yield state representing the country's productive, human, institutional, and resource capacities.
    /// Each sovereign country owns an independent, isolated instance.
    /// </summary>
    public NationalYield Yield { get; init; } = new();

    /// <summary>
    /// Gets the authoritative sovereign treasury holding accumulated liquid REPU balance.
    /// Each sovereign country owns an independent, isolated instance.
    /// </summary>
    public RepuTreasury Treasury { get; init; } = new();

    /// <summary>
    /// Gets or sets the authoritative Republic simulation timestamp of the most recently credited
    /// six-hour National Yield cycle boundary for this sovereign country.
    /// Null if no cycle boundary has been credited yet.
    /// </summary>
    public RepublicTime? LastCreditedBoundary { get; set; }

    /// <summary>
    /// Gets or sets the authoritative snapshot of temporary tapering founding buffs evaluated for this sovereign country.
    /// Null if not yet evaluated.
    /// </summary>
    public FoundingBuffSnapshot? FoundingBuffs { get; set; }

    /// <summary>
    /// Gets or sets the sovereign State Capacity institutional inputs determining conversion rate into results.
    /// Defaults to 0.6 across all seven inputs so existing countries are not suddenly at zero.
    /// Each sovereign country owns an independent, isolated instance.
    /// </summary>
    public StateCapacityInputs StateCapacity { get; set; } = new();

    /// <summary>
    /// Default State Capacity factor for a vacant cabinet portfolio (0.3).
    /// </summary>
    public const double VacantPortfolioFactor = 0.3;

    /// <summary>
    /// Gets the authoritative dictionary of cabinet appointments by portfolio for this sovereign country.
    /// A portfolio mapping to null or absent indicates a vacant portfolio.
    /// Each sovereign country owns an independent, isolated instance.
    /// </summary>
    public Dictionary<CabinetPortfolio, Minister?> Cabinet { get; init; } = new();

    /// <summary>
    /// Gets the authoritative Republic simulation time at which this nation was founded.
    /// Init-only to prevent casual post-creation mutation.
    /// </summary>
    public RepublicTime FoundingTime
    {
        get => _foundingTime;
        init
        {
            _foundingTime = value;
            _isFoundingSealed = value.WatTimestamp != default;
        }
    }

    /// <summary>
    /// Gets or sets the foundational lifecycle status of the country.
    /// </summary>
    public CountryFoundingStatus FoundingStatus { get; set; } = CountryFoundingStatus.NewlyFounded;

    /// <summary>
    /// Gets a value indicating whether the founding moment has been authoritatively established.
    /// </summary>
    [JsonIgnore]
    public bool IsFounded => _isFoundingSealed || _foundingTime.WatTimestamp != default;

    /// <summary>
    /// Gets the Gregorian calendar date of national independence in West Africa Time (WAT).
    /// </summary>
    [JsonIgnore]
    public DateOnly IndependenceDay => FoundingTime.Date;

    /// <summary>
    /// Gets the time of day of national independence in West Africa Time (WAT).
    /// </summary>
    [JsonIgnore]
    public TimeOnly IndependenceTime => FoundingTime.Time;

    /// <summary>
    /// Gets the Republic simulation day number on which national independence occurred (e.g. Day 0, Day 42).
    /// </summary>
    [JsonIgnore]
    public long FoundingRepublicDay => FoundingTime.DayNumber;

    /// <summary>
    /// Gets a value indicating whether the country has transitioned from newly founded to established statehood.
    /// </summary>
    [JsonIgnore]
    public bool IsEstablished => FoundingStatus == CountryFoundingStatus.Established;

    /// <summary>
    /// Establishes the authoritative founding moment for an unsealed country.
    /// Throws <see cref="InvalidOperationException"/> if already established.
    /// </summary>
    public void EstablishFounding(RepublicTime foundingTime)
    {
        if (IsFounded)
        {
            throw new InvalidOperationException(
                $"The founding moment for country '{Name}' ({Id}) has already been authoritatively established and cannot be overwritten.");
        }

        if (foundingTime.WatTimestamp == default)
        {
            throw new ArgumentException("Cannot establish founding with an uninitialized Republic time.", nameof(foundingTime));
        }

        _foundingTime = foundingTime;
        _isFoundingSealed = true;
    }

    /// <summary>
    /// Marks the country as having achieved established statehood.
    /// </summary>
    public void MarkEstablished()
    {
        FoundingStatus = CountryFoundingStatus.Established;
    }

    /// <summary>
    /// Calculates the country's national age relative to the specified Republic simulation time.
    /// Returns <see cref="TimeSpan.Zero"/> if the simulation time is at or before founding, or if not founded yet.
    /// </summary>
    public TimeSpan GetAge(RepublicTime currentRepublicTime)
    {
        if (!IsFounded)
        {
            return TimeSpan.Zero;
        }

        var age = currentRepublicTime - FoundingTime;
        return age < TimeSpan.Zero ? TimeSpan.Zero : age;
    }

    /// <summary>
    /// Calculates the country's national age relative to the authoritative Republic clock.
    /// </summary>
    public TimeSpan GetAge(IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return GetAge(clock.CurrentTime);
    }

    /// <summary>
    /// Determines whether the specified simulation time falls within the country's founding day (Independence Day in WAT).
    /// Respects the exact WAT day boundary (simulation day number), not simply 24 elapsed hours.
    /// </summary>
    public bool IsInFoundingDay(RepublicTime simulationTime)
    {
        if (!IsFounded)
        {
            return false;
        }

        return simulationTime >= FoundingTime && simulationTime.DayNumber == FoundingRepublicDay;
    }

    /// <summary>
    /// Determines whether the country is currently within its founding day relative to the authoritative Republic clock.
    /// </summary>
    public bool IsInFoundingDay(IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return IsInFoundingDay(clock.CurrentTime);
    }

    /// <summary>
    /// Evaluates and returns the sovereign country's temporary tapering founding buffs at the specified simulation time,
    /// storing the snapshot on the country.
    /// </summary>
    public FoundingBuffSnapshot EvaluateFoundingBuffs(RepublicTime currentSimulationTime, IFoundingBuffEvaluator? evaluator = null)
    {
        evaluator ??= new FoundingBuffEvaluator();
        var snapshot = evaluator.Evaluate(this, currentSimulationTime);
        FoundingBuffs = snapshot;
        return snapshot;
    }

    /// <summary>
    /// Evaluates and returns the sovereign country's temporary tapering founding buffs at the current clock time,
    /// storing the snapshot on the country.
    /// </summary>
    public FoundingBuffSnapshot EvaluateFoundingBuffs(IRepublicClock clock, IFoundingBuffEvaluator? evaluator = null)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return EvaluateFoundingBuffs(clock.CurrentTime, evaluator);
    }

    /// <summary>
    /// Evaluates and returns the sovereign country's State Capacity snapshot from its in-memory state.
    /// Does not call real-time APIs (never calls DateTime.Now).
    /// </summary>
    public StateCapacitySnapshot EvaluateStateCapacity(IStateCapacityEvaluator? evaluator = null)
    {
        evaluator ??= new StateCapacityEvaluator();
        return evaluator.Evaluate(this);
    }

    /// <summary>
    /// Gets the authoritative sovereign State Capacity score (0.0 to 1.0) evaluated from current country inputs.
    /// Represents the conversion rate applied at REPU revenue credit time.
    /// </summary>
    [JsonIgnore]
    public double StateCapacityScore => EvaluateStateCapacity().Score;

    /// <summary>
    /// Gets the currently appointed minister for the specified portfolio, or null if the portfolio is vacant.
    /// </summary>
    public Minister? GetMinister(CabinetPortfolio portfolio)
    {
        return Cabinet.TryGetValue(portfolio, out var minister) ? minister : null;
    }

    /// <summary>
    /// Appoints a minister to the specified portfolio (or vacates if null), stores the appointment,
    /// and recalculates the three affected State Capacity inputs (Civil Service Quality, Corruption Control, Infrastructure Condition).
    /// Spends nothing in this slice.
    /// </summary>
    public Minister? AppointMinister(Minister? minister, CabinetPortfolio portfolio)
    {
        if (minister != null)
        {
            minister.Portfolio = portfolio;
            minister.IsAppointed = true;
            Cabinet[portfolio] = minister;
        }
        else
        {
            if (Cabinet.TryGetValue(portfolio, out var existing) && existing != null)
            {
                existing.IsAppointed = false;
            }
            Cabinet[portfolio] = null;
        }

        RecalculateCabinetStateCapacity();
        return minister;
    }

    /// <summary>
    /// Appoints a minister to the portfolio configured on the minister object, stores the appointment,
    /// and recalculates the three affected State Capacity inputs.
    /// </summary>
    public Minister AppointMinister(Minister minister)
    {
        ArgumentNullException.ThrowIfNull(minister);
        AppointMinister(minister, minister.Portfolio);
        return minister;
    }

    /// <summary>
    /// Dismisses the minister from the specified portfolio, marking it vacant,
    /// and recalculates the three affected State Capacity inputs.
    /// </summary>
    public bool DismissMinister(CabinetPortfolio portfolio)
    {
        AppointMinister(null, portfolio);
        return true;
    }

    /// <summary>
    /// Vacates the specified ministerial portfolio and recalculates the three affected State Capacity inputs.
    /// </summary>
    public bool VacatePortfolio(CabinetPortfolio portfolio)
    {
        return DismissMinister(portfolio);
    }

    /// <summary>
    /// Recalculates the three State Capacity inputs affected by cabinet appointments:
    /// - Finance competence and integrity set civil service quality to their minimum (or 0.3 if vacant).
    /// - Interior competence and integrity set corruption control to their minimum (or 0.3 if vacant).
    /// - Infrastructure competence and experience set infrastructure condition to their minimum (or 0.3 if vacant).
    /// Loyalty and political connections do not raise capacity.
    /// </summary>
    public void RecalculateCabinetStateCapacity()
    {
        // 1. Finance -> Civil Service Quality
        var finance = GetMinister(CabinetPortfolio.Finance);
        StateCapacity.CivilServiceQuality = finance != null
            ? Math.Min(finance.Competence, finance.Integrity)
            : VacantPortfolioFactor;

        // 2. Interior -> Corruption Control
        var interior = GetMinister(CabinetPortfolio.Interior);
        StateCapacity.CorruptionControl = interior != null
            ? Math.Min(interior.Competence, interior.Integrity)
            : VacantPortfolioFactor;

        // 3. Infrastructure -> Infrastructure Condition
        var infra = GetMinister(CabinetPortfolio.Infrastructure);
        StateCapacity.InfrastructureCondition = infra != null
            ? Math.Min(infra.Competence, infra.Experience)
            : VacantPortfolioFactor;
    }

    /// <summary>
    /// Factory method to create and found a new sovereign country with an authoritative founding moment.
    /// </summary>
    public static Country Found(
        string name,
        RepublicTime foundingTime,
        string id = "",
        string capitalCity = "",
        string governmentType = "Democratic Republic",
        double territorySizeSqKm = 500000,
        double baselineStability = 75.0,
        NationalYield? yield = null,
        RepuTreasury? treasury = null,
        RepublicTime? lastCreditedBoundary = null,
        FoundingBuffSnapshot? foundingBuffs = null,
        StateCapacityInputs? stateCapacity = null,
        Dictionary<CabinetPortfolio, Minister?>? cabinet = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var countryId = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id;
        var country = new Country
        {
            Id = countryId,
            Name = name,
            FoundingTime = foundingTime,
            CapitalCity = capitalCity,
            GovernmentType = governmentType,
            TerritorySizeSqKm = territorySizeSqKm,
            BaselineStability = baselineStability,
            FoundingStatus = CountryFoundingStatus.NewlyFounded,
            Yield = yield?.Clone() ?? new NationalYield(),
            Treasury = treasury?.Clone() ?? new RepuTreasury(0.0, countryId),
            LastCreditedBoundary = lastCreditedBoundary,
            FoundingBuffs = foundingBuffs,
            StateCapacity = stateCapacity?.Clone() ?? new StateCapacityInputs(),
            Cabinet = cabinet != null ? new Dictionary<CabinetPortfolio, Minister?>(cabinet) : new Dictionary<CabinetPortfolio, Minister?>()
        };

        if (cabinet != null)
        {
            country.RecalculateCabinetStateCapacity();
        }

        return country;
    }
}
