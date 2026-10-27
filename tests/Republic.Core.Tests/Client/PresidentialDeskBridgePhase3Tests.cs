namespace Republic.Core.Tests.Client;

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Republic.Core.Client;
using Republic.Core.Economy.Treasury;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class PresidentialDeskBridgePhase3Tests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _statePath;
    private readonly string _savePath;
    private readonly string _requestPath;

    public PresidentialDeskBridgePhase3Tests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "RepublicPhase3Tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _statePath = Path.Combine(_tempDir, PresidentialDeskBridge.DefaultFileName);
        _savePath = Path.Combine(_tempDir, PresidentialDeskBridge.DefaultSaveFileName);
        _requestPath = Path.Combine(_tempDir, PresidentialDeskBridge.DefaultRequestFileName);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }
        catch { }
    }

    private Country SetupTestEnvironment(double treasury = 5_000_000.0)
    {
        var launchEpoch = new DateTimeOffset(2030, 1, 1, 0, 0, 0, RepublicTime.WatOffset);
        var simTime = RepublicTime.FromDayAndTime(1, new TimeOnly(9, 0), launchEpoch);

        var country = new Country
        {
            Id = "phase3_test_republic",
            Name = "Republic of Veridia",
            CapitalCity = "Novis",
            Treasury = new RepuTreasury(treasury, "phase3_test_republic"),
            HappinessRating = 68.0,
            BaselineStability = 72.0
        };

        PresidentialDeskBridge.ExportState(country, simTime, 1, _statePath, _savePath);
        return country;
    }

    [Fact]
    public void SnapshotCreate_PopulatesMediaAndPrivateSectorContracts()
    {
        var country = SetupTestEnvironment();
        var launchEpoch = new DateTimeOffset(2030, 1, 1, 0, 0, 0, RepublicTime.WatOffset);
        var simTime = RepublicTime.FromDayAndTime(1, new TimeOnly(12, 0), launchEpoch);

        var snapshot = PresidentialDeskSnapshot.Create(country, simTime, 1, _savePath);

        Assert.NotNull(snapshot.MediaOutlets);
        Assert.Equal(4, snapshot.MediaOutlets.Count);
        Assert.NotNull(snapshot.MediaReports);
        Assert.NotEmpty(snapshot.MediaSummary);

        Assert.NotNull(snapshot.PrivateInvestors);
        Assert.Equal(4, snapshot.PrivateInvestors.Count);
        Assert.NotNull(snapshot.InvestmentProposals);
        Assert.NotEmpty(snapshot.InvestmentSummary);
    }

    [Fact]
    public void ProcessPendingDecisionRequest_HandlesMediaResponse()
    {
        var country = SetupTestEnvironment();
        var launchEpoch = new DateTimeOffset(2030, 1, 1, 0, 0, 0, RepublicTime.WatOffset);
        var simTime = RepublicTime.FromDayAndTime(2, new TimeOnly(10, 0), launchEpoch);

        // Export state to generate initial media reports
        PresidentialDeskBridge.ExportState(country, simTime, 2, _statePath, _savePath);

        // Read snapshot to get a real report ID
        var stateJson = File.ReadAllText(_statePath);
        var snapshot = PresidentialDeskBridge.Deserialize(stateJson);
        Assert.NotNull(snapshot);
        Assert.NotEmpty(snapshot.MediaReports);
        var targetReport = snapshot.MediaReports.First();

        // Write decision request
        var requestObj = new
        {
            action = "media_response",
            report_id = targetReport.Id,
            response_type = "statement",
            confirmed = true,
            request_id = "req_media_101"
        };
        File.WriteAllText(_requestPath, JsonSerializer.Serialize(requestObj));

        var result = PresidentialDeskBridge.ProcessPendingDecisionRequest(_statePath, _savePath, _requestPath);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Success", result.Result);
        Assert.Equal("req_media_101", result.RequestId);
        Assert.Contains("Executive press statement delivered", result.Message);
        Assert.False(File.Exists(_requestPath)); // Request file deleted
    }

    [Fact]
    public void ProcessPendingDecisionRequest_HandlesInvestmentDecision()
    {
        var country = SetupTestEnvironment();
        var launchEpoch = new DateTimeOffset(2030, 1, 1, 0, 0, 0, RepublicTime.WatOffset);
        var simTime = RepublicTime.FromDayAndTime(1, new TimeOnly(10, 0), launchEpoch);

        // Export state to generate proposals
        PresidentialDeskBridge.ExportState(country, simTime, 1, _statePath, _savePath);

        var stateJson = File.ReadAllText(_statePath);
        var snapshot = PresidentialDeskBridge.Deserialize(stateJson);
        Assert.NotNull(snapshot);
        Assert.NotEmpty(snapshot.InvestmentProposals);
        var targetProposal = snapshot.InvestmentProposals.First();

        var requestObj = new
        {
            action = "investment_decision",
            proposal_id = targetProposal.Id,
            decision = "negotiate",
            confirmed = true,
            request_id = "req_invest_202"
        };
        File.WriteAllText(_requestPath, JsonSerializer.Serialize(requestObj));

        var result = PresidentialDeskBridge.ProcessPendingDecisionRequest(_statePath, _savePath, _requestPath);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Success", result.Result);
        Assert.Equal("req_invest_202", result.RequestId);
        Assert.Contains("Terms renegotiated", result.Message);
        Assert.False(File.Exists(_requestPath));
    }
}
