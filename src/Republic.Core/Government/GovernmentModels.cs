namespace Republic.Core.Government;

// ── Turn ─────────────────────────────────────────────────────────────────────

public sealed class TurnResult
{
    public int Turn { get; init; }
    public TurnRevenue Revenue { get; init; } = new();
    public RandomEvent? RandomEvent { get; init; }
}

public sealed class TurnRevenue
{
    public decimal TaxRevenue { get; init; }
    public decimal TariffRevenue { get; init; }
    public decimal IndustryIncome { get; init; }
    public decimal ResourceIncome { get; init; }
    public decimal Total { get; init; }
}

// ── Industry ─────────────────────────────────────────────────────────────────

public enum IndustrySector { Oil, Technology, Agriculture, Manufacturing, Tourism, Finance, Mining, Telecom }

public sealed class Industry
{
    public string Name { get; set; } = string.Empty;
    public IndustrySector Sector { get; set; }
    public decimal Investment { get; set; }
    public decimal TurnRevenue { get; set; }
    public int EmployeesGenerated { get; set; }
}

// ── Natural resources ─────────────────────────────────────────────────────────

public enum ResourceType { Oil, Gas, Gold, Diamond, Timber, Water, Uranium, Coal }

public sealed class NaturalResource
{
    public string Name { get; set; } = string.Empty;
    public ResourceType Type { get; set; }
    public decimal TurnYield { get; set; }
    public bool IsExtracting { get; set; }
}

// ── Infrastructure ────────────────────────────────────────────────────────────

public enum InfrastructureType { RoadNetwork, Airport, University, PowerGrid, Hospital, Port, Stadium, TechPark }

public sealed class InfrastructureProject
{
    public InfrastructureType Type { get; set; }
    public decimal Cost { get; set; }
    public int BuiltOnTurn { get; set; }
}

// ── Loans ─────────────────────────────────────────────────────────────────────

public enum LoanLender { Imf, WorldBank, Cbn, RegionalBank, ForeignGovt }

public sealed class LoanAgreement
{
    public Guid Id { get; set; }
    public LoanLender Lender { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public decimal TotalOwed { get; set; }
    public decimal AmountRepaid { get; set; }
    public int RepaymentTurns { get; set; }
    public int TurnsTaken { get; set; }
    public bool IsSettled { get; set; }
}

// ── Military operations ───────────────────────────────────────────────────────

public enum MilitaryOpType { Invasion, Airstrike, CyberAttack, Blockade, PeacekeepingMission }

public sealed class MilitaryAction
{
    public string AttackerCountry { get; set; } = string.Empty;
    public string TargetCountry { get; set; } = string.Empty;
    public MilitaryOpType OpType { get; set; }
    public int TroopsCommitted { get; set; }
    public bool Succeeded { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

// ── Bilateral trade ───────────────────────────────────────────────────────────

public sealed class BilateralTrade
{
    public string PartnerCountry { get; set; } = string.Empty;
    public decimal ExportValue { get; set; }
    public decimal ImportValue { get; set; }
    public decimal NetBenefit { get; set; }
    public int Turn { get; set; }
}

// ── Opposition ────────────────────────────────────────────────────────────────

public sealed class OppositionState
{
    public string LeaderName { get; set; } = "Opposition Leader";
    public decimal Strength { get; set; }
    public string LastAction { get; set; } = string.Empty;
}

// ── Legislator demands ────────────────────────────────────────────────────────

public sealed class LegislatorDemand
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
}

// ── Judiciary ─────────────────────────────────────────────────────────────────

public sealed class Judge
{
    public string Name { get; set; } = string.Empty;
    public decimal Loyalty { get; set; }
    public bool IsAppointed { get; set; }
}

public sealed class JudicialRuling
{
    public Guid BillId { get; set; }
    public string BillTitle { get; set; } = string.Empty;
    public bool Upheld { get; set; }
    public string Notes { get; set; } = string.Empty;
}

// ── Disasters ─────────────────────────────────────────────────────────────────

public enum DisasterType { Flood, Earthquake, Pandemic, Drought, Wildfire, TsunamiAlert }

public sealed class DisasterResponse
{
    public DisasterType DisasterType { get; set; }
    public decimal ReliefBudget { get; set; }
    public decimal Effectiveness { get; set; }
    public int Turn { get; set; }
}

// ── Minister initiatives ──────────────────────────────────────────────────────

public sealed class MinisterInitiative
{
    public Guid Id { get; set; }
    public string MinisterName { get; set; } = string.Empty;
    public string ProposalTitle { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public decimal ProjectedGdpImpact { get; set; }
    public string Status { get; set; } = string.Empty;
}

// ── Intelligence ops ──────────────────────────────────────────────────────────

public enum IntelOpType { Surveillance, Infiltration, Propaganda, CounterEspionage }

public sealed class IntelligenceOp
{
    public IntelOpType OpType { get; set; }
    public string TargetCountry { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public bool Succeeded { get; set; }
    public int Turn { get; set; }
}

// ── Random events ─────────────────────────────────────────────────────────────

public sealed class RandomEvent
{
    public string Name { get; set; } = string.Empty;
    public decimal ApprovalDelta { get; set; }
    public decimal GdpDelta { get; set; }
    public decimal InsecurityDelta { get; set; }
}
