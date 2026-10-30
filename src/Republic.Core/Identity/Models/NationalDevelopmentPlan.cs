namespace Republic.Core.Identity.Models;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Canonical national development priorities establishing inaugural focus areas,
/// infrastructure agendas, and early legislative projects for a new sovereign country.
/// </summary>
public enum NationalDevelopmentPriority
{
    None = 0,

    /// <summary>
    /// Food security, rural farming cooperatives, and grain stockpile resilience.
    /// </summary>
    AgrarianSelfSufficiency = 1,

    /// <summary>
    /// Factory capacity, mineral smelting mills, and raw material processing throughput.
    /// </summary>
    MineralIndustrialization = 2,

    /// <summary>
    /// Solar, hydro, and wind grid resilience with energy independence.
    /// </summary>
    RenewablePowerGrid = 3,

    /// <summary>
    /// Technical universities, academic research grants, and STEM innovation throughput.
    /// </summary>
    HigherEducationResearch = 4,

    /// <summary>
    /// Border observation posts, DEFCON readiness, and territorial counter-espionage.
    /// </summary>
    BorderSecurityDefense = 5,

    /// <summary>
    /// Deepwater shipping berths, container terminals, and export customs revenues.
    /// </summary>
    MaritimeTradePorts = 6,

    /// <summary>
    /// Fiber-optic networks, sovereign cloud data centers, and cyber defense systems.
    /// </summary>
    DigitalInfrastructureCyber = 7,

    /// <summary>
    /// Regional hospitals, emergency care access, and citizen demographic health.
    /// </summary>
    UniversalHealthcare = 8,

    /// <summary>
    /// Bullion stockpiles, treasury interest yields, and economic buffer resilience.
    /// </summary>
    FiscalReserveFortress = 9,

    /// <summary>
    /// Electrified freight and passenger rail corridors connecting inland manufacturing.
    /// </summary>
    TransitRailArteries = 10
}

/// <summary>
/// Domain definition and metadata descriptor for a national development priority.
/// </summary>
public sealed record NationalDevelopmentPlanDefinition(
    NationalDevelopmentPriority Priority,
    string Key,
    string DisplayName,
    string Sector,
    string Description,
    IReadOnlyList<string> KeyBenefits);

/// <summary>
/// Canonical registry and utility helper for national development priorities.
/// </summary>
public static class NationalDevelopmentPlanRegistry
{
    private static readonly Dictionary<NationalDevelopmentPriority, NationalDevelopmentPlanDefinition> Definitions = new()
    {
        [NationalDevelopmentPriority.AgrarianSelfSufficiency] = new(
            NationalDevelopmentPriority.AgrarianSelfSufficiency,
            "agrarian_self_sufficiency",
            "Agrarian Self-Sufficiency",
            "Agriculture & Food Security",
            "Subsidizes domestic crop yields, modern grain storage, and rural farming cooperatives to eliminate food import dependencies.",
            new[] { "+15% Food Security", "+10% Rural Stability", "-10% Food Import Costs" }),

        [NationalDevelopmentPriority.MineralIndustrialization] = new(
            NationalDevelopmentPriority.MineralIndustrialization,
            "mineral_industrialization",
            "Mineral & Heavy Industrialization",
            "Industry & Mining",
            "Constructs state smelters, raw mineral processing mills, and heavy equipment plants to build foundational factory capacity.",
            new[] { "+20% Mineral Processing Capacity", "+15% Industrial Goods Export Value" }),

        [NationalDevelopmentPriority.RenewablePowerGrid] = new(
            NationalDevelopmentPriority.RenewablePowerGrid,
            "renewable_power_grid",
            "Renewable Power Grid & Energy Sovereignty",
            "Energy & Utilities",
            "Deploys solar, wind, and hydroelectric installations integrated with high-capacity battery storage for zero-carbon grid sovereignty.",
            new[] { "+25% Clean Energy Output", "+15% Electrical Grid Reliability" }),

        [NationalDevelopmentPriority.HigherEducationResearch] = new(
            NationalDevelopmentPriority.HigherEducationResearch,
            "higher_education_research",
            "Higher Education & Academic Research",
            "Education & Science",
            "Funds technical universities, postgraduate research centers, and competitive STEM fellowships to multiply national innovation.",
            new[] { "+20% Science & Tech Research Output", "+10% High-Skill Labor Pool" }),

        [NationalDevelopmentPriority.BorderSecurityDefense] = new(
            NationalDevelopmentPriority.BorderSecurityDefense,
            "border_security_defense",
            "Border Security & Territorial Defense",
            "Defense & Border",
            "Fortifies strategic border crossings, radar observation outposts, and mobile rapid-reaction battalions against foreign infiltration.",
            new[] { "+15% DEFCON Operational Readiness", "+25% Counter-Espionage Detection" }),

        [NationalDevelopmentPriority.MaritimeTradePorts] = new(
            NationalDevelopmentPriority.MaritimeTradePorts,
            "maritime_trade_ports",
            "Maritime Trade & Deepwater Ports",
            "Trade & Logistics",
            "Dredges deepwater container ports, automates customs clearance, and expands merchant harbor berths for high-volume ocean commerce.",
            new[] { "+20% Maritime Export Throughput", "+15% Customs Tariff Revenue" }),

        [NationalDevelopmentPriority.DigitalInfrastructureCyber] = new(
            NationalDevelopmentPriority.DigitalInfrastructureCyber,
            "digital_infrastructure_cyber",
            "Digital Infrastructure & Cyber Defense",
            "Technology & Cyber",
            "Lays trans-regional fiber-optic backbones, establishes sovereign cloud data centers, and trains elite cyber defensive response units.",
            new[] { "+20% Digital Government Capacity", "+30% Cyber Attack Resilience" }),

        [NationalDevelopmentPriority.UniversalHealthcare] = new(
            NationalDevelopmentPriority.UniversalHealthcare,
            "universal_healthcare",
            "Universal Healthcare Foundation",
            "Health & Social Welfare",
            "Builds regional clinics, subsidizes essential pharmaceutical supplies, and guarantees emergency medical care for every citizen.",
            new[] { "+15% Citizen Welfare Rating", "+10% Demographic Health Resilience" }),

        [NationalDevelopmentPriority.FiscalReserveFortress] = new(
            NationalDevelopmentPriority.FiscalReserveFortress,
            "fiscal_reserve_fortress",
            "Fiscal Reserve Fortress",
            "Finance & Reserves",
            "Accumulates sovereign bullion reserves, regulates counter-cyclical capital cushions, and earns yields on unencumbered treasury holdings.",
            new[] { "+10% Treasury Interest Yield", "+20% Macroeconomic Shock Mitigation" }),

        [NationalDevelopmentPriority.TransitRailArteries] = new(
            NationalDevelopmentPriority.TransitRailArteries,
            "transit_rail_arteries",
            "High-Speed Transit & Rail Arteries",
            "Transport & Public Works",
            "Connects industrial manufacturing basins to domestic cities via high-capacity electrified heavy rail and logistics terminals.",
            new[] { "+25% Inter-City Transport Speed", "-15% Domestic Logistics Transport Costs" })
    };

    public static IReadOnlyList<NationalDevelopmentPlanDefinition> All { get; } = Definitions.Values.ToList();

    public static NationalDevelopmentPlanDefinition Get(NationalDevelopmentPriority priority)
    {
        if (Definitions.TryGetValue(priority, out var def))
        {
            return def;
        }

        throw new KeyNotFoundException($"No national development priority definition found for '{priority}'.");
    }

    public static bool TryGet(string input, out NationalDevelopmentPlanDefinition? definition)
    {
        definition = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var normalized = input.Trim().ToLowerInvariant().Replace("-", "_").Replace(" ", "_");

        foreach (var def in Definitions.Values)
        {
            if (string.Equals(def.Key, normalized, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(def.DisplayName, input.Trim(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(def.Priority.ToString(), input.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                definition = def;
                return true;
            }
        }

        // Fuzzy matches for convenient shorthand
        if (normalized.Contains("agrarian") || normalized.Contains("food") || normalized.Contains("farming"))
        {
            definition = Definitions[NationalDevelopmentPriority.AgrarianSelfSufficiency];
            return true;
        }
        if (normalized.Contains("mineral") || normalized.Contains("industrial") || normalized.Contains("smelt"))
        {
            definition = Definitions[NationalDevelopmentPriority.MineralIndustrialization];
            return true;
        }
        if (normalized.Contains("renewable") || normalized.Contains("power") || normalized.Contains("energy"))
        {
            definition = Definitions[NationalDevelopmentPriority.RenewablePowerGrid];
            return true;
        }
        if (normalized.Contains("education") || normalized.Contains("research") || normalized.Contains("academic"))
        {
            definition = Definitions[NationalDevelopmentPriority.HigherEducationResearch];
            return true;
        }
        if (normalized.Contains("border") || normalized.Contains("defense") || normalized.Contains("territorial"))
        {
            definition = Definitions[NationalDevelopmentPriority.BorderSecurityDefense];
            return true;
        }
        if (normalized.Contains("maritime") || normalized.Contains("port") || normalized.Contains("harbor"))
        {
            definition = Definitions[NationalDevelopmentPriority.MaritimeTradePorts];
            return true;
        }
        if (normalized.Contains("cyber") || normalized.Contains("digital") || normalized.Contains("fiber"))
        {
            definition = Definitions[NationalDevelopmentPriority.DigitalInfrastructureCyber];
            return true;
        }
        if (normalized.Contains("health") || normalized.Contains("hospital") || normalized.Contains("clinic"))
        {
            definition = Definitions[NationalDevelopmentPriority.UniversalHealthcare];
            return true;
        }
        if (normalized.Contains("fiscal") || normalized.Contains("reserve") || normalized.Contains("fortress") || normalized.Contains("bullion"))
        {
            definition = Definitions[NationalDevelopmentPriority.FiscalReserveFortress];
            return true;
        }
        if (normalized.Contains("rail") || normalized.Contains("transit") || normalized.Contains("artery") || normalized.Contains("train"))
        {
            definition = Definitions[NationalDevelopmentPriority.TransitRailArteries];
            return true;
        }

        return false;
    }
}
