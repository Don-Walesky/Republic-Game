namespace Republic.Core.World;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Republic.Core.Economy.Treasury;
using Republic.Core.Time;

/// <summary>
/// Status enumeration for flexible bilateral trade proposals and contracts.
/// </summary>
public enum FlexibleTradeStatus
{
    Draft,
    Pending,
    Countered,
    Accepted,
    ActiveContract,
    Completed,
    Rejected,
    Cancelled,
    Expired,
    Disputed
}

/// <summary>
/// Domain model specifying flexible bilateral trade terms between two sovereign nations.
/// Supports goods-for-currency, currency-for-goods, barter exchange (goods-for-goods),
/// one-time spot transactions, recurring contract delivery schedules, and multi-round counter-offer revisions.
/// </summary>
public sealed record FlexibleTradeTerms
{
    /// <summary>
    /// Gets the unique identifier of the bilateral trade deal or contract.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets the sovereign nation initiating or originally proposing the deal.
    /// </summary>
    [JsonPropertyName("proposer_country_id")]
    public string ProposerCountryId { get; init; }

    /// <summary>
    /// Gets the sovereign nation receiving the proposal.
    /// </summary>
    [JsonPropertyName("target_country_id")]
    public string TargetCountryId { get; init; }

    /// <summary>
    /// Gets the commodity offered by the proposer (e.g. Iron, Timber, Petroleum), or null/empty if pure currency.
    /// </summary>
    [JsonPropertyName("offered_resource_type")]
    public string? OfferedResourceType { get; init; }

    /// <summary>
    /// Gets the unit quantity of physical goods offered by the proposer. Must be non-negative.
    /// </summary>
    [JsonPropertyName("offered_quantity")]
    public double OfferedQuantity { get; init; }

    /// <summary>
    /// Gets the commodity requested by the proposer from the target, or null/empty if pure currency.
    /// </summary>
    [JsonPropertyName("requested_resource_type")]
    public string? RequestedResourceType { get; init; }

    /// <summary>
    /// Gets the unit quantity of physical goods requested from the target. Must be non-negative.
    /// </summary>
    [JsonPropertyName("requested_quantity")]
    public double RequestedQuantity { get; init; }

    /// <summary>
    /// Gets the monetary transfer in REPU.
    /// Positive value: Proposer pays Target.
    /// Negative value: Target pays Proposer.
    /// Zero: Pure barter without cash balancing payment.
    /// </summary>
    [JsonPropertyName("currency_payment")]
    public double CurrencyPayment { get; init; }

    /// <summary>
    /// Indicates whether this deal represents a recurring delivery contract rather than a one-time spot exchange.
    /// </summary>
    [JsonPropertyName("is_recurring")]
    public bool IsRecurring { get; init; }

    /// <summary>
    /// Gets the total number of delivery cycles for a recurring agreement (e.g. 5 days). Defaults to 1 for spot deals.
    /// </summary>
    [JsonPropertyName("total_delivery_cycles")]
    public int TotalDeliveryCycles { get; init; } = 1;

    /// <summary>
    /// Gets the number of delivery cycles remaining to be executed.
    /// </summary>
    [JsonPropertyName("remaining_delivery_cycles")]
    public int RemainingDeliveryCycles { get; init; } = 1;

    /// <summary>
    /// Gets the current revision iteration of the proposal.
    /// Starts at 1 and increments every time terms are revised via counter-offer.
    /// </summary>
    [JsonPropertyName("revision_number")]
    public int RevisionNumber { get; init; } = 1;

    /// <summary>
    /// Gets the country ID that drafted or last modified these terms.
    /// </summary>
    [JsonPropertyName("last_modified_by_country_id")]
    public string LastModifiedByCountryId { get; init; }

    /// <summary>
    /// Indicates whether the proposer has approved the current revision of the terms.
    /// </summary>
    [JsonPropertyName("proposer_approved")]
    public bool ProposerApproved { get; init; } = true;

    /// <summary>
    /// Indicates whether the target country has approved the current revision of the terms.
    /// </summary>
    [JsonPropertyName("target_approved")]
    public bool TargetApproved { get; init; } = false;

    /// <summary>
    /// Gets the current lifecycle status of the proposal.
    /// </summary>
    [JsonPropertyName("status")]
    public FlexibleTradeStatus Status { get; init; } = FlexibleTradeStatus.Pending;

    /// <summary>
    /// Gets the simulation time boundary when this proposal was created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public RepublicTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the simulation time boundary when this proposal was last modified or countered.
    /// </summary>
    [JsonPropertyName("last_modified_at")]
    public RepublicTime LastModifiedAt { get; init; }

    /// <summary>
    /// Gets optional diplomatic notes or justification accompanying these terms.
    /// </summary>
    [JsonPropertyName("terms_notes")]
    public string TermsNotes { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional simulation time boundary when this proposal expires.
    /// </summary>
    [JsonPropertyName("expires_at")]
    public RepublicTime? ExpiresAt { get; init; }

    /// <summary>
    /// Gets the simulation time boundary when the agreement was executed or decided.
    /// </summary>
    [JsonPropertyName("decided_at")]
    public RepublicTime? DecidedAt { get; init; }

    /// <summary>
    /// Gets an explanatory message describing the latest status change or transaction outcome.
    /// </summary>
    [JsonPropertyName("status_message")]
    public string StatusMessage { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether both parties have approved the exact current revision of these terms.
    /// </summary>
    [JsonIgnore]
    public bool IsMutuallyApproved => ProposerApproved && TargetApproved;

    /// <summary>
    /// Indicates whether this deal represents a pure barter exchange (goods for goods).
    /// </summary>
    [JsonIgnore]
    public bool IsBarter => !string.IsNullOrWhiteSpace(OfferedResourceType) &&
                            OfferedQuantity > 0 &&
                            !string.IsNullOrWhiteSpace(RequestedResourceType) &&
                            RequestedQuantity > 0;

    /// <summary>
    /// Indicates whether this deal is an outbound sale of goods for REPU payment.
    /// </summary>
    [JsonIgnore]
    public bool IsGoodsForCurrency => !string.IsNullOrWhiteSpace(OfferedResourceType) &&
                                      OfferedQuantity > 0 &&
                                      CurrencyPayment < 0;

    /// <summary>
    /// Indicates whether this deal is a purchase of foreign goods with REPU payment.
    /// </summary>
    [JsonIgnore]
    public bool IsCurrencyForGoods => !string.IsNullOrWhiteSpace(RequestedResourceType) &&
                                      RequestedQuantity > 0 &&
                                      CurrencyPayment > 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlexibleTradeTerms"/> record.
    /// </summary>
    public FlexibleTradeTerms(
        string proposerCountryId,
        string targetCountryId,
        string? offeredResourceType = null,
        double offeredQuantity = 0.0,
        string? requestedResourceType = null,
        double requestedQuantity = 0.0,
        double currencyPayment = 0.0,
        bool isRecurring = false,
        int totalDeliveryCycles = 1,
        RepublicTime createdAt = default,
        string? id = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(proposerCountryId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetCountryId);

        if (string.Equals(proposerCountryId.Trim(), targetCountryId.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Proposer and target cannot be the same sovereign country. Internal self-trade is prohibited.", nameof(targetCountryId));
        }

        if (offeredQuantity < 0.0 || !double.IsFinite(offeredQuantity))
        {
            throw new ArgumentOutOfRangeException(nameof(offeredQuantity), "Offered quantity must be a finite non-negative number.");
        }

        if (requestedQuantity < 0.0 || !double.IsFinite(requestedQuantity))
        {
            throw new ArgumentOutOfRangeException(nameof(requestedQuantity), "Requested quantity must be a finite non-negative number.");
        }

        if (!double.IsFinite(currencyPayment))
        {
            throw new ArgumentOutOfRangeException(nameof(currencyPayment), "Currency payment must be a finite number.");
        }

        // Deal must involve at least one transfer of goods or currency
        bool hasOffered = !string.IsNullOrWhiteSpace(offeredResourceType) && offeredQuantity > 0.0;
        bool hasRequested = !string.IsNullOrWhiteSpace(requestedResourceType) && requestedQuantity > 0.0;
        bool hasPayment = Math.Abs(currencyPayment) > 0.0;

        if (!hasOffered && !hasRequested && !hasPayment)
        {
            throw new ArgumentException("Trade terms must specify at least one commodity transfer or monetary payment.");
        }

        Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id.Trim();
        ProposerCountryId = proposerCountryId.Trim();
        TargetCountryId = targetCountryId.Trim();
        OfferedResourceType = string.IsNullOrWhiteSpace(offeredResourceType) ? null : offeredResourceType.Trim();
        OfferedQuantity = hasOffered ? offeredQuantity : 0.0;
        RequestedResourceType = string.IsNullOrWhiteSpace(requestedResourceType) ? null : requestedResourceType.Trim();
        RequestedQuantity = hasRequested ? requestedQuantity : 0.0;
        CurrencyPayment = currencyPayment;
        IsRecurring = isRecurring;
        TotalDeliveryCycles = isRecurring ? Math.Max(1, totalDeliveryCycles) : 1;
        RemainingDeliveryCycles = TotalDeliveryCycles;
        RevisionNumber = 1;
        LastModifiedByCountryId = ProposerCountryId;
        ProposerApproved = true;
        TargetApproved = false;
        Status = FlexibleTradeStatus.Pending;
        CreatedAt = createdAt;
    }

    [JsonConstructor]
    public FlexibleTradeTerms()
    {
        ProposerCountryId = string.Empty;
        TargetCountryId = string.Empty;
        LastModifiedByCountryId = string.Empty;
    }

    /// <summary>
    /// Checks whether the specified country is a party to this bilateral trade deal.
    /// </summary>
    public bool Involves(string countryId)
    {
        if (string.IsNullOrWhiteSpace(countryId))
        {
            return false;
        }

        var trimmed = countryId.Trim();
        return string.Equals(ProposerCountryId, trimmed, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(TargetCountryId, trimmed, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the specified country is authorized to accept the current terms without countering.
    /// A country cannot accept terms that it authored if the other party has not yet reviewed them.
    /// </summary>
    public bool CanBeAcceptedBy(string countryId)
    {
        if (!Involves(countryId))
        {
            return false;
        }

        var trimmed = countryId.Trim();

        // Cannot accept if this country was the last to modify terms and already approved them
        if (string.Equals(LastModifiedByCountryId, trimmed, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // If target, target must not have already approved
        if (string.Equals(TargetCountryId, trimmed, StringComparison.OrdinalIgnoreCase) && TargetApproved)
        {
            return false;
        }

        // If proposer receiving a counter-offer, proposer must not have already approved
        if (string.Equals(ProposerCountryId, trimmed, StringComparison.OrdinalIgnoreCase) && ProposerApproved)
        {
            return false;
        }

        return Status is FlexibleTradeStatus.Pending or FlexibleTradeStatus.Countered;
    }

    /// <summary>
    /// Creates a revised counter-offer from the specified country.
    /// CRITICAL REQUIREMENT: Modifying terms invalidates earlier approvals and resets mutual acceptance.
    /// </summary>
    public FlexibleTradeTerms CreateCounterOffer(
        string counteringCountryId,
        string? revisedOfferedResource,
        double? revisedOfferedQty,
        string? revisedRequestedResource,
        double? revisedRequestedQty,
        double? revisedCurrencyPayment,
        RepublicTime currentTime = default,
        string? revisedNotes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(counteringCountryId);

        if (!Involves(counteringCountryId))
        {
            throw new InvalidOperationException($"Country '{counteringCountryId}' cannot counter a trade it is not party to.");
        }

        var isProposerCountering = string.Equals(counteringCountryId.Trim(), ProposerCountryId, StringComparison.OrdinalIgnoreCase);

        var finalOfferedQty = revisedOfferedQty.HasValue ? Math.Max(0.0, revisedOfferedQty.Value) : OfferedQuantity;
        var finalRequestedQty = revisedRequestedQty.HasValue ? Math.Max(0.0, revisedRequestedQty.Value) : RequestedQuantity;
        var finalPayment = revisedCurrencyPayment ?? CurrencyPayment;

        return this with
        {
            OfferedResourceType = string.IsNullOrWhiteSpace(revisedOfferedResource) ? OfferedResourceType : revisedOfferedResource.Trim(),
            OfferedQuantity = finalOfferedQty,
            RequestedResourceType = string.IsNullOrWhiteSpace(revisedRequestedResource) ? RequestedResourceType : revisedRequestedResource.Trim(),
            RequestedQuantity = finalRequestedQty,
            CurrencyPayment = finalPayment,
            RevisionNumber = RevisionNumber + 1,
            LastModifiedByCountryId = counteringCountryId.Trim(),
            // Countering party approves new terms; other party's previous approval is invalidated!
            ProposerApproved = isProposerCountering,
            TargetApproved = !isProposerCountering,
            Status = FlexibleTradeStatus.Countered,
            LastModifiedAt = currentTime,
            TermsNotes = revisedNotes ?? TermsNotes,
            StatusMessage = $"Terms revised by {counteringCountryId} (Revision {RevisionNumber + 1}). Prior approvals invalidated."
        };
    }

    /// <summary>
    /// Produces a human-readable summary of the trade package.
    /// </summary>
    public string FormatSummary()
    {
        var gives = new List<string>();
        if (!string.IsNullOrWhiteSpace(OfferedResourceType) && OfferedQuantity > 0)
        {
            gives.Add($"{OfferedQuantity:N0} units of {OfferedResourceType}");
        }
        if (CurrencyPayment > 0)
        {
            gives.Add(RepuTreasury.Format(CurrencyPayment));
        }

        var gets = new List<string>();
        if (!string.IsNullOrWhiteSpace(RequestedResourceType) && RequestedQuantity > 0)
        {
            gets.Add($"{RequestedQuantity:N0} units of {RequestedResourceType}");
        }
        if (CurrencyPayment < 0)
        {
            gets.Add(RepuTreasury.Format(Math.Abs(CurrencyPayment)));
        }

        var givesText = gives.Count > 0 ? string.Join(" + ", gives) : "None";
        var getsText = gets.Count > 0 ? string.Join(" + ", gets) : "None";
        var recurringText = IsRecurring ? $" [Recurring: {RemainingDeliveryCycles}/{TotalDeliveryCycles} cycles]" : " [One-Time Spot]";

        return $"{ProposerCountryId} gives [{givesText}] <---> {TargetCountryId} gives [{getsText}]{recurringText} (Rev {RevisionNumber})";
    }

    /// <inheritdoc />
    public override string ToString() => FormatSummary();
}
