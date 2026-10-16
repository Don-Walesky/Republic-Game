namespace Republic.Core.Networking;

using System.Text.Json.Serialization;
using Republic.Core.Time;

/// <summary>
/// Synchronized snapshot of the authoritative Republic simulation clock for multiplayer sessions.
/// </summary>
public sealed record NetworkClockSnapshot
{
    /// <summary>
    /// Gets the current deterministic simulation tick number.
    /// </summary>
    [JsonPropertyName("tick")]
    public ulong Tick { get; init; }

    /// <summary>
    /// Gets the simulation day number from launch (0 for Day 0, 1 for Day 1, etc.).
    /// </summary>
    [JsonPropertyName("day")]
    public long Day { get; init; }

    /// <summary>
    /// Gets the formatted simulation date and time in West Africa Time (e.g. "Day 0, 00:00 WAT").
    /// </summary>
    [JsonPropertyName("time_string")]
    public string TimeString { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the simulation clock is paused.
    /// </summary>
    [JsonPropertyName("is_paused")]
    public bool IsPaused { get; init; }

    /// <summary>
    /// Creates a new <see cref="NetworkClockSnapshot"/> from an authoritative <see cref="RepublicTime"/> instance.
    /// </summary>
    public static NetworkClockSnapshot FromRepublicTime(RepublicTime time, ulong tick = 0, bool isPaused = false) =>
        new()
        {
            Tick = tick,
            Day = time.DayNumber,
            TimeString = time.ToString(),
            IsPaused = isPaused
        };
}
