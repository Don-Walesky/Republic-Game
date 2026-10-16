namespace Republic.Core.Networking;

using System;
using System.Collections.Generic;
using Republic.Core.World;

/// <summary>
/// Serializable snapshot representing the persistent state of an authoritative Republic multiplayer world.
/// Persisted to a single file on the host to ensure all countries, treasuries, diplomacy, and trade
/// survive host restarts.
/// </summary>
public sealed class SharedWorldSaveData
{
    /// <summary>
    /// Gets or sets the save schema format version.
    /// </summary>
    public int FormatVersion { get; set; } = 1;

    /// <summary>
    /// Gets or sets the UTC timestamp when the world was persisted.
    /// </summary>
    public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the synchronized simulation world clock snapshot.
    /// </summary>
    public NetworkClockSnapshot Clock { get; set; } = new();

    /// <summary>
    /// Gets or sets the sovereign country snapshots known to the host.
    /// </summary>
    public List<CountrySnapshot> Countries { get; set; } = new();

    /// <summary>
    /// Gets or sets authoritative liquid treasury balances mapped by country identifier.
    /// </summary>
    public Dictionary<string, double> TreasuryBalances { get; set; } = new();

    /// <summary>
    /// Gets or sets the player identity / connection token ownership mapped by country identifier.
    /// </summary>
    public Dictionary<string, string> CountryOwners { get; set; } = new();

    /// <summary>
    /// Gets or sets active bilateral diplomatic recognition snapshots.
    /// </summary>
    public List<DiplomaticSnapshot> Diplomacy { get; set; } = new();

    /// <summary>
    /// Gets or sets diplomatic dispatches and messages.
    /// </summary>
    public List<DiplomaticMessage> Messages { get; set; } = new();

    /// <summary>
    /// Gets or sets active and resolved trade proposals.
    /// </summary>
    public List<TradeProposal> Trades { get; set; } = new();

    /// <summary>
    /// Gets or sets request identifiers processed by the host for deduplication.
    /// </summary>
    public List<string> ProcessedRequestIds { get; set; } = new();
}
