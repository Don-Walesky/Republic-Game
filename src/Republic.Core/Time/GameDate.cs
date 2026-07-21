namespace Republic.Core.Time;

/// <summary>
/// Immutable in-game calendar date.
/// </summary>
/// <param name="Day">Day of month.</param>
/// <param name="Month">Month of year.</param>
/// <param name="Year">Simulation year.</param>
public sealed record GameDate(int Day, int Month, int Year);
