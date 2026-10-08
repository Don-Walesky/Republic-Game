namespace Republic.Core.NationalYield;

using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Domain contract for pure evaluation of temporary tapering founding buffs following a sovereign country's independence.
/// Evaluates buffs purely on the global Republic clock without accessing real-time APIs (e.g. DateTime.Now).
/// </summary>
public interface IFoundingBuffEvaluator
{
    /// <summary>
    /// Purely calculates a <see cref="FoundingBuffSnapshot"/> from a country's founding time and the evaluation simulation time.
    /// </summary>
    /// <param name="foundingTime">The authoritative founding moment of the country on the Republic clock.</param>
    /// <param name="currentRepublicTime">The current simulation time on the Republic clock.</param>
    /// <returns>An immutable <see cref="FoundingBuffSnapshot"/>.</returns>
    FoundingBuffSnapshot Evaluate(RepublicTime foundingTime, RepublicTime currentRepublicTime);

    /// <summary>
    /// Purely calculates a <see cref="FoundingBuffSnapshot"/> for a sovereign country at the specified simulation time.
    /// If the country has not been founded yet, returns a snapshot with all zero buff factors.
    /// </summary>
    /// <param name="country">The sovereign country whose founding buffs to evaluate.</param>
    /// <param name="currentRepublicTime">The current simulation time on the Republic clock.</param>
    /// <returns>An immutable <see cref="FoundingBuffSnapshot"/>.</returns>
    FoundingBuffSnapshot Evaluate(Country country, RepublicTime currentRepublicTime);

    /// <summary>
    /// Calculates a <see cref="FoundingBuffSnapshot"/> for a sovereign country at the current time of the authoritative Republic clock.
    /// </summary>
    /// <param name="country">The sovereign country whose founding buffs to evaluate.</param>
    /// <param name="clock">The authoritative Republic clock.</param>
    /// <returns>An immutable <see cref="FoundingBuffSnapshot"/>.</returns>
    FoundingBuffSnapshot Evaluate(Country country, IRepublicClock clock);
}
