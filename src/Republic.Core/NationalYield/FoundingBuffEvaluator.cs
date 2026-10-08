namespace Republic.Core.NationalYield;

using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Pure, deterministic evaluator for temporary tapering founding buffs following a sovereign nation's independence.
/// Implements the authoritative domain rules:
/// - Full strength (1.0) for the first 7 days following independence.
/// - Linear decay from 1.0 to 0.0 between day 7.0 and day 28.0.
/// - Exactly zero (0.0) at day 28.0 and beyond.
/// Evaluates entirely on the global Republic clock without invoking real-time APIs (never calls DateTime.Now).
/// </summary>
public sealed class FoundingBuffEvaluator : IFoundingBuffEvaluator
{
    /// <inheritdoc />
    public FoundingBuffSnapshot Evaluate(RepublicTime foundingTime, RepublicTime currentRepublicTime)
    {
        var strength = CalculateStrength(foundingTime, currentRepublicTime);
        return new FoundingBuffSnapshot(strength, currentRepublicTime);
    }

    /// <inheritdoc />
    public FoundingBuffSnapshot Evaluate(Country country, RepublicTime currentRepublicTime)
    {
        ArgumentNullException.ThrowIfNull(country);

        if (!country.IsFounded)
        {
            return new FoundingBuffSnapshot(0.0, currentRepublicTime);
        }

        return Evaluate(country.FoundingTime, currentRepublicTime);
    }

    /// <inheritdoc />
    public FoundingBuffSnapshot Evaluate(Country country, IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return Evaluate(country, clock.CurrentTime);
    }

    /// <summary>
    /// Pure calculation determining the uniform buff strength factor (0.0 to 1.0)
    /// based on the elapsed duration between founding time and current Republic simulation time.
    /// </summary>
    /// <param name="foundingTime">The nation's founding moment.</param>
    /// <param name="currentRepublicTime">The current Republic simulation time.</param>
    /// <returns>A factor between 0.0 and 1.0.</returns>
    public static double CalculateStrength(RepublicTime foundingTime, RepublicTime currentRepublicTime)
    {
        if (currentRepublicTime < foundingTime)
        {
            return 0.0;
        }

        var elapsed = currentRepublicTime - foundingTime;
        var elapsedDays = elapsed.TotalDays;

        if (elapsedDays <= FoundingBuffSnapshot.FullStrengthDays)
        {
            return 1.0;
        }

        if (elapsedDays >= FoundingBuffSnapshot.TotalDurationDays)
        {
            return 0.0;
        }

        var decay = (FoundingBuffSnapshot.TotalDurationDays - elapsedDays) / FoundingBuffSnapshot.DecayWindowDays;
        return Math.Clamp(decay, 0.0, 1.0);
    }
}
