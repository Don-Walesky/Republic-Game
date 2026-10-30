namespace Republic.Core.Identity.Models;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Canonical national development plans establishing inaugural focus areas,
/// infrastructure agendas, and early legislative projects for a new sovereign country.
/// </summary>
public enum NationalDevelopmentPriority
{
    None = 0,

    /// <summary>
    /// Manufacturing, industrial capacity and local production.
    /// </summary>
    IndustrialisationFirst = 1,

    /// <summary>
    /// Agriculture, food production, agribusiness and rural development.
    /// </summary>
    AgriculturalTransformation = 2,

    /// <summary>
    /// Digital infrastructure, research, science, technology and high-tech industries.
    /// </summary>
    TechnologyAndInnovation = 3,

    /// <summary>
    /// Roads, transport, housing, water, power and essential public infrastructure.
    /// </summary>
    InfrastructureRevolution = 4,

    /// <summary>
    /// Education, healthcare, skills development and workforce capability.
    /// </summary>
    HumanCapitalDevelopment = 5,

    /// <summary>
    /// Banking, investment, trade, capital markets and commercial expansion.
    /// </summary>
    FinancialAndCommercialPower = 6,

    /// <summary>
    /// Energy production, mining, strategic minerals and responsible resource management.
    /// </summary>
    EnergyAndNaturalResources = 7,

    /// <summary>
    /// Defence capability, border protection, domestic intelligence and national security.
    /// </summary>
    NationalSecurityAndDefence = 8,

    /// <summary>
    /// Environmental protection, renewable energy, climate resilience and sustainable growth.
    /// </summary>
    SustainableDevelopment = 9,

    /// <summary>
    /// International commerce, foreign investment, diplomatic presence and global partnerships.
    /// </summary>
    GlobalTradeAndInternationalInfluence = 10,

    // Aliases for backward compatibility with previous naming
    MineralIndustrialization = IndustrialisationFirst,
    AgrarianSelfSufficiency = AgriculturalTransformation,
    DigitalInfrastructureCyber = TechnologyAndInnovation,
    TransitRailArteries = InfrastructureRevolution,
    HigherEducationResearch = HumanCapitalDevelopment,
    UniversalHealthcare = HumanCapitalDevelopment,
    FiscalReserveFortress = FinancialAndCommercialPower,
    BorderSecurityDefense = NationalSecurityAndDefence,
    NationalSecurityAndDefense = NationalSecurityAndDefence,
    RenewablePowerGrid = SustainableDevelopment,
    MaritimeTradePorts = GlobalTradeAndInternationalInfluence
}

/// <summary>
/// Domain definition and metadata descriptor for a national development plan.
/// </summary>
public sealed record NationalDevelopmentPlanDefinition(
    NationalDevelopmentPriority Priority,
    string Key,
    string DisplayName,
    string Sector,
    string Focus,
    string Benefit,
    string Challenge,
    IReadOnlyList<string> KeyBenefits,
    string Description = "");

/// <summary>
/// Canonical registry and utility helper for national development plans.
/// </summary>
public static class NationalDevelopmentPlanRegistry
{
    private static readonly Dictionary<NationalDevelopmentPriority, NationalDevelopmentPlanDefinition> Definitions = new()
    {
        [NationalDevelopmentPriority.IndustrialisationFirst] = new(
            NationalDevelopmentPriority.IndustrialisationFirst,
            "industrialisation_first",
            "Industrialisation First",
            "Industry & Manufacturing",
            "Manufacturing, industrial capacity and local production.",
            "Stronger industrial growth, manufacturing output and job creation.",
            "High initial capital costs and environmental impacts.",
            new[] { "+20% Mineral Processing Capacity", "+15% Industrial Goods Export Value", "+10% Factory Jobs" },
            "Constructs state smelters, raw mineral processing mills, and heavy equipment plants to build foundational factory capacity."),

        [NationalDevelopmentPriority.AgriculturalTransformation] = new(
            NationalDevelopmentPriority.AgriculturalTransformation,
            "agricultural_transformation",
            "Agricultural Transformation",
            "Agriculture & Food Security",
            "Agriculture, food production, agribusiness and rural development.",
            "Improved food security, rural prosperity and agricultural exports.",
            "Vulnerability to weather shocks and slower industrial transition.",
            new[] { "+15% Food Security", "+10% Rural Stability", "-10% Food Import Costs" },
            "Subsidizes domestic crop yields, modern grain storage, and rural farming cooperatives to eliminate food import dependencies."),

        [NationalDevelopmentPriority.TechnologyAndInnovation] = new(
            NationalDevelopmentPriority.TechnologyAndInnovation,
            "technology_and_innovation",
            "Technology and Innovation",
            "Technology & Digital",
            "Digital infrastructure, research, science, technology and high-tech industries.",
            "High-value economic growth, technological capability and modern public services.",
            "High skill requirements and risks of digital inequality.",
            new[] { "+20% Digital Government Capacity", "+20% Science & Tech Research Output", "+30% Cyber Attack Resilience" },
            "Lays trans-regional fiber-optic backbones, establishes sovereign cloud data centers, and trains high-tech research teams."),

        [NationalDevelopmentPriority.InfrastructureRevolution] = new(
            NationalDevelopmentPriority.InfrastructureRevolution,
            "infrastructure_revolution",
            "Infrastructure Revolution",
            "Transport & Public Works",
            "Roads, transport, housing, water, power and essential public infrastructure.",
            "Strong foundation for economic productivity, trade and connectivity.",
            "Significant public debt or strain on public finances.",
            new[] { "+25% Inter-City Transport Speed", "-15% Domestic Logistics Transport Costs", "+15% Commercial Connectivity" },
            "Connects industrial basins to domestic cities via high-capacity electrified heavy rail, highways, and logistics terminals."),

        [NationalDevelopmentPriority.HumanCapitalDevelopment] = new(
            NationalDevelopmentPriority.HumanCapitalDevelopment,
            "human_capital_development",
            "Human Capital Development",
            "Education & Healthcare",
            "Education, healthcare, skills development and workforce capability.",
            "A healthier, more skilled, more productive workforce.",
            "Long lead time before major economic returns are realised.",
            new[] { "+15% Citizen Welfare Rating", "+10% High-Skill Labor Pool", "+10% Demographic Health Resilience" },
            "Funds technical universities, postgraduate STEM research, and regional public health clinics."),

        [NationalDevelopmentPriority.FinancialAndCommercialPower] = new(
            NationalDevelopmentPriority.FinancialAndCommercialPower,
            "financial_and_commercial_power",
            "Financial and Commercial Power",
            "Banking & Capital Markets",
            "Banking, investment, trade, capital markets and commercial expansion.",
            "Attracting capital, growing national wealth and building international influence.",
            "Vulnerability to global market volatility and financial risk.",
            new[] { "+10% Treasury Interest Yield", "+20% Macroeconomic Shock Mitigation", "+15% Foreign Direct Investment" },
            "Accumulates sovereign bullion reserves, regulates capital cushions, and deepens commercial liquidity."),

        [NationalDevelopmentPriority.EnergyAndNaturalResources] = new(
            NationalDevelopmentPriority.EnergyAndNaturalResources,
            "energy_and_natural_resources",
            "Energy and Natural Resources",
            "Energy & Mining",
            "Energy production, mining, strategic minerals and responsible resource management.",
            "Reliable domestic energy, export revenues and strategic importance.",
            "Dependence on commodity price cycles and environmental degradation.",
            new[] { "+20% Resource Extraction Efficiency", "+25% Domestic Energy Independence", "+15% Strategic Mineral Reserves" },
            "Explores strategic mineral reserves and builds stable national power generation facilities."),

        [NationalDevelopmentPriority.NationalSecurityAndDefence] = new(
            NationalDevelopmentPriority.NationalSecurityAndDefence,
            "national_security_and_defence",
            "National Security and Defence",
            "Defence & Intelligence",
            "Defence capability, border protection, domestic intelligence and national security.",
            "Greater sovereign protection, regional deterrent power and stability.",
            "High ongoing security expenditure with lower direct economic returns.",
            new[] { "+15% DEFCON Operational Readiness", "+25% Counter-Espionage Detection", "+10% Territorial Integrity" },
            "Fortifies strategic border crossings, radar observation outposts, and mobile rapid-reaction battalions."),

        [NationalDevelopmentPriority.SustainableDevelopment] = new(
            NationalDevelopmentPriority.SustainableDevelopment,
            "sustainable_development",
            "Sustainable Development",
            "Environment & Clean Energy",
            "Environmental protection, renewable energy, climate resilience and sustainable growth.",
            "Long-term ecological resilience, green jobs and international goodwill.",
            "Potential short-term friction with traditional resource extraction.",
            new[] { "+25% Clean Energy Output", "+15% Ecological Stability", "+10% Green Sector Employment" },
            "Deploys solar, wind, and hydroelectric installations integrated with grid storage for ecological longevity."),

        [NationalDevelopmentPriority.GlobalTradeAndInternationalInfluence] = new(
            NationalDevelopmentPriority.GlobalTradeAndInternationalInfluence,
            "global_trade_and_international_influence",
            "Global Trade and International Influence",
            "Trade & Foreign Affairs",
            "International commerce, foreign investment, diplomatic presence and global partnerships.",
            "Expanded export markets, diplomatic weight and foreign capital.",
            "Greater exposure to geopolitical shocks and foreign market dependencies.",
            new[] { "+20% Maritime Export Throughput", "+15% Customs Tariff Revenue", "+15% International Diplomatic Standing" },
            "Dredges deepwater container ports, automates customs clearance, and expands merchant harbor berths for high-volume commerce.")
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

        // Fuzzy and shorthand matches for canonical 10 plans & backward compatibility
        if (normalized.Contains("industrial") || normalized.Contains("smelt") || normalized.Contains("mineral") || normalized.Contains("manufactur"))
        {
            definition = Definitions[NationalDevelopmentPriority.IndustrialisationFirst];
            return true;
        }
        if (normalized.Contains("agrarian") || normalized.Contains("agri") || normalized.Contains("food") || normalized.Contains("farming"))
        {
            definition = Definitions[NationalDevelopmentPriority.AgriculturalTransformation];
            return true;
        }
        if (normalized.Contains("tech") || normalized.Contains("cyber") || normalized.Contains("digital") || normalized.Contains("innovation"))
        {
            definition = Definitions[NationalDevelopmentPriority.TechnologyAndInnovation];
            return true;
        }
        if (normalized.Contains("infra") || normalized.Contains("rail") || normalized.Contains("transit") || normalized.Contains("transport") || normalized.Contains("road"))
        {
            definition = Definitions[NationalDevelopmentPriority.InfrastructureRevolution];
            return true;
        }
        if (normalized.Contains("human_capital") || normalized.Contains("education") || normalized.Contains("health") || normalized.Contains("academic") || normalized.Contains("workforce"))
        {
            definition = Definitions[NationalDevelopmentPriority.HumanCapitalDevelopment];
            return true;
        }
        if (normalized.Contains("financial") || normalized.Contains("commercial") || normalized.Contains("bank") || normalized.Contains("fiscal") || normalized.Contains("reserve"))
        {
            definition = Definitions[NationalDevelopmentPriority.FinancialAndCommercialPower];
            return true;
        }
        if (normalized.Contains("energy") || normalized.Contains("natural_resource") || normalized.Contains("mining") || normalized.Contains("mineral_reserves"))
        {
            definition = Definitions[NationalDevelopmentPriority.EnergyAndNaturalResources];
            return true;
        }
        if (normalized.Contains("security") || normalized.Contains("defence") || normalized.Contains("defense") || normalized.Contains("border"))
        {
            definition = Definitions[NationalDevelopmentPriority.NationalSecurityAndDefence];
            return true;
        }
        if (normalized.Contains("sustainable") || normalized.Contains("renewable") || normalized.Contains("green") || normalized.Contains("climate") || normalized.Contains("eco"))
        {
            definition = Definitions[NationalDevelopmentPriority.SustainableDevelopment];
            return true;
        }
        if (normalized.Contains("global_trade") || normalized.Contains("international") || normalized.Contains("maritime") || normalized.Contains("port") || normalized.Contains("commerce"))
        {
            definition = Definitions[NationalDevelopmentPriority.GlobalTradeAndInternationalInfluence];
            return true;
        }

        return false;
    }
}

