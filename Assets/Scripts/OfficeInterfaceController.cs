using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OfficeInterfaceController : MonoBehaviour
{
    [SerializeField] private Button militaryButton;
    [SerializeField] private Button ministriesButton;
    [SerializeField] private Button legislatureButton;
    [SerializeField] private Button diplomacyButton;
    [SerializeField] private Button pressButton;
    [SerializeField] private Text contentText;

    private readonly Dictionary<string, string> _views = new();
    private string _activeView = "Executive";

    private void Start()
    {
        BuildViews();
        BindButtons();
        ShowView(_activeView);
    }

    private void BuildViews()
    {
        _views["Executive"] = "Executive Office\n\nThe president oversees military planning, cabinet performance, legislation, and diplomacy from a single command center.";
        _views["Military"] = "Military Command Room\n\nMeet the service chiefs, coordinate troop readiness, and manage procurement and strategic posture.";
        _views["Ministries"] = "Ministerial Offices\n\nEnter each ministry office to assign tasks, negotiate implementation costs, and direct policy execution.";
        _views["Legislature"] = "Legislative Chamber\n\nMeet loyalists, discuss bills and motions, and negotiate legislative terms before sending proposals to the assembly.";
        _views["Diplomacy"] = "Diplomatic Desk\n\nForeign states may approach you for loans, bailouts, or grants; you can negotiate terms and select the return you desire.";
        _views["Press"] = "Press Secretary Office\n\nIssue briefings, shape approval, and improve public confidence through media messaging.";
    }

    private void BindButtons()
    {
        if (militaryButton != null) militaryButton.onClick.AddListener(() => ShowView("Military"));
        if (ministriesButton != null) ministriesButton.onClick.AddListener(() => ShowView("Ministries"));
        if (legislatureButton != null) legislatureButton.onClick.AddListener(() => ShowView("Legislature"));
        if (diplomacyButton != null) diplomacyButton.onClick.AddListener(() => ShowView("Diplomacy"));
        if (pressButton != null) pressButton.onClick.AddListener(() => ShowView("Press"));
    }

    private void ShowView(string viewName)
    {
        if (_views.TryGetValue(viewName, out var description))
        {
            _activeView = viewName;
            contentText.text = description;
        }
    }
}
