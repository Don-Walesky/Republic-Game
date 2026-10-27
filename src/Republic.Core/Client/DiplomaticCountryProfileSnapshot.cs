namespace Republic.Core.Client;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Client snapshot contract representing a sovereign country's diplomatic profile,
/// providing comparative metrics and distinguishing confirmed data from intelligence estimates.
/// </summary>
public sealed record DiplomaticCountryProfileSnapshot
{
    [JsonPropertyName("country_id")]
    public string CountryId { get; init; } = string.Empty;

    [JsonPropertyName("country_name")]
    public string CountryName { get; init; } = string.Empty;

    [JsonPropertyName("capital_city")]
    public string CapitalCity { get; init; } = string.Empty;

    [JsonPropertyName("government_type")]
    public string GovernmentType { get; init; } = string.Empty;

    [JsonPropertyName("region")]
    public string Region { get; init; } = string.Empty;

    [JsonPropertyName("primary_resource")]
    public string PrimaryResource { get; init; } = string.Empty;

    [JsonPropertyName("is_self")]
    public bool IsSelf { get; init; }

    [JsonPropertyName("is_authorized_intelligence")]
    public bool IsAuthorizedIntelligence { get; init; }

    [JsonPropertyName("treasury_display")]
    public string TreasuryDisplay { get; init; } = string.Empty;

    [JsonPropertyName("treasury_is_confirmed")]
    public bool TreasuryIsConfirmed { get; init; }

    [JsonPropertyName("gdp_display")]
    public string GdpDisplay { get; init; } = string.Empty;

    [JsonPropertyName("approval_display")]
    public string ApprovalDisplay { get; init; } = string.Empty;

    [JsonPropertyName("approval_is_confirmed")]
    public bool ApprovalIsConfirmed { get; init; }

    [JsonPropertyName("stability_display")]
    public string StabilityDisplay { get; init; } = string.Empty;

    [JsonPropertyName("stability_is_confirmed")]
    public bool StabilityIsConfirmed { get; init; }

    [JsonPropertyName("military_readiness_display")]
    public string MilitaryReadinessDisplay { get; init; } = string.Empty;

    [JsonPropertyName("military_is_confirmed")]
    public bool MilitaryIsConfirmed { get; init; }

    [JsonPropertyName("export_opportunities")]
    public IReadOnlyList<string> ExportOpportunities { get; init; } = Array.Empty<string>();

    [JsonPropertyName("import_needs")]
    public IReadOnlyList<string> ImportNeeds { get; init; } = Array.Empty<string>();

    [JsonPropertyName("available_commodities")]
    public IReadOnlyList<string> AvailableCommodities { get; init; } = Array.Empty<string>();

    [JsonPropertyName("diplomatic_relation_status")]
    public string DiplomaticRelationStatus { get; init; } = "Neutral";

    [JsonPropertyName("diplomatic_reputation")]
    public double DiplomaticReputation { get; init; } = 50.0;

    [JsonPropertyName("active_agreements")]
    public IReadOnlyList<string> ActiveAgreements { get; init; } = Array.Empty<string>();
}
