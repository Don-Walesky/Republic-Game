using System.Text.Json.Serialization;

namespace Republic.Core.Government;

public enum CampaignChoice
{
    AcceptDemand,
    CounterOffer,
    GentlemensAgreement,
    Decline,
    Modify,
}

public enum DefectionResponse
{
    ConcedePatronage,
    LetThemGo,
    LaunchPurge,
}

public enum AllianceType
{
    Economic,
    Security,
    Mixed,
}

public enum IncidentSeverity
{
    Low,
    Medium,
    High,
}

public sealed class ProceduralNationProfile
{
    public string GovernmentStructure { get; set; } = string.Empty;
    public string Geography { get; set; } = string.Empty;
    public string Demographics { get; set; } = string.Empty;
    public string PoliticalCulture { get; set; } = string.Empty;
    public string Constitution { get; set; } = string.Empty;
    public int RegionalStrength { get; set; }
    public int WorldPower { get; set; }
}

public sealed class WorldCountry
{
    public string Name { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public bool IsAiControlled { get; set; }
    public string PresidentName { get; set; } = string.Empty;
    public decimal EconomyPower { get; set; }
    public decimal MilitaryPower { get; set; }
    public decimal DiplomaticPower { get; set; }
    public decimal CurrencyStrength { get; set; }
    public decimal ProxyInfluenceFromPlayer { get; set; }
    public bool Leaderless { get; set; }
}

public sealed class PeaceTreaty
{
    public string Name { get; set; } = string.Empty;
    public bool MandatoryNoWar { get; set; }
    public List<string> Signatories { get; set; } = new();
}

public sealed class GeopoliticalAlliance
{
    public string Name { get; set; } = string.Empty;
    public AllianceType Type { get; set; }
    public List<string> Members { get; set; } = new();
    public bool IncludesNoWarClause { get; set; }
}

public sealed class MilitaryIncident
{
    public string InitiatorCountry { get; set; } = string.Empty;
    public string TargetCountry { get; set; } = string.Empty;
    public string IncidentType { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public bool EscalationPreventedByTreaty { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}

public sealed class PlayerPresence
{
    public bool Online { get; set; } = true;
    public int ConsecutiveOfflineTurns { get; set; }
    public bool GovernmentToppled { get; set; }
    public string CaretakerPresident { get; set; } = string.Empty;
    public List<string> ProxyControlCountries { get; set; } = new();
}

public sealed class CampaignAccountantGeneral
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "Kaelen Thorne";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "Campaign Accountant General";

    [JsonPropertyName("role")]
    public string Role { get; set; } = "Purse Guardian & Financial Compliance";

    [JsonPropertyName("initial_purse")]
    public decimal InitialPurse { get; set; } = 5_000_000m;

    [JsonPropertyName("current_purse")]
    public decimal CurrentPurse { get; set; } = 5_000_000m;

    [JsonPropertyName("total_expenditures")]
    public decimal TotalExpenditures { get; set; }

    [JsonPropertyName("audit_status")]
    public string AuditStatus { get; set; } = "Clean Audit — Campaign expenditures compliant with sovereign election laws.";

    [JsonPropertyName("financial_advice")]
    public string FinancialAdvice { get; set; } = "Strict financial controls in place. Ensure sufficient reserves remain for Election Night operations.";
}

public sealed class CampaignDecision
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("foreseeable_positive_consequence")]
    public string ForeseeablePositiveConsequence { get; set; } = string.Empty;

    [JsonPropertyName("foreseeable_negative_consequence")]
    public string ForeseeableNegativeConsequence { get; set; } = string.Empty;

    [JsonPropertyName("cost_repu")]
    public decimal CostRepu { get; set; }

    [JsonPropertyName("polling_impact")]
    public decimal PollingImpact { get; set; }

    [JsonPropertyName("funding_impact")]
    public decimal FundingImpact { get; set; }

    [JsonPropertyName("approval_impact")]
    public decimal ApprovalImpact { get; set; }

    [JsonPropertyName("stability_impact")]
    public decimal StabilityImpact { get; set; }

    [JsonPropertyName("is_net_positive")]
    public bool IsNetPositive { get; set; } = true;

    [JsonPropertyName("executed")]
    public bool Executed { get; set; }

    [JsonPropertyName("outcome_explanation")]
    public string OutcomeExplanation { get; set; } = string.Empty;

    public static List<CampaignDecision> CreateCanonicalDecisions()
    {
        return new List<CampaignDecision>
        {
            new()
            {
                Id = "dec_meet_business",
                Title = "Meet Business Association",
                Category = "Stakeholder",
                Description = "Convene a closed-door roundtable with the National Chamber of Commerce and industrial employers.",
                ForeseeablePositiveConsequence = "Major corporate endorsement and potential campaign donation (+R 800,000 REPU, +5% Polling).",
                ForeseeableNegativeConsequence = "Policy concession expectations on business deregulation; mild union criticism (-1% Stability).",
                CostRepu = 200_000m,
                PollingImpact = 5.0m,
                FundingImpact = 800_000m,
                ApprovalImpact = 1.5m,
                StabilityImpact = -1.0m,
                IsNetPositive = true
            },
            new()
            {
                Id = "dec_meet_labor",
                Title = "Meet Labour Union",
                Category = "Stakeholder",
                Description = "Address the Central Federation of Trade Unions at their annual delegate congress.",
                ForeseeablePositiveConsequence = "Organised volunteer mobilization and strong working-class endorsement (+6% Polling, +3% Approval).",
                ForeseeableNegativeConsequence = "Binding living-wage commitments and campaign co-sponsorship logistics fees (-R 350,000 REPU).",
                CostRepu = 350_000m,
                PollingImpact = 6.0m,
                FundingImpact = 0m,
                ApprovalImpact = 3.0m,
                StabilityImpact = 1.0m,
                IsNetPositive = true
            },
            new()
            {
                Id = "dec_media_blitz",
                Title = "National Prime-Time Media Blitz",
                Category = "Media",
                Description = "Commission high-impact television broadcasts, radio broadcasts, and sovereign streaming advertising.",
                ForeseeablePositiveConsequence = "Broad nationwide recognition and substantial swing-voter momentum (+7% Polling).",
                ForeseeableNegativeConsequence = "Substantial advertising airtime expenditure (-R 750,000 REPU) and heightened press scrutiny (-1.5% Approval).",
                CostRepu = 750_000m,
                PollingImpact = 7.0m,
                FundingImpact = 0m,
                ApprovalImpact = -1.5m,
                StabilityImpact = 0.5m,
                IsNetPositive = true
            },
            new()
            {
                Id = "dec_regional_tour",
                Title = "Approve Regional Campaign Tour",
                Category = "Fieldwork",
                Description = "Charter campaign motorcades across all interior provinces to meet local mayors and civic leaders.",
                ForeseeablePositiveConsequence = "Direct personal connection with provincial delegates (+5% Polling, +4% Endorsements).",
                ForeseeableNegativeConsequence = "Significant logistics, security, and accommodation drain on the campaign purse (-R 600,000 REPU).",
                CostRepu = 600_000m,
                PollingImpact = 5.0m,
                FundingImpact = 0m,
                ApprovalImpact = 2.0m,
                StabilityImpact = 1.0m,
                IsNetPositive = true
            },
            new()
            {
                Id = "dec_grassroots_townhall",
                Title = "Grassroots Town Hall Series",
                Category = "Fieldwork",
                Description = "Host unscripted Q&A town halls in university auditoriums and community centers.",
                ForeseeablePositiveConsequence = "High voter trust and viral organic social media reach (+4% Polling, +3% Approval).",
                ForeseeableNegativeConsequence = "Rigorous unscripted questions create policy delivery expectations and venue costs (-R 250,000 REPU).",
                CostRepu = 250_000m,
                PollingImpact = 4.0m,
                FundingImpact = 0m,
                ApprovalImpact = 3.0m,
                StabilityImpact = 1.5m,
                IsNetPositive = true
            },
            new()
            {
                Id = "dec_reject_controversial_deal",
                Title = "Reject Controversial Lobby PAC Deal",
                Category = "Ethics",
                Description = "Publicly reject an anonymous backchannel offer of unrestricted offshore campaign donations.",
                ForeseeablePositiveConsequence = "Demonstrates moral authority, anti-corruption leadership, and public respect (+4% Approval, +3% Polling).",
                ForeseeableNegativeConsequence = "Offended donor syndicate retaliates by funding opposition attack ads; zero funding gained (-R 100,000 legal audit).",
                CostRepu = 100_000m,
                PollingImpact = 3.0m,
                FundingImpact = 0m,
                ApprovalImpact = 4.0m,
                StabilityImpact = 2.0m,
                IsNetPositive = true
            },
            new()
            {
                Id = "dec_public_commitment",
                Title = "Make Public Constitutional Commitment",
                Category = "Policy",
                Description = "Sign a formal public charter guaranteeing judicial independence and balanced national development.",
                ForeseeablePositiveConsequence = "Solidifies democratic mandate and locks in civil society coalition endorsements (+5% Polling, +2% Stability).",
                ForeseeableNegativeConsequence = "Restricts future legislative compromise; immediate delivery pressure post-inauguration (-R 150,000 event cost).",
                CostRepu = 150_000m,
                PollingImpact = 5.0m,
                FundingImpact = 0m,
                ApprovalImpact = 2.5m,
                StabilityImpact = 2.0m,
                IsNetPositive = true
            },
            // Canonical 3 Net-Negative / High-Risk decisions (30%):
            new()
            {
                Id = "dec_expensive_gala",
                Title = "Approve Luxury Gala Fundraiser",
                Category = "Finance",
                Description = "Host an extravagant black-tie dinner at a premier resort for ultra-wealthy political patrons.",
                ForeseeablePositiveConsequence = "Access to elite donors raises financial pledges (+R 500,000 REPU donations).",
                ForeseeableNegativeConsequence = "Opposition leaks photos of the lavish banquet; populist backlash against perceived elitism (-4% Approval, -3% Polling, -R 1,200,000 net expense).",
                CostRepu = 1_200_000m,
                PollingImpact = -3.0m,
                FundingImpact = 500_000m,
                ApprovalImpact = -4.0m,
                StabilityImpact = -2.0m,
                IsNetPositive = false
            },
            new()
            {
                Id = "dec_accept_sponsorship",
                Title = "Accept Corporate Monopolist Sponsorship",
                Category = "Finance",
                Description = "Accept a massive exclusive sponsorship deal from a dominant national utility conglomerate.",
                ForeseeablePositiveConsequence = "Massive immediate capital injection directly into the campaign purse (+R 1,500,000 REPU).",
                ForeseeableNegativeConsequence = "Severe reputational damage; editorial boards retract endorsement over regulatory capture fears (-5% Polling, -4% Approval).",
                CostRepu = 150_000m,
                PollingImpact = -5.0m,
                FundingImpact = 1_500_000m,
                ApprovalImpact = -4.0m,
                StabilityImpact = -3.0m,
                IsNetPositive = false
            },
            new()
            {
                Id = "dec_aggressive_attack_ads",
                Title = "Launch Aggressive Smear Campaign",
                Category = "Media",
                Description = "Blanket the airwaves with scorched-earth negative advertisements attacking the opposition candidate's character.",
                ForeseeablePositiveConsequence = "Dents opposition candidate enthusiasm and lowers their debate momentum.",
                ForeseeableNegativeConsequence = "Voter fatigue and civic backlash; toxic atmosphere reduces moderate turnout and harms stability (-3% Polling, -3% Stability, -R 800,000 ad cost).",
                CostRepu = 800_000m,
                PollingImpact = -3.0m,
                FundingImpact = 0m,
                ApprovalImpact = -3.5m,
                StabilityImpact = -3.0m,
                IsNetPositive = false
            }
        };
    }
}

public sealed class CampaignOfficeState
{
    [JsonPropertyName("campaign_manager")]
    public string CampaignManager { get; set; } = "Campaign Manager";

    [JsonPropertyName("polling")]
    public decimal Polling { get; set; } = 35m;

    [JsonPropertyName("funding")]
    public decimal Funding { get; set; } = 20m;

    [JsonPropertyName("endorsements")]
    public decimal Endorsements { get; set; } = 15m;

    [JsonPropertyName("voting_blocs")]
    public decimal VotingBlocs { get; set; } = 25m;

    [JsonPropertyName("purse")]
    public decimal Purse { get; set; } = 5_000_000m;

    [JsonPropertyName("accountant_general")]
    public CampaignAccountantGeneral AccountantGeneral { get; set; } = new();

    [JsonPropertyName("available_decisions")]
    public List<CampaignDecision> AvailableDecisions { get; set; } = CampaignDecision.CreateCanonicalDecisions();

    [JsonPropertyName("completed_decisions")]
    public List<CampaignDecision> CompletedDecisions { get; set; } = new();

    [JsonPropertyName("election_resolved")]
    public bool ElectionResolved { get; set; }

    [JsonPropertyName("election_won")]
    public bool ElectionWon { get; set; }

    [JsonPropertyName("election_tally")]
    public decimal ElectionTally { get; set; }

    [JsonPropertyName("election_summary")]
    public string ElectionSummary { get; set; } = string.Empty;
    public List<CampaignStakeholder> Stakeholders { get; set; } = new();
    public List<LobbyDeal> LobbyDeals { get; set; } = new();
    public List<CampaignPromise> PublicPromises { get; set; } = new();
    public List<SecretDeal> SecretDeals { get; set; } = new();
}

public sealed class CampaignStakeholder
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Bloc { get; set; } = string.Empty;
    public string Demand { get; set; } = string.Empty;
    public decimal Influence { get; set; }
}

public sealed class LobbyDeal
{
    public Guid Id { get; set; }
    public string LobbyName { get; set; } = string.Empty;
    public string FavorRequested { get; set; } = string.Empty;
    public decimal FundingOffered { get; set; }
    public bool Accepted { get; set; }
}

public sealed class CampaignPromise
{
    public Guid Id { get; set; }
    public string Theme { get; set; } = string.Empty;
    public decimal ApprovalImpact { get; set; }
    public decimal EconomyImpact { get; set; }
    public decimal StabilityImpact { get; set; }
    public decimal DiplomacyImpact { get; set; }
}

public sealed class SecretDeal
{
    public Guid Id { get; set; }
    public string Counterparty { get; set; } = string.Empty;
    public string Obligation { get; set; } = string.Empty;
    public decimal ScandalRisk { get; set; }
    public bool Exposed { get; set; }
}

public sealed class PressureEvent
{
    public string Source { get; set; } = string.Empty;
    public string Demand { get; set; } = string.Empty;
    public decimal PenaltyIfIgnored { get; set; }
}

public sealed class LegislatureState
{
    public int SenateSeatsTotal { get; set; } = 109;
    public int HouseSeatsTotal { get; set; } = 360;
    public int SenateSeatsHeld { get; set; } = 60;
    public int HouseSeatsHeld { get; set; } = 190;
    public decimal SenateLoyalty { get; set; } = 42m;
    public decimal HouseLoyalty { get; set; } = 42m;
    public bool DefectionCrisisActive { get; set; }
}
