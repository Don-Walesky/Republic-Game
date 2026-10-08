namespace Republic.Core.NationalYield;

using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Service interface for evaluating and applying offline National Yield catch-up
/// across elapsed six-hour cycle boundaries on return, producing an executive briefing.
/// </summary>
public interface IOfflineYieldCatchUpService
{
    /// <summary>
    /// Gets the underlying schedule engine used to evaluate cycle boundaries.
    /// </summary>
    INationalYieldSchedule Schedule { get; }

    /// <summary>
    /// Gets the underlying cycle service used to execute due boundaries.
    /// </summary>
    INationalYieldCycleService CycleService { get; }

    /// <summary>
    /// Evaluates and credits all due production cycle boundaries for the specified country
    /// up to <paramref name="returnTime"/>, capped at 8 boundaries, and returns the executive briefing.
    /// Reads the country treasury balance before execution, executes due cycles, and calculates
    /// the deposited revenue strictly from the balance difference.
    /// </summary>
    /// <param name="country">The sovereign country catching up on missed production cycles.</param>
    /// <param name="returnTime">The authoritative simulation timestamp on return.</param>
    /// <returns>An <see cref="OfflineYieldBriefing"/> summarizing the catch-up result.</returns>
    OfflineYieldBriefing CatchUp(Country country, RepublicTime returnTime);

    /// <summary>
    /// Evaluates and credits all due production cycle boundaries for the specified country
    /// up to the clock's current time, capped at 8 boundaries, and returns the executive briefing.
    /// </summary>
    /// <param name="country">The sovereign country catching up on missed production cycles.</param>
    /// <param name="clock">The authoritative Republic clock.</param>
    /// <returns>An <see cref="OfflineYieldBriefing"/> summarizing the catch-up result.</returns>
    OfflineYieldBriefing CatchUp(Country country, IRepublicClock clock);
}
