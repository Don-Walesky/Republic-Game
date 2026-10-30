global using BillStatus = Republic.Core.Politics.CountryBillStatus;

namespace Republic.Core.Politics;

using System;
using System.Text.Json.Serialization;
using Republic.Core.Time;

/// <summary>
/// Specifies the legislative status of a country bill.
/// </summary>
public enum CountryBillStatus
{
    Pending,
    Passed,
    Failed,
    Vetoed
}

/// <summary>
/// Domain model representing a legislative bill proposed in a sovereign country.
/// Holds the bill title, owning country identifier, status, consequences, and the authoritative clock boundary when voted.
/// Implements the 50:50 Consequence Balance System where every meaningful bill contains both positive and negative tradeoffs.
/// </summary>
public sealed record CountryBill
{
    private readonly string _title = string.Empty;

    /// <summary>
    /// Gets the official title of the legislative bill (trimmed, not empty).
    /// </summary>
    public string Title
    {
        get => _title;
        init => _title = value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Gets the identifier of the sovereign country where the bill was proposed.
    /// </summary>
    public string CountryId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the current legislative status of the bill (Pending, Passed, Failed, or Vetoed).
    /// </summary>
    public CountryBillStatus Status { get; init; } = CountryBillStatus.Pending;

    /// <summary>
    /// Gets the Republic simulation clock boundary at which the bill was voted on, or null if pending.
    /// </summary>
    public RepublicTime? VotedBoundary { get; init; }

    /// <summary>
    /// Gets the description or policy intent of this bill.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the policy category (Economy, Infrastructure, Health, Defense, Governance, Energy).
    /// </summary>
    public string Category { get; init; } = "Economy";

    /// <summary>
    /// Foreseeable positive consequence of passing this legislation (50:50 rule).
    /// </summary>
    public string PositiveConsequence { get; init; } = string.Empty;

    /// <summary>
    /// Foreseeable negative consequence / trade-off of passing this legislation (50:50 rule).
    /// </summary>
    public string NegativeConsequence { get; init; } = string.Empty;

    /// <summary>
    /// Underlying governance principle taught by this legislative decision.
    /// </summary>
    public string GovernancePrinciple { get; init; } = string.Empty;

    /// <summary>
    /// Required percentage of votes to pass (default 50.0% simple majority).
    /// </summary>
    public double RequiredMajorityPercentage { get; init; } = 50.0;

    /// <summary>
    /// Percentage required for Parliament to override an executive veto (default 66.7% supermajority).
    /// </summary>
    public double VetoThresholdPercentage { get; init; } = 66.7;

    /// <summary>
    /// Total parliamentary seats voting.
    /// </summary>
    public int TotalVotes { get; init; } = 100;

    /// <summary>
    /// Count of Ayes cast on this bill.
    /// </summary>
    public int AyesCount { get; init; }

    /// <summary>
    /// Count of Nays cast on this bill.
    /// </summary>
    public int NaysCount { get; init; }

    /// <summary>
    /// Count of abstentions.
    /// </summary>
    public int AbstentionsCount { get; init; }

    /// <summary>
    /// Indicates whether the President has exercised an executive veto on this passed bill.
    /// </summary>
    public bool IsVetoed { get; init; }

    /// <summary>
    /// Indicates whether the executive veto was overridden by parliamentary supermajority.
    /// </summary>
    public bool VetoOverridden { get; init; }

    /// <summary>
    /// Gets a value indicating whether the bill is currently pending a legislative vote.
    /// </summary>
    [JsonIgnore]
    public bool IsPending => Status == CountryBillStatus.Pending;

    /// <summary>
    /// Gets a value indicating whether the bill has successfully passed.
    /// </summary>
    [JsonIgnore]
    public bool IsPassed => Status == CountryBillStatus.Passed;

    /// <summary>
    /// Gets a value indicating whether the bill has failed.
    /// </summary>
    [JsonIgnore]
    public bool IsFailed => Status == CountryBillStatus.Failed || Status == CountryBillStatus.Vetoed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CountryBill"/> record with default values.
    /// </summary>
    public CountryBill() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CountryBill"/> record with validated parameters.
    /// </summary>
    /// <param name="title">The official bill title (trimmed, not empty).</param>
    /// <param name="countryId">The owning country identifier.</param>
    /// <param name="status">The initial or updated status of the bill.</param>
    /// <param name="votedBoundary">The clock boundary when the bill was voted on, or null.</param>
    public CountryBill(
        string title,
        string countryId,
        CountryBillStatus status = CountryBillStatus.Pending,
        RepublicTime? votedBoundary = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        Title = title.Trim();
        CountryId = countryId ?? string.Empty;
        Status = status;
        VotedBoundary = votedBoundary;

        var (desc, cat, pos, neg, principle) = Derive5050Consequences(Title);
        Description = desc;
        Category = cat;
        PositiveConsequence = pos;
        NegativeConsequence = neg;
        GovernancePrinciple = principle;
    }

    /// <summary>
    /// Derives educational 50:50 consequences and governance principles for known and custom bill titles.
    /// </summary>
    public static (string Description, string Category, string PositiveConsequence, string NegativeConsequence, string GovernancePrinciple) Derive5050Consequences(string title)
    {
        var lower = title?.ToLowerInvariant() ?? string.Empty;

        if (lower.Contains("infra") || lower.Contains("road") || lower.Contains("transit") || lower.Contains("transport"))
        {
            return (
                "Authorizes nationwide logistics and transportation corridor modernization.",
                "Infrastructure",
                "+12% Infrastructure Condition, +8% Industrial Logistics Capacity",
                "-R 350,000 Sovereign Expenditure, -2% Public Approval during construction disruption",
                "Public capital investment creates high logistical multipliers but strains liquid treasury reserves (Keynesian Infrastructure Multiplier)."
            );
        }

        if (lower.Contains("health") || lower.Contains("hospital") || lower.Contains("medical") || lower.Contains("clinic"))
        {
            return (
                "Expands public healthcare clinics and subsidies for citizen medical coverage.",
                "Health",
                "+8% Public Approval, +6% Human Capital Vitality",
                "+R 200,000 Recurring State Expenditure, -1% Administrative Bandwidth",
                "Entitlement programs directly boost labor productivity and social cohesion, but establish sticky ongoing fiscal burdens (Welfare State Sustainability)."
            );
        }

        if (lower.Contains("defense") || lower.Contains("security") || lower.Contains("border") || lower.Contains("military") || lower.Contains("armed"))
        {
            return (
                "Strengthens defensive perimeter installations and equips standing armed forces.",
                "Security",
                "+15% Defensive Readiness, -10% Domestic Insecurity Index",
                "-R 400,000 Treasury Balance, -3% Regional Diplomatic Relations",
                "Strong deterrence protects national sovereignty, but runs the risk of regional arms races and crowding out civilian welfare (The Security Dilemma)."
            );
        }

        if (lower.Contains("tax") || lower.Contains("fiscal") || lower.Contains("revenue") || lower.Contains("budget"))
        {
            return (
                "Restructures corporate and resource extraction fiscal taxation frameworks.",
                "Economy",
                "+R 450,000 Annualized Fiscal Revenue, +5% Financial Capacity",
                "-5% Private Sector Investment Confidence, -3% Public Approval",
                "Higher sovereign extraction yields immediate treasury inflows but diminishes private capital formation incentives (Laffer Curve Dynamics)."
            );
        }

        if (lower.Contains("energy") || lower.Contains("power") || lower.Contains("refinery") || lower.Contains("grid"))
        {
            return (
                "Subsidizes national energy generation, grid distribution, and refinery throughput.",
                "Energy",
                "+10% Energy Capacity, +6% Industrial Output",
                "+4% Inflationary Subsidy Pressure, -R 300,000 Liquid Reserves",
                "Energy subsidies stabilize baseline manufacturing input costs but introduce market price distortions (Price Ceiling & Subsidy Equilibrium)."
            );
        }

        if (lower.Contains("education") || lower.Contains("school") || lower.Contains("university") || lower.Contains("science"))
        {
            return (
                "Allocates national grants to technical academies, universities, and research institutes.",
                "Education",
                "+10% Science Capacity, +8% Human Capital Quality",
                "-R 250,000 Initial Grant Allocation, benefits delayed across multiple cycles",
                "Human capital investments yield the highest long-term structural compounding but require patient multi-generational capital (Endogenous Growth Theory)."
            );
        }

        // Generic default for any custom player bill title
        return (
            $"Enacts executive legislative measures governing {title}.",
            "Governance",
            "+5% Administrative State Capacity, +3% Institutional Integrity",
            "-R 150,000 Implementation Overhead, -2% Political Faction Satisfaction",
            "Every legislative intervention imposes administrative transaction costs and creates winners and losers across societal factions (Public Choice Theory)."
        );
    }

    /// <summary>
    /// Deconstructs the bill into its constituent components.
    /// </summary>
    public void Deconstruct(out string title, out string countryId, out CountryBillStatus status, out RepublicTime? votedBoundary)
    {
        title = Title;
        countryId = CountryId;
        status = Status;
        votedBoundary = VotedBoundary;
    }

    /// <summary>
    /// Deconstructs the bill into its 3-part representation without country id.
    /// </summary>
    public void Deconstruct(out string title, out CountryBillStatus status, out RepublicTime? votedBoundary)
    {
        title = Title;
        status = Status;
        votedBoundary = VotedBoundary;
    }

    /// <summary>
    /// Returns a formatted representation of the bill title and status.
    /// </summary>
    public override string ToString() => VotedBoundary.HasValue
        ? $"{Title} ({Status} at {VotedBoundary.Value})"
        : $"{Title} ({Status})";

    /// <summary>
    /// Implicitly converts a <see cref="CountryBill"/> to a boolean indicating whether the bill was passed.
    /// </summary>
    public static implicit operator bool(CountryBill? bill) => bill != null && bill.Status == CountryBillStatus.Passed;
}
