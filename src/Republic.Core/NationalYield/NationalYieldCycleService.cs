namespace Republic.Core.NationalYield;

using System.Collections.Concurrent;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Service implementation for National Yield cycle execution.
/// Acts as the execution mechanism: at a given simulation time, derives calculation inputs from the country,
/// delegates pure evaluation to <see cref="INationalYieldCalculator"/>, stores the latest snapshot in-memory,
/// and returns the resulting <see cref="NationalYieldSnapshot"/>.
/// </summary>
public sealed class NationalYieldCycleService : INationalYieldCycleService
{
    private readonly INationalYieldCalculator _calculator;
    private readonly ConcurrentDictionary<string, NationalYieldSnapshot> _latestSnapshots = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="NationalYieldCycleService"/> class.
    /// </summary>
    /// <param name="calculator">The pure National Yield calculator to reuse. If null, a default <see cref="NationalYieldCalculator"/> is instantiated.</param>
    public NationalYieldCycleService(INationalYieldCalculator? calculator = null)
    {
        _calculator = calculator ?? new NationalYieldCalculator();
    }

    /// <inheritdoc />
    public NationalYieldSnapshot ExecuteCycle(Country country, RepublicTime simulationTime)
    {
        ArgumentNullException.ThrowIfNull(country);

        // 1. Obtain country's current calculation inputs via existing domain abstraction
        var inputs = NationalYieldCalculationInputs.FromCountry(country);

        // 2. Calculate National Yield using the reused calculator (zero duplicate math)
        var yieldResult = _calculator.Calculate(inputs);

        // 3. Construct the immutable yield snapshot representing the country's state at the requested simulation time
        var snapshot = new NationalYieldSnapshot
        {
            CountryId = country.Id,
            SimulationTime = simulationTime,
            Yield = yieldResult
        };

        // 4. Store the resulting snapshot isolated by country ID
        _latestSnapshots[country.Id] = snapshot;

        return snapshot;
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
        return _latestSnapshots.TryGetValue(countryId, out var snapshot) ? snapshot : null;
    }

    /// <inheritdoc />
    public void ClearSnapshots()
    {
        _latestSnapshots.Clear();
    }
}
