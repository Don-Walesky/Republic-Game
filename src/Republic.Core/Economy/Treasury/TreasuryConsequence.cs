namespace Republic.Core.Economy.Treasury;

using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Domain model representing a systemic consequence resulting from sovereign treasury state at a six-hour cycle boundary.
/// If the treasury balance is below R100,000,000 at a boundary, public approval falls by 2 points (clamped at 0).
/// Evaluated at most once per country per boundary.
/// </summary>
public sealed class TreasuryConsequence
{
    /// <summary>
    /// Treasury threshold below which public approval falls (R100,000,000).
    /// </summary>
    public const double LowTreasuryThreshold = 100_000_000.0;

    /// <summary>
    /// Approval points lost when treasury is below threshold (2.0 points).
    /// </summary>
    public const double ApprovalDropPoints = 2.0;

    /// <summary>
    /// Gets the unique boundary identifier where this consequence occurred.
    /// </summary>
    public string BoundaryId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the simulation time of the cycle boundary.
    /// </summary>
    public RepublicTime BoundaryTime { get; init; }

    /// <summary>
    /// Gets the sovereign treasury balance at the time of evaluation.
    /// </summary>
    public double TreasuryBalance { get; init; }

    /// <summary>
    /// Gets the approval penalty points applied.
    /// </summary>
    public double ApprovalDrop { get; init; }

    /// <summary>
    /// Gets the public approval rating prior to this consequence.
    /// </summary>
    public double PreviousApproval { get; init; }

    /// <summary>
    /// Gets the public approval rating after applying this consequence.
    /// </summary>
    public double CurrentApproval { get; init; }

    /// <summary>
    /// Gets the formatted descriptive summary of the consequence.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    public override string ToString() => Description;

    /// <summary>
    /// Generates a deterministic boundary identifier for a country and simulation timestamp.
    /// </summary>
    public static string GenerateBoundaryId(string countryId, RepublicTime boundary)
    {
        return $"{countryId}:Day{boundary.DayNumber}:{boundary.Time:HHmmss}";
    }

    /// <summary>
    /// Applies the low-treasury consequence to the specified country at a cycle boundary.
    /// Returns the applied consequence if approval fell; null if balance >= R100M or boundary already processed.
    /// </summary>
    public static TreasuryConsequence? Apply(Country country, RepublicTime boundary)
    {
        ArgumentNullException.ThrowIfNull(country);

        var boundaryId = GenerateBoundaryId(country.Id, boundary);
        if (country.HasAppliedConsequenceBoundary(boundaryId))
        {
            return null;
        }

        // Record boundary so it cannot run twice
        country.RecordConsequenceBoundaryApplied(boundaryId);

        var currentBalance = country.Treasury.Balance;
        if (currentBalance >= LowTreasuryThreshold)
        {
            return null;
        }

        var prevApproval = country.ApprovalRating;
        var newApproval = Math.Max(0.0, prevApproval - ApprovalDropPoints);
        var actualDrop = prevApproval - newApproval;

        country.ApprovalRating = newApproval;

        var formattedBalance = RepuTreasury.Format(currentBalance);
        var consequence = new TreasuryConsequence
        {
            BoundaryId = boundaryId,
            BoundaryTime = boundary,
            TreasuryBalance = currentBalance,
            ApprovalDrop = actualDrop,
            PreviousApproval = prevApproval,
            CurrentApproval = newApproval,
            Description = $"Low treasury ({formattedBalance} < R100M): Approval -{actualDrop:0.#}% ({prevApproval:0.#}% -> {newApproval:0.#}%)"
        };

        country.LatestTreasuryConsequence = consequence;
        return consequence;
    }
}
