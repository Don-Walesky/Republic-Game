namespace Republic.Core.NationalYield;

using System.Collections.Concurrent;
using Republic.Core.Economy.Treasury;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Service implementation for National Yield cycle execution.
/// Acts as the execution mechanism: at a given simulation time, derives calculation inputs from the country's
/// current in-memory state, delegates pure evaluation to <see cref="INationalYieldCalculator"/>, applies the
/// authoritative <see cref="IFoundingDayYieldRule"/> to scale yield by x10 on the country's founding day,
/// applies the resulting REPU Treasury Revenue to the sovereign <see cref="RepuTreasury"/> with duplicate protection,
/// stores an independent snapshot in-memory, and returns a defensive copy to protect snapshot integrity.
/// The recorded simulation timestamp identifies the time of calculation; it does not reconstruct historical state.
/// </summary>
public sealed class NationalYieldCycleService : INationalYieldCycleService
{
    private readonly INationalYieldCalculator _calculator;
    private readonly IFoundingDayYieldRule _foundingDayYieldRule;
    private readonly ITreasuryRevenueService _treasuryRevenueService;
    private readonly INationalYieldSchedule _schedule;
    private readonly ConcurrentDictionary<string, NationalYieldSnapshot> _latestSnapshots = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="NationalYieldCycleService"/> class.
    /// </summary>
    /// <param name="calculator">The pure National Yield calculator to reuse. If null, a default <see cref="NationalYieldCalculator"/> is instantiated.</param>
    /// <param name="foundingDayYieldRule">The founding-day yield rule to apply. If null, a default <see cref="FoundingDayYieldRule"/> is instantiated.</param>
    /// <param name="treasuryRevenueService">The treasury revenue service to apply cycle revenue. If null, a default <see cref="TreasuryRevenueService"/> is instantiated.</param>
    /// <param name="schedule">The cycle schedule engine to use. If null, a default <see cref="NationalYieldSchedule"/> is instantiated.</param>
    public NationalYieldCycleService(
        INationalYieldCalculator? calculator = null,
        IFoundingDayYieldRule? foundingDayYieldRule = null,
        ITreasuryRevenueService? treasuryRevenueService = null,
        INationalYieldSchedule? schedule = null)
    {
        _calculator = calculator ?? new NationalYieldCalculator();
        _foundingDayYieldRule = foundingDayYieldRule ?? new FoundingDayYieldRule();
        _treasuryRevenueService = treasuryRevenueService ?? new TreasuryRevenueService();
        _schedule = schedule ?? new NationalYieldSchedule();
    }

    /// <inheritdoc />
    public NationalYieldSnapshot ExecuteCycle(Country country, RepublicTime simulationTime)
    {
        ArgumentNullException.ThrowIfNull(country);

        // 1. Obtain country's current in-memory calculation inputs via existing domain abstraction
        // Evaluates current in-memory state; does not reconstruct historical state based on simulationTime.
        var inputs = NationalYieldCalculationInputs.FromCountry(country);

        // 2. Calculate normal National Yield using the reused calculator (zero duplicate math)
        var normalYield = _calculator.Calculate(inputs);

        // 3. Apply the existing founding-day yield rule (single source of truth for x10 decision)
        var finalYield = _foundingDayYieldRule.Apply(country, normalYield, simulationTime);

        // 4. Construct an independent yield snapshot representing the country's final yield at the recorded simulation time
        var storedSnapshot = new NationalYieldSnapshot(country.Id, simulationTime, finalYield);

        // 5. Apply the REPU Treasury Revenue from the final yield snapshot to the country's existing REPU Treasury,
        // scaled by sovereign State Capacity at credit time with deterministic duplicate protection.
        _treasuryRevenueService.ApplyRevenue(country, storedSnapshot);

        // 6. Store the independent snapshot isolated by country ID
        _latestSnapshots[country.Id] = storedSnapshot;

        // 7. Return an independent defensive copy so callers cannot mutate the service's stored cache
        return storedSnapshot.Clone();
    }

    /// <inheritdoc />
    public NationalYieldSnapshot ExecuteCycle(Country country, IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return ExecuteCycle(country, clock.CurrentTime);
    }

    /// <inheritdoc />
    public NationalYieldSnapshot? GetLatestSnapshot(string countryId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryId);
        return _latestSnapshots.TryGetValue(countryId, out var snapshot) ? snapshot.Clone() : null;
    }

    /// <inheritdoc />
    public INationalYieldSchedule Schedule => _schedule;

    /// <inheritdoc />
    public IReadOnlyList<NationalYieldSnapshot> ExecuteDueCycles(Country country, RepublicTime simulationTime)
    {
        ArgumentNullException.ThrowIfNull(country);

        // 1. Calculate due boundaries up to simulation time, capped at 8
        var dueBoundaries = _schedule.GetDueBoundaries(country, simulationTime, NationalYieldSchedule.MaxBoundaryCap);
        if (dueBoundaries.Count == 0)
        {
            return Array.Empty<NationalYieldSnapshot>();
        }

        var results = new List<NationalYieldSnapshot>(dueBoundaries.Count);
        foreach (var boundary in dueBoundaries)
        {
            // ExecuteCycle evaluates the country's current in-memory state,
            // applies founding-day rule, constructs snapshot with deterministic boundary ID,
            // deposits revenue once to treasury, caches snapshot, and returns defensive copy.
            var snapshot = ExecuteCycle(country, boundary);

            // Update the country's authoritative last credited boundary
            country.LastCreditedBoundary = boundary;

            results.Add(snapshot);
        }

        return results;
    }

    /// <inheritdoc />
    public IReadOnlyList<NationalYieldSnapshot> ExecuteDueCycles(Country country, IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return ExecuteDueCycles(country, clock.CurrentTime);
    }

    /// <inheritdoc />
    public IReadOnlyList<RepublicTime> GetDueBoundaries(Country country, RepublicTime simulationTime)
    {
        ArgumentNullException.ThrowIfNull(country);
        return _schedule.GetDueBoundaries(country, simulationTime, NationalYieldSchedule.MaxBoundaryCap);
    }

    /// <inheritdoc />
    public IReadOnlyList<RepublicTime> GetDueBoundaries(Country country, IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return GetDueBoundaries(country, clock.CurrentTime);
    }

    /// <inheritdoc />
    public void ClearSnapshots()
    {
        _latestSnapshots.Clear();
    }
}
