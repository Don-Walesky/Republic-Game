namespace Republic.Unity.UI;

using UnityEngine;
using UnityEngine.UI;
using Republic.Unity.Bridge;

/// <summary>
/// Unity UI controller managing intelligence dossiers, covert ops alerts, press conference logs, and media headlines.
/// </summary>
public sealed class IntelligenceMediaUIController : MonoBehaviour
{
    [Header("Intelligence Displays")]
    [SerializeField] private Text intelligenceSummaryText = null!;
    [SerializeField] private Text covertOpsLogText = null!;

    [Header("Media & Press Displays")]
    [SerializeField] private Text headlineTickerText = null!;
    [SerializeField] private Text publicApprovalMeterText = null!;
    [SerializeField] private Text pressConferenceSummaryText = null!;

    private void Start()
    {
        if (RepublicGameManager.Instance != null)
        {
            var bridge = RepublicGameManager.Instance.UnityBridge;
            bridge.IntelligenceInfiltrated += OnIntelligenceInfiltrated;
            bridge.CovertOperationCompleted += OnCovertOperationCompleted;
            bridge.HeadlinePublished += OnHeadlinePublished;
            bridge.PressConferenceConducted += OnPressConferenceConducted;
            bridge.PublicApprovalRatingUpdated += OnPublicApprovalUpdated;
        }
    }

    public void OnIntelligenceInfiltrated(string targetCountryId, int spyLevel)
    {
        if (intelligenceSummaryText != null)
        {
            intelligenceSummaryText.text = $"[INTEL ALERT] Asset infiltrated target '{targetCountryId}' - Network Level: {spyLevel}";
        }
    }

    public void OnCovertOperationCompleted(string operationName, bool success, string details)
    {
        if (covertOpsLogText != null)
        {
            string status = success ? "SUCCESS" : "COMPROMISED";
            covertOpsLogText.text = $"[COVERT OP: {status}] {operationName} - {details}";
        }
    }

    public void OnHeadlinePublished(string outletName, string headlineText, string category)
    {
        if (headlineTickerText != null)
        {
            headlineTickerText.text = $"[{outletName.ToUpper()}] ({category}) {headlineText}";
        }
    }

    public void OnPressConferenceConducted(string topic, double publicApprovalDelta, string transcriptSummary)
    {
        if (pressConferenceSummaryText != null)
        {
            string deltaSign = publicApprovalDelta >= 0 ? "+" : "";
            pressConferenceSummaryText.text = $"[PRESS BRIEFING] Topic: {topic} (Approval: {deltaSign}{publicApprovalDelta:0.0}%) - {transcriptSummary}";
        }
    }

    public void OnPublicApprovalUpdated(double approvalRating)
    {
        if (publicApprovalMeterText != null)
        {
            publicApprovalMeterText.text = $"PUBLIC APPROVAL: {approvalRating:0.0}%";
        }
    }

    private void OnDestroy()
    {
        if (RepublicGameManager.Instance != null)
        {
            var bridge = RepublicGameManager.Instance.UnityBridge;
            bridge.IntelligenceInfiltrated -= OnIntelligenceInfiltrated;
            bridge.CovertOperationCompleted -= OnCovertOperationCompleted;
            bridge.HeadlinePublished -= OnHeadlinePublished;
            bridge.PressConferenceConducted -= OnPressConferenceConducted;
            bridge.PublicApprovalRatingUpdated -= OnPublicApprovalUpdated;
        }
    }
}
