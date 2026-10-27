namespace Republic.Core.World.Models;

using System.Text.Json.Serialization;
using Republic.Core.Cabinet.Models;
using Republic.Core.Economy.Budget.Models;
using Republic.Core.Economy.PrivateSector.Models;
using Republic.Core.Economy.Projects;
using Republic.Core.Economy.Treasury;
using Republic.Core.Government.Officials.Models;
using Republic.Core.Media.Models;
using Republic.Core.NationalYield;
using Republic.Core.Politics;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Rules;

/// <summary>
/// Domain model representing a sovereign nation entity.
/// Holds the authoritative national identity, constitution, and historical founding moment.
/// </summary>
public sealed class Country
{
    private RepublicTime _foundingTime;
    private bool _isFoundingSealed;
    private CountryProfile? _profile;

    /// <summary>
    /// Gets the unique identifier of the sovereign country.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets the official name of the sovereign nation.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the capital city.
    /// </summary>
    public string CapitalCity { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the constitutional system of governance.
    /// </summary>
    public string GovernmentType { get; set; } = "Democratic Republic";

    /// <summary>
    /// Gets or sets whether this sovereign country is autonomous and governed by an AI government.
    /// </summary>
    public bool IsAIControlled { get; set; }

    /// <summary>
    /// Gets or sets a description of the latest decision enacted by the sovereign AI government.
    /// </summary>
    public string? LastAIDecision { get; set; }

    /// <summary>
    /// Gets or sets an executive briefing summary of the most recent offline National Yield catch-up.
    /// </summary>
    public string? LastOfflineCatchUpSummary { get; set; }

    /// <summary>
    /// Gets or sets the sovereign land area in square kilometers.
    /// </summary>
    public double TerritorySizeSqKm { get; init; } = 500000;

    /// <summary>
    /// Gets the national traits and cultural characteristics.
    /// </summary>
    public List<string> NationalTraits { get; init; } = new();

    /// <summary>
    /// Gets or sets the baseline political and societal stability (0.0 to 100.0).
    /// </summary>
    public double BaselineStability { get; set; } = 75.0;

    /// <summary>
    /// Gets or sets the sovereign public approval / happiness rating percentage (0.0 to 100.0).
    /// Defaults to the standard national baseline (68.5).
    /// </summary>
    public double HappinessRating { get; set; } = 68.5;

    /// <summary>
    /// Alias for <see cref="HappinessRating"/> representing public approval sentiment.
    /// </summary>
    [JsonIgnore]
    public double ApprovalRating
    {
        get => HappinessRating;
        set => HappinessRating = value;
    }

    /// <summary>
    /// Gets or sets the authoritative national taxation policy for this sovereign country.
    /// Defaults to the standard national baseline (Income: 25%, Corporate: 20%, Tariff: 5%).
    /// </summary>
    public TaxPolicy TaxPolicy { get; set; } = new();

    /// <summary>
    /// Gets or sets the authoritative ministry budget defining expenditure allocations across government sectors.
    /// Defaults to the standard national baseline (Defense: 500M, Health: 400M, Education: 350M, Infra: 600M, Science: 250M).
    /// </summary>
    public MinistryBudget MinistryBudget { get; set; } = new();

    /// <summary>
    /// Gets or sets the sovereign trade balance in REPU.
    /// Defaults to the standard national baseline (2,100,000,000.0).
    /// </summary>
    public double TradeBalance { get; set; } = 2_100_000_000.0;

    /// <summary>
    /// Gets or sets the latest citizen welfare and public approval feedback record resulting from domestic policy actions.
    /// Null if no domestic policy adjustment has been enacted yet.
    /// </summary>
    public CitizenFeedbackRecord? LatestCitizenFeedback { get; set; }

    /// <summary>
    /// Gets or sets the latest quarterly fiscal report summarizing revenue, expenditure, and treasury outcomes.
    /// Null if no quarterly fiscal report has been executed yet.
    /// </summary>
    public FiscalQuarterReport? LatestFiscalReport { get; set; }

    /// <summary>
    /// Gets or sets the last fiscal quarter index processed and applied to this country's treasury.
    /// </summary>
    public int LastProcessedFiscalQuarter { get; set; }

    /// <summary>
    /// Gets or sets the sovereign gross domestic product (GDP) in REPU.
    /// Defaults to the standard national baseline (450,000,000,000.0).
    /// </summary>
    public double GrossDomesticProduct { get; set; } = 450_000_000_000.0;

    /// <summary>
    /// Alias for <see cref="GrossDomesticProduct"/>.
    /// </summary>
    [JsonIgnore]
    public double GDP
    {
        get => GrossDomesticProduct;
        set => GrossDomesticProduct = value;
    }

    /// <summary>
    /// Gets or sets the latest National Yield calculation snapshot for this sovereign country.
    /// Null if no yield cycle has been executed yet.
    /// </summary>
    public NationalYieldSnapshot? LatestYieldSnapshot { get; set; }

    /// <summary>
    /// Gets the authoritative National Yield state representing the country's productive, human, institutional, and resource capacities.
    /// Each sovereign country owns an independent, isolated instance.
    /// </summary>
    public NationalYield Yield { get; init; } = new();

    /// <summary>
    /// Gets the authoritative sovereign treasury holding accumulated liquid REPU balance.
    /// Each sovereign country owns an independent, isolated instance.
    /// </summary>
    public RepuTreasury Treasury { get; init; } = new();

    /// <summary>
    /// Gets the authoritative sovereign physical goods inventory holding accumulated resource stockpiles.
    /// Each sovereign country owns an independent, isolated instance.
    /// </summary>
    public ResourceInventory Inventory { get; init; } = new();

    /// <summary>
    /// Retrieves the available quantity of the specified resource type from this country's physical inventory.
    /// </summary>
    public double GetResourceQuantity(string resourceType) => Inventory.GetQuantity(resourceType);

    /// <summary>
    /// Adds physical goods to this country's resource stockpile.
    /// </summary>
    public void AddResource(string resourceType, double quantity) => Inventory.Add(resourceType, quantity);

    /// <summary>
    /// Gets the list of domestic resource nodes associated with this sovereign country.
    /// Each sovereign country owns an independent, isolated list.
    /// </summary>
    public List<ResourceNode> ResourceNodes { get; init; } = new();

    /// <summary>
    /// Associates a domestic resource node with this sovereign country.
    /// Preserves legitimate unassigned node association by assigning ownership to this country.
    /// Explicitly rejects nodes already owned by another country or having invalid IDs.
    /// </summary>
    /// <param name="node">The resource node to associate.</param>
    /// <returns><c>true</c> if association succeeded; <c>false</c> if rejected due to mismatched ownership or invalid IDs.</returns>
    public bool AssociateResourceNode(ResourceNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(node.Id))
        {
            return false;
        }

        // A country must not associate a resource node owned by another country
        if (!string.IsNullOrWhiteSpace(node.CountryId) &&
            !string.Equals(node.CountryId.Trim(), Id.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Preserve legitimate unassigned-node association
        if (string.IsNullOrWhiteSpace(node.CountryId))
        {
            node.CountryId = Id.Trim();
        }

        if (!ResourceNodes.Any(n => string.Equals(n.Id?.Trim(), node.Id.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ResourceNodes.Add(node);
        }

        return true;
    }

    /// <summary>
    /// Gets or sets the authoritative Republic simulation timestamp of the most recently credited
    /// six-hour domestic resource production cycle boundary for this sovereign country.
    /// Null if no production cycle has been credited yet.
    /// </summary>
    public RepublicTime? LastResourceProductionBoundary { get; set; }

    private readonly HashSet<string> _appliedProductionBoundaries = new(StringComparer.Ordinal);

    /// <summary>
    /// Determines whether the specified cycle boundary has already had domestic production credited.
    /// </summary>
    public bool HasAppliedProductionBoundary(string boundaryId)
    {
        if (string.IsNullOrWhiteSpace(boundaryId))
        {
            return false;
        }

        lock (_appliedProductionBoundaries)
        {
            return _appliedProductionBoundaries.Contains(boundaryId);
        }
    }

    /// <summary>
    /// Records that the specified cycle boundary has had domestic production credited.
    /// </summary>
    public bool RecordProductionBoundaryApplied(string boundaryId)
    {
        if (string.IsNullOrWhiteSpace(boundaryId))
        {
            return false;
        }

        lock (_appliedProductionBoundaries)
        {
            return _appliedProductionBoundaries.Add(boundaryId);
        }
    }

    /// <summary>
    /// Executes domestic resource production from the specified associated resource node for an authoritative six-hour cycle boundary.
    /// </summary>
    public ResourceProductionResult ProduceResource(ResourceNode node, RepublicTime boundary) =>
        new Republic.Core.World.Services.ResourceProductionService().Produce(this, node, boundary);

    /// <summary>
    /// Gets or sets the authoritative Republic simulation timestamp of the most recently credited
    /// six-hour National Yield cycle boundary for this sovereign country.
    /// Null if no cycle boundary has been credited yet.
    /// </summary>
    public RepublicTime? LastCreditedBoundary { get; set; }

    /// <summary>
    /// Gets or sets the authoritative snapshot of temporary tapering founding buffs evaluated for this sovereign country.
    /// Null if not yet evaluated.
    /// </summary>
    public FoundingBuffSnapshot? FoundingBuffs { get; set; }

    /// <summary>
    /// Gets or sets the sovereign State Capacity institutional inputs determining conversion rate into results.
    /// Defaults to 0.6 across all seven inputs so existing countries are not suddenly at zero.
    /// Each sovereign country owns an independent, isolated instance.
    /// </summary>
    public StateCapacityInputs StateCapacity { get; set; } = new();

    /// <summary>
    /// Default State Capacity factor for a vacant cabinet portfolio (0.3).
    /// </summary>
    public const double VacantPortfolioFactor = 0.3;

    /// <summary>
    /// Gets the authoritative dictionary of cabinet appointments by portfolio for this sovereign country.
    /// A portfolio mapping to null or absent indicates a vacant portfolio.
    /// Each sovereign country owns an independent, isolated instance.
    /// </summary>
    public Dictionary<CabinetPortfolio, Minister?> Cabinet { get; init; } = new();

    /// <summary>
    /// Gets the list of development projects associated with this sovereign country.
    /// Each sovereign country owns an independent, isolated list.
    /// This is the authoritative collection for all development projects in Republic.
    /// </summary>
    public List<DevelopmentProject> Projects { get; init; } = new();

    /// <summary>
    /// Gets or sets the latest systemic treasury consequence evaluated for this sovereign country.
    /// Null if no low-treasury consequence has applied yet.
    /// </summary>
    public TreasuryConsequence? LatestTreasuryConsequence { get; set; }

    /// <summary>
    /// Gets the authoritative list of proactive executive recommendations generated by the Chief of Staff.
    /// Preserves accepted, rejected, and unresolved recommendation histories across simulation saves.
    /// </summary>
    public List<ChiefOfStaffRecommendation> ChiefOfStaffRecommendations { get; set; } = new();

    /// <summary>
    /// Gets or sets the latest comprehensive national security leadership assessment.
    /// </summary>
    public SecurityLeadershipAssessment? LatestSecurityAssessment { get; set; }

    /// <summary>
    /// Gets or sets the active security threat reports monitored by security leadership.
    /// </summary>
    public List<SecurityThreatReport> ActiveSecurityThreats { get; set; } = new();

    /// <summary>
    /// Gets or sets the latest ministerial advice evaluated across all six constitutional portfolios.
    /// </summary>
    public List<MinisterialAdvice> LatestMinisterialAdvice { get; set; } = new();

    /// <summary>
    /// Gets the authoritative media outlets operating within the sovereign republic.
    /// </summary>
    public List<MediaOutlet> MediaOutlets { get; set; } = new();

    /// <summary>
    /// Gets the published and historical media reports evaluating government actions and national events.
    /// </summary>
    public List<MediaReport> MediaReports { get; set; } = new();

    /// <summary>
    /// Gets the active commercial enterprises, industrialists, and private-sector investors.
    /// </summary>
    public List<PrivateInvestor> PrivateInvestors { get; set; } = new();

    /// <summary>
    /// Gets the pending, accepted, or negotiated commercial investment proposals submitted to the administration.
    /// </summary>
    public List<InvestmentProposal> InvestmentProposals { get; set; } = new();

    /// <summary>
    /// Gets the active public-private co-investment facilities currently under development or completed.
    /// </summary>
    public List<PrivateInvestmentProject> ActivePrivateInvestments { get; set; } = new();

    private readonly HashSet<string> _appliedConsequenceBoundaryIds = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets the collection of boundary identifiers where treasury consequences have already been evaluated.
    /// </summary>
    public IReadOnlyCollection<string> AppliedConsequenceBoundaryIds
    {
        get
        {
            lock (_appliedConsequenceBoundaryIds)
            {
                return _appliedConsequenceBoundaryIds.ToArray();
            }
        }
    }

    /// <summary>
    /// Determines whether the specified cycle boundary has already had treasury consequences evaluated.
    /// </summary>
    public bool HasAppliedConsequenceBoundary(string boundaryId)
    {
        if (string.IsNullOrWhiteSpace(boundaryId))
        {
            return false;
        }

        lock (_appliedConsequenceBoundaryIds)
        {
            return _appliedConsequenceBoundaryIds.Contains(boundaryId);
        }
    }

    /// <summary>
    /// Records that the specified cycle boundary has had treasury consequences evaluated.
    /// </summary>
    public bool RecordConsequenceBoundaryApplied(string boundaryId)
    {
        if (string.IsNullOrWhiteSpace(boundaryId))
        {
            return false;
        }

        lock (_appliedConsequenceBoundaryIds)
        {
            return _appliedConsequenceBoundaryIds.Add(boundaryId);
        }
    }

    /// <summary>
    /// Evaluates and applies the low-treasury approval consequence at a six-hour cycle boundary.
    /// If the treasury balance is below R100,000,000, approval falls by 2 points (clamped at 0).
    /// Runs at most once per country per boundary.
    /// </summary>
    public TreasuryConsequence? ApplyTreasuryConsequence(RepublicTime boundary)
    {
        return TreasuryConsequence.Apply(this, boundary);
    }

    /// <summary>
    /// Gets or sets the sovereign ruling political party founded in this country, or null if none exists.
    /// The first party founded is always the ruling party.
    /// </summary>
    public PoliticalParty? RulingParty { get; set; }

    /// <summary>
    /// Gets or sets the sovereign opposition political party founded in this country, or null if none exists.
    /// The second party founded is always the opposition party.
    /// </summary>
    public PoliticalParty? OppositionParty { get; set; }

    /// <summary>
    /// Alias for <see cref="RulingParty"/> maintaining backward-compatibility with Phase 6.1 single-party slice.
    /// </summary>
    [JsonIgnore]
    public PoliticalParty? Party
    {
        get => RulingParty;
        set => RulingParty = value;
    }

    /// <summary>
    /// Alias for <see cref="RulingParty"/>.
    /// </summary>
    [JsonIgnore]
    public PoliticalParty? PoliticalParty
    {
        get => RulingParty;
        set => RulingParty = value;
    }

    /// <summary>
    /// Gets the list of active political parties in this sovereign country (up to two: ruling and opposition).
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<PoliticalParty> Parties
    {
        get
        {
            var list = new List<PoliticalParty>(2);
            if (RulingParty != null) list.Add(RulingParty);
            if (OppositionParty != null) list.Add(OppositionParty);
            return list;
        }
    }

    /// <summary>
    /// Founds a sovereign political party for this country.
    /// The first party becomes the ruling party; the second becomes the opposition party.
    /// A third founding is rejected and returns null.
    /// Names must be trimmed and non-empty. The two party names cannot match (case-insensitive); duplicate names are rejected.
    /// Opposition support starts at 10.0 while ruling support remains where it is.
    /// Grounded strictly in Republic simulation clock boundaries; never calls DateTime.Now.
    /// </summary>
    public PoliticalParty? FoundParty(string? name, RepublicTime foundedBoundary, double support = PoliticalParty.InitialSupport)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var trimmedName = name.Trim();

        // 1. If no ruling party yet: first party becomes Ruling party
        if (RulingParty == null)
        {
            var rulingParty = new PoliticalParty(trimmedName, Id, foundedBoundary, support, PoliticalPartyRole.Ruling);
            RulingParty = rulingParty;
            return rulingParty;
        }

        // 2. If ruling party exists, check for duplicate name (case-insensitive)
        if (string.Equals(RulingParty.Name, trimmedName, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // 3. If opposition party does not exist yet: second party becomes Opposition party
        if (OppositionParty == null)
        {
            // Opposition support starts at 10. Ruling support stays where it is.
            var oppositionParty = new PoliticalParty(trimmedName, Id, foundedBoundary, PoliticalParty.InitialSupport, PoliticalPartyRole.Opposition);
            OppositionParty = oppositionParty;
            return oppositionParty;
        }

        // 4. A third founding is rejected
        return null;
    }

    /// <summary>
    /// Overload for founding a political party using an authoritative Republic clock.
    /// </summary>
    public PoliticalParty? FoundParty(string? name, IRepublicClock clock, double support = PoliticalParty.InitialSupport)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return FoundParty(name, clock.CurrentTime, support);
    }

    /// <summary>
    /// Overload for founding a political party defaulting to the country's founding time or last boundary.
    /// </summary>
    public PoliticalParty? FoundParty(string? name, double support = PoliticalParty.InitialSupport)
    {
        return FoundParty(name, LastCreditedBoundary ?? FoundingTime, support);
    }

    /// <summary>
    /// Attempts to found a political party for this country, returning true on success.
    /// </summary>
    public bool TryFoundParty(string? name, RepublicTime foundedBoundary, out PoliticalParty? party, double support = PoliticalParty.InitialSupport)
    {
        party = FoundParty(name, foundedBoundary, support);
        return party != null;
    }

    /// <summary>
    /// Gets or sets the sovereign interest group founded in this country, or null if none exists.
    /// In this slice, a country can have at most one interest group.
    /// </summary>
    public InterestGroup? InterestGroup { get; set; }

    /// <summary>
    /// Gets the list of active interest groups in this sovereign country (up to one in this slice).
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<InterestGroup> InterestGroups => InterestGroup != null ? [InterestGroup] : [];

    /// <summary>
    /// Founds a sovereign interest group for this country.
    /// In this slice, a country can have at most one interest group. A second founding is rejected and returns null.
    /// Names must be trimmed and non-empty. Empty or whitespace-only names are rejected and return null.
    /// Grounded strictly in deterministic simulation rules; never calls DateTime.Now.
    /// </summary>
    public InterestGroup? FoundInterestGroup(string? name, InterestGroupFocus focus, double influence = InterestGroup.InitialInfluence)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (InterestGroup != null)
        {
            return null;
        }

        var trimmedName = name.Trim();
        var group = new InterestGroup(trimmedName, Id, focus, influence);
        InterestGroup = group;
        return group;
    }

    /// <summary>
    /// Attempts to found an interest group for this country, returning true on success.
    /// </summary>
    public bool TryFoundInterestGroup(string? name, InterestGroupFocus focus, out InterestGroup? group, double influence = InterestGroup.InitialInfluence)
    {
        group = FoundInterestGroup(name, focus, influence);
        return group != null;
    }

    private readonly List<CountryBill> _bills = new();

    /// <summary>
    /// Gets or sets the primary or most recent legislative bill proposed in this country, or null if none exists.
    /// </summary>
    public CountryBill? Bill
    {
        get => _bills.LastOrDefault();
        set
        {
            if (value == null)
            {
                _bills.Clear();
            }
            else
            {
                var idx = _bills.FindIndex(b => b.Title.Equals(value.Title, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0)
                {
                    _bills[idx] = value;
                }
                else
                {
                    _bills.Add(value);
                }
            }
        }
    }

    /// <summary>
    /// Alias for <see cref="Bill"/>.
    /// </summary>
    [JsonIgnore]
    public CountryBill? CountryBill
    {
        get => Bill;
        set => Bill = value;
    }

    /// <summary>
    /// Gets the bill currently pending a legislative vote in this country, or null if none is pending.
    /// In this slice, a country can have at most one pending bill.
    /// </summary>
    [JsonIgnore]
    public CountryBill? PendingBill => _bills.FirstOrDefault(b => b.Status == CountryBillStatus.Pending);

    /// <summary>
    /// Gets the list of legislative bills proposed in this sovereign country.
    /// </summary>
    public IReadOnlyList<CountryBill> Bills
    {
        get => _bills;
        init
        {
            if (value != null)
            {
                foreach (var b in value)
                {
                    if (!_bills.Any(existing => existing.Title.Equals(b.Title, StringComparison.OrdinalIgnoreCase)))
                    {
                        _bills.Add(b);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Proposes a new legislative bill for this country.
    /// In this slice, a country can have at most one pending bill. A second pending bill is rejected and returns null.
    /// Titles must be trimmed and non-empty. Empty or whitespace-only titles are rejected and return null.
    /// Grounded strictly in deterministic simulation rules; never calls DateTime.Now.
    /// </summary>
    public CountryBill? ProposeBill(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        if (PendingBill != null)
        {
            return null;
        }

        var trimmedTitle = title.Trim();
        var bill = new CountryBill(trimmedTitle, Id, CountryBillStatus.Pending, null);
        _bills.Add(bill);
        return bill;
    }

    /// <summary>
    /// Attempts to propose a new legislative bill for this country, returning true on success.
    /// </summary>
    public bool TryProposeBill(string? title, out CountryBill? bill)
    {
        bill = ProposeBill(title);
        return bill != null;
    }

    /// <summary>
    /// Conducts a legislative vote on the specified bill at the authoritative Republic simulation clock boundary.
    /// A vote requires an active ruling party. Without one, the vote is rejected and the bill remains Pending.
    /// If the ruling party exists, the bill passes. Voting the same bill twice does nothing and does not alter the boundary.
    /// Grounded strictly in Republic simulation clock boundaries; never calls DateTime.Now.
    /// </summary>
    public CountryBill? VoteBill(CountryBill bill, RepublicTime votedBoundary)
    {
        if (bill == null)
        {
            return null;
        }

        // Country isolation: cannot vote on another country's bill
        if (!string.Equals(bill.CountryId, Id, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Voting the same bill twice does nothing; preserves existing status and voted boundary
        if (bill.Status != CountryBillStatus.Pending)
        {
            return bill;
        }

        // Vote requires a ruling party. Without one, the vote is rejected and the bill stays Pending.
        if (RulingParty == null)
        {
            return bill;
        }

        // If the ruling party exists, the bill passes
        var passedBill = bill with
        {
            Status = CountryBillStatus.Passed,
            VotedBoundary = votedBoundary
        };

        var index = _bills.FindIndex(b => b.Title.Equals(bill.Title, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            _bills[index] = passedBill;
        }
        else
        {
            _bills.Add(passedBill);
        }

        return passedBill;
    }

    /// <summary>
    /// Conducts a legislative vote on the currently pending bill (or latest bill) at the authoritative Republic clock boundary.
    /// </summary>
    public CountryBill? VoteBill(RepublicTime votedBoundary)
    {
        var targetBill = PendingBill ?? Bill;
        if (targetBill == null)
        {
            return null;
        }

        return VoteBill(targetBill, votedBoundary);
    }

    /// <summary>
    /// Conducts a legislative vote on the currently pending bill using an authoritative Republic clock.
    /// </summary>
    public CountryBill? VoteBill(IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return VoteBill(clock.CurrentTime);
    }

    /// <summary>
    /// Conducts a legislative vote on the specified bill using an authoritative Republic clock.
    /// </summary>
    public CountryBill? VoteBill(CountryBill bill, IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return VoteBill(bill, clock.CurrentTime);
    }

    /// <summary>
    /// Conducts a legislative vote on the bill identified by title at the authoritative clock boundary.
    /// </summary>
    public CountryBill? VoteBill(string title, RepublicTime votedBoundary)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var bill = _bills.FirstOrDefault(b => b.Title.Equals(title.Trim(), StringComparison.OrdinalIgnoreCase));
        if (bill == null)
        {
            return null;
        }

        return VoteBill(bill, votedBoundary);
    }

    /// <summary>
    /// Conducts a legislative vote on the bill identified by title using an authoritative Republic clock.
    /// </summary>
    public CountryBill? VoteBill(string title, IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return VoteBill(title, clock.CurrentTime);
    }

    private readonly List<CountryElection> _elections = new();

    /// <summary>
    /// Gets the most recent national election held in this country, or null if none has been held.
    /// </summary>
    public CountryElection? LastElection => _elections.LastOrDefault();

    /// <summary>
    /// Alias for <see cref="LastElection"/>.
    /// </summary>
    [JsonIgnore]
    public CountryElection? Election => LastElection;

    /// <summary>
    /// Gets the list of democratic national elections conducted in this sovereign country.
    /// </summary>
    public IReadOnlyList<CountryElection> Elections
    {
        get => _elections;
        init
        {
            if (value != null)
            {
                foreach (var e in value)
                {
                    if (!_elections.Any(existing => existing.Boundary == e.Boundary))
                    {
                        _elections.Add(e);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Holds a democratic national election between the country's ruling party and opposition party at the specified clock boundary.
    /// Requires both a ruling party and an opposition party; otherwise the election is rejected and nothing changes.
    /// The party with higher Support wins; on a tie, the ruling party wins.
    /// The winner becomes the ruling party, and the loser becomes the opposition party.
    /// A second election call at the same clock boundary is idempotent and does not swap roles again.
    /// Grounded strictly in Republic simulation clock boundaries; never calls DateTime.Now.
    /// </summary>
    public CountryElection? HoldElection(RepublicTime boundary)
    {
        // HoldElection requires a ruling party and an opposition party. Otherwise it is rejected and nothing changes.
        if (RulingParty == null || OppositionParty == null)
        {
            return null;
        }

        // A second election at the same boundary does nothing.
        if (LastElection != null && LastElection.Boundary == boundary)
        {
            return LastElection;
        }

        var ruling = RulingParty;
        var opposition = OppositionParty;

        // The party with higher Support wins. On a tie, the ruling party wins.
        bool oppositionWins = opposition.Support > ruling.Support;
        var winner = oppositionWins ? opposition : ruling;
        var loser = oppositionWins ? ruling : opposition;

        // The winner becomes the ruling party. The loser becomes the opposition.
        var newRuling = winner with { Role = PoliticalPartyRole.Ruling };
        var newOpposition = loser with { Role = PoliticalPartyRole.Opposition };

        RulingParty = newRuling;
        OppositionParty = newOpposition;

        var election = new CountryElection(
            Id,
            boundary,
            newRuling.Name,
            newOpposition.Name,
            newRuling.Support,
            newOpposition.Support);

        _elections.Add(election);
        return election;
    }

    /// <summary>
    /// Holds a democratic national election between the ruling party and opposition party using an authoritative Republic clock.
    /// </summary>
    public CountryElection? HoldElection(IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return HoldElection(clock.CurrentTime);
    }

    /// <summary>
    /// Attempts to hold a democratic national election in this country, returning true on success.
    /// </summary>
    public bool TryHoldElection(RepublicTime boundary, out CountryElection? election)
    {
        election = HoldElection(boundary);
        return election != null;
    }

    /// <summary>
    /// Attempts to hold a democratic national election in this country using an authoritative Republic clock.
    /// </summary>
    public bool TryHoldElection(IRepublicClock clock, out CountryElection? election)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return TryHoldElection(clock.CurrentTime, out election);
    }

    private readonly List<DiplomaticRelation> _relations = new();

    /// <summary>
    /// Gets the most recent diplomatic relation opened by this sovereign country, or null if none exists.
    /// </summary>
    public DiplomaticRelation? Relation => _relations.LastOrDefault();

    /// <summary>
    /// Alias for <see cref="Relation"/>.
    /// </summary>
    [JsonIgnore]
    public DiplomaticRelation? DiplomaticRelation => Relation;

    /// <summary>
    /// Alias for <see cref="Relation"/>.
    /// </summary>
    [JsonIgnore]
    public DiplomaticRelation? LastRelation => Relation;

    /// <summary>
    /// Gets the read-only list of diplomatic relations opened by this sovereign country.
    /// </summary>
    public IReadOnlyList<DiplomaticRelation> Relations
    {
        get => _relations;
        init
        {
            if (value != null)
            {
                foreach (var r in value)
                {
                    if (!_relations.Any(existing => existing.TargetCountryId == r.TargetCountryId))
                    {
                        _relations.Add(r);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Alias for <see cref="Relations"/>.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiplomaticRelation> DiplomaticRelations => _relations;

    /// <summary>
    /// Opens a diplomatic relation from this sovereign country to the specified target country.
    /// Requires two different countries; a country cannot open a relation with itself.
    /// A second relation between the same ordered pair is rejected.
    /// The reverse pair is a different relation in this slice.
    /// Returns the newly opened relation, or null if the request was rejected.
    /// Grounded strictly in Republic simulation clock boundaries; never calls DateTime.Now.
    /// </summary>
    public DiplomaticRelation? OpenRelation(
        string targetCountryId,
        RepublicTime boundary,
        DiplomaticRelationStatus status = DiplomaticRelationStatus.Unrecognized)
    {
        if (string.IsNullOrWhiteSpace(targetCountryId))
        {
            return null;
        }

        var trimmedTargetId = targetCountryId.Trim();

        // OpenRelation requires two different countries. A country cannot open a relation with itself.
        if (string.Equals(trimmedTargetId, Id, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // A second relation between the same ordered pair is rejected.
        if (_relations.Any(r => string.Equals(r.ToCountryId, trimmedTargetId, StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        var relation = new DiplomaticRelation(Id, trimmedTargetId, status, boundary);
        _relations.Add(relation);
        return relation;
    }

    /// <summary>
    /// Opens a diplomatic relation from this sovereign country to the specified target country.
    /// </summary>
    public DiplomaticRelation? OpenRelation(
        Country targetCountry,
        RepublicTime boundary,
        DiplomaticRelationStatus status = DiplomaticRelationStatus.Unrecognized)
    {
        if (targetCountry == null)
        {
            return null;
        }

        return OpenRelation(targetCountry.Id, boundary, status);
    }

    /// <summary>
    /// Opens a diplomatic relation from this sovereign country to the target country using an authoritative Republic clock.
    /// </summary>
    public DiplomaticRelation? OpenRelation(
        string targetCountryId,
        IRepublicClock clock,
        DiplomaticRelationStatus status = DiplomaticRelationStatus.Unrecognized)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return OpenRelation(targetCountryId, clock.CurrentTime, status);
    }

    /// <summary>
    /// Opens a diplomatic relation from this sovereign country to the target country using an authoritative Republic clock.
    /// </summary>
    public DiplomaticRelation? OpenRelation(
        Country targetCountry,
        IRepublicClock clock,
        DiplomaticRelationStatus status = DiplomaticRelationStatus.Unrecognized)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return OpenRelation(targetCountry, clock.CurrentTime, status);
    }

    /// <summary>
    /// Attempts to open a diplomatic relation from this country to the specified target country.
    /// </summary>
    public bool TryOpenRelation(
        string targetCountryId,
        RepublicTime boundary,
        out DiplomaticRelation? relation,
        DiplomaticRelationStatus status = DiplomaticRelationStatus.Unrecognized)
    {
        relation = OpenRelation(targetCountryId, boundary, status);
        return relation != null;
    }

    /// <summary>
    /// Attempts to open a diplomatic relation from this country to the target country using an authoritative Republic clock.
    /// </summary>
    public bool TryOpenRelation(
        string targetCountryId,
        IRepublicClock clock,
        out DiplomaticRelation? relation,
        DiplomaticRelationStatus status = DiplomaticRelationStatus.Unrecognized)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return TryOpenRelation(targetCountryId, clock.CurrentTime, out relation, status);
    }

    /// <summary>
    /// Attempts to open a diplomatic relation from this country to the target country using an authoritative Republic clock.
    /// </summary>
    public bool TryOpenRelation(
        Country targetCountry,
        IRepublicClock clock,
        out DiplomaticRelation? relation,
        DiplomaticRelationStatus status = DiplomaticRelationStatus.Unrecognized)
    {
        ArgumentNullException.ThrowIfNull(clock);
        if (targetCountry == null)
        {
            relation = null;
            return false;
        }

        return TryOpenRelation(targetCountry.Id, clock.CurrentTime, out relation, status);
    }

    /// <summary>
    /// Retrieves the diplomatic relation opened by this country toward the specified target country, if any.
    /// </summary>
    public DiplomaticRelation? GetRelation(string targetCountryId)
    {
        if (string.IsNullOrWhiteSpace(targetCountryId))
        {
            return null;
        }

        return _relations.FirstOrDefault(r => string.Equals(r.ToCountryId, targetCountryId.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks whether this sovereign country has an opened diplomatic relation toward the specified target country.
    /// </summary>
    public bool HasRelation(string targetCountryId) => GetRelation(targetCountryId) != null;

    /// <summary>
    /// Formally recognizes an existing diplomatic relation opened toward the specified target country.
    /// A country cannot recognize a relation that has not been opened.
    /// Repeating recognition is safe and idempotent without duplicating records or changing unrelated state.
    /// Grounded strictly in Republic simulation clock boundaries; never calls DateTime.Now.
    /// </summary>
    public DiplomaticRelation? RecognizeRelation(string targetCountryId, RepublicTime boundary)
    {
        if (string.IsNullOrWhiteSpace(targetCountryId))
        {
            return null;
        }

        var trimmedTargetId = targetCountryId.Trim();

        // Cannot recognize own nation
        if (string.Equals(trimmedTargetId, Id, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var index = _relations.FindIndex(r => string.Equals(r.ToCountryId, trimmedTargetId, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            // A country cannot recognize a relation that has not been opened
            return null;
        }

        var existing = _relations[index];
        if (existing.Status == DiplomaticRelationStatus.Recognized)
        {
            // Repeating recognition is handled safely without duplicating records or changing unrelated state
            return existing;
        }

        var recognized = existing with
        {
            Status = DiplomaticRelationStatus.Recognized,
            RecognizedBoundary = boundary
        };

        _relations[index] = recognized;
        return recognized;
    }

    /// <summary>
    /// Formally recognizes an existing diplomatic relation opened toward the target country.
    /// </summary>
    public DiplomaticRelation? RecognizeRelation(Country targetCountry, RepublicTime boundary)
    {
        if (targetCountry == null)
        {
            return null;
        }

        return RecognizeRelation(targetCountry.Id, boundary);
    }

    /// <summary>
    /// Formally recognizes an existing diplomatic relation toward the target country using an authoritative Republic clock.
    /// </summary>
    public DiplomaticRelation? RecognizeRelation(string targetCountryId, IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return RecognizeRelation(targetCountryId, clock.CurrentTime);
    }

    /// <summary>
    /// Formally recognizes an existing diplomatic relation toward the target country using an authoritative Republic clock.
    /// </summary>
    public DiplomaticRelation? RecognizeRelation(Country targetCountry, IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return RecognizeRelation(targetCountry, clock.CurrentTime);
    }

    /// <summary>
    /// Attempts to formally recognize an existing diplomatic relation toward the target country.
    /// </summary>
    public bool TryRecognizeRelation(string targetCountryId, RepublicTime boundary, out DiplomaticRelation? relation)
    {
        relation = RecognizeRelation(targetCountryId, boundary);
        return relation != null && relation.IsRecognized;
    }

    /// <summary>
    /// Attempts to formally recognize an existing diplomatic relation toward the target country using an authoritative Republic clock.
    /// </summary>
    public bool TryRecognizeRelation(string targetCountryId, IRepublicClock clock, out DiplomaticRelation? relation)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return TryRecognizeRelation(targetCountryId, clock.CurrentTime, out relation);
    }

    private readonly List<DiplomaticAgreement> _agreements = new();

    /// <summary>
    /// Gets the most recent bilateral diplomatic agreement (trade agreement or alliance), or null if none exists.
    /// </summary>
    public DiplomaticAgreement? Agreement => _agreements.LastOrDefault();

    /// <summary>
    /// Gets the most recent bilateral trade agreement, or null if none exists.
    /// </summary>
    [JsonIgnore]
    public DiplomaticAgreement? TradeAgreement => _agreements.LastOrDefault(a => a.IsTrade);

    /// <summary>
    /// Gets the most recent bilateral alliance, or null if none exists.
    /// </summary>
    [JsonIgnore]
    public DiplomaticAgreement? Alliance => _agreements.LastOrDefault(a => a.IsAlliance);

    /// <summary>
    /// Gets the active bilateral trade agreement, or null if none is currently active.
    /// </summary>
    [JsonIgnore]
    public DiplomaticAgreement? ActiveTradeAgreement => _agreements.LastOrDefault(a => a.IsTrade && a.IsActive);

    /// <summary>
    /// Gets the active bilateral alliance, or null if none is currently active.
    /// </summary>
    [JsonIgnore]
    public DiplomaticAgreement? ActiveAlliance => _agreements.LastOrDefault(a => a.IsAlliance && a.IsActive);

    /// <summary>
    /// Gets the read-only list of all diplomatic agreements attached to this sovereign country.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiplomaticAgreement> Agreements => _agreements;

    /// <summary>
    /// Gets the read-only list of active (accepted) diplomatic agreements attached to this country.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiplomaticAgreement> ActiveAgreements =>
        _agreements.Where(a => a.IsActive).ToList().AsReadOnly();

    /// <summary>
    /// Gets the read-only list of pending diplomatic proposals attached to this country.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiplomaticAgreement> PendingAgreements =>
        _agreements.Where(a => a.IsPending).ToList().AsReadOnly();

    /// <summary>
    /// Retrieves a diplomatic agreement by its unique identifier.
    /// </summary>
    public DiplomaticAgreement? GetAgreement(string agreementId)
    {
        if (string.IsNullOrWhiteSpace(agreementId))
        {
            return null;
        }

        return _agreements.FirstOrDefault(a => string.Equals(a.Id, agreementId.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Retrieves the most recent agreement with the specified foreign country and agreement type.
    /// </summary>
    public DiplomaticAgreement? GetAgreementWith(string targetCountryId, DiplomaticAgreementType type)
    {
        if (string.IsNullOrWhiteSpace(targetCountryId))
        {
            return null;
        }

        var trimmed = targetCountryId.Trim();
        return _agreements.LastOrDefault(a => a.AgreementType == type && a.Involves(trimmed));
    }

    /// <summary>
    /// Checks whether this country has an active (accepted) agreement of the specified type with the target country.
    /// </summary>
    public bool HasActiveAgreement(string targetCountryId, DiplomaticAgreementType type)
    {
        var existing = GetAgreementWith(targetCountryId, type);
        return existing != null && existing.IsActive;
    }

    /// <summary>
    /// Checks whether this country has a pending agreement proposal of the specified type with the target country.
    /// </summary>
    public bool HasPendingAgreement(string targetCountryId, DiplomaticAgreementType type)
    {
        var existing = GetAgreementWith(targetCountryId, type);
        return existing != null && existing.IsPending;
    }

    /// <summary>
    /// Proposes a bilateral diplomatic agreement (Trade or Alliance) to a target country.
    /// Requires an existing diplomatic relation. For Alliances, requires an established (recognized) relationship.
    /// Prevents duplicate active or pending agreements for the same country pair and agreement type.
    /// Grounded strictly in Republic simulation clock boundaries; never calls DateTime.Now.
    /// </summary>
    public DiplomaticAgreement? ProposeAgreement(
        string targetCountryId,
        DiplomaticAgreementType type,
        RepublicTime boundary,
        Country? targetCountry = null)
    {
        if (string.IsNullOrWhiteSpace(targetCountryId))
        {
            return null;
        }

        var trimmedTargetId = targetCountryId.Trim();

        // Self-agreement prohibited
        if (string.Equals(trimmedTargetId, Id, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Must have an existing diplomatic relation
        var relation = GetRelation(trimmedTargetId);
        if (relation == null)
        {
            return null;
        }

        // Alliances require an established (recognized) relationship
        if (type == DiplomaticAgreementType.Alliance && !relation.IsRecognized)
        {
            return null;
        }

        // Prevent duplicate active or pending agreements for the same country pair and agreement type
        if (_agreements.Any(a => a.AgreementType == type && a.Involves(trimmedTargetId) && (a.IsActive || a.IsPending)))
        {
            return null;
        }

        var agreement = new DiplomaticAgreement(
            Id,
            trimmedTargetId,
            type,
            DiplomaticAgreementStatus.Pending,
            boundary);

        _agreements.Add(agreement);

        if (targetCountry != null && !targetCountry._agreements.Any(a => a.Id == agreement.Id))
        {
            targetCountry._agreements.Add(agreement);
        }

        return agreement;
    }

    /// <summary>
    /// Proposes a bilateral diplomatic agreement (Trade or Alliance) to the target country.
    /// </summary>
    public DiplomaticAgreement? ProposeAgreement(
        Country targetCountry,
        DiplomaticAgreementType type,
        RepublicTime boundary)
    {
        if (targetCountry == null)
        {
            return null;
        }

        return ProposeAgreement(targetCountry.Id, type, boundary, targetCountry);
    }

    /// <summary>
    /// Proposes a bilateral diplomatic agreement using an authoritative Republic clock.
    /// </summary>
    public DiplomaticAgreement? ProposeAgreement(
        string targetCountryId,
        DiplomaticAgreementType type,
        IRepublicClock clock,
        Country? targetCountry = null)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return ProposeAgreement(targetCountryId, type, clock.CurrentTime, targetCountry);
    }

    /// <summary>
    /// Proposes a bilateral diplomatic agreement using an authoritative Republic clock.
    /// </summary>
    public DiplomaticAgreement? ProposeAgreement(
        Country targetCountry,
        DiplomaticAgreementType type,
        IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return ProposeAgreement(targetCountry, type, clock.CurrentTime);
    }

    /// <summary>
    /// Proposes a bilateral trade agreement to the target country.
    /// </summary>
    public DiplomaticAgreement? ProposeTradeAgreement(Country targetCountry, RepublicTime boundary) =>
        ProposeAgreement(targetCountry, DiplomaticAgreementType.Trade, boundary);

    /// <summary>
    /// Proposes a bilateral trade agreement to the target country using an authoritative Republic clock.
    /// </summary>
    public DiplomaticAgreement? ProposeTradeAgreement(Country targetCountry, IRepublicClock clock) =>
        ProposeAgreement(targetCountry, DiplomaticAgreementType.Trade, clock);

    /// <summary>
    /// Proposes a bilateral trade deal transaction with explicit economic terms to a target country.
    /// Requires an existing diplomatic relation. Terms must specify valid resource, positive quantity, and positive price.
    /// Prevents duplicate pending trade proposals with the same target.
    /// Grounded strictly in Republic simulation clock boundaries; never calls DateTime.Now.
    /// </summary>
    public DiplomaticAgreement? ProposeTradeDeal(
        string targetCountryId,
        BilateralTradeTerms terms,
        RepublicTime boundary,
        Country? targetCountry = null)
    {
        ArgumentNullException.ThrowIfNull(terms);

        if (string.IsNullOrWhiteSpace(targetCountryId))
        {
            return null;
        }

        var trimmedTargetId = targetCountryId.Trim();

        // Self-trade prohibited
        if (string.Equals(trimmedTargetId, Id, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Must involve both countries
        if (!terms.Involves(Id) || !terms.Involves(trimmedTargetId))
        {
            return null;
        }

        // Must have an existing diplomatic relation
        var relation = GetRelation(trimmedTargetId);
        if (relation == null)
        {
            return null;
        }

        // Prevent duplicate pending trade proposals with the same target
        if (_agreements.Any(a => a.AgreementType == DiplomaticAgreementType.Trade && a.Involves(trimmedTargetId) && a.IsPending))
        {
            return null;
        }

        var agreement = new DiplomaticAgreement(
            Id,
            trimmedTargetId,
            DiplomaticAgreementType.Trade,
            DiplomaticAgreementStatus.Pending,
            boundary,
            terms: terms);

        _agreements.Add(agreement);

        if (targetCountry != null && !targetCountry._agreements.Any(a => a.Id == agreement.Id))
        {
            targetCountry._agreements.Add(agreement);
        }

        return agreement;
    }

    /// <summary>
    /// Proposes a bilateral trade deal transaction with explicit economic terms to the target country.
    /// </summary>
    public DiplomaticAgreement? ProposeTradeDeal(
        Country targetCountry,
        BilateralTradeTerms terms,
        RepublicTime boundary)
    {
        if (targetCountry == null)
        {
            return null;
        }

        return ProposeTradeDeal(targetCountry.Id, terms, boundary, targetCountry);
    }

    /// <summary>
    /// Proposes a bilateral trade deal transaction with explicit economic terms using an authoritative Republic clock.
    /// </summary>
    public DiplomaticAgreement? ProposeTradeDeal(
        Country targetCountry,
        BilateralTradeTerms terms,
        IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return ProposeTradeDeal(targetCountry, terms, clock.CurrentTime);
    }

    /// <summary>
    /// Proposes a bilateral alliance to the target country.
    /// </summary>
    public DiplomaticAgreement? ProposeAlliance(Country targetCountry, RepublicTime boundary) =>
        ProposeAgreement(targetCountry, DiplomaticAgreementType.Alliance, boundary);

    /// <summary>
    /// Proposes a bilateral alliance to the target country using an authoritative Republic clock.
    /// </summary>
    public DiplomaticAgreement? ProposeAlliance(Country targetCountry, IRepublicClock clock) =>
        ProposeAgreement(targetCountry, DiplomaticAgreementType.Alliance, clock);

    /// <summary>
    /// Responds to a pending bilateral agreement proposal by accepting or rejecting it.
    /// When accepting a trade deal with explicit economic terms, validates buyer funds and seller goods,
    /// and executes the atomic transfer of REPU and resources between both nations.
    /// Updates the stored agreement state on both countries if provided.
    /// Grounded strictly in Republic simulation clock boundaries; never calls DateTime.Now.
    /// </summary>
    public DiplomaticAgreement? RespondToAgreement(
        string agreementId,
        bool accept,
        RepublicTime boundary,
        Country? otherCountry = null)
    {
        if (string.IsNullOrWhiteSpace(agreementId))
        {
            return null;
        }

        var index = _agreements.FindIndex(a => string.Equals(a.Id, agreementId.Trim(), StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return null;
        }

        var agreement = _agreements[index];

        // Safe and idempotent: if already decided, return current record without duplicate processing
        if (agreement.Status != DiplomaticAgreementStatus.Pending)
        {
            return agreement;
        }

        if (!accept)
        {
            var rejected = agreement with
            {
                Status = DiplomaticAgreementStatus.Rejected,
                DecidedBoundary = boundary
            };

            _agreements[index] = rejected;

            if (otherCountry != null)
            {
                var otherIndex = otherCountry._agreements.FindIndex(a => string.Equals(a.Id, rejected.Id, StringComparison.OrdinalIgnoreCase));
                if (otherIndex >= 0)
                {
                    otherCountry._agreements[otherIndex] = rejected;
                }
                else
                {
                    otherCountry._agreements.Add(rejected);
                }
            }

            return rejected;
        }

        // Acceptance of trade deal with explicit economic terms:
        if (agreement.Terms != null)
        {
            if (otherCountry == null)
            {
                return null;
            }

            var terms = agreement.Terms;

            Country? buyer = string.Equals(Id, terms.BuyerCountryId, StringComparison.OrdinalIgnoreCase)
                ? this
                : (string.Equals(otherCountry.Id, terms.BuyerCountryId, StringComparison.OrdinalIgnoreCase) ? otherCountry : null);

            Country? seller = string.Equals(Id, terms.SellerCountryId, StringComparison.OrdinalIgnoreCase)
                ? this
                : (string.Equals(otherCountry.Id, terms.SellerCountryId, StringComparison.OrdinalIgnoreCase) ? otherCountry : null);

            if (buyer == null || seller == null)
            {
                return null;
            }

            // Must have sufficient funds and goods
            if (buyer.Treasury.Balance < terms.Price || !seller.Inventory.HasSufficient(terms.ResourceType, terms.Quantity))
            {
                return null;
            }

            // Atomic transfer execution
            if (!buyer.Treasury.TryWithdraw(terms.Price))
            {
                return null;
            }

            if (!seller.Inventory.TryRemove(terms.ResourceType, terms.Quantity))
            {
                // Rollback payment if goods removal failed
                buyer.Treasury.Deposit(terms.Price);
                return null;
            }

            seller.Treasury.Deposit(terms.Price);
            buyer.Inventory.Add(terms.ResourceType, terms.Quantity);

            var completed = agreement with
            {
                Status = DiplomaticAgreementStatus.Completed,
                DecidedBoundary = boundary
            };

            _agreements[index] = completed;

            var otherIndex = otherCountry._agreements.FindIndex(a => string.Equals(a.Id, completed.Id, StringComparison.OrdinalIgnoreCase));
            if (otherIndex >= 0)
            {
                otherCountry._agreements[otherIndex] = completed;
            }
            else
            {
                otherCountry._agreements.Add(completed);
            }

            return completed;
        }

        // Framework agreement (without explicit trade terms)
        var decided = agreement with
        {
            Status = DiplomaticAgreementStatus.Accepted,
            DecidedBoundary = boundary
        };

        _agreements[index] = decided;

        if (otherCountry != null)
        {
            var otherIndex = otherCountry._agreements.FindIndex(a => string.Equals(a.Id, decided.Id, StringComparison.OrdinalIgnoreCase));
            if (otherIndex >= 0)
            {
                otherCountry._agreements[otherIndex] = decided;
            }
            else
            {
                otherCountry._agreements.Add(decided);
            }
        }

        return decided;
    }

    /// <summary>
    /// Responds to a pending bilateral agreement proposal using an authoritative Republic clock.
    /// </summary>
    public DiplomaticAgreement? RespondToAgreement(
        string agreementId,
        bool accept,
        IRepublicClock clock,
        Country? otherCountry = null)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return RespondToAgreement(agreementId, accept, clock.CurrentTime, otherCountry);
    }

    /// <summary>
    /// Accepts a pending bilateral agreement proposal.
    /// </summary>
    public DiplomaticAgreement? AcceptAgreement(
        string agreementId,
        RepublicTime boundary,
        Country? otherCountry = null) =>
        RespondToAgreement(agreementId, accept: true, boundary, otherCountry);

    /// <summary>
    /// Accepts a pending bilateral agreement proposal using an authoritative Republic clock.
    /// </summary>
    public DiplomaticAgreement? AcceptAgreement(
        string agreementId,
        IRepublicClock clock,
        Country? otherCountry = null) =>
        RespondToAgreement(agreementId, accept: true, clock, otherCountry);

    /// <summary>
    /// Rejects a pending bilateral agreement proposal.
    /// </summary>
    public DiplomaticAgreement? RejectAgreement(
        string agreementId,
        RepublicTime boundary,
        Country? otherCountry = null) =>
        RespondToAgreement(agreementId, accept: false, boundary, otherCountry);

    /// <summary>
    /// Rejects a pending bilateral agreement proposal using an authoritative Republic clock.
    /// </summary>
    public DiplomaticAgreement? RejectAgreement(
        string agreementId,
        IRepublicClock clock,
        Country? otherCountry = null) =>
        RespondToAgreement(agreementId, accept: false, clock, otherCountry);

    /// Gets or sets the national road development project record for this country.
    /// Backward-compatibility accessor backed authoritatively by <see cref="Projects"/>.
    /// </summary>
    [JsonIgnore]
    public NationalRoadProject? RoadProject
    {
        get => Projects.OfType<NationalRoadProject>().LastOrDefault();
        set
        {
            if (value == null)
            {
                Projects.RemoveAll(p => p is NationalRoadProject);
                return;
            }

            var existingIndex = Projects.FindIndex(p => p.Id == value.Id || (p is NationalRoadProject && p.IsActive));
            if (existingIndex >= 0)
            {
                Projects[existingIndex] = value;
            }
            else
            {
                Projects.Add(value);
            }
        }
    }

    /// <summary>
    /// Deserialization fallback for legacy save payloads containing a top-level roadProject property.
    /// Never written to serialized output.
    /// </summary>
    [JsonPropertyName("roadProject")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public NationalRoadProject? LegacyRoadProject
    {
        get => null;
        set
        {
            if (value != null && !Projects.Any(p => p.Id == value.Id))
            {
                Projects.Add(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the sovereign country profile containing descriptive identity, geography, and demographics.
    /// Lazy-initialized to stable Arcadia defaults if this country is Arcadia, or default unspecified profile.
    /// </summary>
    public CountryProfile Profile
    {
        get
        {
            if (_profile == null)
            {
                if (IsArcadia(Name) || Id == "player-country")
                {
                    var arcadiaName = CountryNameRule.Default.IsValid(Name) ? Name : "Republic of Arcadia";
                    _profile = CountryProfile.CreateArcadia(
                        foundingTime: FoundingTime,
                        officialName: arcadiaName,
                        capital: string.IsNullOrWhiteSpace(CapitalCity) ? "Grand Arcadia" : CapitalCity);
                }
                else
                {
                    var officialName = CountryNameRule.Default.IsValid(Name) ? Name : CountryProfile.DefaultUnspecified;
                    _profile = new CountryProfile(
                        officialName: officialName,
                        capital: string.IsNullOrWhiteSpace(CapitalCity) ? CountryProfile.DefaultUnspecified : CapitalCity,
                        foundingTime: FoundingTime);
                }
            }

            return _profile;
        }
        set => _profile = value;
    }

    private static bool IsArcadia(string? name) =>
        !string.IsNullOrWhiteSpace(name) && name.Contains("Arcadia", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Alias for <see cref="RoadProject"/> to support generic project queries.
    /// </summary>
    [JsonIgnore]
    public NationalRoadProject? Project
    {
        get => RoadProject;
        set => RoadProject = value;
    }

    /// <summary>
    /// Gets the currently active (in-progress) road project, or null if none is active.
    /// Backed authoritatively by <see cref="Projects"/>.
    /// </summary>
    [JsonIgnore]
    public NationalRoadProject? ActiveRoadProject => Projects.OfType<NationalRoadProject>().FirstOrDefault(p => p.IsActive);

    /// <summary>
    /// Gets the currently active development project, or null if none is active.
    /// Backed authoritatively by <see cref="Projects"/>.
    /// </summary>
    [JsonIgnore]
    public DevelopmentProject? ActiveProject => Projects.FirstOrDefault(p => p.IsActive);

    /// <summary>
    /// Gets the national university development project record for this country, if any.
    /// Backed authoritatively by <see cref="Projects"/>.
    /// </summary>
    [JsonIgnore]
    public NationalUniversityProject? UniversityProject => Projects.OfType<NationalUniversityProject>().LastOrDefault();

    /// <summary>
    /// Gets the currently active (in-progress) university project, or null if none is active.
    /// Backed authoritatively by <see cref="Projects"/>.
    /// </summary>
    [JsonIgnore]
    public NationalUniversityProject? ActiveUniversityProject => Projects.OfType<NationalUniversityProject>().FirstOrDefault(p => p.IsActive);

    /// <summary>
    /// Gets the national power plant development project record for this country, if any.
    /// Backed authoritatively by <see cref="Projects"/>.
    /// </summary>
    [JsonIgnore]
    public NationalPowerPlantProject? PowerPlantProject => Projects.OfType<NationalPowerPlantProject>().LastOrDefault();

    /// <summary>
    /// Gets the currently active (in-progress) power plant project, or null if none is active.
    /// Backed authoritatively by <see cref="Projects"/>.
    /// </summary>
    [JsonIgnore]
    public NationalPowerPlantProject? ActivePowerPlantProject => Projects.OfType<NationalPowerPlantProject>().FirstOrDefault(p => p.IsActive);

    /// <summary>
    /// Gets the authoritative Republic simulation time at which this nation was founded.
    /// Init-only to prevent casual post-creation mutation.
    /// </summary>
    public RepublicTime FoundingTime
    {
        get => _foundingTime;
        init
        {
            _foundingTime = value;
            _isFoundingSealed = value.WatTimestamp != default;
        }
    }

    /// <summary>
    /// Gets or sets the foundational lifecycle status of the country.
    /// </summary>
    public CountryFoundingStatus FoundingStatus { get; set; } = CountryFoundingStatus.NewlyFounded;

    /// <summary>
    /// Gets a value indicating whether the founding moment has been authoritatively established.
    /// </summary>
    [JsonIgnore]
    public bool IsFounded => _isFoundingSealed || _foundingTime.WatTimestamp != default;

    /// <summary>
    /// Gets the Gregorian calendar date of national independence in West Africa Time (WAT).
    /// </summary>
    [JsonIgnore]
    public DateOnly IndependenceDay => FoundingTime.Date;

    /// <summary>
    /// Gets the time of day of national independence in West Africa Time (WAT).
    /// </summary>
    [JsonIgnore]
    public TimeOnly IndependenceTime => FoundingTime.Time;

    /// <summary>
    /// Gets the Republic simulation day number on which national independence occurred (e.g. Day 0, Day 42).
    /// </summary>
    [JsonIgnore]
    public long FoundingRepublicDay => FoundingTime.DayNumber;

    /// <summary>
    /// Gets a value indicating whether the country has transitioned from newly founded to established statehood.
    /// </summary>
    [JsonIgnore]
    public bool IsEstablished => FoundingStatus == CountryFoundingStatus.Established;

    /// <summary>
    /// Establishes the authoritative founding moment for an unsealed country.
    /// Throws <see cref="InvalidOperationException"/> if already established.
    /// </summary>
    public void EstablishFounding(RepublicTime foundingTime)
    {
        if (IsFounded)
        {
            throw new InvalidOperationException(
                $"The founding moment for country '{Name}' ({Id}) has already been authoritatively established and cannot be overwritten.");
        }

        if (foundingTime.WatTimestamp == default)
        {
            throw new ArgumentException("Cannot establish founding with an uninitialized Republic time.", nameof(foundingTime));
        }

        _foundingTime = foundingTime;
        _isFoundingSealed = true;
    }

    /// <summary>
    /// Marks the country as having achieved established statehood.
    /// </summary>
    public void MarkEstablished()
    {
        FoundingStatus = CountryFoundingStatus.Established;
    }

    /// <summary>
    /// Calculates the country's national age relative to the specified Republic simulation time.
    /// Returns <see cref="TimeSpan.Zero"/> if the simulation time is at or before founding, or if not founded yet.
    /// </summary>
    public TimeSpan GetAge(RepublicTime currentRepublicTime)
    {
        if (!IsFounded)
        {
            return TimeSpan.Zero;
        }

        var age = currentRepublicTime - FoundingTime;
        return age < TimeSpan.Zero ? TimeSpan.Zero : age;
    }

    /// <summary>
    /// Calculates the country's national age relative to the authoritative Republic clock.
    /// </summary>
    public TimeSpan GetAge(IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return GetAge(clock.CurrentTime);
    }

    /// <summary>
    /// Determines whether the specified simulation time falls within the country's founding day (Independence Day in WAT).
    /// Respects the exact WAT day boundary (simulation day number), not simply 24 elapsed hours.
    /// </summary>
    public bool IsInFoundingDay(RepublicTime simulationTime)
    {
        if (!IsFounded)
        {
            return false;
        }

        return simulationTime >= FoundingTime && simulationTime.DayNumber == FoundingRepublicDay;
    }

    /// <summary>
    /// Determines whether the country is currently within its founding day relative to the authoritative Republic clock.
    /// </summary>
    public bool IsInFoundingDay(IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return IsInFoundingDay(clock.CurrentTime);
    }

    /// <summary>
    /// Evaluates and returns the sovereign country's temporary tapering founding buffs at the specified simulation time,
    /// storing the snapshot on the country.
    /// </summary>
    public FoundingBuffSnapshot EvaluateFoundingBuffs(RepublicTime currentSimulationTime, IFoundingBuffEvaluator? evaluator = null)
    {
        evaluator ??= new FoundingBuffEvaluator();
        var snapshot = evaluator.Evaluate(this, currentSimulationTime);
        FoundingBuffs = snapshot;
        return snapshot;
    }

    /// <summary>
    /// Evaluates and returns the sovereign country's temporary tapering founding buffs at the current clock time,
    /// storing the snapshot on the country.
    /// </summary>
    public FoundingBuffSnapshot EvaluateFoundingBuffs(IRepublicClock clock, IFoundingBuffEvaluator? evaluator = null)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return EvaluateFoundingBuffs(clock.CurrentTime, evaluator);
    }

    /// <summary>
    /// Evaluates and returns the sovereign country's State Capacity snapshot from its in-memory state.
    /// Does not call real-time APIs (never calls DateTime.Now).
    /// </summary>
    public StateCapacitySnapshot EvaluateStateCapacity(IStateCapacityEvaluator? evaluator = null)
    {
        evaluator ??= new StateCapacityEvaluator();
        return evaluator.Evaluate(this);
    }

    /// <summary>
    /// Gets the authoritative sovereign State Capacity score (0.0 to 1.0) evaluated from current country inputs.
    /// Represents the conversion rate applied at REPU revenue credit time.
    /// </summary>
    [JsonIgnore]
    public double StateCapacityScore => EvaluateStateCapacity().Score;

    /// <summary>
    /// Gets the currently appointed minister for the specified portfolio, or null if the portfolio is vacant.
    /// </summary>
    public Minister? GetMinister(CabinetPortfolio portfolio)
    {
        return Cabinet.TryGetValue(portfolio, out var minister) ? minister : null;
    }

    /// <summary>
    /// Appoints a minister to the specified portfolio (or vacates if null), stores the appointment,
    /// and recalculates the three affected State Capacity inputs (Civil Service Quality, Corruption Control, Infrastructure Condition).
    /// Spends nothing in this slice.
    /// </summary>
    public Minister? AppointMinister(Minister? minister, CabinetPortfolio portfolio)
    {
        if (minister != null)
        {
            minister.Portfolio = portfolio;
            minister.IsAppointed = true;
            Cabinet[portfolio] = minister;
        }
        else
        {
            if (Cabinet.TryGetValue(portfolio, out var existing) && existing != null)
            {
                existing.IsAppointed = false;
            }
            Cabinet[portfolio] = null;
        }

        RecalculateCabinetStateCapacity();
        return minister;
    }

    /// <summary>
    /// Appoints a minister to the portfolio configured on the minister object, stores the appointment,
    /// and recalculates the three affected State Capacity inputs.
    /// </summary>
    public Minister AppointMinister(Minister minister)
    {
        ArgumentNullException.ThrowIfNull(minister);
        AppointMinister(minister, minister.Portfolio);
        return minister;
    }

    /// <summary>
    /// Dismisses the minister from the specified portfolio, marking it vacant,
    /// and recalculates the three affected State Capacity inputs.
    /// </summary>
    public bool DismissMinister(CabinetPortfolio portfolio)
    {
        AppointMinister(null, portfolio);
        return true;
    }

    /// <summary>
    /// Vacates the specified ministerial portfolio and recalculates the three affected State Capacity inputs.
    /// </summary>
    public bool VacatePortfolio(CabinetPortfolio portfolio)
    {
        return DismissMinister(portfolio);
    }

    /// <summary>
    /// Recalculates the three State Capacity inputs affected by cabinet appointments:
    /// - Finance competence and integrity set civil service quality to their minimum (or 0.3 if vacant).
    /// - Interior competence and integrity set corruption control to their minimum (or 0.3 if vacant).
    /// - Infrastructure competence and experience set infrastructure condition to their minimum (or 0.3 if vacant).
    /// Loyalty and political connections do not raise capacity.
    /// </summary>
    public void RecalculateCabinetStateCapacity()
    {
        // 1. Finance -> Civil Service Quality
        var finance = GetMinister(CabinetPortfolio.Finance);
        StateCapacity.CivilServiceQuality = finance != null
            ? Math.Min(finance.Competence, finance.Integrity)
            : VacantPortfolioFactor;

        // 2. Interior -> Corruption Control
        var interior = GetMinister(CabinetPortfolio.Interior);
        StateCapacity.CorruptionControl = interior != null
            ? Math.Min(interior.Competence, interior.Integrity)
            : VacantPortfolioFactor;

        // 3. Infrastructure -> Infrastructure Condition
        var infra = GetMinister(CabinetPortfolio.Infrastructure);
        StateCapacity.InfrastructureCondition = infra != null
            ? Math.Min(infra.Competence, infra.Experience)
            : VacantPortfolioFactor;
    }

    /// <summary>
    /// Recalculates and updates the sovereign State Capacity snapshot from current institutional inputs.
    /// </summary>
    public StateCapacitySnapshot RecalculateStateCapacity()
    {
        return EvaluateStateCapacity();
    }

    /// <summary>
    /// Initiates a National Road Program development project starting from the specified boundary/simulation time.
    /// Costs R500,000, withdrawn once from this country's RepuTreasury.
    /// Takes 4 six-hour boundaries (24 hours on the Republic clock) from the start boundary.
    /// Returns null if the treasury cannot pay or if another road project is currently active.
    /// The treasury balance remains unchanged on a failed start.
    /// </summary>
    public NationalRoadProject? StartRoadProject(RepublicTime startedBoundary, double cost = NationalRoadProject.StandardCost)
    {
        // 1. Must have a valid sovereign country ID
        if (string.IsNullOrWhiteSpace(Id))
        {
            return null;
        }

        // 2. Cannot start a second road while one is active
        if (Projects.OfType<NationalRoadProject>().Any(p => p.IsActive))
        {
            return null;
        }

        // 3. Cannot start if treasury cannot pay; balance must not change on failed start
        if (Treasury.Balance < cost)
        {
            return null;
        }

        // 4. Withdraw exactly R500,000 once from that country's RepuTreasury
        if (!Treasury.TryWithdraw(cost))
        {
            return null;
        }

        // 5. Takes 4 six-hour boundaries (24 hours on the Republic clock) from the start boundary
        var finishBoundary = startedBoundary.Add(NationalRoadProject.Duration);
        var project = new NationalRoadProject
        {
            Id = Guid.NewGuid().ToString("N"),
            CountryId = Id,
            StartedBoundary = startedBoundary,
            FinishBoundary = finishBoundary,
            Cost = cost,
            Completed = false
        };

        Projects.Add(project);
        return project;
    }

    /// <summary>
    /// Overload for starting a road project using an authoritative Republic clock.
    /// </summary>
    public NationalRoadProject? StartRoadProject(IRepublicClock clock, double cost = NationalRoadProject.StandardCost)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return StartRoadProject(clock.CurrentTime, cost);
    }

    /// <summary>
    /// Initiates a National Road Program, returning true on success and false on failure.
    /// </summary>
    public bool StartRoadProgram(RepublicTime startedBoundary, double cost = NationalRoadProject.StandardCost)
    {
        return StartRoadProject(startedBoundary, cost) != null;
    }

    /// <summary>
    /// Overload for starting a road program using an authoritative clock, returning true on success.
    /// </summary>
    public bool StartRoadProgram(IRepublicClock clock, double cost = NationalRoadProject.StandardCost)
    {
        return StartRoadProject(clock, cost) != null;
    }

    /// <summary>
    /// Initiates a National University development project starting from the specified boundary/simulation time.
    /// Costs R750,000, withdrawn once from this country's RepuTreasury.
    /// Takes 8 six-hour boundaries (48 hours on the Republic clock) from the start boundary.
    /// Returns null if the treasury cannot pay, country ID is invalid, or if another university project is currently active.
    /// </summary>
    public NationalUniversityProject? StartUniversityProject(RepublicTime startedBoundary, double cost = NationalUniversityProject.StandardCost)
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            return null;
        }

        var project = new NationalUniversityProject(Id, startedBoundary, cost);
        return StartProject(project) ? project : null;
    }

    /// <summary>
    /// Overload for starting a national university project using an authoritative Republic clock.
    /// </summary>
    public NationalUniversityProject? StartUniversityProject(IRepublicClock clock, double cost = NationalUniversityProject.StandardCost)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return StartUniversityProject(clock.CurrentTime, cost);
    }

    /// <summary>
    /// Initiates a National Power Plant development project starting from the specified boundary/simulation time.
    /// Costs R1,000,000, withdrawn once from this country's RepuTreasury.
    /// Takes 12 six-hour boundaries (72 hours on the Republic clock) from the start boundary.
    /// Returns null if the treasury cannot pay, country ID is invalid, or if another power plant project is currently active.
    /// </summary>
    public NationalPowerPlantProject? StartPowerPlantProject(RepublicTime startedBoundary, double cost = NationalPowerPlantProject.StandardCost)
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            return null;
        }

        var project = new NationalPowerPlantProject(Id, startedBoundary, cost);
        return StartProject(project) ? project : null;
    }

    /// <summary>
    /// Overload for starting a national power plant project using an authoritative Republic clock.
    /// </summary>
    public NationalPowerPlantProject? StartPowerPlantProject(IRepublicClock clock, double cost = NationalPowerPlantProject.StandardCost)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return StartPowerPlantProject(clock.CurrentTime, cost);
    }

    /// <summary>
    /// Initiates a generic development project for this sovereign country.
    /// Withdraws project cost immediately from the country's RepuTreasury.
    /// Enforces strict country ownership and ensures no duplicate active project of the same type.
    /// </summary>
    public bool StartProject(DevelopmentProject project)
    {
        ArgumentNullException.ThrowIfNull(project);

        // Project must have a valid non-empty CountryId belonging strictly to this country
        if (string.IsNullOrWhiteSpace(project.CountryId) ||
            string.IsNullOrWhiteSpace(Id) ||
            !string.Equals(project.CountryId, Id, StringComparison.Ordinal))
        {
            return false;
        }

        // Cannot start if an active project of the same type is already in progress
        if (Projects.Any(p => p.ProjectType == project.ProjectType && p.IsActive))
        {
            return false;
        }

        if (Treasury.Balance < project.Cost)
        {
            return false;
        }

        if (!Treasury.TryWithdraw(project.Cost))
        {
            return false;
        }

        if (!Projects.Contains(project))
        {
            Projects.Add(project);
        }

        return true;
    }

    /// <summary>
    /// Completes the active National Road Program if the current simulation time has reached or passed its finish boundary.
    /// Raises this country's infrastructure condition by 0.1 (clamped at 1.0) and recalculates State Capacity.
    /// If no simulation time is provided, force-completes the active road project.
    /// Returns false if no road project is active or if current time is before the finish boundary.
    /// </summary>
    public bool CompleteRoadProject(RepublicTime? currentTime = null)
    {
        var road = ActiveRoadProject ?? RoadProject;
        if (road == null)
        {
            return false;
        }

        return road.Complete(this, currentTime);
    }

    /// <summary>
    /// Alias for <see cref="CompleteRoadProject"/>.
    /// </summary>
    public bool CompleteRoadProgram(RepublicTime? currentTime = null) => CompleteRoadProject(currentTime);

    /// <summary>
    /// Gets the construction progress (0.0 to 1.0) of the national road program, or null if no road program exists.
    /// </summary>
    public double? GetRoadProgress(RepublicTime currentTime) => RoadProject?.GetProgress(currentTime);

    /// <summary>
    /// Overload for getting national road program construction progress using an authoritative clock.
    /// </summary>
    public double? GetRoadProgress(IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return GetRoadProgress(clock.CurrentTime);
    }

    /// <summary>
    /// Completes the active National University project if the current simulation time has reached or passed its finish boundary.
    /// Elevates Human Capital and Science / Research Capacity upon completion.
    /// </summary>
    public bool CompleteUniversityProject(RepublicTime? currentTime = null)
    {
        var university = ActiveUniversityProject ?? UniversityProject;
        return university != null && CompleteProject(university, currentTime);
    }

    /// <summary>
    /// Gets the construction progress (0.0 to 1.0) of the national university project, or null if no university project exists.
    /// </summary>
    public double? GetUniversityProgress(RepublicTime currentTime) => UniversityProject?.GetProgress(currentTime);

    /// <summary>
    /// Overload for getting national university construction progress using an authoritative clock.
    /// </summary>
    public double? GetUniversityProgress(IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return GetUniversityProgress(clock.CurrentTime);
    }

    /// <summary>
    /// Completes the active National Power Plant project if the current simulation time has reached or passed its finish boundary.
    /// Elevates Energy Capacity upon completion.
    /// </summary>
    public bool CompletePowerPlantProject(RepublicTime? currentTime = null)
    {
        var powerPlant = ActivePowerPlantProject ?? PowerPlantProject;
        return powerPlant != null && CompleteProject(powerPlant, currentTime);
    }

    /// <summary>
    /// Gets the construction progress (0.0 to 1.0) of the national power plant project, or null if no power plant project exists.
    /// </summary>
    public double? GetPowerPlantProgress(RepublicTime currentTime) => PowerPlantProject?.GetProgress(currentTime);

    /// <summary>
    /// Overload for getting national power plant construction progress using an authoritative clock.
    /// </summary>
    public double? GetPowerPlantProgress(IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return GetPowerPlantProgress(clock.CurrentTime);
    }

    /// <summary>
    /// Gets the construction progress (0.0 to 1.0) of the currently active development project, or null if none is active.
    /// </summary>
    public double? GetActiveProjectProgress(RepublicTime currentTime) => ActiveProject?.GetProgress(currentTime);

    /// <summary>
    /// Overload for getting active project construction progress using an authoritative clock.
    /// </summary>
    public double? GetActiveProjectProgress(IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return GetActiveProjectProgress(clock.CurrentTime);
    }

    /// <summary>
    /// Checks and completes the active road project if the simulation time has reached or passed the finish boundary.
    /// </summary>
    public bool CheckRoadProjectCompletion(RepublicTime currentTime) => CompleteRoadProject(currentTime);

    /// <summary>
    /// Completes the specified development project for this country.
    /// Idempotent and enforces strict country ownership.
    /// </summary>
    public bool CompleteProject(DevelopmentProject project, RepublicTime? currentTime = null)
    {
        ArgumentNullException.ThrowIfNull(project);

        // Project must have a valid non-empty CountryId belonging strictly to this country
        if (string.IsNullOrWhiteSpace(project.CountryId) ||
            string.IsNullOrWhiteSpace(Id) ||
            !string.Equals(project.CountryId, Id, StringComparison.Ordinal))
        {
            return false;
        }

        return project.Complete(this, currentTime);
    }

    /// <summary>
    /// Checks and completes any active development projects whose finish boundary has been reached.
    /// </summary>
    public int CheckProjectsCompletion(RepublicTime currentTime)
    {
        var completed = 0;
        foreach (var project in Projects.Where(p => p.IsActive).ToList())
        {
            if (project.CanComplete(currentTime) && CompleteProject(project, currentTime))
            {
                completed++;
            }
        }

        return completed;
    }

    /// <summary>
    /// Applies a new national taxation policy, evaluating its direct citizen welfare and public approval consequences
    /// according to established Republic domain rules.
    /// </summary>
    /// <param name="newPolicy">The new taxation policy to enact.</param>
    /// <param name="enactedTime">The authoritative Republic simulation timestamp at which the decision is enacted.</param>
    /// <returns>A domain feedback record detailing the citizen reaction, approval delta, and stability impact.</returns>
    public CitizenFeedbackRecord ApplyTaxPolicy(TaxPolicy newPolicy, RepublicTime enactedTime)
    {
        ArgumentNullException.ThrowIfNull(newPolicy);

        var prevIncome = TaxPolicy.IncomeTaxRate;
        var prevCorp = TaxPolicy.CorporateTaxRate;
        var prevTariff = TaxPolicy.ImportTariffRate;

        // Clamp rates within constitutional boundaries (0.0 to 0.70 for income, 0.0 to 0.60 for corporate, 0.0 to 0.50 for tariff)
        var clampedIncome = double.IsFinite(newPolicy.IncomeTaxRate) ? Math.Clamp(newPolicy.IncomeTaxRate, 0.0, 0.70) : prevIncome;
        var clampedCorp = double.IsFinite(newPolicy.CorporateTaxRate) ? Math.Clamp(newPolicy.CorporateTaxRate, 0.0, 0.60) : prevCorp;
        var clampedTariff = double.IsFinite(newPolicy.ImportTariffRate) ? Math.Clamp(newPolicy.ImportTariffRate, 0.0, 0.50) : prevTariff;

        TaxPolicy.IncomeTaxRate = clampedIncome;
        TaxPolicy.CorporateTaxRate = clampedCorp;
        TaxPolicy.ImportTariffRate = clampedTariff;

        var incomeDelta = clampedIncome - prevIncome;
        var prevApproval = HappinessRating;
        var prevStability = BaselineStability;

        // Feedback rule:
        // Income tax directly affects citizen household disposable income and welfare:
        // Each +1% (+0.01) increase in income tax rate produces a -0.5 point drop in public approval.
        // Each -1% (-0.01) decrease in income tax rate produces a +0.5 point increase in public approval.
        // Clamped between 0.0% and 100.0%.
        var rawApprovalDelta = (incomeDelta == 0.0) ? 0.0 : -incomeDelta * 50.0;
        var targetApproval = Math.Clamp(prevApproval + rawApprovalDelta, 0.0, 100.0);
        var actualApprovalDelta = Math.Round(targetApproval - prevApproval, 2);

        HappinessRating = targetApproval;

        // Extreme taxation threshold (> 50% income tax rate):
        // If income tax rate exceeds 50% (0.50), high societal stress inflicts a -2.0% stability penalty.
        // If reduced back to <= 50% from > 50%, the penalty is restored (+2.0% stability).
        var stabilityDelta = 0.0;
        if (prevIncome <= 0.50 && clampedIncome > 0.50)
        {
            stabilityDelta = -2.0;
        }
        else if (prevIncome > 0.50 && clampedIncome <= 0.50)
        {
            stabilityDelta = 2.0;
        }

        if (stabilityDelta != 0.0)
        {
            BaselineStability = Math.Clamp(BaselineStability + stabilityDelta, 0.0, 100.0);
        }

        var description = BuildCitizenFeedbackDescription(clampedIncome, prevIncome, actualApprovalDelta, stabilityDelta);

        var feedback = new CitizenFeedbackRecord
        {
            PreviousIncomeTaxRate = prevIncome,
            NewIncomeTaxRate = clampedIncome,
            PreviousCorporateTaxRate = prevCorp,
            NewCorporateTaxRate = clampedCorp,
            ApprovalDelta = actualApprovalDelta,
            PreviousApproval = prevApproval,
            CurrentApproval = HappinessRating,
            StabilityDelta = stabilityDelta,
            PreviousStability = prevStability,
            CurrentStability = BaselineStability,
            EnactedTime = enactedTime,
            Description = description
        };

        LatestCitizenFeedback = feedback;
        return feedback;
    }

    private static string BuildCitizenFeedbackDescription(double newIncome, double prevIncome, double approvalDelta, double stabilityDelta)
    {
        if (newIncome > prevIncome)
        {
            var text = $"Tax hike enacted ({prevIncome * 100:0.#}% -> {newIncome * 100:0.#}%). Household disposable income reduced; public approval dropped by {Math.Abs(approvalDelta):0.#}%.";
            if (stabilityDelta < 0)
            {
                text += $" Societal friction from extreme tax (>50%) reduced national stability by {Math.Abs(stabilityDelta):0.#}%.";
            }
            return text;
        }
        else if (newIncome < prevIncome)
        {
            var text = $"Tax relief enacted ({prevIncome * 100:0.#}% -> {newIncome * 100:0.#}%). Household disposable income expanded; public approval rose by {approvalDelta:0.#}%.";
            if (stabilityDelta > 0)
            {
                text += $" Easing extreme taxation restored national stability by {stabilityDelta:0.#}%.";
            }
            return text;
        }

        return $"Tax policy maintained at {newIncome * 100:0.#}%. Citizen sentiment remains steady.";
    }

    /// <summary>
    /// Applies an evaluated fiscal quarter report to the sovereign treasury, safely depositing surplus or deducting deficit.
    /// Enforces duplicate prevention: if the quarter has already been applied, returns existing state without modifying treasury.
    /// </summary>
    /// <param name="report">The evaluated fiscal quarter report.</param>
    /// <returns>The confirmed fiscal quarter report reflecting the updated sovereign treasury balance.</returns>
    public FiscalQuarterReport ApplyFiscalQuarter(FiscalQuarterReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (!double.IsFinite(report.TotalTaxRevenue) || !double.IsFinite(report.TotalExpenditures))
        {
            throw new ArgumentException("Fiscal quarter financial values must be finite numbers.", nameof(report));
        }

        var transactionKey = $"fiscal-quarter-{Id}-Q{report.QuarterIndex}";

        // Deduplication guard: if transaction was already applied, do not duplicate financial transaction
        if (Treasury.HasAppliedSnapshot(transactionKey) || LastProcessedFiscalQuarter >= report.QuarterIndex)
        {
            return LatestFiscalReport ?? report;
        }

        var net = report.NetSurplusOrDeficit;
        if (!double.IsFinite(net))
        {
            throw new ArgumentException("Net surplus or deficit must be a finite number.", nameof(report));
        }

        if (net > 0.0)
        {
            Treasury.Deposit(net);
        }
        else if (net < 0.0)
        {
            var deficit = Math.Abs(net);
            if (deficit <= Treasury.Balance)
            {
                Treasury.Withdraw(deficit);
            }
            else
            {
                // Safely consume all available liquid funds without causing negative overdraft
                var available = Treasury.Balance;
                if (available > 0.0)
                {
                    Treasury.Withdraw(available);
                }
            }
        }

        Treasury.RecordSnapshotApplied(transactionKey);
        LastProcessedFiscalQuarter = report.QuarterIndex;

        var finalReport = new FiscalQuarterReport
        {
            CountryId = Id,
            QuarterIndex = report.QuarterIndex,
            IncomeTaxRevenue = report.IncomeTaxRevenue,
            CorporateTaxRevenue = report.CorporateTaxRevenue,
            TariffRevenue = report.TariffRevenue,
            TotalTaxRevenue = report.TotalTaxRevenue,
            TotalExpenditures = report.TotalExpenditures,
            UpdatedTreasuryBalance = Treasury.Balance,
            Timestamp = report.Timestamp
        };

        LatestFiscalReport = finalReport;
        return finalReport;
    }

    /// <summary>
    /// Evaluates and returns the read-only national dashboard for this sovereign country at the specified simulation time.
    /// Induces zero mutation or side-effects on country or simulation state.
    /// </summary>
    public NationalDashboard GetDashboard(RepublicTime simulationTime) => new(this, simulationTime);

    /// <summary>
    /// Evaluates and returns the read-only national dashboard for this sovereign country using an authoritative clock.
    /// </summary>
    public NationalDashboard GetDashboard(IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return GetDashboard(clock.CurrentTime);
    }

    /// <summary>
    /// Evaluates and returns the read-only comparable national strength metrics for this sovereign country.
    /// Induces zero mutation or side-effects on country or simulation state.
    /// </summary>
    public NationalStrength GetNationalStrength() => new(this);

    /// <summary>
    /// Evaluates and returns the read-only comparable national strength metrics for this sovereign country with an explicit founding buff snapshot.
    /// </summary>
    public NationalStrength GetNationalStrength(FoundingBuffSnapshot? foundingBuffs) => new(this, foundingBuffs);

    /// <summary>
    /// Factory method to create and found a new sovereign country with an authoritative founding moment.
    /// </summary>
    public static Country Found(
        string name,
        RepublicTime foundingTime,
        string id = "",
        string capitalCity = "",
        string governmentType = "Democratic Republic",
        double territorySizeSqKm = 500000,
        double baselineStability = 75.0,
        NationalYield? yield = null,
        RepuTreasury? treasury = null,
        RepublicTime? lastCreditedBoundary = null,
        FoundingBuffSnapshot? foundingBuffs = null,
        StateCapacityInputs? stateCapacity = null,
        Dictionary<CabinetPortfolio, Minister?>? cabinet = null,
        NationalRoadProject? roadProject = null,
        IEnumerable<DevelopmentProject>? projects = null,
        CountryProfile? profile = null,
        double grossDomesticProduct = 450_000_000_000.0,
        double happinessRating = 68.5,
        NationalYieldSnapshot? latestYieldSnapshot = null,
        TreasuryConsequence? latestTreasuryConsequence = null,
        PoliticalParty? party = null,
        PoliticalParty? oppositionParty = null,
        InterestGroup? interestGroup = null,
        CountryBill? bill = null,
        CountryElection? election = null,
        DiplomaticRelation? relation = null,
        DiplomaticAgreement? agreement = null,
        IEnumerable<DiplomaticAgreement>? agreements = null,
        ResourceInventory? inventory = null,
        IEnumerable<ResourceNode>? resourceNodes = null,
        TaxPolicy? taxPolicy = null,
        CitizenFeedbackRecord? latestCitizenFeedback = null,
        MinistryBudget? ministryBudget = null,
        double tradeBalance = 2_100_000_000.0,
        FiscalQuarterReport? latestFiscalReport = null,
        int lastProcessedFiscalQuarter = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var countryId = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id;
        var country = new Country
        {
            Id = countryId,
            Name = name,
            FoundingTime = foundingTime,
            CapitalCity = capitalCity,
            GovernmentType = governmentType,
            TerritorySizeSqKm = territorySizeSqKm,
            BaselineStability = baselineStability,
            FoundingStatus = CountryFoundingStatus.NewlyFounded,
            Yield = yield?.Clone() ?? new NationalYield(),
            Treasury = treasury?.Clone() ?? new RepuTreasury(0.0, countryId),
            Inventory = inventory?.Clone() ?? new ResourceInventory(),
            LastCreditedBoundary = lastCreditedBoundary,
            FoundingBuffs = foundingBuffs,
            StateCapacity = stateCapacity?.Clone() ?? new StateCapacityInputs(),
            Cabinet = cabinet != null ? new Dictionary<CabinetPortfolio, Minister?>(cabinet) : new Dictionary<CabinetPortfolio, Minister?>(),
            GrossDomesticProduct = grossDomesticProduct,
            HappinessRating = happinessRating,
            LatestYieldSnapshot = latestYieldSnapshot,
            LatestTreasuryConsequence = latestTreasuryConsequence,
            TaxPolicy = taxPolicy != null
                ? new TaxPolicy { IncomeTaxRate = taxPolicy.IncomeTaxRate, CorporateTaxRate = taxPolicy.CorporateTaxRate, ImportTariffRate = taxPolicy.ImportTariffRate }
                : new TaxPolicy(),
            MinistryBudget = ministryBudget != null
                ? new MinistryBudget
                {
                    DefenseAllocation = ministryBudget.DefenseAllocation,
                    HealthcareAllocation = ministryBudget.HealthcareAllocation,
                    EducationAllocation = ministryBudget.EducationAllocation,
                    InfrastructureAllocation = ministryBudget.InfrastructureAllocation,
                    ScienceAllocation = ministryBudget.ScienceAllocation
                }
                : new MinistryBudget(),
            TradeBalance = tradeBalance,
            LatestCitizenFeedback = latestCitizenFeedback,
            LatestFiscalReport = latestFiscalReport,
            LastProcessedFiscalQuarter = lastProcessedFiscalQuarter,
            Party = party,
            OppositionParty = oppositionParty,
            InterestGroup = interestGroup,
            Bill = bill
        };

        if (election != null)
        {
            country._elections.Add(election);
        }

        if (relation != null)
        {
            country._relations.Add(relation);
        }

        if (agreement != null)
        {
            country._agreements.Add(agreement);
        }

        if (agreements != null)
        {
            foreach (var a in agreements)
            {
                if (!country._agreements.Any(existing => existing.Id == a.Id))
                {
                    country._agreements.Add(a);
                }
            }
        }

        if (profile != null)
        {
            country.Profile = profile;
        }

        if (resourceNodes != null)
        {
            foreach (var node in resourceNodes)
            {
                country.AssociateResourceNode(node);
            }
        }

        if (roadProject != null)
        {
            if (string.IsNullOrWhiteSpace(roadProject.CountryId))
            {
                roadProject.CountryId = countryId;
            }

            if (!country.Projects.Any(p => p.Id == roadProject.Id))
            {
                country.Projects.Add(roadProject);
            }
        }

        if (projects != null)
        {
            foreach (var p in projects)
            {
                if (!country.Projects.Any(existing => existing.Id == p.Id))
                {
                    country.Projects.Add(p);
                }
            }
        }

        if (cabinet != null)
        {
            country.RecalculateCabinetStateCapacity();
        }

        return country;
    }
}
