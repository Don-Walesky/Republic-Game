namespace Republic.Unity.Bridge;

using System;
using System.Collections.Generic;
using Republic.Core.Decisions.Events;
using Republic.Core.Decisions.Models;
using Republic.Core.Events;
using Republic.Core.Military.Models;
using Republic.Core.World;
using Republic.Core.World.Events;
using Republic.Core.World.Models;
using Republic.Core.Workspace.Events;
using Republic.Core.Workspace.Models;

/// <summary>
/// Unified C# event listener bridging Core simulation events to registered Unity UI views.
/// </summary>
public sealed class RepublicUnityBridge : IWorkspacePresenter, IDecisionPresenter, IWorldOverviewPresenter, IMilitaryPresenter, IRegionalPresenter, IIntelligencePresenter, IMediaPresenter
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

    public event Action<MilitaryReadinessReport>? MilitaryReadinessReportUpdated;
    public event Action<DefconLevel, DefconLevel>? DefconLevelChanged;
    public event Action<MilitaryDirectiveResult>? MilitaryDirectiveExecuted;

    public event Action<IReadOnlyList<ProvinceState>>? ProvincialListUpdated;
    public event Action<string, string, double>? ProvinceStabilityChanged;
    public event Action<string, string, double>? RegionalInfrastructureBuilt;
    public event Action<string, string, double>? RebellionRiskElevated;

    public event Action<string, int>? IntelligenceInfiltrated;
    public event Action<string, bool, string>? CovertOperationCompleted;
    public event Action<string, double>? ThreatLevelEscalated;

    public event Action<string, double, string>? PressConferenceConducted;
    public event Action<string, string, string>? HeadlinePublished;
    public event Action<double>? PublicApprovalRatingUpdated;

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

    public void OnMilitaryReadinessReportUpdated(MilitaryReadinessReport report) => MilitaryReadinessReportUpdated?.Invoke(report);
    public void OnDefconLevelChanged(DefconLevel previousLevel, DefconLevel newLevel) => DefconLevelChanged?.Invoke(previousLevel, newLevel);
    public void OnMilitaryDirectiveExecuted(MilitaryDirectiveResult result) => MilitaryDirectiveExecuted?.Invoke(result);

    public void OnProvincialListUpdated(IReadOnlyList<ProvinceState> provinces) => ProvincialListUpdated?.Invoke(provinces);
    public void OnProvinceStabilityChanged(string provinceId, string provinceName, double newStability) => ProvinceStabilityChanged?.Invoke(provinceId, provinceName, newStability);
    public void OnRegionalInfrastructureBuilt(string provinceId, string provinceName, double newInfrastructureIndex) => RegionalInfrastructureBuilt?.Invoke(provinceId, provinceName, newInfrastructureIndex);
    public void OnRebellionRiskElevated(string provinceId, string provinceName, double riskLevel) => RebellionRiskElevated?.Invoke(provinceId, provinceName, riskLevel);

    public void OnIntelligenceInfiltrated(string targetCountryId, int spyLevel) => IntelligenceInfiltrated?.Invoke(targetCountryId, spyLevel);
    public void OnCovertOperationCompleted(string operationName, bool success, string details) => CovertOperationCompleted?.Invoke(operationName, success, details);
    public void OnThreatLevelEscalated(string regionOrCountry, double threatScore) => ThreatLevelEscalated?.Invoke(regionOrCountry, threatScore);

    public void OnPressConferenceConducted(string topic, double publicApprovalDelta, string transcriptSummary) => PressConferenceConducted?.Invoke(topic, publicApprovalDelta, transcriptSummary);
    public void OnHeadlinePublished(string outletName, string headlineText, string category) => HeadlinePublished?.Invoke(outletName, headlineText, category);
    public void OnPublicApprovalRatingUpdated(double approvalRating) => PublicApprovalRatingUpdated?.Invoke(approvalRating);
}
