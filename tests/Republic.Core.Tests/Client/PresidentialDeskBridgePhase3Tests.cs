namespace Republic.Core.Tests.Client;

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Republic.Core.Client;
using Republic.Core.Economy.Treasury;
using Republic.Core.Persistence;
using Republic.Core.Persistence.Models;
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

    [Fact]
    public void ApplyPurchaseFurnitureDecision_SuccessfulPurchase_DeductsTreasuryExactlyOnceAndRecordsOwnership()
    {
        var country = SetupTestEnvironment(treasury: 5_000_000.0);

        // carpet_emerald_runner costs 520,000 REPU
        var result = PresidentialDeskBridge.ApplyPurchaseFurnitureDecision(
            itemId: "carpet_emerald_runner",
            price: 520_000.0,
            confirmed: true,
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestId: "req_furn_success");

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Success", result.Result);
        Assert.Equal(5_000_000.0 - 520_000.0, result.TreasuryBalance);
        Assert.Contains("Emerald Silk Runner", result.Message);
        Assert.Contains(RepuTreasury.Format(520_000.0), result.Message);

        // Verify save file persistence
        var savePayload = File.ReadAllText(_savePath);
        var serializer = new JsonStateSerializer();
        var envelope = serializer.Deserialize<FullGameState>(savePayload);
        Assert.NotNull(envelope?.State?.World?.Countries);
        Assert.Equal(4_480_000.0, envelope!.State.World.Countries.First().Treasury.Balance);
        Assert.NotNull(envelope.State.Workspace?.RoomState?.OwnedFurniture);
        Assert.Contains("carpet_emerald_runner", envelope.State.Workspace.RoomState.OwnedFurniture);

        // Verify second purchase attempts do not deduct funds twice (exactly once rule)
        var secondResult = PresidentialDeskBridge.ApplyPurchaseFurnitureDecision(
            itemId: "carpet_emerald_runner",
            confirmed: true,
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestId: "req_furn_repeat");

        Assert.NotNull(secondResult);
        Assert.True(secondResult.Success);
        Assert.Equal("AlreadyOwned", secondResult.Result);
        Assert.Equal(4_480_000.0, secondResult.TreasuryBalance);
    }

    [Fact]
    public void ApplyPurchaseFurnitureDecision_InsufficientFunds_RejectsAndPreservesTreasury()
    {
        // Setup with low treasury
        var country = SetupTestEnvironment(treasury: 100_000.0);

        // wall_walnut_granite costs 1,200,000 REPU
        var result = PresidentialDeskBridge.ApplyPurchaseFurnitureDecision(
            itemId: "wall_walnut_granite",
            confirmed: true,
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestId: "req_furn_insufficient");

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("InsufficientFunds", result.Result);
        Assert.Equal(100_000.0, result.TreasuryBalance);
        Assert.Contains("Insufficient sovereign treasury funds", result.Message);

        // Verify save file treasury was not mutated
        var savePayload = File.ReadAllText(_savePath);
        var serializer = new JsonStateSerializer();
        var envelope = serializer.Deserialize<FullGameState>(savePayload);
        Assert.NotNull(envelope?.State?.World?.Countries);
        Assert.Equal(100_000.0, envelope!.State.World.Countries.First().Treasury.Balance);
        if (envelope.State.Workspace?.RoomState?.OwnedFurniture != null)
        {
            Assert.DoesNotContain("wall_walnut_granite", envelope.State.Workspace.RoomState.OwnedFurniture);
        }
    }

    [Fact]
    public void ApplyPurchaseFurnitureDecision_InvalidItem_RejectsAndPreservesTreasury()
    {
        var country = SetupTestEnvironment(treasury: 5_000_000.0);

        var result = PresidentialDeskBridge.ApplyPurchaseFurnitureDecision(
            itemId: "fictional_laser_couch_9000",
            confirmed: true,
            stateFilePath: _statePath,
            saveFilePath: _savePath,
            requestId: "req_furn_invalid");

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("InvalidItem", result.Result);
        Assert.Equal(5_000_000.0, result.TreasuryBalance);
        Assert.Contains("not recognized", result.Message);

        // Verify save file treasury was not mutated
        var savePayload = File.ReadAllText(_savePath);
        var serializer = new JsonStateSerializer();
        var envelope = serializer.Deserialize<FullGameState>(savePayload);
        Assert.NotNull(envelope?.State?.World?.Countries);
        Assert.Equal(5_000_000.0, envelope!.State.World.Countries.First().Treasury.Balance);
    }

    [Fact]
    public void ProcessPendingDecisionRequest_HandlesPurchaseFurniture()
    {
        var country = SetupTestEnvironment(treasury: 5_000_000.0);

        // desk_obsidian_modern costs 350,000 REPU
        var requestObj = new
        {
            action = "purchase_furniture",
            item_id = "desk_obsidian_modern",
            category = "desk_and_chair",
            price_repu = 350_000.0,
            confirmed = true,
            request_id = "req_furn_pipeline_01"
        };
        File.WriteAllText(_requestPath, JsonSerializer.Serialize(requestObj));

        var result = PresidentialDeskBridge.ProcessPendingDecisionRequest(_statePath, _savePath, _requestPath);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Success", result.Result);
        Assert.Equal("req_furn_pipeline_01", result.RequestId);
        Assert.Equal(5_000_000.0 - 350_000.0, result.TreasuryBalance);
        Assert.Contains("Obsidian Glass", result.Message);
        Assert.False(File.Exists(_requestPath)); // Request file consumed and deleted
    }
}
