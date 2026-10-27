namespace Republic.Core.Media.Services;

using Republic.Core.Crises.Models;
using Republic.Core.Economy.Treasury;
using Republic.Core.Media.Models;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Authoritative implementation of the Republic media simulation.
/// Generates realistic procedural outlets, crafts reports reflecting live simulation events,
/// and processes presidential media-management actions with balanced trade-offs.
/// </summary>
public sealed class MediaSimulationService : IMediaSimulationService
{
    public const double StatementCost = 10_000.0;
    public const double HushMoneyCost = 150_000.0;

    public IReadOnlyList<MediaOutlet> EnsureCountryMediaOutlets(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        if (country.MediaOutlets.Count == 0)
        {
            country.MediaOutlets.AddRange(new[]
            {
                new MediaOutlet
                {
                    Id = "outlet_sovereign_gazette",
                    Name = $"{country.Name} Sovereign Gazette",
                    Leaning = MediaEditorialLeaning.StateAligned,
                    AudienceReach = 75.0,
                    Credibility = 65.0,
                    GovernmentAttitude = 40.0
                },
                new MediaOutlet
                {
                    Id = "outlet_criterion_dispatch",
                    Name = "The Criterion Dispatch",
                    Leaning = MediaEditorialLeaning.InvestigativeIndependent,
                    AudienceReach = 55.0,
                    Credibility = 92.0,
                    GovernmentAttitude = 0.0
                },
                new MediaOutlet
                {
                    Id = "outlet_capital_ledger",
                    Name = "The Capital Financial Ledger",
                    Leaning = MediaEditorialLeaning.FinancialBusiness,
                    AudienceReach = 45.0,
                    Credibility = 88.0,
                    GovernmentAttitude = 10.0
                },
                new MediaOutlet
                {
                    Id = "outlet_peoples_voice",
                    Name = "The People's Voice & Tribune",
                    Leaning = MediaEditorialLeaning.PopulistOpposition,
                    AudienceReach = 65.0,
                    Credibility = 50.0,
                    GovernmentAttitude = -35.0
                }
            });
        }

        return country.MediaOutlets.AsReadOnly();
    }

    public IReadOnlyList<MediaReport> GenerateMediaReports(Country country, RepublicTime simulationTime, bool applySimulationImpacts = true)
    {
        ArgumentNullException.ThrowIfNull(country);

        EnsureCountryMediaOutlets(country);

        var day = simulationTime.DayNumber;
        var newReports = new List<MediaReport>();

        // 1. Economic & Fiscal Reporting
        var finOutlet = country.MediaOutlets.FirstOrDefault(o => o.Leaning == MediaEditorialLeaning.FinancialBusiness) ?? country.MediaOutlets[0];
        var popOutlet = country.MediaOutlets.FirstOrDefault(o => o.Leaning == MediaEditorialLeaning.PopulistOpposition) ?? country.MediaOutlets[0];
        var stateOutlet = country.MediaOutlets.FirstOrDefault(o => o.Leaning == MediaEditorialLeaning.StateAligned) ?? country.MediaOutlets[0];
        var indepOutlet = country.MediaOutlets.FirstOrDefault(o => o.Leaning == MediaEditorialLeaning.InvestigativeIndependent) ?? country.MediaOutlets[0];

        if (country.Treasury.Balance < 1_000_000.0)
        {
            var repId = $"rep_fiscal_deficit_{day}";
            if (!country.MediaReports.Any(r => r.Id == repId))
            {
                newReports.Add(new MediaReport
                {
                    Id = repId,
                    OutletId = finOutlet.Id,
                    OutletName = finOutlet.Name,
                    Leaning = finOutlet.Leaning,
                    Headline = "Fiscal Deficit Warning: Sovereign Liquidity Pressures Mount",
                    ArticleSummary = $"Treasury balances (R {country.Treasury.Balance:N0}) have fallen near the emergency operating threshold. Financial analysts advise immediate spending restraint.",
                    Category = "Economic",
                    Sentiment = MediaReportSentiment.Critical,
                    EventSource = "treasury_deficit",
                    OutletCredibility = finOutlet.Credibility,
                    OutletReach = finOutlet.AudienceReach,
                    PublishedDay = day,
                    PublicApprovalImpact = -2.5,
                    StabilityImpact = -1.5
                });
            }

            var popRepId = $"rep_populist_austerity_{day}";
            if (!country.MediaReports.Any(r => r.Id == popRepId))
            {
                newReports.Add(new MediaReport
                {
                    Id = popRepId,
                    OutletId = popOutlet.Id,
                    OutletName = popOutlet.Name,
                    Leaning = popOutlet.Leaning,
                    Headline = "Empty Coffers: Working Families Fear Cuts as Reserves Drop",
                    ArticleSummary = "Civic organizers question executive fiscal management as municipal social programs face delays.",
                    Category = "Economic",
                    Sentiment = MediaReportSentiment.Sensationalized,
                    EventSource = "treasury_deficit",
                    OutletCredibility = popOutlet.Credibility,
                    OutletReach = popOutlet.AudienceReach,
                    PublishedDay = day,
                    PublicApprovalImpact = -3.5,
                    StabilityImpact = -2.0
                });
            }
        }
        else if (country.Treasury.Balance >= 5_000_000.0)
        {
            var repId = $"rep_fiscal_surplus_{day}";
            if (!country.MediaReports.Any(r => r.Id == repId))
            {
                newReports.Add(new MediaReport
                {
                    Id = repId,
                    OutletId = finOutlet.Id,
                    OutletName = finOutlet.Name,
                    Leaning = finOutlet.Leaning,
                    Headline = "Sovereign Treasury Bolstered: Robust Reserves Support Investment",
                    ArticleSummary = $"Liquid capital reserves reach R {country.Treasury.Balance:N0}, reinforcing national creditworthiness.",
                    Category = "Economic",
                    Sentiment = MediaReportSentiment.Positive,
                    EventSource = "treasury_surplus",
                    OutletCredibility = finOutlet.Credibility,
                    OutletReach = finOutlet.AudienceReach,
                    PublishedDay = day,
                    PublicApprovalImpact = 2.0,
                    StabilityImpact = 1.0
                });
            }
        }

        // 2. Infrastructure & Development Projects
        var completedProj = country.Projects.LastOrDefault(p => p.IsCompleted);
        if (completedProj != null)
        {
            var projRepId = $"rep_proj_completed_{completedProj.Id}_{day}";
            if (!country.MediaReports.Any(r => r.Id == projRepId))
            {
                newReports.Add(new MediaReport
                {
                    Id = projRepId,
                    OutletId = stateOutlet.Id,
                    OutletName = stateOutlet.Name,
                    Leaning = stateOutlet.Leaning,
                    Headline = $"National Progress: {completedProj.Name} Delivered to Republic",
                    ArticleSummary = $"Ceremonial ribbon-cutting marks the official completion of {completedProj.Name}. Officials praise executive stewardship.",
                    Category = "Infrastructure",
                    Sentiment = MediaReportSentiment.Positive,
                    EventSource = "project_completion",
                    OutletCredibility = stateOutlet.Credibility,
                    OutletReach = stateOutlet.AudienceReach,
                    PublishedDay = day,
                    PublicApprovalImpact = 3.5,
                    StabilityImpact = 2.0
                });
            }
        }

        // 3. Civil Order & Civic Grievances
        if (country.BaselineStability < 50.0 || country.HappinessRating < 45.0)
        {
            var unrestRepId = $"rep_civic_unrest_{day}";
            if (!country.MediaReports.Any(r => r.Id == unrestRepId))
            {
                newReports.Add(new MediaReport
                {
                    Id = unrestRepId,
                    OutletId = indepOutlet.Id,
                    OutletName = indepOutlet.Name,
                    Leaning = indepOutlet.Leaning,
                    Headline = "Civic Frustration Simmers: Polls Show Deepening Public Discontent",
                    ArticleSummary = $"Citizen happiness rating dropped to {country.HappinessRating:0.0}%. Independent observers urge transparent reform.",
                    Category = "Governance",
                    Sentiment = MediaReportSentiment.Critical,
                    EventSource = "civil_discontent",
                    OutletCredibility = indepOutlet.Credibility,
                    OutletReach = indepOutlet.AudienceReach,
                    PublishedDay = day,
                    PublicApprovalImpact = -3.0,
                    StabilityImpact = -2.5
                });
            }
        }

        // 4. Security Alerts
        var severeThreat = country.ActiveSecurityThreats.FirstOrDefault(t => t.Severity == CrisisSeverity.Severe || t.Severity == CrisisSeverity.Catastrophic);
        if (severeThreat != null)
        {
            var secRepId = $"rep_sec_alert_{severeThreat.ThreatId}_{day}";
            if (!country.MediaReports.Any(r => r.Id == secRepId))
            {
                newReports.Add(new MediaReport
                {
                    Id = secRepId,
                    OutletId = indepOutlet.Id,
                    OutletName = indepOutlet.Name,
                    Leaning = indepOutlet.Leaning,
                    Headline = $"Security Alert: {severeThreat.Title}",
                    ArticleSummary = $"National intelligence sources confirm heightened alert regarding: {severeThreat.Details}",
                    Category = "Security",
                    Sentiment = MediaReportSentiment.Critical,
                    EventSource = "security_threat",
                    OutletCredibility = indepOutlet.Credibility,
                    OutletReach = indepOutlet.AudienceReach,
                    PublishedDay = day,
                    PublicApprovalImpact = -2.0,
                    StabilityImpact = -2.0
                });
            }
        }

        // 5. Active Private Investment Partnerships
        var activeInvestment = country.ActivePrivateInvestments.LastOrDefault(p => p.IsActive);
        if (activeInvestment != null)
        {
            var invRepId = $"rep_active_invest_{activeInvestment.ProposalId}_{day}";
            if (!country.MediaReports.Any(r => r.Id == invRepId))
            {
                newReports.Add(new MediaReport
                {
                    Id = invRepId,
                    OutletId = finOutlet.Id,
                    OutletName = finOutlet.Name,
                    Leaning = finOutlet.Leaning,
                    Headline = $"Commercial Expansion: {activeInvestment.InvestorName} Advances Construction",
                    ArticleSummary = $"Public-private partnership in {activeInvestment.Sector} sector underway. Projected to create {activeInvestment.JobsCreated:N0} jobs.",
                    Category = "Investment",
                    Sentiment = MediaReportSentiment.Positive,
                    EventSource = "private_investment",
                    OutletCredibility = finOutlet.Credibility,
                    OutletReach = finOutlet.AudienceReach,
                    PublishedDay = day,
                    PublicApprovalImpact = 2.5,
                    StabilityImpact = 1.5
                });
            }
        }

        // Apply balanced impact to country stats for newly published unsuppressed reports
        foreach (var r in newReports)
        {
            if (applySimulationImpacts && !r.IsSuppressed)
            {
                var appDelta = Math.Clamp(r.PublicApprovalImpact * (r.OutletReach / 100.0) * (r.OutletCredibility / 100.0), -4.0, 4.0);
                var stabDelta = Math.Clamp(r.StabilityImpact * (r.OutletReach / 100.0) * (r.OutletCredibility / 100.0), -3.0, 3.0);
                country.HappinessRating = Math.Clamp(country.HappinessRating + appDelta, 0.0, 100.0);
                country.BaselineStability = Math.Clamp(country.BaselineStability + stabDelta, 0.0, 100.0);
            }
            country.MediaReports.Add(r);
        }

        return country.MediaReports.AsReadOnly();
    }

    public bool ApplyMediaResponse(Country country, string reportId, string responseType, out string outcomeMessage)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentException.ThrowIfNullOrWhiteSpace(reportId);

        var report = country.MediaReports.FirstOrDefault(r => r.Id == reportId);
        if (report == null)
        {
            outcomeMessage = $"Media report with ID '{reportId}' was not found in active publication records.";
            return false;
        }

        if (report.ResponseStatus != MediaResponseStatus.Unanswered)
        {
            outcomeMessage = $"A presidential response has already been enacted for this story ({report.ResponseStatus}).";
            return false;
        }

        var outlet = country.MediaOutlets.FirstOrDefault(o => o.Id == report.OutletId);

        switch (responseType.ToLowerInvariant())
        {
            case "statement":
            {
                if (country.Treasury.Balance < StatementCost)
                {
                    outcomeMessage = $"Treasury balance (R {country.Treasury.Balance:N0}) is insufficient for press conference logistics (R {StatementCost:N0}).";
                    return false;
                }

                country.Treasury.Withdraw(StatementCost);

                // Recover partial approval and enhance credibility
                report.ResponseStatus = MediaResponseStatus.StatementIssued;
                var recovery = Math.Abs(report.PublicApprovalImpact) * 0.5;
                country.HappinessRating = Math.Clamp(country.HappinessRating + recovery, 0.0, 100.0);
                country.BaselineStability = Math.Clamp(country.BaselineStability + 1.0, 0.0, 100.0);

                if (outlet != null)
                {
                    outlet.GovernmentAttitude = Math.Clamp(outlet.GovernmentAttitude + 5.0, -100.0, 100.0);
                }

                outcomeMessage = $"Executive press statement delivered regarding '{report.Headline}'. Address recovered public confidence (+{recovery:0.1}% approval).";
                return true;
            }

            case "hush_money":
            {
                if (country.Treasury.Balance < HushMoneyCost)
                {
                    outcomeMessage = $"Treasury balance (R {country.Treasury.Balance:N0}) cannot cover covert payment of R {HushMoneyCost:N0}.";
                    return false;
                }

                country.Treasury.Withdraw(HushMoneyCost);

                // Determine exposure probability based on outlet leaning and corruption control
                double corruptionControl = country.StateCapacity?.CorruptionControl ?? 0.3;
                double exposureChance = report.Leaning switch
                {
                    MediaEditorialLeaning.InvestigativeIndependent => 0.85,
                    MediaEditorialLeaning.PopulistOpposition => 0.50,
                    MediaEditorialLeaning.FinancialBusiness => 0.40,
                    _ => 0.15
                };

                // High state integrity/transparency increases chance of whistleblowers
                if (corruptionControl > 0.6)
                {
                    exposureChance = Math.Clamp(exposureChance + 0.15, 0.1, 0.95);
                }

                var randomSeed = Math.Abs(report.Id.GetHashCode()) % 100;
                var randomRoll = randomSeed / 100.0;
                bool wasExposed = randomRoll < exposureChance;

                if (wasExposed)
                {
                    report.ResponseStatus = MediaResponseStatus.HushMoneyExposed;
                    report.IsUndergroundLeak = true;

                    // Severe scandal fallout
                    country.HappinessRating = Math.Clamp(country.HappinessRating - 12.0, 0.0, 100.0);
                    country.BaselineStability = Math.Clamp(country.BaselineStability - 8.0, 0.0, 100.0);

                    // Add immediate scandal story
                    var scandalId = $"rep_scandal_hushmoney_{report.Id}";
                    if (!country.MediaReports.Any(r => r.Id == scandalId))
                    {
                        country.MediaReports.Insert(0, new MediaReport
                        {
                            Id = scandalId,
                            OutletId = outlet?.Id ?? "outlet_independent",
                            OutletName = outlet?.Name ?? "Press Corps",
                            Leaning = MediaEditorialLeaning.InvestigativeIndependent,
                            Headline = "EXECUTIVE BRIBERY EXPOSED: Secret Treasury Funds Offered to Silence Press",
                            ArticleSummary = $"Investigative journalists confirm the administration offered R {HushMoneyCost:N0} in hush money to suppress reporting on '{report.Headline}'. Public uproar erupts.",
                            Category = "Scandal",
                            Sentiment = MediaReportSentiment.Critical,
                            EventSource = "hush_money_exposed",
                            OutletCredibility = 99.0,
                            OutletReach = 85.0,
                            PublishedDay = report.PublishedDay,
                            PublicApprovalImpact = -12.0,
                            StabilityImpact = -8.0,
                            IsUndergroundLeak = true
                        });
                    }

                    outcomeMessage = "Disaster! The media outlet refused the covert payment and published a front-page exposé detailing executive bribery.";
                    return true;
                }
                else
                {
                    report.IsSuppressed = true;
                    report.ResponseStatus = MediaResponseStatus.HushMoneyAccepted;

                    // Nullify damage from original story
                    country.HappinessRating = Math.Clamp(country.HappinessRating + Math.Abs(report.PublicApprovalImpact) * 0.8, 0.0, 100.0);

                    outcomeMessage = $"Covert transaction completed (R {HushMoneyCost:N0}). Publication schedule modified to bury the story.";
                    return true;
                }
            }

            case "censorship":
            {
                report.IsSuppressed = true;
                report.ResponseStatus = MediaResponseStatus.Censored;

                if (outlet != null)
                {
                    outlet.IsCensored = true;
                    outlet.ConsecutiveStrikes++;
                    outlet.GovernmentAttitude = Math.Clamp(outlet.GovernmentAttitude - 25.0, -100.0, 100.0);
                }

                // Civil liberties and democratic stability hit
                country.HappinessRating = Math.Clamp(country.HappinessRating - 4.5, 0.0, 100.0);
                country.BaselineStability = Math.Clamp(country.BaselineStability - 3.5, 0.0, 100.0);

                // Underground samizdat leak possibility for non-state outlets
                if (report.Leaning != MediaEditorialLeaning.StateAligned)
                {
                    report.IsUndergroundLeak = true;
                    var leakId = $"rep_censorship_leak_{report.Id}";
                    if (!country.MediaReports.Any(r => r.Id == leakId))
                    {
                        country.MediaReports.Insert(0, new MediaReport
                        {
                            Id = leakId,
                            OutletId = "outlet_underground",
                            OutletName = "Underground Samizdat Network",
                            Leaning = MediaEditorialLeaning.PopulistOpposition,
                            Headline = $"BANNED DISPATCH: Censored Report Circulating on '{report.Headline}'",
                            ArticleSummary = "Executive gag orders failed to halt public dissemination. Copies of the banned report circulate across citizen networks.",
                            Category = "Governance",
                            Sentiment = MediaReportSentiment.Critical,
                            EventSource = "censorship_backlash",
                            OutletCredibility = 75.0,
                            OutletReach = 60.0,
                            PublishedDay = report.PublishedDay,
                            PublicApprovalImpact = -4.0,
                            StabilityImpact = -3.0,
                            IsUndergroundLeak = true
                        });
                    }
                }

                outcomeMessage = $"Emergency gag order issued against '{report.Headline}'. Official distribution blocked, but civil liberties concerns trigger domestic backlash.";
                return true;
            }

            default:
                outcomeMessage = $"Unrecognized media response type '{responseType}'. Supported options: statement, hush_money, censorship.";
                return false;
        }
    }
}
