namespace Republic.Unity.Bridge;

using Republic.Core.Decisions.Events;
using Republic.Core.Decisions.Models;
using Republic.Core.Events;
using Republic.Core.World;
using Republic.Core.World.Events;
using Republic.Core.World.Models;
using Republic.Core.Workspace.Events;
using Republic.Core.Workspace.Models;

/// <summary>
/// Unified C# event listener bridging Core simulation events to registered Unity UI views.
/// </summary>
public sealed class RepublicUnityBridge : IWorkspacePresenter, IDecisionPresenter, IWorldOverviewPresenter
{
    public event Action<WorkspaceState>? WorkspaceStateUpdated;
    public event Action<Visitor>? VisitorArrived;
    public event Action<PhoneCall>? PhoneRinging;
    public event Action<EmailMessage>? EmailReceived;
    public event Action<NewsArticle>? NewsPublished;
    public event Action<CalendarAppointment>? AppointmentReminded;

    public event Action<DecisionContext>? DecisionPrompted;
    public event Action<string, DecisionOption>? DecisionExecuted;
    public event Action<string, string>? DecreeEnacted;

    public event Action<WorldState>? WorldStateUpdated;
    public event Action<string, double>? CountryStabilityChanged;
    public event Action<EconomicIndicator>? EconomicIndicatorsUpdated;
    public event Action<string, string, string>? CrisisTriggered;

    public void OnWorkspaceStateUpdated(WorkspaceState state) => WorkspaceStateUpdated?.Invoke(state);
    public void OnVisitorArrived(Visitor visitor) => VisitorArrived?.Invoke(visitor);
    public void OnPhoneRinging(PhoneCall call) => PhoneRinging?.Invoke(call);
    public void OnEmailReceived(EmailMessage email) => EmailReceived?.Invoke(email);
    public void OnNewsPublished(NewsArticle article) => NewsPublished?.Invoke(article);
    public void OnAppointmentReminded(CalendarAppointment appointment) => AppointmentReminded?.Invoke(appointment);

    public void OnDecisionPrompted(DecisionContext decision) => DecisionPrompted?.Invoke(decision);
    public void OnDecisionExecuted(string decisionId, DecisionOption chosenOption) => DecisionExecuted?.Invoke(decisionId, chosenOption);
    public void OnDecreeEnacted(string decreeId, string title) => DecreeEnacted?.Invoke(decreeId, title);

    public void OnWorldStateUpdated(WorldState worldState) => WorldStateUpdated?.Invoke(worldState);
    public void OnCountryStabilityChanged(string countryId, double newStability) => CountryStabilityChanged?.Invoke(countryId, newStability);
    public void OnEconomicIndicatorsUpdated(EconomicIndicator indicators) => EconomicIndicatorsUpdated?.Invoke(indicators);
    public void OnCrisisTriggered(string crisisTitle, string category, string severity) => CrisisTriggered?.Invoke(crisisTitle, category, severity);
}
