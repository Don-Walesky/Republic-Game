namespace Republic.Core.NationalYield;

/// <summary>
/// Immutable value representation of sovereign State Capacity.
/// Represents the conversion rate (0.0 to 1.0) from national conditions and institutional capacities into realized results.
/// Determined strictly as the minimum of the seven institutional inputs (bottleneck principle).
/// </summary>
public sealed class StateCapacitySnapshot
{
    /// <summary>
    /// Gets the unique identifier of the sovereign country this snapshot belongs to.
    /// </summary>
    public string CountryId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the evaluated sovereign State Capacity score (0.0 to 1.0).
    /// Represents the conversion rate applied at REPU revenue credit time.
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// Gets the Government Effectiveness factor (0.0 to 1.0).
    /// </summary>
    public double GovernmentEffectiveness { get; init; }

    /// <summary>
    /// Gets the Administrative Competence factor (0.0 to 1.0).
    /// </summary>
    public double AdministrativeCompetence { get; init; }

    /// <summary>
    /// Gets the Institution Quality factor (0.0 to 1.0).
    /// </summary>
    public double InstitutionQuality { get; init; }

    /// <summary>
    /// Gets the Corruption Control factor (0.0 to 1.0, where 1.0 is clean and 0.0 is fully corrupt).
    /// </summary>
    public double CorruptionControl { get; init; }

    /// <summary>
    /// Gets the Stability factor (0.0 to 1.0).
    /// </summary>
    public double Stability { get; init; }

    /// <summary>
    /// Gets the Infrastructure Condition factor (0.0 to 1.0).
    /// </summary>
    public double InfrastructureCondition { get; init; }

    /// <summary>
    /// Gets the Civil Service Quality factor (0.0 to 1.0).
    /// </summary>
    public double CivilServiceQuality { get; init; }

    /// <summary>
    /// Gets the name of the limiting institutional factor determining the bottleneck score.
    /// </summary>
    public string BottleneckFactor { get; init; } = string.Empty;

    /// <summary>
    /// Gets the State Capacity score formatted as a whole-number percentage (e.g. "60%", "100%", "25%").
    /// </summary>
    public string FormattedScore => $"{Score:P0}";

    /// <summary>
    /// Gets the State Capacity score as a percentage value (0.0 to 100.0).
    /// </summary>
    public double ScorePercentage => Score * 100.0;

    /// <summary>
    /// Initializes a new instance of the <see cref="StateCapacitySnapshot"/> class with default zeros.
    /// </summary>
    public StateCapacitySnapshot()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StateCapacitySnapshot"/> class with explicit factors.
    /// </summary>
    public StateCapacitySnapshot(
        double score,
        double governmentEffectiveness,
        double administrativeCompetence,
        double institutionQuality,
        double corruptionControl,
        double stability,
        double infrastructureCondition,
        double civilServiceQuality,
        string bottleneckFactor = "",
        string countryId = "")
    {
        Score = Math.Clamp(score, 0.0, 1.0);
        GovernmentEffectiveness = Math.Clamp(governmentEffectiveness, 0.0, 1.0);
        AdministrativeCompetence = Math.Clamp(administrativeCompetence, 0.0, 1.0);
        InstitutionQuality = Math.Clamp(institutionQuality, 0.0, 1.0);
        CorruptionControl = Math.Clamp(corruptionControl, 0.0, 1.0);
        Stability = Math.Clamp(stability, 0.0, 1.0);
        InfrastructureCondition = Math.Clamp(infrastructureCondition, 0.0, 1.0);
        CivilServiceQuality = Math.Clamp(civilServiceQuality, 0.0, 1.0);
        BottleneckFactor = bottleneckFactor;
        CountryId = countryId;
    }

    /// <summary>
    /// Creates an independent defensive copy of this snapshot.
    /// </summary>
    public StateCapacitySnapshot Clone() => new(
        score: Score,
        governmentEffectiveness: GovernmentEffectiveness,
        administrativeCompetence: AdministrativeCompetence,
        institutionQuality: InstitutionQuality,
        corruptionControl: CorruptionControl,
        stability: Stability,
        infrastructureCondition: InfrastructureCondition,
        civilServiceQuality: CivilServiceQuality,
        bottleneckFactor: BottleneckFactor,
        countryId: CountryId);
}
