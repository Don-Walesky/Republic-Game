namespace Republic.Unity.UI;

using UnityEngine;
using UnityEngine.UI;
using Republic.Core.Workspace.Models;
using Republic.Core.World.Models;
using Republic.Unity.Bridge;

/// <summary>
/// Unity UI controller managing executive desk HUD displays (Treasury, GDP, Stability, Email Badge, News Ticker).
/// </summary>
public sealed class ExecutiveDeskUIController : MonoBehaviour
{
    [Header("Top Bar Indicators")]
    [SerializeField] private Text treasuryText = null!;
    [SerializeField] private Text gdpText = null!;
    [SerializeField] private Text stabilityText = null!;
    [SerializeField] private Text happinessText = null!;

    [Header("Workspace Media Panels")]
    [SerializeField] private Text emailBadgeText = null!;
    [SerializeField] private Text newsTickerText = null!;
    [SerializeField] private Text visitorQueueText = null!;
    [SerializeField] private Text calendarAlertText = null!;
    [SerializeField] private GameObject phoneRingIndicator = null!;

    private void Start()
    {
        if (RepublicGameManager.Instance != null)
        {
            var bridge = RepublicGameManager.Instance.UnityBridge;
            bridge.EconomicIndicatorsUpdated += UpdateEconomicHUD;
            bridge.WorkspaceStateUpdated += UpdateWorkspaceHUD;
            bridge.NewsPublished += OnNewsArticlePublished;
            bridge.VisitorArrived += OnVisitorArrived;
            bridge.AppointmentReminded += OnAppointmentReminded;
        }
    }

    private void UpdateEconomicHUD(EconomicIndicator indicators)
    {
        if (treasuryText != null)
        {
            treasuryText.text = $"Treasury: ${indicators.TreasuryBalance:N0}";
        }

        if (gdpText != null)
        {
            gdpText.text = $"GDP: ${indicators.GrossDomesticProduct:N0}";
        }
    }

    private void UpdateWorkspaceHUD(WorkspaceState state)
    {
        if (emailBadgeText != null && state.Emails != null)
        {
            emailBadgeText.text = $"{state.Emails.Count} Messages";
        }

        if (visitorQueueText != null && state.Visitors != null)
        {
            visitorQueueText.text = $"Visitors in Lobby: {state.Visitors.Count}";
        }

        if (phoneRingIndicator != null && state.ActivePhoneCall != null)
        {
            phoneRingIndicator.SetActive(state.ActivePhoneCall.IsRinging);
        }
    }

    private void OnVisitorArrived(Visitor visitor)
    {
        if (visitorQueueText != null && visitor != null)
        {
            visitorQueueText.text = $"LOBBY: Visitor '{visitor.Name}' ({visitor.Title}) arrived for {visitor.Purpose}";
        }
    }

    private void OnAppointmentReminded(CalendarAppointment appointment)
    {
        if (calendarAlertText != null && appointment != null)
        {
            calendarAlertText.text = $"CALENDAR: {appointment.Title} scheduled at {appointment.ScheduledTime:HH:mm} ({appointment.Location})";
        }
    }

    private void OnNewsArticlePublished(NewsArticle article)
    {
        if (newsTickerText != null && article != null)
        {
            newsTickerText.text = $"[NEWS TICKER] {article.Headline.ToUpperInvariant()} - {article.Summary}";
        }
    }

    private void OnDestroy()
    {
        if (RepublicGameManager.Instance != null)
        {
            var bridge = RepublicGameManager.Instance.UnityBridge;
            bridge.EconomicIndicatorsUpdated -= UpdateEconomicHUD;
            bridge.WorkspaceStateUpdated -= UpdateWorkspaceHUD;
            bridge.NewsPublished -= OnNewsArticlePublished;
            bridge.VisitorArrived -= OnVisitorArrived;
            bridge.AppointmentReminded -= OnAppointmentReminded;
        }
    }
}
