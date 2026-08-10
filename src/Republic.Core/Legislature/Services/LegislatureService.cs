namespace Republic.Core.Legislature.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Legislature.Events;
using Republic.Core.Legislature.Models;
using Republic.Core.Workspace.Models;
using Republic.Core.Workspace.Services;

/// <summary>
/// Service implementation conducting parliamentary floor debates, votes, and presidential vetoes.
/// </summary>
public sealed class LegislatureService : ILegislatureService
{
    private readonly List<PoliticalParty> _parties = new();
    private readonly List<Bill> _bills = new();
    private readonly IWorkspaceManager? _workspaceManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private readonly object _lock = new();

    public LegislatureService(IEventBus eventBus, IWorkspaceManager? workspaceManager = null, ILogger? logger = null)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _workspaceManager = workspaceManager;
        _logger = logger;
    }

    public IReadOnlyList<PoliticalParty> GetParties()
    {
        lock (_lock)
        {
            return _parties.ToList().AsReadOnly();
        }
    }

    public void RegisterParty(PoliticalParty party)
    {
        ArgumentNullException.ThrowIfNull(party);
        lock (_lock)
        {
            if (!_parties.Any(p => p.Id == party.Id))
            {
                _parties.Add(party);
            }
        }
    }

    public async Task<Bill> IntroduceBillAsync(string title, string description, double requiredMajority = 50.0, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var bill = new Bill
        {
            Title = title,
            Description = description,
            RequiredMajorityPercentage = requiredMajority
        };

        lock (_lock)
        {
            _bills.Add(bill);
        }

        _logger?.LogInfo($"Bill introduced in Assembly: '{title}' (Required Majority: {requiredMajority:0}%)");
        await _eventBus.PublishAsync(new BillIntroducedEvent(bill, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);

        _workspaceManager?.News.PublishArticle(new NewsArticle
        {
            Source = "Parliamentary Gazette",
            Headline = $"NEW LEGISLATION INTRODUCED: '{title.ToUpperInvariant()}'",
            Summary = description,
            Category = "Legislature",
            ImpactRating = 3
        });

        return bill;
    }

    public async Task<VoteResult> VoteOnBillAsync(string billId, CancellationToken cancellationToken = default)
    {
        Bill? bill;
        List<PoliticalParty> parties;
        lock (_lock)
        {
            bill = _bills.FirstOrDefault(b => b.Id == billId);
            parties = _parties.ToList();
        }

        if (bill == null || bill.IsVotedOn)
        {
            throw new InvalidOperationException($"Bill '{billId}' is invalid or already voted on.");
        }

        var totalSeats = parties.Sum(p => p.SeatCount);
        if (totalSeats == 0) totalSeats = 100; // Default baseline

        var coalitionSeats = parties.Where(p => p.IsGovernmentCoalition).Sum(p => p.SeatCount);
        var oppositionSeats = totalSeats - coalitionSeats;

        var ayes = (int)(coalitionSeats * 0.9) + (int)(oppositionSeats * 0.1);
        var nays = totalSeats - ayes;

        var percentageAye = ((double)ayes / totalSeats) * 100.0;
        var passed = percentageAye >= bill.RequiredMajorityPercentage;

        bill.IsVotedOn = true;
        bill.IsPassed = passed;

        var result = new VoteResult
        {
            BillId = billId,
            TotalVotes = totalSeats,
            AyesCount = ayes,
            NaysCount = nays,
            AbstentionsCount = 0,
            Passed = passed
        };

        _logger?.LogInfo($"Parliamentary Vote: '{bill.Title}' -> Passed: {passed} ({ayes}/{totalSeats} Ayes)");
        await _eventBus.PublishAsync(new ParliamentaryVoteConductedEvent(bill, result, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);

        if (passed)
        {
            await _eventBus.PublishAsync(new BillEnactedEvent(bill, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    public async Task<bool> ExerciseExecutiveVetoAsync(string billId, CancellationToken cancellationToken = default)
    {
        Bill? bill;
        lock (_lock)
        {
            bill = _bills.FirstOrDefault(b => b.Id == billId);
            if (bill == null || !bill.IsPassed || bill.IsVetoed)
            {
                return false;
            }

            bill.IsVetoed = true;
            bill.IsPassed = false;
        }

        _logger?.LogWarning($"EXECUTIVE VETO EXERCISED: Bill '{bill.Title}' vetoed by President!");
        await _eventBus.PublishAsync(new ExecutiveVetoExercisedEvent(bill, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);

        _workspaceManager?.Email.ReceiveEmail(new EmailMessage
        {
            Sender = "Speaker of the House",
            Subject = $"EXECUTIVE VETO NOTIFICATION: '{bill.Title}'",
            Body = $"The President has exercised constitutional veto power to override the enacted legislation '{bill.Title}'.",
            Folder = "Inbox",
            ActionRequired = false
        });

        return true;
    }
}
