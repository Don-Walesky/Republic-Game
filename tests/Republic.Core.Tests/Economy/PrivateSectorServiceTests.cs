namespace Republic.Core.Tests.Economy;

using System;
using System.Linq;
using Republic.Core.Economy.PrivateSector.Models;
using Republic.Core.Economy.PrivateSector.Services;
using Republic.Core.Economy.Treasury;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class PrivateSectorServiceTests
{
    private readonly PrivateSectorService _service = new();

    private static Country CreateTestCountry(double treasuryBalance = 5_000_000.0)
    {
        var launchEpoch = new DateTimeOffset(2030, 1, 1, 0, 0, 0, RepublicTime.WatOffset);
        var foundingTime = RepublicTime.FromDayAndTime(1, new TimeOnly(8, 0), launchEpoch);

        return new Country
        {
            Id = "country_test_biz",
            Name = "Aethelgard",
            CapitalCity = "Oakhaven",
            Treasury = new RepuTreasury(treasuryBalance, "country_test_biz"),
            HappinessRating = 65.0,
            BaselineStability = 70.0,
            GrossDomesticProduct = 10_000_000_000.0
        };
    }

    [Fact]
    public void EnsureCountryPrivateInvestors_InitializesDiverseEnterprises()
    {
        var country = CreateTestCountry();

        var investors = _service.EnsureCountryPrivateInvestors(country);

        Assert.Equal(4, investors.Count);
        Assert.Contains(investors, i => i.Sector == "Manufacturing");
        Assert.Contains(investors, i => i.Sector == "Energy");
        Assert.Contains(investors, i => i.Sector == "Agriculture");
        Assert.Contains(investors, i => i.Sector == "Technology");

        foreach (var inv in investors)
        {
            Assert.True(inv.CapitalCapacity > 0);
            Assert.False(string.IsNullOrWhiteSpace(inv.Name));
        }
    }

    [Fact]
    public void GenerateInvestmentProposals_CreatesPlausibleProposals()
    {
        var country = CreateTestCountry();
        var simTime = RepublicTime.FromDayAndTime(1, new TimeOnly(9, 0), new DateTimeOffset(2030, 1, 1, 0, 0, 0, RepublicTime.WatOffset));

        var proposals = _service.GenerateInvestmentProposals(country, simTime);

        Assert.NotEmpty(proposals);
        Assert.True(proposals.Count <= 3);

        foreach (var prop in proposals)
        {
            Assert.Equal(InvestmentProposalStatus.Pending, prop.Status);
            Assert.True(prop.InvestorContribution > 0);
            Assert.True(prop.RequestedGovernmentFunding > 0);
            Assert.True(prop.ProjectedJobsCreated > 0);
            Assert.True(prop.ProjectedGdpBoost > 0);
            Assert.False(string.IsNullOrWhiteSpace(prop.RisksAndConditions));
        }
    }

    [Fact]
    public void ApplyInvestmentDecision_Accept_DeductsTreasuryOnceAndRegistersProject()
    {
        var country = CreateTestCountry(treasuryBalance: 2_000_000.0);
        var simTime = RepublicTime.FromDayAndTime(1, new TimeOnly(9, 0), new DateTimeOffset(2030, 1, 1, 0, 0, 0, RepublicTime.WatOffset));

        _service.GenerateInvestmentProposals(country, simTime);
        var proposal = country.InvestmentProposals.First();
        var initialTreasury = country.Treasury.Balance;
        var requestedFunding = proposal.RequestedGovernmentFunding;

        bool success = _service.ApplyInvestmentDecision(country, proposal.Id, "accept", out var outcomeMessage);

        Assert.True(success);
        Assert.Contains("authorized", outcomeMessage);
        Assert.Equal(InvestmentProposalStatus.Accepted, proposal.Status);
        Assert.Equal(initialTreasury - requestedFunding, country.Treasury.Balance);

        // Project registered in Projects and ActivePrivateInvestments
        var project = country.Projects.OfType<PrivateInvestmentProject>().FirstOrDefault(p => p.ProposalId == proposal.Id);
        Assert.NotNull(project);
        Assert.True(project.IsActive);
        Assert.False(project.IsCompleted);
        Assert.Contains(country.ActivePrivateInvestments, p => p.ProposalId == proposal.Id);

        // Media coverage generated
        var mediaReport = country.MediaReports.FirstOrDefault(r => r.EventSource == "investment_authorized");
        Assert.NotNull(mediaReport);
        Assert.Equal(Republic.Core.Media.Models.MediaReportSentiment.Positive, mediaReport.Sentiment);
    }

    [Fact]
    public void ApplyInvestmentDecision_Accept_RejectsWhenTreasuryInsufficient()
    {
        var country = CreateTestCountry(treasuryBalance: 100_000.0); // Less than 350,000 requested
        var simTime = RepublicTime.FromDayAndTime(1, new TimeOnly(9, 0), new DateTimeOffset(2030, 1, 1, 0, 0, 0, RepublicTime.WatOffset));

        _service.GenerateInvestmentProposals(country, simTime);
        var proposal = country.InvestmentProposals.First();

        bool success = _service.ApplyInvestmentDecision(country, proposal.Id, "accept", out var outcomeMessage);

        Assert.False(success);
        Assert.Contains("insufficient", outcomeMessage);
        Assert.Equal(InvestmentProposalStatus.Pending, proposal.Status);
        Assert.Equal(100_000.0, country.Treasury.Balance);
    }

    [Fact]
    public void ApplyInvestmentDecision_Reject_ArchivesProposalAndAdjustsRelationship()
    {
        var country = CreateTestCountry();
        var simTime = RepublicTime.FromDayAndTime(1, new TimeOnly(9, 0), new DateTimeOffset(2030, 1, 1, 0, 0, 0, RepublicTime.WatOffset));

        _service.GenerateInvestmentProposals(country, simTime);
        var proposal = country.InvestmentProposals.First();
        var investor = country.PrivateInvestors.First(i => i.Id == proposal.InvestorId);
        var initialRel = investor.GovernmentRelationship;

        bool success = _service.ApplyInvestmentDecision(country, proposal.Id, "reject", out var outcomeMessage);

        Assert.True(success);
        Assert.Contains("declined", outcomeMessage);
        Assert.Equal(InvestmentProposalStatus.Rejected, proposal.Status);
        Assert.True(investor.GovernmentRelationship < initialRel);
    }

    [Fact]
    public void ApplyInvestmentDecision_Negotiate_ReducesGovernmentFunding()
    {
        var country = CreateTestCountry();
        var simTime = RepublicTime.FromDayAndTime(1, new TimeOnly(9, 0), new DateTimeOffset(2030, 1, 1, 0, 0, 0, RepublicTime.WatOffset));

        _service.GenerateInvestmentProposals(country, simTime);
        var proposal = country.InvestmentProposals.First();
        var originalFunding = proposal.RequestedGovernmentFunding;

        bool success = _service.ApplyInvestmentDecision(country, proposal.Id, "negotiate", out var outcomeMessage);

        Assert.True(success);
        Assert.Equal(InvestmentProposalStatus.Negotiated, proposal.Status);
        Assert.True(proposal.RequestedGovernmentFunding < originalFunding);

        // Cannot negotiate a second time
        bool repeatNegotiate = _service.ApplyInvestmentDecision(country, proposal.Id, "negotiate", out var repeatMsg);
        Assert.False(repeatNegotiate);
        Assert.Contains("already renegotiated", repeatMsg);
    }

    [Fact]
    public void ApplyInvestmentDecision_PreventsDuplicateDecisions()
    {
        var country = CreateTestCountry(treasuryBalance: 5_000_000.0);
        var simTime = RepublicTime.FromDayAndTime(1, new TimeOnly(9, 0), new DateTimeOffset(2030, 1, 1, 0, 0, 0, RepublicTime.WatOffset));

        _service.GenerateInvestmentProposals(country, simTime);
        var proposal = country.InvestmentProposals.First();

        _service.ApplyInvestmentDecision(country, proposal.Id, "accept", out _);

        // Attempting to accept again must fail and not double-deduct
        var balanceAfterAccept = country.Treasury.Balance;
        bool duplicateResult = _service.ApplyInvestmentDecision(country, proposal.Id, "accept", out var errorMsg);

        Assert.False(duplicateResult);
        Assert.Contains("already finalized", errorMsg);
        Assert.Equal(balanceAfterAccept, country.Treasury.Balance);
    }

    [Fact]
    public void CheckInvestmentCompletions_WhenFinishBoundaryReached_RealizesGdpAndJobs()
    {
        var country = CreateTestCountry(treasuryBalance: 5_000_000.0);
        var epoch = new DateTimeOffset(2030, 1, 1, 0, 0, 0, RepublicTime.WatOffset);
        var startTime = RepublicTime.FromDayAndTime(1, new TimeOnly(9, 0), epoch);

        _service.GenerateInvestmentProposals(country, startTime);
        var proposal = country.InvestmentProposals.First();

        _service.ApplyInvestmentDecision(country, proposal.Id, "accept", out _);
        var initialGdp = country.GrossDomesticProduct;
        var initialApproval = country.HappinessRating;
        var initialTreasury = country.Treasury.Balance;

        // Advance clock past the finish boundary
        var finishTime = startTime.Add(TimeSpan.FromDays(proposal.EstimatedConstructionDays + 1));

        int completed = _service.CheckInvestmentCompletions(country, finishTime);

        Assert.Equal(1, completed);
        Assert.Equal(InvestmentProposalStatus.Completed, proposal.Status);
        Assert.True(country.GrossDomesticProduct > initialGdp);
        Assert.True(country.HappinessRating > initialApproval);
        Assert.True(country.Treasury.Balance > initialTreasury); // Quarterly tax receipts deposited

        // Completion media story published
        var completionNews = country.MediaReports.FirstOrDefault(r => r.EventSource == "investment_completed");
        Assert.NotNull(completionNews);
        Assert.Contains("Commissioned", completionNews.Headline);
    }
}
