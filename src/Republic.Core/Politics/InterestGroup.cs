namespace Republic.Core.Politics;

using System;

/// <summary>
/// Specifies the societal, economic, or occupational focus of an interest group.
/// </summary>
public enum InterestGroupFocus
{
    Labor,
    Business,
    Farmers,
    Students,
    CivilService
}

/// <summary>
/// Domain model representing a sovereign interest group founded in a country.
/// Holds the group name, country ownership, societal focus, and public influence rating.
/// </summary>
public sealed record InterestGroup
{
    private readonly string _name = string.Empty;
    private readonly double _influence = InitialInfluence;

    /// <summary>
    /// Initial baseline public influence rating for a newly founded interest group.
    /// </summary>
    public const double InitialInfluence = 15.0;

    /// <summary>
    /// Gets the official name of the interest group (trimmed, not empty).
    /// </summary>
    public string Name
    {
        get => _name;
        init => _name = value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Gets the identifier of the sovereign country where the interest group was founded.
    /// </summary>
    public string CountryId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the societal focus of the interest group.
    /// </summary>
    public InterestGroupFocus Focus { get; init; } = InterestGroupFocus.Labor;

    /// <summary>
    /// Gets the public influence rating (0.0 to 100.0), starting at 15.
    /// </summary>
    public double Influence
    {
        get => _influence;
        init => _influence = Math.Clamp(value, 0.0, 100.0);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InterestGroup"/> record with default values.
    /// </summary>
    public InterestGroup() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InterestGroup"/> record with validated parameters.
    /// </summary>
    /// <param name="name">The official interest group name (trimmed, not empty).</param>
    /// <param name="countryId">The owning country identifier.</param>
    /// <param name="focus">The societal/economic focus of the interest group.</param>
    /// <param name="influence">Public influence rating between 0 and 100 (defaults to 15.0).</param>
    public InterestGroup(
        string name,
        string countryId,
        InterestGroupFocus focus,
        double influence = InitialInfluence)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (influence < 0.0 || influence > 100.0)
        {
            throw new ArgumentOutOfRangeException(nameof(influence), "Influence must be between 0.0 and 100.0.");
        }

        Name = name.Trim();
        CountryId = countryId ?? string.Empty;
        Focus = focus;
        Influence = influence;
    }

    /// <summary>
    /// Deconstructs the interest group into its constituent components.
    /// </summary>
    public void Deconstruct(out string name, out string countryId, out InterestGroupFocus focus, out double influence)
    {
        name = Name;
        countryId = CountryId;
        focus = Focus;
        influence = Influence;
    }

    /// <summary>
    /// Deconstructs the interest group into its 3-part representation without country id.
    /// </summary>
    public void Deconstruct(out string name, out InterestGroupFocus focus, out double influence)
    {
        name = Name;
        focus = Focus;
        influence = Influence;
    }

    /// <summary>
    /// Returns a formatted representation of the interest group name, focus, and influence.
    /// </summary>
    public override string ToString() => $"{Name} ({Focus}, Influence: {Influence:0.##}%)";
}
