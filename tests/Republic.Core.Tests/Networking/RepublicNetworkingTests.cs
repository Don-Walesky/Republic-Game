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
}
