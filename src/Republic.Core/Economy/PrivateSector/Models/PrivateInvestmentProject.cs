namespace Republic.Core.Economy.PrivateSector.Models;

using Republic.Core.Economy.Projects;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Domain model representing a commercial facility or public-private co-investment project.
/// Integrates directly into Republic's authoritative <see cref="DevelopmentProject"/> lifecycle.
/// </summary>
public sealed class PrivateInvestmentProject : DevelopmentProject
{
    private string _projectName = "Private Sector Investment";

    /// <summary>
    /// Gets the unique identifier of the originating investment proposal.
    /// </summary>
    public string ProposalId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the corporate name of the private investor partner.
    /// </summary>
    public string InvestorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the private capital contributed by the enterprise in REPU.
    /// </summary>
    public double InvestorContribution { get; set; }

    /// <summary>
    /// Gets or sets the direct jobs created upon project commissioning.
    /// </summary>
    public int JobsCreated { get; set; } = 500;

    /// <summary>
    /// Gets or sets the permanent annualized boost to national Gross Domestic Product in REPU.
    /// </summary>
    public double GdpBoost { get; set; } = 1_000_000_000.0;

    /// <summary>
    /// Gets or sets the projected annual corporate tax receipts generated for the national treasury.
    /// </summary>
    public double AnnualTaxRevenue { get; set; } = 80_000.0;

    /// <summary>
    /// Gets or sets the industrial sector.
    /// </summary>
    public string Sector { get; set; } = "Manufacturing";

    /// <inheritdoc />
    public override DevelopmentProjectType ProjectType => DevelopmentProjectType.PrivateInvestment;

    /// <inheritdoc />
    public override string Name => _projectName;

    /// <summary>
    /// Sets the custom display name of the commercial facility.
    /// </summary>
    public void SetProjectName(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            _projectName = name;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrivateInvestmentProject"/> class.
    /// Parameterless constructor required for polymorphic JSON deserialization.
    /// </summary>
    public PrivateInvestmentProject()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrivateInvestmentProject"/> class with explicit proposal details.
    /// </summary>
    public PrivateInvestmentProject(
        string countryId,
        string proposalId,
        string title,
        string investorName,
        string sector,
        double governmentFunding,
        double investorContribution,
        int constructionDays,
        int jobsCreated,
        double gdpBoost,
        double annualTaxRevenue,
        RepublicTime startedBoundary)
    {
        CountryId = countryId;
        ProposalId = proposalId;
        _projectName = title;
        InvestorName = investorName;
        Sector = sector;
        Cost = governmentFunding;
        InvestorContribution = investorContribution;
        JobsCreated = jobsCreated;
        GdpBoost = gdpBoost;
        AnnualTaxRevenue = annualTaxRevenue;
        StartedBoundary = startedBoundary;
        FinishBoundary = startedBoundary.Add(TimeSpan.FromDays(constructionDays));
    }

    /// <inheritdoc />
    protected override void ApplyCompletionEffect(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        // Apply permanent Gross Domestic Product expansion
        country.GrossDomesticProduct += GdpBoost;

        // Apply civic welfare / employment boost
        country.HappinessRating = Math.Clamp(country.HappinessRating + 2.5, 0.0, 100.0);

        // Deposit first quarter's tax receipts into the national treasury
        country.Treasury.Deposit(AnnualTaxRevenue / 4.0);
    }
}
