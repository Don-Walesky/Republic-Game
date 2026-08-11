namespace Republic.Core.Analytics.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Republic.Core.Analytics.Models;
using Republic.Core.Diagnostics;

/// <summary>
/// Service tracking, storing, and evaluating turn-by-turn campaign telemetry metrics.
/// </summary>
public sealed class TelemetryService
{
    private readonly List<TelemetrySnapshot> _snapshots = new();
    private readonly ILogger? _logger;
    private readonly object _lock = new();

    public TelemetryService(ILogger? logger = null)
    {
        _logger = logger;
    }

    public void RecordSnapshot(ulong tick, double treasury, double gdp, double happiness, double militaryReadiness)
    {
        var snapshot = new TelemetrySnapshot
        {
            Tick = tick,
            TreasuryBalance = treasury,
            GrossDomesticProduct = gdp,
            PublicHappiness = happiness,
            CompositeMilitaryReadiness = militaryReadiness,
            RecordedAt = DateTimeOffset.UtcNow
        };

        lock (_lock)
        {
            _snapshots.Add(snapshot);
        }

        _logger?.LogInfo($"[Telemetry] Recorded tick {tick}: Treasury=${treasury:N0}, GDP=${gdp:N0}, Happiness={happiness:0.0}%, Military={militaryReadiness:0.0}%");
    }

    public IReadOnlyList<TelemetrySnapshot> GetHistory()
    {
        lock (_lock)
        {
            return _snapshots.ToList().AsReadOnly();
        }
    }

    public double CalculateAverageHappiness()
    {
        lock (_lock)
        {
            if (_snapshots.Count == 0) return 0.0;
            return _snapshots.Average(s => s.PublicHappiness);
        }
    }
}
