namespace Republic.Core.Tests.Media;

using System;
using System.Linq;
using Republic.Core.Economy.Projects;
using Republic.Core.Economy.Treasury;
using Republic.Core.Media.Models;
using Republic.Core.Media.Services;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class MediaSimulationServiceTests
{
    private readonly MediaSimulationService _service = new();

    private static Country CreateTestCountry(double treasuryBalance = 5_000_000.0)
    {
        var launchEpoch = new DateTimeOffset(2030, 1, 1, 0, 0, 0, RepublicTime.WatOffset);
        var foundingTime = RepublicTime.FromDayAndTime(1, new TimeOnly(8, 0), launchEpoch);

        return new Country
        {
            Id = "country_test_1",
            Name = "Aethelgard",
            CapitalCity = "Oakhaven",
            Treasury = new RepuTreasury(treasuryBalance, "country_test_1"),
            HappinessRating = 65.0,
            BaselineStability = 70.0
        };
    }

    [Fact]
    public void EnsureCountryMediaOutlets_InitializesDiverseOutlets()
    {
        var country = CreateTestCountry();

        var outlets = _service.EnsureCountryMediaOutlets(country);

        Assert.Equal(4, outlets.Count);
        Assert.Contains(outlets, o => o.Leaning == MediaEditorialLeaning.StateAligned);
        Assert.Contains(outlets, o => o.Leaning == MediaEditorialLeaning.InvestigativeIndependent);
        Assert.Contains(outlets, o => o.Leaning == MediaEditorialLeaning.FinancialBusiness);
        Assert.Contains(outlets, o => o.Leaning == MediaEditorialLeaning.PopulistOpposition);

        // Verify distinct credibility and audience reaches
        var indep = outlets.First(o => o.Leaning == MediaEditorialLeaning.InvestigativeIndependent);
        var state = outlets.First(o => o.Leaning == MediaEditorialLeaning.StateAligned);

        Assert.True(indep.Credibility > state.Credibility);
        Assert.True(state.GovernmentAttitude > indep.GovernmentAttitude);
    }

    [Fact]
    public void GenerateMediaReports_ReflectsLowTreasuryFiscalWarning()
    {
        var country = CreateTestCountry(treasuryBalance: 500_000.0); // Below 1,000,000 threshold
        var simTime = RepublicTime.FromDayAndTime(3, new TimeOnly(10, 0), new DateTimeOffset(2030, 1, 1, 0, 0, 0, RepublicTime.WatOffset));

        var reports = _service.GenerateMediaReports(country, simTime);

        Assert.NotEmpty(reports);
        var deficitReport = reports.FirstOrDefault(r => r.EventSource == "treasury_deficit");
        Assert.NotNull(deficitReport);
        Assert.Equal(MediaReportSentiment.Critical, deficitReport.Sentiment);
        Assert.True(deficitReport.PublicApprovalImpact < 0);
        Assert.True(deficitReport.StabilityImpact < 0);
    }

    [Fact]
    public void GenerateMediaReports_ReflectsCompletedInfrastructure()
    {
        var country = CreateTestCountry(treasuryBalance: 8_000_000.0);
        var simTime = RepublicTime.FromDayAndTime(10, new TimeOnly(12, 0), new DateTimeOffset(2030, 1, 1, 0, 0, 0, RepublicTime.WatOffset));

        var completedRoad = new NationalRoadProject(country.Id, simTime)
        {
            Completed = true
        };
        country.Projects.Add(completedRoad);

        var reports = _service.GenerateMediaReports(country, simTime);

        var infraReport = reports.FirstOrDefault(r => r.EventSource == "project_completion");
        Assert.NotNull(infraReport);
        Assert.Equal(MediaReportSentiment.Positive, infraReport.Sentiment);
        Assert.True(infraReport.PublicApprovalImpact > 0);
    }

    [Fact]
    public void ApplyMediaResponse_OfficialStatement_RecoversApprovalAndCostsTreasury()
    {
        var country = CreateTestCountry(treasuryBalance: 2_000_000.0);
        var simTime = RepublicTime.FromDayAndTime(2, new TimeOnly(9, 0), new DateTimeOffset(2030, 1, 1, 0, 0, 0, RepublicTime.WatOffset));

        _service.EnsureCountryMediaOutlets(country);
        country.MediaReports.Add(new MediaReport
        {
            Id = "rep_test_critical",
            OutletId = "outlet_criterion_dispatch",
            Headline = "Government Deficit Deepens",
            PublicApprovalImpact = -4.0,
            StabilityImpact = -2.0,
            PublishedDay = 2,
            Leaning = MediaEditorialLeaning.InvestigativeIndependent
        });

        var initialApproval = country.HappinessRating;
        var initialTreasury = country.Treasury.Balance;

        bool success = _service.ApplyMediaResponse(country, "rep_test_critical", "statement", out var outcomeMessage);

        Assert.True(success);
        Assert.Contains("Executive press statement delivered", outcomeMessage);
        Assert.Equal(initialTreasury - MediaSimulationService.StatementCost, country.Treasury.Balance);
        Assert.True(country.HappinessRating > initialApproval);

        var report = country.MediaReports.First(r => r.Id == "rep_test_critical");
        Assert.Equal(MediaResponseStatus.StatementIssued, report.ResponseStatus);
    }

    [Fact]
    public void ApplyMediaResponse_OfficialStatement_RejectsWhenTreasuryInsufficient()
    {
        var country = CreateTestCountry(treasuryBalance: 5_000.0); // Below 10,000 StatementCost
        country.MediaReports.Add(new MediaReport
        {
            Id = "rep_test_broke",
            Headline = "Costly Decision",
            PublicApprovalImpact = -3.0
        });

        bool success = _service.ApplyMediaResponse(country, "rep_test_broke", "statement", out var outcomeMessage);

        Assert.False(success);
        Assert.Contains("insufficient", outcomeMessage);
        Assert.Equal(5_000.0, country.Treasury.Balance);
    }

    [Fact]
    public void ApplyMediaResponse_HushMoney_RequiresValidTreasury()
    {
        var country = CreateTestCountry(treasuryBalance: 50_000.0); // Below 150,000 HushMoneyCost
        country.MediaReports.Add(new MediaReport
        {
            Id = "rep_test_hush",
            Headline = "Damaging Investigation",
            PublicApprovalImpact = -6.0
        });

        bool success = _service.ApplyMediaResponse(country, "rep_test_hush", "hush_money", out var outcomeMessage);

        Assert.False(success);
        Assert.Contains("cannot cover covert payment", outcomeMessage);
    }

    [Fact]
    public void ApplyMediaResponse_Censorship_AppliesCivilLibertiesHitAndTriggersSamizdatLeak()
    {
        var country = CreateTestCountry(treasuryBalance: 3_000_000.0);
        _service.EnsureCountryMediaOutlets(country);

        var outlet = country.MediaOutlets.First(o => o.Leaning == MediaEditorialLeaning.InvestigativeIndependent);
        var report = new MediaReport
        {
            Id = "rep_investigative_leak",
            OutletId = outlet.Id,
            Headline = "Secret Mineral Concessions Uncovered",
            Leaning = outlet.Leaning,
            PublicApprovalImpact = -5.0,
            PublishedDay = 4
        };
        country.MediaReports.Add(report);

        var initialApproval = country.HappinessRating;
        var initialStability = country.BaselineStability;

        bool success = _service.ApplyMediaResponse(country, "rep_investigative_leak", "censorship", out var outcomeMessage);

        Assert.True(success);
        Assert.Contains("Emergency gag order", outcomeMessage);
        Assert.True(report.IsSuppressed);
        Assert.Equal(MediaResponseStatus.Censored, report.ResponseStatus);

        // Civil liberties consequences applied
        Assert.True(country.HappinessRating < initialApproval);
        Assert.True(country.BaselineStability < initialStability);

        // Outlet marked as censored and strikes incremented
        Assert.True(outlet.IsCensored);
        Assert.Equal(1, outlet.ConsecutiveStrikes);

        // Underground samizdat leak created
        var samizdat = country.MediaReports.FirstOrDefault(r => r.IsUndergroundLeak);
        Assert.NotNull(samizdat);
        Assert.Contains("BANNED DISPATCH", samizdat.Headline);
    }

    [Fact]
    public void ApplyMediaResponse_Approve_PublishesAndAppliesImpact()
    {
        var country = CreateTestCountry();
        _service.EnsureCountryMediaOutlets(country);

        var report = new MediaReport
        {
            Id = "rep_sensitive_test",
            OutletId = "outlet_criterion_dispatch",
            OutletName = "The Criterion Dispatch",
            Headline = "Ministerial Audit Discrepancies",
            PublicApprovalImpact = -4.0,
            StabilityImpact = -2.0,
            OutletCredibility = 90.0,
            OutletReach = 60.0,
            PublishedDay = 5,
            RequiresDecision = true,
            ResponseStatus = MediaResponseStatus.Unanswered
        };
        country.MediaReports.Add(report);

        var initialApproval = country.HappinessRating;
        var initialStability = country.BaselineStability;
        var initialCorruption = country.StateCapacity?.CorruptionControl ?? 0.3;

        bool success = _service.ApplyMediaResponse(country, "rep_sensitive_test", "approve", out var outcomeMessage);

        Assert.True(success);
        Assert.Contains("approved for publication", outcomeMessage);
        Assert.False(report.RequiresDecision);
        Assert.False(report.IsSuppressed);
        Assert.Equal(MediaResponseStatus.StatementIssued, report.ResponseStatus);
        Assert.True(country.HappinessRating < initialApproval);
        Assert.True(country.BaselineStability < initialStability);
        // Corruption did NOT increase on transparent approval
        Assert.Equal(initialCorruption, country.StateCapacity?.CorruptionControl ?? 0.3);
    }

    [Fact]
    public void ApplyMediaResponse_Hush_SuppressesStoryAndIncreasesCorruption()
    {
        var country = CreateTestCountry(treasuryBalance: 5_000_000.0);
        _service.EnsureCountryMediaOutlets(country);

        var report = new MediaReport
        {
            Id = "rep_scandal_test",
            OutletId = "outlet_criterion_dispatch",
            OutletName = "The Criterion Dispatch",
            Headline = "Damaging Investigation into Government Contracts",
            PublicApprovalImpact = -5.0,
            StabilityImpact = -3.0,
            OutletCredibility = 90.0,
            OutletReach = 60.0,
            PublishedDay = 5,
            RequiresDecision = true,
            ResponseStatus = MediaResponseStatus.Unanswered
        };
        country.MediaReports.Add(report);

        var initialApproval = country.HappinessRating;
        var initialStability = country.BaselineStability;
        var initialCorruptionControl = country.StateCapacity?.CorruptionControl ?? 0.3;

        bool success = _service.ApplyMediaResponse(country, "rep_scandal_test", "hush", out var outcomeMessage);

        Assert.True(success);
        Assert.Contains("Story suppressed from press", outcomeMessage);
        Assert.True(report.IsSuppressed);
        Assert.False(report.RequiresDecision);
        Assert.Equal(MediaResponseStatus.HushMoneyAccepted, report.ResponseStatus);

        // Story damage to approval/stability was suppressed
        Assert.Equal(initialApproval, country.HappinessRating);
        Assert.Equal(initialStability, country.BaselineStability);

        // Corruption increased (CorruptionControl decreased)
        var newCorruptionControl = country.StateCapacity?.CorruptionControl ?? 0.3;
        Assert.True(newCorruptionControl < initialCorruptionControl);
    }

    [Fact]
    public void ApplyMediaResponse_Ignore_AutoPublishesStoryAndAppliesImpact()
    {
        var country = CreateTestCountry();
        _service.EnsureCountryMediaOutlets(country);

        var report = new MediaReport
        {
            Id = "rep_ignored_story",
            OutletId = "outlet_criterion_dispatch",
            OutletName = "The Criterion Dispatch",
            Headline = "Neglected Infrastructure Warning",
            PublicApprovalImpact = -3.0,
            StabilityImpact = -2.0,
            OutletCredibility = 90.0,
            OutletReach = 50.0,
            PublishedDay = 6,
            RequiresDecision = true,
            ResponseStatus = MediaResponseStatus.Unanswered
        };
        country.MediaReports.Add(report);

        var initialApproval = country.HappinessRating;

        bool success = _service.ApplyMediaResponse(country, "rep_ignored_story", "ignore", out var outcomeMessage);

        Assert.True(success);
        Assert.Contains("auto-published after being ignored", outcomeMessage);
        Assert.False(report.RequiresDecision);
        Assert.False(report.IsSuppressed);
        Assert.True(country.HappinessRating < initialApproval);
    }

    [Fact]
    public void GenerateMediaReports_SeparatesDecisionRequiredFromBackgroundNews()
    {
        var country = CreateTestCountry(treasuryBalance: 50_000_000.0);
        var simTime = RepublicTime.FromDayAndTime(1, new TimeOnly(10, 0), new DateTimeOffset(2030, 1, 1, 0, 0, 0, RepublicTime.WatOffset));

        var reports = _service.GenerateMediaReports(country, simTime);

        // Both decision-required sensitive stories and background ordinary stories are generated
        var decisionStories = reports.Where(r => r.RequiresDecision).ToList();
        var backgroundStories = reports.Where(r => !r.RequiresDecision).ToList();

        Assert.NotEmpty(decisionStories);
        Assert.NotEmpty(backgroundStories);

        // Decision stories have predicted effect documented
        Assert.All(decisionStories, r => Assert.False(string.IsNullOrWhiteSpace(r.PredictedEffect)));

        // Ordinary background stories (like fiscal surplus or strategy) publish automatically
        Assert.Contains(backgroundStories, r => r.EventSource == "treasury_surplus" || r.EventSource == "national_strategic_direction");
    }
}
