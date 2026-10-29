namespace Republic.Core.Tests.Networking;

using System;
using System.IO;
using System.Threading.Tasks;
using Republic.Core.Client;
using Republic.Core.NationalYield;
using Republic.Core.Networking;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Xunit;

public sealed class MvpMultiplayerSimulationTests
{
    private static readonly DateTimeOffset LaunchEpoch = new(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));

    private static RepublicTime CreateTime(int days, int hours = 0, int minutes = 0)
    {
        var timestamp = LaunchEpoch.AddDays(days).AddHours(hours).AddMinutes(minutes);
        return new RepublicTime(timestamp, LaunchEpoch);
    }

    [Fact]
    public async Task PlayerIdentity_PersistentClientId_AllowsReconnectingClientToReclaimAndActAsOwner()
    {
        using var host = new RepublicHost(port: 0);
        host.Start();

        var hostCountry = new CountrySnapshot
        {
            Id = "country-host",
            Name = "Republic of Arcadia",
            Treasury = "R 10,000,000,000",
            Approval = "70.0%",
            TaxRate = "25.0%"
        };
        host.SetHostCountry(hostCountry);

        const string persistentId = "persistent-player-12345";
        const string countryId = "country-valoria";

        // Step 1: Initial connection with persistent client ID
        using (var initialClient = new RepublicNetworkClient(persistentId))
        {
            await initialClient.ConnectAsync("127.0.0.1", host.Port);
            var countrySnap = new CountrySnapshot
            {
                Id = countryId,
                Name = "Federation of Valoria",
                Treasury = "R 5,000,000,000",
                Approval = "65.0%",
                TaxRate = "20.0%"
            };
            await initialClient.SendCountrySnapshotAsync(countrySnap);

            // Wait a brief moment for host registration
            await Task.Delay(250);
            Assert.True(host.IsOwner(persistentId, countryId));
        }

        // Step 2: Reconnection with the same persistent client ID
        using (var reconnectedClient = new RepublicNetworkClient(persistentId))
        {
            await reconnectedClient.ConnectAsync("127.0.0.1", host.Port);

            // Verify owner is still recognized immediately
            Assert.True(host.IsOwner(persistentId, countryId));

            // Execute an authoritative tax decision as owner
            var decisionTcs = new TaskCompletionSource<NetworkMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
            reconnectedClient.OnDecisionResultReceived += res =>
            {
                if (res.CountryId == countryId)
                {
                    decisionTcs.TrySetResult(res);
                }
            };

            await reconnectedClient.SendTaxDecisionRequestAsync(countryId, proposedRate: 0.28);
            var completed = await Task.WhenAny(decisionTcs.Task, Task.Delay(4000));
            Assert.Same(decisionTcs.Task, completed);

            var result = await decisionTcs.Task;
            Assert.True(result.Success);
            Assert.Equal("Success", result.Result);
            Assert.Equal("28%", host.KnownCountries[countryId].TaxRate);
        }

        // Step 3: Reconnection with a newly generated client ID using SendReconnectAsync
        using (var migratingClient = new RepublicNetworkClient())
        {
            await migratingClient.ConnectAsync("127.0.0.1", host.Port);

            var reconnectTcs = new TaskCompletionSource<NetworkMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
            migratingClient.OnDecisionResultReceived += res =>
            {
                if (res.CountryId == countryId)
                {
                    reconnectTcs.TrySetResult(res);
                }
            };

            await migratingClient.SendReconnectAsync(countryId, previousClientId: persistentId);
            var completed = await Task.WhenAny(reconnectTcs.Task, Task.Delay(4000));
            Assert.Same(reconnectTcs.Task, completed);

            var reconnectResult = await reconnectTcs.Task;
            Assert.True(reconnectResult.Success);
            Assert.True(host.IsOwner(migratingClient.ClientId, countryId));
            Assert.False(host.IsOwner(persistentId, countryId));
        }
    }

    [Fact]
    public async Task Host_NeverTrustsClientDeclaredTreasury_EnforcesAuthoritativeBalance()
    {
        using var host = new RepublicHost(port: 0);
        host.Start();

        var hostCountry = new CountrySnapshot
        {
            Id = "country-host",
            Name = "Republic of Arcadia",
            Treasury = "R 10,000,000,000",
            Approval = "70.0%",
            TaxRate = "25.0%"
        };
        host.SetHostCountry(hostCountry);

        using var client = new RepublicNetworkClient();
        await client.ConnectAsync("127.0.0.1", host.Port);

        // Step 1: Client joins with legitimate R 1,000,000
        var legitimateCountry = new CountrySnapshot
        {
            Id = "country-solaria",
            Name = "Republic of Solaria",
            Treasury = "R 1,000,000",
            Approval = "60.0%",
            TaxRate = "20.0%"
        };
        var initialJoinTcs = new TaskCompletionSource<CountrySnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnInitialCountryUpdated(CountrySnapshot c)
        {
            if (c.Id == "country-solaria")
            {
                initialJoinTcs.TrySetResult(c);
            }
        }
        host.OnCountryUpdated += OnInitialCountryUpdated;

        await client.SendCountrySnapshotAsync(legitimateCountry);
        await Task.WhenAny(initialJoinTcs.Task, Task.Delay(2000));
        host.OnCountryUpdated -= OnInitialCountryUpdated;

        var authTreasury = host.GetOrCreateTreasury("country-solaria");
        Assert.Equal(1_000_000.0, authTreasury.Balance);
        Assert.Equal("R 1,000,000", host.KnownCountries["country-solaria"].Treasury);

        // Step 2: Malicious or untrusted client attempts to spoof its treasury balance to 999 billion
        var spoofedCountry = legitimateCountry with
        {
            Treasury = "R 999,999,999,999",
            Approval = "99.9%"
        };

        var hostUpdatedTcs = new TaskCompletionSource<CountrySnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        host.OnCountryUpdated += c =>
        {
            if (c.Id == "country-solaria")
            {
                hostUpdatedTcs.TrySetResult(c);
            }
        };

        await client.SendCountrySnapshotAsync(spoofedCountry);
        var completed = await Task.WhenAny(hostUpdatedTcs.Task, Task.Delay(4000));
        Assert.Same(hostUpdatedTcs.Task, completed);

        // Assert: Host sanitized the snapshot with its authoritative balance (R 1,000,000), rejecting spoof
        var sanitizedSnapshot = await hostUpdatedTcs.Task;
        Assert.Equal("R 1,000,000", sanitizedSnapshot.Treasury);
        Assert.Equal("R 1,000,000", host.KnownCountries["country-solaria"].Treasury);
        Assert.Equal(1_000_000.0, authTreasury.Balance);
    }

    [Fact]
    public void TradeProposal_FullLifecycle_DistinguishesStages_AndSettlesAuthoritatively()
    {
        // 1. Verify all 5 Lifecycle stages and human-readable names
        var draft = new TradeProposal("A", "B", 100, status: "Draft");
        Assert.Equal("Draft Proposal", draft.LifecycleStage);

        var pending = new TradeProposal("A", "B", 100, status: "Pending");
        Assert.Equal("Pending Proposal", pending.LifecycleStage);

        var accepted = new TradeProposal("A", "B", 100, status: "Accepted");
        Assert.Equal("Accepted Agreement", accepted.LifecycleStage);

        var completed = new TradeProposal("A", "B", 100, status: "Completed");
        Assert.Equal("Completed Transaction", completed.LifecycleStage);

        var rejected = new TradeProposal("A", "B", 100, status: "Rejected");
        Assert.Equal("Rejected Proposal", rejected.LifecycleStage);

        // 2. Programmatic lifecycle transition from Draft -> Pending -> Accepted -> Completed
        var liveProposal = new TradeProposal("A", "B", 500, status: "Draft");
        Assert.Equal("Draft Proposal", liveProposal.LifecycleStage);

        liveProposal.Status = TradeProposalStatus.Pending.ToString();
        Assert.Equal("Pending Proposal", liveProposal.LifecycleStage);

        var simTime = CreateTime(1, 0, 0);
        var countryA = Country.Found("Country A", simTime);
        var countryB = Country.Found("Country B", simTime);

        countryA.Treasury.Withdraw(countryA.Treasury.Balance);
        countryA.Treasury.Deposit(2_000.0);

        countryB.Treasury.Withdraw(countryB.Treasury.Balance);
        countryB.Treasury.Deposit(1_000.0);

        var acceptSuccess = liveProposal.Accept(countryA, countryB);
        Assert.True(acceptSuccess);
        Assert.Equal("Accepted Agreement", liveProposal.LifecycleStage);
        Assert.Equal(1_500.0, countryA.Treasury.Balance);
        Assert.Equal(1_500.0, countryB.Treasury.Balance);

        liveProposal.Complete();
        Assert.Equal("Completed Transaction", liveProposal.LifecycleStage);
    }

    [Fact]
    public void OfflineProgression_GeneratesBriefing_PersistsAccuratelyOnCountryAndSnapshot()
    {
        var epoch = CreateTime(0, 0, 0);
        var country = Country.Found("Republic of Arcadia", epoch);

        // Initial state before offline time passes
        Assert.Null(country.LastOfflineCatchUpSummary);

        var initialSnapshot = PresidentialDeskSnapshot.Create(country, epoch);
        Assert.Contains("Real-time simulation aligned", initialSnapshot.OfflineBriefingSummary);

        // Advance simulation time by 4 days (multiple offline yield cycles)
        var advancedTime = CreateTime(4, 0, 0);
        var catchUpService = new OfflineYieldCatchUpService();
        var briefing = catchUpService.CatchUp(country, advancedTime);

        // Assert 1: Offline catch-up report is generated accurately
        Assert.True(briefing.BoundariesCredited > 0);
        Assert.NotNull(country.LastOfflineCatchUpSummary);
        Assert.Contains("Offline Catch-Up", country.LastOfflineCatchUpSummary);
        Assert.Contains($"{briefing.BoundariesCredited} production cycles credited", country.LastOfflineCatchUpSummary);

        // Assert 2: PresidentialDeskSnapshot renders the offline briefing summary clearly
        var updatedSnapshot = PresidentialDeskSnapshot.Create(country, advancedTime);
        Assert.Equal(country.LastOfflineCatchUpSummary, updatedSnapshot.OfflineBriefingSummary);
        Assert.Contains("Offline Catch-Up", updatedSnapshot.OfflineBriefingSummary);
    }

    [Fact]
    public void MilitaryReadiness_DefconLevel_ReportedAccuratelyInSnapshot()
    {
        var simTime = CreateTime(1, 12, 0);
        var country = Country.Found("Republic of Arcadia", simTime);

        var snapshot = PresidentialDeskSnapshot.Create(country, simTime);

        // Verify national defense readiness posture and DEFCON monitoring
        Assert.NotNull(snapshot.DefconLevel);
        Assert.Contains("DEFCON 5", snapshot.DefconLevel);
        Assert.Contains("Peacetime", snapshot.DefconLevel);

        Assert.NotNull(snapshot.MilitaryReadiness);
        Assert.Contains("Armed Forces", snapshot.MilitaryReadiness);
    }
}
