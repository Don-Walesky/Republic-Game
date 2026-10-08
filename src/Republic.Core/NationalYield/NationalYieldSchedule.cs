namespace Republic.Core.NationalYield;

using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Authoritative schedule engine for six-hour National Yield production cycles on the global Republic clock.
/// Cycles are strictly aligned to West Africa Time (WAT / UTC+1) boundaries at 00:00, 06:00, 12:00, and 18:00 WAT.
/// </summary>
public sealed class NationalYieldSchedule : INationalYieldSchedule
{
    /// <summary>
    /// Maximum number of cycle boundaries that may be returned in a single evaluation call.
    /// </summary>
    public const int MaxBoundaryCap = 8;

    /// <summary>
    /// Fixed duration of each production cycle (6 hours).
    /// </summary>
    public static readonly TimeSpan CycleDuration = TimeSpan.FromHours(6);

    /// <inheritdoc />
    public RepublicTime GetNextBoundary(RepublicTime time)
    {
        // Production cycles are aligned to 00:00, 06:00, 12:00, and 18:00 WAT.
        // We find the earliest boundary strictly greater than `time`.
        if (time.Time < new TimeOnly(6, 0, 0))
        {
            return RepublicTime.FromDayAndTime(time.DayNumber, new TimeOnly(6, 0, 0), time.LaunchEpochWat);
        }
        if (time.Time < new TimeOnly(12, 0, 0))
        {
            return RepublicTime.FromDayAndTime(time.DayNumber, new TimeOnly(12, 0, 0), time.LaunchEpochWat);
        }
        if (time.Time < new TimeOnly(18, 0, 0))
        {
            return RepublicTime.FromDayAndTime(time.DayNumber, new TimeOnly(18, 0, 0), time.LaunchEpochWat);
        }

        // At or after 18:00 on Day D, the next boundary is 00:00 on Day D + 1
        return RepublicTime.FromDayAndTime(time.DayNumber + 1, new TimeOnly(0, 0, 0), time.LaunchEpochWat);
    }

    /// <inheritdoc />
    public RepublicTime GetNextDueBoundary(Country country, RepublicTime currentSimulationTime)
    {
        ArgumentNullException.ThrowIfNull(country);

        if (country.LastCreditedBoundary.HasValue)
        {
            return GetNextBoundary(country.LastCreditedBoundary.Value);
        }

        if (country.IsFounded)
        {
            return GetNextBoundary(country.FoundingTime);
        }

        var epoch = RepublicTime.FromDayAndTime(0, new TimeOnly(0, 0, 0), currentSimulationTime.LaunchEpochWat);
        return GetNextBoundary(epoch);
    }

    /// <inheritdoc />
    public IReadOnlyList<RepublicTime> GetDueBoundaries(
        RepublicTime? lastCreditedBoundary,
        RepublicTime currentSimulationTime,
        int maxBoundaries = 8)
    {
        var effectiveCap = Math.Clamp(maxBoundaries, 0, MaxBoundaryCap);
        if (effectiveCap == 0)
        {
            return Array.Empty<RepublicTime>();
        }

        // If no boundary has been credited, start from launch epoch (Day 0, 00:00 WAT)
        var start = lastCreditedBoundary ?? RepublicTime.FromDayAndTime(0, new TimeOnly(0, 0, 0), currentSimulationTime.LaunchEpochWat);
        var candidate = GetNextBoundary(start);

        var due = new List<RepublicTime>(effectiveCap);
        while (candidate <= currentSimulationTime && due.Count < effectiveCap)
        {
            due.Add(candidate);
            candidate = candidate.Add(CycleDuration);
        }

        return due;
    }

    /// <inheritdoc />
    public IReadOnlyList<RepublicTime> GetDueBoundaries(
        Country country,
        RepublicTime currentSimulationTime,
        int maxBoundaries = 8)
    {
        ArgumentNullException.ThrowIfNull(country);

        if (country.LastCreditedBoundary.HasValue)
        {
            return GetDueBoundaries(country.LastCreditedBoundary.Value, currentSimulationTime, maxBoundaries);
        }

        // If never credited, start from country's founding time if founded, else Day 0 00:00
        var baseTime = country.IsFounded
            ? country.FoundingTime
            : RepublicTime.FromDayAndTime(0, new TimeOnly(0, 0, 0), currentSimulationTime.LaunchEpochWat);

        return GetDueBoundaries(baseTime, currentSimulationTime, maxBoundaries);
    }
}
