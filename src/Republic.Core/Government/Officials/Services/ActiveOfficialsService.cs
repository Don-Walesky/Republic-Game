namespace Republic.Core.Government.Officials.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Republic.Core.Cabinet.Models;
using Republic.Core.Crises.Models;
using Republic.Core.Government.Officials.Models;
using Republic.Core.Military.Models;
using Republic.Core.NationalStrategy.Models;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Service implementation managing proactive government officials, Chief of Staff prioritization,
/// ministerial advice generation, and security leadership intelligence monitoring.
/// </summary>
public sealed class ActiveOfficialsService : IActiveOfficialsService
{
    public List<ChiefOfStaffRecommendation> EvaluateChiefOfStaffRecommendations(Country country, RepublicTime simulationTime)
    {
        ArgumentNullException.ThrowIfNull(country);

        country.ChiefOfStaffRecommendations ??= new List<ChiefOfStaffRecommendation>();
        var existingRecs = country.ChiefOfStaffRecommendations;

        // Mark previously pending recommendations as Unresolved if they spanned simulation days without player resolution
        foreach (var r in existingRecs.Where(r => r.Status == RecommendationStatus.Pending && r.CreatedDay < simulationTime.DayNumber))
        {
            r.Status = RecommendationStatus.Unresolved;
            r.IsUrgent = true;
        }

        var newCandidates = new List<ChiefOfStaffRecommendation>();

        // 1. Fiscal Conditions
        if (country.Treasury.Balance < 500_000.0)
        {
            newCandidates.Add(new ChiefOfStaffRecommendation
            {
                Id = "rec_fiscal_insolvency",
                Title = "Critical: Impending Fiscal Insolvency",
                Summary = $"Treasury balance (R {country.Treasury.Balance:N0}) has fallen below minimum state reserve threshold. Immediate austerity decree or emergency fiscal revenue measures required.",
                Category = "FiscalCrisis",
                Priority = RecommendationPriority.Critical,
                IsUrgent = true,
                RecommendedAction = "emergency_austerity",
                CreatedDay = simulationTime.DayNumber
            });
        }
        else if (country.Treasury.Balance > 20_000_000_000.0 && country.Projects.All(p => !p.IsActive))
        {
            newCandidates.Add(new ChiefOfStaffRecommendation
            {
                Id = "rec_capital_investment",
                Title = "Strategic Opportunity: Sovereign Capital Reinvestment",
                Summary = $"Treasury reserves are exceptionally liquid (R {country.Treasury.Balance:N0}) with zero active capital projects. Recommending national infrastructure expansion.",
                Category = "Infrastructure",
                Priority = RecommendationPriority.Medium,
                IsUrgent = false,
                RecommendedAction = "start_road_project",
                EstimatedCost = 500_000.0,
                CreatedDay = simulationTime.DayNumber
            });
        }

        // 2. Democratic Approval & Domestic Stability
        if (country.HappinessRating < 35.0)
        {
            newCandidates.Add(new ChiefOfStaffRecommendation
            {
                Id = "rec_civil_discontent",
                Title = "Urgent: Civil Discontent & Protest Alert",
                Summary = $"Public approval ({country.HappinessRating:0.#}%) has breached critical democratic stability limits. Authorize an emergency civic relief package or schedule a presidential address.",
                Category = "CivilOrder",
                Priority = RecommendationPriority.Critical,
                IsUrgent = true,
                RecommendedAction = "emergency_relief",
                EstimatedCost = 100_000.0,
                CreatedDay = simulationTime.DayNumber
            });
        }
        else if (country.BaselineStability < 40.0)
        {
            newCandidates.Add(new ChiefOfStaffRecommendation
            {
                Id = "rec_stability_erosion",
                Title = "High Priority: Institutional Stability Erosion",
                Summary = $"Baseline political stability ({country.BaselineStability:0.#}%) is dangerously strained. Reinforce civil security apparatus and mediate parliamentary faction demands.",
                Category = "Security",
                Priority = RecommendationPriority.High,
                IsUrgent = true,
                RecommendedAction = "stabilize_institutions",
                CreatedDay = simulationTime.DayNumber
            });
        }

        // 3. Cabinet Bottlenecks & Vacancies
        var finance = country.GetMinister(CabinetPortfolio.Finance);
        if (finance == null || !finance.IsAppointed)
        {
            newCandidates.Add(new ChiefOfStaffRecommendation
            {
                Id = "rec_vacant_finance",
                Title = "Administrative Bottleneck: Vacant Ministry of Finance",
                Summary = "Civil Service Quality is capped at emergency baseline (30.0%). Appoint a Finance Minister to lift national revenue yield efficiency.",
                Category = "Cabinet",
                Priority = RecommendationPriority.High,
                IsUrgent = true,
                RecommendedAction = "appoint_finance",
                CreatedDay = simulationTime.DayNumber
            });
        }

        var interior = country.GetMinister(CabinetPortfolio.Interior);
        if (interior == null || !interior.IsAppointed)
        {
            newCandidates.Add(new ChiefOfStaffRecommendation
            {
                Id = "rec_vacant_interior",
                Title = "Governance Vulnerability: Vacant Ministry of the Interior",
                Summary = "Corruption Control is capped at emergency baseline (30.0%). Appoint an Interior Minister to enforce public integrity and deter contraband.",
                Category = "Cabinet",
                Priority = RecommendationPriority.High,
                IsUrgent = true,
                RecommendedAction = "appoint_interior",
                CreatedDay = simulationTime.DayNumber
            });
        }

        var infra = country.GetMinister(CabinetPortfolio.Infrastructure);
        if (infra == null || !infra.IsAppointed)
        {
            newCandidates.Add(new ChiefOfStaffRecommendation
            {
                Id = "rec_vacant_infrastructure",
                Title = "Development Bottleneck: Vacant Infrastructure Ministry",
                Summary = "Infrastructure Condition is capped at baseline (30.0%). Appoint an Infrastructure Minister to oversee public works.",
                Category = "Cabinet",
                Priority = RecommendationPriority.Medium,
                IsUrgent = false,
                RecommendedAction = "appoint_infrastructure",
                CreatedDay = simulationTime.DayNumber
            });
        }

        // 4. Electoral Schedule
        long nextElectDay = country.LastElection != null ? country.LastElection.Boundary.DayNumber + 30 : simulationTime.DayNumber + 30;
        long daysUntilElection = nextElectDay - simulationTime.DayNumber;
        if (daysUntilElection is > 0 and <= 15 && country.RulingParty != null && country.RulingParty.Support < 45.0)
        {
            newCandidates.Add(new ChiefOfStaffRecommendation
            {
                Id = "rec_campaign_deficit",
                Title = "Electoral Warning: Campaign Deficit",
                Summary = $"National democratic elections are scheduled in {daysUntilElection} days, with ruling party support trailing at {country.RulingParty.Support:0.#}%. Authorize a national campaign rally.",
                Category = "Elections",
                Priority = RecommendationPriority.High,
                IsUrgent = true,
                RecommendedAction = "campaign_rally",
                CreatedDay = simulationTime.DayNumber
            });
        }

        // 5. National Strategic Direction Alignment
        var stratDir = country.StrategicDirection;
        var stratProfile = country.StrategyProfile;
        var stratProg = stratProfile?.ProgressIndex ?? 15.0;
        newCandidates.Add(new ChiefOfStaffRecommendation
        {
            Id = $"rec_strategy_{stratDir}",
            Title = $"National Priority: {stratDir.GetDisplayName()}",
            Summary = $"Current strategic progress is evaluated at {stratProg:0.#}%. Executive focus should center on {string.Join(", ", stratDir.GetPrimarySectors())} to solidify long-term sovereign goals.",
            Category = "NationalStrategy",
            Priority = stratProfile?.IsTransitioning == true ? RecommendationPriority.High : RecommendationPriority.Medium,
            IsUrgent = stratProfile?.IsTransitioning == true,
            RecommendedAction = $"advance_strategy_{stratDir.ToString().ToLowerInvariant()}",
            CreatedDay = simulationTime.DayNumber
        });

        // Deduplicate and integrate candidates with existing recommendations
        foreach (var cand in newCandidates)
        {
            var existing = existingRecs.FirstOrDefault(r =>
                r.Category == cand.Category &&
                r.RecommendedAction == cand.RecommendedAction &&
                (r.Status == RecommendationStatus.Pending || r.Status == RecommendationStatus.Unresolved));

            if (existing == null)
            {
                existingRecs.Add(cand);
            }
            else
            {
                // Refresh title & summary with latest live telemetry
                existing.Title = cand.Title;
                existing.Summary = cand.Summary;
                existing.Priority = cand.Priority;
                existing.IsUrgent = cand.IsUrgent;
            }
        }

        // Sort by priority descending (Critical > High > Medium > Routine)
        return existingRecs
            .OrderByDescending(r => r.Status == RecommendationStatus.Pending || r.Status == RecommendationStatus.Unresolved)
            .ThenByDescending(r => (int)r.Priority)
            .ToList();
    }

    public List<MinisterialAdvice> GenerateMinisterialAdvice(Country country, RepublicTime simulationTime)
    {
        ArgumentNullException.ThrowIfNull(country);

        var adviceList = new List<MinisterialAdvice>();
        var portfolios = Enum.GetValues<CabinetPortfolio>();

        foreach (var p in portfolios)
        {
            var minister = country.GetMinister(p);
            if (minister == null || !minister.IsAppointed)
            {
                adviceList.Add(new MinisterialAdvice
                {
                    Portfolio = p,
                    MinisterName = "Vacant (Unassigned)",
                    IsAppointed = false,
                    Headline = $"Ministry of {p}: Vacant Portfolio",
                    DetailedAdvice = $"No executive leadership assigned. State capacity and departmental coordination bounded at emergency baseline 30.0%.",
                    RiskLevel = "Elevated",
                    ConfidenceRating = 30.0,
                    CompetenceRating = 0.0,
                    LoyaltyRating = 0.0,
                    IntegrityRating = 0.0,
                    RecommendedAction = $"appoint_{p.ToString().ToLowerInvariant()}",
                    WarningNotes = "Institutional capacity bottleneck active."
                });
                continue;
            }

            var comp = minister.Competence * 100.0;
            var loy = minister.Loyalty * 100.0;
            var integ = minister.Integrity * 100.0;
            var exp = minister.Experience * 100.0;

            string headline;
            string details;
            string risk = "Low";
            bool isSelfServing = false;
            string warning = string.Empty;
            string action = string.Empty;
            double cost = 0.0;

            switch (p)
            {
                case CabinetPortfolio.Finance:
                    if (integ < 40.0)
                    {
                        headline = "Discretionary Treasury Reallocation";
                        details = $"Minister {minister.Name} proposes redirecting surplus revenues into unmonitored ministerial accounts. Intelligence monitors note severe misappropriation hazard.";
                        risk = "Elevated";
                        isSelfServing = true;
                        warning = "Integrity warning: High fiscal leakage probability.";
                        action = "audit_finance";
                    }
                    else if (country.Treasury.Balance < 1_000_000.0)
                    {
                        headline = "Fiscal Austerity & Reserve Conservation";
                        details = $"Treasury balance ({country.Treasury.Balance:N0} REPU) demands immediate consolidation. Recommend freezing non-essential department outlays.";
                        risk = "High";
                        action = "emergency_austerity";
                    }
                    else
                    {
                        headline = "Optimized Revenue Trajectory";
                        details = $"Civil service efficiency ({comp:0.#}%) supports sustained fiscal stability. Tax receipts yield steady operational surplus.";
                        risk = "Low";
                        action = "maintain_rates";
                    }
                    break;

                case CabinetPortfolio.Defense:
                    if (loy < 40.0)
                    {
                        headline = "Military Command Friction";
                        details = $"Minister {minister.Name} exhibits resistance to civilian executive directives. Discontent noted among senior general staff.";
                        risk = "Elevated";
                        isSelfServing = true;
                        warning = "Loyalty alert: Factional military dissent possible.";
                        action = "review_military_command";
                    }
                    else if (country.BaselineStability < 50.0)
                    {
                        headline = "Heightened Security Standby Recommended";
                        details = "Regional civil disturbance risks warrant placing rapid-deployment armed forces on heightened standby.";
                        risk = "Moderate";
                        action = "heighten_readiness";
                    }
                    else
                    {
                        headline = "Frontier Readiness Assured";
                        details = $"Defense readiness index ({comp:0.#}%) maintains sovereign deterrence. Border sectors report peacetime stability.";
                        risk = "Low";
                        action = "sustain_defcon5";
                    }
                    break;

                case CabinetPortfolio.Interior:
                    if (integ < 40.0)
                    {
                        headline = "Customs Oversight Compromised";
                        details = $"Interior reports suggest lax customs enforcement. Minister {minister.Name} downplays contraband reports.";
                        risk = "High";
                        isSelfServing = true;
                        warning = "Integrity alert: Potential illicit smuggling facilitation.";
                        action = "order_anti_corruption_audit";
                    }
                    else if (country.HappinessRating < 40.0)
                    {
                        headline = "Urban Civil Order Precaution";
                        details = "Civilian demonstration sentiment elevated. Interior police forces prepared for proportional non-lethal crowd management.";
                        risk = "Moderate";
                        action = "deploy_civil_guards";
                    }
                    else
                    {
                        headline = "Domestic Order Stable";
                        details = $"Corruption control enforced at {integ:0.#}%. Law enforcement and judicial services operating smoothly.";
                        risk = "Low";
                        action = "routine_patrols";
                    }
                    break;

                case CabinetPortfolio.ForeignAffairs:
                    if (country.TradeBalance < 0)
                    {
                        headline = "Trade Deficit Negotiations";
                        details = "Bilateral trade corridors demonstrate net capital outflow. Recommending foreign trade agreement reviews.";
                        risk = "Moderate";
                        action = "bilateral_trade_summit";
                    }
                    else
                    {
                        headline = "Diplomatic Bilateral Outreach";
                        details = "International standing is respected. Proposing expanded sovereign diplomatic pacts and cultural exchanges.";
                        risk = "Low";
                        action = "propose_diplomatic_pact";
                    }
                    break;

                case CabinetPortfolio.Health:
                    if (country.HappinessRating < 50.0)
                    {
                        headline = "Public Welfare & Healthcare Support";
                        details = "Public welfare demands urgent healthcare modernization. Proposing targeted clinic resource distribution.";
                        risk = "Moderate";
                        action = "expand_public_clinics";
                        cost = 100_000.0;
                    }
                    else
                    {
                        headline = "Public Health Standards Sound";
                        details = "Hospital networks and emergency medical reserves report adequate capacity across all provinces.";
                        risk = "Low";
                        action = "routine_health_monitoring";
                    }
                    break;

                case CabinetPortfolio.Infrastructure:
                    var activeProj = country.Projects.FirstOrDefault(p => p.IsActive);
                    if (activeProj != null)
                    {
                        headline = $"Capital Progress: {activeProj.Name}";
                        details = $"Project proceeding on schedule under ministerial supervision ({exp:0.#}% experience rating).";
                        risk = "Low";
                        action = "monitor_project";
                    }
                    else
                    {
                        headline = "Transit Infrastructure Program Ready";
                        details = "Civil works departments have surveyed proposed national transport corridors. Ready for executive commissioning.";
                        risk = "Low";
                        action = "start_road_project";
                        cost = 500_000.0;
                    }
                    break;

                default:
                    headline = $"Ministry of {p} Review";
                    details = "Standard administrative operations proceeding according to constitutional mandates.";
                    break;
            }

            if (loy < 35.0 && !warning.Contains("Disloyalty Alert", StringComparison.OrdinalIgnoreCase))
            {
                warning = string.IsNullOrEmpty(warning)
                    ? "Disloyalty Alert: Minister exhibits factional dissent or resistance to presidential oversight."
                    : $"{warning} | Disloyalty Alert: Minister exhibits factional dissent.";
            }
            if (integ < 35.0 && !warning.Contains("Integrity Risk", StringComparison.OrdinalIgnoreCase))
            {
                warning = string.IsNullOrEmpty(warning)
                    ? "Integrity Risk: Reports indicate potential kickbacks or conflict of interest."
                    : $"{warning} | Integrity Risk: Reports indicate potential conflict of interest.";
            }

            adviceList.Add(new MinisterialAdvice
            {
                Portfolio = p,
                MinisterName = minister.Name,
                IsAppointed = true,
                Headline = headline,
                DetailedAdvice = details,
                RiskLevel = risk,
                ConfidenceRating = Math.Clamp((comp * 0.5) + (exp * 0.3) + (integ * 0.2), 20.0, 99.0),
                CompetenceRating = comp,
                LoyaltyRating = loy,
                IntegrityRating = integ,
                RecommendedAction = action,
                ActionCost = cost,
                IsSelfServing = isSelfServing,
                WarningNotes = warning
            });
        }

        country.LatestMinisterialAdvice = adviceList;
        return adviceList;
    }

    public SecurityLeadershipAssessment EvaluateSecurityLeadership(Country country, RepublicTime simulationTime)
    {
        ArgumentNullException.ThrowIfNull(country);

        double corrControl = country.StateCapacity?.CorruptionControl ?? 0.3;
        double domesticThreat = Math.Clamp(
            ((100.0 - country.HappinessRating) * 0.4) +
            ((100.0 - country.BaselineStability) * 0.4) +
            ((1.0 - corrControl) * 20.0),
            0.0,
            100.0);

        double externalThreat = Math.Clamp(
            (country.TradeBalance < 0 ? 30.0 : 10.0) +
            (country.Treasury.Balance < 1_000_000.0 ? 30.0 : 5.0) +
            (country.BaselineStability < 50.0 ? 25.0 : 0.0),
            0.0,
            100.0);

        var def = country.GetMinister(CabinetPortfolio.Defense);
        var defName = def?.Name ?? "Vacant";
        double compReadiness = def != null && def.IsAppointed
            ? Math.Clamp(def.Competence * 100.0, 40.0, 95.0)
            : 85.0;

        var threats = new List<SecurityThreatReport>();

        // 1. Domestic Unrest Threat (Confirmed)
        if (country.HappinessRating < 40.0)
        {
            threats.Add(new SecurityThreatReport
            {
                ThreatId = "threat_domestic_unrest",
                Title = "Urban Civil Unrest & Strike Mobilization",
                Category = "DomesticSecurity",
                Severity = CrisisSeverity.Severe,
                Confidence = IntelligenceConfidence.Confirmed,
                SourceAgency = "Special Branch (Interior)",
                Status = "Active",
                Details = "Multiple union blocs and citizen committees coordinating synchronized public demonstrations across metropolitan centers.",
                RecommendedResponse = "Deploy Civil Guard & Authorize Civic Relief",
                ReportedDay = simulationTime.DayNumber
            });
        }

        // 2. Contraband & Customs Leakages (High Probability)
        if ((country.StateCapacity?.CorruptionControl ?? 0.3) < 0.40)
        {
            threats.Add(new SecurityThreatReport
            {
                ThreatId = "threat_customs_contraband",
                Title = "Contraband Flow & Border Tariff Leakages",
                Category = "EconomicSabotage",
                Severity = CrisisSeverity.Moderate,
                Confidence = IntelligenceConfidence.HighProbability,
                SourceAgency = "National Intelligence Service",
                Status = "Active",
                Details = "Unmonitored transit corridors reporting unexplained cargo discrepancies. Estimated revenue leakage at R 500,000 per fiscal cycle.",
                RecommendedResponse = "Order Anti-Smuggling Customs Sweep",
                ReportedDay = simulationTime.DayNumber
            });
        }

        // 3. Foreign Signals / Unverified Rumors
        if (country.BaselineStability < 60.0)
        {
            threats.Add(new SecurityThreatReport
            {
                ThreatId = "threat_frontier_signals",
                Title = "Intercepted Signals Regarding Frontier Incursions",
                Category = "ForeignInfiltration",
                Severity = CrisisSeverity.Moderate,
                Confidence = IntelligenceConfidence.InterceptedSignal,
                SourceAgency = "Military Intelligence Division",
                Status = "Active",
                Details = "Radio communications intercepted along frontier sectors suggesting foreign proxy reconnaissance. Verification ongoing.",
                RecommendedResponse = "Authorize Aerial Reconnaissance Sweep",
                ReportedDay = simulationTime.DayNumber
            });
        }
        else
        {
            threats.Add(new SecurityThreatReport
            {
                ThreatId = "threat_basin_smuggling",
                Title = "Unverified Smuggling Rumors in Southern Basin",
                Category = "BorderSecurity",
                Severity = CrisisSeverity.Minor,
                Confidence = IntelligenceConfidence.UnverifiedRumor,
                SourceAgency = "Regional Coast Watch",
                Status = "Active",
                Details = "Informants report irregular nighttime maritime vessel activity. Zero hard military movement confirmed.",
                RecommendedResponse = "Routine Maritime Patrols",
                ReportedDay = simulationTime.DayNumber
            });
        }

        country.ActiveSecurityThreats = threats;

        string summary = domesticThreat > 50.0 || externalThreat > 50.0
            ? $"DEFCON Status Alert: Heightened domestic or frontier tensions detected (Domestic Threat: {domesticThreat:0.#}%, External: {externalThreat:0.#}%)."
            : "Sovereign frontiers secure. Domestic order and armed forces peacetime readiness maintained.";

        var assessment = new SecurityLeadershipAssessment
        {
            Defcon = domesticThreat > 60.0 ? DefconLevel.Defcon3_AirForceStandby : DefconLevel.Defcon5_Peace,
            CompositeReadiness = compReadiness,
            DomesticThreatIndex = domesticThreat,
            ExternalThreatIndex = externalThreat,
            DefenseMinisterName = defName,
            AssessmentSummary = summary,
            Threats = threats
        };

        country.LatestSecurityAssessment = assessment;
        return assessment;
    }

    public bool ApplyRecommendationDecision(Country country, string recommendationId, string decision, out string outcomeMessage)
    {
        ArgumentNullException.ThrowIfNull(country);
        country.ChiefOfStaffRecommendations ??= new List<ChiefOfStaffRecommendation>();

        var rec = country.ChiefOfStaffRecommendations.FirstOrDefault(r => r.Id == recommendationId);
        if (rec == null)
        {
            var launchEpoch = new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));
            var evaluated = EvaluateChiefOfStaffRecommendations(country, new RepublicTime(DateTimeOffset.UtcNow, launchEpoch));
            rec = evaluated.FirstOrDefault(r => r.Id == recommendationId);
        }

        if (rec == null)
        {
            outcomeMessage = $"Recommendation with ID '{recommendationId}' not found.";
            return false;
        }

        var normDec = decision.Trim().ToLowerInvariant();
        if (normDec is "accept" or "accepted" or "confirm" or "enact")
        {
            rec.Status = RecommendationStatus.Accepted;
            rec.OutcomeMessage = $"Accepted by President. Action '{rec.RecommendedAction}' approved.";
            outcomeMessage = rec.OutcomeMessage;

            // Execute immediate associated action if applicable
            switch (rec.RecommendedAction.ToLowerInvariant())
            {
                case "emergency_relief":
                    if (country.Treasury.Balance >= 100_000.0)
                    {
                        country.Treasury.Withdraw(100_000.0);
                        country.HappinessRating = Math.Clamp(country.HappinessRating + 4.0, 0.0, 100.0);
                        country.BaselineStability = Math.Clamp(country.BaselineStability + 3.0, 0.0, 100.0);
                        outcomeMessage += " Civic relief funds deployed (+4.0% approval, +3.0% stability).";
                    }
                    break;

                case "emergency_austerity":
                    country.Treasury.Deposit(500_000.0);
                    country.HappinessRating = Math.Clamp(country.HappinessRating - 4.0, 0.0, 100.0);
                    country.BaselineStability = Math.Clamp(country.BaselineStability - 2.0, 0.0, 100.0);
                    outcomeMessage += " Emergency austerity enacted (+R 500,000 treasury, -4.0% approval).";
                    break;

                case "campaign_rally":
                    if (country.RulingParty != null)
                    {
                        country.RulingParty = country.RulingParty with { Support = Math.Clamp(country.RulingParty.Support + 3.5, 0.0, 100.0) };
                        outcomeMessage += " National campaign rally mobilized (+3.5% polling support).";
                    }
                    break;
            }

            return true;
        }

        if (normDec is "reject" or "rejected" or "dismiss")
        {
            rec.Status = RecommendationStatus.Rejected;
            rec.OutcomeMessage = "Rejected by Presidential decree.";
            outcomeMessage = rec.OutcomeMessage;
            return true;
        }

        if (normDec is "defer" or "delay" or "postpone")
        {
            rec.Status = RecommendationStatus.Pending;
            rec.OutcomeMessage = "Deferred for future executive review.";
            outcomeMessage = rec.OutcomeMessage;
            return true;
        }

        outcomeMessage = $"Unknown decision '{decision}'. Expected accept, reject, or defer.";
        return false;
    }

    public bool ApplySecurityDirective(Country country, string threatId, string directive, out string outcomeMessage)
    {
        ArgumentNullException.ThrowIfNull(country);
        country.ActiveSecurityThreats ??= new List<SecurityThreatReport>();

        var threat = country.ActiveSecurityThreats.FirstOrDefault(t => t.ThreatId == threatId);
        if (threat == null)
        {
            var launchEpoch = new DateTimeOffset(2027, 2, 10, 0, 0, 0, TimeSpan.FromHours(1));
            EvaluateSecurityLeadership(country, new RepublicTime(DateTimeOffset.UtcNow, launchEpoch));
            threat = country.ActiveSecurityThreats.FirstOrDefault(t => t.ThreatId == threatId);
        }

        if (threat == null)
        {
            outcomeMessage = $"Security threat with ID '{threatId}' not found.";
            return false;
        }

        var normDir = directive.Trim().ToLowerInvariant();
        threat.Status = "Mitigated";

        switch (normDir)
        {
            case "mobilize" or "deploy":
                country.BaselineStability = Math.Clamp(country.BaselineStability + 2.0, 0.0, 100.0);
                outcomeMessage = $"Security directive executed: Security units mobilized against '{threat.Title}'. Threat mitigated (+2.0% stability).";
                return true;

            case "sweep" or "investigate":
                country.StateCapacity.CorruptionControl = Math.Clamp((country.StateCapacity?.CorruptionControl ?? 0.3) + 0.05, 0.0, 1.0);
                outcomeMessage = $"Intelligence sweep conducted: Customs and signal monitoring reinforced against '{threat.Title}'. Threat neutralized (+5.0% corruption control).";
                return true;

            case "deescalate" or "monitor":
                outcomeMessage = $"Security directive: Passive reconnaissance maintained on '{threat.Title}'.";
                return true;

            default:
                threat.Status = "Active";
                outcomeMessage = $"Unknown security directive '{directive}'. Threat remains active.";
                return false;
        }
    }
}
