namespace Republic.Core.Tests.World;

using System.Text.Json;
using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Events;
using Republic.Core.World.Models;
using Republic.Core.World.Services;
using Xunit;

public sealed class CountryFoundingTests
{
    private readonly EventBus _eventBus = new(new EventBusOptions(), new TestLogger());
    private readonly TestLogger _logger = new();

    [Fact]
    public void Test1_CountryCreation_RecordsCurrentRepublicTime()
    {
        var clock = RepublicClock.CreateControlled();
        // Advance clock to Day 42, 14:37 WAT
        clock.AdvanceDays(42);
        clock.AdvanceHours(14);
        clock.AdvanceMinutes(37);

        var countryService = new CountryService(_eventBus, _logger, clock);
        var country = countryService.FoundCountry("Federal Republic of Arcadia");

        Assert.True(country.IsFounded);
        Assert.Equal(clock.CurrentTime, country.FoundingTime);
        Assert.Equal(42, country.FoundingRepublicDay);
        Assert.Equal(CountryFoundingStatus.NewlyFounded, country.FoundingStatus);
    }

    [Fact]
    public void Test2_FoundingDateAndTime_AreCorrect()
    {
        var clock = RepublicClock.CreateControlled();
        // Launch is 2027-02-10 00:00 WAT. Advance 10 days and 9 hours -> 2027-02-20 09:00 WAT
        clock.AdvanceDays(10);
        clock.AdvanceHours(9);

        var country = Country.Found("Republic of Solaria", clock.CurrentTime);

        Assert.Equal(new DateOnly(2027, 2, 20), country.IndependenceDay);
        Assert.Equal(new TimeOnly(9, 0, 0), country.IndependenceTime);
        Assert.Equal(10, country.FoundingRepublicDay);
        Assert.Equal(TimeSpan.FromHours(1), country.FoundingTime.WatTimestamp.Offset);
        Assert.Equal("Republic of Solaria", country.Name);
    }

    [Fact]
    public void Test3_CountryAge_StartsAtZero()
    {
        var clock = RepublicClock.CreateControlled();
        clock.AdvanceDays(5);
        clock.AdvanceHours(12);

        var country = Country.Found("Valeria", clock.CurrentTime);

        var initialAge = country.GetAge(clock);
        Assert.Equal(TimeSpan.Zero, initialAge);

        // Also test with explicit RepublicTime argument
        Assert.Equal(TimeSpan.Zero, country.GetAge(clock.CurrentTime));
    }

    [Fact]
    public void Test4_CountryAge_AdvancesCorrectly()
    {
        var clock = RepublicClock.CreateControlled();
        // Founder time: Day 10, 2027-02-20 09:00 WAT
        clock.AdvanceDays(10);
        clock.AdvanceHours(9);

        var country = Country.Found("Arcadia", clock.CurrentTime);
        Assert.Equal(TimeSpan.Zero, country.GetAge(clock));

        // Advance clock by 2 days and 6 hours -> Day 12, 15:00 WAT
        clock.AdvanceDays(2);
        clock.AdvanceHours(6);

        var calculatedAge = country.GetAge(clock);
        Assert.Equal(new TimeSpan(2, 6, 0, 0), calculatedAge);
        Assert.Equal(2, calculatedAge.Days);
        Assert.Equal(6, calculatedAge.Hours);
        Assert.Equal(54.0, calculatedAge.TotalHours);
    }

    [Fact]
    public void Test5_TwoCountries_CanHaveDifferentFoundingDates_InSameGlobalClock()
    {
        var clock = RepublicClock.CreateControlled();
        var countryService = new CountryService(_eventBus, _logger, clock);

        // Create Country A on Day 0, 00:00 WAT
        var countryA = countryService.FoundCountry("Ancient Republic");

        // Advance global clock by 15 days
        clock.AdvanceDays(15);
        clock.AdvanceHours(4);

        // Create Country B on Day 15, 04:00 WAT
        var countryB = countryService.FoundCountry("Young Republic");

        // Verify different founding moments
        Assert.NotEqual(countryA.FoundingTime, countryB.FoundingTime);
        Assert.Equal(0, countryA.FoundingRepublicDay);
        Assert.Equal(15, countryB.FoundingRepublicDay);

        // Country A is strictly older than Country B
        var ageA = countryA.GetAge(clock);
        var ageB = countryB.GetAge(clock);

        Assert.True(ageA > ageB);
        Assert.Equal(TimeSpan.FromDays(15) + TimeSpan.FromHours(4), ageA);
        Assert.Equal(TimeSpan.Zero, ageB);

        // Advance further by 5 days
        clock.AdvanceDays(5);
        Assert.Equal(TimeSpan.FromDays(20) + TimeSpan.FromHours(4), countryA.GetAge(clock));
        Assert.Equal(TimeSpan.FromDays(5), countryB.GetAge(clock));
    }

    [Fact]
    public void Test6_FoundingTime_CannotBeCasuallyChanged()
    {
        var clock = RepublicClock.CreateControlled();
        var country = Country.Found("Protected Nation", clock.CurrentTime);

        // Verify country is marked as founded
        Assert.True(country.IsFounded);

        // Attempting to overwrite established founding must throw InvalidOperationException
        var laterTime = clock.CurrentTime.Add(TimeSpan.FromDays(10));
        var ex = Assert.Throws<InvalidOperationException>(() => country.EstablishFounding(laterTime));
        Assert.Contains("already been authoritatively established", ex.Message);

        // Verify original founding time was preserved unchanged
        Assert.Equal(clock.CurrentTime, country.FoundingTime);
    }

    [Fact]
    public void Test7_TimezoneCorrectness_MaintainsWATUtcPlusOne()
    {
        var clock = RepublicClock.CreateControlled();
        clock.AdvanceDays(20);
        clock.AdvanceHours(18);
        clock.AdvanceMinutes(30);

        var country = Country.Found("Equatorial Union", clock.CurrentTime);

        // Must always be fixed at WAT (+01:00)
        Assert.Equal(RepublicTime.WatOffset, country.FoundingTime.WatTimestamp.Offset);
        Assert.Equal(TimeSpan.FromHours(1), country.FoundingTime.WatTimestamp.Offset);

        // Check UTC conversion consistency
        Assert.Equal(
            country.FoundingTime.WatTimestamp.UtcDateTime,
            country.FoundingTime.UtcTimestamp.UtcDateTime);

        // When WAT is 18:30, UTC is 17:30
        Assert.Equal(18, country.IndependenceTime.Hour);
        Assert.Equal(17, country.FoundingTime.UtcTimestamp.Hour);
    }

    [Fact]
    public void FoundingStatus_TransitionsFromNewlyFoundedToEstablished()
    {
        var clock = RepublicClock.CreateControlled();
        var country = Country.Found("Novus State", clock.CurrentTime);

        Assert.Equal(CountryFoundingStatus.NewlyFounded, country.FoundingStatus);
        Assert.False(country.IsEstablished);

        country.MarkEstablished();

        Assert.Equal(CountryFoundingStatus.Established, country.FoundingStatus);
        Assert.True(country.IsEstablished);
    }

    [Fact]
    public async Task CountryService_RegisterCountry_PublishesCountryCreatedEvent_WithFoundingTime()
    {
        CountryCreatedEvent? capturedEvent = null;
        _eventBus.Subscribe<CountryCreatedEvent>((evt, _) =>
        {
            capturedEvent = evt;
            return ValueTask.CompletedTask;
        });

        var clock = RepublicClock.CreateControlled();
        clock.AdvanceDays(3);
        clock.AdvanceHours(10);

        var service = new CountryService(_eventBus, _logger, clock);
        var country = service.FoundCountry("Republic of Maridia");
        await _eventBus.ProcessQueuedEventsAsync();

        Assert.NotNull(capturedEvent);
        Assert.Equal(country.Id, capturedEvent.Country.Id);
        Assert.Equal(country.FoundingTime.WatTimestamp, capturedEvent.OccurredAt);
    }

    [Fact]
    public void FullPersistence_SerializesAndRestoresCountry_PreservingFoundingState()
    {
        var clock = RepublicClock.CreateControlled();
        clock.AdvanceDays(12);
        clock.AdvanceHours(8);

        var originalCountry = Country.Found(
            name: "Persistent Republic",
            foundingTime: clock.CurrentTime,
            id: "country-persist-1",
            capitalCity: "Aethelgard",
            governmentType: "Constitutional Republic");
        originalCountry.MarkEstablished();

        // Serialize to JSON
        var json = JsonSerializer.Serialize(originalCountry);
        var restored = JsonSerializer.Deserialize<Country>(json);

        Assert.NotNull(restored);
        Assert.Equal(originalCountry.Id, restored.Id);
        Assert.Equal(originalCountry.Name, restored.Name);
        Assert.Equal(originalCountry.FoundingTime, restored.FoundingTime);
        Assert.Equal(originalCountry.IndependenceDay, restored.IndependenceDay);
        Assert.Equal(originalCountry.IndependenceTime, restored.IndependenceTime);
        Assert.Equal(originalCountry.FoundingRepublicDay, restored.FoundingRepublicDay);
        Assert.Equal(CountryFoundingStatus.Established, restored.FoundingStatus);
        Assert.True(restored.IsFounded);

        // Advance clock and verify restored country calculates correct age
        clock.AdvanceDays(3);
        Assert.Equal(TimeSpan.FromDays(3), restored.GetAge(clock));
    }

    [Fact]
    public void WorldManager_SnapshotsAndRestoresCountries()
    {
        var clock = RepublicClock.CreateControlled();
        clock.AdvanceDays(5);

        var worldManager = new WorldManager(_eventBus, _logger, clock);
        worldManager.Countries.FoundCountry("Republic One");
        worldManager.Countries.FoundCountry("Republic Two");

        var snapshot = worldManager.Snapshot();
        Assert.Equal(2, snapshot.Countries.Count);

        var newWorldManager = new WorldManager(_eventBus, _logger, clock);
        newWorldManager.Restore(snapshot);

        var restoredCountries = newWorldManager.Countries.GetAllCountries();
        Assert.Equal(2, restoredCountries.Count);
        Assert.Contains(restoredCountries, c => c.Name == "Republic One");
        Assert.Contains(restoredCountries, c => c.Name == "Republic Two");
    }
}
