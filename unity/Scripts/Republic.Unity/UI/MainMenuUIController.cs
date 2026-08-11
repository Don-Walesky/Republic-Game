namespace Republic.Unity.UI;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unity UI controller managing new game campaign scenario selection, difficulty picks, and quick-load triggers.
/// </summary>
public sealed class MainMenuUIController : MonoBehaviour
{
    [Header("Menu Views")]
    [SerializeField] private GameObject titlePanel = null!;
    [SerializeField] private GameObject scenarioSelectPanel = null!;
    [SerializeField] private GameObject saveSlotPanel = null!;

    [Header("Selected Scenario Summary Display")]
    [SerializeField] private Text selectedScenarioTitleText = null!;
    [SerializeField] private Text selectedScenarioDescriptionText = null!;
    [SerializeField] private Text difficultyBadgeText = null!;

    private string currentScenarioId = "arcadia-day1";
    private string selectedDifficulty = "Standard";

    public void OnStartCampaignClicked()
    {
        Debug.Log($"[Main Menu] Launching Campaign Scenario '{currentScenarioId}' under '{selectedDifficulty}' difficulty!");
        // Triggers bootstrapper and loads ExecutiveDeskScene
    }

    public void SelectScenario(string scenarioId)
    {
        currentScenarioId = scenarioId;
        if (selectedScenarioTitleText != null)
        {
            selectedScenarioTitleText.text = $"SELECTED SCENARIO: {scenarioId.ToUpper()}";
        }
    }

    public void SetDifficulty(string difficulty)
    {
        selectedDifficulty = difficulty;
        if (difficultyBadgeText != null)
        {
            difficultyBadgeText.text = $"DIFFICULTY: {difficulty.ToUpper()}";
        }
    }

    public void OnLoadSlotClicked(string slotName)
    {
        Debug.Log($"[Main Menu] Loading save slot '{slotName}'...");
    }

    public void OnExitClicked()
    {
        Debug.Log("[Main Menu] Exiting Republic session.");
        Application.Quit();
    }
}
