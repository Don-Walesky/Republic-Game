namespace Republic.Core.World.Models;

using Republic.Core.NationalYield;

/// <summary>
/// Value object representing comparable national strength metrics across six institutional,
/// economic, and strategic dimensions, plus a composite total score.
/// All scores are normalized between 0.0 and 1.0.
/// Strictly read-only and pure; calculating or reading strength does not mutate country or simulation state.
/// </summary>
public sealed record NationalStrength
{
    /// <summary>
    /// Gets the unique identifier of the sovereign country.
    /// </summary>
    public string CountryId { get; init; }

    /// <summary>
    /// Gets the name of the sovereign nation.
    /// </summary>
    public string CountryName { get; init; }

    /// <summary>
    /// Gets the economic strength score (0.0 to 1.0).
    /// Treasury balance divided by R1,000,000,000, clamped at 1. R0 is 0. R1B or more is 1.
    /// </summary>
    public double Economic { get; init; }

    /// <summary>
    /// Gets the institutional strength score (0.0 to 1.0).
    /// Backed by the country's evaluated State Capacity score.
    /// </summary>
    public double Institutional { get; init; }

    /// <summary>
    /// Gets the infrastructure strength score (0.0 to 1.0).
    /// Backed by the country's State Capacity infrastructure condition input.
    /// </summary>
    public double Infrastructure { get; init; }

    /// <summary>
    /// Gets the human capital strength score (0.0 to 1.0).
    /// Uses the institution-building founding buff if above 0, otherwise the civil service quality input.
    /// </summary>
    public double HumanCapital { get; init; }

    /// <summary>
    /// Gets the diplomatic strength score (0.0).
    /// Always 0.0 as the diplomatic system is not yet established.
    /// </summary>
    public double Diplomatic { get; init; }

    /// <summary>
    /// Gets the military strength score (0.0).
    /// Always 0.0 as the military system is not yet established.
    /// </summary>
    public double Military { get; init; }

    /// <summary>
    /// Gets the composite total national strength score (0.0 to 1.0).
    /// Evaluated as the unweighted arithmetic mean (average) of the six dimensions.
    /// </summary>
    public double Total { get; init; }

    /// <summary>
    /// Gets the economic score formatted as a percentage string (e.g. "100%", "50%").
    /// </summary>
    public string FormattedEconomic => FormatPercent(Economic);

    /// <summary>
    /// Gets the institutional score formatted as a percentage string.
    /// </summary>
    public string FormattedInstitutional => FormatPercent(Institutional);

    /// <summary>
    /// Gets the infrastructure score formatted as a percentage string.
    /// </summary>
    public string FormattedInfrastructure => FormatPercent(Infrastructure);

    /// <summary>
    /// Gets the human capital score formatted as a percentage string.
    /// </summary>
    public string FormattedHumanCapital => FormatPercent(HumanCapital);

    /// <summary>
    /// Gets the diplomatic score formatted as a percentage string ("0%").
    /// </summary>
    public string FormattedDiplomatic => FormatPercent(Diplomatic);

    /// <summary>
    /// Gets the military score formatted as a percentage string ("0%").
    /// </summary>
    public string FormattedMilitary => FormatPercent(Military);

    /// <summary>
    /// Gets the total composite score formatted as a percentage string.
    /// </summary>
    public string FormattedTotal => FormatPercent(Total);

    /// <summary>
    /// Initializes a new instance of the <see cref="NationalStrength"/> value object from a sovereign country.
    /// </summary>
    /// <param name="country">The sovereign country entity.</param>
    /// <param name="foundingBuffs">Optional explicit founding buff snapshot; falls back to <see cref="Country.FoundingBuffs"/> if null.</param>
    public NationalStrength(Country country, FoundingBuffSnapshot? foundingBuffs = null)
    {
        ArgumentNullException.ThrowIfNull(country);

        CountryId = country.Id;
        CountryName = country.Name;

        // 1. Economic: treasury balance divided by R1,000,000,000, clamped at 1. R0 is 0. R1B or more is 1.
        Economic = Math.Clamp(country.Treasury.Balance / 1_000_000_000.0, 0.0, 1.0);

        // 2. Institutional: the existing State Capacity score.
        Institutional = Math.Clamp(country.StateCapacityScore, 0.0, 1.0);

        // 3. Infrastructure: the existing infrastructure input.
        Infrastructure = Math.Clamp(country.StateCapacity?.InfrastructureCondition ?? 0.0, 0.0, 1.0);

        // 4. Human capital: the institution-building founding buff if it is above 0, otherwise the civil service input.
        var buffs = foundingBuffs ?? country.FoundingBuffs;
        var institutionBuff = buffs?.InstitutionBuilding ?? 0.0;
        HumanCapital = institutionBuff > 0.0
            ? Math.Clamp(institutionBuff, 0.0, 1.0)
            : Math.Clamp(country.StateCapacity?.CivilServiceQuality ?? 0.0, 0.0, 1.0);

        // 5. Diplomatic: 0. Diplomacy is not established.
        Diplomatic = 0.0;

        // 6. Military: 0. The military system is not established.
        Military = 0.0;

        // 7. Total: the average of the six scores.
        Total = (Economic + Institutional + Infrastructure + HumanCapital + Diplomatic + Military) / 6.0;
    }

    /// <summary>
    /// Factory helper to calculate <see cref="NationalStrength"/> for a country.
    /// </summary>
    public static NationalStrength Calculate(Country country, FoundingBuffSnapshot? buffs = null) => new(country, buffs);

    /// <summary>
    /// Gets the formatted score lines for CLI presentation.
    /// </summary>
    public IReadOnlyList<string> Lines => new[]
    {
        $"Economic: {FormattedEconomic}",
        $"Institutional: {FormattedInstitutional}",
        $"Infrastructure: {FormattedInfrastructure}",
        $"Human Capital: {FormattedHumanCapital}",
        $"Diplomatic: {FormattedDiplomatic}",
        $"Military: {FormattedMilitary}",
        $"Total: {FormattedTotal}"
    };

    private static string FormatPercent(double value)
    {
        var clamped = Math.Clamp(value, 0.0, 1.0) * 100.0;
        return (clamped % 1 == 0)
            ? $"{clamped:0}%"
            : $"{clamped:0.#}%";
    }

    public override string ToString() => string.Join(Environment.NewLine, Lines);
}
