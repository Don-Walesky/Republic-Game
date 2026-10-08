namespace Republic.Core.NationalYield;

using Republic.Core.Time;

/// <summary>
/// Snapshot representing evaluated sovereign National Yield for a specific country at an authoritative Republic simulation time.
/// Protects snapshot integrity by maintaining an independent copy of the evaluated <see cref="NationalYield"/>.
/// The <see cref="SimulationTime"/> records the simulation timestamp of the calculation and does not reconstruct historical state.
/// </summary>
public sealed class NationalYieldSnapshot
{
    private readonly NationalYield _yield;
    private string _id = string.Empty;

    /// <summary>
    /// Gets the unique identifier of this yield snapshot / completed cycle.
    /// Defaults to a deterministic identifier based on <see cref="CountryId"/> and <see cref="SimulationTime"/>.
    /// </summary>
    public string Id
    {
        get => string.IsNullOrEmpty(_id) ? GenerateSnapshotId(CountryId, SimulationTime) : _id;
        init => _id = value;
    }

    /// <summary>
    /// Gets the unique identifier of this yield snapshot (alias for <see cref="Id"/>).
    /// </summary>
    public string SnapshotId => Id;

    /// <summary>
    /// Gets the unique identifier of the sovereign country.
    /// </summary>
    public string CountryId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the authoritative Republic simulation timestamp recorded for this calculation.
    /// Note: The calculation reflects the country's in-memory state at execution time;
    /// it does not reconstruct historical country state.
    /// </summary>
    public RepublicTime SimulationTime { get; init; }

    /// <summary>
    /// Gets the evaluated National Yield across all 14 canonical categories.
    /// Preserves snapshot integrity via defensive copying.
    /// </summary>
    public NationalYield Yield
    {
        get => _yield;
        init => _yield = value?.Clone() ?? new NationalYield();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NationalYieldSnapshot"/> class.
    /// </summary>
    public NationalYieldSnapshot()
    {
        _yield = new NationalYield();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NationalYieldSnapshot"/> class with explicit values.
    /// Defensively copies the supplied <paramref name="yield"/> to protect snapshot integrity.
    /// </summary>
    /// <param name="countryId">The unique identifier of the sovereign country.</param>
    /// <param name="simulationTime">The simulation timestamp recorded for the calculation.</param>
    /// <param name="yield">The evaluated National Yield result.</param>
    /// <param name="id">Optional unique snapshot identifier. If null or empty, a deterministic cycle identifier is generated.</param>
    public NationalYieldSnapshot(string countryId, RepublicTime simulationTime, NationalYield yield, string? id = null)
    {
        CountryId = countryId ?? string.Empty;
        SimulationTime = simulationTime;
        _id = id ?? GenerateSnapshotId(CountryId, simulationTime);
        _yield = yield?.Clone() ?? new NationalYield();
    }

    /// <summary>
    /// Generates a deterministic cycle snapshot identifier for a sovereign country at a simulation timestamp.
    /// </summary>
    public static string GenerateSnapshotId(string countryId, RepublicTime simulationTime)
    {
        if (string.IsNullOrWhiteSpace(countryId))
        {
            return $"snapshot_{Guid.NewGuid():N}";
        }

        return $"{countryId}:Day{simulationTime.DayNumber}:{simulationTime.Time:HHmmss.fffffff}";
    }

    /// <summary>
    /// Implicitly converts the snapshot to its underlying <see cref="NationalYield"/> result for seamless calculation usage.
    /// </summary>
    public static implicit operator NationalYield(NationalYieldSnapshot snapshot) => snapshot.Yield;

    /// <summary>
    /// Creates an independent deep clone of the snapshot, ensuring all 14 categories and identity are defensively preserved.
    /// </summary>
    public NationalYieldSnapshot Clone() => new()
    {
        Id = Id,
        CountryId = CountryId,
        SimulationTime = SimulationTime,
        Yield = _yield.Clone()
    };
}
