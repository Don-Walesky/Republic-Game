global using BillStatus = Republic.Core.Politics.CountryBillStatus;

namespace Republic.Core.Tests.Politics;

using Republic.Core.Politics;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

public sealed class CountryBillTests
{
    private static RepublicTime CreateTestTime(int hour = 6)
    {
        var clock = RepublicClock.CreateControlled();
        return clock.FromDayAndTime(0, new TimeOnly(hour, 0));
    }

    [Fact]
    public void Proposing_RoadsAct_StoresTitleAndStatusPending()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary, id: "player-country");

        var bill = country.ProposeBill("Roads Act");

        Assert.NotNull(bill);
        Assert.Equal("Roads Act", bill.Title);
        Assert.Equal(CountryBillStatus.Pending, bill.Status);
        Assert.Equal(BillStatus.Pending, bill.Status);
        Assert.True(bill.IsPending);
        Assert.False(bill.IsPassed);
        Assert.False(bill.IsFailed);
        Assert.Null(bill.VotedBoundary);
        Assert.Equal("player-country", bill.CountryId);
        Assert.Same(bill, country.Bill);
        Assert.Same(bill, country.CountryBill);
        Assert.Same(bill, country.PendingBill);
        Assert.Single(country.Bills);
        Assert.Same(bill, country.Bills[0]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EmptyTitle_IsRejected_AndDoesNotCreateBill(string? invalidTitle)
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        var bill = country.ProposeBill(invalidTitle);

        Assert.Null(bill);
        Assert.Null(country.Bill);
        Assert.Null(country.PendingBill);
        Assert.Empty(country.Bills);
    }

    [Fact]
    public void SecondPendingBill_IsRejected()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        var first = country.ProposeBill("Roads Act");
        Assert.NotNull(first);

        var second = country.ProposeBill("Education Act");

        Assert.Null(second);
        Assert.Same(first, country.Bill);
        Assert.Same(first, country.PendingBill);
        Assert.Equal("Roads Act", country.Bill!.Title);
        Assert.Equal(CountryBillStatus.Pending, country.Bill.Status);
        Assert.Single(country.Bills);
    }

    [Fact]
    public void Voting_WithoutRulingParty_LeavesBillPending()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        var bill = country.ProposeBill("Roads Act");
        Assert.NotNull(bill);

        var result = country.VoteBill(boundary);

        Assert.NotNull(result);
        Assert.Equal(CountryBillStatus.Pending, result!.Status);
        Assert.Equal(BillStatus.Pending, result.Status);
        Assert.True(result.IsPending);
        Assert.False(result.IsPassed);
        Assert.Null(result.VotedBoundary);

        Assert.Equal(CountryBillStatus.Pending, country.Bill!.Status);
        Assert.Null(country.Bill.VotedBoundary);
        Assert.Same(country.Bill, country.PendingBill);

        // Implicit boolean evaluates to false when bill is not passed
        bool passed = result;
        Assert.False(passed);
    }

    [Fact]
    public void Voting_WithRulingParty_SetsStatusPassed_AndRecordsBoundary()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        // Establish ruling party
        var ruling = country.FoundParty("Civic Union", boundary);
        Assert.NotNull(ruling);

        var bill = country.ProposeBill("Roads Act");
        Assert.NotNull(bill);

        var result = country.VoteBill(boundary);

        Assert.NotNull(result);
        Assert.Equal(CountryBillStatus.Passed, result!.Status);
        Assert.Equal(BillStatus.Passed, result.Status);
        Assert.True(result.IsPassed);
        Assert.False(result.IsPending);
        Assert.Equal(boundary, result.VotedBoundary);

        Assert.Equal(CountryBillStatus.Passed, country.Bill!.Status);
        Assert.Equal(boundary, country.Bill.VotedBoundary);
        Assert.Null(country.PendingBill);

        // Implicit boolean evaluates to true when bill is passed
        bool passed = result;
        Assert.True(passed);
    }

    [Fact]
    public void VotingTwice_DoesNotChangeBoundary()
    {
        var boundary1 = CreateTestTime(6);
        var boundary2 = CreateTestTime(12);
        var country = Country.Found("Republic of Arcadia", boundary1);

        country.FoundParty("Civic Union", boundary1);
        country.ProposeBill("Roads Act");

        var firstVote = country.VoteBill(boundary1);
        Assert.NotNull(firstVote);
        Assert.Equal(CountryBillStatus.Passed, firstVote!.Status);
        Assert.Equal(boundary1, firstVote.VotedBoundary);

        // Vote a second time at a later boundary
        var secondVote = country.VoteBill(boundary2);
        Assert.NotNull(secondVote);
        Assert.Equal(CountryBillStatus.Passed, secondVote!.Status);
        Assert.Equal(boundary1, secondVote.VotedBoundary); // Boundary remains original boundary1

        Assert.Equal(boundary1, country.Bill!.VotedBoundary);
    }

    [Fact]
    public void CountryIsolation_CountryB_DoesNotReceiveCountryABill()
    {
        var boundary = CreateTestTime(6);
        var countryA = Country.Found("Republic of Arcadia", boundary, id: "country-a");
        var countryB = Country.Found("Federation of Norse", boundary, id: "country-b");

        var billA = countryA.ProposeBill("Roads Act");

        Assert.NotNull(billA);
        Assert.Null(countryB.Bill);
        Assert.Null(countryB.PendingBill);
        Assert.Empty(countryB.Bills);

        // Country B can independently propose its own bill
        var billB = countryB.ProposeBill("Health Act");

        Assert.NotNull(billB);
        Assert.Equal("Health Act", countryB.Bill?.Title);
        Assert.Equal("country-b", countryB.Bill?.CountryId);

        // Country A remains unaffected
        Assert.Equal("Roads Act", countryA.Bill?.Title);
        Assert.Equal("country-a", countryA.Bill?.CountryId);

        // Country A voting does not affect Country B
        countryA.FoundParty("Civic Union", boundary);
        countryA.VoteBill(boundary);

        Assert.Equal(CountryBillStatus.Passed, countryA.Bill!.Status);
        Assert.Equal(CountryBillStatus.Pending, countryB.Bill!.Status);
    }

    [Fact]
    public void Proposing_WithWhitespaceTitle_TrimsTitle()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);

        var bill = country.ProposeBill("   Roads Act   ");

        Assert.NotNull(bill);
        Assert.Equal("Roads Act", bill.Title);
    }

    [Fact]
    public void Constructor_WithEmptyOrWhitespaceTitle_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new CountryBill("", "c1"));
        Assert.Throws<ArgumentException>(() => new CountryBill("   ", "c1"));
        Assert.Throws<ArgumentNullException>(() => new CountryBill(null!, "c1"));
    }

    [Fact]
    public void ToString_FormatsPendingAndPassedCorrectly()
    {
        var boundary = CreateTestTime(6);
        var pendingBill = new CountryBill("Roads Act", "c1", CountryBillStatus.Pending, null);
        Assert.Equal("Roads Act (Pending)", pendingBill.ToString());

        var passedBill = new CountryBill("Roads Act", "c1", CountryBillStatus.Passed, boundary);
        Assert.Contains("Roads Act (Passed at Day 0, 06:00 WAT", passedBill.ToString());
    }

    [Fact]
    public void Deconstruct_ReturnsAllComponents()
    {
        var boundary = CreateTestTime(6);
        var bill = new CountryBill("Roads Act", "c1", CountryBillStatus.Passed, boundary);

        var (title, countryId, status, votedBoundary) = bill;

        Assert.Equal("Roads Act", title);
        Assert.Equal("c1", countryId);
        Assert.Equal(CountryBillStatus.Passed, status);
        Assert.Equal(boundary, votedBoundary);
    }

    [Fact]
    public void VoteBill_UsingClock_UsesSimulationClockWithoutCallingDateTimeNow()
    {
        var clock = RepublicClock.CreateControlled();
        var country = Country.Found("Republic of Arcadia", clock.CurrentTime);
        country.FoundParty("Civic Union", clock);
        country.ProposeBill("Roads Act");

        var result = country.VoteBill(clock);

        Assert.NotNull(result);
        Assert.Equal(CountryBillStatus.Passed, result!.Status);
        Assert.Equal(clock.CurrentTime, result.VotedBoundary);
        Assert.Equal(clock.CurrentTime, country.Bill!.VotedBoundary);
    }

    [Fact]
    public void OnceBillIsPassed_AnotherBillCanBeProposed()
    {
        var boundary = CreateTestTime(6);
        var country = Country.Found("Republic of Arcadia", boundary);
        country.FoundParty("Civic Union", boundary);

        var first = country.ProposeBill("Roads Act");
        var voted = country.VoteBill(boundary);

        Assert.Equal(CountryBillStatus.Passed, voted!.Status);
        Assert.Equal(CountryBillStatus.Passed, country.Bill!.Status);
        Assert.Null(country.PendingBill);

        // Since no bill is pending now, a second bill can be proposed
        var second = country.ProposeBill("Education Act");

        Assert.NotNull(second);
        Assert.Equal("Education Act", second.Title);
        Assert.Equal(CountryBillStatus.Pending, second.Status);
        Assert.Equal(2, country.Bills.Count);
    }

    [Fact]
    public void CountryFound_SupportsOptionalBill()
    {
        var boundary = CreateTestTime(6);
        var initialBill = new CountryBill("Founding Charter", "arcadia", CountryBillStatus.Passed, boundary);
        var country = Country.Found("Republic of Arcadia", boundary, id: "arcadia", bill: initialBill);

        Assert.Same(initialBill, country.Bill);
        Assert.Single(country.Bills);
    }
}
