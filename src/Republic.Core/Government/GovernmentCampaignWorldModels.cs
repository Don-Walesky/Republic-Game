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

public sealed class CampaignOfficeState
{
    public string CampaignManager { get; set; } = "Campaign Manager";
    public decimal Polling { get; set; } = 30m;
    public decimal Funding { get; set; } = 20m;
    public decimal Endorsements { get; set; } = 10m;
    public decimal VotingBlocs { get; set; } = 20m;
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
