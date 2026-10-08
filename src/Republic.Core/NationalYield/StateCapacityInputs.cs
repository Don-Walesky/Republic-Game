namespace Republic.Core.NationalYield;

/// <summary>
/// Domain model encapsulating the seven institutional and governance inputs determining sovereign State Capacity.
/// Each input is a rated factor from 0.0 to 1.0.
/// Missing inputs evaluate to 0.0.
/// Defaults to 0.6 across all seven inputs so existing countries are not suddenly at zero.
/// </summary>
public sealed class StateCapacityInputs
{
    /// <summary>
    /// Default baseline factor (0.6) for established countries.
    /// </summary>
    public const double DefaultTestFactor = 0.6;

    private double? _governmentEffectiveness = DefaultTestFactor;
    private double? _administrativeCompetence = DefaultTestFactor;
    private double? _institutionQuality = DefaultTestFactor;
    private double? _corruptionControl = DefaultTestFactor;
    private double? _stability = DefaultTestFactor;
    private double? _infrastructureCondition = DefaultTestFactor;
    private double? _civilServiceQuality = DefaultTestFactor;

    /// <summary>
    /// Gets or sets Government Effectiveness factor (0.0 to 1.0). Null represents a missing input.
    /// </summary>
    public double? GovernmentEffectiveness
    {
        get => _governmentEffectiveness;
        set => _governmentEffectiveness = value.HasValue ? Math.Clamp(value.Value, 0.0, 1.0) : null;
    }

    /// <summary>
    /// Gets or sets Administrative Competence factor (0.0 to 1.0). Null represents a missing input.
    /// </summary>
    public double? AdministrativeCompetence
    {
        get => _administrativeCompetence;
        set => _administrativeCompetence = value.HasValue ? Math.Clamp(value.Value, 0.0, 1.0) : null;
    }

    /// <summary>
    /// Gets or sets Institution Quality factor (0.0 to 1.0). Null represents a missing input.
    /// </summary>
    public double? InstitutionQuality
    {
        get => _institutionQuality;
        set => _institutionQuality = value.HasValue ? Math.Clamp(value.Value, 0.0, 1.0) : null;
    }

    /// <summary>
    /// Gets or sets Corruption Control factor (0.0 to 1.0, where 1.0 is clean and 0.0 is fully corrupt). Null represents a missing input.
    /// </summary>
    public double? CorruptionControl
    {
        get => _corruptionControl;
        set => _corruptionControl = value.HasValue ? Math.Clamp(value.Value, 0.0, 1.0) : null;
    }

    /// <summary>
    /// Gets or sets Stability factor (0.0 to 1.0). Null represents a missing input.
    /// </summary>
    public double? Stability
    {
        get => _stability;
        set => _stability = value.HasValue ? Math.Clamp(value.Value, 0.0, 1.0) : null;
    }

    /// <summary>
    /// Gets or sets Infrastructure Condition factor (0.0 to 1.0). Null represents a missing input.
    /// </summary>
    public double? InfrastructureCondition
    {
        get => _infrastructureCondition;
        set => _infrastructureCondition = value.HasValue ? Math.Clamp(value.Value, 0.0, 1.0) : null;
    }

    /// <summary>
    /// Gets or sets Civil Service Quality factor (0.0 to 1.0). Null represents a missing input.
    /// </summary>
    public double? CivilServiceQuality
    {
        get => _civilServiceQuality;
        set => _civilServiceQuality = value.HasValue ? Math.Clamp(value.Value, 0.0, 1.0) : null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StateCapacityInputs"/> class with test defaults (0.6 each).
    /// </summary>
    public StateCapacityInputs()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StateCapacityInputs"/> class with uniform value across all seven factors.
    /// </summary>
    public StateCapacityInputs(double uniformValue)
    {
        var clamped = Math.Clamp(uniformValue, 0.0, 1.0);
        GovernmentEffectiveness = clamped;
        AdministrativeCompetence = clamped;
        InstitutionQuality = clamped;
        CorruptionControl = clamped;
        Stability = clamped;
        InfrastructureCondition = clamped;
        CivilServiceQuality = clamped;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StateCapacityInputs"/> class with explicit factors.
    /// </summary>
    public StateCapacityInputs(
        double? governmentEffectiveness,
        double? administrativeCompetence,
        double? institutionQuality,
        double? corruptionControl,
        double? stability,
        double? infrastructureCondition,
        double? civilServiceQuality)
    {
        GovernmentEffectiveness = governmentEffectiveness;
        AdministrativeCompetence = administrativeCompetence;
        InstitutionQuality = institutionQuality;
        CorruptionControl = corruptionControl;
        Stability = stability;
        InfrastructureCondition = infrastructureCondition;
        CivilServiceQuality = civilServiceQuality;
    }

    /// <summary>
    /// Factory creating inputs where all seven factors are initialized to the specified value.
    /// </summary>
    public static StateCapacityInputs All(double value) => new(value);

    /// <summary>
    /// Creates a deep copy of the inputs.
    /// </summary>
    public StateCapacityInputs Clone() => new()
    {
        GovernmentEffectiveness = GovernmentEffectiveness,
        AdministrativeCompetence = AdministrativeCompetence,
        InstitutionQuality = InstitutionQuality,
        CorruptionControl = CorruptionControl,
        Stability = Stability,
        InfrastructureCondition = InfrastructureCondition,
        CivilServiceQuality = CivilServiceQuality
    };
}
