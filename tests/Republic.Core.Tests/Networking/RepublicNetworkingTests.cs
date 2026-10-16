namespace Republic.Core.Tests.Networking;

using System;
using System.IO;
using System.Threading.Tasks;
using Republic.Core.Networking;
using Republic.Core.Time;
using Republic.Core.World;
using Xunit;

public sealed class RepublicNetworkingTests
{
    private static readonly DateTimeOffset LaunchEpoch = new(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));

    private static RepublicTime CreateTime(int days, int hours = 0, int minutes = 0)
    {
        var timestamp = LaunchEpoch.AddDays(days).AddHours(hours).AddMinutes(minutes);
        return new RepublicTime(timestamp, LaunchEpoch);
    }

    [Fact]
    public async Task Host_AcceptsClient_BothSeeSameClockTickAndCountrySnapshot()
    {
        // Arrange: Start host on an ephemeral port (0)
        using var host = new RepublicHost(port: 0);
        host.Start();
        Assert.True(host.Port > 0);

        var simTime = CreateTime(3, 14, 30);
        host.UpdateClock(simTime, tick: 120);

        var hostCountry = new CountrySnapshot
        {
            Id = "player-host",
            Name = "Republic of Arcadia",
            Treasury = "R 15,000,000,000",
            Approval = "70.0%",
            TaxRate = "25.0%"
        };
        host.SetHostCountry(hostCountry);

        var clientClockTcs = new TaskCompletionSource<NetworkClockSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        var clientCountryTcs = new TaskCompletionSource<CountrySnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        var hostCountryTcs = new TaskCompletionSource<CountrySnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);

        host.OnCountryUpdated += country =>
        {
            if (country.Id == "player-client")
            {
                hostCountryTcs.TrySetResult(country);
            }
        };

        // Act: Connect client to host
        using var client = new RepublicNetworkClient();
        client.OnClockSynchronized += clock => clientClockTcs.TrySetResult(clock);
        client.OnCountryReceived += country =>
        {
            if (country.Id == "player-host")
            {
                clientCountryTcs.TrySetResult(country);
            }
        };

        await client.ConnectAsync("127.0.0.1", host.Port);

        // Assert 1: Client receives the host's clock and both see the identical clock tick
        var clockCompleted = await Task.WhenAny(clientClockTcs.Task, Task.Delay(4000));
        Assert.Same(clientClockTcs.Task, clockCompleted);

        var clientClock = await clientClockTcs.Task;
        Assert.NotNull(clientClock);
        Assert.Equal(120UL, clientClock.Tick);
        Assert.Equal(host.CurrentClock.Tick, clientClock.Tick);
        Assert.Equal(simTime.DayNumber, clientClock.Day);
        Assert.Equal(simTime.ToString(), clientClock.TimeString);

        // Assert 2: Client receives the host's country snapshot
        var countryCompleted = await Task.WhenAny(clientCountryTcs.Task, Task.Delay(4000));
        Assert.Same(clientCountryTcs.Task, countryCompleted);

        var receivedHostCountry = await clientCountryTcs.Task;
        Assert.NotNull(receivedHostCountry);
        Assert.Equal("Republic of Arcadia", receivedHostCountry.Name);
        Assert.Equal("R 15,000,000,000", receivedHostCountry.Treasury);
        Assert.Equal("70.0%", receivedHostCountry.Approval);

        // Act 2: Client sends its own country snapshot to host
        var clientCountry = new CountrySnapshot
        {
            Id = "player-client",
            Name = "Federation of Norse",
            Treasury = "R 12,000,000,000",
            Approval = "65.0%",
            TaxRate = "28.0%"
        };
        await client.SendCountrySnapshotAsync(clientCountry);

        // Assert 3: Host receives the client's country snapshot
        var hostCountryCompleted = await Task.WhenAny(hostCountryTcs.Task, Task.Delay(4000));
        Assert.Same(hostCountryTcs.Task, hostCountryCompleted);

        var receivedClientCountry = await hostCountryTcs.Task;
        Assert.NotNull(receivedClientCountry);
        Assert.Equal("Federation of Norse", receivedClientCountry.Name);
        Assert.Equal("R 12,000,000,000", receivedClientCountry.Treasury);
        Assert.Equal("65.0%", receivedClientCountry.Approval);

        // Assert 4: Update clock on host to tick 121 and verify client receives updated tick
        var tick121Tcs = new TaskCompletionSource<NetworkClockSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.OnClockSynchronized += clock =>
        {
            if (clock.Tick == 121UL)
            {
                tick121Tcs.TrySetResult(clock);
            }
        };

        var nextSimTime = CreateTime(3, 14, 31);
        host.UpdateClock(nextSimTime, tick: 121);

        var tick121Completed = await Task.WhenAny(tick121Tcs.Task, Task.Delay(4000));
        Assert.Same(tick121Tcs.Task, tick121Completed);

        var updatedClientClock = await tick121Tcs.Task;
        Assert.Equal(121UL, updatedClientClock.Tick);
        Assert.Equal(host.CurrentClock.Tick, updatedClientClock.Tick);
    }

    [Fact]
    public void TradeProposal_Accepted_BothTreasuriesUpdateCorrectly_UnderAuthoritativeRules()
    {
        // Arrange: Two sovereign country models with initial treasury balances
        var simTime = CreateTime(1, 0, 0);
        var countryA = Republic.Core.World.Models.Country.Found("Republic of Arcadia", simTime);
        var countryB = Republic.Core.World.Models.Country.Found("Federation of Valoria", simTime);

        // Explicit initial balances
        countryA.Treasury.Withdraw(countryA.Treasury.Balance);
        countryA.Treasury.Deposit(10_000_000_000.0);

        countryB.Treasury.Withdraw(countryB.Treasury.Balance);
        countryB.Treasury.Deposit(5_000_000_000.0);

        Assert.Equal(10_000_000_000.0, countryA.Treasury.Balance);
        Assert.Equal(5_000_000_000.0, countryB.Treasury.Balance);

        // Act 1: Propose 2,000,000,000 REPU trade from Country A to Country B
        var proposal = new Republic.Core.World.TradeProposal(countryA.Id, countryB.Id, 2_000_000_000.0);
        Assert.Equal("Pending", proposal.Status);

        var accepted = proposal.Accept(countryA, countryB);

        // Assert 1: Trade is accepted and both treasuries update under authoritative simulation rules
        Assert.True(accepted);
        Assert.Equal("Accepted", proposal.Status);
        Assert.Equal(8_000_000_000.0, countryA.Treasury.Balance);
        Assert.Equal(7_000_000_000.0, countryB.Treasury.Balance);

        // Act 2: Overdraft attempt (propose more than available balance)
        var excessiveProposal = new Republic.Core.World.TradeProposal(countryA.Id, countryB.Id, 50_000_000_000.0);
        var excessiveAccepted = excessiveProposal.Accept(countryA, countryB);

        // Assert 2: Overdraft rejected, balances unchanged
        Assert.False(excessiveAccepted);
        Assert.Equal("Pending", excessiveProposal.Status);
        Assert.Equal(8_000_000_000.0, countryA.Treasury.Balance);
        Assert.Equal(7_000_000_000.0, countryB.Treasury.Balance);
    }

    [Fact]
    public async Task HostAndClient_BilateralDiplomacy_Message_AndTradeSync()
    {
        // Arrange: Start Host with Country A
        using var host = new RepublicHost(port: 0);
        host.Start();

        var simTime = CreateTime(2, 10, 0);
        host.UpdateClock(simTime, tick: 50);

        var hostCountry = new CountrySnapshot
        {
            Id = "country-host",
            Name = "Republic of Arcadia",
            Treasury = "R 10,000,000,000",
            Approval = "70.0%",
            TaxRate = "25.0%"
        };
        host.SetHostCountry(hostCountry);
        var hostTreasury = host.GetOrCreateTreasury(hostCountry.Id, hostCountry);

        using var client = new RepublicNetworkClient();
        var clientCountry = new CountrySnapshot
        {
            Id = "country-client",
            Name = "Federation of Valoria",
            Treasury = "R 5,000,000,000",
            Approval = "65.0%",
            TaxRate = "20.0%"
        };

        var clientDiplomacyTcs = new TaskCompletionSource<DiplomaticSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        var clientMessageTcs = new TaskCompletionSource<Republic.Core.World.DiplomaticMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var clientTradeTcs = new TaskCompletionSource<Republic.Core.World.TradeProposal>(TaskCreationOptions.RunContinuationsAsynchronously);
        var clientHostCountryUpdatedTcs = new TaskCompletionSource<CountrySnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);

        client.OnDiplomacyReceived += dip => clientDiplomacyTcs.TrySetResult(dip);
        client.OnMessageReceived += msg => clientMessageTcs.TrySetResult(msg);
        client.OnTradeReceived += tr =>
        {
            if (tr.Status == "Accepted")
            {
                clientTradeTcs.TrySetResult(tr);
            }
        };
        client.OnCountryReceived += c =>
        {
            if (c.Id == "country-host" && c.Treasury != "R 10,000,000,000")
            {
                clientHostCountryUpdatedTcs.TrySetResult(c);
            }
        };

        await client.ConnectAsync("127.0.0.1", host.Port);
        await client.SendCountrySnapshotAsync(clientCountry);

        // Act 1: Recognition status sync
        var recognition = new DiplomaticSnapshot("country-client", "country-host", "Recognized");
        await client.SendDiplomacyAsync(recognition);

        // Host broadcasts recognition
        host.BroadcastDiplomacy(recognition);
        var dipCompleted = await Task.WhenAny(clientDiplomacyTcs.Task, Task.Delay(4000));
        Assert.Same(clientDiplomacyTcs.Task, dipCompleted);
        var receivedDip = await clientDiplomacyTcs.Task;
        Assert.Equal("Recognized", receivedDip.Status);
        Assert.True(receivedDip.IsRecognized);

        // Act 2: Send Message
        var message = new Republic.Core.World.DiplomaticMessage("country-client", "country-host", "Greetings from Valoria!");
        host.BroadcastDiplomaticMessage(message);
        var msgCompleted = await Task.WhenAny(clientMessageTcs.Task, Task.Delay(4000));
        Assert.Same(clientMessageTcs.Task, msgCompleted);
        var receivedMsg = await clientMessageTcs.Task;
        Assert.Equal("Greetings from Valoria!", receivedMsg.Text);
        Assert.Contains(host.World.Messages, m => m.Text == "Greetings from Valoria!");

        // Act 3: Trade proposal and acceptance
        var trade = new Republic.Core.World.TradeProposal("country-client", "country-host", 1_000_000_000.0, "REPU", "Accepted");
        // Sending trade with Accepted status triggers host execution
        var clientTreasury = host.GetOrCreateTreasury(clientCountry.Id, clientCountry);
        trade.Accept(clientTreasury, hostTreasury);

        host.BroadcastTradeProposal(trade);

        var tradeCompleted = await Task.WhenAny(clientTradeTcs.Task, Task.Delay(4000));
        Assert.Same(clientTradeTcs.Task, tradeCompleted);
        var receivedTrade = await clientTradeTcs.Task;
        Assert.Equal("Accepted", receivedTrade.Status);

        Assert.Equal(4_000_000_000.0, clientTreasury.Balance);
        Assert.Equal(11_000_000_000.0, hostTreasury.Balance);
    }

    [Fact]
    public async Task Host_TwoClients_ConnectDisconnectReconnect_HandlesPeerCountsAndSync()
    {
        // Arrange: Start host on dynamic port
        using var host = new RepublicHost(port: 0);
        host.Start();

        var hostCountry = new CountrySnapshot
        {
            Id = "host-country",
            Name = "Republic of Arcadia",
            Treasury = "R 10,000,000,000",
            Approval = "70.0%",
            TaxRate = "25.0%"
        };
        host.SetHostCountry(hostCountry);

        var countTcs2 = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var countTcs1 = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var countTcsReconnected = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        int currentCount = 0;
        host.OnClientCountChanged += c =>
        {
            currentCount = c;
            if (c == 2 && !countTcs2.Task.IsCompleted)
            {
                countTcs2.TrySetResult(c);
            }
            else if (c == 1 && countTcs2.Task.IsCompleted && !countTcs1.Task.IsCompleted)
            {
                countTcs1.TrySetResult(c);
            }
            else if (c == 2 && countTcs1.Task.IsCompleted && !countTcsReconnected.Task.IsCompleted)
            {
                countTcsReconnected.TrySetResult(c);
            }
        };

        // Act 1: Connect Client 1 and Client 2
        var client1 = new RepublicNetworkClient();
        var client2 = new RepublicNetworkClient();

        await client1.ConnectAsync("127.0.0.1", host.Port);
        await client2.ConnectAsync("127.0.0.1", host.Port);

        var completed2 = await Task.WhenAny(countTcs2.Task, Task.Delay(4000));
        Assert.Same(countTcs2.Task, completed2);
        Assert.Equal(2, await countTcs2.Task);

        // Act 2: Disconnect Client 1
        client1.Dispose();

        var completed1 = await Task.WhenAny(countTcs1.Task, Task.Delay(4000));
        Assert.Same(countTcs1.Task, completed1);
        Assert.Equal(1, await countTcs1.Task);

        // Act 3: Reconnect Client 1 (new socket)
        using var client1Reconnected = new RepublicNetworkClient();
        await client1Reconnected.ConnectAsync("127.0.0.1", host.Port);

        var completedReconnected = await Task.WhenAny(countTcsReconnected.Task, Task.Delay(4000));
        Assert.Same(countTcsReconnected.Task, completedReconnected);
        Assert.Equal(2, await countTcsReconnected.Task);

        client2.Dispose();
    }

    [Fact]
    public async Task ClientRequest_RejectedIfNotOwner_AndAcceptedTrade_UpdatesTreasuriesExactlyOnce()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"republic_test_world_{Guid.NewGuid():N}.json");
        try
        {
            // 1. Arrange: Start host with single-file persistence
            using var host = new RepublicHost(port: 0, saveFilePath: tempFile);
            host.Start();

            // Set up host initial country & treasury
            var hostCountry = new CountrySnapshot
            {
                Id = "country-arcadia",
                Name = "Republic of Arcadia",
                Treasury = "R 10,000,000,000",
                Approval = "70.0%",
                TaxRate = "25.0%"
            };
            host.SetHostCountry(hostCountry);
            var hostTreasury = host.GetOrCreateTreasury(hostCountry.Id, hostCountry);

            // Connect Client 1 (representing owner of Country 1 / Valoria)
            using var client1 = new RepublicNetworkClient();
            await client1.ConnectAsync("127.0.0.1", host.Port);

            var valoriaCountry = new CountrySnapshot
            {
                Id = "country-valoria",
                Name = "Federation of Valoria",
                Treasury = "R 5,000,000,000",
                Approval = "65.0%",
                TaxRate = "20.0%"
            };
            await client1.SendCountrySnapshotAsync(valoriaCountry);

            // Connect Client 2 (representing untrusted intruder or owner of Country 2 / Norse)
            using var client2 = new RepublicNetworkClient();
            await client2.ConnectAsync("127.0.0.1", host.Port);

            var norseCountry = new CountrySnapshot
            {
                Id = "country-norse",
                Name = "Commonwealth of Norse",
                Treasury = "R 8,000,000,000",
                Approval = "60.0%",
                TaxRate = "22.0%"
            };
            await client2.SendCountrySnapshotAsync(norseCountry);

            // 2. Part A: Client Request Rejected If Not Owner
            // Client 2 attempts to change tax rate on Valoria (owned by Client 1)
            var client2RejectionTcs = new TaskCompletionSource<NetworkMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
            client2.OnDecisionResultReceived += res =>
            {
                if (res.CountryId == "country-valoria")
                {
                    client2RejectionTcs.TrySetResult(res);
                }
            };

            await client2.SendTaxDecisionRequestAsync("country-valoria", proposedRate: 0.50);

            var rejectCompleted = await Task.WhenAny(client2RejectionTcs.Task, Task.Delay(4000));
            Assert.Same(client2RejectionTcs.Task, rejectCompleted);

            var rejectionMsg = await client2RejectionTcs.Task;
            Assert.False(rejectionMsg.Success);
            Assert.Equal("Rejected", rejectionMsg.Result);

            // Verify Valoria's tax rate on host was NOT altered
            var valoriaOnHost = host.KnownCountries["country-valoria"];
            Assert.Equal("20.0%", valoriaOnHost.TaxRate);

            // Also test direct host API verification for ownership rejection
            var unauthorizedTax = host.SubmitTaxDecision(playerId: client2.ClientId, countryId: "country-valoria", proposedRate: 0.45);
            Assert.False(unauthorizedTax);

            // 3. Part B: Trade Validation & Exactly-Once Treasury Update
            // Client 1 (Valoria) proposes a trade of 1,000,000,000 REPU to Norse (owned by Client 2)
            var tradeId = "trade-" + Guid.NewGuid().ToString("N");
            var proposal = new TradeProposal("country-valoria", "country-norse", 1_000_000_000.0)
            {
                Id = tradeId
            };

            var proposed = host.SubmitTradeProposal(client1.ClientId, proposal);
            Assert.True(proposed);

            var valoriaTreasury = host.GetOrCreateTreasury("country-valoria");
            var norseTreasury = host.GetOrCreateTreasury("country-norse");
            var initialValoriaBal = valoriaTreasury.Balance;
            var initialNorseBal = norseTreasury.Balance;

            // Step B1: Unauthorized accept attempt (Client 1 tries to accept its own proposal)
            var unauthorizedAccept = host.AcceptTrade(playerId: client1.ClientId, tradeId: tradeId);
            Assert.False(unauthorizedAccept);
            Assert.Equal(initialValoriaBal, valoriaTreasury.Balance);
            Assert.Equal(initialNorseBal, norseTreasury.Balance);

            // Step B2: Authorized accept by Client 2 (the recipient)
            var accepted = host.AcceptTrade(playerId: client2.ClientId, tradeId: tradeId);
            Assert.True(accepted);

            // Verify both treasuries updated
            Assert.Equal(initialValoriaBal - 1_000_000_000.0, valoriaTreasury.Balance);
            Assert.Equal(initialNorseBal + 1_000_000_000.0, norseTreasury.Balance);

            // Step B3: Duplicate accept attempt (cannot be applied twice / exactly once)
            var duplicateAccept = host.AcceptTrade(playerId: client2.ClientId, tradeId: tradeId);
            Assert.False(duplicateAccept);

            // Treasuries must remain unchanged after duplicate accept attempt
            Assert.Equal(initialValoriaBal - 1_000_000_000.0, valoriaTreasury.Balance);
            Assert.Equal(initialNorseBal + 1_000_000_000.0, norseTreasury.Balance);

            // 4. Part C: Persistence check - restart host and ensure state survives
            host.SaveWorld();
            host.Dispose();

            using var restartedHost = new RepublicHost(port: 0, saveFilePath: tempFile);
            restartedHost.Start();

            // Verify countries, ownership, treasuries, and trade status were restored
            Assert.True(restartedHost.KnownCountries.ContainsKey("country-valoria"));
            Assert.True(restartedHost.KnownCountries.ContainsKey("country-norse"));
            Assert.Equal(client1.ClientId, restartedHost.GetCountryOwner("country-valoria"));
            Assert.Equal(client2.ClientId, restartedHost.GetCountryOwner("country-norse"));

            var reloadedValoriaTreasury = restartedHost.GetOrCreateTreasury("country-valoria");
            var reloadedNorseTreasury = restartedHost.GetOrCreateTreasury("country-norse");
            Assert.Equal(initialValoriaBal - 1_000_000_000.0, reloadedValoriaTreasury.Balance);
            Assert.Equal(initialNorseBal + 1_000_000_000.0, reloadedNorseTreasury.Balance);

            Assert.True(restartedHost.KnownTrades.TryGetValue(tradeId, out var reloadedTrade));
            Assert.Equal("Accepted", reloadedTrade.Status);

            // Duplicate accept on restarted host is also rejected!
            Assert.False(restartedHost.AcceptTrade(client2.ClientId, tradeId));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                try { File.Delete(tempFile); } catch { }
            }
        }
    }
}

