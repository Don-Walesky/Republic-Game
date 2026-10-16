namespace Republic.Core.World;

using System;
using System.Text.Json.Serialization;
using Republic.Core.Economy.Treasury;

/// <summary>
/// Status enumeration for bilateral trade proposals.
/// </summary>
public enum TradeProposalStatus
{
    Pending,
    Accepted,
    Rejected
}

/// <summary>
/// Minimal trade proposal between two sovereign nations.
/// Tracks proposed resource or treasury amount, proposer, recipient, and current resolution status.
/// </summary>
public sealed class TradeProposal
{
    /// <summary>
    /// Gets the unique identifier of the trade proposal.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets the sovereign sender/proposer ID or name.
    /// </summary>
    [JsonPropertyName("from")]
    public string From { get; init; } = string.Empty;

    /// <summary>
    /// Gets the sovereign recipient ID or name.
    /// </summary>
    [JsonPropertyName("to")]
    public string To { get; init; } = string.Empty;

    /// <summary>
    /// Gets the monetary or resource amount to transfer.
    /// </summary>
    [JsonPropertyName("amount")]
    public double Amount { get; init; }

    /// <summary>
    /// Gets the resource type or currency being traded (defaults to "REPU").
    /// </summary>
    [JsonPropertyName("resource")]
    public string Resource { get; init; } = "REPU";

    /// <summary>
    /// Gets or sets the resolution status of this trade proposal ("Pending", "Accepted", "Rejected").
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = TradeProposalStatus.Pending.ToString();

    /// <summary>
    /// Initializes a new instance of the <see cref="TradeProposal"/> class.
    /// </summary>
    public TradeProposal() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TradeProposal"/> class with explicit properties.
    /// </summary>
    public TradeProposal(string from, string to, double amount, string resource = "REPU", string status = "Pending")
    {
        From = from ?? string.Empty;
        To = to ?? string.Empty;
        Amount = amount;
        Resource = resource ?? "REPU";
        Status = status ?? TradeProposalStatus.Pending.ToString();
    }

    /// <summary>
    /// Executes the trade between two treasuries using existing authoritative treasury rules.
    /// Deducts amount from the proposing country's treasury and deposits into the recipient country's treasury.
    /// </summary>
    /// <param name="fromTreasury">Proposer treasury.</param>
    /// <param name="toTreasury">Recipient treasury.</param>
    /// <returns>True if the transaction succeeded under treasury overdraft and non-negative constraints; otherwise, false.</returns>
    public bool Accept(RepuTreasury fromTreasury, RepuTreasury toTreasury)
    {
        ArgumentNullException.ThrowIfNull(fromTreasury);
        ArgumentNullException.ThrowIfNull(toTreasury);

        if (Amount <= 0)
        {
            return false;
        }

        if (!fromTreasury.TryWithdraw(Amount))
        {
            return false;
        }

        toTreasury.Deposit(Amount);
        Status = TradeProposalStatus.Accepted.ToString();
        return true;
    }

    /// <summary>
    /// Executes the trade between two sovereign country models.
    /// </summary>
    public bool Accept(Republic.Core.World.Models.Country fromCountry, Republic.Core.World.Models.Country toCountry)
    {
        ArgumentNullException.ThrowIfNull(fromCountry);
        ArgumentNullException.ThrowIfNull(toCountry);
        return Accept(fromCountry.Treasury, toCountry.Treasury);
    }

    /// <summary>
    /// Rejects the trade proposal.
    /// </summary>
    public void Reject()
    {
        Status = TradeProposalStatus.Rejected.ToString();
    }
}
