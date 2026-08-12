namespace Republic.Unity.UI;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Republic.Core.Workspace.Models;

/// <summary>
/// Unity UI Controller managing Executive Workspace view (News feeds, Email inbox, and Visitor status).
/// </summary>
public sealed class WorkspaceUIController : MonoBehaviour
{
    [Header("News Ticker UI")]
    [SerializeField] private Text newsHeadlineText = null!;
    [SerializeField] private Text newsSourceText = null!;

    [Header("Email Inbox UI")]
    [SerializeField] private Text unreadEmailCountText = null!;
    [SerializeField] private Text latestEmailSubjectText = null!;
    [SerializeField] private Text latestEmailSenderText = null!;

    [Header("Executive Desk Schedule HUD")]
    [SerializeField] private Text visitorStatusText = null!;
    [SerializeField] private Text calendarScheduleText = null!;

    private readonly List<NewsArticle> recentArticles = new();
    private readonly List<EmailMessage> inbox = new();

    private void Start()
    {
        if (RepublicGameManager.Instance != null)
        {
            var bridge = RepublicGameManager.Instance.UnityBridge;
            bridge.NewsPublished += OnNewsPublished;
            bridge.EmailReceived += OnEmailReceived;
            bridge.VisitorArrived += OnVisitorArrived;
            bridge.AppointmentReminded += OnAppointmentReminded;
        }
    }

    private void OnVisitorArrived(Visitor visitor)
    {
        if (visitorStatusText != null && visitor != null)
        {
            visitorStatusText.text = $"VISITOR ARRIVED: {visitor.Name} ({visitor.Title}) - '{visitor.Purpose}'";
        }
    }

    private void OnAppointmentReminded(CalendarItem item)
    {
        if (calendarScheduleText != null && item != null)
        {
            calendarScheduleText.text = $"SCHEDULED: {item.Title} @ {item.StartTime:HH:mm} ({item.Location})";
        }
    }

    private void OnNewsPublished(NewsArticle article)
    {
        recentArticles.Add(article);
        if (newsHeadlineText != null)
        {
            newsHeadlineText.text = article.Headline;
        }
        if (newsSourceText != null)
        {
            newsSourceText.text = $"SOURCE: {article.Source.ToUpper()}";
        }
    }

    private void OnEmailReceived(EmailMessage email)
    {
        inbox.Add(email);
        UpdateInboxDisplay();
    }

    private void UpdateInboxDisplay()
    {
        if (unreadEmailCountText != null)
        {
            unreadEmailCountText.text = $"UNREAD: {inbox.Count}";
        }
        if (inbox.Count > 0)
        {
            var latest = inbox[^1];
            if (latestEmailSubjectText != null)
            {
                latestEmailSubjectText.text = latest.Subject;
            }
            if (latestEmailSenderText != null)
            {
                latestEmailSenderText.text = $"FROM: {latest.Sender}";
            }
        }
    }

    private void OnDestroy()
    {
        if (RepublicGameManager.Instance != null)
        {
            var bridge = RepublicGameManager.Instance.UnityBridge;
            bridge.NewsPublished -= OnNewsPublished;
            bridge.EmailReceived -= OnEmailReceived;
            bridge.VisitorArrived -= OnVisitorArrived;
            bridge.AppointmentReminded -= OnAppointmentReminded;
        }
    }
}
