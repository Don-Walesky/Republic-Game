namespace Republic.Core.NationalYield;

/// <summary>
/// Domain model representing an explicitly defined national modifier that can influence yield calculation inputs.
/// Holds descriptive and contextual information (e.g. traits, directives, treaties, executive decrees).
/// Does NOT implement mathematical formulas, arbitrary multipliers, or yield calculation logic.
/// </summary>
public sealed class NationalYieldModifier
{
    /// <summary>
    /// Gets the unique identifier of the national modifier.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets the descriptive display name of the modifier.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the specific <see cref="NationalYieldCategory"/> targeted by this modifier, or null if broadly scoped.
    /// </summary>
    public NationalYieldCategory? TargetCategory { get; init; }

    /// <summary>
    /// Gets the origin or institutional source of the modifier (e.g. "NationalTrait", "PolicyDirective", "CabinetMinistry", "Treaty").
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// Gets the narrative or explanatory text describing the context of the modifier.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the magnitude or rated strength of the modifier.
    /// Note: Final calibration, mathematical weighting, and formula application belong strictly to later calculation engine steps.
    /// </summary>
    public double Value { get; init; }

    /// <summary>
    /// Creates a deep clone of the modifier.
    /// </summary>
    public NationalYieldModifier Clone() => new()
    {
        Id = Id,
        Name = Name,
        TargetCategory = TargetCategory,
        Source = Source,
        Description = Description,
        Value = Value
    };
}
