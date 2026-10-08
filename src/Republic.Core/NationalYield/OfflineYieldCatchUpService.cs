namespace Republic.Core.NationalYield;

using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Service implementation for evaluating and processing offline National Yield catch-up.
/// Calculates missed six-hour production cycle boundaries between the country's last credited boundary
/// and the return simulation timestamp, executes them (capped at 8 boundaries), tracks the exact REPU
/// balance difference on the sovereign treasury, and produces an authoritative <see cref="OfflineYieldBriefing"/>.
/// Relies strictly on the durable country state (<see cref="Country.LastCreditedBoundary"/> and <see cref="Republic.Core.Economy.Treasury.RepuTreasury"/>)
/// rather than transient in-memory caches for idempotency.
/// </summary>
public sealed class OfflineYieldCatchUpService : IOfflineYieldCatchUpService
{
    private readonly INationalYieldCycleService _cycleService;
    private readonly INationalYieldSchedule _schedule;

    /// <summary>
    /// Initializes a new instance of the <see cref="OfflineYieldCatchUpService"/> class.
    /// </summary>
    /// <param name="cycleService">The cycle service to execute due cycles. If null, a default <see cref="NationalYieldCycleService"/> is used.</param>
    /// <param name="schedule">The schedule engine to evaluate due boundaries. If null, the cycle service's schedule or a default is used.</param>
    public OfflineYieldCatchUpService(
        INationalYieldCycleService? cycleService = null,
        INationalYieldSchedule? schedule = null)
    {
        _cycleService = cycleService ?? new NationalYieldCycleService();
        _schedule = schedule ?? _cycleService.Schedule ?? new NationalYieldSchedule();
    }

    /// <inheritdoc />
    public INationalYieldSchedule Schedule => _schedule;

    /// <inheritdoc />
    public INationalYieldCycleService CycleService => _cycleService;

    /// <inheritdoc />
    public OfflineYieldBriefing CatchUp(Country country, RepublicTime returnTime)
    {
        ArgumentNullException.ThrowIfNull(country);

        // 1. Determine starting moment before catch-up
        var fromTime = country.LastCreditedBoundary
            ?? (country.IsFounded ? country.FoundingTime : RepublicTime.FromDayAndTime(0, new TimeOnly(0, 0), returnTime.LaunchEpochWat));

        // 2. Count total missed boundaries between fromTime and returnTime (without cap)
        var totalDue = _schedule.CountDueBoundaries(country, returnTime);

        // 3. Read durable treasury balance before execution
        var balanceBefore = country.Treasury.Balance;

        // 4. Execute due cycles through existing cycle service (processes at most 8 boundaries)
        var snapshots = _cycleService.ExecuteDueCycles(country, returnTime);

        // 5. Read durable treasury balance after execution and calculate deposited REPU from the difference
        var balanceAfter = country.Treasury.Balance;
        var repuDeposited = Math.Max(0.0, balanceAfter - balanceBefore);

        // 6. Calculate boundaries credited and remaining uncredited boundaries (if total due exceeded 8)
        var creditedCount = snapshots.Count;
        var remainingBoundaries = Math.Max(0, totalDue - creditedCount);

        // 7. Determine the next six-hour cycle boundary due after this execution
        var nextDue = _schedule.GetNextDueBoundary(country, returnTime);

        return new OfflineYieldBriefing
        {
            CountryId = country.Id,
            FromTime = fromTime,
            ToTime = returnTime,
            BoundariesCredited = creditedCount,
            RepuDeposited = repuDeposited,
            RemainingBoundaries = remainingBoundaries,
            NextDueBoundary = nextDue
        };
    }

    /// <inheritdoc />
    public OfflineYieldBriefing CatchUp(Country country, IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return CatchUp(country, clock.CurrentTime);
    }
}
