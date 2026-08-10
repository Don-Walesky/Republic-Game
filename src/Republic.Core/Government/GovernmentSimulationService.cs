namespace Republic.Core.Government;

/// <summary>
/// Tracks the player-facing office, cabinet, economic, military, legislative, and diplomatic systems.
/// </summary>
public sealed partial class GovernmentSimulationService
{
    public GovernmentState CreateInitialState(
        string countryName,
        string leaderName,
        string currencyCode,
        decimal treasuryBalance,
        decimal initialExchangeRate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryName);
        ArgumentException.ThrowIfNullOrWhiteSpace(leaderName);
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);

        var state = new GovernmentState
        {
            CountryName = countryName,
            LeaderName = leaderName,
            TreasuryBalance = treasuryBalance,
            Phase = OfficePhase.Campaign,
            IsInOffice = false,
            Cabinet = new List<CabinetMinister>(),
            EmploymentPrograms = new List<EmploymentProgram>(),
            TradeHistory = new List<TradeAction>(),
            Military = new MilitaryState(),
            Currencies = new List<CurrencyState>
            {
                new()
                {
                    Name = countryName,
                    Code = currencyCode,
                    ExchangeRate = initialExchangeRate,
                    TradeVolume = 0m,
                    MarketPerformance = 0m,
                },
            },
            Population = 1_000_000,
            Gdp = 20_000_000m,
            InfrastructureIndex = 22m,
            Hdi = 0.31m,
            EducationIndex = 25m,
            InsecurityIndex = 55m,
            UnemploymentRate = 28m,
            ApprovalRating = 28m,
            EconomyIndex = 28m,
            StabilityIndex = 26m,
            DiplomacyIndex = 24m,
            CorruptionLevel = 45m,
            CivilUnrestIndex = 40m,
            LegislatorLoyalty = 42m,
            ImmigrationCount = 0,
            ExternalDebt = 0m,
            TertiaryInstitutions = 2,
            CurrentTurn = 0,
            Bills = new List<LegislativeBill>(),
            MinisterialTasks = new List<MinisterialTask>(),
            ForeignAidHistory = new List<ForeignAidAgreement>(),
            PressBriefings = new List<PressBriefing>(),
            AppointmentReviews = new List<CabinetAppointmentReview>(),
            Industries = new List<Industry>(),
            NaturalResources = new List<NaturalResource>(),
            InfrastructureProjects = new List<InfrastructureProject>(),
            Loans = new List<LoanAgreement>(),
            BilateralTrades = new List<BilateralTrade>(),
            Opposition = new OppositionState { LeaderName = "Rival Leader", Strength = 55m },
            PendingLegislatorDemands = new List<LegislatorDemand>(),
            Judges = new List<Judge>(),
            JudicialRulings = new List<JudicialRuling>(),
            DisasterResponses = new List<DisasterResponse>(),
            PendingMinisterInitiatives = new List<MinisterInitiative>(),
            MinisterInitiativesHistory = new List<MinisterInitiative>(),
            IntelligenceOps = new List<IntelligenceOp>(),
            Legislature = new LegislatureState(),
            CampaignOffice = new CampaignOfficeState(),
            WorldCountries = new List<WorldCountry>(),
            PeaceTreaties = new List<PeaceTreaty>(),
            Alliances = new List<GeopoliticalAlliance>(),
            MilitaryIncidents = new List<MilitaryIncident>(),
            PlayerPresence = new PlayerPresence(),
            ElectionCycle = 1,
            HasPartyMajority = true,
            MajorityLeader = "Majority Leader",
        };

        state.RegionalProfile = GenerateProceduralNation(countryName, currencyCode, leaderName);
        InitializeCampaignFramework(state);
        GenerateNeighborAiCountries(state, count: 8);
        CreateDefaultPeaceAccords(state);

        return state;
    }

    public void WinElection(GovernmentState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.Phase = OfficePhase.InOffice;
        state.IsInOffice = true;
        state.ElectionCycle += 1;
        state.ApprovalRating = Math.Min(100m, state.ApprovalRating + 5m);
    }

    public void AssignCabinetMember(GovernmentState state, string ministerName, string portfolio)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(ministerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(portfolio);

        state.Cabinet.RemoveAll(item => item.Portfolio.Equals(portfolio, StringComparison.OrdinalIgnoreCase));
        state.Cabinet.Add(new CabinetMinister
        {
            Name = ministerName,
            Portfolio = portfolio,
        });
    }

    public CabinetAppointmentReview ReviewAndAssignCabinetMember(GovernmentState state, string ministerName, string portfolio, int senateApprovalScore)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(ministerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(portfolio);

        if (!state.IsInOffice)
        {
            throw new InvalidOperationException("Appointments require the president to be in office.");
        }

        var review = new CabinetAppointmentReview
        {
            MinisterName = ministerName,
            Portfolio = portfolio,
            SenateApprovalScore = senateApprovalScore,
            Approved = senateApprovalScore >= 60,
            ReviewNotes = senateApprovalScore >= 60 ? "Confirmed by the senate committee." : "Returned for additional vetting.",
        };

        state.AppointmentReviews.Add(review);

        if (!review.Approved)
        {
            return review;
        }

        AssignCabinetMember(state, ministerName, portfolio);
        state.ApprovalRating = Math.Min(100m, state.ApprovalRating + 1.5m);
        return review;
    }

    public MinisterialTask IssueMinisterialTask(GovernmentState state, string ministerName, string taskDescription, decimal proposedCost)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(ministerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(taskDescription);

        if (!state.IsInOffice)
        {
            throw new InvalidOperationException("Ministerial tasks require the president to be in office.");
        }

        var minister = state.Cabinet.FirstOrDefault(item => item.Name.Equals(ministerName, StringComparison.OrdinalIgnoreCase));
        if (minister is null)
        {
            throw new InvalidOperationException("Only confirmed ministers can receive tasks.");
        }

        var negotiatedCost = Math.Max(10000m, proposedCost * 0.8m);
        var task = new MinisterialTask
        {
            MinisterName = ministerName,
            TaskDescription = taskDescription,
            ProposedCost = proposedCost,
            NegotiatedCost = negotiatedCost,
            Negotiated = true,
            Timestamp = DateTimeOffset.UtcNow,
        };

        state.MinisterialTasks.Add(task);
        state.TreasuryBalance -= negotiatedCost;
        state.ApprovalRating = Math.Min(100m, state.ApprovalRating + 0.5m);
        return task;
    }

    public EmploymentProgram CreateEmploymentProgram(GovernmentState state, string name, int jobsCreated, decimal cost)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (cost > state.TreasuryBalance)
        {
            throw new InvalidOperationException("Insufficient treasury to fund employment program.");
        }

        state.TreasuryBalance -= cost;
        var program = new EmploymentProgram
        {
            Name = name,
            JobsCreated = jobsCreated,
            Cost = cost,
        };

        state.EmploymentPrograms.Add(program);
        ApplyPopulationAndDevelopment(state, jobsCreated / 100m, jobsCreated / 250m, jobsCreated / 500m, Math.Max(100, jobsCreated / 10));
        return program;
    }

    public TradeAction ExecuteTradeThroughMinister(GovernmentState state, string ministerName, string partnerCountry, decimal amount, decimal newExchangeRate)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(ministerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(partnerCountry);

        if (!state.IsInOffice)
        {
            throw new InvalidOperationException("Trade actions require the player to be in office.");
        }

        var minister = state.Cabinet.FirstOrDefault(item => item.Name.Equals(ministerName, StringComparison.OrdinalIgnoreCase));
        if (minister is null)
        {
            throw new InvalidOperationException("Only assigned ministers can execute trade actions.");
        }

        if (!minister.Portfolio.Equals("Trade", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The selected minister is not responsible for trade.");
        }

        var currency = state.Currencies.First();
        currency.ExchangeRate = Math.Max(newExchangeRate, currency.ExchangeRate + (amount / 100000m));
        currency.TradeVolume += amount;
        currency.MarketPerformance += amount / 1000m;

        var trade = new TradeAction
        {
            MinisterName = ministerName,
            PartnerCountry = partnerCountry,
            Amount = amount,
            ExchangeRate = newExchangeRate,
            Timestamp = DateTimeOffset.UtcNow,
        };

        state.TradeHistory.Add(trade);
        ApplyPopulationAndDevelopment(state, 1.25m, 0.5m, 0.25m, 250);
        return trade;
    }

    public void RecruitPersonnel(GovernmentState state, int count, decimal cost)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (cost > state.TreasuryBalance)
        {
            throw new InvalidOperationException("Insufficient treasury for recruitment.");
        }

        state.TreasuryBalance -= cost;
        state.Military.Personnel += count;
        ApplyPopulationAndDevelopment(state, 0.25m, 0.5m, 0.5m, 0);
    }

    public void PurchaseWeapons(GovernmentState state, int weapons, decimal cost)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (weapons <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weapons));
        }

        if (cost > state.TreasuryBalance)
        {
            throw new InvalidOperationException("Insufficient treasury for weapons procurement.");
        }

        state.TreasuryBalance -= cost;
        state.Military.WeaponsInventory += weapons;
        ApplyPopulationAndDevelopment(state, 0m, 0m, 0.75m, 0);
    }

    public LegislativeBill SponsorBill(GovernmentState state, string title, string sponsor, decimal proposedCost)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(sponsor);

        if (!state.IsInOffice)
        {
            throw new InvalidOperationException("Legislative sponsorship requires the president to be in office.");
        }

        var bill = new LegislativeBill
        {
            Id = Guid.NewGuid(),
            Title = title,
            Sponsor = sponsor,
            ProposedCost = proposedCost,
            NegotiatedCost = proposedCost,
            Status = state.HasPartyMajority ? "Draft" : "Needs Coalition Support",
            SupportScore = Math.Min(100, 55 + (int)(state.ApprovalRating / 2m)),
            MajorityLeader = state.HasPartyMajority ? state.MajorityLeader : "Opposition Leader",
            NegotiationNotes = "Negotiating with loyalists and the majority leader.",
        };

        state.Bills.Add(bill);
        return bill;
    }

    public LegislativeBill NegotiateBill(GovernmentState state, Guid billId, decimal negotiatedCost, int supportScore)
    {
        ArgumentNullException.ThrowIfNull(state);

        var bill = state.Bills.FirstOrDefault(item => item.Id == billId);
        if (bill is null)
        {
            throw new InvalidOperationException("The bill could not be located.");
        }

        bill.NegotiatedCost = negotiatedCost;
        bill.SupportScore = Math.Clamp(supportScore, 0, 100);
        bill.Status = "Negotiated";
        bill.NegotiationNotes = "Bill terms adjusted for governance priorities.";
        state.ApprovalRating = Math.Min(100m, state.ApprovalRating + 0.5m);
        return bill;
    }

    public ForeignAidAgreement NegotiateForeignAid(GovernmentState state, string partnerCountry, string aidType, decimal amount, decimal interestRate, decimal returnValue)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(partnerCountry);
        ArgumentException.ThrowIfNullOrWhiteSpace(aidType);

        if (amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        var agreement = new ForeignAidAgreement
        {
            PartnerCountry = partnerCountry,
            AidType = aidType,
            Amount = amount,
            InterestRate = interestRate,
            ReturnValue = returnValue,
            NetBenefit = amount + returnValue,
            Timestamp = DateTimeOffset.UtcNow,
        };

        state.ForeignAidHistory.Add(agreement);
        state.TreasuryBalance += amount;
        state.ApprovalRating = Math.Min(100m, state.ApprovalRating + 2m);

        switch (aidType.ToLowerInvariant())
        {
            case "loan":
                state.ExternalDebt += amount * (1m + interestRate);
                break;
            case "bailout":
                ApplyPopulationAndDevelopment(state, 1.5m, 1m, 0.5m, 500);
                break;
            case "grant":
                ApplyPopulationAndDevelopment(state, 0.5m, 0.75m, 0.25m, 250);
                break;
        }

        return agreement;
    }

    public void HoldPressBriefing(GovernmentState state, string message, int mediaBoost)
    {
        ArgumentNullException.ThrowIfNull(state);

        var approvalGain = Math.Max(1m, mediaBoost * 1.5m) + 3m;
        state.ApprovalRating = Math.Min(100m, state.ApprovalRating + approvalGain);
        state.PressBriefings.Add(new PressBriefing
        {
            Message = message,
            MediaBoost = mediaBoost,
            ApprovalBoost = approvalGain,
            Timestamp = DateTimeOffset.UtcNow,
        });

        ApplyPopulationAndDevelopment(state, 0.4m, 0.3m, 0.2m, mediaBoost * 100);
    }

    public void PrepareForNextElection(GovernmentState state, decimal campaignBudget)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (campaignBudget > state.TreasuryBalance)
        {
            throw new InvalidOperationException("Insufficient treasury for the next election cycle.");
        }

        state.TreasuryBalance -= campaignBudget;
        state.Phase = OfficePhase.Campaign;
        state.IsInOffice = false;
        state.ElectionCycle += 1;
        state.ApprovalRating = Math.Min(100m, state.ApprovalRating + 2m);
    }

    private static void ApplyPopulationAndDevelopment(GovernmentState state, decimal infrastructureDelta, decimal educationDelta, decimal securityDelta, int immigrationDelta)
    {
        state.Population += immigrationDelta;
        state.Gdp += state.Population / 100m;
        state.InfrastructureIndex = Math.Min(100m, state.InfrastructureIndex + infrastructureDelta);
        state.Hdi = Math.Min(1m, state.Hdi + (educationDelta / 100m) + (infrastructureDelta / 200m));
        state.EducationIndex = Math.Min(100m, state.EducationIndex + educationDelta);
        state.InsecurityIndex = Math.Max(0m, state.InsecurityIndex - securityDelta);
        state.UnemploymentRate = Math.Max(0m, state.UnemploymentRate - (infrastructureDelta / 2m));
        state.ImmigrationCount += immigrationDelta;
    }
}

/// <summary>
/// The overall state for the office-driven gameplay loop.
/// </summary>
public sealed class GovernmentState
{
    public string CountryName { get; set; } = string.Empty;
    public string LeaderName { get; set; } = string.Empty;
    public decimal TreasuryBalance { get; set; }
    public OfficePhase Phase { get; set; }
    public bool IsInOffice { get; set; }
    public List<CabinetMinister> Cabinet { get; set; } = new();
    public List<CurrencyState> Currencies { get; set; } = new();
    public List<EmploymentProgram> EmploymentPrograms { get; set; } = new();
    public List<TradeAction> TradeHistory { get; set; } = new();
    public MilitaryState Military { get; set; } = new();
    public int Population { get; set; }
    public decimal Gdp { get; set; }
    public decimal InfrastructureIndex { get; set; }
    public decimal Hdi { get; set; }
    public decimal EducationIndex { get; set; }
    public decimal InsecurityIndex { get; set; }
    public decimal UnemploymentRate { get; set; }
    public decimal ApprovalRating { get; set; }
    public decimal EconomyIndex { get; set; }
    public decimal StabilityIndex { get; set; }
    public decimal DiplomacyIndex { get; set; }
    public int ImmigrationCount { get; set; }
    public decimal ExternalDebt { get; set; }
    public int ElectionCycle { get; set; }
    public int CurrentTurn { get; set; }
    public bool HasPartyMajority { get; set; }
    public string MajorityLeader { get; set; } = string.Empty;
    public int TertiaryInstitutions { get; set; }
    public decimal CorruptionLevel { get; set; }
    public decimal CivilUnrestIndex { get; set; }
    public decimal LegislatorLoyalty { get; set; }
    public List<LegislativeBill> Bills { get; set; } = new();
    public List<MinisterialTask> MinisterialTasks { get; set; } = new();
    public List<ForeignAidAgreement> ForeignAidHistory { get; set; } = new();
    public List<PressBriefing> PressBriefings { get; set; } = new();
    public List<CabinetAppointmentReview> AppointmentReviews { get; set; } = new();
    public List<Industry> Industries { get; set; } = new();
    public List<NaturalResource> NaturalResources { get; set; } = new();
    public List<InfrastructureProject> InfrastructureProjects { get; set; } = new();
    public List<LoanAgreement> Loans { get; set; } = new();
    public List<BilateralTrade> BilateralTrades { get; set; } = new();
    public OppositionState Opposition { get; set; } = new();
    public List<LegislatorDemand> PendingLegislatorDemands { get; set; } = new();
    public List<Judge> Judges { get; set; } = new();
    public List<JudicialRuling> JudicialRulings { get; set; } = new();
    public List<DisasterResponse> DisasterResponses { get; set; } = new();
    public List<MinisterInitiative> PendingMinisterInitiatives { get; set; } = new();
    public List<MinisterInitiative> MinisterInitiativesHistory { get; set; } = new();
    public List<IntelligenceOp> IntelligenceOps { get; set; } = new();
    public LegislatureState Legislature { get; set; } = new();
    public CampaignOfficeState CampaignOffice { get; set; } = new();
    public List<WorldCountry> WorldCountries { get; set; } = new();
    public List<PeaceTreaty> PeaceTreaties { get; set; } = new();
    public List<GeopoliticalAlliance> Alliances { get; set; } = new();
    public List<MilitaryIncident> MilitaryIncidents { get; set; } = new();
    public PlayerPresence PlayerPresence { get; set; } = new();
    public ProceduralNationProfile RegionalProfile { get; set; } = new();
    public RandomEvent? LastRandomEvent { get; set; }
}

/// <summary>
/// Office phase for the player experience.
/// </summary>
public enum OfficePhase
{
    Campaign,
    InOffice,
}

/// <summary>
/// A cabinet minister representing a ministry office.
/// </summary>
public sealed class CabinetMinister
{
    public string Name { get; set; } = string.Empty;
    public string Portfolio { get; set; } = string.Empty;
}

/// <summary>
/// A cabinet appointment review result from the senate.
/// </summary>
public sealed class CabinetAppointmentReview
{
    public string MinisterName { get; set; } = string.Empty;
    public string Portfolio { get; set; } = string.Empty;
    public int SenateApprovalScore { get; set; }
    public bool Approved { get; set; }
    public string ReviewNotes { get; set; } = string.Empty;
}

/// <summary>
/// A ministerial task negotiated through an office conversation.
/// </summary>
public sealed class MinisterialTask
{
    public string MinisterName { get; set; } = string.Empty;
    public string TaskDescription { get; set; } = string.Empty;
    public decimal ProposedCost { get; set; }
    public decimal NegotiatedCost { get; set; }
    public bool Negotiated { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// A single country currency with forex market state.
/// </summary>
public sealed class CurrencyState
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public decimal TradeVolume { get; set; }
    public decimal MarketPerformance { get; set; }
}

/// <summary>
/// Employment program state for domestic job creation.
/// </summary>
public sealed class EmploymentProgram
{
    public string Name { get; set; } = string.Empty;
    public int JobsCreated { get; set; }
    public decimal Cost { get; set; }
}

/// <summary>
/// A trade action performed through a ministerial office.
/// </summary>
public sealed class TradeAction
{
    public string MinisterName { get; set; } = string.Empty;
    public string PartnerCountry { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal ExchangeRate { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// A legislative bill sponsored through the executive and legislature loop.
/// </summary>
public sealed class LegislativeBill
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Sponsor { get; set; } = string.Empty;
    public decimal ProposedCost { get; set; }
    public decimal NegotiatedCost { get; set; }
    public int SupportScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public string MajorityLeader { get; set; } = string.Empty;
    public string NegotiationNotes { get; set; } = string.Empty;
}

/// <summary>
/// A foreign assistance package negotiated with another country.
/// </summary>
public sealed class ForeignAidAgreement
{
    public string PartnerCountry { get; set; } = string.Empty;
    public string AidType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; }
    public decimal ReturnValue { get; set; }
    public decimal NetBenefit { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// A press briefing that improves public approval.
/// </summary>
public sealed class PressBriefing
{
    public string Message { get; set; } = string.Empty;
    public int MediaBoost { get; set; }
    public decimal ApprovalBoost { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Military state for recruitment, weapons procurement, and operations.
/// </summary>
public sealed class MilitaryState
{
    public int Personnel { get; set; }
    public int WeaponsInventory { get; set; }
    public List<MilitaryAction> OperationHistory { get; set; } = new();
}
