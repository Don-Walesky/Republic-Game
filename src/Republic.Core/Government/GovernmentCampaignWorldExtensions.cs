namespace Republic.Core.Government;

public sealed partial class GovernmentSimulationService
{
    public void InitializeCampaignFramework(GovernmentState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.CampaignOffice = new CampaignOfficeState
        {
            CampaignManager = "Avery Cole",
            Polling = 30m,
            Funding = 20m,
            Endorsements = 10m,
            VotingBlocs = 20m,
            Stakeholders = BuildDefaultStakeholders(),
            LobbyDeals = BuildDefaultLobbyDeals(),
        };
    }

    public void GenerateNeighborAiCountries(GovernmentState state, int count)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.WorldCountries.Clear();
        for (var i = 1; i <= count; i++)
        {
            state.WorldCountries.Add(new WorldCountry
            {
                Name = $"Nation-{i}",
                CurrencyCode = $"N{i:00}",
                IsAiControlled = true,
                PresidentName = $"AI President {i}",
                EconomyPower = 25m + i * 3m,
                MilitaryPower = 20m + i * 2m,
                DiplomaticPower = 18m + i * 2m,
                CurrencyStrength = 1m,
            });
        }

        RecalculateForexMarket(state);
    }

    public ProceduralNationProfile GenerateProceduralNation(string countryName, string currencyCode, string leaderName)
    {
        var seed = Math.Abs(HashCode.Combine(countryName, currencyCode, leaderName));
        var structures = new[] { "Federal Republic", "Constitutional Republic", "Parliamentary Republic" };
        var geographies = new[] { "Coastal + Savannah", "River Basin + Highlands", "Archipelago + Plains" };
        var demographics = new[] { "Young urbanizing", "Balanced urban-rural", "Diverse multilingual" };
        var cultures = new[] { "Consensus-driven", "Partisan-competitive", "Institution-first" };
        var constitutions = new[] { "Two-term executive", "Four-year renewable", "Strong bicameral checks" };

        return new ProceduralNationProfile
        {
            GovernmentStructure = structures[seed % structures.Length],
            Geography = geographies[(seed / 11) % geographies.Length],
            Demographics = demographics[(seed / 17) % demographics.Length],
            PoliticalCulture = cultures[(seed / 19) % cultures.Length],
            Constitution = constitutions[(seed / 23) % constitutions.Length],
            RegionalStrength = 20 + (seed % 41),
            WorldPower = 15 + ((seed / 7) % 46),
        };
    }

    public void ResolveCampaignMeeting(GovernmentState state, Guid stakeholderId, CampaignChoice choice)
    {
        ArgumentNullException.ThrowIfNull(state);

        var stakeholder = state.CampaignOffice.Stakeholders.FirstOrDefault(s => s.Id == stakeholderId)
            ?? throw new InvalidOperationException("Stakeholder not found.");

        switch (choice)
        {
            case CampaignChoice.AcceptDemand:
                state.CampaignOffice.Polling += stakeholder.Influence * 0.8m;
                state.CampaignOffice.Endorsements += stakeholder.Influence * 0.5m;
                state.CampaignOffice.PublicPromises.Add(new CampaignPromise
                {
                    Id = Guid.NewGuid(),
                    Theme = stakeholder.Demand,
                    ApprovalImpact = 2m,
                    EconomyImpact = 1m,
                    StabilityImpact = -0.5m,
                    DiplomacyImpact = 0.5m,
                });
                break;
            case CampaignChoice.CounterOffer:
                state.CampaignOffice.Polling += stakeholder.Influence * 0.35m;
                state.CampaignOffice.Endorsements += stakeholder.Influence * 0.2m;
                break;
            case CampaignChoice.GentlemensAgreement:
                state.CampaignOffice.Polling += stakeholder.Influence * 0.5m;
                state.CampaignOffice.SecretDeals.Add(new SecretDeal
                {
                    Id = Guid.NewGuid(),
                    Counterparty = stakeholder.Name,
                    Obligation = stakeholder.Demand,
                    ScandalRisk = 25m + stakeholder.Influence * 0.5m,
                });
                break;
            case CampaignChoice.Modify:
                state.CampaignOffice.Polling += stakeholder.Influence * 0.4m;
                state.CampaignOffice.PublicPromises.Add(new CampaignPromise
                {
                    Id = Guid.NewGuid(),
                    Theme = $"Modified: {stakeholder.Demand}",
                    ApprovalImpact = 1m,
                    EconomyImpact = 0.6m,
                    StabilityImpact = 0.3m,
                    DiplomacyImpact = 0.3m,
                });
                break;
            case CampaignChoice.Decline:
                state.CampaignOffice.Polling -= stakeholder.Influence * 0.5m;
                break;
        }

        state.CampaignOffice.Polling = Math.Clamp(state.CampaignOffice.Polling, 0m, 100m);
    }

    public bool ResolveElectionNight(GovernmentState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var pollingWithFundingBoost = state.CampaignOffice.Polling + (state.CampaignOffice.Funding * 0.3m);
        var won = pollingWithFundingBoost >= 50m;

        if (won)
        {
            state.Phase = OfficePhase.InOffice;
            state.IsInOffice = true;
            ApplyCampaignCarryover(state);
        }
        else
        {
            state.IsInOffice = false;
            state.Phase = OfficePhase.Campaign;
            state.PlayerPresence.CaretakerPresident = "AI Caretaker";
        }

        return won;
    }

    public void SimulateSecretDealExposure(GovernmentState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        foreach (var deal in state.CampaignOffice.SecretDeals.Where(d => !d.Exposed))
        {
            if (deal.ScandalRisk >= 40m)
            {
                deal.Exposed = true;
                state.ApprovalRating = Math.Max(0m, state.ApprovalRating - 6m);
                state.StabilityIndex = Math.Max(0m, state.StabilityIndex - 4m);
            }
        }
    }

    public void UpdatePlayerPresence(GovernmentState state, int consecutiveOfflineTurns, int toppleThresholdTurns)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.PlayerPresence.ConsecutiveOfflineTurns = consecutiveOfflineTurns;
        state.PlayerPresence.Online = consecutiveOfflineTurns == 0;

        if (consecutiveOfflineTurns < toppleThresholdTurns || state.PlayerPresence.GovernmentToppled)
        {
            return;
        }

        state.PlayerPresence.GovernmentToppled = true;
        state.PlayerPresence.CaretakerPresident = "AI Transitional President";
        state.IsInOffice = false;
        state.Phase = OfficePhase.Campaign;

        var exploitableNeighbors = state.WorldCountries.OrderByDescending(item => item.EconomyPower).Take(3).ToList();
        foreach (var neighbor in exploitableNeighbors)
        {
            neighbor.ProxyInfluenceFromPlayer = Math.Min(80m, neighbor.ProxyInfluenceFromPlayer + 20m);
            state.PlayerPresence.ProxyControlCountries.Add(neighbor.Name);
        }
    }

    public void RecalculateForexMarket(GovernmentState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var playerPower = (state.Gdp / 2_000_000m) + (state.RegionalProfile.RegionalStrength * 0.4m) + (state.RegionalProfile.WorldPower * 0.3m);
        var totalPower = playerPower + state.WorldCountries.Sum(c => c.EconomyPower + c.DiplomaticPower + c.MilitaryPower * 0.25m);
        if (totalPower <= 0m)
        {
            return;
        }

        var playerCurrency = state.Currencies.FirstOrDefault();
        if (playerCurrency is not null)
        {
            playerCurrency.ExchangeRate = Math.Clamp(0.5m + (playerPower / totalPower) * 6m, 0.5m, 6m);
            playerCurrency.MarketPerformance = playerPower;
        }

        foreach (var country in state.WorldCountries)
        {
            var countryPower = country.EconomyPower + country.DiplomaticPower + country.MilitaryPower * 0.25m;
            country.CurrencyStrength = Math.Clamp(0.5m + (countryPower / totalPower) * 6m, 0.5m, 6m);
        }
    }

    public GeopoliticalAlliance FormAlliance(GovernmentState state, string allianceName, AllianceType type, IReadOnlyCollection<string> members)
    {
        ArgumentNullException.ThrowIfNull(state);

        var alliance = new GeopoliticalAlliance
        {
            Name = allianceName,
            Type = type,
            IncludesNoWarClause = true,
            Members = members.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
        };

        if (!alliance.Members.Contains(state.CountryName, StringComparer.OrdinalIgnoreCase))
        {
            alliance.Members.Add(state.CountryName);
        }

        state.Alliances.Add(alliance);
        return alliance;
    }

    public PeaceTreaty CreateDefaultPeaceAccords(GovernmentState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var treaty = new PeaceTreaty
        {
            Name = "Non-Aggression Democratic Charter",
            MandatoryNoWar = true,
            Signatories = new[] { state.CountryName }.Concat(state.WorldCountries.Select(c => c.Name)).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
        };

        state.PeaceTreaties.Clear();
        state.PeaceTreaties.Add(treaty);
        return treaty;
    }

    public MilitaryIncident RecordMilitaryIncident(GovernmentState state, string initiator, string target, string incidentType, IncidentSeverity severity)
    {
        ArgumentNullException.ThrowIfNull(state);

        var noWar = state.PeaceTreaties.Any(t => t.MandatoryNoWar &&
            t.Signatories.Contains(initiator, StringComparer.OrdinalIgnoreCase) &&
            t.Signatories.Contains(target, StringComparer.OrdinalIgnoreCase));

        if (incidentType.Equals("War", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("War declarations are disabled in Republic. Use constrained military incidents instead.");
        }

        var incident = new MilitaryIncident
        {
            InitiatorCountry = initiator,
            TargetCountry = target,
            IncidentType = incidentType,
            Severity = severity,
            EscalationPreventedByTreaty = noWar,
            OccurredAt = DateTimeOffset.UtcNow,
        };

        state.MilitaryIncidents.Add(incident);
        state.StabilityIndex = Math.Max(0m, state.StabilityIndex - ((int)severity + 1) * 1.2m);
        state.DiplomacyIndex = Math.Max(0m, state.DiplomacyIndex - ((int)severity + 1) * 0.8m);
        return incident;
    }

    public void RunMidtermElection(GovernmentState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var swing = (int)Math.Round((state.ApprovalRating - 50m) / 4m);
        state.Legislature.SenateSeatsHeld = Math.Clamp(state.Legislature.SenateSeatsHeld + swing, 0, state.Legislature.SenateSeatsTotal);
        state.Legislature.HouseSeatsHeld = Math.Clamp(state.Legislature.HouseSeatsHeld + swing * 3, 0, state.Legislature.HouseSeatsTotal);

        state.HasPartyMajority =
            state.Legislature.SenateSeatsHeld > (state.Legislature.SenateSeatsTotal / 2) &&
            state.Legislature.HouseSeatsHeld > (state.Legislature.HouseSeatsTotal / 2);
    }

    public void ResolveDefectionCrisis(GovernmentState state, DefectionResponse response)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.LegislatorLoyalty >= 25m)
        {
            state.Legislature.DefectionCrisisActive = false;
            return;
        }

        state.Legislature.DefectionCrisisActive = true;

        switch (response)
        {
            case DefectionResponse.ConcedePatronage:
                state.TreasuryBalance -= 350_000m;
                state.LegislatorLoyalty = Math.Min(MaxLegislatorLoyalty, state.LegislatorLoyalty + 12m);
                break;
            case DefectionResponse.LetThemGo:
                state.Legislature.SenateSeatsHeld = Math.Max(0, state.Legislature.SenateSeatsHeld - 8);
                state.Legislature.HouseSeatsHeld = Math.Max(0, state.Legislature.HouseSeatsHeld - 24);
                break;
            case DefectionResponse.LaunchPurge:
                state.StabilityIndex = Math.Max(0m, state.StabilityIndex - 7m);
                state.Legislature.SenateSeatsHeld = Math.Max(0, state.Legislature.SenateSeatsHeld - 4);
                state.Legislature.HouseSeatsHeld = Math.Max(0, state.Legislature.HouseSeatsHeld - 14);
                state.LegislatorLoyalty = Math.Min(MaxLegislatorLoyalty, state.LegislatorLoyalty + 6m);
                break;
        }

        if (state.Legislature.SenateSeatsHeld == 0 || state.Legislature.HouseSeatsHeld == 0)
        {
            state.IsInOffice = false;
            state.Phase = OfficePhase.Campaign;
            state.PlayerPresence.CaretakerPresident = "AI Interim Executive";
            state.PlayerPresence.GovernmentToppled = true;
        }
    }

    private static List<CampaignStakeholder> BuildDefaultStakeholders() =>
    [
        new() { Id = Guid.NewGuid(), Name = "Union Boss", Bloc = "Labor", Demand = "Raise public-sector wages", Influence = 9m },
        new() { Id = Guid.NewGuid(), Name = "Business Tycoon", Bloc = "Capital", Demand = "Lower corporate tax", Influence = 11m },
        new() { Id = Guid.NewGuid(), Name = "Climate Coalition", Bloc = "Environment", Demand = "Commit to green transition", Influence = 8m },
        new() { Id = Guid.NewGuid(), Name = "Faith Council", Bloc = "Faith", Demand = "Protect traditional values", Influence = 7m },
        new() { Id = Guid.NewGuid(), Name = "Tech CEO Forum", Bloc = "Innovation", Demand = "Digital infrastructure grants", Influence = 10m },
        new() { Id = Guid.NewGuid(), Name = "Generals Council", Bloc = "Security", Demand = "Defense modernization budget", Influence = 9m },
    ];

    private static List<LobbyDeal> BuildDefaultLobbyDeals() =>
    [
        new() { Id = Guid.NewGuid(), LobbyName = "Defense Contractors", FavorRequested = "Procurement preference", FundingOffered = 4m },
        new() { Id = Guid.NewGuid(), LobbyName = "Pharma Alliance", FavorRequested = "Drug pricing reform delay", FundingOffered = 3m },
        new() { Id = Guid.NewGuid(), LobbyName = "Wall Street Group", FavorRequested = "Capital controls rollback", FundingOffered = 5m },
        new() { Id = Guid.NewGuid(), LobbyName = "Energy Producers", FavorRequested = "Exploration licenses", FundingOffered = 4m },
        new() { Id = Guid.NewGuid(), LobbyName = "Silicon Valley PAC", FavorRequested = "Tax credits for AI firms", FundingOffered = 4m },
    ];

    private static void ApplyCampaignCarryover(GovernmentState state)
    {
        foreach (var promise in state.CampaignOffice.PublicPromises)
        {
            state.ApprovalRating = Math.Clamp(state.ApprovalRating + promise.ApprovalImpact, 0m, 100m);
            state.EconomyIndex = Math.Clamp(state.EconomyIndex + promise.EconomyImpact, 0m, 100m);
            state.StabilityIndex = Math.Clamp(state.StabilityIndex + promise.StabilityImpact, 0m, 100m);
            state.DiplomacyIndex = Math.Clamp(state.DiplomacyIndex + promise.DiplomacyImpact, 0m, 100m);
        }
    }
}
