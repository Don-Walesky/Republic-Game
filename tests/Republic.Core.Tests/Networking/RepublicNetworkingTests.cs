namespace Republic.Core.Tests.Networking;

using System;
using System.Threading.Tasks;
using Republic.Core.Networking;
using Republic.Core.Time;
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
}
