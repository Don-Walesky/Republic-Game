namespace Republic.Core.Economy.Budget.Services;

using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Domain schedule defining authoritative simulation-time fiscal cycles and quarter boundaries.
/// Enforces consistent 90-day simulation fiscal quarters anchored to West Africa Time (WAT / UTC+1).
/// </summary>
public static class FiscalSchedule
{
    /// <summary>
    /// The standard number of Republic simulation days comprising one fiscal quarter (90 days).
    /// </summary>
    public const int DaysPerQuarter = 90;

    /// <summary>
    /// Evaluates the 1-based fiscal quarter index for the specified simulation time.
    /// Quarter 1: Days 0..89, Quarter 2: Days 90..179, Quarter 3: Days 180..269, Quarter 4: Days 270..359, etc.
    /// </summary>
    public static int GetQuarterIndex(RepublicTime time)
    {
        if (time.DayNumber < 0)
        {
            return 1;
        }

        return (int)(time.DayNumber / DaysPerQuarter) + 1;
    }

    /// <summary>
    /// Gets the first eligible fiscal quarter for a sovereign country based on its founding date.
    /// Pre-founding quarters are never eligible for taxation or expenditure settlement.
    /// </summary>
    public static int GetFirstEligibleQuarter(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        if (!country.IsFounded)
        {
            return 1;
        }

        return GetQuarterIndex(country.FoundingTime);
    }

    /// <summary>
    /// Calculates the authoritative RepublicTime start boundary for a specified 1-based quarter index.
    /// </summary>
    public static RepublicTime GetQuarterStartTime(int quarterIndex, DateTimeOffset launchEpochWat)
    {
        if (quarterIndex < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(quarterIndex), "Quarter index must be at least 1.");
        }

        var startDayNumber = (long)(quarterIndex - 1) * DaysPerQuarter;
        return RepublicTime.FromDayAndTime(startDayNumber, TimeOnly.MinValue, launchEpochWat);
    }

    /// <summary>
    /// Evaluates all due fiscal quarters for a sovereign country at the specified simulation time.
    /// Returns quarters in strictly ascending order that have not yet been settled.
    /// </summary>
    public static IReadOnlyList<int> GetDueQuarters(Country country, RepublicTime simulationTime)
    {
        ArgumentNullException.ThrowIfNull(country);

        var currentQuarter = GetQuarterIndex(simulationTime);
        var firstEligible = GetFirstEligibleQuarter(country);
        var startQuarter = Math.Max(firstEligible, country.LastProcessedFiscalQuarter + 1);

        if (startQuarter > currentQuarter)
        {
            return Array.Empty<int>();
        }

        var count = currentQuarter - startQuarter + 1;
        var due = new List<int>(count);
        for (var q = startQuarter; q <= currentQuarter; q++)
        {
            due.Add(q);
        }

        return due;
    }
}
