namespace Republic.Core.NationalStrategy.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Republic.Core.Diagnostics;
using Republic.Core.NationalStrategy.Models;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Authoritative implementation of the <see cref="INationalStrategyService"/> governing long-term strategic direction,
/// real administrative transition costs, political friction, and empirical progress measurement.
/// </summary>
public sealed class NationalStrategyService : INationalStrategyService
{
    private readonly ILogger? _logger;
    private readonly double _transitionCost;
    private readonly int _cooldownDays;
    private readonly int _transitionFrictionDays;

    public NationalStrategyService(
        ILogger? logger = null,
        double transitionCost = INationalStrategyService.DefaultTransitionCost,
        int cooldownDays = INationalStrategyService.DefaultCooldownDays,
        int transitionFrictionDays = INationalStrategyService.DefaultTransitionFrictionDays)
    {
        _logger = logger;
        _transitionCost = transitionCost;
        _cooldownDays = cooldownDays;
        _transitionFrictionDays = transitionFrictionDays;
    }

    /// <inheritdoc />
    public NationalStrategyProfile EnsureStrategyProfile(Country country, RepublicTime? simulationTime = null)
    {
        ArgumentNullException.ThrowIfNull(country);

        if (country.StrategyProfile == null)
        {
            var day = simulationTime?.DayNumber ?? 1;
            var dir = country.StrategicDirection != NationalStrategicDirection.BalancedGrowth
                ? country.StrategicDirection
                : NationalStrategicDirection.Industrialisation; // Standard sovereign default

            country.StrategicDirection = dir;
            country.StrategyProfile = new NationalStrategyProfile
            {
                Direction = dir,
                AdoptedDay = 0,
                TransitionDaysRemaining = 0,
                ProgressIndex = 15.0,
                FocusStatement = dir.GetDescription(),
                PrioritySectors = dir.GetPrimarySectors().ToList()
            };
        }

        return country.StrategyProfile;
    }

    /// <inheritdoc />
    public (bool Success, string Message, NationalStrategyProfile Profile) SetStrategicDirection(
        Country country,
        NationalStrategicDirection newDirection,
        RepublicTime simulationTime,
        bool bypassFriction = false)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(simulationTime);

        var profile = EnsureStrategyProfile(country, simulationTime);

        if (profile.Direction == newDirection && !profile.IsTransitioning)
        {
            return (false, $"The Republic is already actively pursuing {newDirection.GetDisplayName()}.", profile);
        }

        // Cooldown enforcement
        if (!bypassFriction && profile.AdoptedDay > 0)
        {
            var daysSinceAdoption = simulationTime.DayNumber - profile.AdoptedDay;
            if (daysSinceAdoption < _cooldownDays)
            {
                var remaining = _cooldownDays - daysSinceAdoption;
                return (false,
                    $"Cooldown active: Strategic direction cannot be altered yet. Current policy orientation must remain locked for {remaining} more day(s) to maintain administrative stability.",
                    profile);
            }
        }

        // Financial cost enforcement
        if (!bypassFriction)
        {
            if (country.Treasury.Balance < _transitionCost)
            {
                return (false,
                    $"Insufficient sovereign treasury. Real administrative retooling requires R {_transitionCost:N0}, but treasury holds R {country.Treasury.Balance:N0}.",
                    profile);
            }

            country.Treasury.Withdraw(_transitionCost);
            profile.TransitionCostIncurred += _transitionCost;

            // Apply political friction: policy shifts disrupt established interests
            country.BaselineStability = Math.Max(10.0, country.BaselineStability - 5.0);
            country.HappinessRating = Math.Max(10.0, country.HappinessRating - 4.0);
        }

        var prev = profile.Direction;
        profile.PreviousDirection = prev;
        profile.Direction = newDirection;
        country.StrategicDirection = newDirection;
        profile.AdoptedDay = simulationTime.DayNumber;
        profile.TransitionDaysRemaining = bypassFriction ? 0 : _transitionFrictionDays;
        profile.PrioritySectors = newDirection.GetPrimarySectors().ToList();
        profile.FocusStatement = newDirection.GetDescription();

        // Calculate initial empirical progress baseline for new direction
        profile.ProgressIndex = CalculateEmpiricalProgress(country);

        var msg = bypassFriction
            ? $"Strategic direction set to {newDirection.GetDisplayName()}."
            : $"National strategic direction shifted from {prev.GetDisplayName()} to {newDirection.GetDisplayName()}. R {_transitionCost:N0} expended on retooling; initial administrative friction applied (-5% stability, -4% approval).";

        _logger?.LogInfo($"[NationalStrategy] {country.Name}: {msg}");
        return (true, msg, profile);
    }

    /// <inheritdoc />
    public double GetAlignmentMultiplier(Country country, string sectorOrCategory)
    {
        ArgumentNullException.ThrowIfNull(country);
        if (string.IsNullOrWhiteSpace(sectorOrCategory)) return 1.0;

        var profile = EnsureStrategyProfile(country);
        var sectors = profile.PrioritySectors;

        foreach (var s in sectors)
        {
            if (sectorOrCategory.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                s.Contains(sectorOrCategory, StringComparison.OrdinalIgnoreCase))
            {
                // Active alignment bonus
                return profile.IsTransitioning ? 1.25 : 1.5;
            }
        }

        // Misaligned or non-priority sector
        return 0.85;
    }

    /// <inheritdoc />
    public double UpdateStrategicProgress(Country country, RepublicTime simulationTime)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(simulationTime);

        var profile = EnsureStrategyProfile(country, simulationTime);

        // Advance transition days if transitioning
        if (profile.TransitionDaysRemaining > 0)
        {
            profile.TransitionDaysRemaining = Math.Max(0, profile.TransitionDaysRemaining - 1);
        }

        var empirical = CalculateEmpiricalProgress(country);
        profile.ProgressIndex = Math.Round(empirical, 1);
        return profile.ProgressIndex;
    }

    private static double CalculateEmpiricalProgress(Country country)
    {
        var dir = country.StrategicDirection;
        double progress = 10.0; // Baseline starting capacity

        switch (dir)
        {
            case NationalStrategicDirection.Industrialisation:
            {
                // Driven by completed factories/mining/heavy infrastructure + iron/steel inventory + GDP
                var industrialProjects = country.Projects?.Count(p => p.Completed &&
                    (p.Name.Contains("Mine", StringComparison.OrdinalIgnoreCase) ||
                     p.Name.Contains("Factory", StringComparison.OrdinalIgnoreCase) ||
                     p.Name.Contains("Industrial", StringComparison.OrdinalIgnoreCase) ||
                     p.Name.Contains("Road", StringComparison.OrdinalIgnoreCase) ||
                     p.Name.Contains("Port", StringComparison.OrdinalIgnoreCase))) ?? 0;

                var metalStockpile = country.GetResourceQuantity("Iron") + country.GetResourceQuantity("Steel");
                var gdpFactor = Math.Min(30.0, (country.GrossDomesticProduct / 100_000_000_000.0) * 10.0);

                progress = 15.0 + (industrialProjects * 12.0) + Math.Min(25.0, metalStockpile / 100.0) + gdpFactor;
                break;
            }

            case NationalStrategicDirection.ScienceAndTechnology:
            {
                // Driven by Cyber readiness + Science ministry budget + state capacity civil service quality
                var cyberReadiness = country.StateCapacity?.CivilServiceQuality ?? 0.3;
                var scienceBudget = country.MinistryBudget?.ScienceAllocation ?? 250_000_000.0;
                var scienceFactor = Math.Min(35.0, (scienceBudget / 50_000_000.0) * 5.0);

                var techInvestments = country.ActivePrivateInvestments?.Count(i => i.IsActive &&
                    (i.Name.Contains("Tech", StringComparison.OrdinalIgnoreCase) ||
                     i.Name.Contains("Research", StringComparison.OrdinalIgnoreCase) ||
                     i.Name.Contains("Cyber", StringComparison.OrdinalIgnoreCase) ||
                     i.Sector.Contains("Technology", StringComparison.OrdinalIgnoreCase))) ?? 0;

                progress = 10.0 + (cyberReadiness * 30.0) + scienceFactor + (techInvestments * 15.0);
                break;
            }

            case NationalStrategicDirection.EnergyIndependence:
            {
                // Driven by petroleum/gas stockpiles + power/energy resource nodes + energy facilities
                var petroleumStock = country.GetResourceQuantity("Petroleum") + country.GetResourceQuantity("NaturalGas");
                var energyNodes = country.ResourceNodes?.Count(n =>
                    string.Equals(n.ResourceType, "Petroleum", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(n.ResourceType, "NaturalGas", StringComparison.OrdinalIgnoreCase)) ?? 0;

                var energyInvestments = country.ActivePrivateInvestments?.Count(i => i.IsActive &&
                    (i.Name.Contains("Refinery", StringComparison.OrdinalIgnoreCase) ||
                     i.Name.Contains("Power", StringComparison.OrdinalIgnoreCase) ||
                     i.Name.Contains("Energy", StringComparison.OrdinalIgnoreCase) ||
                     i.Name.Contains("Fuel", StringComparison.OrdinalIgnoreCase) ||
                     i.Sector.Contains("Energy", StringComparison.OrdinalIgnoreCase))) ?? 0;

                progress = 10.0 + Math.Min(35.0, petroleumStock / 80.0) + (energyNodes * 12.0) + (energyInvestments * 15.0);
                break;
            }

            case NationalStrategicDirection.HumanDevelopment:
            {
                // Driven by citizen approval rating + civil service quality + Health & Education budget
                var approval = country.ApprovalRating;
                var civilService = country.StateCapacity?.CivilServiceQuality ?? 0.3;
                var socialBudget = (country.MinistryBudget?.HealthcareAllocation ?? 400_000_000.0) + (country.MinistryBudget?.EducationAllocation ?? 350_000_000.0);
                var budgetScore = Math.Min(25.0, (socialBudget / 100_000_000.0) * 3.0);

                progress = (approval * 0.4) + (civilService * 25.0) + budgetScore;
                break;
            }

            case NationalStrategicDirection.InfrastructureDevelopment:
            {
                // Driven by infrastructure condition + completed transportation/logistics projects
                var infraCond = country.StateCapacity?.InfrastructureCondition ?? 0.3;
                var infraBudget = country.MinistryBudget?.InfrastructureAllocation ?? 600_000_000.0;
                var infraProjects = country.Projects?.Count(p => p.Completed &&
                    (p.Name.Contains("Road", StringComparison.OrdinalIgnoreCase) ||
                     p.Name.Contains("Rail", StringComparison.OrdinalIgnoreCase) ||
                     p.Name.Contains("Bridge", StringComparison.OrdinalIgnoreCase) ||
                     p.Name.Contains("Port", StringComparison.OrdinalIgnoreCase))) ?? 0;

                progress = (infraCond * 40.0) + (infraProjects * 15.0) + Math.Min(20.0, (infraBudget / 100_000_000.0) * 3.0);
                break;
            }

            case NationalStrategicDirection.RegionalInfluence:
            {
                // Driven by diplomatic standing + trade agreements + military readiness + foreign intelligence
                var treaties = country.Agreements?.Count(a => a.Status is Republic.Core.World.DiplomaticAgreementStatus.Accepted) ?? 0;
                var tradeContracts = country.FlexibleTradeProposals?.Count(t => t.Status is Republic.Core.World.FlexibleTradeStatus.Accepted or Republic.Core.World.FlexibleTradeStatus.Completed) ?? 0;
                var militaryReadiness = country.BaselineStability; // fallback baseline

                progress = 15.0 + (treaties * 10.0) + (tradeContracts * 8.0) + Math.Min(25.0, militaryReadiness * 0.25);
                break;
            }

            default:
            {
                progress = (country.BaselineStability * 0.3) + (country.ApprovalRating * 0.3) +
                           Math.Min(40.0, (country.Treasury.Balance / 100_000_000.0) * 5.0);
                break;
            }
        }

        return Math.Clamp(progress, 5.0, 100.0);
    }
}
