namespace Republic.Core.Client;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Serializable data contract representing a representative listed company for client views.
/// </summary>
public sealed record ListedCompanySnapshot
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("ticker")]
    public string Ticker { get; init; } = string.Empty;

    [JsonPropertyName("market_cap_repu")]
    public double MarketCapRepu { get; init; }

    [JsonPropertyName("share_price_repu")]
    public double SharePriceRepu { get; init; }

    [JsonPropertyName("change_percent")]
    public double ChangePercent { get; init; }
}

/// <summary>
/// Serializable data contract representing an equity market sector sub-index and drivers for client views.
/// </summary>
public sealed record StockMarketSectorSnapshot
{
    [JsonPropertyName("sector_name")]
    public string SectorName { get; init; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; init; } = "📊";

    [JsonPropertyName("current_points")]
    public double CurrentPoints { get; init; }

    [JsonPropertyName("change_percent")]
    public double ChangePercent { get; init; }

    [JsonPropertyName("direction")]
    public string Direction { get; init; } = "Neutral ➡️";

    [JsonPropertyName("key_drivers")]
    public string KeyDrivers { get; init; } = string.Empty;

    [JsonPropertyName("representative_companies")]
    public IReadOnlyList<ListedCompanySnapshot> RepresentativeCompanies { get; init; } = Array.Empty<ListedCompanySnapshot>();
}

