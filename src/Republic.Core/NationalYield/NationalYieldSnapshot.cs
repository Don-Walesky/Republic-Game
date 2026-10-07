namespace Republic.Core.NationalYield;

using Republic.Core.Time;

/// <summary>
/// Immutable snapshot representing evaluated sovereign National Yield for a specific country at an authoritative Republic simulation time.
/// </summary>
public sealed class NationalYieldSnapshot
{
    /// <summary>
    /// Gets the unique identifier of the sovereign country.
    /// </summary>
    public string CountryId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the authoritative Republic simulation time at which this yield snapshot was calculated.
    /// </summary>
    public RepublicTime SimulationTime { get; init; }

    /// <summary>
    /// Gets the evaluated National Yield across all 14 canonical categories.
    /// </summary>
    public NationalYield Yield { get; init; } = new();

    /// <summary>
    /// Implicitly converts the snapshot to its underlying <see cref="NationalYield"/> result for seamless calculation usage.
    /// </summary>
    public static implicit operator NationalYield(NationalYieldSnapshot snapshot) => snapshot.Yield;

    /// <summary>
    /// Creates an independent deep clone of the snapshot.
    /// </summary>
    public NationalYieldSnapshot Clone() => new()
    {
        CountryId = CountryId,
        SimulationTime = SimulationTime,
        Yield = Yield.Clone()
    };
}
