namespace Republic.Core.Tests.Politics;

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Republic.Core.Client;
using Republic.Core.Economy.Treasury;
using Republic.Core.Persistence;
using Republic.Core.Persistence.Models;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Xunit;

public sealed class CountryElectionJourneyTests
{
    private static readonly DateTimeOffset LaunchEpoch = new(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));

    private static RepublicTime CreateTime(int days, int hours = 0, int minutes = 0)
    {
        var timestamp = LaunchEpoch.AddDays(days).AddHours(hours).AddMinutes(minutes);
        return new RepublicTime(timestamp, LaunchEpoch);
    }

    [Fact]
    public void FoundingCountry_EstablishesPoliticalSystem_WithMandateAndOpposition()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "Republic_Test_Politics_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var tempSaveFile = Path.Combine(tempDir, "game.sav");
        var tempStateFile = Path.Combine(tempDir, "state.json");

        try
        {
            var result = PresidentialDeskBridge.CreateOrLoadCountry(
                "Republic of Veridia",
                tempStateFile,
                tempSaveFile);

            Assert.True(result.Success);
            Assert.Equal("Democratic Union", result.RulingParty);
            Assert.Equal("National Civic Front", result.OppositionParty);
            Assert.Contains("Veridia", result.CountryName);

            var snapshot = PresidentialDeskBridge.LoadState(tempStateFile);
            Assert.NotNull(snapshot);
            Assert.Equal("Democratic Union", snapshot.RulingParty);
            Assert.Equal("National Civic Front", snapshot.OppositionParty);
            Assert.True(snapshot.PollingSupport >= 50.0);
            Assert.True(snapshot.OppositionSupport >= 40.0);
            Assert.True(snapshot.NextElectionDay > 0);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void ApplyCampaignRallyDecision_Cancelled_LeavesTreasuryAndSupportUnchanged()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "Republic_Test_RallyCancel_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var tempSaveFile = Path.Combine(tempDir, "game.sav");
        var tempStateFile = Path.Combine(tempDir, "state.json");

        try
        {
            PresidentialDeskBridge.CreateOrLoadCountry("Republic of Veridia", tempStateFile, tempSaveFile);
            var initialSnapshot = PresidentialDeskBridge.LoadState(tempStateFile)!;
            var initialTreasury = initialSnapshot.TreasuryBalance;
            var initialSupport = initialSnapshot.PollingSupport;

            var cancelResult = PresidentialDeskBridge.ApplyCampaignRallyDecision(
                confirmed: false,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);

            Assert.False(cancelResult.Success);
            Assert.Equal("Cancelled", cancelResult.Result);
            Assert.Equal(initialTreasury, cancelResult.TreasuryBalance);

            var snapshot = PresidentialDeskBridge.LoadState(tempStateFile)!;
            Assert.Equal(initialTreasury, snapshot.TreasuryBalance);
            Assert.Equal(initialSupport, snapshot.PollingSupport);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void ApplyCampaignRallyDecision_Confirmed_DeductsFundsAndBoostsSupport()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "Republic_Test_RallySuccess_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var tempSaveFile = Path.Combine(tempDir, "game.sav");
        var tempStateFile = Path.Combine(tempDir, "state.json");

        try
        {
            PresidentialDeskBridge.CreateOrLoadCountry("Republic of Veridia", tempStateFile, tempSaveFile);
            var initialSnapshot = PresidentialDeskBridge.LoadState(tempStateFile)!;
            var initialTreasury = initialSnapshot.TreasuryBalance;
            var initialSupport = initialSnapshot.PollingSupport;
            var initialApproval = initialSnapshot.ApprovalRating;

            var result = PresidentialDeskBridge.ApplyCampaignRallyDecision(
                confirmed: true,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);

            Assert.True(result.Success);
            Assert.Equal("Success", result.Result);
            Assert.Equal(initialTreasury - 250_000.0, result.TreasuryBalance);
            Assert.True(result.ApprovalRating > initialApproval);

            var reloaded = PresidentialDeskBridge.LoadState(tempStateFile)!;
            Assert.Equal(initialTreasury - 250_000.0, reloaded.TreasuryBalance);
            Assert.Equal(initialSupport + 4.0, reloaded.PollingSupport);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void ApplyCampaignRallyDecision_InsufficientFunds_RejectsCleanly()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "Republic_Test_RallyNoFunds_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var tempSaveFile = Path.Combine(tempDir, "game.sav");
        var tempStateFile = Path.Combine(tempDir, "state.json");

        try
        {
            PresidentialDeskBridge.CreateOrLoadCountry("Republic of Veridia", tempStateFile, tempSaveFile);

            // Deplete treasury in save file directly
            var serializer = new JsonStateSerializer();
            var envelope = serializer.Deserialize<FullGameState>(File.ReadAllText(tempSaveFile));
            Assert.NotNull(envelope);
            Assert.NotNull(envelope.State);
            var country = envelope.State.World.Countries.First();
            country.Treasury.Withdraw(country.Treasury.Balance); // balance = 0
            File.WriteAllText(tempSaveFile, serializer.Serialize(envelope));

            var result = PresidentialDeskBridge.ApplyCampaignRallyDecision(
                confirmed: true,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);

            Assert.False(result.Success);
            Assert.Equal("InsufficientFunds", result.Result);
            Assert.Equal(0.0, result.TreasuryBalance);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void ApplyElectionDecision_Cancelled_LeavesMandateIntact()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "Republic_Test_ElectionCancel_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var tempSaveFile = Path.Combine(tempDir, "game.sav");
        var tempStateFile = Path.Combine(tempDir, "state.json");

        try
        {
            PresidentialDeskBridge.CreateOrLoadCountry("Republic of Veridia", tempStateFile, tempSaveFile);
            var initialSnapshot = PresidentialDeskBridge.LoadState(tempStateFile)!;

            var cancelResult = PresidentialDeskBridge.ApplyElectionDecision(
                confirmed: false,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);

            Assert.False(cancelResult.Success);
            Assert.Equal("Cancelled", cancelResult.Result);
            Assert.Equal(initialSnapshot.RulingParty, cancelResult.RulingParty);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void ApplyElectionDecision_HighApproval_RenewsMandateWithElectoralVictory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "Republic_Test_ElectionVictory_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var tempSaveFile = Path.Combine(tempDir, "game.sav");
        var tempStateFile = Path.Combine(tempDir, "state.json");

        try
        {
            PresidentialDeskBridge.CreateOrLoadCountry("Republic of Veridia", tempStateFile, tempSaveFile);

            // Boost approval rating high
            var serializer = new JsonStateSerializer();
            var envelope = serializer.Deserialize<FullGameState>(File.ReadAllText(tempSaveFile));
            Assert.NotNull(envelope);
            Assert.NotNull(envelope.State);
            var country = envelope.State.World.Countries.First();
            country.HappinessRating = 85.0;
            File.WriteAllText(tempSaveFile, serializer.Serialize(envelope));

            var result = PresidentialDeskBridge.ApplyElectionDecision(
                confirmed: true,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);

            Assert.True(result.Success);
            Assert.Equal("Success", result.Result);
            Assert.True(result.IsReelected);
            Assert.Contains("Electoral Victory", result.Message);
            Assert.Equal("Democratic Union", result.RulingParty);

            var snapshot = PresidentialDeskBridge.LoadState(tempStateFile)!;
            Assert.Equal("Democratic Union", snapshot.RulingParty);
            Assert.Contains("re-elected", snapshot.LatestDecisionResult.ToLowerInvariant());
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void ApplyElectionDecision_OppositionLeading_TransitionsPowerPeacefully()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "Republic_Test_ElectionDefeat_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var tempSaveFile = Path.Combine(tempDir, "game.sav");
        var tempStateFile = Path.Combine(tempDir, "state.json");

        try
        {
            PresidentialDeskBridge.CreateOrLoadCountry("Republic of Veridia", tempStateFile, tempSaveFile);

            // Depress approval rating and boost opposition
            var serializer = new JsonStateSerializer();
            var envelope = serializer.Deserialize<FullGameState>(File.ReadAllText(tempSaveFile));
            Assert.NotNull(envelope);
            Assert.NotNull(envelope.State);
            var country = envelope.State.World.Countries.First();
            country.HappinessRating = 20.0;
            if (country.RulingParty != null)
            {
                country.RulingParty = country.RulingParty with { Support = 30.0 };
            }
            if (country.OppositionParty != null)
            {
                country.OppositionParty = country.OppositionParty with { Support = 70.0 };
            }
            File.WriteAllText(tempSaveFile, serializer.Serialize(envelope));

            var result = PresidentialDeskBridge.ApplyElectionDecision(
                confirmed: true,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);

            Assert.True(result.Success);
            Assert.Equal("Success", result.Result);
            Assert.False(result.IsReelected);
            Assert.Contains("Democratic Transition", result.Message);
            Assert.Contains("Opposition assumes", result.Message);
            Assert.Equal("National Civic Front", result.RulingParty);
            Assert.Equal("Democratic Union", result.OppositionParty);

            var snapshot = PresidentialDeskBridge.LoadState(tempStateFile)!;
            Assert.Equal("National Civic Front", snapshot.RulingParty);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void ProcessPendingDecisionRequest_DispatchesRallyAndElectionFileRequests()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "Republic_Test_RequestFileDispatch_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var tempSaveFile = Path.Combine(tempDir, "game.sav");
        var tempStateFile = Path.Combine(tempDir, "state.json");
        var requestFile = Path.Combine(tempDir, "decision_request.json");
        var resultFile = PresidentialDeskBridge.ResolveDefaultResultFilePath(tempStateFile);

        try
        {
            PresidentialDeskBridge.CreateOrLoadCountry("Republic of Veridia", tempStateFile, tempSaveFile);

            // 1. Submit campaign rally request
            var rallyRequest = new
            {
                action = "campaign_rally",
                confirmed = true,
                request_id = "req-rally-001"
            };
            File.WriteAllText(requestFile, JsonSerializer.Serialize(rallyRequest));

            var rallyActionResult = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: requestFile);

            Assert.NotNull(rallyActionResult);
            Assert.True(rallyActionResult.Success);
            Assert.False(File.Exists(requestFile));
            Assert.True(File.Exists(resultFile));

            var rallyResult = JsonSerializer.Deserialize<PresidentialActionResult>(File.ReadAllText(resultFile));
            Assert.NotNull(rallyResult);
            Assert.Equal("req-rally-001", rallyResult.RequestId);
            Assert.True(rallyResult.Success);
            Assert.Contains("Campaign rally successfully conducted", rallyResult.Message);

            // 2. Submit contest election request
            File.Delete(resultFile);
            var electionRequest = new
            {
                action = "contest_election",
                confirmed = true,
                request_id = "req-election-002"
            };
            File.WriteAllText(requestFile, JsonSerializer.Serialize(electionRequest));

            var electionActionResult = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: requestFile);

            Assert.NotNull(electionActionResult);
            Assert.True(electionActionResult.Success);
            Assert.False(File.Exists(requestFile));
            Assert.True(File.Exists(resultFile));

            var electionResult = JsonSerializer.Deserialize<PresidentialActionResult>(File.ReadAllText(resultFile));
            Assert.NotNull(electionResult);
            Assert.Equal("req-election-002", electionResult.RequestId);
            Assert.True(electionResult.Success);
            Assert.True(!string.IsNullOrEmpty(electionResult.ElectionResult));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void ApplyCabinetAppointment_Technocrat_ElevatesStateCapacity_AndReducesPartyPollingSupport()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "Republic_Test_CabinetTechnocrat_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var tempSaveFile = Path.Combine(tempDir, "game.sav");
        var tempStateFile = Path.Combine(tempDir, "state.json");

        try
        {
            PresidentialDeskBridge.CreateOrLoadCountry("Republic of Veridia", tempStateFile, tempSaveFile);
            var initialSnapshot = PresidentialDeskBridge.LoadState(tempStateFile)!;
            var initialSupport = initialSnapshot.PollingSupport;

            var result = PresidentialDeskBridge.ApplyCabinetAppointmentDecision(
                "Finance",
                "technocrat",
                confirmed: true,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);

            Assert.True(result.Success);
            Assert.Equal("Success", result.Result);
            Assert.Equal("Dr. Elena Rostova", result.AppointedMinister);
            Assert.Equal(0.90, result.CivilServiceQuality);
            Assert.Contains("Elena Rostova", result.Message);

            var updatedSnapshot = PresidentialDeskBridge.LoadState(tempStateFile)!;
            Assert.Equal(0.90, updatedSnapshot.CivilServiceQuality);
            Assert.Equal(initialSupport - 3.0, updatedSnapshot.PollingSupport);
            Assert.Equal("Dr. Elena Rostova", updatedSnapshot.FinanceMinister);
            Assert.Contains("Elena Rostova", updatedSnapshot.CabinetSummary);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void ApplyCabinetAppointment_PartyLoyalist_IncreasesPartyPollingSupport_AndCompromisesCapacity()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "Republic_Test_CabinetLoyalist_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var tempSaveFile = Path.Combine(tempDir, "game.sav");
        var tempStateFile = Path.Combine(tempDir, "state.json");

        try
        {
            PresidentialDeskBridge.CreateOrLoadCountry("Republic of Veridia", tempStateFile, tempSaveFile);
            var initialSnapshot = PresidentialDeskBridge.LoadState(tempStateFile)!;
            var initialSupport = initialSnapshot.PollingSupport;

            var result = PresidentialDeskBridge.ApplyCabinetAppointmentDecision(
                "Finance",
                "loyalist",
                confirmed: true,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);

            Assert.True(result.Success);
            Assert.Equal("Success", result.Result);
            Assert.Equal("Senator Donald Vance", result.AppointedMinister);
            Assert.Equal(0.45, result.CivilServiceQuality);
            Assert.Contains("Donald Vance", result.Message);

            var updatedSnapshot = PresidentialDeskBridge.LoadState(tempStateFile)!;
            Assert.Equal(0.45, updatedSnapshot.CivilServiceQuality);
            Assert.Equal(initialSupport + 3.0, updatedSnapshot.PollingSupport);
            Assert.Equal("Senator Donald Vance", updatedSnapshot.FinanceMinister);
            Assert.Contains("Donald Vance", updatedSnapshot.CabinetSummary);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void ApplyCabinetAppointment_Cancelled_LeavesStateAndSupportUnchanged()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "Republic_Test_CabinetCancel_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var tempSaveFile = Path.Combine(tempDir, "game.sav");
        var tempStateFile = Path.Combine(tempDir, "state.json");

        try
        {
            PresidentialDeskBridge.CreateOrLoadCountry("Republic of Veridia", tempStateFile, tempSaveFile);
            var initialSnapshot = PresidentialDeskBridge.LoadState(tempStateFile)!;
            var initialSupport = initialSnapshot.PollingSupport;
            var initialCivilQuality = initialSnapshot.CivilServiceQuality;

            var result = PresidentialDeskBridge.ApplyCabinetAppointmentDecision(
                "Finance",
                "technocrat",
                confirmed: false,
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile);

            Assert.False(result.Success);
            Assert.Equal("Cancelled", result.Result);

            var snapshot = PresidentialDeskBridge.LoadState(tempStateFile)!;
            Assert.Equal(initialSupport, snapshot.PollingSupport);
            Assert.Equal(initialCivilQuality, snapshot.CivilServiceQuality);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void ProcessPendingDecisionRequest_HandlesCabinetAppointmentFileRequest()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "Republic_Test_CabinetFileReq_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var tempSaveFile = Path.Combine(tempDir, "game.sav");
        var tempStateFile = Path.Combine(tempDir, "state.json");
        var requestFile = Path.Combine(tempDir, "presidential_desk_request.json");
        var resultFile = Path.Combine(tempDir, "presidential_desk_result.json");

        try
        {
            PresidentialDeskBridge.CreateOrLoadCountry("Republic of Veridia", tempStateFile, tempSaveFile);

            var request = new
            {
                action = "appoint_minister",
                portfolio = "Interior",
                candidate_type = "technocrat",
                confirmed = true,
                request_id = "req-cab-interior-001"
            };
            File.WriteAllText(requestFile, JsonSerializer.Serialize(request));

            var result = PresidentialDeskBridge.ProcessPendingDecisionRequest(
                stateFilePath: tempStateFile,
                saveFilePath: tempSaveFile,
                requestFilePath: requestFile);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("req-cab-interior-001", result.RequestId);
            Assert.Equal(0.90, result.CorruptionControl);
            Assert.Equal("Dr. Alistair Finch", result.AppointedMinister);
            Assert.False(File.Exists(requestFile));
            Assert.True(File.Exists(resultFile));

            var snapshot = PresidentialDeskBridge.LoadState(tempStateFile)!;
            Assert.Equal(0.90, snapshot.CorruptionControl);
            Assert.Equal("Dr. Alistair Finch", snapshot.InteriorMinister);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }
}

