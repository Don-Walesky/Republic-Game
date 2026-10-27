namespace Republic.Core.World;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Republic.Core.Economy.Treasury;
using Republic.Core.Time;
using Republic.Core.World.Factories;
using Republic.Core.World.Models;

/// <summary>
/// Domain model providing an authoritative diplomatic profile of a sovereign country,
/// tailored specifically for bilateral comparisons and international negotiations.
/// Enforces information classification access rules: confirmed exact figures are only exposed
/// for the player's own nation or formally allied partners; foreign confidential intelligence is estimated.
/// </summary>
public sealed record DiplomaticCountryProfile
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

    /// <summary>
    /// Creates a comparative diplomatic country profile adhering strictly to simulation state and information access rules.
    /// </summary>
    public static DiplomaticCountryProfile Create(
        Country viewingCountry,
        Country targetCountry,
        RepublicTime currentTime)
    {
        ArgumentNullException.ThrowIfNull(viewingCountry);
        ArgumentNullException.ThrowIfNull(targetCountry);

        bool isSelf = string.Equals(viewingCountry.Id, targetCountry.Id, StringComparison.OrdinalIgnoreCase);

        // Check if viewing country has an alliance or formal treaty with the target country
        var relation = viewingCountry.GetRelation(targetCountry.Id);
        bool hasAlliance = viewingCountry.HasActiveAgreement(targetCountry.Id, DiplomaticAgreementType.Alliance);
        bool isAuthorized = isSelf || hasAlliance;

        // Treasury classification
        string treasuryDisplay;
        bool treasuryConfirmed;
        if (isAuthorized)
        {
            treasuryDisplay = RepuTreasury.Format(targetCountry.Treasury.Balance) + " (Confirmed)";
            treasuryConfirmed = true;
        }
        else
        {
            var bal = targetCountry.Treasury.Balance;
            string tier = bal switch
            {
                >= 10_000_000.0 => "Very High (R 10M+)",
                >= 3_000_000.0 => "High (R 3M - R 10M)",
                >= 1_000_000.0 => "Moderate (R 1M - R 3M)",
                >= 500_000.0 => "Cautious (R 500K - R 1M)",
                _ => "Fiscal Strain (< R 500K)"
            };
            treasuryDisplay = $"Estimated Reserve: {tier} [Classified Intelligence]";
            treasuryConfirmed = false;
        }

        // GDP public indicator
        var gdpDisplay = RepuTreasury.Format(targetCountry.GrossDomesticProduct);

        // Approval & Stability classification
        string approvalDisplay;
        bool approvalConfirmed;
        string stabilityDisplay;
        bool stabilityConfirmed;

        if (isAuthorized)
        {
            approvalDisplay = $"{targetCountry.HappinessRating:0.0}% (Confirmed)";
            approvalConfirmed = true;
            stabilityDisplay = $"{targetCountry.BaselineStability:0.0}% (Confirmed)";
            stabilityConfirmed = true;
        }
        else
        {
            // Disclose bracketed estimate for foreign nations
            var appBracket = Math.Round(targetCountry.HappinessRating / 5.0) * 5.0;
            approvalDisplay = $"Estimated Sentiment: ~{appBracket:0}% (Public Polling Estimate)";
            approvalConfirmed = false;

            var stabBracket = Math.Round(targetCountry.BaselineStability / 5.0) * 5.0;
            stabilityDisplay = $"Estimated Stability: ~{stabBracket:0}% (External Observer Estimate)";
            stabilityConfirmed = false;
        }

        // Military Readiness classification
        string milDisplay;
        bool milConfirmed;
        if (isAuthorized)
        {
            milDisplay = "85.0% Operational (Peacetime Readiness Confirmed)";
            milConfirmed = true;
        }
        else
        {
            milDisplay = "Armed Forces Alert: Peacetime Watch [Classified Defense Data]";
            milConfirmed = false;
        }

        // Export Opportunities: actual commodities where target has extraction nodes or stock
        var exportOpportunities = new List<string>();
        var activeNodes = targetCountry.ResourceNodes ?? new List<ResourceNode>();
        foreach (var node in activeNodes.Where(n => n.IsActive))
        {
            if (!exportOpportunities.Contains(node.ResourceType))
            {
                exportOpportunities.Add(node.ResourceType);
            }
        }

        // Add any commodities with positive stockpile
        var stock = targetCountry.Inventory?.Stockpiles ?? new Dictionary<string, double>();
        foreach (var (res, qty) in stock)
        {
            if (qty > 0 && !exportOpportunities.Contains(res))
            {
                exportOpportunities.Add(res);
            }
        }

        if (exportOpportunities.Count == 0 && !string.IsNullOrWhiteSpace(targetCountry.Profile?.PrimaryResource))
        {
            exportOpportunities.Add(targetCountry.Profile.PrimaryResource);
        }

        // Import Needs: critical natural resources that the nation does not domestically extract
        var importNeeds = new List<string>();
        string[] standardResources = { "Iron", "Petroleum", "Bauxite", "Natural Gas", "Copper", "Rare Earths", "Timber", "Uranium", "Gold", "Lithium" };
        foreach (var res in standardResources)
        {
            bool hasNode = activeNodes.Any(n => string.Equals(n.ResourceType, res, StringComparison.OrdinalIgnoreCase) && n.IsActive);
            double qty = targetCountry.Inventory?.GetQuantity(res) ?? 0.0;
            if (!hasNode && qty < 250.0)
            {
                importNeeds.Add(res);
            }
        }

        // Available Commodities: list with quantities if authorized; list without exact numbers if foreign
        var availableCommodities = new List<string>();
        if (isAuthorized)
        {
            foreach (var (res, qty) in stock)
            {
                if (qty > 0)
                {
                    availableCommodities.Add($"{res}: {qty:N0} units");
                }
            }
        }
        else
        {
            foreach (var res in exportOpportunities)
            {
                availableCommodities.Add($"{res} (Surplus Available)");
            }
        }

        // Diplomatic Status
        string relationStatus = "Neutral (No Relation)";
        if (relation != null)
        {
            relationStatus = relation.Status.ToString();
        }
        if (hasAlliance)
        {
            relationStatus = "Allied Sovereign";
        }

        // Active Agreements
        var activeAgreements = targetCountry.Agreements
            .Where(a => a.Involves(viewingCountry.Id) && a.IsActive)
            .Select(a => $"{a.AgreementType} Agreement ({a.Status})")
            .ToList();

        var primaryRes = targetCountry.Profile?.PrimaryResource ?? "Diversified";
        var regionName = targetCountry.Profile?.Region.ToString() ?? "Global";

        return new DiplomaticCountryProfile
        {
            CountryId = targetCountry.Id,
            CountryName = targetCountry.Name,
            CapitalCity = targetCountry.CapitalCity,
            GovernmentType = targetCountry.GovernmentType,
            Region = regionName,
            PrimaryResource = primaryRes,
            IsSelf = isSelf,
            IsAuthorizedIntelligence = isAuthorized,
            TreasuryDisplay = treasuryDisplay,
            TreasuryIsConfirmed = treasuryConfirmed,
            GdpDisplay = gdpDisplay,
            ApprovalDisplay = approvalDisplay,
            ApprovalIsConfirmed = approvalConfirmed,
            StabilityDisplay = stabilityDisplay,
            StabilityIsConfirmed = stabilityConfirmed,
            MilitaryReadinessDisplay = milDisplay,
            MilitaryIsConfirmed = milConfirmed,
            ExportOpportunities = exportOpportunities.AsReadOnly(),
            ImportNeeds = importNeeds.AsReadOnly(),
            AvailableCommodities = availableCommodities.AsReadOnly(),
            DiplomaticRelationStatus = relationStatus,
            DiplomaticReputation = relation?.IsRecognized == true ? 70.0 : 50.0,
            ActiveAgreements = activeAgreements.AsReadOnly()
        };
    }
}
