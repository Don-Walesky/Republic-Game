namespace Republic.Core.World.Models;

using System.Globalization;
using Republic.Core.Economy.Treasury;
using Republic.Core.NationalYield;
using Republic.Core.Time;

/// <summary>
/// Read model representing the national dashboard for a sovereign country at an authoritative Republic simulation time.
/// Strictly read-only; evaluating or reading this dashboard induces zero side-effects on simulation state.
/// </summary>
public sealed record NationalDashboard
{
    /// <summary>
    /// Placeholder text for domains and capacities not yet established in this simulation phase.
    /// </summary>
    public const string NotEstablishedText = "Not established";

    /// <summary>
    /// Gets the authoritative Republic simulation timestamp at which this dashboard snapshot was generated.
    /// </summary>
    public RepublicTime SimulationTime { get; init; }

    /// <summary>
    /// Gets the unique identifier of the sovereign country.
    /// </summary>
    public string CountryId { get; init; }

    /// <summary>
    /// Gets the official name of the sovereign nation.
    /// </summary>
    public string CountryName { get; init; }

    /// <summary>
    /// Gets the sovereign public approval rating percentage (0.0 to 100.0).
    /// Backed by the country's existing happiness rating without creating a new approval simulation.
    /// </summary>
    public double ApprovalRating { get; init; }

    /// <summary>
    /// Gets the formatted approval percentage string (e.g. "68.5%").
    /// </summary>
    public string Approval { get; init; }

    /// <summary>
    /// Gets the economy overview line containing the treasury balance and GDP already on the country.
    /// </summary>
    public string Economy { get; init; }

    /// <summary>
    /// Gets the sovereign baseline stability factor (0.0 to 100.0).
    /// </summary>
    public double BaselineStability { get; init; }

    /// <summary>
    /// Gets the formatted baseline stability percentage string (e.g. "75%").
    /// </summary>
    public string Stability { get; init; }

    /// <summary>
    /// Gets the sovereign liquid treasury balance amount in REPU.
    /// </summary>
    public double TreasuryBalance { get; init; }

    /// <summary>
    /// Gets the formatted sovereign treasury balance string in standard REPU currency notation (e.g. "R15B", "R0").
    /// </summary>
    public string Treasury { get; init; }

    /// <summary>
    /// Gets the sovereign gross domestic product (GDP) in REPU.
    /// </summary>
    public double GrossDomesticProduct { get; init; }

    /// <summary>
    /// Alias for <see cref="GrossDomesticProduct"/>.
    /// </summary>
    public double GDP => GrossDomesticProduct;

    /// <summary>
    /// Gets the formatted gross domestic product string (e.g. "R450,000,000,000").
    /// </summary>
    public string Gdp { get; init; }

    /// <summary>
    /// Gets the evaluated revenue from the latest National Yield snapshot, or 0.0 if none exists.
    /// </summary>
    public double YieldRevenue { get; init; }

    /// <summary>
    /// Gets the formatted yield revenue string in standard REPU notation (e.g. "R0", "R500K").
    /// Shows "R0" if no yield snapshot exists.
    /// </summary>
    public string Yield { get; init; }

    /// <summary>
    /// Gets the State Capacity infrastructure condition input (0.0 to 1.0).
    /// </summary>
    public double InfrastructureCondition { get; init; }

    /// <summary>
    /// Gets the formatted infrastructure input percentage string (e.g. "60%").
    /// </summary>
    public string Infrastructure { get; init; }

    /// <summary>
    /// Gets the diplomatic capacity status. Defaults to "Not established".
    /// </summary>
    public string Diplomacy { get; init; } = NotEstablishedText;

    /// <summary>
    /// Gets the science and research capacity status. Defaults to "Not established".
    /// </summary>
    public string Science { get; init; } = NotEstablishedText;

    /// <summary>
    /// Gets the military readiness capacity status. Defaults to "Not established".
    /// </summary>
    public string Military { get; init; } = NotEstablishedText;

    /// <summary>
    /// Initializes a new instance of the <see cref="NationalDashboard"/> read model from a sovereign country.
    /// </summary>
    /// <param name="country">The sovereign country whose state is read.</param>
    /// <param name="simulationTime">The current authoritative Republic simulation timestamp.</param>
    /// <param name="yieldSnapshot">Optional explicit yield snapshot; falls back to <see cref="Country.LatestYieldSnapshot"/> if null.</param>
    public NationalDashboard(Country country, RepublicTime simulationTime, NationalYieldSnapshot? yieldSnapshot = null)
    {
        ArgumentNullException.ThrowIfNull(country);

        SimulationTime = simulationTime;
        CountryId = country.Id;
        CountryName = country.Name;

        // 1. Approval: existing happiness rating shown as percentage
        ApprovalRating = country.HappinessRating;
        Approval = FormatPercentage(ApprovalRating);

        // 2. Stability: existing baseline stability shown as percentage
        BaselineStability = country.BaselineStability;
        Stability = FormatPercentage(BaselineStability);

        // 3. Economy & Treasury: treasury balance and GDP already on the country
        TreasuryBalance = country.Treasury.Balance;
        Treasury = country.Treasury.FormattedBalance;
        GrossDomesticProduct = country.GrossDomesticProduct;
        Gdp = $"R{GrossDomesticProduct:N0}";
        Economy = $"Treasury: {Treasury} | GDP: {Gdp}";

        // 4. Yield: latest snapshot's RepuTreasuryRevenue, or R0 if none exists
        var snapshot = yieldSnapshot ?? country.LatestYieldSnapshot;
        if (snapshot != null)
        {
            YieldRevenue = snapshot.Yield.RepuTreasuryRevenue;
            Yield = RepuTreasury.Format(YieldRevenue);
        }
        else
        {
            YieldRevenue = 0.0;
            Yield = "R0";
        }

        // 5. Infrastructure: State Capacity infrastructure input, shown as percentage
        InfrastructureCondition = country.StateCapacity?.InfrastructureCondition ?? 0.0;
        Infrastructure = FormatUnitPercentage(InfrastructureCondition);

        // 6. Diplomacy, Science, and Military: "Not established"
        Diplomacy = NotEstablishedText;
        Science = NotEstablishedText;
        Military = NotEstablishedText;
    }

    /// <summary>
    /// Factory helper to build a <see cref="NationalDashboard"/> from a sovereign country and simulation time.
    /// </summary>
    public static NationalDashboard Create(Country country, RepublicTime simulationTime, NationalYieldSnapshot? snapshot = null) =>
        new(country, simulationTime, snapshot);

    /// <summary>
    /// Gets the formatted dashboard lines for display.
    /// </summary>
    public IReadOnlyList<string> Lines => new[]
    {
        $"Approval: {Approval}",
        $"Economy: {Economy}",
        $"Stability: {Stability}",
        $"Treasury: {Treasury}",
        $"Yield: {Yield}",
        $"Infrastructure: {Infrastructure}",
        $"Diplomacy: {Diplomacy}",
        $"Science: {Science}",
        $"Military: {Military}"
    };

    private static string FormatPercentage(double value)
    {
        var scaled = (value > 0.0 && value <= 1.0) ? value * 100.0 : value;
        return (scaled % 1 == 0)
            ? $"{scaled:0}%"
            : $"{scaled:0.#}%";
    }

    private static string FormatUnitPercentage(double unitValue)
    {
        var percent = Math.Clamp(unitValue, 0.0, 1.0) * 100.0;
        return (percent % 1 == 0)
            ? $"{percent:0}%"
            : $"{percent:0.#}%";
    }

    public override string ToString() => string.Join(Environment.NewLine, Lines);
}
