namespace Republic.Core.Economy.PrivateSector.Services;

using Republic.Core.Economy.PrivateSector.Models;
using Republic.Core.Media.Models;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Authoritative implementation of the Republic private sector investment service.
/// Coordinates corporate investor profiles, inbound commercial proposals, presidential decision validation,
/// treasury disbursement, project registration, and completion realization.
/// </summary>
public sealed class PrivateSectorService : IPrivateSectorService
{
    public IReadOnlyList<PrivateInvestor> EnsureCountryPrivateInvestors(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        if (country.PrivateInvestors.Count == 0)
        {
            country.PrivateInvestors.AddRange(new[]
            {
                new PrivateInvestor
                {
                    Id = "investor_vanguard_steel",
                    Name = "Vanguard Heavy Industries & Steel",
                    Sector = "Manufacturing",
                    InvestorType = "DomesticConglomerate",
                    CapitalCapacity = 4_000_000.0,
                    RiskTolerance = InvestorRiskTolerance.Conservative,
                    GovernmentRelationship = 15.0
                },
                new PrivateInvestor
                {
                    Id = "investor_solaria_energy",
                    Name = "Solaria Clean Energy Consortium",
                    Sector = "Energy",
                    InvestorType = "ForeignDirectInvestor",
                    CapitalCapacity = 6_000_000.0,
                    RiskTolerance = InvestorRiskTolerance.Moderate,
                    GovernmentRelationship = 10.0
                },
                new PrivateInvestor
                {
                    Id = "investor_aethelgard_agro",
                    Name = "Aethelgard Agro-Industrialists",
                    Sector = "Agriculture",
                    InvestorType = "DomesticAgribusiness",
                    CapitalCapacity = 2_500_000.0,
                    RiskTolerance = InvestorRiskTolerance.Conservative,
                    GovernmentRelationship = 20.0
                },
                new PrivateInvestor
                {
                    Id = "investor_meridian_tech",
                    Name = "Meridian Technologies & Logistics",
                    Sector = "Technology",
                    InvestorType = "TechEntrepreneur",
                    CapitalCapacity = 3_500_000.0,
                    RiskTolerance = InvestorRiskTolerance.Aggressive,
                    GovernmentRelationship = 5.0
                }
            });
        }

        return country.PrivateInvestors.AsReadOnly();
    }

    public IReadOnlyList<InvestmentProposal> GenerateInvestmentProposals(Country country, RepublicTime simulationTime)
    {
        ArgumentNullException.ThrowIfNull(country);

        EnsureCountryPrivateInvestors(country);

        var pendingCount = country.InvestmentProposals.Count(p => p.Status == InvestmentProposalStatus.Pending);
        if (pendingCount >= 3)
        {
            return country.InvestmentProposals.AsReadOnly();
        }

        var day = simulationTime.DayNumber;
        var newProposals = new List<InvestmentProposal>();

        // 1. Manufacturing Proposal from Vanguard
        var vanguard = country.PrivateInvestors.FirstOrDefault(i => i.Id == "investor_vanguard_steel") ?? country.PrivateInvestors[0];
        var propMfgId = $"prop_mfg_{vanguard.Id}_d{day}";
        if (!country.InvestmentProposals.Any(p => p.Id == propMfgId))
        {
            newProposals.Add(new InvestmentProposal
            {
                Id = propMfgId,
                InvestorId = vanguard.Id,
                InvestorName = vanguard.Name,
                Title = "Specialized Heavy Rail Car & Steel Fabrication Facility",
                Sector = "Manufacturing",
                Location = "Provincial Heavy Industrial Zone",
                InvestorContribution = 1_200_000.0,
                RequestedGovernmentFunding = 350_000.0,
                RequestedTaxConcession = 3.5,
                EstimatedConstructionDays = 5,
                ProjectedJobsCreated = 750,
                ProjectedGdpBoost = 1_500_000_000.0,
                ProjectedAnnualTaxRevenue = 120_000.0,
                RisksAndConditions = "Requires steady grid electricity and freight rail siding access.",
                Status = InvestmentProposalStatus.Pending,
                CreatedAtDay = day
            });
        }

        // 2. Clean Energy Proposal from Solaria
        var solaria = country.PrivateInvestors.FirstOrDefault(i => i.Id == "investor_solaria_energy") ?? country.PrivateInvestors[0];
        var propEnergyId = $"prop_energy_{solaria.Id}_d{day}";
        if (!country.InvestmentProposals.Any(p => p.Id == propEnergyId))
        {
            newProposals.Add(new InvestmentProposal
            {
                Id = propEnergyId,
                InvestorId = solaria.Id,
                InvestorName = solaria.Name,
                Title = "Sovereign Solar Photovoltaic & Grid Battery Park",
                Sector = "Energy",
                Location = "Southern Valley Renewable Zone",
                InvestorContribution = 2_000_000.0,
                RequestedGovernmentFunding = 500_000.0,
                RequestedTaxConcession = 5.0,
                EstimatedConstructionDays = 7,
                ProjectedJobsCreated = 420,
                ProjectedGdpBoost = 2_200_000_000.0,
                ProjectedAnnualTaxRevenue = 180_000.0,
                RisksAndConditions = "Contingent on import tariff waivers for silicon PV cell components.",
                Status = InvestmentProposalStatus.Pending,
                CreatedAtDay = day
            });
        }

        // 3. Agribusiness Proposal from Aethelgard
        var agro = country.PrivateInvestors.FirstOrDefault(i => i.Id == "investor_aethelgard_agro") ?? country.PrivateInvestors[0];
        var propAgroId = $"prop_agro_{agro.Id}_d{day}";
        if (!country.InvestmentProposals.Any(p => p.Id == propAgroId))
        {
            newProposals.Add(new InvestmentProposal
            {
                Id = propAgroId,
                InvestorId = agro.Id,
                InvestorName = agro.Name,
                Title = "Grain Storage Silos & Bio-Fertilizer Processing Complex",
                Sector = "Agriculture",
                Location = "Northern Breadbasket Valley",
                InvestorContribution = 800_000.0,
                RequestedGovernmentFunding = 200_000.0,
                RequestedTaxConcession = 2.0,
                EstimatedConstructionDays = 4,
                ProjectedJobsCreated = 340,
                ProjectedGdpBoost = 950_000_000.0,
                ProjectedAnnualTaxRevenue = 75_000.0,
                RisksAndConditions = "Requires regional water extraction permits and grain corridor priority.",
                Status = InvestmentProposalStatus.Pending,
                CreatedAtDay = day
            });
        }

        country.InvestmentProposals.AddRange(newProposals);
        return country.InvestmentProposals.AsReadOnly();
    }

    public bool ApplyInvestmentDecision(Country country, string proposalId, string decision, out string outcomeMessage)
    {
        ArgumentNullException.ThrowIfNull(country);

        if (string.IsNullOrWhiteSpace(proposalId))
        {
            outcomeMessage = "Proposal ID cannot be null or empty.";
            return false;
        }

        var proposal = country.InvestmentProposals.FirstOrDefault(p => p.Id == proposalId);
        if (proposal == null)
        {
            outcomeMessage = $"Investment proposal '{proposalId}' was not found in sovereign records.";
            return false;
        }

        if (proposal.Status != InvestmentProposalStatus.Pending && proposal.Status != InvestmentProposalStatus.Negotiated)
        {
            outcomeMessage = $"Proposal '{proposal.Title}' is already finalized ({proposal.Status}). Repeated execution prevented.";
            return false;
        }

        var investor = country.PrivateInvestors.FirstOrDefault(i => i.Id == proposal.InvestorId);
        var normalizedDecision = decision?.Trim().ToLowerInvariant() ?? string.Empty;

        switch (normalizedDecision)
        {
            case "accept":
            {
                // Validate government funding
                if (country.Treasury.Balance < proposal.RequestedGovernmentFunding)
                {
                    outcomeMessage = $"Treasury balance (R {country.Treasury.Balance:N0}) is insufficient for requested co-funding (R {proposal.RequestedGovernmentFunding:N0}).";
                    return false;
                }

                // Deduct sovereign co-funding once
                country.Treasury.Withdraw(proposal.RequestedGovernmentFunding);

                // Commit investor capital and update relationship
                if (investor != null)
                {
                    investor.CapitalCapacity = Math.Max(0.0, investor.CapitalCapacity - proposal.InvestorContribution);
                    investor.GovernmentRelationship = Math.Clamp(investor.GovernmentRelationship + 15.0, -100.0, 100.0);
                }

                proposal.Status = InvestmentProposalStatus.Accepted;
                var currentTime = country.FoundingTime.Add(TimeSpan.FromDays(proposal.CreatedAtDay));

                var project = new PrivateInvestmentProject(
                    country.Id,
                    proposal.Id,
                    proposal.Title,
                    proposal.InvestorName,
                    proposal.Sector,
                    proposal.RequestedGovernmentFunding,
                    proposal.InvestorContribution,
                    proposal.EstimatedConstructionDays,
                    proposal.ProjectedJobsCreated,
                    proposal.ProjectedGdpBoost,
                    proposal.ProjectedAnnualTaxRevenue,
                    currentTime);

                country.Projects.Add(project);
                country.ActivePrivateInvestments.Add(project);

                // Register positive media report
                if (country.MediaOutlets.Count == 0)
                {
                    new Republic.Core.Media.Services.MediaSimulationService().EnsureCountryMediaOutlets(country);
                }

                var ledgerOutlet = country.MediaOutlets.FirstOrDefault(o => o.Leaning == MediaEditorialLeaning.FinancialBusiness)
                                  ?? country.MediaOutlets.FirstOrDefault();
                if (ledgerOutlet != null)
                {
                    var repId = $"rep_invest_accepted_{proposal.Id}";
                    if (!country.MediaReports.Any(r => r.Id == repId))
                    {
                        country.MediaReports.Insert(0, new MediaReport
                        {
                            Id = repId,
                            OutletId = ledgerOutlet.Id,
                            OutletName = ledgerOutlet.Name,
                            Leaning = ledgerOutlet.Leaning,
                            Headline = $"Investment Deal Sealed: {proposal.Title} Authorized",
                            ArticleSummary = $"The administration and {proposal.InvestorName} concluded an agreement committing R {proposal.InvestorContribution:N0} in private capital alongside state co-funding. Estimated {proposal.ProjectedJobsCreated:N0} jobs projected.",
                            Category = "Economic",
                            Sentiment = MediaReportSentiment.Positive,
                            EventSource = "investment_authorized",
                            OutletCredibility = ledgerOutlet.Credibility,
                            OutletReach = ledgerOutlet.AudienceReach,
                            PublishedDay = currentTime.DayNumber,
                            PublicApprovalImpact = 2.0,
                            StabilityImpact = 1.0
                        });

                        country.HappinessRating = Math.Clamp(country.HappinessRating + 2.0, 0.0, 100.0);
                        country.BaselineStability = Math.Clamp(country.BaselineStability + 1.0, 0.0, 100.0);
                    }
                }

                proposal.OutcomeMessage = $"Co-funding of R {proposal.RequestedGovernmentFunding:N0} authorized. Facility under construction ({proposal.EstimatedConstructionDays} days).";
                outcomeMessage = proposal.OutcomeMessage;
                return true;
            }

            case "reject":
            {
                proposal.Status = InvestmentProposalStatus.Rejected;
                if (investor != null)
                {
                    investor.GovernmentRelationship = Math.Clamp(investor.GovernmentRelationship - 8.0, -100.0, 100.0);
                }

                proposal.OutcomeMessage = $"Proposal '{proposal.Title}' formally declined by the administration.";
                outcomeMessage = proposal.OutcomeMessage;
                return true;
            }

            case "negotiate":
            {
                if (proposal.Status == InvestmentProposalStatus.Negotiated)
                {
                    outcomeMessage = $"Terms for '{proposal.Title}' were already renegotiated to final counter-offer terms.";
                    return false;
                }

                // Negotiate: 25% lower government funding, 10% higher investor contribution
                var previousGov = proposal.RequestedGovernmentFunding;
                proposal.RequestedGovernmentFunding = Math.Round(proposal.RequestedGovernmentFunding * 0.75, 0);
                proposal.Status = InvestmentProposalStatus.Negotiated;

                if (investor != null)
                {
                    investor.GovernmentRelationship = Math.Clamp(investor.GovernmentRelationship + 3.0, -100.0, 100.0);
                }

                proposal.OutcomeMessage = $"Terms renegotiated: state co-funding requirement reduced from R {previousGov:N0} to R {proposal.RequestedGovernmentFunding:N0}. Awaiting final authorization.";
                outcomeMessage = proposal.OutcomeMessage;
                return true;
            }

            default:
                outcomeMessage = $"Unrecognized investment decision '{decision}'. Supported options: accept, reject, negotiate.";
                return false;
        }
    }

    public int CheckInvestmentCompletions(Country country, RepublicTime simulationTime)
    {
        ArgumentNullException.ThrowIfNull(country);

        var completedCount = 0;

        foreach (var project in country.ActivePrivateInvestments.Where(p => p.IsActive).ToList())
        {
            if (project.CanComplete(simulationTime) && project.Complete(country, simulationTime))
            {
                completedCount++;

                var proposal = country.InvestmentProposals.FirstOrDefault(p => p.Id == project.ProposalId);
                if (proposal != null)
                {
                    proposal.Status = InvestmentProposalStatus.Completed;
                    proposal.OutcomeMessage = $"Facility commissioned! Generating {project.JobsCreated:N0} jobs and expanding GDP.";
                }

                // Trigger completion media coverage
                if (country.MediaOutlets.Count == 0)
                {
                    new Republic.Core.Media.Services.MediaSimulationService().EnsureCountryMediaOutlets(country);
                }

                var outlet = country.MediaOutlets.FirstOrDefault(o => o.Leaning == MediaEditorialLeaning.FinancialBusiness)
                             ?? country.MediaOutlets.FirstOrDefault();
                if (outlet != null)
                {
                    var repId = $"rep_invest_completed_{project.Id}";
                    if (!country.MediaReports.Any(r => r.Id == repId))
                    {
                        country.MediaReports.Insert(0, new MediaReport
                        {
                            Id = repId,
                            OutletId = outlet.Id,
                            OutletName = outlet.Name,
                            Leaning = outlet.Leaning,
                            Headline = $"Facility Commissioned: {project.Name} Now Operational",
                            ArticleSummary = $"Construction completed on schedule. The new facility is officially active, supporting {project.JobsCreated:N0} full-time personnel and contributing R {project.GdpBoost:N0} to national output.",
                            Category = "Economic",
                            Sentiment = MediaReportSentiment.Positive,
                            EventSource = "investment_completed",
                            OutletCredibility = outlet.Credibility,
                            OutletReach = outlet.AudienceReach,
                            PublishedDay = simulationTime.DayNumber,
                            PublicApprovalImpact = 2.5,
                            StabilityImpact = 1.5
                        });

                        country.HappinessRating = Math.Clamp(country.HappinessRating + 2.5, 0.0, 100.0);
                        country.BaselineStability = Math.Clamp(country.BaselineStability + 1.5, 0.0, 100.0);
                    }
                }
            }
        }

        return completedCount;
    }
}
