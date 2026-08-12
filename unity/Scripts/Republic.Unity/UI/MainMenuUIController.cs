namespace Republic.Unity.UI;

using UnityEngine;
using UnityEngine.UI;
using Republic.Core.Engine;

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
    private DifficultyPreset selectedDifficultyPreset = DifficultyPreset.Standard;

    public void OnStartCampaignClicked()
    {
        Debug.Log($"[Main Menu] Launching Campaign Scenario '{currentScenarioId}' under '{selectedDifficultyPreset}' difficulty!");
        var settings = GameDifficultySettings.FromPreset(selectedDifficultyPreset);
        
        if (RepublicGameManager.Instance != null)
        {
            Debug.Log($"[Main Menu] Configured Campaign Difficulty: AiAggression={settings.AiAggressionMultiplier}x, InsolvencyThreshold=${settings.InsolvencyThreshold:N0}");
        }
        // Loads ExecutiveDeskScene / transitions views
    }

    public void SelectScenario(string scenarioId)
    {
        currentScenarioId = scenarioId;
        if (selectedScenarioTitleText != null)
        {
            selectedScenarioTitleText.text = $"SELECTED SCENARIO: {scenarioId.ToUpper()}";
        }
    }

    public void SetDifficulty(string difficultyName)
    {
        if (System.Enum.TryParse<DifficultyPreset>(difficultyName, true, out var preset))
        {
            selectedDifficultyPreset = preset;
        }
        else
        {
            selectedDifficultyPreset = DifficultyPreset.Standard;
        }

        if (difficultyBadgeText != null)
        {
            difficultyBadgeText.text = $"DIFFICULTY: {selectedDifficultyPreset.ToString().ToUpper()}";
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

