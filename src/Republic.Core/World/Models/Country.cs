namespace Republic.Core.World.Models;

using System.Text.Json.Serialization;
using Republic.Core.Cabinet.Models;
using Republic.Core.Economy.Projects;
using Republic.Core.Economy.Treasury;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World.Rules;

/// <summary>
/// Domain model representing a sovereign nation entity.
/// Holds the authoritative national identity, constitution, and historical founding moment.
/// </summary>
public sealed class Country
{
    private RepublicTime _foundingTime;
    private bool _isFoundingSealed;
    private CountryProfile? _profile;

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
    /// Gets or sets the sovereign public approval / happiness rating percentage (0.0 to 100.0).
    /// Defaults to the standard national baseline (68.5).
    /// </summary>
    public double HappinessRating { get; set; } = 68.5;

    /// <summary>
    /// Alias for <see cref="HappinessRating"/> representing public approval sentiment.
    /// </summary>
    [JsonIgnore]
    public double ApprovalRating
    {
        get => HappinessRating;
        set => HappinessRating = value;
    }

    /// <summary>
    /// Gets or sets the sovereign gross domestic product (GDP) in REPU.
    /// Defaults to the standard national baseline (450,000,000,000.0).
    /// </summary>
    public double GrossDomesticProduct { get; set; } = 450_000_000_000.0;

    /// <summary>
    /// Alias for <see cref="GrossDomesticProduct"/>.
    /// </summary>
    [JsonIgnore]
    public double GDP
    {
        get => GrossDomesticProduct;
        set => GrossDomesticProduct = value;
    }

    /// <summary>
    /// Gets or sets the latest National Yield calculation snapshot for this sovereign country.
    /// Null if no yield cycle has been executed yet.
    /// </summary>
    public NationalYieldSnapshot? LatestYieldSnapshot { get; set; }

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
    /// Gets the list of development projects associated with this sovereign country.
    /// Each sovereign country owns an independent, isolated list.
    /// This is the authoritative collection for all development projects in Republic.
    /// </summary>
    public List<DevelopmentProject> Projects { get; init; } = new();

    /// <summary>
    /// Gets or sets the national road development project record for this country.
    /// Backward-compatibility accessor backed authoritatively by <see cref="Projects"/>.
    /// </summary>
    [JsonIgnore]
    public NationalRoadProject? RoadProject
    {
        get => Projects.OfType<NationalRoadProject>().LastOrDefault();
        set
        {
            if (value == null)
            {
                Projects.RemoveAll(p => p is NationalRoadProject);
                return;
            }

            var existingIndex = Projects.FindIndex(p => p.Id == value.Id || (p is NationalRoadProject && p.IsActive));
            if (existingIndex >= 0)
            {
                Projects[existingIndex] = value;
            }
            else
            {
                Projects.Add(value);
            }
        }
    }

    /// <summary>
    /// Deserialization fallback for legacy save payloads containing a top-level roadProject property.
    /// Never written to serialized output.
    /// </summary>
    [JsonPropertyName("roadProject")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public NationalRoadProject? LegacyRoadProject
    {
        get => null;
        set
        {
            if (value != null && !Projects.Any(p => p.Id == value.Id))
            {
                Projects.Add(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the sovereign country profile containing descriptive identity, geography, and demographics.
    /// Lazy-initialized to stable Arcadia defaults if this country is Arcadia, or default unspecified profile.
    /// </summary>
    public CountryProfile Profile
    {
        get
        {
            if (_profile == null)
            {
                if (IsArcadia(Name) || Id == "player-country")
                {
                    var arcadiaName = CountryNameRule.Default.IsValid(Name) ? Name : "Republic of Arcadia";
                    _profile = CountryProfile.CreateArcadia(
                        foundingTime: FoundingTime,
                        officialName: arcadiaName,
                        capital: string.IsNullOrWhiteSpace(CapitalCity) ? "Grand Arcadia" : CapitalCity);
                }
                else
                {
                    var officialName = CountryNameRule.Default.IsValid(Name) ? Name : CountryProfile.DefaultUnspecified;
                    _profile = new CountryProfile(
                        officialName: officialName,
                        capital: string.IsNullOrWhiteSpace(CapitalCity) ? CountryProfile.DefaultUnspecified : CapitalCity,
                        foundingTime: FoundingTime);
                }
            }

            return _profile;
        }
        set => _profile = value;
    }

    private static bool IsArcadia(string? name) =>
        !string.IsNullOrWhiteSpace(name) && name.Contains("Arcadia", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Alias for <see cref="RoadProject"/> to support generic project queries.
    /// </summary>
    [JsonIgnore]
    public NationalRoadProject? Project
    {
        get => RoadProject;
        set => RoadProject = value;
    }

    /// <summary>
    /// Gets the currently active (in-progress) road project, or null if none is active.
    /// Backed authoritatively by <see cref="Projects"/>.
    /// </summary>
    [JsonIgnore]
    public NationalRoadProject? ActiveRoadProject => Projects.OfType<NationalRoadProject>().FirstOrDefault(p => p.IsActive);

    /// <summary>
    /// Gets the currently active development project, or null if none is active.
    /// Backed authoritatively by <see cref="Projects"/>.
    /// </summary>
    [JsonIgnore]
    public DevelopmentProject? ActiveProject => Projects.FirstOrDefault(p => p.IsActive);

    /// <summary>
    /// Gets the national university development project record for this country, if any.
    /// Backed authoritatively by <see cref="Projects"/>.
    /// </summary>
    [JsonIgnore]
    public NationalUniversityProject? UniversityProject => Projects.OfType<NationalUniversityProject>().LastOrDefault();

    /// <summary>
    /// Gets the currently active (in-progress) university project, or null if none is active.
    /// Backed authoritatively by <see cref="Projects"/>.
    /// </summary>
    [JsonIgnore]
    public NationalUniversityProject? ActiveUniversityProject => Projects.OfType<NationalUniversityProject>().FirstOrDefault(p => p.IsActive);

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
    /// Recalculates and updates the sovereign State Capacity snapshot from current institutional inputs.
    /// </summary>
    public StateCapacitySnapshot RecalculateStateCapacity()
    {
        return EvaluateStateCapacity();
    }

    /// <summary>
    /// Initiates a National Road Program development project starting from the specified boundary/simulation time.
    /// Costs R500,000, withdrawn once from this country's RepuTreasury.
    /// Takes 4 six-hour boundaries (24 hours on the Republic clock) from the start boundary.
    /// Returns null if the treasury cannot pay or if another road project is currently active.
    /// The treasury balance remains unchanged on a failed start.
    /// </summary>
    public NationalRoadProject? StartRoadProject(RepublicTime startedBoundary, double cost = NationalRoadProject.StandardCost)
    {
        // 1. Must have a valid sovereign country ID
        if (string.IsNullOrWhiteSpace(Id))
        {
            return null;
        }

        // 2. Cannot start a second road while one is active
        if (Projects.OfType<NationalRoadProject>().Any(p => p.IsActive))
        {
            return null;
        }

        // 3. Cannot start if treasury cannot pay; balance must not change on failed start
        if (Treasury.Balance < cost)
        {
            return null;
        }

        // 4. Withdraw exactly R500,000 once from that country's RepuTreasury
        if (!Treasury.TryWithdraw(cost))
        {
            return null;
        }

        // 5. Takes 4 six-hour boundaries (24 hours on the Republic clock) from the start boundary
        var finishBoundary = startedBoundary.Add(NationalRoadProject.Duration);
        var project = new NationalRoadProject
        {
            Id = Guid.NewGuid().ToString("N"),
            CountryId = Id,
            StartedBoundary = startedBoundary,
            FinishBoundary = finishBoundary,
            Cost = cost,
            Completed = false
        };

        Projects.Add(project);
        return project;
    }

    /// <summary>
    /// Overload for starting a road project using an authoritative Republic clock.
    /// </summary>
    public NationalRoadProject? StartRoadProject(IRepublicClock clock, double cost = NationalRoadProject.StandardCost)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return StartRoadProject(clock.CurrentTime, cost);
    }

    /// <summary>
    /// Initiates a National Road Program, returning true on success and false on failure.
    /// </summary>
    public bool StartRoadProgram(RepublicTime startedBoundary, double cost = NationalRoadProject.StandardCost)
    {
        return StartRoadProject(startedBoundary, cost) != null;
    }

    /// <summary>
    /// Overload for starting a road program using an authoritative clock, returning true on success.
    /// </summary>
    public bool StartRoadProgram(IRepublicClock clock, double cost = NationalRoadProject.StandardCost)
    {
        return StartRoadProject(clock, cost) != null;
    }

    /// <summary>
    /// Initiates a National University development project starting from the specified boundary/simulation time.
    /// Costs R750,000, withdrawn once from this country's RepuTreasury.
    /// Takes 8 six-hour boundaries (48 hours on the Republic clock) from the start boundary.
    /// Returns null if the treasury cannot pay, country ID is invalid, or if another university project is currently active.
    /// </summary>
    public NationalUniversityProject? StartUniversityProject(RepublicTime startedBoundary, double cost = NationalUniversityProject.StandardCost)
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            return null;
        }

        var project = new NationalUniversityProject(Id, startedBoundary, cost);
        return StartProject(project) ? project : null;
    }

    /// <summary>
    /// Overload for starting a national university project using an authoritative Republic clock.
    /// </summary>
    public NationalUniversityProject? StartUniversityProject(IRepublicClock clock, double cost = NationalUniversityProject.StandardCost)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return StartUniversityProject(clock.CurrentTime, cost);
    }

    /// <summary>
    /// Initiates a generic development project for this sovereign country.
    /// Withdraws project cost immediately from the country's RepuTreasury.
    /// Enforces strict country ownership and ensures no duplicate active project of the same type.
    /// </summary>
    public bool StartProject(DevelopmentProject project)
    {
        ArgumentNullException.ThrowIfNull(project);

        // Project must have a valid non-empty CountryId belonging strictly to this country
        if (string.IsNullOrWhiteSpace(project.CountryId) ||
            string.IsNullOrWhiteSpace(Id) ||
            !string.Equals(project.CountryId, Id, StringComparison.Ordinal))
        {
            return false;
        }

        // Cannot start if an active project of the same type is already in progress
        if (Projects.Any(p => p.ProjectType == project.ProjectType && p.IsActive))
        {
            return false;
        }

        if (Treasury.Balance < project.Cost)
        {
            return false;
        }

        if (!Treasury.TryWithdraw(project.Cost))
        {
            return false;
        }

        if (!Projects.Contains(project))
        {
            Projects.Add(project);
        }

        return true;
    }

    /// <summary>
    /// Completes the active National Road Program if the current simulation time has reached or passed its finish boundary.
    /// Raises this country's infrastructure condition by 0.1 (clamped at 1.0) and recalculates State Capacity.
    /// If no simulation time is provided, force-completes the active road project.
    /// Returns false if no road project is active or if current time is before the finish boundary.
    /// </summary>
    public bool CompleteRoadProject(RepublicTime? currentTime = null)
    {
        var road = ActiveRoadProject ?? RoadProject;
        if (road == null)
        {
            return false;
        }

        return road.Complete(this, currentTime);
    }

    /// <summary>
    /// Alias for <see cref="CompleteRoadProject"/>.
    /// </summary>
    public bool CompleteRoadProgram(RepublicTime? currentTime = null) => CompleteRoadProject(currentTime);

    /// <summary>
    /// Gets the construction progress (0.0 to 1.0) of the national road program, or null if no road program exists.
    /// </summary>
    public double? GetRoadProgress(RepublicTime currentTime) => RoadProject?.GetProgress(currentTime);

    /// <summary>
    /// Overload for getting national road program construction progress using an authoritative clock.
    /// </summary>
    public double? GetRoadProgress(IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return GetRoadProgress(clock.CurrentTime);
    }

    /// <summary>
    /// Completes the active National University project if the current simulation time has reached or passed its finish boundary.
    /// Elevates Human Capital and Science / Research Capacity upon completion.
    /// </summary>
    public bool CompleteUniversityProject(RepublicTime? currentTime = null)
    {
        var university = ActiveUniversityProject ?? UniversityProject;
        return university != null && CompleteProject(university, currentTime);
    }

    /// <summary>
    /// Gets the construction progress (0.0 to 1.0) of the national university project, or null if no university project exists.
    /// </summary>
    public double? GetUniversityProgress(RepublicTime currentTime) => UniversityProject?.GetProgress(currentTime);

    /// <summary>
    /// Overload for getting national university construction progress using an authoritative clock.
    /// </summary>
    public double? GetUniversityProgress(IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return GetUniversityProgress(clock.CurrentTime);
    }

    /// <summary>
    /// Gets the construction progress (0.0 to 1.0) of the currently active development project, or null if none is active.
    /// </summary>
    public double? GetActiveProjectProgress(RepublicTime currentTime) => ActiveProject?.GetProgress(currentTime);

    /// <summary>
    /// Overload for getting active project construction progress using an authoritative clock.
    /// </summary>
    public double? GetActiveProjectProgress(IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return GetActiveProjectProgress(clock.CurrentTime);
    }

    /// <summary>
    /// Checks and completes the active road project if the simulation time has reached or passed the finish boundary.
    /// </summary>
    public bool CheckRoadProjectCompletion(RepublicTime currentTime) => CompleteRoadProject(currentTime);

    /// <summary>
    /// Completes the specified development project for this country.
    /// Idempotent and enforces strict country ownership.
    /// </summary>
    public bool CompleteProject(DevelopmentProject project, RepublicTime? currentTime = null)
    {
        ArgumentNullException.ThrowIfNull(project);

        // Project must have a valid non-empty CountryId belonging strictly to this country
        if (string.IsNullOrWhiteSpace(project.CountryId) ||
            string.IsNullOrWhiteSpace(Id) ||
            !string.Equals(project.CountryId, Id, StringComparison.Ordinal))
        {
            return false;
        }

        return project.Complete(this, currentTime);
    }

    /// <summary>
    /// Checks and completes any active development projects whose finish boundary has been reached.
    /// </summary>
    public int CheckProjectsCompletion(RepublicTime currentTime)
    {
        var completed = 0;
        foreach (var project in Projects.Where(p => p.IsActive).ToList())
        {
            if (project.CanComplete(currentTime) && CompleteProject(project, currentTime))
            {
                completed++;
            }
        }

        return completed;
    }

    /// <summary>
    /// Evaluates and returns the read-only national dashboard for this sovereign country at the specified simulation time.
    /// Induces zero mutation or side-effects on country or simulation state.
    /// </summary>
    public NationalDashboard GetDashboard(RepublicTime simulationTime) => new(this, simulationTime);

    /// <summary>
    /// Evaluates and returns the read-only national dashboard for this sovereign country using an authoritative clock.
    /// </summary>
    public NationalDashboard GetDashboard(IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return GetDashboard(clock.CurrentTime);
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
        Dictionary<CabinetPortfolio, Minister?>? cabinet = null,
        NationalRoadProject? roadProject = null,
        IEnumerable<DevelopmentProject>? projects = null,
        CountryProfile? profile = null,
        double grossDomesticProduct = 450_000_000_000.0,
        double happinessRating = 68.5,
        NationalYieldSnapshot? latestYieldSnapshot = null)
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
            Cabinet = cabinet != null ? new Dictionary<CabinetPortfolio, Minister?>(cabinet) : new Dictionary<CabinetPortfolio, Minister?>(),
            GrossDomesticProduct = grossDomesticProduct,
            HappinessRating = happinessRating,
            LatestYieldSnapshot = latestYieldSnapshot
        };

        if (profile != null)
        {
            country.Profile = profile;
        }

        if (roadProject != null)
        {
            if (string.IsNullOrWhiteSpace(roadProject.CountryId))
            {
                roadProject.CountryId = countryId;
            }

            if (!country.Projects.Any(p => p.Id == roadProject.Id))
            {
                country.Projects.Add(roadProject);
            }
        }

        if (projects != null)
        {
            foreach (var p in projects)
            {
                if (!country.Projects.Any(existing => existing.Id == p.Id))
                {
                    country.Projects.Add(p);
                }
            }
        }

        if (cabinet != null)
        {
            country.RecalculateCabinetStateCapacity();
        }

        return country;
    }
}
