namespace Republic.Core.NationalYield;

using Republic.Core.Economy.Treasury;
using Republic.Core.Time;

/// <summary>
/// Immutable value representation of an executive briefing summarizing offline National Yield catch-up.
/// Captures the simulation time range, boundaries credited, liquid REPU revenue deposited,
/// remaining unprocessed boundaries (if capped at 8), and the next due production cycle boundary.
/// </summary>
public sealed class OfflineYieldBriefing
{
    /// <summary>
    /// Gets the unique identifier of the sovereign country receiving the briefing.
    /// </summary>
    public string CountryId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the starting simulation timestamp from which catch-up was evaluated
    /// (the country's prior last credited boundary, founding time, or epoch).
    /// </summary>
    public RepublicTime FromTime { get; init; }

    /// <summary>
    /// Gets the simulation timestamp on return at which catch-up was executed.
    /// </summary>
    public RepublicTime ToTime { get; init; }

    /// <summary>
    /// Gets the number of six-hour production cycle boundaries credited in this catch-up execution.
    /// Capped at a maximum of 8 boundaries per login / execution.
    /// </summary>
    public int BoundariesCredited { get; init; }

    /// <summary>
    /// Gets the total liquid REPU revenue deposited into the sovereign treasury across all credited boundaries.
    /// Calculated strictly as the difference between the treasury balance before and after execution.
    /// </summary>
    public double RepuDeposited { get; init; }

    /// <summary>
    /// Gets the number of due production cycle boundaries that remain uncredited because of the 8-boundary cap.
    /// </summary>
    public int RemainingBoundaries { get; init; }

    /// <summary>
    /// Gets the next six-hour production cycle boundary due to be credited after this catch-up execution.
    /// </summary>
    public RepublicTime NextDueBoundary { get; init; }

    /// <summary>
    /// Gets the formatted REPU currency string for the deposited revenue (e.g. R0, R45M, R1.2B).
    /// </summary>
    public string FormattedRepuDeposited => RepuTreasury.Format(RepuDeposited);

    /// <summary>
    /// Gets a value indicating whether any revenue was credited and deposited during this catch-up execution.
    /// </summary>
    public bool HasCredited => BoundariesCredited > 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="OfflineYieldBriefing"/> class.
    /// </summary>
    public OfflineYieldBriefing()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OfflineYieldBriefing"/> class with explicit values.
    /// </summary>
    public OfflineYieldBriefing(
        string countryId,
        RepublicTime fromTime,
        RepublicTime toTime,
        int boundariesCredited,
        double repuDeposited,
        int remainingBoundaries,
        RepublicTime nextDueBoundary)
    {
        CountryId = countryId ?? string.Empty;
        FromTime = fromTime;
        ToTime = toTime;
        BoundariesCredited = boundariesCredited;
        RepuDeposited = repuDeposited;
        RemainingBoundaries = remainingBoundaries;
        NextDueBoundary = nextDueBoundary;
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"Country: {CountryId} | Period: [{FromTime} -> {ToTime}] | Credited: {BoundariesCredited} | Deposited: {FormattedRepuDeposited} | Remaining: {RemainingBoundaries} | Next Due: {NextDueBoundary}";
}
