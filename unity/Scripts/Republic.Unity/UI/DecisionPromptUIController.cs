namespace Republic.Unity.UI;

using UnityEngine;
using UnityEngine.UI;
using Republic.Core.Decisions.Models;

/// <summary>
/// Unity UI controller managing crisis decision modals and executive choice selection.
/// </summary>
public sealed class DecisionPromptUIController : MonoBehaviour
{
    [Header("Modal Containers")]
    [SerializeField] private GameObject modalPanel = null!;
    [SerializeField] private Text titleText = null!;
    [SerializeField] private Text descriptionText = null!;
    [SerializeField] private Text categoryBadgeText = null!;

    [Header("Option Buttons")]
    [SerializeField] private Button optionButtonA = null!;
    [SerializeField] private Text optionTextA = null!;
    [SerializeField] private Button optionButtonB = null!;
    [SerializeField] private Text optionTextB = null!;

    private DecisionContext? _currentDecision;

    private void Start()
    {
        if (modalPanel != null)
        {
            modalPanel.SetActive(false);
        }

        if (RepublicGameManager.Instance != null)
        {
            RepublicGameManager.Instance.UnityBridge.DecisionPrompted += DisplayDecisionModal;
        }
    }

    public void DisplayDecisionModal(DecisionContext decision)
    {
        _currentDecision = decision;
        if (modalPanel != null)
        {
            modalPanel.SetActive(true);
        }

        if (titleText != null) titleText.text = decision.Title;
        if (descriptionText != null) descriptionText.text = decision.Description;
        if (categoryBadgeText != null) categoryBadgeText.text = decision.Category.ToUpperInvariant();

        if (decision.Options.Count > 0 && optionButtonA != null && optionTextA != null)
        {
            var optA = decision.Options[0];
            optionTextA.text = $"{optA.Label}\n<size=11>${optA.TreasuryCost:N0}</size>";
            optionButtonA.onClick.RemoveAllListeners();
            optionButtonA.onClick.AddListener(() => OnOptionClicked(optA.Id));
        }

        if (decision.Options.Count > 1 && optionButtonB != null && optionTextB != null)
        {
            var optB = decision.Options[1];
            optionTextB.text = $"{optB.Label}\n<size=11>${optB.TreasuryCost:N0}</size>";
            optionButtonB.onClick.RemoveAllListeners();
            optionButtonB.onClick.AddListener(() => OnOptionClicked(optB.Id));
        }
    }

    private async void OnOptionClicked(string optionId)
    {
        if (_currentDecision != null && RepublicGameManager.Instance != null)
        {
            await RepublicGameManager.Instance.ExecuteDecisionAsync(_currentDecision.Id, optionId);
            CloseModal();
        }
    }

    public void CloseModal()
    {
        if (modalPanel != null)
        {
            modalPanel.SetActive(false);
        }
        _currentDecision = null;
    }

    private void OnDestroy()
    {
        if (RepublicGameManager.Instance != null)
        {
            RepublicGameManager.Instance.UnityBridge.DecisionPrompted -= DisplayDecisionModal;
        }
    }
}
