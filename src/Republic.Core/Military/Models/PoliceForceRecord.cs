namespace Republic.Core.Military.Models;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Domain model encapsulating the national police force and law enforcement command.
/// Governs domestic crime suppression, municipal safety, community order, and riot containment.
/// </summary>
public sealed class PoliceForceRecord
{
    [JsonPropertyName("is_established")]
    public bool IsEstablished { get; set; } = true;

    [JsonPropertyName("force_name")]
    public string ForceName { get; set; } = "Republic National Police Service (RNPS)";

    [JsonPropertyName("officer_count")]
    public int OfficerCount { get; set; } = 28000;

    [JsonPropertyName("quarterly_funding")]
    public double QuarterlyFunding { get; set; } = 45_000_000.0; // R / Qtr

    [JsonPropertyName("crime_rate_index")]
    public double CrimeRateIndex => Math.Clamp(Math.Round(50.0 - (OfficerCount / 1000.0 * 0.8) - (QuarterlyFunding / 2_000_000.0 * 0.5), 1), 5.0, 85.0);

    [JsonPropertyName("public_order_score")]
    public double PublicOrderScore => Math.Clamp(Math.Round(50.0 + (OfficerCount / 1000.0 * 1.0) + (QuarterlyFunding / 2_000_000.0 * 0.6) - (CrimeRateIndex * 0.3), 1), 10.0, 98.0);

    [JsonPropertyName("community_trust_score")]
    public double CommunityTrustScore { get; set; } = 72.0;

    [JsonPropertyName("status_summary")]
    public string StatusSummary => PublicOrderScore >= 75.0
        ? "Civil Order Secure 🟢: Professional police presence maintains low crime and swift municipal emergency response."
        : (PublicOrderScore >= 50.0
            ? "Moderate Law Enforcement 🟡: City centers generally secure, but under-policed suburbs experience localized property theft."
            : "Under-Policed Crisis 🔴: Acute officer shortages and funding cuts result in elevated crime waves and public vulnerability.");

    /// <summary>
    /// Expands the police force with newly recruited and trained sworn officers.
    /// </summary>
    public void ExpandForce(int additionalOfficers, double additionalFunding = 0.0)
    {
        if (additionalOfficers <= 0) return;
        OfficerCount += additionalOfficers;
        if (additionalFunding > 0.0)
        {
            QuarterlyFunding += additionalFunding;
        }
        CommunityTrustScore = Math.Clamp(CommunityTrustScore + 1.5, 10.0, 100.0);
    }

    /// <summary>
    /// Adjusts police operational and fleet funding.
    /// </summary>
    public void SetFunding(double newQuarterlyFunding)
    {
        QuarterlyFunding = Math.Max(15_000_000.0, newQuarterlyFunding);
        if (QuarterlyFunding >= 65_000_000.0)
        {
            CommunityTrustScore = Math.Clamp(CommunityTrustScore + 2.0, 10.0, 100.0);
        }
        else if (QuarterlyFunding < 25_000_000.0)
        {
            CommunityTrustScore = Math.Clamp(CommunityTrustScore - 4.0, 10.0, 100.0);
        }
    }
}

