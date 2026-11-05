namespace Republic.Core.Economy.Businesses.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Republic.Core.Cabinet.Models;
using Republic.Core.Economy.Businesses.Models;
using Republic.Core.Economy.Treasury;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Authoritative implementation of the global business approach and industrialist integration service.
/// Coordinates the First Wave (5 businesses at 30-day intervals after presidency begins)
/// and the Second Wave (5 multinational corporations at 30-day intervals after national currency creation).
/// </summary>
public sealed class GlobalBusinessService : IGlobalBusinessService
{
    public IReadOnlyList<GlobalBusinessProposal> GetPredefinedProposals(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        string GetMinisterTitle(CabinetPortfolio portfolio, string fallbackTitle)
        {
            var min = country.GetMinister(portfolio);
            return min != null ? $"{min.Name} ({fallbackTitle})" : fallbackTitle;
        }

        return new List<GlobalBusinessProposal>
        {
            // =========================================================================
            // WAVE 1: INAUGURAL INDUSTRIAL WAVE (Starts Day 30 of Presidency, 30-day cadence)
            // =========================================================================
            new()
            {
                Id = "biz_wave1_fmcg",
                BusinessName = "Nestor-Valo Consumer Goods",
                IndustrialistName = "CEO Jean-Luc Vane",
                Industry = "FMCG",
                Wave = GlobalBusinessWave.FirstWave,
                ScheduledDay = 30,
                PresentingMinister = GetMinisterTitle(CabinetPortfolio.Interior, "Minister of Interior & Domestic Commerce"),
                ProposalTitle = "National Packaged Foods & Clean Drinking Water Bottling Plant",
                Description = "Nestor-Valo proposes constructing a modern, automated food packaging and purified water bottling hub. Supplies municipal markets with fortified essential staples while stabilizing domestic consumer retail prices.",
                CapitalInvestment = 15_000_000.0,
                ProjectedJobs = 2400,
                QuarterlyTaxContribution = 3_500_000.0,
                GdpContribution = 120_000_000.0,
                QuarterlyExportContribution = 10_000_000.0,
                StockMarketConfidenceBonus = 35.0,
                ForeignReservesInflow = 8_000_000.0
            },
            new()
            {
                Id = "biz_wave1_tech",
                BusinessName = "OmniNet Cloud Systems",
                IndustrialistName = "Founder Mira Chen",
                Industry = "Technology",
                Wave = GlobalBusinessWave.FirstWave,
                ScheduledDay = 60,
                PresentingMinister = GetMinisterTitle(CabinetPortfolio.Education, "Minister of Education & Digital Innovation"),
                ProposalTitle = "Hyperscale Regional Cloud Data Center & Tech Academy",
                Description = "OmniNet seeks site allocation for a tier-4 fiber-connected data hub and high-tech software vocational campus. Guarantees 1,800 direct software engineering and cybersecurity apprenticeships for domestic youth.",
                CapitalInvestment = 28_000_000.0,
                ProjectedJobs = 1800,
                QuarterlyTaxContribution = 5_200_000.0,
                GdpContribution = 250_000_000.0,
                QuarterlyExportContribution = 20_000_000.0,
                StockMarketConfidenceBonus = 60.0,
                ForeignReservesInflow = 15_000_000.0
            },
            new()
            {
                Id = "biz_wave1_auto",
                BusinessName = "Voltax Motor Works",
                IndustrialistName = "Chairperson Henrik Lindqvist",
                Industry = "Automobile",
                Wave = GlobalBusinessWave.FirstWave,
                ScheduledDay = 90,
                PresentingMinister = GetMinisterTitle(CabinetPortfolio.Infrastructure, "Minister of Infrastructure & Transport"),
                ProposalTitle = "Commercial Electric Bus & Fleet Vehicle Assembly Facility",
                Description = "Voltax will erect a specialized heavy vehicle manufacturing facility to produce electric public transit buses and municipal utility fleets, reducing national petroleum dependence and expanding regional exports.",
                CapitalInvestment = 35_000_000.0,
                ProjectedJobs = 3500,
                QuarterlyTaxContribution = 7_000_000.0,
                GdpContribution = 380_000_000.0,
                QuarterlyExportContribution = 35_000_000.0,
                StockMarketConfidenceBonus = 75.0,
                ForeignReservesInflow = 20_000_000.0
            },
            new()
            {
                Id = "biz_wave1_energy",
                BusinessName = "Helios Bio-Refineries",
                IndustrialistName = "Managing Director Tariq Al-Hassan",
                Industry = "Energy",
                Wave = GlobalBusinessWave.FirstWave,
                ScheduledDay = 120,
                PresentingMinister = GetMinisterTitle(CabinetPortfolio.OilAndGas, "Minister of Oil & Gas Resources"),
                ProposalTitle = "Agro-Fuel Synthesis & Clean Synthetic Lubricants Refinery",
                Description = "Helios partners with regional agricultural producers to convert organic waste and biomass into clean synthetic diesel and lubricants. Bolsters sovereign energy security while curtailing fuel import bills.",
                CapitalInvestment = 42_000_000.0,
                ProjectedJobs = 2200,
                QuarterlyTaxContribution = 8_500_000.0,
                GdpContribution = 450_000_000.0,
                QuarterlyExportContribution = 50_000_000.0,
                StockMarketConfidenceBonus = 85.0,
                ForeignReservesInflow = 25_000_000.0
            },
            new()
            {
                Id = "biz_wave1_pharma",
                BusinessName = "Aegis BioPharma Global",
                IndustrialistName = "Chief Scientist Dr. Sarah Sterling",
                Industry = "Pharmaceuticals",
                Wave = GlobalBusinessWave.FirstWave,
                ScheduledDay = 150,
                PresentingMinister = GetMinisterTitle(CabinetPortfolio.Health, "Minister of Health"),
                ProposalTitle = "Essential Antibiotics & Diagnostic Formulation Laboratory",
                Description = "Aegis BioPharma establishes a WHO-standard formulation laboratory to synthesize essential antibiotics, diagnostic reagents, and vaccines domestically. Guarantees state hospital supplies at fixed discounted rates.",
                CapitalInvestment = 30_000_000.0,
                ProjectedJobs = 1900,
                QuarterlyTaxContribution = 6_000_000.0,
                GdpContribution = 310_000_000.0,
                QuarterlyExportContribution = 25_000_000.0,
                StockMarketConfidenceBonus = 55.0,
                ForeignReservesInflow = 18_000_000.0
            },

            // =========================================================================
            // WAVE 2: MULTINATIONAL CURRENCY EXPANSION WAVE (30-day cadence post-currency creation)
            // =========================================================================
            new()
            {
                Id = "biz_wave2_logistics",
                BusinessName = "AeroCargo Intercontinental",
                IndustrialistName = "CEO Montgomery Vance",
                Industry = "Retail & Logistics",
                Wave = GlobalBusinessWave.SecondWave,
                ScheduledDay = 30,
                PresentingMinister = GetMinisterTitle(CabinetPortfolio.Infrastructure, "Minister of Infrastructure"),
                ProposalTitle = "Continental Logistics Megahub & Automated Freight Depot",
                Description = "Capitalizing on the sovereign currency float, AeroCargo establishes its continental freight transshipment terminal. Connects national rail and sea corridors with high-throughput automated sorting yards.",
                CapitalInvestment = 65_000_000.0,
                ProjectedJobs = 6000,
                QuarterlyTaxContribution = 14_000_000.0,
                GdpContribution = 800_000_000.0,
                QuarterlyExportContribution = 90_000_000.0,
                StockMarketConfidenceBonus = 110.0,
                ForeignReservesInflow = 40_000_000.0
            },
            new()
            {
                Id = "biz_wave2_semiconductors",
                BusinessName = "Apex Silicon Foundry",
                IndustrialistName = "President Kenji Sato",
                Industry = "Semiconductors",
                Wave = GlobalBusinessWave.SecondWave,
                ScheduledDay = 60,
                PresentingMinister = GetMinisterTitle(CabinetPortfolio.Resources, "Minister of Strategic Resources"),
                ProposalTitle = "Advanced Semiconductor Wafer Fabrication Complex",
                Description = "Apex Silicon invests in a high-precision semiconductor fab facility, leveraging domestic clean water and power grids to fabricate microcontrollers and power management chips for export.",
                CapitalInvestment = 90_000_000.0,
                ProjectedJobs = 4500,
                QuarterlyTaxContribution = 20_000_000.0,
                GdpContribution = 1_200_000_000.0,
                QuarterlyExportContribution = 160_000_000.0,
                StockMarketConfidenceBonus = 140.0,
                ForeignReservesInflow = 60_000_000.0
            },
            new()
            {
                Id = "biz_wave2_finance",
                BusinessName = "Sovereign Trust Financial Corp",
                IndustrialistName = "Managing Partner Vivienne Moreau",
                Industry = "Finance",
                Wave = GlobalBusinessWave.SecondWave,
                ScheduledDay = 90,
                PresentingMinister = GetMinisterTitle(CabinetPortfolio.Finance, "Minister of Finance"),
                ProposalTitle = "International Forex Clearinghouse & Securities Trading Center",
                Description = "Sovereign Trust charters a regional merchant banking and securities settlement headquarters. Anchors international foreign exchange trading directly in the sovereign currency.",
                CapitalInvestment = 80_000_000.0,
                ProjectedJobs = 3000,
                QuarterlyTaxContribution = 18_000_000.0,
                GdpContribution = 950_000_000.0,
                QuarterlyExportContribution = 70_000_000.0,
                StockMarketConfidenceBonus = 125.0,
                ForeignReservesInflow = 50_000_000.0
            },
            new()
            {
                Id = "biz_wave2_aerospace",
                BusinessName = "Orbital Dynamics Consortium",
                IndustrialistName = "Chief Architect David Sterling",
                Industry = "Aerospace & Defense",
                Wave = GlobalBusinessWave.SecondWave,
                ScheduledDay = 120,
                PresentingMinister = GetMinisterTitle(CabinetPortfolio.Defense, "Minister of Defense"),
                ProposalTitle = "Low-Earth Satellite Assembly & Commercial Avionics Center",
                Description = "Orbital Dynamics builds a certified aerospace component fabrication and satellite integration cleanroom, manufacturing Earth-observation arrays and commercial transponders.",
                CapitalInvestment = 85_000_000.0,
                ProjectedJobs = 3800,
                QuarterlyTaxContribution = 19_000_000.0,
                GdpContribution = 1_100_000_000.0,
                QuarterlyExportContribution = 140_000_000.0,
                StockMarketConfidenceBonus = 130.0,
                ForeignReservesInflow = 55_000_000.0
            },
            new()
            {
                Id = "biz_wave2_metallurgy",
                BusinessName = "Titan Heavy Minerals & Metallurgy",
                IndustrialistName = "Industrialist Baroness Alicia Thorne",
                Industry = "Heavy Industry",
                Wave = GlobalBusinessWave.SecondWave,
                ScheduledDay = 150,
                PresentingMinister = GetMinisterTitle(CabinetPortfolio.Resources, "Minister of Resources"),
                ProposalTitle = "Rare-Earth Refining & High-Capacity Smelting Complex",
                Description = "Titan Heavy Minerals commissions an advanced smelting and metallurgical processing plant to refine battery-grade lithium, copper, and titanium directly before ocean export.",
                CapitalInvestment = 100_000_000.0,
                ProjectedJobs = 7500,
                QuarterlyTaxContribution = 25_000_000.0,
                GdpContribution = 1_500_000_000.0,
                QuarterlyExportContribution = 200_000_000.0,
                StockMarketConfidenceBonus = 160.0,
                ForeignReservesInflow = 70_000_000.0
            }
        };
    }

    public IReadOnlyList<GlobalBusinessProposal> UpdateBusinessApproaches(
        Country country,
        RepublicTime simTime,
        bool forceUnlockAll = false)
    {
        ArgumentNullException.ThrowIfNull(country);

        var predefined = GetPredefinedProposals(country);
        long presidencyDays = country.PresidencyStartDay <= 1 ? simTime.DayNumber : Math.Max(0, simTime.DayNumber - country.PresidencyStartDay);

        long? currencyDays = null;
        if (country.CurrencyCreationDay.HasValue)
        {
            currencyDays = Math.Max(0, simTime.DayNumber - country.CurrencyCreationDay.Value);
        }

        foreach (var def in predefined)
        {
            // Check if already in country's list
            var existing = country.GlobalBusinesses.FirstOrDefault(b => b.Id == def.Id);
            if (existing != null)
            {
                continue;
            }

            bool shouldApproach = false;

            if (forceUnlockAll)
            {
                shouldApproach = true;
            }
            else if (def.Wave == GlobalBusinessWave.FirstWave)
            {
                // Wave 1: based on elapsed days of presidency (Day 30, 60, 90, 120, 150)
                shouldApproach = presidencyDays >= def.ScheduledDay;
            }
            else if (def.Wave == GlobalBusinessWave.SecondWave && currencyDays.HasValue)
            {
                // Wave 2: based on elapsed days since national currency creation (Day 30, 60, 90, 120, 150)
                shouldApproach = currencyDays.Value >= def.ScheduledDay;
            }

            if (shouldApproach)
            {
                var copy = new GlobalBusinessProposal
                {
                    Id = def.Id,
                    BusinessName = def.BusinessName,
                    IndustrialistName = def.IndustrialistName,
                    Industry = def.Industry,
                    Wave = def.Wave,
                    ScheduledDay = def.ScheduledDay,
                    PresentingMinister = def.PresentingMinister,
                    ProposalTitle = def.ProposalTitle,
                    Description = def.Description,
                    CapitalInvestment = def.CapitalInvestment,
                    ProjectedJobs = def.ProjectedJobs,
                    QuarterlyTaxContribution = def.QuarterlyTaxContribution,
                    GdpContribution = def.GdpContribution,
                    QuarterlyExportContribution = def.QuarterlyExportContribution,
                    StockMarketConfidenceBonus = def.StockMarketConfidenceBonus,
                    ForeignReservesInflow = def.ForeignReservesInflow,
                    Status = GlobalBusinessDecisionStatus.Pending
                };
                country.GlobalBusinesses.Add(copy);
            }
        }

        return country.GlobalBusinesses.AsReadOnly();
    }

    public bool AcceptProposal(Country country, string businessId, RepublicTime simTime, out string outcomeMessage)
    {
        ArgumentNullException.ThrowIfNull(country);

        var proposal = country.GlobalBusinesses.FirstOrDefault(b => string.Equals(b.Id, businessId, StringComparison.OrdinalIgnoreCase));
        if (proposal == null)
        {
            outcomeMessage = $"Global enterprise proposal '{businessId}' not found.";
            return false;
        }

        if (proposal.Status != GlobalBusinessDecisionStatus.Pending)
        {
            outcomeMessage = $"Proposal '{proposal.BusinessName}' has already been {proposal.Status.ToString().ToLowerInvariant()}.";
            return false;
        }

        proposal.Status = GlobalBusinessDecisionStatus.Accepted;
        proposal.DecidedOnDay = (int)simTime.DayNumber;
        proposal.DecisionMessage = "Authorized commercial operations by Presidential Decree.";

        // Realize tangible economic impacts
        // 1. Direct Foreign Investment (injected into Treasury & Foreign Reserves)
        country.Treasury.Deposit(proposal.CapitalInvestment);
        country.CentralBank.DepositForeignReserves(proposal.ForeignReservesInflow);

        // 2. Gross Domestic Product permanent boost
        country.GrossDomesticProduct += proposal.GdpContribution;

        // 3. Trade Balance export enhancement
        country.TradeBalance += proposal.QuarterlyExportContribution;

        // 4. Update macroeconomic cycle (synthesizes new tax revenue flow, inflation impact, and forex threshold)
        country.UpdateEconomicCycle(simTime);

        // 5. Stock Market confidence lift
        country.StockMarket.CurrentPoints += proposal.StockMarketConfidenceBonus;
        country.StockMarket.Direction = "Bullish ↗️";

        outcomeMessage = $"Authorized commercial entry of {proposal.BusinessName} ({proposal.Industry}). Capital investment of {RepuTreasury.Format(proposal.CapitalInvestment)} deposited. {proposal.ProjectedJobs:N0} direct jobs created. Ongoing quarterly tax revenue: {RepuTreasury.Format(proposal.QuarterlyTaxContribution)}.";
        return true;
    }

    public bool DeclineProposal(Country country, string businessId, RepublicTime simTime, out string outcomeMessage)
    {
        ArgumentNullException.ThrowIfNull(country);

        var proposal = country.GlobalBusinesses.FirstOrDefault(b => string.Equals(b.Id, businessId, StringComparison.OrdinalIgnoreCase));
        if (proposal == null)
        {
            outcomeMessage = $"Global enterprise proposal '{businessId}' not found.";
            return false;
        }

        if (proposal.Status != GlobalBusinessDecisionStatus.Pending)
        {
            outcomeMessage = $"Proposal '{proposal.BusinessName}' has already been {proposal.Status.ToString().ToLowerInvariant()}.";
            return false;
        }

        proposal.Status = GlobalBusinessDecisionStatus.Declined;
        proposal.DecidedOnDay = (int)simTime.DayNumber;
        proposal.DecisionMessage = "Turned away by Presidential decree.";

        // Minor opportunity cost: slight investor confidence cooling (-15 stock market points)
        country.UpdateEconomicCycle(simTime);
        country.StockMarket.CurrentPoints = Math.Max(500.0, country.StockMarket.CurrentPoints - 15.0);
        country.StockMarket.Direction = "Cooling ↘️";

        outcomeMessage = $"Declined commercial entry of {proposal.BusinessName}. The delegation has withdrawn without state concessions.";
        return true;
    }
}

