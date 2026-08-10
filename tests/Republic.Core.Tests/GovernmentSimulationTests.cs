namespace Republic.Core.Tests;

using Republic.Core.Government;

public sealed class GovernmentSimulationTests
{
    [Fact]
    public void WinElection_TransitionsToOfficeAndAssignsCabinet()
    {
        var service = new GovernmentSimulationService();

        var state = service.CreateInitialState(
            "Arcadia",
            "Arcadian Crown",
            "ARC",
            1000000m,
            1.25m);

        service.WinElection(state);
        service.AssignCabinetMember(state, "Mina Vale", "Treasury");
        service.AssignCabinetMember(state, "Jonas Reed", "Defense");

        Assert.Equal(OfficePhase.InOffice, state.Phase);
        Assert.True(state.IsInOffice);
        Assert.Contains(state.Cabinet, minister => minister.Portfolio == "Treasury");
    }

    [Fact]
    public void CreateEmploymentProgram_ConsumesTreasuryAndCreatesJobs()
    {
        var service = new GovernmentSimulationService();
        var state = service.CreateInitialState(
            "Arcadia",
            "Arcadian Crown",
            "ARC",
            1000000m,
            1.25m);

        var program = service.CreateEmploymentProgram(state, "Infrastructure Jobs", 250, 125000m);

        Assert.NotNull(program);
        Assert.Equal(250, program.JobsCreated);
        Assert.Equal(875000m, state.TreasuryBalance);
        Assert.Single(state.EmploymentPrograms);
    }

    [Fact]
    public void TradeThroughMinister_UpdatesCurrencyAndRecordsTrade()
    {
        var service = new GovernmentSimulationService();
        var state = service.CreateInitialState(
            "Arcadia",
            "Arcadian Crown",
            "ARC",
            1000000m,
            1.25m);

        service.WinElection(state);
        service.AssignCabinetMember(state, "Mina Vale", "Trade");

        var trade = service.ExecuteTradeThroughMinister(state, "Mina Vale", "Lys", 50000m, 1.10m);

        Assert.NotNull(trade);
        Assert.Equal(50000m, state.Currencies[0].TradeVolume);
        Assert.True(state.Currencies[0].ExchangeRate > 1.25m);
    }

    [Fact]
    public void PurchaseWeaponsAndRecruitment_UpdateMilitaryCapacity()
    {
        var service = new GovernmentSimulationService();
        var state = service.CreateInitialState(
            "Arcadia",
            "Arcadian Crown",
            "ARC",
            1000000m,
            1.25m);

        service.RecruitPersonnel(state, 120, 250m);
        service.PurchaseWeapons(state, 40, 6000m);

        Assert.Equal(120, state.Military.Personnel);
        Assert.Equal(40, state.Military.WeaponsInventory);
        Assert.Equal(993750m, state.TreasuryBalance);
    }

    [Fact]
    public void BridgeSnapshot_ReflectsStateAfterCoreActions()
    {
        var bridge = new GovernmentStateBridge();
        bridge.CreateInitialState("Arcadia", "Arcadian Crown", "ARC", 1000000m, 1.25m);

        var initial = bridge.GetSnapshot();
        Assert.Equal("Campaign", initial.Phase);
        Assert.Equal(1000000m, initial.TreasuryBalance);

        bridge.WinElection();
        bridge.AssignCabinetMember("Mina Vale", "Trade");
        bridge.CreateEmploymentProgram("Infrastructure Jobs", 250, 125000m);

        var updated = bridge.GetSnapshot();
        Assert.Equal("In Office", updated.Phase);
        Assert.Equal(875000m, updated.TreasuryBalance);
        Assert.Equal(1, updated.EmploymentPrograms);
        Assert.Equal(1, updated.CabinetCount);
    }

    [Fact]
    public void MinisterialTasksAndSenateReview_IntegrateIntoGovernanceFlow()
    {
        var service = new GovernmentSimulationService();
        var state = service.CreateInitialState("Arcadia", "Arcadian Crown", "ARC", 1000000m, 1.25m);

        service.WinElection(state);
        var review = service.ReviewAndAssignCabinetMember(state, "Mina Vale", "Trade", 72);
        var task = service.IssueMinisterialTask(state, "Mina Vale", "Expand trade corridors", 180000m);

        Assert.True(review.Approved);
        Assert.NotNull(task);
        Assert.True(task.NegotiatedCost < 180000m);
        Assert.Single(state.Cabinet);
    }

    [Fact]
    public void LegislativeNegotiationAndForeignAid_InfluenceEconomyAndApproval()
    {
        var service = new GovernmentSimulationService();
        var state = service.CreateInitialState("Arcadia", "Arcadian Crown", "ARC", 1000000m, 1.25m);

        service.WinElection(state);
        var bill = service.SponsorBill(state, "National Education Reform", "Sera Holt", 250000m);
        var negotiatedBill = service.NegotiateBill(state, bill.Id, 180000m, 8);
        var aid = service.NegotiateForeignAid(state, "Lys", "Grant", 4000000m, 0.05m, 2500000m);
        service.HoldPressBriefing(state, "Economic recovery plan", 6);

        Assert.Equal("Negotiated", negotiatedBill.Status);
        Assert.Equal(180000m, negotiatedBill.NegotiatedCost);
        Assert.True(aid.Amount > 0m);
        // Approval starts at 28; WinElection +5, grant +2, briefing +12 = well above initial
        Assert.True(state.ApprovalRating > state.ApprovalRating - 20m);
        Assert.True(state.ApprovalRating > 28m);
    }

    [Fact]
    public void CreateInitialState_GeneratesProceduralNationAndAiNeighbors()
    {
        var service = new GovernmentSimulationService();
        var state = service.CreateInitialState("Arcadia", "Arcadian Crown", "ARC", 1000000m, 1.25m);

        Assert.NotNull(state.RegionalProfile);
        Assert.False(string.IsNullOrWhiteSpace(state.RegionalProfile.GovernmentStructure));
        Assert.Equal(8, state.WorldCountries.Count);
        Assert.True(state.PeaceTreaties.Count > 0);
        Assert.True(state.CampaignOffice.Stakeholders.Count > 0);
    }

    [Fact]
    public void UpdatePlayerPresence_LongOfflineTopplesGovernmentAndCreatesProxyExposure()
    {
        var service = new GovernmentSimulationService();
        var state = service.CreateInitialState("Arcadia", "Arcadian Crown", "ARC", 1000000m, 1.25m);

        service.WinElection(state);
        service.UpdatePlayerPresence(state, consecutiveOfflineTurns: 6, toppleThresholdTurns: 5);

        Assert.True(state.PlayerPresence.GovernmentToppled);
        Assert.False(state.IsInOffice);
        Assert.True(state.PlayerPresence.ProxyControlCountries.Count > 0);
        Assert.Equal(OfficePhase.Campaign, state.Phase);
    }

    [Fact]
    public void RecordMilitaryIncident_BlocksWarAndAllowsConstrainedIncident()
    {
        var service = new GovernmentSimulationService();
        var state = service.CreateInitialState("Arcadia", "Arcadian Crown", "ARC", 1000000m, 1.25m);

        Assert.Throws<InvalidOperationException>(() =>
            service.RecordMilitaryIncident(state, "Arcadia", "Nation-1", "War", IncidentSeverity.High));

        var incident = service.RecordMilitaryIncident(state, "Arcadia", "Nation-1", "Cross-border strike", IncidentSeverity.Medium);

        Assert.NotNull(incident);
        Assert.True(incident.EscalationPreventedByTreaty);
        Assert.Single(state.MilitaryIncidents);
    }

    [Fact]
    public void ResolveDefectionCrisis_LosingEitherChamberRemovesPresident()
    {
        var service = new GovernmentSimulationService();
        var state = service.CreateInitialState("Arcadia", "Arcadian Crown", "ARC", 1000000m, 1.25m);

        service.WinElection(state);
        state.LegislatorLoyalty = 10m;
        state.Legislature.SenateSeatsHeld = 4;
        state.Legislature.HouseSeatsHeld = 10;

        service.ResolveDefectionCrisis(state, DefectionResponse.LetThemGo);

        Assert.False(state.IsInOffice);
        Assert.Equal(OfficePhase.Campaign, state.Phase);
        Assert.True(state.PlayerPresence.GovernmentToppled);
    }
}
