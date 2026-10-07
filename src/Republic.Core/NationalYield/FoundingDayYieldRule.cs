namespace Republic.Core.NationalYield;

using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Domain rule implementation governing the founding-day National Yield multiplier (x10).
/// Pure, deterministic domain logic that evaluates whether a country's calculation falls within its
/// Independence Day in the Republic simulation calendar and applies the x10 multiplier.
/// Does not mutate the input yield, treasury, or clock.
/// </summary>
public class FoundingDayYieldRule : IFoundingYieldMultiplierRule
{
    /// <inheritdoc />
    public bool IsFoundingDay(Country country, RepublicTime simulationTime)
    {
        ArgumentNullException.ThrowIfNull(country);

        if (!country.IsFounded)
        {
            return false;
        }

        // The founding day is based on the country's actual Independence Day in the Republic simulation calendar (WAT),
        // not simply 24 elapsed hours from creation.
        return simulationTime >= country.FoundingTime && simulationTime.DayNumber == country.FoundingRepublicDay;
    }

    /// <inheritdoc />
    public bool IsFoundingDay(Country country, IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return IsFoundingDay(country, clock.CurrentTime);
    }

    /// <inheritdoc />
    public double GetMultiplier(Country country, RepublicTime simulationTime) =>
        IsFoundingDay(country, simulationTime) ? IFoundingDayYieldRule.Multiplier : IFoundingDayYieldRule.StandardMultiplier;

    /// <inheritdoc />
    public NationalYield Apply(Country country, NationalYield yield, RepublicTime simulationTime)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(yield);

        var multiplier = GetMultiplier(country, simulationTime);
        return yield.Multiply(multiplier);
    }

    /// <inheritdoc />
    public NationalYield Apply(Country country, NationalYield yield, IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return Apply(country, yield, clock.CurrentTime);
    }

    /// <inheritdoc />
    public NationalYieldSnapshot Apply(Country country, NationalYieldSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(snapshot);

        var scaledYield = Apply(country, snapshot.Yield, snapshot.SimulationTime);
        return new NationalYieldSnapshot(snapshot.CountryId, snapshot.SimulationTime, scaledYield);
    }
}

/// <summary>
/// Alias class for <see cref="FoundingDayYieldRule"/>.
/// </summary>
public sealed class FoundingYieldMultiplierRule : FoundingDayYieldRule
{
}
