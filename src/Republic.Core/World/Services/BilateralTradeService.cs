namespace Republic.Core.World.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Republic.Core.Economy.Treasury;
using Republic.Core.Time;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;
using Republic.Core.World;
using Republic.Core.World.Models;

/// <summary>
/// Service coordinating flexible bilateral trade deals, multi-round counter-offer negotiations,
/// atomic execution with rollback safety, and recurring contract delivery enforcement.
/// </summary>
public sealed class BilateralTradeService : IBilateralTradeService
{
    private readonly object _lock = new();

    /// <inheritdoc />
    public (bool Success, string Message, FlexibleTradeTerms? Proposal) CreateProposal(
        Country proposer,
        Country target,
        FlexibleTradeTerms terms,
        RepublicTime currentTime,
        IWorkspaceManager? workspaceManager = null)
    {
        ArgumentNullException.ThrowIfNull(proposer);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(terms);

        if (string.Equals(proposer.Id, target.Id, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Internal self-trade is prohibited. A country cannot trade with itself.", null);
        }

        // Validate proposing country's initial solvency
        if (!string.IsNullOrWhiteSpace(terms.OfferedResourceType) && terms.OfferedQuantity > 0)
        {
            if (!proposer.Inventory.HasSufficient(terms.OfferedResourceType, terms.OfferedQuantity))
            {
                var available = proposer.Inventory.GetQuantity(terms.OfferedResourceType);
                return (false, $"Proposal rejected: {proposer.Name} has insufficient {terms.OfferedResourceType}. Offered: {terms.OfferedQuantity:N0}, Available: {available:N0}.", null);
            }
        }

        if (terms.CurrencyPayment > 0 && proposer.Treasury.Balance < terms.CurrencyPayment)
        {
            return (false, $"Proposal rejected: {proposer.Name} has insufficient REPU. Offered: {RepuTreasury.Format(terms.CurrencyPayment)}, Available: {proposer.Treasury.FormattedBalance}.", null);
        }

        var finalized = terms with
        {
            ProposerCountryId = proposer.Id,
            TargetCountryId = target.Id,
            LastModifiedByCountryId = proposer.Id,
            ProposerApproved = true,
            TargetApproved = false,
            Status = FlexibleTradeStatus.Pending,
            CreatedAt = currentTime,
            StatusMessage = $"Pending foreign sovereign review from {target.Name}."
        };

        lock (_lock)
        {
            proposer.StoreFlexibleTradeProposal(finalized);
            target.StoreFlexibleTradeProposal(finalized);

            // Also mirror as DiplomaticAgreement for backward compatibility
            var dipAgreement = new DiplomaticAgreement(
                sourceCountryId: proposer.Id,
                targetCountryId: target.Id,
                agreementType: DiplomaticAgreementType.Trade,
                status: DiplomaticAgreementStatus.Pending,
                proposedBoundary: currentTime,
                id: finalized.Id);

            proposer.StoreAgreement(dipAgreement);
            target.StoreAgreement(dipAgreement);
        }

        // Persistent notification via recipient's Email system
        workspaceManager?.Email.ReceiveEmail(new EmailMessage
        {
            Sender = $"Foreign Ministry ({proposer.Name})",
            Subject = $"BILATERAL TRADE PROPOSAL: {finalized.FormatSummary()}",
            Body = $"The Sovereign Government of '{proposer.Name}' has submitted a bilateral trade proposal for your executive consideration:\n\n{finalized.FormatSummary()}\n\nYou may accept the agreement, decline the proposal, or draft counter-terms.",
            Folder = "Inbox",
            ActionRequired = true
        });

        return (true, $"Bilateral trade proposal successfully dispatched to {target.Name}: {finalized.FormatSummary()}", finalized);
    }

    /// <inheritdoc />
    public (bool Success, string Message, FlexibleTradeTerms? Proposal) CounterProposal(
        Country actor,
        string proposalId,
        FlexibleTradeTerms counterTerms,
        RepublicTime currentTime,
        IWorkspaceManager? workspaceManager = null)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(counterTerms);

        if (string.IsNullOrWhiteSpace(proposalId))
        {
            return (false, "Proposal identifier cannot be null or empty.", null);
        }

        var existing = actor.GetFlexibleTradeProposal(proposalId);
        if (existing == null)
        {
            return (false, $"Trade proposal '{proposalId}' was not found on {actor.Name}.", null);
        }

        if (existing.Status is not (FlexibleTradeStatus.Pending or FlexibleTradeStatus.Countered))
        {
            return (false, $"Cannot counter proposal: Agreement has already been decided (Status: {existing.Status}).", existing);
        }

        // Validate actor is party to the deal
        if (!existing.Involves(actor.Id))
        {
            return (false, $"Country '{actor.Name}' is not a party to this bilateral negotiation.", existing);
        }

        // Solvency validation for actor's new proposed obligations
        bool isProposer = string.Equals(actor.Id, existing.ProposerCountryId, StringComparison.OrdinalIgnoreCase);

        if (isProposer)
        {
            if (!string.IsNullOrWhiteSpace(counterTerms.OfferedResourceType) && counterTerms.OfferedQuantity > 0)
            {
                if (!actor.Inventory.HasSufficient(counterTerms.OfferedResourceType, counterTerms.OfferedQuantity))
                {
                    return (false, $"Counter-offer rejected: {actor.Name} has insufficient {counterTerms.OfferedResourceType}. Required: {counterTerms.OfferedQuantity:N0}, Available: {actor.Inventory.GetQuantity(counterTerms.OfferedResourceType):N0}.", existing);
                }
            }
            if (counterTerms.CurrencyPayment > 0 && actor.Treasury.Balance < counterTerms.CurrencyPayment)
            {
                return (false, $"Counter-offer rejected: {actor.Name} has insufficient funds. Required: {RepuTreasury.Format(counterTerms.CurrencyPayment)}, Available: {actor.Treasury.FormattedBalance}.", existing);
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(counterTerms.RequestedResourceType) && counterTerms.RequestedQuantity > 0)
            {
                if (!actor.Inventory.HasSufficient(counterTerms.RequestedResourceType, counterTerms.RequestedQuantity))
                {
                    return (false, $"Counter-offer rejected: {actor.Name} has insufficient {counterTerms.RequestedResourceType}. Required: {counterTerms.RequestedQuantity:N0}, Available: {actor.Inventory.GetQuantity(counterTerms.RequestedResourceType):N0}.", existing);
                }
            }
            if (counterTerms.CurrencyPayment < 0 && actor.Treasury.Balance < Math.Abs(counterTerms.CurrencyPayment))
            {
                return (false, $"Counter-offer rejected: {actor.Name} has insufficient funds. Required: {RepuTreasury.Format(Math.Abs(counterTerms.CurrencyPayment))}, Available: {actor.Treasury.FormattedBalance}.", existing);
            }
        }

        // CRITICAL: Countering invalidates earlier approvals and advances revision number
        var revised = existing.CreateCounterOffer(
            actor.Id,
            counterTerms.OfferedResourceType,
            counterTerms.OfferedQuantity,
            counterTerms.RequestedResourceType,
            counterTerms.RequestedQuantity,
            counterTerms.CurrencyPayment,
            currentTime);

        lock (_lock)
        {
            actor.StoreFlexibleTradeProposal(revised);
        }

        workspaceManager?.Email.ReceiveEmail(new EmailMessage
        {
            Sender = $"Foreign Ministry ({actor.Name})",
            Subject = $"REVISED TRADE TERMS: {revised.FormatSummary()}",
            Body = $"The Sovereign Government of '{actor.Name}' has submitted revised counter-terms (Revision {revised.RevisionNumber}):\n\n{revised.FormatSummary()}\n\nNote: Prior approvals have been invalidated. Both parties must re-authorize these revised terms before execution.",
            Folder = "Inbox",
            ActionRequired = true
        });

        return (true, $"Counter-offer submitted successfully (Revision {revised.RevisionNumber}). Prior approvals invalidated.", revised);
    }

    /// <inheritdoc />
    public (bool Success, string Message, FlexibleTradeTerms? ExecutedDeal) AcceptProposal(
        Country actor,
        string proposalId,
        RepublicTime currentTime,
        IEnumerable<Country> worldCountries,
        IWorkspaceManager? workspaceManager = null)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(worldCountries);

        if (string.IsNullOrWhiteSpace(proposalId))
        {
            return (false, "Proposal identifier cannot be null or empty.", null);
        }

        var existing = actor.GetFlexibleTradeProposal(proposalId);
        if (existing == null)
        {
            return (false, $"Trade proposal '{proposalId}' was not found on {actor.Name}.", null);
        }

        if (existing.Status is not (FlexibleTradeStatus.Pending or FlexibleTradeStatus.Countered))
        {
            return (false, $"Agreement has already been decided or executed (Status: {existing.Status}, Decided: {existing.DecidedAt}).", existing);
        }

        // Validation: Cannot accept if actor was the one who drafted this revision and already approved it
        if (!existing.CanBeAcceptedBy(actor.Id))
        {
            return (false, "Cannot accept trade: You authored the current revision and have already approved it. Awaiting foreign sovereign acceptance.", existing);
        }

        var partnerId = string.Equals(actor.Id, existing.ProposerCountryId, StringComparison.OrdinalIgnoreCase)
            ? existing.TargetCountryId
            : existing.ProposerCountryId;

        var partner = worldCountries.FirstOrDefault(c => string.Equals(c.Id, partnerId, StringComparison.OrdinalIgnoreCase));
        if (partner == null)
        {
            return (false, $"Partner country '{partnerId}' does not exist in the world registry.", existing);
        }

        var proposer = string.Equals(actor.Id, existing.ProposerCountryId, StringComparison.OrdinalIgnoreCase) ? actor : partner;
        var target = string.Equals(actor.Id, existing.TargetCountryId, StringComparison.OrdinalIgnoreCase) ? actor : partner;

        lock (_lock)
        {
            // PRE-EXECUTION VALIDATION:
            // 1. Proposer goods sufficiency
            if (!string.IsNullOrWhiteSpace(existing.OfferedResourceType) && existing.OfferedQuantity > 0)
            {
                if (!proposer.Inventory.HasSufficient(existing.OfferedResourceType, existing.OfferedQuantity))
                {
                    var avail = proposer.Inventory.GetQuantity(existing.OfferedResourceType);
                    return (false, $"Execution aborted: Proposer '{proposer.Name}' has insufficient {existing.OfferedResourceType}. Required: {existing.OfferedQuantity:N0}, Available: {avail:N0}.", existing);
                }
            }

            // 2. Proposer funds sufficiency
            if (existing.CurrencyPayment > 0 && proposer.Treasury.Balance < existing.CurrencyPayment)
            {
                return (false, $"Execution aborted: Proposer '{proposer.Name}' has insufficient funds. Required: {RepuTreasury.Format(existing.CurrencyPayment)}, Available: {proposer.Treasury.FormattedBalance}.", existing);
            }

            // 3. Target goods sufficiency
            if (!string.IsNullOrWhiteSpace(existing.RequestedResourceType) && existing.RequestedQuantity > 0)
            {
                if (!target.Inventory.HasSufficient(existing.RequestedResourceType, existing.RequestedQuantity))
                {
                    var avail = target.Inventory.GetQuantity(existing.RequestedResourceType);
                    return (false, $"Execution aborted: Target '{target.Name}' has insufficient {existing.RequestedResourceType}. Required: {existing.RequestedQuantity:N0}, Available: {avail:N0}.", existing);
                }
            }

            // 4. Target funds sufficiency
            if (existing.CurrencyPayment < 0 && target.Treasury.Balance < Math.Abs(existing.CurrencyPayment))
            {
                return (false, $"Execution aborted: Target '{target.Name}' has insufficient funds. Required: {RepuTreasury.Format(Math.Abs(existing.CurrencyPayment))}, Available: {target.Treasury.FormattedBalance}.", existing);
            }

            // ATOMIC EXECUTION WITH COMPLETE ROLLBACK ON FAILURE:
            bool debitedProposerFunds = false;
            bool debitedProposerGoods = false;
            bool debitedTargetFunds = false;
            bool debitedTargetGoods = false;

            try
            {
                // Debit Proposer Funds
                if (existing.CurrencyPayment > 0)
                {
                    if (!proposer.Treasury.TryWithdraw(existing.CurrencyPayment))
                    {
                        throw new InvalidOperationException("Failed to withdraw proposer currency.");
                    }
                    debitedProposerFunds = true;
                }

                // Debit Proposer Goods
                if (!string.IsNullOrWhiteSpace(existing.OfferedResourceType) && existing.OfferedQuantity > 0)
                {
                    if (!proposer.Inventory.TryRemove(existing.OfferedResourceType, existing.OfferedQuantity))
                    {
                        throw new InvalidOperationException("Failed to remove proposer goods.");
                    }
                    debitedProposerGoods = true;
                }

                // Debit Target Funds
                if (existing.CurrencyPayment < 0)
                {
                    if (!target.Treasury.TryWithdraw(Math.Abs(existing.CurrencyPayment)))
                    {
                        throw new InvalidOperationException("Failed to withdraw target currency.");
                    }
                    debitedTargetFunds = true;
                }

                // Debit Target Goods
                if (!string.IsNullOrWhiteSpace(existing.RequestedResourceType) && existing.RequestedQuantity > 0)
                {
                    if (!target.Inventory.TryRemove(existing.RequestedResourceType, existing.RequestedQuantity))
                    {
                        throw new InvalidOperationException("Failed to remove target goods.");
                    }
                    debitedTargetGoods = true;
                }

                // ALL DEBITS SUCCEEDED -> NOW CREDIT BOTH SIDES SAFELY
                if (!string.IsNullOrWhiteSpace(existing.OfferedResourceType) && existing.OfferedQuantity > 0)
                {
                    target.Inventory.Add(existing.OfferedResourceType, existing.OfferedQuantity);
                }

                if (existing.CurrencyPayment > 0)
                {
                    target.Treasury.Deposit(existing.CurrencyPayment);
                }

                if (!string.IsNullOrWhiteSpace(existing.RequestedResourceType) && existing.RequestedQuantity > 0)
                {
                    proposer.Inventory.Add(existing.RequestedResourceType, existing.RequestedQuantity);
                }

                if (existing.CurrencyPayment < 0)
                {
                    proposer.Treasury.Deposit(Math.Abs(existing.CurrencyPayment));
                }
            }
            catch (Exception ex)
            {
                // COMPLETE ROLLBACK: restore all debited assets to original owners
                if (debitedProposerFunds && existing.CurrencyPayment > 0)
                {
                    proposer.Treasury.Deposit(existing.CurrencyPayment);
                }

                if (debitedProposerGoods && !string.IsNullOrWhiteSpace(existing.OfferedResourceType) && existing.OfferedQuantity > 0)
                {
                    proposer.Inventory.Add(existing.OfferedResourceType, existing.OfferedQuantity);
                }

                if (debitedTargetFunds && existing.CurrencyPayment < 0)
                {
                    target.Treasury.Deposit(Math.Abs(existing.CurrencyPayment));
                }

                if (debitedTargetGoods && !string.IsNullOrWhiteSpace(existing.RequestedResourceType) && existing.RequestedQuantity > 0)
                {
                    target.Inventory.Add(existing.RequestedResourceType, existing.RequestedQuantity);
                }

                return (false, $"Atomic execution failed and was rolled back cleanly: {ex.Message}. Zero assets transferred.", existing);
            }

            // Update status and cycle obligations
            var remainingCycles = existing.RemainingDeliveryCycles - 1;
            var isContractFinished = !existing.IsRecurring || remainingCycles <= 0;
            var finalStatus = isContractFinished ? FlexibleTradeStatus.Completed : FlexibleTradeStatus.ActiveContract;

            var executed = existing with
            {
                ProposerApproved = true,
                TargetApproved = true,
                Status = finalStatus,
                RemainingDeliveryCycles = Math.Max(0, remainingCycles),
                DecidedAt = currentTime,
                StatusMessage = isContractFinished
                    ? $"Transaction completed successfully at {currentTime}."
                    : $"Delivery cycle executed at {currentTime}. {remainingCycles} cycles remaining."
            };

            proposer.StoreFlexibleTradeProposal(executed);
            target.StoreFlexibleTradeProposal(executed);

            // Update DiplomaticAgreement records
            var dipAgreement = new DiplomaticAgreement(
                sourceCountryId: proposer.Id,
                targetCountryId: target.Id,
                agreementType: DiplomaticAgreementType.Trade,
                status: isContractFinished ? DiplomaticAgreementStatus.Completed : DiplomaticAgreementStatus.Accepted,
                proposedBoundary: existing.CreatedAt,
                decidedBoundary: currentTime,
                id: executed.Id);

            proposer.StoreAgreement(dipAgreement);
            target.StoreAgreement(dipAgreement);

            // Bilateral relation improvements
            var relationFromProposer = proposer.GetRelation(target.Id);
            if (relationFromProposer == null)
            {
                proposer.OpenRelation(target.Id, currentTime);
                relationFromProposer = proposer.GetRelation(target.Id);
            }
            if (relationFromProposer != null && !relationFromProposer.IsRecognized)
            {
                proposer.RecognizeRelation(target.Id, currentTime);
            }

            var relationFromTarget = target.GetRelation(proposer.Id);
            if (relationFromTarget == null)
            {
                target.OpenRelation(proposer.Id, currentTime);
                relationFromTarget = target.GetRelation(proposer.Id);
            }
            if (relationFromTarget != null && !relationFromTarget.IsRecognized)
            {
                target.RecognizeRelation(proposer.Id, currentTime);
            }

            // Persistent Email delivery for ratification
            workspaceManager?.Email.ReceiveEmail(new EmailMessage
            {
                Sender = "Ministry of Foreign Affairs & Trade",
                Subject = $"TRADE RATIFIED: {executed.FormatSummary()}",
                Body = $"Bilateral trade deal executed between '{proposer.Name}' and '{target.Name}' at {currentTime}.\n\nTerms: {executed.FormatSummary()}\n\nPhysical commodities and sovereign treasuries have been authoritatively settled.",
                Folder = "Inbox",
                ActionRequired = false
            });

            return (true, $"Bilateral trade deal successfully executed at {currentTime}: {executed.FormatSummary()}", executed);
        }
    }

    /// <inheritdoc />
    public (bool Success, string Message, FlexibleTradeTerms? RejectedDeal) RejectProposal(
        Country actor,
        string proposalId,
        RepublicTime currentTime,
        IWorkspaceManager? workspaceManager = null)
    {
        ArgumentNullException.ThrowIfNull(actor);

        if (string.IsNullOrWhiteSpace(proposalId))
        {
            return (false, "Proposal identifier cannot be null or empty.", null);
        }

        var existing = actor.GetFlexibleTradeProposal(proposalId);
        if (existing == null)
        {
            return (false, $"Trade proposal '{proposalId}' was not found on {actor.Name}.", null);
        }

        if (existing.Status is not (FlexibleTradeStatus.Pending or FlexibleTradeStatus.Countered))
        {
            return (false, $"Proposal cannot be rejected: Current status is {existing.Status}.", existing);
        }

        var rejected = existing with
        {
            Status = FlexibleTradeStatus.Rejected,
            DecidedAt = currentTime,
            StatusMessage = $"Proposal declined by {actor.Name} at {currentTime}. Zero funds or goods transferred."
        };

        lock (_lock)
        {
            actor.StoreFlexibleTradeProposal(rejected);

            var dipAgreement = actor.GetAgreement(proposalId);
            if (dipAgreement != null && dipAgreement.IsPending)
            {
                var rejDip = dipAgreement with
                {
                    Status = DiplomaticAgreementStatus.Rejected,
                    DecidedBoundary = currentTime
                };
                actor.StoreAgreement(rejDip);
            }
        }

        workspaceManager?.Email.ReceiveEmail(new EmailMessage
        {
            Sender = "Ministry of Foreign Affairs",
            Subject = $"TRADE PROPOSAL DECLINED: {existing.Id}",
            Body = $"Sovereign trade proposal '{existing.FormatSummary()}' was formally rejected at {currentTime}. No goods or funds were transferred.",
            Folder = "Inbox",
            ActionRequired = false
        });

        return (true, $"Trade proposal was rejected at {currentTime}. Zero funds or goods transferred.", rejected);
    }

    /// <inheritdoc />
    public IReadOnlyList<(string ContractId, bool Success, string Message)> ProcessRecurringContracts(
        Country country,
        RepublicTime currentTime,
        IEnumerable<Country> worldCountries,
        IWorkspaceManager? workspaceManager = null)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(worldCountries);

        var results = new List<(string, bool, string)>();
        var activeContracts = country.FlexibleTradeProposals
            .Where(p => p.Status == FlexibleTradeStatus.ActiveContract && p.RemainingDeliveryCycles > 0)
            .ToList();

        foreach (var contract in activeContracts)
        {
            var partnerId = string.Equals(country.Id, contract.ProposerCountryId, StringComparison.OrdinalIgnoreCase)
                ? contract.TargetCountryId
                : contract.ProposerCountryId;

            var partner = worldCountries.FirstOrDefault(c => string.Equals(c.Id, partnerId, StringComparison.OrdinalIgnoreCase));
            if (partner == null)
            {
                results.Add((contract.Id, false, $"Partner country '{partnerId}' not found."));
                continue;
            }

            var proposer = string.Equals(country.Id, contract.ProposerCountryId, StringComparison.OrdinalIgnoreCase) ? country : partner;
            var target = string.Equals(country.Id, contract.TargetCountryId, StringComparison.OrdinalIgnoreCase) ? country : partner;

            // Check if parties have sufficient funds/goods for this cycle
            bool proposerSolvent = true;
            if (!string.IsNullOrWhiteSpace(contract.OfferedResourceType) && contract.OfferedQuantity > 0)
            {
                proposerSolvent &= proposer.Inventory.HasSufficient(contract.OfferedResourceType, contract.OfferedQuantity);
            }
            if (contract.CurrencyPayment > 0)
            {
                proposerSolvent &= proposer.Treasury.Balance >= contract.CurrencyPayment;
            }

            bool targetSolvent = true;
            if (!string.IsNullOrWhiteSpace(contract.RequestedResourceType) && contract.RequestedQuantity > 0)
            {
                targetSolvent &= target.Inventory.HasSufficient(contract.RequestedResourceType, contract.RequestedQuantity);
            }
            if (contract.CurrencyPayment < 0)
            {
                targetSolvent &= target.Treasury.Balance >= Math.Abs(contract.CurrencyPayment);
            }

            if (!proposerSolvent || !targetSolvent)
            {
                // Breach of contract: mark disputed, damage diplomatic reputation
                var disputed = contract with
                {
                    Status = FlexibleTradeStatus.Disputed,
                    DecidedAt = currentTime,
                    StatusMessage = $"Delivery default at {currentTime}. Contract suspended due to insolvency/shortfall."
                };

                country.StoreFlexibleTradeProposal(disputed);
                partner.StoreFlexibleTradeProposal(disputed);

                workspaceManager?.Email.ReceiveEmail(new EmailMessage
                {
                    Sender = "Diplomatic Corps Alert",
                    Subject = $"TRADE DISPUTE: Breach of Contract {contract.Id}",
                    Body = $"Recurring delivery failed at {currentTime}. One or both parties lacked the promised goods or funds. The contract has been suspended and bilateral relations strained.",
                    Folder = "Inbox",
                    ActionRequired = true
                });

                results.Add((contract.Id, false, $"Delivery defaulted at {currentTime}: Contract suspended."));
                continue;
            }

            // Execute delivery cycle
            if (contract.CurrencyPayment > 0)
            {
                proposer.Treasury.TryWithdraw(contract.CurrencyPayment);
                target.Treasury.Deposit(contract.CurrencyPayment);
            }
            if (contract.CurrencyPayment < 0)
            {
                target.Treasury.TryWithdraw(Math.Abs(contract.CurrencyPayment));
                proposer.Treasury.Deposit(Math.Abs(contract.CurrencyPayment));
            }
            if (!string.IsNullOrWhiteSpace(contract.OfferedResourceType) && contract.OfferedQuantity > 0)
            {
                proposer.Inventory.TryRemove(contract.OfferedResourceType, contract.OfferedQuantity);
                target.Inventory.Add(contract.OfferedResourceType, contract.OfferedQuantity);
            }
            if (!string.IsNullOrWhiteSpace(contract.RequestedResourceType) && contract.RequestedQuantity > 0)
            {
                target.Inventory.TryRemove(contract.RequestedResourceType, contract.RequestedQuantity);
                proposer.Inventory.Add(contract.RequestedResourceType, contract.RequestedQuantity);
            }

            var remaining = contract.RemainingDeliveryCycles - 1;
            var isDone = remaining <= 0;
            var updated = contract with
            {
                RemainingDeliveryCycles = Math.Max(0, remaining),
                Status = isDone ? FlexibleTradeStatus.Completed : FlexibleTradeStatus.ActiveContract,
                DecidedAt = currentTime,
                StatusMessage = isDone ? $"Contract fulfilled completely at {currentTime}." : $"Cycle delivered. {remaining} cycles remaining."
            };

            country.StoreFlexibleTradeProposal(updated);
            partner.StoreFlexibleTradeProposal(updated);
            results.Add((contract.Id, true, $"Delivered cycle ({remaining} remaining)."));
        }

        return results.AsReadOnly();
    }

    /// <inheritdoc />
    public IReadOnlyList<FlexibleTradeTerms> GetProposalsForCountry(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);
        return country.FlexibleTradeProposals;
    }

    /// <inheritdoc />
    public IReadOnlyList<FlexibleTradeTerms> GetActiveContractsForCountry(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);
        return country.FlexibleTradeProposals
            .Where(p => p.Status == FlexibleTradeStatus.ActiveContract && p.RemainingDeliveryCycles > 0)
            .ToList()
            .AsReadOnly();
    }
}
