namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Client snapshot contract representing a ratified bilateral treaty or completed trade deal.
/// </summary>
public sealed record DiplomaticAgreementSnapshot
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("partner_country_id")]
    public string PartnerCountryId { get; init; } = string.Empty;

    [JsonPropertyName("partner_country_name")]
    public string PartnerCountryName { get; init; } = string.Empty;

    [JsonPropertyName("agreement_type")]
    public string AgreementType { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("proposed_boundary")]
    public string ProposedBoundary { get; init; } = string.Empty;

    [JsonPropertyName("decided_boundary")]
    public string DecidedBoundary { get; init; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; init; } = string.Empty;
}
