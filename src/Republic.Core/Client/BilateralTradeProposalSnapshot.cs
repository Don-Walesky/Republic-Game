namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Client snapshot contract representing a flexible bilateral trade proposal or active contract,
/// detailing participating countries, offered goods, requested commodities, currency, and revision state.
/// </summary>
public sealed record BilateralTradeProposalSnapshot
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("proposer_country_id")]
    public string ProposerCountryId { get; init; } = string.Empty;

    [JsonPropertyName("proposer_country_name")]
    public string ProposerCountryName { get; init; } = string.Empty;

    [JsonPropertyName("target_country_id")]
    public string TargetCountryId { get; init; } = string.Empty;

    [JsonPropertyName("target_country_name")]
    public string TargetCountryName { get; init; } = string.Empty;

    [JsonPropertyName("offered_resource_type")]
    public string? OfferedResourceType { get; init; }

    [JsonPropertyName("offered_quantity")]
    public double OfferedQuantity { get; init; }

    [JsonPropertyName("requested_resource_type")]
    public string? RequestedResourceType { get; init; }

    [JsonPropertyName("requested_quantity")]
    public double RequestedQuantity { get; init; }

    [JsonPropertyName("currency_payment")]
    public double CurrencyPayment { get; init; }

    [JsonPropertyName("currency_payment_formatted")]
    public string CurrencyPaymentFormatted { get; init; } = string.Empty;

    [JsonPropertyName("is_recurring")]
    public bool IsRecurring { get; init; }

    [JsonPropertyName("total_delivery_cycles")]
    public int TotalDeliveryCycles { get; init; } = 1;

    [JsonPropertyName("remaining_delivery_cycles")]
    public int RemainingDeliveryCycles { get; init; } = 1;

    [JsonPropertyName("revision_number")]
    public int RevisionNumber { get; init; } = 1;

    [JsonPropertyName("last_modified_by_country_id")]
    public string LastModifiedByCountryId { get; init; } = string.Empty;

    [JsonPropertyName("can_accept")]
    public bool CanAccept { get; init; }

    [JsonPropertyName("can_counter")]
    public bool CanCounter { get; init; }

    [JsonPropertyName("can_reject")]
    public bool CanReject { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; init; } = string.Empty;

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; init; } = string.Empty;

    [JsonPropertyName("status_message")]
    public string StatusMessage { get; init; } = string.Empty;
}
