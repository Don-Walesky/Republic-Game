using System.Linq;

namespace Republic.Core.Government;

/// <summary>
/// A simple bridge facade that exposes the government simulation state for other runtimes.
/// </summary>
public sealed class GovernmentStateBridge
{
    private readonly GovernmentSimulationService _service = new();
    private GovernmentState _state = new();

    public GovernmentState CreateInitialState(
        string countryName,
        string leaderName,
        string currencyCode,
        decimal treasuryBalance,
        decimal initialExchangeRate)
    {
        _state = _service.CreateInitialState(countryName, leaderName, currencyCode, treasuryBalance, initialExchangeRate);
        return _state;
    }

    public GovernmentState GetState() => _state;

    public GovernmentStateSnapshot GetSnapshot()
    {
        var currency = _state.Currencies.FirstOrDefault();
        var activeLoan = _state.Loans.FirstOrDefault(l => !l.IsSettled);
        var pendingDemand = _state.PendingLegislatorDemands.FirstOrDefault();
        var pendingInitiative = _state.PendingMinisterInitiatives.FirstOrDefault();
        var stakeholder = _state.CampaignOffice.Stakeholders.FirstOrDefault();
        var neighbor = _state.WorldCountries.OrderByDescending(c => c.EconomyPower).FirstOrDefault();
        return new GovernmentStateSnapshot
        {
            Phase = _state.Phase == OfficePhase.InOffice ? "In Office" : "Campaign",
            TreasuryBalance = _state.TreasuryBalance,
            ExchangeRate = currency?.ExchangeRate ?? 0m,
            TradeVolume = currency?.TradeVolume ?? 0m,
            EmploymentPrograms = _state.EmploymentPrograms.Count,
            Personnel = _state.Military.Personnel,
            Weapons = _state.Military.WeaponsInventory,
            CabinetCount = _state.Cabinet.Count,
            ApprovalRating = _state.ApprovalRating,
            EconomyIndex = _state.EconomyIndex,
            StabilityIndex = _state.StabilityIndex,
            DiplomacyIndex = _state.DiplomacyIndex,
            Population = _state.Population,
            Gdp = _state.Gdp,
            InfrastructureIndex = _state.InfrastructureIndex,
            Hdi = _state.Hdi,
            EducationIndex = _state.EducationIndex,
            InsecurityIndex = _state.InsecurityIndex,
            UnemploymentRate = _state.UnemploymentRate,
            ExternalDebt = _state.ExternalDebt,
            ElectionCycle = _state.ElectionCycle,
            CurrentTurn = _state.CurrentTurn,
            TertiaryInstitutions = _state.TertiaryInstitutions,
            CorruptionLevel = _state.CorruptionLevel,
            CivilUnrestIndex = _state.CivilUnrestIndex,
            LegislatorLoyalty = _state.LegislatorLoyalty,
            OppositionStrength = _state.Opposition.Strength,
            OppositionLastAction = _state.Opposition.LastAction,
            SenateSeatsHeld = _state.Legislature.SenateSeatsHeld,
            HouseSeatsHeld = _state.Legislature.HouseSeatsHeld,
            CampaignPolling = _state.CampaignOffice.Polling,
            CampaignFunding = _state.CampaignOffice.Funding,
            CampaignEndorsements = _state.CampaignOffice.Endorsements,
            CampaignVotingBlocs = _state.CampaignOffice.VotingBlocs,
            CurrentStakeholderId = stakeholder?.Id ?? System.Guid.Empty,
            CurrentStakeholderName = stakeholder?.Name ?? string.Empty,
            CurrentStakeholderDemand = stakeholder?.Demand ?? string.Empty,
            IsGovernmentToppled = _state.PlayerPresence.GovernmentToppled,
            CaretakerPresident = _state.PlayerPresence.CaretakerPresident,
            NeighborCountries = _state.WorldCountries.Count,
            ActiveTreaties = _state.PeaceTreaties.Count,
            StrongestNeighbor = neighbor?.Name ?? string.Empty,
            StrongestNeighborCurrency = neighbor?.CurrencyCode ?? string.Empty,
            StrongestNeighborPower = neighbor is null ? 0m : neighbor.EconomyPower + neighbor.MilitaryPower + neighbor.DiplomaticPower,
            IndustryCount = _state.Industries.Count,
            IndustryTurnIncome = _state.Industries.Sum(i => i.TurnRevenue),
            ResourceCount = _state.NaturalResources.Count,
            ActiveLoansCount = _state.Loans.Count(l => !l.IsSettled),
            ActiveLoanId = activeLoan?.Id ?? System.Guid.Empty,
            ActiveLoanOwed = activeLoan != null ? activeLoan.TotalOwed - activeLoan.AmountRepaid : 0m,
            ActiveLoanLender = activeLoan?.Lender.ToString() ?? string.Empty,
            BilateralTradesCount = _state.BilateralTrades.Count,
            BillCount = _state.Bills.Count,
            ForeignAidCount = _state.ForeignAidHistory.Count,
            MinisterialTaskCount = _state.MinisterialTasks.Count,
            PendingDemandId = pendingDemand?.Id ?? System.Guid.Empty,
            PendingDemandDesc = pendingDemand?.Description ?? string.Empty,
            PendingDemandCost = pendingDemand?.Cost ?? 0m,
            PendingInitiativeId = pendingInitiative?.Id ?? System.Guid.Empty,
            PendingInitiativeTitle = pendingInitiative?.ProposalTitle ?? string.Empty,
            PendingInitiativeCost = pendingInitiative?.Cost ?? 0m,
            LastBillId = _state.Bills.Count > 0 ? _state.Bills[^1].Id : System.Guid.Empty,
            LastBillTitle = _state.Bills.Count > 0 ? _state.Bills[^1].Title : string.Empty,
            LastBillStatus = _state.Bills.Count > 0 ? _state.Bills[^1].Status : string.Empty,
            LastRandomEvent = _state.LastRandomEvent?.Name ?? string.Empty,
        };
    }

    public void WinElection() => _service.WinElection(_state);

    public void AssignCabinetMember(string ministerName, string portfolio) => _service.AssignCabinetMember(_state, ministerName, portfolio);

    public CabinetAppointmentReview ReviewAndAssignCabinetMember(string ministerName, string portfolio, int senateScore) => _service.ReviewAndAssignCabinetMember(_state, ministerName, portfolio, senateScore);

    public MinisterialTask IssueMinisterialTask(string ministerName, string task, decimal cost) => _service.IssueMinisterialTask(_state, ministerName, task, cost);

    public EmploymentProgram CreateEmploymentProgram(string name, int jobsCreated, decimal cost) => _service.CreateEmploymentProgram(_state, name, jobsCreated, cost);

    public TradeAction ExecuteTradeThroughMinister(string ministerName, string partnerCountry, decimal amount, decimal newExchangeRate) => _service.ExecuteTradeThroughMinister(_state, ministerName, partnerCountry, amount, newExchangeRate);

    public LegislativeBill SponsorBill(string title, string sponsor, decimal cost) => _service.SponsorBill(_state, title, sponsor, cost);

    public LegislativeBill NegotiateBill(System.Guid billId, decimal negotiatedCost, int supportScore) => _service.NegotiateBill(_state, billId, negotiatedCost, supportScore);

    public ForeignAidAgreement NegotiateForeignAid(string partnerCountry, string aidType, decimal amount, decimal interestRate, decimal returnValue) => _service.NegotiateForeignAid(_state, partnerCountry, aidType, amount, interestRate, returnValue);

    public void HoldPressBriefing(string message, int mediaBoost) => _service.HoldPressBriefing(_state, message, mediaBoost);

    public void PrepareForNextElection(decimal campaignBudget) => _service.PrepareForNextElection(_state, campaignBudget);

    public void RecruitPersonnel(int count, decimal cost) => _service.RecruitPersonnel(_state, count, cost);

    public void PurchaseWeapons(int weapons, decimal cost) => _service.PurchaseWeapons(_state, weapons, cost);

    public TurnResult AdvanceTurn() => _service.AdvanceTurn(_state);
    public TurnRevenue CalculateTurnRevenue() => _service.CalculateTurnRevenue(_state);
    public Industry AddIndustry(string name, string sector, decimal cost) => _service.AddIndustry(_state, name, ParseEnum<IndustrySector>(sector), cost);
    public NaturalResource DiscoverResource(string name, string type) => _service.DiscoverResource(_state, name, ParseEnum<ResourceType>(type));
    public void BeginResourceExtraction(string name, decimal cost) => _service.BeginResourceExtraction(_state, name, cost);
    public InfrastructureProject BuildInfrastructure(string type, decimal cost) => _service.BuildInfrastructure(_state, ParseEnum<InfrastructureType>(type), cost);
    public LoanAgreement TakeLoan(string lender, decimal amount, decimal interestRate, int turns) => _service.TakeLoan(_state, ParseEnum<LoanLender>(lender), amount, interestRate, turns);
    public void RepayLoan(System.Guid loanId, decimal amount) => _service.RepayLoan(_state, loanId, amount);
    public void RenegotiateLoan(System.Guid loanId, decimal newRate, int extraTurns) => _service.RenegotiateLoan(_state, loanId, newRate, extraTurns);
    public MilitaryAction LaunchMilitaryOperation(string target, string opType, int troops) => _service.LaunchMilitaryOperation(_state, target, ParseEnum<MilitaryOpType>(opType), troops);
    public void ReceiveMilitaryAttack(string attacker, int strength) => _service.ReceiveMilitaryAttack(_state, attacker, strength);
    public BilateralTrade NegotiateBilateralTrade(string partner, decimal exports, decimal imports) => _service.NegotiateBilateralTrade(_state, partner, exports, imports);
    public void GrantLegislatorDemand(System.Guid demandId, bool grant) => _service.GrantLegislatorDemand(_state, demandId, grant);
    public Judge AppointJudge(string name, int score) => _service.AppointJudge(_state, name, score);
    public JudicialRuling RuleOnLaw(System.Guid billId) => _service.RuleOnLaw(_state, billId);
    public DisasterResponse RespondToDisaster(string type, decimal budget) => _service.RespondToDisaster(_state, ParseEnum<DisasterType>(type), budget);
    public MinisterInitiative ReceiveMinisterInitiative(string minister, string title, decimal cost, decimal gdpImpact) => _service.ReceiveMinisterInitiative(_state, minister, title, cost, gdpImpact);
    public void ApproveMinisterInitiative(System.Guid id, bool approve) => _service.ApproveMinisterInitiative(_state, id, approve);
    public void AddressProtest(string concession, decimal cost) => _service.AddressProtest(_state, concession, cost);
    public IntelligenceOp RunIntelligenceOp(string opType, string target, decimal cost) => _service.RunIntelligenceOp(_state, ParseEnum<IntelOpType>(opType), target, cost);
    public void InvestigateCorruption(decimal cost) => _service.InvestigateCorruption(_state, cost);
    public void ResolveCampaignMeeting(System.Guid stakeholderId, string choice) => _service.ResolveCampaignMeeting(_state, stakeholderId, ParseEnum<CampaignChoice>(choice));
    public bool ResolveElectionNight() => _service.ResolveElectionNight(_state);
    public void SimulateSecretDealExposure() => _service.SimulateSecretDealExposure(_state);
    public void UpdatePlayerPresence(int consecutiveOfflineTurns, int toppleThresholdTurns) => _service.UpdatePlayerPresence(_state, consecutiveOfflineTurns, toppleThresholdTurns);
    public void RecalculateForexMarket() => _service.RecalculateForexMarket(_state);
    public GeopoliticalAlliance FormAlliance(string allianceName, string type, string[] members) => _service.FormAlliance(_state, allianceName, ParseEnum<AllianceType>(type), members);
    public MilitaryIncident RecordMilitaryIncident(string initiator, string target, string incidentType, string severity) => _service.RecordMilitaryIncident(_state, initiator, target, incidentType, ParseEnum<IncidentSeverity>(severity));
    public void RunMidtermElection() => _service.RunMidtermElection(_state);
    public void ResolveDefectionCrisis(string response) => _service.ResolveDefectionCrisis(_state, ParseEnum<DefectionResponse>(response));

    private static T ParseEnum<T>(string value) where T : struct, System.Enum =>
        System.Enum.TryParse<T>(value, ignoreCase: true, out var result) ? result
        : throw new System.ArgumentException($"Unknown value '{value}' for {typeof(T).Name}.");
}

/// <summary>
/// A lightweight, serializable snapshot of the government state for external runtimes.
/// </summary>
public sealed class GovernmentStateSnapshot
{
    public string Phase { get; init; } = string.Empty;
    public decimal TreasuryBalance { get; init; }
    public decimal ExchangeRate { get; init; }
    public decimal TradeVolume { get; init; }
    public int EmploymentPrograms { get; init; }
    public int Personnel { get; init; }
    public int Weapons { get; init; }
    public int CabinetCount { get; init; }
    public decimal ApprovalRating { get; init; }
    public decimal EconomyIndex { get; init; }
    public decimal StabilityIndex { get; init; }
    public decimal DiplomacyIndex { get; init; }
    public int Population { get; init; }
    public decimal Gdp { get; init; }
    public decimal InfrastructureIndex { get; init; }
    public decimal Hdi { get; init; }
    public decimal EducationIndex { get; init; }
    public decimal InsecurityIndex { get; init; }
    public decimal UnemploymentRate { get; init; }
    public decimal ExternalDebt { get; init; }
    public int ElectionCycle { get; init; }
    public int CurrentTurn { get; init; }
    public int TertiaryInstitutions { get; init; }
    public decimal CorruptionLevel { get; init; }
    public decimal CivilUnrestIndex { get; init; }
    public decimal LegislatorLoyalty { get; init; }
    public decimal OppositionStrength { get; init; }
    public string OppositionLastAction { get; init; } = string.Empty;
    public int SenateSeatsHeld { get; init; }
    public int HouseSeatsHeld { get; init; }
    public decimal CampaignPolling { get; init; }
    public decimal CampaignFunding { get; init; }
    public decimal CampaignEndorsements { get; init; }
    public decimal CampaignVotingBlocs { get; init; }
    public System.Guid CurrentStakeholderId { get; init; }
    public string CurrentStakeholderName { get; init; } = string.Empty;
    public string CurrentStakeholderDemand { get; init; } = string.Empty;
    public bool IsGovernmentToppled { get; init; }
    public string CaretakerPresident { get; init; } = string.Empty;
    public int NeighborCountries { get; init; }
    public int ActiveTreaties { get; init; }
    public string StrongestNeighbor { get; init; } = string.Empty;
    public string StrongestNeighborCurrency { get; init; } = string.Empty;
    public decimal StrongestNeighborPower { get; init; }
    public int IndustryCount { get; init; }
    public decimal IndustryTurnIncome { get; init; }
    public int ResourceCount { get; init; }
    public int ActiveLoansCount { get; init; }
    public System.Guid ActiveLoanId { get; init; }
    public decimal ActiveLoanOwed { get; init; }
    public string ActiveLoanLender { get; init; } = string.Empty;
    public int BilateralTradesCount { get; init; }
    public int BillCount { get; init; }
    public int ForeignAidCount { get; init; }
    public int MinisterialTaskCount { get; init; }
    public System.Guid PendingDemandId { get; init; }
    public string PendingDemandDesc { get; init; } = string.Empty;
    public decimal PendingDemandCost { get; init; }
    public System.Guid PendingInitiativeId { get; init; }
    public string PendingInitiativeTitle { get; init; } = string.Empty;
    public decimal PendingInitiativeCost { get; init; }
    public System.Guid LastBillId { get; init; }
    public string LastBillTitle { get; init; } = string.Empty;
    public string LastBillStatus { get; init; } = string.Empty;
    public string LastRandomEvent { get; init; } = string.Empty;
}
