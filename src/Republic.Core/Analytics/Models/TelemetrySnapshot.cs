namespace Republic.Core.Analytics.Models;

using System;

/// <summary>
/// Historical telemetry data point recorded at a specific tick in the game simulation.
/// </summary>
public sealed record TelemetrySnapshot
{
    public ulong Tick { get; init; }
    public double TreasuryBalance { get; init; }
    public double GrossDomesticProduct { get; init; }
    public double PublicHappiness { get; init; }
    public double CompositeMilitaryReadiness { get; init; }
    public DateTimeOffset RecordedAt { get; init; } = DateTimeOffset.UtcNow;
}
