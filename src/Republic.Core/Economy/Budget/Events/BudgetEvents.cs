namespace Republic.Core.Economy.Budget.Events;

using Republic.Core.Economy.Budget.Models;
using Republic.Core.Events;

public sealed record TaxPolicyUpdatedEvent(TaxPolicy NewPolicy, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record MinistryBudgetUpdatedEvent(MinistryBudget NewBudget, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record QuarterlyFiscalReportGeneratedEvent(FiscalQuarterReport Report, DateTimeOffset OccurredAt) : IGameEvent;
