namespace Republic.Core.Government;

/// <summary>
/// All new complex governance systems: revenue inflows, industries, loans, military ops,
/// opposition, disasters, judiciary, intelligence, infrastructure projects, natural resources,
/// corruption, civil society, bilateral trade, minister initiatives, and legislator loyalty.
/// </summary>
public sealed partial class GovernmentSimulationService
{
    // ── Turn advance & daily inflows ────────────────────────────────────────

    public TurnResult AdvanceTurn(GovernmentState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.CurrentTurn >= MaxTurnsPerTerm)
        {
            state.Phase = OfficePhase.Campaign;
            state.IsInOffice = false;
        }

        state.CurrentTurn++;

        var revenue = CalculateTurnRevenue(state);
        state.TreasuryBalance += revenue.Total;

        ApplyCorruptionDrain(state);
        ApplyLoanInterest(state);
        TickLegislatorLoyalty(state);
        TickOppositionCampaign(state);
        TickCivilUnrest(state);
        CheckForRandomEvent(state);

        state.ApprovalRating = Math.Clamp(state.ApprovalRating - 0.3m, 0m, 100m);
        state.UnemploymentRate = Math.Clamp(
            state.UnemploymentRate + (state.InsecurityIndex * 0.01m) - (state.InfrastructureIndex * 0.02m), 0m, 100m);

        return new TurnResult
        {
            Turn = state.CurrentTurn,
            Revenue = revenue,
            RandomEvent = state.LastRandomEvent,
        };
    }

    public TurnRevenue CalculateTurnRevenue(GovernmentState state)
    {
        var taxRevenue = state.Population * 0.012m;
        var tariffRevenue = state.Currencies.FirstOrDefault()?.TradeVolume * 0.02m ?? 0m;
        var industryIncome = state.Industries.Sum(i => i.TurnRevenue);
        var resourceIncome = state.NaturalResources.Where(r => r.IsExtracting).Sum(r => r.TurnYield);

        return new TurnRevenue
        {
            TaxRevenue = taxRevenue,
            TariffRevenue = tariffRevenue,
            IndustryIncome = industryIncome,
            ResourceIncome = resourceIncome,
            Total = taxRevenue + tariffRevenue + industryIncome + resourceIncome,
        };
    }

    // ── Industries ──────────────────────────────────────────────────────────

    public Industry AddIndustry(GovernmentState state, string name, IndustrySector sector, decimal investmentCost)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (investmentCost > state.TreasuryBalance)
            throw new InvalidOperationException("Insufficient treasury to invest in industry.");

        state.TreasuryBalance -= investmentCost;

        var industry = new Industry
        {
            Name = name,
            Sector = sector,
            Investment = investmentCost,
            TurnRevenue = investmentCost * 0.04m,
            EmployeesGenerated = (int)(investmentCost / 2500m),
        };

        state.Industries.Add(industry);
        state.UnemploymentRate = Math.Max(0m, state.UnemploymentRate - industry.EmployeesGenerated / 10000m);
        state.Gdp += industryGdpBoost(sector);
        state.InfrastructureIndex = Math.Min(100m, state.InfrastructureIndex + 0.5m);
        return industry;
    }

    private static decimal industryGdpBoost(IndustrySector sector) => sector switch
    {
        IndustrySector.Oil => 8_000_000m,
        IndustrySector.Technology => 5_000_000m,
        IndustrySector.Agriculture => 2_000_000m,
        IndustrySector.Manufacturing => 3_500_000m,
        IndustrySector.Tourism => 1_500_000m,
        IndustrySector.Finance => 4_000_000m,
        _ => 1_000_000m,
    };

    // ── Natural resources ───────────────────────────────────────────────────

    public NaturalResource DiscoverResource(GovernmentState state, string resourceName, ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(state);

        var resource = new NaturalResource
        {
            Name = resourceName,
            Type = resourceType,
            TurnYield = 0m,
            IsExtracting = false,
        };

        state.NaturalResources.Add(resource);
        state.ApprovalRating = Math.Min(100m, state.ApprovalRating + 3m);
        return resource;
    }

    public void BeginResourceExtraction(GovernmentState state, string resourceName, decimal extractionCost)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (extractionCost > state.TreasuryBalance)
            throw new InvalidOperationException("Insufficient treasury to begin extraction.");

        var resource = state.NaturalResources.FirstOrDefault(r =>
            r.Name.Equals(resourceName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Resource not found.");

        state.TreasuryBalance -= extractionCost;
        resource.IsExtracting = true;
        resource.TurnYield = extractionCost * 0.15m;
        state.Gdp += resource.TurnYield * 20m;
        state.InfrastructureIndex = Math.Min(100m, state.InfrastructureIndex + 1m);
    }

    // ── Infrastructure projects ─────────────────────────────────────────────

    public InfrastructureProject BuildInfrastructure(GovernmentState state, InfrastructureType projectType, decimal cost)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (cost > state.TreasuryBalance)
            throw new InvalidOperationException("Insufficient treasury for infrastructure project.");

        state.TreasuryBalance -= cost;

        var project = new InfrastructureProject
        {
            Type = projectType,
            Cost = cost,
            BuiltOnTurn = state.CurrentTurn,
        };

        state.InfrastructureProjects.Add(project);
        ApplyInfrastructureEffects(state, projectType, cost);
        return project;
    }

    private static void ApplyInfrastructureEffects(GovernmentState state, InfrastructureType type, decimal cost)
    {
        var scale = cost / 100_000m;
        switch (type)
        {
            case InfrastructureType.RoadNetwork:
                state.InfrastructureIndex = Math.Min(100m, state.InfrastructureIndex + 2m * scale);
                state.Gdp += 500_000m * scale;
                state.UnemploymentRate = Math.Max(0m, state.UnemploymentRate - 0.5m);
                break;
            case InfrastructureType.Airport:
                state.InfrastructureIndex = Math.Min(100m, state.InfrastructureIndex + 3m * scale);
                state.Gdp += 2_000_000m * scale;
                state.Currencies.FirstOrDefault()!.TradeVolume += 200_000m * scale;
                break;
            case InfrastructureType.University:
                state.TertiaryInstitutions++;
                state.EducationIndex = Math.Min(100m, state.EducationIndex + 3m * scale);
                state.Hdi = Math.Min(1m, state.Hdi + 0.01m);
                state.UnemploymentRate = Math.Max(0m, state.UnemploymentRate - 0.8m);
                state.InsecurityIndex = Math.Max(0m, state.InsecurityIndex - 0.5m);
                break;
            case InfrastructureType.PowerGrid:
                state.InfrastructureIndex = Math.Min(100m, state.InfrastructureIndex + 2.5m * scale);
                state.Gdp += 1_500_000m * scale;
                state.InsecurityIndex = Math.Max(0m, state.InsecurityIndex - 0.3m);
                break;
            case InfrastructureType.Hospital:
                state.Hdi = Math.Min(1m, state.Hdi + 0.015m);
                state.Population += (int)(500 * scale);
                state.InsecurityIndex = Math.Max(0m, state.InsecurityIndex - 0.4m);
                break;
        }

        state.ApprovalRating = Math.Min(100m, state.ApprovalRating + 1.5m * scale);
        state.ImmigrationCount += (int)(200 * scale);
        state.Population += (int)(200 * scale);
    }

    // ── Tertiary institutions ───────────────────────────────────────────────

    public void RecalculateTertiaryEffects(GovernmentState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var count = state.TertiaryInstitutions;
        state.EducationIndex = Math.Min(100m, 30m + count * 2.5m);
        state.Hdi = Math.Min(1m, 0.30m + count * 0.015m);
        state.UnemploymentRate = Math.Max(0m, state.UnemploymentRate - count * 0.3m);
        state.InsecurityIndex = Math.Max(0m, state.InsecurityIndex - count * 0.2m);
        state.Gdp += count * 800_000m;
    }

    // ── Loans ───────────────────────────────────────────────────────────────

    public LoanAgreement TakeLoan(GovernmentState state, LoanLender lender, decimal amount, decimal interestRate, int repaymentTurns)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (amount <= 0m) throw new ArgumentOutOfRangeException(nameof(amount));

        var loan = new LoanAgreement
        {
            Id = Guid.NewGuid(),
            Lender = lender,
            PrincipalAmount = amount,
            InterestRate = interestRate,
            TotalOwed = amount * (1m + interestRate),
            AmountRepaid = 0m,
            RepaymentTurns = repaymentTurns,
            TurnsTaken = state.CurrentTurn,
            IsSettled = false,
        };

        state.TreasuryBalance += amount;
        state.ExternalDebt += loan.TotalOwed;
        state.Loans.Add(loan);

        // Low approval penalty for IMF/World Bank conditionality
        if (lender is LoanLender.Imf or LoanLender.WorldBank)
            state.ApprovalRating = Math.Max(0m, state.ApprovalRating - 4m);

        return loan;
    }

    public void RepayLoan(GovernmentState state, Guid loanId, decimal amount)
    {
        ArgumentNullException.ThrowIfNull(state);
        var loan = state.Loans.FirstOrDefault(l => l.Id == loanId)
            ?? throw new InvalidOperationException("Loan not found.");
        if (loan.IsSettled) throw new InvalidOperationException("Loan already settled.");
        if (amount > state.TreasuryBalance) throw new InvalidOperationException("Insufficient treasury.");

        var payment = Math.Min(amount, loan.TotalOwed - loan.AmountRepaid);
        state.TreasuryBalance -= payment;
        state.ExternalDebt -= payment;
        loan.AmountRepaid += payment;

        if (loan.AmountRepaid >= loan.TotalOwed)
        {
            loan.IsSettled = true;
            state.ApprovalRating = Math.Min(100m, state.ApprovalRating + 2m);
        }
    }

    public void RenegotiateLoan(GovernmentState state, Guid loanId, decimal newInterestRate, int additionalTurns)
    {
        ArgumentNullException.ThrowIfNull(state);
        var loan = state.Loans.FirstOrDefault(l => l.Id == loanId && !l.IsSettled)
            ?? throw new InvalidOperationException("Active loan not found.");

        var remaining = loan.TotalOwed - loan.AmountRepaid;
        loan.TotalOwed = loan.AmountRepaid + remaining * (1m + newInterestRate);
        loan.RepaymentTurns += additionalTurns;
        state.ExternalDebt += remaining * newInterestRate;
        state.ApprovalRating = Math.Max(0m, state.ApprovalRating - 1.5m);
    }

    // ── Military operations (AI vs player and player vs AI) ─────────────────

    public MilitaryAction LaunchMilitaryOperation(GovernmentState attacker, string targetCountry, MilitaryOpType opType, int troopsCommitted)
    {
        ArgumentNullException.ThrowIfNull(attacker);
        if (troopsCommitted > attacker.Military.Personnel)
            throw new InvalidOperationException("Not enough personnel for this operation.");

        var successChance = attacker.Military.Personnel / 10000m + attacker.Military.WeaponsInventory / 5000m;
        var success = successChance > 0.4m;
        var cost = troopsCommitted * 12m;

        if (cost > attacker.TreasuryBalance)
            throw new InvalidOperationException("Insufficient treasury for military operation.");

        attacker.TreasuryBalance -= cost;
        attacker.Military.Personnel -= troopsCommitted / 5;

        var action = new MilitaryAction
        {
            AttackerCountry = attacker.CountryName,
            TargetCountry = targetCountry,
            OpType = opType,
            TroopsCommitted = troopsCommitted,
            Succeeded = success,
            Timestamp = DateTimeOffset.UtcNow,
        };

        attacker.Military.OperationHistory.Add(action);

        if (success)
        {
            attacker.Gdp += 1_500_000m;
            attacker.ApprovalRating = Math.Min(100m, attacker.ApprovalRating + 3m);
            attacker.InsecurityIndex = Math.Max(0m, attacker.InsecurityIndex - 1m);
        }
        else
        {
            attacker.ApprovalRating = Math.Max(0m, attacker.ApprovalRating - 5m);
            attacker.InsecurityIndex += 2m;
        }

        return action;
    }

    public void ReceiveMilitaryAttack(GovernmentState defender, string attackerCountry, int attackStrength)
    {
        ArgumentNullException.ThrowIfNull(defender);

        var defenseScore = defender.Military.Personnel / 5000m + defender.Military.WeaponsInventory / 2500m;
        var repelled = defenseScore > attackStrength / 10000m;

        defender.InsecurityIndex = Math.Min(100m, defender.InsecurityIndex + (repelled ? 1m : 5m));
        defender.ApprovalRating = Math.Max(0m, defender.ApprovalRating - (repelled ? 1m : 6m));
        defender.TreasuryBalance -= repelled ? 50_000m : 200_000m;

        if (!repelled)
        {
            defender.Gdp = Math.Max(0m, defender.Gdp - 3_000_000m);
            defender.InfrastructureIndex = Math.Max(0m, defender.InfrastructureIndex - 2m);
        }

        defender.Military.OperationHistory.Add(new MilitaryAction
        {
            AttackerCountry = attackerCountry,
            TargetCountry = defender.CountryName,
            OpType = MilitaryOpType.Invasion,
            Succeeded = !repelled,
            Timestamp = DateTimeOffset.UtcNow,
        });
    }

    // ── Bilateral trade ─────────────────────────────────────────────────────

    public BilateralTrade NegotiateBilateralTrade(GovernmentState state, string partnerCountry, decimal exportValue, decimal importValue)
    {
        ArgumentNullException.ThrowIfNull(state);

        var netBenefit = exportValue - importValue;
        state.TreasuryBalance += netBenefit * 0.1m;
        state.Gdp += netBenefit;
        var currency = state.Currencies.FirstOrDefault();
        if (currency != null) currency.TradeVolume += exportValue + importValue;

        var trade = new BilateralTrade
        {
            PartnerCountry = partnerCountry,
            ExportValue = exportValue,
            ImportValue = importValue,
            NetBenefit = netBenefit,
            Turn = state.CurrentTurn,
        };

        state.BilateralTrades.Add(trade);
        state.ApprovalRating = Math.Min(100m, state.ApprovalRating + 1m);
        ApplyPopulationAndDevelopment(state, 1m, 0.5m, 0.2m, 150);
        return trade;
    }

    // ── Opposition AI ───────────────────────────────────────────────────────

    public void TickOppositionCampaign(GovernmentState state)
    {
        if (state.ApprovalRating < 40m)
        {
            state.Opposition.Strength = Math.Min(100m, state.Opposition.Strength + 2m);
            state.ApprovalRating = Math.Max(0m, state.ApprovalRating - 0.5m);
        }
        else if (state.ApprovalRating > 65m)
        {
            state.Opposition.Strength = Math.Max(0m, state.Opposition.Strength - 1m);
        }

        state.Opposition.LastAction = state.Opposition.Strength > 60m
            ? "Opposition launched public rally against economic policy."
            : "Opposition issued statement criticizing governance.";
    }

    // ── Legislator loyalty ──────────────────────────────────────────────────

    public void TickLegislatorLoyalty(GovernmentState state)
    {
        // Loyalty never exceeds MaxLegislatorLoyalty; it decays naturally each turn
        state.LegislatorLoyalty = Math.Clamp(
            state.LegislatorLoyalty - 0.4m + (state.ApprovalRating * 0.02m),
            10m,
            MaxLegislatorLoyalty);

        // Legislators pester: each turn they want something
        var demand = GenerateLegislatorDemand(state);
        if (demand != null) state.PendingLegislatorDemands.Add(demand);
    }

    public void GrantLegislatorDemand(GovernmentState state, Guid demandId, bool grant)
    {
        ArgumentNullException.ThrowIfNull(state);
        var demand = state.PendingLegislatorDemands.FirstOrDefault(d => d.Id == demandId)
            ?? throw new InvalidOperationException("Demand not found.");

        if (grant)
        {
            state.TreasuryBalance -= demand.Cost;
            state.LegislatorLoyalty = Math.Min(MaxLegislatorLoyalty, state.LegislatorLoyalty + 3m);
            state.ApprovalRating = Math.Min(100m, state.ApprovalRating + 0.5m);
        }
        else
        {
            state.LegislatorLoyalty = Math.Max(10m, state.LegislatorLoyalty - 4m);
        }

        state.PendingLegislatorDemands.Remove(demand);
    }

    private static LegislatorDemand? GenerateLegislatorDemand(GovernmentState state)
    {
        if (state.CurrentTurn % 3 != 0) return null;

        var demands = new[]
        {
            new LegislatorDemand { Id = Guid.NewGuid(), Description = "Fund constituency road project",    Cost = 80_000m },
            new LegislatorDemand { Id = Guid.NewGuid(), Description = "Increase education allocation",     Cost = 120_000m },
            new LegislatorDemand { Id = Guid.NewGuid(), Description = "Support local agriculture subsidy", Cost = 60_000m },
            new LegislatorDemand { Id = Guid.NewGuid(), Description = "Approve new hospital funding",      Cost = 150_000m },
        };

        return demands[state.CurrentTurn % demands.Length];
    }

    // ── Corruption ──────────────────────────────────────────────────────────

    public void ApplyCorruptionDrain(GovernmentState state)
    {
        var drain = state.TreasuryBalance * state.CorruptionLevel * 0.001m;
        state.TreasuryBalance -= drain;
        state.InsecurityIndex = Math.Min(100m, state.InsecurityIndex + state.CorruptionLevel * 0.05m);
        state.ApprovalRating = Math.Max(0m, state.ApprovalRating - state.CorruptionLevel * 0.02m);
    }

    public void InvestigateCorruption(GovernmentState state, decimal cost)
    {
        if (cost > state.TreasuryBalance) throw new InvalidOperationException("Insufficient funds.");
        state.TreasuryBalance -= cost;
        state.CorruptionLevel = Math.Max(0m, state.CorruptionLevel - 5m);
        state.ApprovalRating = Math.Min(100m, state.ApprovalRating + 3m);
        state.InsecurityIndex = Math.Max(0m, state.InsecurityIndex - 2m);
    }

    // ── Judiciary ───────────────────────────────────────────────────────────

    public Judge AppointJudge(GovernmentState state, string name, int senateScore)
    {
        ArgumentNullException.ThrowIfNull(state);

        var judge = new Judge
        {
            Name = name,
            Loyalty = Math.Clamp(senateScore - 20m, 10m, 70m),
            IsAppointed = senateScore >= 60,
        };

        if (judge.IsAppointed)
        {
            state.Judges.Add(judge);
            state.ApprovalRating = Math.Min(100m, state.ApprovalRating + 1m);
        }

        return judge;
    }

    public JudicialRuling RuleOnLaw(GovernmentState state, Guid billId)
    {
        ArgumentNullException.ThrowIfNull(state);
        var bill = state.Bills.FirstOrDefault(b => b.Id == billId)
            ?? throw new InvalidOperationException("Bill not found.");
        var loyalJudges = state.Judges.Count(j => j.Loyalty > 50m);
        var totalJudges = Math.Max(1, state.Judges.Count);
        var upholdChance = loyalJudges / (decimal)totalJudges;
        var upheld = upholdChance >= 0.5m;

        var ruling = new JudicialRuling
        {
            BillId = billId,
            BillTitle = bill.Title,
            Upheld = upheld,
            Notes = upheld ? "Law upheld by majority." : "Law struck down — unconstitutional.",
        };

        state.JudicialRulings.Add(ruling);
        bill.Status = upheld ? "Signed into Law" : "Struck Down";
        if (!upheld) state.ApprovalRating = Math.Max(0m, state.ApprovalRating - 4m);
        return ruling;
    }

    // ── Natural disasters & pandemics ────────────────────────────────────────

    public DisasterResponse RespondToDisaster(GovernmentState state, DisasterType disasterType, decimal reliefBudget)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (reliefBudget > state.TreasuryBalance) throw new InvalidOperationException("Insufficient funds.");

        state.TreasuryBalance -= reliefBudget;
        var effectiveness = reliefBudget / 500_000m;

        var response = new DisasterResponse
        {
            DisasterType = disasterType,
            ReliefBudget = reliefBudget,
            Effectiveness = Math.Min(1m, effectiveness),
            Turn = state.CurrentTurn,
        };

        state.DisasterResponses.Add(response);

        var approvalDelta = (effectiveness >= 0.6m) ? 4m : -5m;
        state.ApprovalRating = Math.Clamp(state.ApprovalRating + approvalDelta, 0m, 100m);
        state.InsecurityIndex = Math.Max(0m, state.InsecurityIndex - effectiveness * 3m);
        state.Population -= (int)(5000 * (1m - effectiveness));
        state.Gdp = Math.Max(0m, state.Gdp - 500_000m * (1m - effectiveness));

        return response;
    }

    // ── Minister initiatives ─────────────────────────────────────────────────

    public MinisterInitiative ReceiveMinisterInitiative(GovernmentState state, string ministerName, string proposalTitle, decimal cost, decimal projectedGdpImpact)
    {
        ArgumentNullException.ThrowIfNull(state);
        var minister = state.Cabinet.FirstOrDefault(m =>
            m.Name.Equals(ministerName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Minister not in cabinet.");

        var initiative = new MinisterInitiative
        {
            Id = Guid.NewGuid(),
            MinisterName = ministerName,
            ProposalTitle = proposalTitle,
            Cost = cost,
            ProjectedGdpImpact = projectedGdpImpact,
            Status = "Awaiting Presidential Approval",
        };

        state.PendingMinisterInitiatives.Add(initiative);
        return initiative;
    }

    public void ApproveMinisterInitiative(GovernmentState state, Guid initiativeId, bool approve)
    {
        ArgumentNullException.ThrowIfNull(state);
        var initiative = state.PendingMinisterInitiatives.FirstOrDefault(i => i.Id == initiativeId)
            ?? throw new InvalidOperationException("Initiative not found.");

        if (approve)
        {
            if (initiative.Cost > state.TreasuryBalance) throw new InvalidOperationException("Insufficient funds.");
            state.TreasuryBalance -= initiative.Cost;
            state.Gdp += initiative.ProjectedGdpImpact;
            initiative.Status = "Approved";
            state.ApprovalRating = Math.Min(100m, state.ApprovalRating + 1.5m);
        }
        else
        {
            initiative.Status = "Rejected";
        }

        state.PendingMinisterInitiatives.Remove(initiative);
        state.MinisterInitiativesHistory.Add(initiative);
    }

    // ── Civil society & protests ─────────────────────────────────────────────

    public void TickCivilUnrest(GovernmentState state)
    {
        var grievanceScore = (100m - state.ApprovalRating) * 0.1m
                           + state.UnemploymentRate * 0.2m
                           + state.InsecurityIndex * 0.1m
                           + state.CorruptionLevel * 0.15m;

        state.CivilUnrestIndex = Math.Clamp(grievanceScore, 0m, 100m);

        if (state.CivilUnrestIndex > 60m)
        {
            state.ApprovalRating = Math.Max(0m, state.ApprovalRating - 2m);
            state.Gdp = Math.Max(0m, state.Gdp - 200_000m);
            state.InsecurityIndex = Math.Min(100m, state.InsecurityIndex + 0.5m);
        }
    }

    public void AddressProtest(GovernmentState state, string concession, decimal cost)
    {
        if (cost > state.TreasuryBalance) throw new InvalidOperationException("Insufficient funds.");
        state.TreasuryBalance -= cost;
        state.CivilUnrestIndex = Math.Max(0m, state.CivilUnrestIndex - 15m);
        state.ApprovalRating = Math.Min(100m, state.ApprovalRating + 3m);
    }

    // ── Intelligence & espionage ─────────────────────────────────────────────

    public IntelligenceOp RunIntelligenceOp(GovernmentState state, IntelOpType opType, string targetCountry, decimal cost)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (cost > state.TreasuryBalance) throw new InvalidOperationException("Insufficient funds.");

        state.TreasuryBalance -= cost;
        var success = state.Military.Personnel > 5000 && cost > 50_000m;

        var op = new IntelligenceOp
        {
            OpType = opType,
            TargetCountry = targetCountry,
            Cost = cost,
            Succeeded = success,
            Turn = state.CurrentTurn,
        };

        state.IntelligenceOps.Add(op);

        if (success)
            state.InsecurityIndex = Math.Max(0m, state.InsecurityIndex - 2m);
        else
            state.InsecurityIndex = Math.Min(100m, state.InsecurityIndex + 1m);

        return op;
    }

    // ── Loan interest tick ───────────────────────────────────────────────────

    private static void ApplyLoanInterest(GovernmentState state)
    {
        foreach (var loan in state.Loans.Where(l => !l.IsSettled))
        {
            var interestDue = (loan.TotalOwed - loan.AmountRepaid) * 0.005m;
            state.ExternalDebt += interestDue;
            loan.TotalOwed += interestDue;
        }
    }

    // ── Random events ────────────────────────────────────────────────────────

    private static void CheckForRandomEvent(GovernmentState state)
    {
        if (state.CurrentTurn % 7 != 0) return;

        var events = new[]
        {
            new RandomEvent { Name = "Oil price spike",           ApprovalDelta = 3m,  GdpDelta = 5_000_000m,  InsecurityDelta = 0m },
            new RandomEvent { Name = "Drought hits agriculture",  ApprovalDelta = -4m, GdpDelta = -2_000_000m, InsecurityDelta = 2m },
            new RandomEvent { Name = "Investor confidence surge", ApprovalDelta = 2m,  GdpDelta = 8_000_000m,  InsecurityDelta = 0m },
            new RandomEvent { Name = "Workers' strike",           ApprovalDelta = -3m, GdpDelta = -1_500_000m, InsecurityDelta = 1m },
            new RandomEvent { Name = "Trade partner recession",   ApprovalDelta = -2m, GdpDelta = -3_000_000m, InsecurityDelta = 0m },
            new RandomEvent { Name = "Technology boom",           ApprovalDelta = 4m,  GdpDelta = 6_000_000m,  InsecurityDelta = 0m },
        };

        var ev = events[state.CurrentTurn % events.Length];
        state.ApprovalRating = Math.Clamp(state.ApprovalRating + ev.ApprovalDelta, 0m, 100m);
        state.Gdp = Math.Max(0m, state.Gdp + ev.GdpDelta);
        state.InsecurityIndex = Math.Clamp(state.InsecurityIndex + ev.InsecurityDelta, 0m, 100m);
        state.LastRandomEvent = ev;
    }

    // ── Constants ────────────────────────────────────────────────────────────

    public const int MaxTurnsPerTerm = 30;
    public const decimal MaxLegislatorLoyalty = 68m;
}
